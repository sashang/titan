/// Functions for managing the database.
module Database

open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

[<RequireQualifiedAccess>]
type DatabaseType =
    | FileSystem

type IDatabaseFunctions =
    abstract member load_schools: Task<Domain.Schools>
    abstract member add_user: string -> string -> Task<bool>

let get_database db_type =
    match db_type with
    | DatabaseType.FileSystem ->
        { new IDatabaseFunctions with
            member __.load_schools = task { return FileSystemDatabase.load_schools }
            member __.add_user username password = task { return FileSystemDatabase.add_user username password } }
