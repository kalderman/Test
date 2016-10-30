#r @".tools/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing.XUnit2

type ProjectInfo =
  {
    Name : string
    Description : string
    Authors : string list
  }

let projectInfo = 
  {
    Name = "Lib"
    Description = "test"
    Authors = [ "kalderman" ]
  }
let xunitConsolePath = ".tools/xunit.runner.console/tools/xunit.console.exe"

let buildDir = "./build/"
let buildProjectFiles = 
  !! ("**/" + projectInfo.Name + ".csproj")
let testDir = "./test/"
let testProjectFiles =
  !! ("**/" + projectInfo.Name + ".Tests.fsproj")
let testDlls =
  !! (testDir + "**/*.Tests.dll")
let packagesDir = "./packages/"
let isAppVeyorBuild = buildServer = BuildServer.AppVeyor
let artifactsDir = "./artifacts/"

let clean _ =
    CleanDirs [buildDir; testDir; packagesDir; artifactsDir;]

let buildRelease _ =
  MSBuildRelease buildDir "Build" buildProjectFiles
    |> ignore

let buildReleaseTests _ =
  MSBuildRelease testDir "Build" testProjectFiles
    |> ignore
  
let testRelease _ =
  testDlls
    |> xUnit2 (fun p -> 
      { p with 
          ToolPath = xunitConsolePath
          ShadowCopy = false 
          ForceAppVeyor = isAppVeyorBuild})

let restorePackages _ =
  RestorePackages()

let buildNugetPackage _ = 
  NuGet (fun p -> 
      {p with
        Authors = projectInfo.Authors
        Project = projectInfo.Name
        Description = projectInfo.Description
        OutputPath = artifactsDir
        Summary = ""
        WorkingDir = buildDir
        Version = (if buildVersion = "LocalBuild" then "0.0.0.0-pre" else buildVersion)
        Publish = false 
        Files = !! (buildDir + "*.dll") |> Seq.map (fun f -> (f, Some "lib", None)) |> Seq.toList }) 
        (projectInfo.Name + ".nuspec")

module AppVeyor =  
  let execOnAppveyor arguments =
    let result =
      ExecProcess (fun info ->
        info.FileName <- "appveyor"
        info.Arguments <- arguments
        ) (System.TimeSpan.FromMinutes 2.0)
    if result <> 0 then failwith (sprintf "Failed to execute appveyor command: %s" arguments)

  let publishNugetPackage folder =
    !! (folder + "*.nupkg")
    |> Seq.iter (fun artifact -> execOnAppveyor (sprintf "PushArtifact %s" artifact))

let deployNugetPackage _ = 
  AppVeyor.publishNugetPackage artifactsDir

Target "Clean" clean
Target "RestorePackages" restorePackages
Target "BuildRelease" buildRelease 
Target "BuildReleaseTests" buildReleaseTests
Target "TestRelease" testRelease
Target "BuildNugetPackage" buildNugetPackage
Target "DeployNugetPackage" deployNugetPackage
Target "Default" ignore

"Clean" 
  ==> "RestorePackages"
  ==> "BuildRelease"
  ==> "BuildReleaseTests"
  ==> "TestRelease"
  ==> "BuildNugetPackage"
  =?> ("DeployNugetPackage", isAppVeyorBuild)
  ==> "Default"

RunTargetOrDefault "Default"