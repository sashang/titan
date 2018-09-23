module FileSystemDatabase

open System.IO
open Domain
open Newtonsoft.Json

let load_schools =
    let fi = FileInfo("./filesystem-db/schools.json")
    File.ReadAllText(fi.FullName)
    |> FableJson.from_json<School>
