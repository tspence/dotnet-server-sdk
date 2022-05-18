using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DevCycle.Api;
using DevCycle.SDK.Server.Common.Exception;
using DevCycle.SDK.Server.Common.Model;
using DevCycle.SDK.Server.Common.Model.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using ErrorResponse = DevCycle.SDK.Server.Common.Model.ErrorResponse;

namespace DevCycle.ConfigManager
{
    public class EnvironmentConfigManager : IDisposable
    {
        private const int MinimumPollingIntervalMs = 1000;

        private readonly string environmentKey;
        private readonly int pollingIntervalMs;
        private readonly int requestTimeoutMs;
        private readonly IRestClient restClient;
        private readonly ILogger logger;
        private readonly LocalBucketing localBucketing;
        private readonly DVCEventArgs dvcEventArgs;

        private Timer pollingTimer;

        public virtual string Config { get; private set; }
        public virtual bool Initialized { get; private set; }

        private string configEtag;

        // internal parameterless constructor for testing
        internal EnvironmentConfigManager() : this("not-a-real-key", new DVCOptions(), new NullLoggerFactory(), new LocalBucketing())
        {
        }

        public EnvironmentConfigManager(string environmentKey, DVCOptions dvcOptions, ILoggerFactory loggerFactory, LocalBucketing localBucketing)
        {
            this.environmentKey = environmentKey;
            
            pollingIntervalMs = dvcOptions.ConfigPollingIntervalMs >= MinimumPollingIntervalMs
                ? dvcOptions.ConfigPollingIntervalMs
                : MinimumPollingIntervalMs;
            requestTimeoutMs = dvcOptions.ConfigPollingTimeoutMs <= pollingIntervalMs
                ? pollingIntervalMs
                : dvcOptions.ConfigPollingTimeoutMs;

            restClient = new RestClient(dvcOptions.CdnUri);
            this.logger = loggerFactory.CreateLogger<EnvironmentConfigManager>();
            this.localBucketing = localBucketing;
            dvcEventArgs = new DVCEventArgs();
        }

        public virtual async Task<DVCEventArgs> InitializeConfigAsync()
        {
            await FetchConfigAsyncWithTask();
            
            pollingTimer = new Timer(FetchConfigAsync, null, pollingIntervalMs, pollingIntervalMs);

            return dvcEventArgs;
        }

        public void Dispose()
        {
            pollingTimer?.Dispose();
            restClient.Dispose();
        }

        private string GetConfigUrl()
        {
            return $"/config/v1/server/{environmentKey}.json";
        }
        
        private void SetConfig(IRestResponse res)
        {
            if (res.StatusCode == HttpStatusCode.NotModified)
            {
                logger.LogInformation("Config not modified, using cache, etag: {ConfigEtag}", configEtag);
            }
            else if (res.StatusCode == HttpStatusCode.OK)
            {
                var isInitialFetch = Config == null;
                Config = res.Content;
                localBucketing.StoreConfig(environmentKey, Config);

                IEnumerable<string> headerValues = res.Headers.GetValues("etag");
                configEtag = new List<string>(headerValues).FirstOrDefault();

                logger.LogInformation("Config successfully initialized with etag: {ConfigEtag}", configEtag);

                if (!isInitialFetch) return;
                
                Initialized = true;
                dvcEventArgs.Success = true;
            }
            else if (Config != null)
            {
                logger.LogError("Failed to download config, using cached version: {ConfigEtag}", configEtag);
            }
            else
            {
                logger.LogError("Failed to download DevCycle config");

                var exception = new DVCException(HttpStatusCode.InternalServerError,
                    new ErrorResponse("Failed to download DevCycle config."));
                dvcEventArgs.Error = exception;

                throw exception;
            }
        }

        private async Task FetchConfigAsyncWithTask()
        {
            restClient.IgnoreResponseStatusCode = true;
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(requestTimeoutMs));
            var request = new RestRequest(GetConfigUrl(), Method.GET);
            if (configEtag != null) request.AddHeader("If-None-Match", configEtag);

            try
            {
                IRestResponse res = await restClient.Execute(request, cts.Token);
                SetConfig(res);
            }
            catch (DVCException e)
            {
                if (Config == null && configEtag == null)
                {
                    logger.LogError("Error loading initial config. Exception: {Exception}", e.Message);
                    dvcEventArgs.Error = e;
                } 
                else
                {
                    logger.LogError("Error loading config. Using cache etag: {ConfigEtag}. Exception: {Exception}", configEtag, e.Message);
                }
            }
        }
        
        private async void FetchConfigAsync(object state = null)
        {
            await FetchConfigAsyncWithTask();
        }
    }
}