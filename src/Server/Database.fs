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
    abstract member insert_school: Models.School -> Task<Result<bool, string>>
    abstract member school_from_user_id: string -> Task<Result<Models.School, string>>

type Database() = 
    interface IDatabase with
        member this.insert_school (school : Models.School) : Task<Result<bool, string>> = task {
            try 
                use pg_connection = new NpgsqlConnection(PG_DEV_CON)
                pg_connection.Open()
                let cmd = """insert into "Schools"("Name","UserId", "Principal") values(@Name,@UserId,@Principal)"""
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

        member this.school_from_user_id (user_id : string) : Task<Result<Models.School, string>> = task {
            try 
                use pg_connection = new NpgsqlConnection(PG_DEV_CON)
                pg_connection.Open()
                let sql = """select "Name", "Principal" from "Schools" where "UserId" = @UserId"""
                let result = pg_connection.QueryFirst<Models.School>(sql, {Models.default_school with Models.School.UserId = user_id})
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
