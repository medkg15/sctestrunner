Why
---

Sometimes you have to test some dependencies that aren't testable at all or it's really painful to mock everything. For example when working with a CMS like Sitecore.
In this case one of the techniques used is using an embedded test runner. You can find more information (specifically for Sitecore) in [this book][1] and some videos from [codeflood][2].

Take into account that although we only mention Sitecore, this project is agnostic and can be used in any ASP.NET application.

Features
--------

This test runner will allow you to run all nunit test on a test suite, but also filter by category, test fixture and run individual tests. It will also generate a TestResult.xml file based on the NUnit test result.

Settings
--------

 - **enabled** to enable/disable the test runner
 - **routePath** the route you'll use to access the test runner, for example http://yourhost/testrunner/. This setting is **mandatory**
 - **resultPath** is the output file for the test result. Please make sure the process has write access to that file.
 - **assemblies** A list with all the dlls containing the tests.

Configuration file example
--------------------------

    <!-- You must add this section in the configSections of your web.config -->
    <section name="testrunner" type="NUnitContrib.Web.TestRunner.Configuration.TestRunnerSection, NUnitContrib.Web.TestRunner" />

    <testrunner enabled="false" routePath="testrunner" resultPath="TestResult.xml">
        <assemblies>
            <assembly name="Tests\NUnitContrib.Web.TestExamples" />
            <assembly name="Tests\NUnitContrib.Web.TestExamples2" />
        </assemblies>
    </testrunner>

    <!-- Register the handler in any path you like -->
    <add verb="*" path="testrunner" type="NUnitContrib.Web.TestRunner.Handlers.RunnerHandler, NUnitContrib.Web.TestRunner" name="NUnitContrib.Web.TestRunner" />

**Important:** You must ignore the route you've configured for the handler or it'll be handled by the default router, therefore you'll get a 404 error.

For raw ASP.NET MVC, add the following line to your RouteConfig file.

    routes.IgnoreRoute("testrunner");

Full route table example:

    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        routes.IgnoreRoute("testrunner");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }

For Sitecore, there is a setting called IgnoreUrsPrefixes in the web.config where you should add the handler route, example:

    <setting name="IgnoreUrlPrefixes" value="/sitecore/default.aspx|/trace.axd|/webresource.axd|/sitecore/shell/Controls/Rich Text Editor/Telerik.Web.UI.DialogHandler.aspx|/sitecore/shell/applications/content manager/telerik.web.ui.dialoghandler.aspx|/sitecore/shell/Controls/Rich Text Editor/Telerik.Web.UI.SpellCheckHandler.axd|/Telerik.Web.UI.WebResource.axd|/sitecore/admin/upgrade/|/layouts/testing|/testrunner" />

License and source code
-------

 Code licensed under the The [MIT license][3] and you can find the source code on [github][4].

  [1]: http://www.amazon.co.uk/Professional-Sitecore-Development-John-West/dp/047093901X
  [2]: http://www.youtube.com/user/codeflood
  [3]: http://opensource.org/licenses/mit-license.php
  [4]: https://github.com/jorgelusar/sctestrunner
