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

        /// <summary>
        /// Returns a NodeResult with the input params as input to the node method
        /// </summary>
        /// <param name="paramsObjects"></param>
        /// <returns></returns>
        protected async Task<NodeResult> NodeResultAsync(params object[] paramsObjects)
        {
            string[] jsonObjects = new string[paramsObjects.Length];
            
            for (int i = 0; i < paramsObjects.Length ; i++)
            {
                // object is most likely already serialized to json
                if (paramsObjects[i] is string)
                {
                    jsonObjects[i] = paramsObjects[i].ToString();
                }
                else
                {
                    jsonObjects[i] = JsonConvert.SerializeObject(paramsObjects[i], _serializerSettings);
                }
            }
            
            var result = await _nodeServices.InvokeAsync<string>(_nodeServerEntryFilePath, HttpContext.Request.Url?.LocalPath ?? "/", jsonObjects);
            return new NodeResult(result);
        }

        /// <summary>
        /// Returns a NodeResult based on a custom NodeServerEntryFilePath
        /// </summary>
        /// <param name="paramsObjects">First object must be a string containing NodeServerEntryFilePath</param>
        /// <returns></returns>
        protected async Task<NodeResult> NodeResultEntryFileAsync(params object[] paramsObjects)
        {
            string[] jsonObjects = new string[paramsObjects.Length];

            if (paramsObjects.Length < 1)
            {
                throw new ArgumentException("Expected at least one object in params.");
            }

            if (!(paramsObjects[0] is string))
            {
                throw new ArgumentException("First param must be of type string and contain a NodeServerEntryFilePath ");
            }

            for (int i = 1; i < paramsObjects.Length; i++)
            {
                if (paramsObjects[i] is string)
                {
                    // object is most likely already serialized
                    jsonObjects[i-1] = paramsObjects[i].ToString();
                }
                else
                {
                    jsonObjects[i-1] = JsonConvert.SerializeObject(paramsObjects[i], _serializerSettings);
                }
            }

            var result = await _nodeServices.InvokeAsync<string>(paramsObjects[0].ToString(), HttpContext.Request.Url?.LocalPath ?? "/", jsonObjects);
            return new NodeResult(result);
        }        
    }
}
