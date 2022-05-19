using System;
using System.Collections.Generic;
using RestSharp.Portable;
using System.Threading.Tasks;
using DevCycle.SDK.Server.Common.API;
using DevCycle.SDK.Server.Common.Exception;
using DevCycle.SDK.Server.Common.Model;
using DevCycle.SDK.Server.Common.Model.Cloud;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DevCycle.SDK.Server.Cloud.Api
{

    public class DVCCloudClientBuilder : DVCClientBuilder
    {
        public override IDVCClient Build()
        {
            return new DVCCloudClient(environmentKey, loggerFactory);
        }
    }
    public sealed class DVCCloudClient : DVCBaseClient
    {
        private readonly DVCApiClient apiClient;
        private readonly ILogger logger;

        internal DVCCloudClient(string serverKey, ILoggerFactory loggerFactory)
        {
            apiClient = new DVCApiClient(serverKey);
            logger = loggerFactory.CreateLogger<DVCCloudClient>();
        }
        
        public override string Platform()
        {
            return "Cloud";
        }

        public override IDVCApiClient GetApiClient()
        {
            return apiClient;
        }

        public async Task<Dictionary<string, Feature>> AllFeaturesAsync(User user)
        {
            ValidateUser(user);

            AddDefaults(user);

            string urlFragment = "v1/features";

            return await GetResponseAsync<Dictionary<string, Feature>>(user, urlFragment);
        }

        public async Task<IVariable> VariableAsync<T>(User user, string key, T defaultValue)
        {
            ValidateUser(user);

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key cannot be null or empty");
            }

            if (defaultValue == null)
            {
                throw new ArgumentNullException(nameof(defaultValue));
            }

            AddDefaults(user);

            string lowerKey = key.ToLower();

            string urlFragment = "v1/variables/" + lowerKey;

            Variable variable;

            try
            {
                variable = await GetResponseAsync<Variable>(user, urlFragment);
            }
            catch (DVCException e)
            {
                variable = new Variable(lowerKey, (object) defaultValue, e.Message);
            }

            return variable;
        }

        public async Task<Dictionary<string, IVariable>> AllVariablesAsync(User user)
        {
            ValidateUser(user);

            AddDefaults(user);

            string urlFragment = "v1/variables";

            return await GetResponseAsync<Dictionary<string, IVariable>>(user, urlFragment);
        }

        public async Task<DVCResponse> TrackAsync(User user, Event userEvent)
        {
            ValidateUser(user);

            AddDefaults(user);

            string urlFragment = "v1/track";

            UserAndEvents userAndEvents = new UserAndEvents(new List<Event>() {userEvent}, user);

            return await GetResponseAsync<DVCResponse>(userAndEvents, urlFragment);
        }

        public override void Dispose()
        {
            ((IDisposable) apiClient).Dispose();
        }

    
    }
}