namespace Acando.AspNet.NodeServices.Mvc
{
    using System.Configuration;
    using System.Net;
    using System.Web;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Mvc;
    using log4net;
    using Polly;
    using Polly.Timeout;

    public class NodeResultBuilder
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly INodeServices _nodeServices;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly string _nodeServerEntryFilePath;
        private readonly bool _renderServerSide;
        private readonly int _timeoutInSeconds;
        private readonly bool _disableFallbackRendering;

        public NodeResultBuilder(INodeServices nodeServices, int timeoutInSeconds = 3)
        {
            _nodeServices = nodeServices;
            _renderServerSide = timeoutInSeconds > 0;
            _timeoutInSeconds = timeoutInSeconds;
            _serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCaseExceptDictionaryKeysResolver() };
            _nodeServerEntryFilePath = ConfigurationManager.AppSettings["NodeServices.Mvc.NodeServerEntryFilePath"] ?? "./react.server";
            bool.TryParse(ConfigurationManager.AppSettings["NodeServices.Mvc.DisableFallbackRendering"], out var disableFallback);
            _disableFallbackRendering = disableFallback;
        }

        #region PartialRendering 

        public async Task<ActionResult> PartialNodeResultAsync(object globalData, object routeData)
        {
            return await PartialNodeResultBaseAsync(_nodeServerEntryFilePath, globalData, routeData);
        }

        private async Task<ActionResult> PartialNodeResultBaseAsync(string entryFilePath, object globalData, object routeData)
        {
            var wrapper = new ParamsWrapper(globalData, routeData, _serializerSettings);

            if (HttpContext.Current.Request.HasJsonAcceptType())
            {
                return new JsonNodeResult(wrapper.RouteDataJson);
            }

            if (_renderServerSide == false)
            {
                return new PartialNodeResult(wrapper.GlobalDataJson, wrapper.RouteDataJson);
            }

            try
            {
                var wrapperJson = JsonConvert.SerializeObject(new { globalData = wrapper.GlobalData, routeData = wrapper.RouteDataDictionary }, _serializerSettings);

                NodeResultData result;

                var timeoutPolicy = Policy.TimeoutAsync(_timeoutInSeconds, TimeoutStrategy.Pessimistic);

                var policyResult = await timeoutPolicy.ExecuteAndCaptureAsync(() => _nodeServices.InvokeAsync<NodeResultData>(entryFilePath, wrapperJson));

                if (policyResult.Outcome == OutcomeType.Successful)
                {
                    result = policyResult.Result;
                }
                else
                {
                    if (policyResult.ExceptionType.HasValue)
                    {
                        throw policyResult.FinalException;
                    }
                    throw new TimeoutException("InvokeAsync policy timed out.");
                }

                return new PartialNodeResult(result, wrapper.GlobalDataJson, wrapper.RouteDataJson);
            }
            catch (TimeoutRejectedException timeoutException)
            {
                Logger.Error("Failed to Server Render React due to timeout. Defaulted to client rendering.", timeoutException);
                return new PartialNodeResult(wrapper.GlobalDataJson, wrapper.RouteDataJson);
            }
            catch (TimeoutException timeoutException)
            {
                Logger.Error("Failed to Server Render React due to timeout. Defaulted to client rendering.", timeoutException);
                return new PartialNodeResult(wrapper.GlobalDataJson, wrapper.RouteDataJson);
            }
            catch (Exception exception)
            {
                Logger.Error("Failed to Server Render React. Defaulted to client rendering.", exception);
                return new PartialNodeResult(wrapper.GlobalDataJson, wrapper.RouteDataJson);
            }
        }

        public async Task<ActionResult> PartialNodeResultEntryFileAsync(string entryFilePath, object globalData, object routeData)
        {
            return await PartialNodeResultBaseAsync(entryFilePath, globalData, routeData);
        }

        #endregion

        #region FullRendering
        public async Task<ActionResult> NodeResultAsync(object globalData, object routeData)
        {
            return await NodeResultBaseAsync(_nodeServerEntryFilePath, globalData, routeData);
        }

        public async Task<ActionResult> NodeResultEntryFileAsync(string entryFilePath, object globalData, object routeData)
        {
            return await NodeResultBaseAsync(entryFilePath, globalData, routeData);
        }

        private async Task<ActionResult> NodeResultBaseAsync(string entryfilePath, object globalData, object routeData)
        {
            var wrapper = new ParamsWrapper(globalData, routeData, _serializerSettings);

            if (HttpContext.Current.Request.HasJsonAcceptType())
            {
                return new NodeResult(JsonConvert.SerializeObject(wrapper.RouteData), true);
            }

            try
            {
                var wrapperJson = JsonConvert.SerializeObject(new { globalData = wrapper.GlobalData, routeData = wrapper.RouteDataDictionary }, _serializerSettings);

                string result;

                var timeoutPolicy = Policy.TimeoutAsync(_timeoutInSeconds, TimeoutStrategy.Pessimistic);

                var policyResult = await timeoutPolicy.ExecuteAndCaptureAsync(() => _nodeServices.InvokeAsync<string>(entryfilePath, wrapperJson));

                if (policyResult.Outcome == OutcomeType.Successful)
                {
                    result = policyResult.Result;
                }
                else
                {
                    if (policyResult.ExceptionType.HasValue)
                    {
                        throw policyResult.FinalException;
                    }
                    throw new TimeoutException("InvokeAsync policy timed out.");
                }

                return new NodeResult(result);
            }
            catch (TimeoutException timeoutException)
            {
                if (_disableFallbackRendering)
                {
                    Logger.Error("Failed to Server Render React due to timeout.", timeoutException);
                    return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable);
                }

                Logger.Error("Failed to Server Render React due to timeout. Defaulted to client rendering.", timeoutException);
                return new PartialNodeResult(wrapper.GlobalDataJson, wrapper.RouteDataJson);
            }
            catch (TimeoutRejectedException timeoutException)
            {
                if (_disableFallbackRendering)
                {
                    Logger.Error("Failed to Server Render React due to timeout.", timeoutException);
                    return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable);
                }

                Logger.Error("Failed to Server Render React due to timeout. Defaulted to client rendering.", timeoutException);
                return new PartialNodeResult(wrapper.GlobalDataJson, wrapper.RouteDataJson);
            }
            catch (Exception exception)
            {
                if (_disableFallbackRendering)
                {
                    Logger.Error("Failed to Server Render React.", exception);
                    return new HttpStatusCodeResult(500, exception.Message);
                }

                Logger.Error("Failed to Server Render React. Defaulted to client rendering.", exception);
                return new PartialNodeResult(wrapper.GlobalDataJson, wrapper.RouteDataJson);
            }
        }

        #endregion

        /// <summary>
        /// This class is used to override the JSON serialization
        /// </summary>
        private class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
        {

            /// <summary>
            /// Preserve the casing for keys in Dictionaries
            /// </summary>
            /// <param name="objectType"></param>
            /// <returns></returns>
            protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
            {
                JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);

                contract.DictionaryKeyResolver = propertyName => propertyName;

                return contract;
            }
        }

        private class ParamsWrapper
        {
            private readonly JsonSerializerSettings _serializerSettings;

            public ParamsWrapper(object globalData, object routeData, JsonSerializerSettings serializerSettings)
            {
                _serializerSettings = serializerSettings;
                GlobalData = globalData;

                if (routeData is string)
                {
                    RouteData = JsonConvert.DeserializeObject(routeData.ToString());
                }
                else
                {
                    RouteData = routeData;
                }
            }

            // This is used since it is serialized to json.
            public object GlobalData { get; set; }
            public object RouteData { get; private set; }

            [JsonIgnore]
            public string GlobalDataJson => JsonConvert.SerializeObject(GlobalData, _serializerSettings);

            [JsonIgnore]
            public string RouteDataJson
            {
                get
                {
                    string rootNodeName;

                    if (RouteData != null)
                    {
                        if (!(RouteData.GetType().GetCustomAttribute(typeof(JsonObjectAttribute)) is JsonObjectAttribute attr))
                        {
                            // Looks like someone forgot to att JsonObject[Title="Whatever"] to the ViewModel root class...
                            rootNodeName = RouteData.GetType().Name.Replace("ViewModel", "");
                        }
                        else
                        {
                            rootNodeName = attr.Title;
                        }
                    }
                    else
                    {
                        rootNodeName = "root";
                    }

                    return JsonConvert.SerializeObject(new Dictionary<string, object>() { { rootNodeName, RouteData } }, _serializerSettings);
                }
            }

            public Dictionary<string, object> RouteDataDictionary
            {
                get
                {
                    string rootNodeName;
                    if (RouteData != null)
                    {
                        if (!(RouteData.GetType().GetCustomAttribute(typeof(JsonObjectAttribute)) is JsonObjectAttribute attr))
                        {
                            // Looks like someone forgot to att JsonObject[Title="Whatever"] to the ViewModel root class...
                            rootNodeName = RouteData.GetType().Name.Replace("ViewModel", "");
                        }
                        else
                        {
                            rootNodeName = attr.Title;
                        }
                    }
                    else
                    {
                        rootNodeName = "root";
                    }

                    return new Dictionary<string, object>() { { rootNodeName, RouteData } };
                }
            }
        }

    }
}
