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
        /// Returns an ActionResult with the input params as input to the node method
        /// </summary>
        /// <returns></returns>
        protected async Task<ActionResult> NodeResultAsync(object globalData, object routeData)
        {
            return await _builder.NodeResultAsync(globalData, routeData);
        }

        /// <summary>
        /// Returns an ActionResult that renders to a view with the input params as input to the node method
        /// </summary>
        /// <returns></returns>
        protected async Task<ActionResult> PartialNodeResultAsync(object globalData, object routeData)
        {
            return await _builder.PartialNodeResultAsync(globalData, routeData);
        }

        /// <summary>
        /// Returns an ActionResult based on a custom NodeServerEntryFilePath
        /// </summary>
        /// <returns></returns>
        protected async Task<ActionResult> NodeResultEntryFileAsync(string entryFilePath, object globalData, object routeData)
        {
            return await _builder.NodeResultEntryFileAsync(entryFilePath, globalData, routeData);
        }

        /// <summary>
        /// Returns an ActionResult that renders to a view based on a custom NodeServerEntryFilePath
        /// </summary>
        /// <returns></returns>
        protected async Task<ActionResult> PartiualNodeResultEntryFileAsync(string entryFilePath, object globalData, object routeData)
        {
            return await _builder.PartialNodeResultEntryFileAsync(entryFilePath, globalData, routeData);
        }
    }
}
