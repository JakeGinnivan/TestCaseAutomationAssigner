Test Case Automation Assigner
==========================

Allows you to assign non-mstest tests to Test Cases in TFS

Simply select a test assembly and then browse for the test method, specify your TFS Server, Project and the Test Case #
![README](README_images\README.png)

Easily search through all your tests to find the one you want
![README1](README_images\README1.png)

Then press **Assign!**

You can use [https://github.com/JakeGinnivan/TfsCreateBuild](https://github.com/JakeGinnivan/TfsCreateBuild) to then import your test results into TFS (using VSTest.Console.exe yourtestassembly.dll /logger:trx /UseVsixExtensions:true)

## Supported Frameworks
We currently support xUnit and NUnit. Report an issue if you would like to see others supported.