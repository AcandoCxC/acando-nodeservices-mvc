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

        public async Task<NodeResult> NodeResultAsync(params string[] paramsStrings)
        {
            var result = await _nodeServices.InvokeAsync<string>(_nodeServerEntryFilePath, HttpContext.Request.Url?.LocalPath ?? "/", paramsStrings);
            return new NodeResult(result);
        }

        protected async Task<NodeResult> NodeResultAsync(params object[] paramsObjects)
        {
            var result = await _nodeServices.InvokeAsync<string>(_nodeServerEntryFilePath, HttpContext.Request.Url?.LocalPath ?? "/", paramsObjects.Select(x => JsonConvert.SerializeObject(x, _serializerSettings)).ToArray());
            return new NodeResult(result);
        }
    }
}
