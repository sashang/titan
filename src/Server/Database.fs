/// Functions for managing the database.
module Database

open Dapper
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.Extensions.Logging
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
    let dict = expando :> IDictionary<string,obj>
    for value in param do
        dict.Add(value.Key, value.Value :> obj)

    connection |> dapper_param_query query expando

let private dapper_map_param_exec(sql : string) (param : Map<string,_>) (connection : NpgsqlConnection) : int =
    let expando = ExpandoObject()
    let dict = expando :> IDictionary<string,obj>
    for value in param do
        dict.Add(value.Key, value.Value :> obj)

    connection.Execute(sql, expando)

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
    
    /// query to get all students
    abstract member query_all_students : Task<Result<Models.Student list, string>>

    abstract member query_all_schools : Task<Result<Models.School list, string>>

    /// query to get all pending students
    abstract member query_pending : Task<Result<Models.Student list, string>>

    /// insert student school mapping
    abstract member insert_student_school : string -> string -> Task<Result<unit, string>>

    /// insert student into pending table for enrollement approval 
    abstract member insert_pending : Models.Student -> string -> Task<Result<unit, string>>

    /// delete pending student with given email
    abstract member delete_pending : string -> Task<Result<unit, string>>

    /// delete pending for a tutor
    abstract member delete_pending_for_tutor : string -> string -> Task<Result<unit, string>>

    
type Database(c : string) = 
    member this.connection = c

    interface IDatabase with
        member this.insert_student_school student_email tutor_user_id : Task<Result<unit, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """insert into "StudentSchool" ("StudentId","SchoolId") VALUES ((select "Id" from "Student" where "Student"."Email" = @Email),(select "School"."Id" from "School" inner join "AspNetUsers" on "AspNetUsers"."Id" = @Id))"""
                let m = (Map ["Email", student_email; "Id", tutor_user_id])
                if dapper_map_param_exec cmd m pg_connection = 1 then  
                    return Ok ()
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.delete_pending_for_tutor email user_id : Task<Result<unit, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """delete from "Pending" where ("Pending"."Email" = @Email and "SchoolId" = (select "School"."Id" from "School" where "School"."UserId" = @UserID))"""
                let m = (Map ["Email", email; "UserId", user_id])
                if dapper_map_param_exec cmd m pg_connection = 1 then  
                    return Ok ()
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        member this.delete_pending email : Task<Result<unit, string>> = task {
            try
                //normally we validate the email address using asp.net but incase that doesn't happen
                //and we call this function with an empty email then this will trigger.
                if email = "" then
                    raise (failwith "Email cannot be empty")
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """delete from "Pending" where "Email" = @Email"""
                let m = (Map ["Email", email])
                if dapper_map_param_exec cmd m pg_connection = 1 then  
                    return Ok ()
                else
                    return Error ("Did not delete the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.insert_pending (student : Models.Student) (school_name : string) : Task<Result<unit, string>> = task {
            try
                //postgres treats empty strings and null as different. AN empty string is a value in postgres
                //so we have to explicity check this here.
                if student.FirstName = "" || student.LastName = "" then 
                    raise (failwith "First name or last name cannot be empty")

                //normally we validate the email address using asp.net but incase that doesn't happen
                //and we call this function with an empty email then this will trigger.
                if student.Email = "" then
                    raise (failwith "Email cannot be empty")
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let cmd = """insert into "Pending" ("FirstName","LastName","Email","SchoolId") VALUES (@FirstName, @LastName, @Email,(select "School"."Id" from "School" where "School"."Name" = @SchoolName))"""
                let m = (Map ["Email", student.Email; "FirstName", student.FirstName; "LastName", student.FirstName; "SchoolName", school_name ])
                if dapper_map_param_exec cmd m pg_connection = 1 then  
                //if pg_connection.Execute(cmd, m) = 1 then  
                    return Ok ()
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }


        member this.query_pending : Task<Result<Models.Student list, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let sql = """select "FirstName", "LastName", "Email" from "Pending";"""
                let result = pg_connection
                             |> dapper_query<Models.Student> sql
                             |> Seq.toList
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }


        member this.query_all_schools : Task<Result<Models.School list, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let sql = """select * from "School";"""
                let result = pg_connection
                             |> dapper_query<Models.School> sql
                             |> Seq.toList
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.query_all_students : Task<Result<Models.Student list, string>> = task {
            try
                use pg_connection = new NpgsqlConnection(this.connection)
                pg_connection.Open()
                let sql = """select "FirstName", "LastName", "Email" from "Student";"""
                let result = pg_connection
                             |> dapper_query<Models.Student> sql
                             |> Seq.toList
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

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
