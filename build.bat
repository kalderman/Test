@echo off
cls
".nuget\NuGet.exe" "Install" "FAKE" "-OutputDirectory" ".tools" "-ExcludeVersion"
".nuget\NuGet.exe" "Install" "xunit.runner.console" "-OutputDirectory" ".tools" "-ExcludeVersion"
".tools\FAKE\tools\Fake.exe" build.fsx