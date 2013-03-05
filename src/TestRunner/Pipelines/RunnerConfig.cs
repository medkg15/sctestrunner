namespace NUnitContrib.Web.TestRunner.Pipelines
{
    using Sitecore.Diagnostics;
    using Sitecore.Pipelines;

    public class RunnerConfig
    {
        public void Process(PipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!Settings.NUnitTestRunnerEnabled) return;
            Settings.RegisterDefaultRoutes();
        }
    }
}
