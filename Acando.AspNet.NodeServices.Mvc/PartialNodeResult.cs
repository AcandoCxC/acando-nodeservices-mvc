namespace Acando.AspNet.NodeServices.Mvc
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web.Mvc;
    using System.Configuration;

    public class PartialNodeResult : ActionResult
    {
        private readonly NodeResultData _data;
        private readonly string _globalInitialData;
        private readonly string _routeData;
        private readonly string _rawData = "";

        public PartialNodeResult(NodeResultData resultData, string globalInitialData, string routeData)
        {
            _data = resultData;
            _globalInitialData = globalInitialData;
            _routeData = routeData;
        }

        /// <summary>
        /// Returns a NodeResult.
        /// </summary>
        /// <param name="globalInitialData"></param>
        /// <param name="routeData"></param>
        public PartialNodeResult(string globalInitialData, string routeData)
        {            
            _globalInitialData = globalInitialData;
            _routeData = routeData;
        }
        
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var viewName = ConfigurationManager.AppSettings["NodeServices.Mvc.ViewName"] ?? "_NodeResult";
            var clientAppName = ConfigurationManager.AppSettings["NodeServices.Mvc.ClientAppName"] ?? "client";

            context.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, max-age=0, must-revalidate, no-store");

            bool isClientRendered = string.IsNullOrEmpty(_data?.Html);
            
            var viewEngineResult = ViewEngines.Engines.FindView(context, viewName, null);

            var nodeResultModel = new NodeResultModel()
            {
                ErrorMessage = isClientRendered ? _rawData : string.Empty,
                HtmlResult = _data?.Html ?? string.Empty,
                IsClientRendered = isClientRendered,
                RouteData = _routeData,
                InitialGlobalData = _globalInitialData
            };

            if (viewEngineResult.View != null)
            {
                var view = viewEngineResult.View;

                context.Controller.ViewData.Model = nodeResultModel;
                context.Controller.ViewBag.NodeCss = _data?.Css;
                context.Controller.ViewBag.NodeChunks = _data?.Chunks;               
                context.Controller.ViewBag.ClientAppName = clientAppName;

                string result;

                using (var stringWriter = new StringWriter())
                {
                    var viewContext = new ViewContext(context, view,
                        context.Controller.ViewData,
                        context.Controller.TempData,
                        stringWriter);
                    view.Render(viewContext, stringWriter);
                    result = stringWriter.ToString();
                }

                context.HttpContext.Response.Write(result);
            }
            else
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("View not found. Locations searched:");
                foreach (string searchedLocation in viewEngineResult.SearchedLocations)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append(searchedLocation);
                }
                stringBuilder.AppendLine();
                throw new InvalidOperationException(stringBuilder.ToString());
            }
        }
    }
}
