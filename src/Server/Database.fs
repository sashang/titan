/// Functions for managing the database.
module Database

open Dapper
open FSharp.Control.Tasks.ContextInsensitive
open Npgsql
open System.Collections.Generic
open System.Dynamic
open System.Threading.Tasks

let private dapper_query<'Result> (query:string) (connection:NpgsqlConnection) =
    connection.Query<'Result>(query)
    
let private dapper_param_query<'Result> (query:string) (param:obj) (connection:NpgsqlConnection) : 'Result seq =
    connection.Query<'Result>(query, param)
    
let private dapper_map_param_query<'Result> (query:string) (param : Map<string,_>) (connection:NpgsqlConnection) : 'Result seq =
    let expando = ExpandoObject()
    let expandoDictionary = expando :> IDictionary<string,obj>
    for paramValue in param do
        expandoDictionary.Add(paramValue.Key, paramValue.Value :> obj)

    connection |> dapper_param_query query expando

type IDatabase =
    abstract member insert_school: Models.School -> Task<Result<bool, string>>

    /// Query the database to see if a school is linked to a user
    abstract member user_has_school: string -> Task<Result<bool, string>>

    abstract member query_id: string -> Task<Result<string, string>>

    /// Update a school given the user id
    abstract member update_school_by_user_id: Models.School -> Task<Result<bool, string>>

    abstract member school_from_user_id: string -> Task<Result<Models.School, string>>

    ///register interested partys
    abstract member register_punters: Models.Punter -> Task<Result<bool, string>>

    /// add student to Student table
    abstract member insert_student : Models.Student -> Task<Result<bool, string>>


type Database(c : string) = 
    member this.connection = c

    interface IDatabase with

        member this.insert_student (student : Models.Student) : Task<Result<bool, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """insert into "Student"("Email", "FirstName", "LastName") values(@Email, @FirstName, @LastName)"""
                if pg_connection.Execute(cmd, student) = 1 then  
                    return (Ok true)
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
                with
                | :? Npgsql.PostgresException as e ->
                    return Error e.MessageText
                |  e ->
                    return Error e.Message
        }

        member this.query_id (username : string) : Task<Result<string, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let sql = """select "Id" from "AspNetUsers" where "UserName" = @UserName"""
                let result = pg_connection
                             |> dapper_map_param_query<string> sql (Map ["UserName", username])
                             |> Seq.head
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.register_punters (punter : Models.Punter) : Task<Result<bool, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """insert into "Punter"("Email") values(@Email)"""
                if pg_connection.Execute(cmd, punter) = 1 then  
                    return (Ok true)
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        member this.user_has_school (user_id : string) : Task<Result<bool, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """select exists(select 1 from "School" where "UserId" = @UserId)"""
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

        member this.update_school_by_user_id (school : Models.School) : Task<Result<bool, string>> = task {

            try 
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """update "School" set "Name" = @Name, "Principal" = @Principal where "UserId" = @UserId"""
                if pg_connection.Execute(cmd, school) = 1 then  
                    return (Ok true)
                else
                    return Error ("Did not update the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.insert_school (school : Models.School) : Task<Result<bool, string>> = task {
            try 
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """insert into "School"("Name","UserId", "Principal") values(@Name,@UserId,@Principal)"""
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
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let sql = """select "Name", "Principal" from "School" where "UserId" = @UserId"""
                let result = pg_connection.QueryFirst<Models.School>(sql, {Models.default_school with Models.School.UserId = user_id})
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
