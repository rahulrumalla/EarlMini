using System.Web.Http;

namespace EarlMini.Api
{
    public static class WebApiConfig
    {
        public static void Register( HttpConfiguration config )
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "v1/api/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Site",
                routeTemplate: "{url}/{controller}/{action}/{id}",
                defaults: new { action = "Index", controller = "Home", id = RouteParameter.Optional }
            );
        }
    }
}
