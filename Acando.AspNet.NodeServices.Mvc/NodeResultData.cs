namespace Acando.AspNet.NodeServices.Mvc
{
    using Newtonsoft.Json;
    public class NodeResultData
    {
        [JsonProperty(PropertyName = "css")] 
        public string Css { get; set; } = null;

        [JsonProperty(PropertyName = "html")]
        public string Html { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "chunks")]
        public string Chunks { get; set; } = null;
    }
}