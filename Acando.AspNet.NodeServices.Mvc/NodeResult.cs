namespace Acando.AspNet.NodeServices.Mvc
{
    using System.Web.Mvc;

    public class NodeResult : ActionResult
    {
        private readonly string _data;

        public NodeResult(string data)
        {
            _data = data;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.Write(_data);
        }
    }
}