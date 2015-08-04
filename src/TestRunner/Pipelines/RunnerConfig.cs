namespace NUnitContrib.Web.TestRunner.Pipelines
{
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.Diagnostics;
    using Sitecore.Pipelines;
    using System.Web.Routing;
    using NUnitContrib.Web.TestRunner.Handlers;

    public class RunnerConfig
    {
        public bool Enabled { get; set; }
        public string RouteName { get; set; }
        public string RoutePath { get; set; }
        public List<string> Assemblies { get; private set; }
        public string ResultPath { get; set; }

        public RunnerConfig()
        {
            Assemblies = new List<string>();
            RoutePath = "testrunner";
        }

        public void Process(PipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!Enabled) return;

            Assert.IsNotNullOrEmpty(RouteName, "You must configure a RouteName");
            Assert.IsNotNullOrEmpty(RoutePath, "You must configure a RoutePath");
            Assert.IsTrue(Assemblies.Any(), "You must configure at least one assembly");
            RegisterRoutes();
        }

        protected virtual void RegisterRoutes()
        {
            var routes = RouteTable.Routes;
            var handler = new RunnerHandler(RoutePath, Assemblies, ResultPath);
            using (routes.GetWriteLock())
            {
                var defaultRoute = new Route(RoutePath + "/", handler)
                {
                    // we have to specify these, so no MVC route helpers will match, e.g. @Html.ActionLink("Home", "Index", "Home")
                    Defaults = new RouteValueDictionary(new { controller = "RunnerHandler", action = "ProcessRequest" }),
                    Constraints = new RouteValueDictionary(new { controller = "RunnerHandler", action = "ProcessRequest" })
                };

                routes.Add("RouteName", defaultRoute);

                var filenameRoute = new Route(RoutePath + "/{filename}", handler)
                {
                    Defaults = new RouteValueDictionary(new { controller = "RunnerHandler", action = "ProcessRequest" }),
                    Constraints = new RouteValueDictionary(new { controller = "RunnerHandler", action = "ProcessRequest" })
                };

                routes.Add(RouteName + "Files", filenameRoute);
            }
        }
    }
}
