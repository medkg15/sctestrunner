Why
---

One of the techniques used for testing Sitecore is using embedded test runner. You can find more information in [this book][1] and some videos from [codeflood][2].

![alt text][3]

Features
--------

This test runner will allow you to run all nunit test on a test suite, but also filter by category, text fixture and run individual tests. It will also generate a TestResult.xml file based on the Nunit test result.

Settings
--------

 - **NUnitTestRunnerEnabled** to enable/disable the test runner
 - **NUnitTestRunnerRoute** to set the route. The default route is *testrunner* and you should be able to access the test runner using your browser http://youhost/testnunner/
 - **NUnitTestRunnerPath** is the default path to the dll containing the tests, example *ScBootstrap.Tests.dll* asumming the dll is in the bin folder or a full path i.e *C:\YourPath\NUnitContrib.Web.TestExamples.dll*. This setting is **mandatory**
 - **NUnitTestRunnerResultPath** is the output file for the test result. The default value is *TestResult.xml* meaning that it will be saved on your bin folder. You can also provide a full path, i.e, *C:\Temp\TestResult.xml*. Please make sure the process has write access to that file.


Configuration file example
--------------------------

    <configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
      <sitecore>
        <pipelines>
          <initialize>
            <processor type="NUnitContrib.Web.TestRunner.Pipelines.RunnerConfig, NUnitContrib.Web.TestRunner" />
          </initialize>
        </pipelines>
        <settings>
          <setting name="NUnitTestRunnerEnabled" value="true" />
    
          <setting name="NUnitTestRunnerRoute" value="testrunner"/>
          <!--<setting name="NUnitTestRunnerRoute" value="tests"/>-->
    
          <setting name="NUnitTestRunnerPath" value="ScBootstrap.Tests.dll"/>
          <!--<setting name="NUnitTestRunnerPath" value="C:\Projects\TestRunner\src\TestExamples\bin\Debug\NUnitContrib.Web.TestExamples.dll"/>-->
    
          <setting name="NUnitTestRunnerResultPath" value="TestResult.xml"/>
          <!--<setting name="NUnitTestRunnerResultPath" value="C:\Temp\TestResult.xml"/>-->
        </settings>
      </sitecore>
    </configuration>

License and source code
-------

 Code licensed under the The [MIT license][4] and you can find the source code on [github][5].


  [1]: http://www.amazon.co.uk/Professional-Sitecore-Development-John-West/dp/047093901X
  [2]: http://www.youtube.com/user/codeflood
  [3]: http://jlusar.es/get/testrunner/test-runner-small.png "Test runner"
  [4]: http://opensource.org/licenses/mit-license.php
  [5]: https://github.com/jorgelusar/sctestrunner