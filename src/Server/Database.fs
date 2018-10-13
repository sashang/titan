/// Functions for managing the database.
module Database

open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

[<RequireQualifiedAccess>]
type DatabaseType =
    | FileSystem

type IDatabaseFunctions =
    abstract member load_schools : Task<Domain.Schools>

let get_database db_type =
    match db_type with
    | DatabaseType.FileSystem ->
        { new IDatabaseFunctions with
            member __.load_schools = task { return FileSystemDatabase.load_schools } }
