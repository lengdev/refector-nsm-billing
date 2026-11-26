using Newtonsoft.Json.Serialization;
using NSMSOLUTION.Helper_Code.Common;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace NSMSOLUTION
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            AuthorizationHeaderHandler tokenInspector = new AuthorizationHeaderHandler() { InnerHandler = new HttpControllerDispatcher(config) };
            // Web API configuration and services
            // 1. Enable CORS for attribute routing
            config.EnableCors();
             
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null,
                handler: tokenInspector
            );

            // WebAPI when dealing with JSON & JavaScript!
            // Setup json serialization to serialize classes to camel (std. Json format)
            var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.XmlFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data"));
            // Adding JSON type web api formatting.
            config.Formatters.Clear();
            config.Formatters.Add(formatter);
        }
    }
}
