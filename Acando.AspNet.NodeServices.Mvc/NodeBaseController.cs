namespace Acando.AspNet.NodeServices.Mvc
{
    using System.Web.Mvc;
    using System.Threading.Tasks;

    public class NodeBaseController : Controller
    {
        private readonly INodeServices _nodeServices;
        private NodeResultBuilder _builder;

        public NodeBaseController(INodeServices nodeServices)
        {
            _nodeServices = nodeServices;
            _builder = new NodeResultBuilder(nodeServices);
        }

        /// <summary>
        /// Returns a NodeResult with the input params as input to the node method
        /// </summary>
        /// <returns></returns>
        protected async Task<NodeResult> NodeResultAsync(object customData)
        {
            return await _builder.NodeResultAsync(customData);
        }
        
        /// <summary>
        /// Returns a NodeResult with the input params as input to the node method
        /// </summary>
        /// <returns></returns>
        protected async Task<NodeResult> NodeResultAsync(object globalData, object routeData)
        {
            return await _builder.NodeResultAsync(globalData, routeData);
        }

        /// <summary>
        /// Returns a NodeResult based on a custom NodeServerEntryFilePath
        /// </summary>
        /// <returns></returns>
        protected async Task<NodeResult> NodeResultEntryFileAsync(string entryFilePath, object globalData, object routeData)
        {
            return await _builder.NodeResultEntryFileAsync(entryFilePath, globalData, routeData);
        }
    }
}
