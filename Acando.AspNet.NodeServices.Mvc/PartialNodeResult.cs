namespace Acando.AspNet.NodeServices.Mvc
{
    using System;
    using System.IO;
    using System.Text;
    using System.Web.Mvc;
    using System.Configuration;

    public class PartialNodeResult : ActionResult
    {
        private readonly string _data;
        private readonly string _globalInitialData;
        private readonly string _routeData;
        private readonly bool _isJson;

        /// <summary>
        /// Returns a NodeResult.
        /// </summary>
        /// <param name="resultData"></param>
        /// <param name="globalInitialData"></param>
        /// <param name="routeData"></param>
        public PartialNodeResult(string resultData, string globalInitialData, string routeData)
        {
            _data = resultData ?? string.Empty;
            _globalInitialData = globalInitialData;
            _routeData = routeData;
            _isJson = false;
        }

        /// <summary>
        /// Returns a NodeResult.
        /// </summary>
        /// <param name="globalInitialData"></param>
        /// <param name="routeData"></param>
        public PartialNodeResult(string globalInitialData, string routeData)
        {
            _data = string.Empty;
            _globalInitialData = globalInitialData;
            _routeData = routeData;
            _isJson = false;
        }

        /// <summary>
        /// Returns a NodeResult as Json
        /// </summary>
        /// <param name="jsonData"></param>
        public PartialNodeResult(string jsonData)
        {
            _data = jsonData;
            _isJson = true;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var cssDivider = ConfigurationManager.AppSettings["NodeServices.Mvc.CssDivider"] ?? "|css|";
            var viewName = ConfigurationManager.AppSettings["NodeServices.Mvc.ViewName"] ?? "_NodeResult";
            var clientAppName = ConfigurationManager.AppSettings["NodeServices.Mvc.ClientAppName"] ?? "client";

            context.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, max-age=0, must-revalidate, no-store");

            if (_isJson)
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.Write(_data);
                return;
            }

            // If we get css that needs to be injected then we put it here.
            string css = null;
            var html = _data;
            bool isClientRendered = false;

            if (_data.Contains(cssDivider))
            {
                var splitResult = _data.Split(new[] { cssDivider }, StringSplitOptions.None);
                css = splitResult[0];

                if (splitResult.Length > 1)
                {
                    html = splitResult[1];
                }
            }
            else
            {
                isClientRendered = true;
            }

            var viewEngineResult = ViewEngines.Engines.FindView(context, viewName, null);

            var nodeResultModel = new NodeResultModel()
            {
                ErrorMessage = isClientRendered ? html : "",
                HtmlResult = html,
                IsClientRendered = isClientRendered,
                RouteData = _routeData,
                InitialGlobalData = _globalInitialData
            };

            if (viewEngineResult.View != null)
            {
                var view = viewEngineResult.View;

                context.Controller.ViewData.Model = nodeResultModel;
                context.Controller.ViewBag.NodeCss = css;

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