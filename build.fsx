#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing.XUnit2

let projectName = "Lib"
let buildDir = "./build/"
let buildProjectFiles = 
  !! ("**/" + projectName + ".csproj")
let testDir = "./test/"
let testProjectFiles =
  !! ("**/" + projectName + ".Tests.fsproj")
let testDlls =
  !! (testDir + "**/*.Tests.dll")
let packagesDir = "./packages/"

let clean _ =
    CleanDirs [buildDir; testDir; packagesDir;]

let buildRelease _ =
  MSBuildRelease buildDir "Build" buildProjectFiles
    |> ignore

let buildReleaseTests _ =
  MSBuildRelease testDir "Build" testProjectFiles
    |> ignore
  
let testRelease _ =
  testDlls
    |> xUnit2 (fun p -> p)//{ p with ForceAppVeyor = true }

let restorePackages _ =
  RestorePackages |> ignore

Target "Clean" clean
Target "RestorePackages" restorePackages
Target "BuildRelease" buildRelease 
Target "BuildReleaseTests" buildReleaseTests
Target "TestRelease" testRelease

"Clean" ==> "RestorePackages"
"RestorePackages" ==> "BuildRelease"
"RestorePackages" ==> "BuildReleaseTests"
"BuildRelease" ==> "BuildReleaseTests"
"BuildReleaseTests" ==> "TestRelease"

RunTargetOrDefault "TestRelease"