#r "tools/FAKE/tools/FakeLib.dll"
open Fake

let projName="CavemanTools.Persistence.Sql"
let projDir= "..\src" @@ projName
let testDir="..\src" @@ "Tests"

let localNugetRepo="E:/Libs/nuget"
let nugetExeDir="tools"



