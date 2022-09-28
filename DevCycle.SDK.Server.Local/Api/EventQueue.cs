using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DevCycle.SDK.Server.Common.Exception;
using DevCycle.SDK.Server.Common.Model;
using DevCycle.SDK.Server.Common.Model.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using RestSharp;

namespace DevCycle.SDK.Server.Local.Api
{
    internal class EventQueue
    {
        private readonly DVCLocalOptions localOptions;
        private readonly DVCEventsApiClient dvcEventsApiClient;
        private readonly ILocalBucketing localBucketing;
        private readonly string environmentKey;

        private static readonly SemaphoreSlim EventQueueSemaphore = new(1, 1);

        private readonly SemaphoreSlim eventQueueMutex = new(1, 1);
        private readonly SemaphoreSlim aggregateEventQueueMutex = new(1, 1);
        private readonly SemaphoreSlim batchQueueMutex = new(1, 1);

        private readonly Dictionary<DVCPopulatedUser, UserEventsBatchRecord> eventPayloadsToFlush;

        private readonly AggregateEventQueues aggregateEvents;

        private readonly List<BatchOfUserEventsBatch> batchQueue = new();

        private readonly ILogger logger;

        private CancellationTokenSource tokenSource = new();
        private bool schedulerIsRunning;
        private bool flushInProgress;
        private event EventHandler<DVCEventArgs> FlushedEvents;
        
        public EventQueue(string environmentKey, DVCLocalOptions localOptions, ILoggerFactory loggerFactory,
            ILocalBucketing localBucketing, RestClientOptions restClientOptions = null)
        {
            dvcEventsApiClient = new DVCEventsApiClient(environmentKey, localOptions, restClientOptions);
            this.environmentKey = environmentKey;
            this.localOptions = localOptions;
            this.localBucketing = localBucketing;
            this.localBucketing.InitEventQueue(environmentKey, JsonConvert.SerializeObject(localOptions));
            eventPayloadsToFlush = new Dictionary<DVCPopulatedUser, UserEventsBatchRecord>();
            aggregateEvents = new AggregateEventQueues();

            logger = loggerFactory.CreateLogger<EventQueue>();
        }

        public void AddFlushedEventsSubscriber(EventHandler<DVCEventArgs> flushedEventsSubscriber)
        {
            FlushedEvents += flushedEventsSubscriber;
        }

        public void RemoveFlushedEventsSubscriber(EventHandler<DVCEventArgs> flushedEventsSubscriber)
        {
            FlushedEvents -= flushedEventsSubscriber;
        }

        private async Task<Tuple<FlushPayload, RestResponse>> GetPayloadResult(FlushPayload flushPayload)
        {
            return new Tuple<FlushPayload, RestResponse>(flushPayload, await dvcEventsApiClient.PublishEvents(flushPayload.Records));
        }

        public virtual async Task FlushEvents()
        {
            flushInProgress = true;
            var flushPayloads = GetPayloads();
            var flushResultEvent = new DVCEventArgs
            {
                Success = true
            };

            if (flushPayloads.Count == 0)
            {
                OnFlushedEvents(flushResultEvent);
                return;
            }

            logger.LogDebug($"AS Flush Payloads: ${flushPayloads}");

            Func<int, FlushPayload, int> reducer = (val, batches) => val + batches.EventCount;
            var eventCount = flushPayloads.Aggregate(0, reducer);
            logger.LogDebug($"DVC Flush ${eventCount} AS Events, for ${flushPayloads.Count} Users");

            var requestTasks = flushPayloads.Select(GetPayloadResult).ToList();
            await Task.WhenAll(requestTasks);
            var results = requestTasks.Select(task => task.Result);
            foreach (var (flushPayload, res) in results)
            {
                try
                {
                    if (res.StatusCode != HttpStatusCode.Created)
                    {
                        logger.LogError($"Error publishing events, status: ${res.StatusCode}, body: ${res.Content}");
                        localBucketing.OnPayloadFailure(this.environmentKey, flushPayload.PayloadID, (int)res.StatusCode >= 500);
                        flushResultEvent.Success = false;
                        flushResultEvent.Error = new DVCException(res.StatusCode, new ErrorResponse(res.ErrorMessage ?? ""));
                    }
                    else
                    {
                        logger.LogDebug($"DVC Flushed ${eventCount} Events, for ${flushPayload.Records.Count} Users");
                        localBucketing.OnPayloadSuccess(this.environmentKey, flushPayload.PayloadID);
                    }
                }
                catch (DVCException ex)
                {
                    logger.LogError($"DVC Error Flushing Events response message: ${ex.Message}");
                    localBucketing.OnPayloadFailure(this.environmentKey, flushPayload.PayloadID, true);
                    flushResultEvent.Success = false;
                    flushResultEvent.Error = ex;
                }
            }
            OnFlushedEvents(flushResultEvent);
            flushInProgress = false;
        }

        private List<FlushPayload> GetPayloads()
        {
            List<FlushPayload> flushPayloads;
            try
            {
                flushPayloads = localBucketing.FlushEventQueue(environmentKey);
            }
            catch (Exception ex)
            {
                logger.LogError($"DVC Error Flushing Events: ${ex.Message}");
                throw;
            }

            return flushPayloads;
        }

