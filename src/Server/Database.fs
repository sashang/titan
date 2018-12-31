/// Functions for managing the database.
module Database

open Domain
open Dapper
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.Extensions.Logging
open Npgsql
open System.Threading.Tasks
open ValueDeclarations


type IDatabase =
    abstract member insert_school: School -> Task<Result<bool, string>>

type Database() = 
    interface IDatabase with
        member this.insert_school (school : School) : Task<Result<bool, string>> = task {
            try 
                use pg_connection = new NpgsqlConnection(PG_DEV_CON)
                pg_connection.Open()
                let cmd = """insert into "Schools"("Name","Principal") values(@Name,@Principal)"""
                if pg_connection.Execute(cmd, school) = 1 then  
                    return (Ok true)
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")

            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }