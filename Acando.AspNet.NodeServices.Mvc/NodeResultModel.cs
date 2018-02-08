namespace Acando.AspNet.NodeServices.Mvc
{
    public class NodeResultModel
    {
        public string RouteData { get; set; }
        public string ErrorMessage { get; set; }
        public string HtmlResult { get; set; }
        public bool IsClientRendered { get; set; }
        public string InitialGlobalData { get; set; }
    }
}