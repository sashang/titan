/// Functions for managing the database.
module Database

open Dapper
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql
open System.Collections.Generic
open System.Data.SqlClient
open System.Dynamic
open System.Threading.Tasks
open ValueDeclarations

let dapper_query<'Result> (query:string) (connection:NpgsqlConnection) =
    connection.Query<'Result>(query)
    
let dapper_param_query<'Result> (query:string) (param:obj) (connection:NpgsqlConnection) : 'Result seq =
    connection.Query<'Result>(query, param)
    
let dapper_map_param_query<'Result> (query:string) (param : Map<string,_>) (connection:NpgsqlConnection) : 'Result seq =
    let expando = ExpandoObject()
    let expandoDictionary = expando :> IDictionary<string,obj>
    for paramValue in param do
        expandoDictionary.Add(paramValue.Key, paramValue.Value :> obj)

    connection |> dapper_param_query query expando

type IDatabase =
    abstract member insert_school: Models.School -> Task<Result<bool, string>>

    /// Query the database to see if a school is linked to a user
    abstract member user_has_school: string -> Task<Result<bool, string>>

    /// Update a school given the user id
    abstract member update_school: string -> Models.School -> Task<Result<bool, string>>

    abstract member school_from_user_id: string -> Task<Result<Models.School, string>>


type private Exists = {t_or_f : string}

type Database() = 
    interface IDatabase with
        
        member this.user_has_school (user_id : string) : Task<Result<bool, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(PG_DEV_CON)
                pg_connection.Open()
                let cmd = """select exists(select 1 from "Schools" where "UserId" = @UserId)"""
                let exists = pg_connection
                             |> dapper_map_param_query<bool> cmd (Map ["UserId", user_id])
                             |> Seq.head
                return Ok exists
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.update_school (user_id : string) (school : Models.School) : Task<Result<bool, string>> = task {

            try 
                use pg_connection = new NpgsqlConnection(PG_DEV_CON)
                pg_connection.Open()
                let cmd = """update "Schools" set "Name" = @Name, "Principal" = @Principal)"""
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
