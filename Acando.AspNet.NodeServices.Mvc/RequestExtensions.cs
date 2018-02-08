namespace Acando.AspNet.NodeServices.Mvc
{
    using System;
    using System.Web;
    using System.Linq;

    public static class RequestExtensions
    {
        public static bool HasJsonAcceptType(this HttpRequestBase request)
        {
            return (request.AcceptTypes ?? Enumerable.Empty<string>()).Contains("application/json",
                StringComparer.OrdinalIgnoreCase);
        }

        public static bool HasJsonAcceptType(this HttpRequest request)
        {
            return (request.AcceptTypes ?? Enumerable.Empty<string>()).Contains("application/json",
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
