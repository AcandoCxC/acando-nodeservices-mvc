namespace Acando.AspNet.NodeServices.Mvc
{
    using System.Configuration;
    using System.Web;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class NodeResultBuilder
    {
        private readonly INodeServices _nodeServices;
        private readonly string _returnAsJsonHeaderKey;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly string _nodeServerEntryFilePath;

        public NodeResultBuilder(INodeServices nodeServices)
        {
            _nodeServices = nodeServices;
            _serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            _nodeServerEntryFilePath = ConfigurationManager.AppSettings["NodeServerEntryFilePath"] ?? "./react.server";
            _returnAsJsonHeaderKey = ConfigurationManager.AppSettings["ReturnAsJsonHeaderKey"] ?? "poop";
        }

        public async Task<NodeResult> NodeResultAsync(object customData)
        {
            var wrapper = new ParamsWrapper(null, customData);

            if (HttpContext.Current.Request.Headers[_returnAsJsonHeaderKey] != null)
            {
                return new NodeResult(JsonConvert.SerializeObject(wrapper.RouteData), true);
            }

            var result = await _nodeServices.InvokeAsync<string>(_nodeServerEntryFilePath, 
                HttpContext.Current.Request.Url?.LocalPath ?? "/",
                JsonConvert.SerializeObject(wrapper.RouteData, _serializerSettings));

            return new NodeResult(result);
        }

        public async Task<NodeResult> NodeResultAsync(object globalData, object routeData)
        {
            var wrapper = new ParamsWrapper(globalData, routeData);

            if (HttpContext.Current.Request.Headers[_returnAsJsonHeaderKey] != null)
            {
                return new NodeResult(JsonConvert.SerializeObject(wrapper.RouteData), true);
            }

            var result = await _nodeServices.InvokeAsync<string>(_nodeServerEntryFilePath, HttpContext.Current.Request.Url?.LocalPath ?? "/", JsonConvert.SerializeObject(wrapper, _serializerSettings));
            return new NodeResult(result);
        }
        public async Task<NodeResult> NodeResultEntryFileAsync(string entryFilePath, object globalData, object routeData)
        {
            var wrapper = new ParamsWrapper(globalData, routeData);

            if (HttpContext.Current.Request.Headers[_returnAsJsonHeaderKey] != null)
            {
                return new NodeResult(JsonConvert.SerializeObject(wrapper.RouteData), true);
            }

            var result = await _nodeServices.InvokeAsync<string>(entryFilePath, HttpContext.Current.Request.Url?.LocalPath ?? "/", JsonConvert.SerializeObject(new ParamsWrapper(globalData, routeData), _serializerSettings));
            return new NodeResult(result);
        }

        private class ParamsWrapper
        {
            public ParamsWrapper(object globalData, object routeData)
            {
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

            public object GlobalData { get; set; }
            public object RouteData { get; set; }
        }

    }
}
