using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acando.AspNet.NodeServices.Mvc
{
    using System.Configuration;
    using System.Web.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public class NodeBaseController : Controller
    {
        private readonly INodeServices _nodeServices;
        private readonly JsonSerializerSettings _serializerSettings;

        private readonly string _nodeServerEntryFilePath;

        public NodeBaseController(INodeServices nodeServices)
        {
            _nodeServices = nodeServices;
            _serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            _nodeServerEntryFilePath = ConfigurationManager.AppSettings["NodeServerEntryFilePath"] ?? "./react.server";
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

        /// <summary>
        /// Returns a NodeResult with the input params as input to the node method
        /// </summary>
        /// <returns></returns>
        protected async Task<NodeResult> NodeResultAsync(object customData)
        {
            var result = await _nodeServices.InvokeAsync<string>(_nodeServerEntryFilePath, HttpContext.Request.Url?.LocalPath ?? "/", JsonConvert.SerializeObject(customData, _serializerSettings));
            return new NodeResult(result);
        }
        
        /// <summary>
        /// Returns a NodeResult with the input params as input to the node method
        /// </summary>
        /// <returns></returns>
        protected async Task<NodeResult> NodeResultAsync(object globalData, object routeData)
        {
            var result = await _nodeServices.InvokeAsync<string>(_nodeServerEntryFilePath, HttpContext.Request.Url?.LocalPath ?? "/", JsonConvert.SerializeObject(new ParamsWrapper(globalData, routeData), _serializerSettings));
            return new NodeResult(result);
        }

        /// <summary>
        /// Returns a NodeResult based on a custom NodeServerEntryFilePath
        /// </summary>
        /// <returns></returns>
        protected async Task<NodeResult> NodeResultEntryFileAsync(string entryFilePath, object globalData, object routeData)
        {
            var result = await _nodeServices.InvokeAsync<string>(entryFilePath, HttpContext.Request.Url?.LocalPath ?? "/", JsonConvert.SerializeObject(new ParamsWrapper(globalData, routeData), _serializerSettings));
            return new NodeResult(result);
        }
    }
}
