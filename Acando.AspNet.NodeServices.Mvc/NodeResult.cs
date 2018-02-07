namespace Acando.AspNet.NodeServices.Mvc
{
    using System.Web.Mvc;

    public class NodeResult : ActionResult
    {
        private readonly string _data;
        private readonly bool _isJson;

        public NodeResult(string data, bool isJson = false)
        {
            _data = data;
            _isJson = isJson;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (_isJson)
            {
                context.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, max-age=0, must-revalidate, no-store");
                context.HttpContext.Response.ContentType = "application/json";
            }

            context.HttpContext.Response.Write(_data);
        }
    }
}