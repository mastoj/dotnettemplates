#r "paket:
nuget Fantomas.Extras
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fantomas.Extras.FakeHelpers
open FSharp.Compiler.Interactive.Shell.Settings

// Properties
let buildDir = "./.build/"
let publishDir = sprintf "%s/publish/" buildDir

// Helper functions
let runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let buildDocker dockerFile tag contextPath =
    let args = sprintf "build -t %s -f %s %s" tag dockerFile contextPath
    runTool "docker" args __SOURCE_DIRECTORY__

// *** Define Targets ***
Target.create "Clean" (fun _ ->
    Trace.log " --- Cleaning stuff --- "
    [ buildDir; publishDir ]
    |> List.iter Shell.cleanDir
)

Target.create "Build" (fun _ ->
    Trace.log " -- Building solution --- "
    let buildMode = Environment.environVarOrDefault "buildMode" "Release"
    let setParams (defaults:DotNet.BuildOptions) =
            { defaults with
                MSBuildParams = {
                    defaults.MSBuildParams with
                        Verbosity = Some(MSBuildVerbosity.Minimal)
                        Targets = ["Publish"]
                        Properties =
                            [
                                "Optimize", "True"
                                "DebugSymbols", "True"
                                "Configuration", buildMode
                                "PublishDir", publishDir 
                            ]
                }
            }
    DotNet.build setParams "./Simplefsharp.sln"
)

Target.create "CheckCodeFormat" (fun _ ->
    !!"src/*/*/*.fs"
    |> checkCode
    |> Async.RunSynchronously
    |> printfn "Format check result: %A")

Target.create "Format" (fun _ ->
    !!"src/*/*/*.fs"
    |> formatCode
    |> Async.RunSynchronously
    |> printfn "Formatted files: %A")

open Fake.Core.TargetOperators

// *** Define Dependencies ***
"Clean"
    ==> "CheckCodeFormat"
    ==> "Build"

Target.runOrDefault "Build"