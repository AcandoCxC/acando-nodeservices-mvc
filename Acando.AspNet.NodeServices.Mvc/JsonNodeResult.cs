namespace Acando.AspNet.NodeServices.Mvc
{
    using System;
    using System.Web.Mvc;

    public class JsonNodeResult : ActionResult
    {
        private readonly string _jsonData;

        /// <summary>
        /// Returns a NodeResult as Json
        /// </summary>
        /// <param name="jsonData"></param>
        public JsonNodeResult(string jsonData)
        {
            _jsonData = jsonData;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            
            context.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, max-age=0, must-revalidate, no-store");
            
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.Write(_jsonData);
        }
    }
}