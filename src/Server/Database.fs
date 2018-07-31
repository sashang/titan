/// Functions for managing the database.
module ServerCode.Database

open ServerCode
open System.Threading.Tasks
open FSharp.Control.Tasks.ContextInsensitive

[<RequireQualifiedAccess>]
type DatabaseType =
    | FileSystem

type IDatabaseFunctions =
    abstract member LoadWishList : string -> Task<Domain.WishList>
    abstract member SaveWishList : Domain.WishList -> Task<unit>
    abstract member GetLastResetTime : unit -> Task<System.DateTime>

/// Start the web server and connect to database
let getDatabase databaseType startupTime =
    match databaseType with

    | DatabaseType.FileSystem ->
        { new IDatabaseFunctions with
            member __.LoadWishList key = task { return Storage.FileSystem.getWishListFromDB key }
            member __.SaveWishList wishList = task { return Storage.FileSystem.saveWishListToDB wishList }
            member __.GetLastResetTime () = task { return startupTime } }

