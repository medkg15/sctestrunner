namespace NUnitContrib.Web.TestRunner 
{
    using System.Web.Routing;
    using Handlers;
    using SCSettings = Sitecore.Configuration.Settings;

    public static class Settings
    {
        public static bool NUnitTestRunnerEnabled = SCSettings.GetBoolSetting("NUnitTestRunnerEnabled", false);

        public static string NUnitTestRunnerRoute = SCSettings.GetSetting("NUnitTestRunnerRoute", "testrunner");

        public static string NUnitTestRunnerPath = SCSettings.GetSetting("NUnitTestRunnerPath");

        public static string NUnitTestRunnerResultPath = SCSettings.GetSetting("NUnitTestRunnerResultPath", "TestResult.xml");

        public static void RegisterRoutes(string route, string nUnitTestPath, string nUnitTestResultPath)
        {
            var routes = RouteTable.Routes;
            var handler = new RunnerHandler(route, nUnitTestPath, nUnitTestResultPath);
            using (routes.GetWriteLock())
            {
                var defaultRoute = new Route(route + "/", handler)
                {
                    // we have to specify these, so no MVC route helpers will match, e.g. @Html.ActionLink("Home", "Index", "Home")
                    Defaults = new RouteValueDictionary(new { controller = "RunnerHandler", action = "ProcessRequest" }),
                    Constraints = new RouteValueDictionary(new { controller = "RunnerHandler", action = "ProcessRequest" })
                };

                routes.Add("RunnerHandlerRoute", defaultRoute);

                var filenameRoute = new Route(NUnitTestRunnerRoute + "/{filename}", handler)
                {
                    Defaults = new RouteValueDictionary(new { controller = "RunnerHandler", action = "ProcessRequest" }),
                    Constraints = new RouteValueDictionary(new { controller = "RunnerHandler", action = "ProcessRequest" })
                };

                routes.Add("RunnerHandlerFilenameRoute", filenameRoute);
            }
        }

        public static void RegisterDefaultRoutes()
        {
            RegisterRoutes(NUnitTestRunnerRoute, NUnitTestRunnerPath, NUnitTestRunnerResultPath);            
        }
    }
}