        public virtual void QueueEvent(DVCPopulatedUser user, Event @event, BucketedUserConfig config, bool throwOnQueueMax = false)
        {
            if (user is null)
            {
                throw new Exception("User can't be null");
            }
            if ((localOptions.DisableCustomEvents && @event.Type.Equals("customEvent")) ||
                localOptions.DisableAutomaticEvents && !@event.Type.Equals("customEvent"))
                return;

            if (CheckEventQueueSize())
            {
                logger.LogWarning(
                    "{Event} failed to be queued; events in queue exceed {Max}. Triggering a forced flush", @event,
                    localOptions.MaxEventsInQueue);
                if (throwOnQueueMax)
                    throw new DVCException(
                        new ErrorResponse("Failed to queue an event. Events in queue exceeded the max"));
                logger.Log(LogLevel.Error, "Failed to queue an event. Events in queue exceeded the max");
                return;
            }

            eventQueueMutex.Wait();
            localBucketing.QueueEvent(environmentKey, JsonConvert.SerializeObject(user), JsonConvert.SerializeObject(@event));
            eventQueueMutex.Release();

            logger.LogInformation("{Event} queued successfully", @event);
        }

        /**
         * Queue Event that can be aggregated together, where multiple calls are aggregated
         * by incrementing the 'value' field.
         */
        public virtual void QueueAggregateEvent(DVCPopulatedUser user, Event @event, BucketedUserConfig config, bool throwOnQueueMax = false)
        {
            if ((localOptions.DisableCustomEvents && @event.Type.Equals("customEvent")) ||
                localOptions.DisableAutomaticEvents && !@event.Type.Equals("customEvent"))
                return;

            if (CheckEventQueueSize())
            {
                logger.LogWarning("{Event} failed to be queued; events in queue exceed {Max}", @event,
                    localOptions.MaxEventsInQueue);
                if (throwOnQueueMax)
                    throw new DVCException(
                        new ErrorResponse("Failed to queue an event. Events in queue exceeded the max"));
                logger.Log(LogLevel.Error, "Failed to queue an event. Events in queue exceeded the max");
                return;
            }

            if (string.IsNullOrEmpty(user.UserId))
            {
                throw new ArgumentException("UserId must be set");
            }

            if (string.IsNullOrEmpty(@event.Target))
            {
                throw new ArgumentException("Target must be set");
            }

            if (@event.Type == string.Empty)
            {
                throw new ArgumentException("Type must be set");
            }

            var eventCopy = @event.Clone();
            eventCopy.Date = DateTimeOffset.UtcNow.DateTime;
            eventCopy.Value = 1;

            var requestEvent = new DVCRequestEvent(
                eventCopy,
                user.UserId,
                config == null ? new Dictionary<string, string>() : config.FeatureVariationMap
            );

            var userAndFeatureVars = new UserAndFeatureVars(user, requestEvent.FeatureVars);

            aggregateEventQueueMutex.Wait();
            localBucketing.QueueAggregateEvent(
                environmentKey,
                JsonConvert.SerializeObject(@event),
                JsonConvert.SerializeObject(config.VariableVariationMap)
                );
            aggregateEvents.AddEvent(userAndFeatureVars, requestEvent);
            aggregateEventQueueMutex.Release();
        }

        private Dictionary<DVCPopulatedUser, UserEventsBatchRecord> CombineUsersEventsToFlush()
        {
            var userEventsBatchRecords = aggregateEvents.GetEventBatches();

            foreach (var (user, userEventsRecord) in eventPayloadsToFlush)
            {
                if (userEventsBatchRecords.ContainsKey(user))
                {
                    userEventsBatchRecords[user].Events.AddRange(userEventsRecord.Events);
                }
                else
                {
                    userEventsBatchRecords.Add(user, userEventsRecord);
                }
            }

            return userEventsBatchRecords;
        }

        private IEnumerable<DVCRequestEvent> EventsFromAggregateEvents(
            Dictionary<string, Dictionary<string, DVCRequestEvent>> aggUserEventsRecord)
        {
            return (from eventType in aggUserEventsRecord
                    from eventTarget in eventType.Value
                    select eventTarget.Value).ToList();
        }

        private bool CheckEventQueueSize()
        {
            var queueSize = localBucketing.EventQueueSize(this.environmentKey);
            if (queueSize >= localOptions.FlushEventQueueSize)
            {
                if (!this.flushInProgress)
                {
                    ScheduleFlushWithDelay();
                }
                if (queueSize >= localOptions.MaxEventsInQueue)
                {
                    return true;
                }
            }

            return false;
        }

        public void ScheduleFlushWithDelay(bool queueRequest = false)
        {
            if (schedulerIsRunning && !queueRequest) return;

            schedulerIsRunning = true;
            flushInProgress = true;
            tokenSource = new CancellationTokenSource();

            Task.Run(async delegate
            {
                if (tokenSource.IsCancellationRequested)
                {
                    schedulerIsRunning = false;
                    tokenSource.Token.ThrowIfCancellationRequested();
                }

                await Task.Delay(localOptions.ConfigPollingIntervalMs);
                if (tokenSource.IsCancellationRequested)
                {
                    schedulerIsRunning = false;
                    tokenSource.Token.ThrowIfCancellationRequested();
                }

                await FlushEvents();
                schedulerIsRunning = false;
            }, tokenSource.Token);
        }

        private void OnFlushedEvents(DVCEventArgs e)
        {
            if (FlushedEvents?.Target == null) return;
            FlushedEvents?.Invoke(this, e);
        }
    }
}