﻿namespace NUnitContrib.Web.TestRunner.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web;
    using Configuration;
    using Core;

    public class RunnerHandler : BaseHttpHandler
    {
        private static readonly TestRunnerSection testRunnerConfig =
            ConfigurationManager.GetSection("testrunner") as TestRunnerSection;

        private static readonly IWebRunner runner;

        static RunnerHandler()
        {
            if (string.IsNullOrEmpty(testRunnerConfig.RoutePath))
                throw new ConfigurationErrorsException("You must configure a route path.");

            if (testRunnerConfig.Assemblies == null || testRunnerConfig.Assemblies.Count == 0)
                throw new ConfigurationErrorsException("You must configure at least one assembly.");

            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var directoryName = Path.GetDirectoryName(codeBase);
            if (directoryName == null) throw new DirectoryNotFoundException("Unable to determine running location for test runner");

            // Ensure the assembly path exists
            var currentDirectory = new Uri(directoryName).LocalPath;

            var assemblyList = new List<string>();
            foreach (AssemblyElement testAssembly in testRunnerConfig.Assemblies)
            {
                var assemblypath = Path.GetFullPath(Path.Combine(currentDirectory, testAssembly.Name + ".dll"));
                if (!File.Exists(assemblypath)) throw new FileNotFoundException("Cannot find test assembly at " + assemblypath);
                assemblyList.Add(assemblypath);
            }

            string testResultPath = null;
            // Get the test result path
            if (!string.IsNullOrEmpty(testRunnerConfig.ResultPath))
            {
                testResultPath = Path.GetFullPath(Path.Combine(currentDirectory, testRunnerConfig.ResultPath));
            }

            //runner = new NUnitWebRunner(assemblyList, testResultPath);
            runner = new XUnitWebRunner(assemblyList, testResultPath);
        }

        public override Task ProcessRequest(HttpContextBase context)
        {
            var path = context.Request.Path;
            var fileName = Path.GetFileName(path);
            if (fileName == null)
            {
                NotFound(context);
                return Task.CompletedTask;
            }

            var file = fileName.ToLowerInvariant();
            if (testRunnerConfig.RoutePath.Equals(file))
            {
                // If the users come to /testrunner instead of /testrunner/
                // the assets links would be absolute so they couldn't be
                // loaded.
                context.Response.Redirect("/" + testRunnerConfig.RoutePath + "/");
                return Task.CompletedTask;
            }

            runner.SessionId = context.Session.SessionID;

            switch (file)
            {
                case "":
                    ReturnResource(context, "index.html", "text/html");
                    break;
                case "results.html":
                case "list.html":
                    ReturnResource(context, file, "text/html");
                    break;
                case "angular.js":
                case "angular-route.js":
                case "app.js":
                case "bootstrap.js":
                case "controllers.js":
                case "jquery.js":
                    ReturnResource(context, file, "application/javascript");
                    break;
                case "app.css":
                case "bootstrap.css":
                    ReturnResource(context, file, "text/css");
                    break;
                case "glyphicons-halflings-regular.eot":
                    ReturnResource(context, file, "application/vnd.ms-fontobject");
                    break;
                case "glyphicons-halflings-regular.svg":
                    ReturnResource(context, file, "image/svg+xml");
                    break;
                case "glyphicons-halflings-regular.ttf":
                    ReturnResource(context, file, "application/octet-stream");
                    break;
                case "glyphicons-halflings-regular.woff":
                    ReturnResource(context, file, "application/font-woff");
                    break;
                case "glyphicons-halflings-regular.woff2":
                    ReturnResource(context, file, "application/font-woff2");
                    break;
                case "gettestsuite.json":
                    ReturnJson(context, runner.GetTestSuiteConfigInfo());
                    break;
                case "gettests.json":
                    ReturnJson(context, runner.GetTestSuiteInfo());
                    break;
                case "getrunnerstatus.json":
                    ReturnJson(context, runner.GetRunnerStatus());
                    break;
                case "runfixture.json":
                    ReturnJson(context, runner.RunFixture(context.Request["name"]));
                    break;
                case "runtest.json":
                    ReturnJson(context, runner.RunTest(context.Request["id"]));
                    break;
                case "runcategories.json":
                    var cats = context.Request["name"]
                        .Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(HttpUtility.UrlDecode);
                    ReturnJson(context, runner.RunCategories(cats));
                    break;
                case "runtests.json":
                    ReturnJson(context, runner.RunAllTests());
                    break;
                case "cancel.json":
                    ReturnJson(context, runner.CancelRunner());
                    break;
                default:
                    NotFound(context);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
