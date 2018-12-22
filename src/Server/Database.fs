/// Functions for managing the database.
module Database

open Domain
open Dapper
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql
open System.Threading.Tasks
open ValueDeclarations


type IDatabase =
    abstract member insert_school: CreateSchool -> Task<bool>

type Database() =
    interface IDatabase with
        member this.insert_school (school : CreateSchool) : Task<bool> = task {
            use pg_connection = new NpgsqlConnection(PG_DEV_CON)
            pg_connection.Open()
            let cmd = "insert into \"Schools\"(\"Name\",\"Principal\") values(@Name,@Principal)"
            return pg_connection.Execute(cmd, school) = 1
        }