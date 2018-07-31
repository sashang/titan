/// Functions for managing the database.
module ServerCode.Database

open ServerCode
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

[<RequireQualifiedAccess>]
type DatabaseType =
    | FileSystem
