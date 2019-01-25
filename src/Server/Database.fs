/// Functions for managing the database.
module Database



open Dapper
open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.Extensions.Logging
open Npgsql
open System.Data.SqlClient
open System.Collections.Generic
open System.Dynamic
open System.Threading.Tasks


let private dapper_query<'Result> (query:string) (connection:SqlConnection) =
    connection.Query<'Result>(query)
    
let private dapper_param_query<'Result> (query:string) (param:obj) (connection:SqlConnection) : 'Result seq =
    connection.Query<'Result>(query, param)
    
let private dapper_map_param_query<'Result> (query:string) (param : Map<string,_>) (connection:SqlConnection) : 'Result seq =
    let expando = ExpandoObject()
    let dict = expando :> IDictionary<string,obj>
    for value in param do
        dict.Add(value.Key, value.Value :> obj)

    connection |> dapper_param_query query expando

let private dapper_map_param_exec(sql : string) (param : Map<string,_>) (connection : SqlConnection) : int =
    let expando = ExpandoObject()
    let dict = expando :> IDictionary<string,obj>
    for value in param do
        dict.Add(value.Key, value.Value :> obj)
    connection.Execute(sql, expando)

type IDatabase =

    abstract member delete_student_from_school: string -> string -> Task<Result<unit, string>>
    /// get a list of claims for the user based on their email
    abstract member query_claims: string -> Task<Result<Models.TitanClaims list, string>>

    abstract member insert_tutor: string -> string -> string -> string -> Task<Result<unit, string>>
    abstract member insert_student: string -> string -> string -> Task<Result<unit, string>>
    abstract member insert_school: Models.School -> Task<Result<bool, string>>

    /// Query the database to see if a school is linked to a user
    abstract member user_has_school: string -> Task<Result<bool, string>>

    abstract member handle_save_request: string -> SaveRequest -> Task<Result<unit, string>>
    /// Update a school given the user id
    abstract member update_school_by_user_id: Models.School -> Task<Result<bool, string>>

    abstract member school_from_user_id: string -> Task<Result<Models.School, string>>
    
    abstract member school_from_email: string -> Task<Result<SchoolResponse, string>>
    
    abstract member user_from_email: string -> Task<Result<Models.User, string>>

    ///register interested partys
    abstract member register_punters: Models.Punter -> Task<Result<bool, string>>

    /// query to get all students for a tutor
    abstract member query_students : string -> Task<Result<Domain.Student list, string>>
    
    /// 
    abstract member query_all_schools : Task<Result<Models.School list, string>>

    /// query to get all pending students
    abstract member query_pending : string -> Task<Result<Models.PendingStudent list, string>>

    /// insert student school mapping
    abstract member insert_student_school : string -> string -> Task<Result<unit, string>>

    /// delete pending student with given email
    abstract member delete_pending : string -> Task<Result<unit, string>>

    /// delete pending for a tutor
    abstract member delete_pending_for_tutor : string -> string -> Task<Result<unit, string>>
    
    /// update user given email
    abstract member update_user : string->string -> string -> Task<Result<unit, string>>
    
    /// update school name given email
    abstract member update_school_name : string -> string -> Task<Result<unit, string>>
    
    /// get list of school names and tutors
    abstract member get_school_view : Task<Result<School list, string>>
    
    abstract member insert_enrol_request : string -> string -> Task<Result<unit, string>>
    
    //approve enrol request. Means creating a user in the school table that the user wants to
    //enrol with, then removing the user from the pending table
    abstract member approve_enrol_request : string -> string -> Task<Result<unit, string>>
    

    
type Database(c : string) = 
    member this.connection = c

    interface IDatabase with
        member this.delete_student_from_school tutor_email student_email : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """delete from "Student" where "Student"."UserId" =
                             (select "Id" from "User" where "User"."Email" = @StudentEmail) and
                             "Student"."SchoolId" = (select "Id" from "School" where "School"."UserId" =
                             (select "User"."Id" from "User" where "User"."Email" = @TutorEmail))"""
                let m = (Map ["StudentEmail", student_email; "TutorEmail", tutor_email])
                if dapper_map_param_exec sql m conn = 1 then  
                    return Ok ()
                else 
                    return Error ("Did not insert the expected number of records. sql is \"" + sql + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        
        member this.get_school_view : Task<Result<School list, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select "School"."Name","User"."FirstName","User"."LastName","School"."Info",
                             "School"."Subjects", "School"."Location" from "School" join "User" on "User"."Id" = "School"."UserId";"""
                let result = conn
                             |> dapper_query<Models.SchoolTutor> sql
                             |> Seq.toList
                             |> List.map (fun x -> School.init x.FirstName x.LastName x.SchoolName x.Info x.Subjects x.Location)
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        
        
        member this.update_school_name school_name email : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """update "School" set "Name" = @Name where "UserId" = (select "Id" from "User" where "Email" = @Email)"""
                let m = (Map ["Email", email; "Name", school_name])
                if dapper_map_param_exec cmd m conn = 1 then  
                    return Ok ()
                else 
                    return Error ("Did not update the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        member this.handle_save_request tutor_email request : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """update "School" set "Name" = @Name, "Info" = @Info, "Subjects" = @Subjects, "Location" = @Location
                             where "UserId" = (select "Id" from "User" where "Email" = @Email)"""
                let m = (Map ["Email", tutor_email; "Name", request.SchoolName; "Info", request.Info; "Subjects", request.Subjects;
                              "Location", request.Location])
                if dapper_map_param_exec cmd m conn = 1 then  
                    let cmd = """update "User" set "FirstName" = @FirstName, "LastName" = @LastName
                                 where "Email" = @Email"""
                    let m = (Map ["Email", tutor_email; "FirstName", request.FirstName; "LastName", request.LastName;])
                    if dapper_map_param_exec cmd m conn = 1 then  
                        return Ok ()
                    else 
                        return Error ("Did not update the expected number of records. sql is \"" + cmd + "\"")
                else
                    return Error ("Did not update the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        
        member this.update_user first last email : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """update "User" set "FirstName" = @FirstName, "LastName" = @LastName where "Email" = @Email"""
                let m = (Map ["Email", email; "FirstName", first; "LastName", last])
                if dapper_map_param_exec cmd m conn = 1 then  
                    return Ok ()
                else 
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        
        member this.insert_enrol_request student_email school_name : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """insert into "Pending" ("UserId", "SchoolId") values ((select "Id" from "User" where "Email" = @Email),
                             (select "Id" from "School" where "Name" = @Name))"""
                let m = (Map ["Email", student_email; "Name", school_name])
                if dapper_map_param_exec cmd m conn = 1 then  
                    return Ok ()
                else 
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        member this.insert_tutor first last schoolname email : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """insert into "User" ("FirstName","LastName","Email", "Phone") VALUES (@FirstName, @LastName, @Email, @Phone)"""
                let m = (Map ["Email", email; "FirstName", first; "LastName", last; "Phone", ""])
                if dapper_map_param_exec cmd m conn = 1 then  
                    let cmd = """insert into "TitanClaims" ("UserId","Type","Value") VALUES
                        ((select "Id" from "User" where "Email" = @Email), 'IsTutor', 'true')"""
                    let m = (Map ["Email", email])
                    if dapper_map_param_exec cmd m conn = 1 then  
                        let cmd = """insert into "School" ("UserId","Name","Info","Subjects","Location")
                            VALUES ((select "User"."Id" from "User" where "Email" = @Email), @Name, @Info, @Subjects, @Location)"""
                        let m = (Map ["Email", email;"Name", schoolname;"Info", "";"Subjects", ""; "Location", ""])
                        if dapper_map_param_exec cmd m conn = 1 then  
                            return Ok ()
                        else 
                            return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
                    else
                        return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        member this.insert_student first last email : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """insert into "User" ("FirstName","LastName","Email","Phone") VALUES (@FirstName, @LastName, @Email, @Phone)"""
                let m = (Map ["Email", email; "FirstName", first; "LastName", last; "Phone", ""])
                if dapper_map_param_exec cmd m conn = 1 then  
                    let cmd = """insert into "TitanClaims" ("UserId","Type","Value") VALUES
                        ((select "Id" from "User" where "Email" = @Email), 'IsStudent', 'true')"""
                    let m = (Map ["Email", email])
                    if dapper_map_param_exec cmd m conn = 1 then  
                        return Ok ()
                    else
                        return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.query_claims email : Task<Result<Models.TitanClaims list, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select * from "TitanClaims" where ("UserId" = (select "Id" from "User" where "Email" = @Email))"""
                let result = conn
                             |> dapper_map_param_query<Models.TitanClaims> sql (Map["Email", email])
                             |> Seq.toList
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.insert_student_school student_email tutor_user_id : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """insert into "StudentSchool" ("StudentId","SchoolId") VALUES ((select "Id" from "Student" where "Student"."Email" = @Email),(select "School"."Id" from "School" inner join "AspNetUsers" on "AspNetUsers"."Id" = @Id))"""
                let m = (Map ["Email", student_email; "Id", tutor_user_id])
                if dapper_map_param_exec cmd m conn = 1 then  
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
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """delete from "Pending" where ("Pending"."Email" = @Email and "SchoolId" = (select "School"."Id" from "School" where "School"."UserId" = @UserID))"""
                let m = (Map ["Email", email; "UserId", user_id])
                if dapper_map_param_exec cmd m conn = 1 then  
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
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """delete from "Pending" where "Email" = @Email"""
                let m = (Map ["Email", email])
                if dapper_map_param_exec cmd m conn = 1 then  
                    return Ok ()
                else
                    return Error ("Did not delete the expected number of records. sql is \"" + cmd + "\"")
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }


        member this.query_pending tutor_email : Task<Result<Models.PendingStudent list, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select "User"."FirstName","User"."LastName", "User"."Phone", "User"."Email"
                             from "Pending" join "User" on "User"."Id" = "Pending"."UserId";"""
                let result = conn
                             |> dapper_query<Models.PendingStudent> sql
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
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select * from "School";"""
                let result = conn
                             |> dapper_query<Models.School> sql
                             |> Seq.toList
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.query_students tutor_email : Task<Result<Domain.Student list, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                //holy fucking shit sql can get complicated. I built this up backwards, i.e. worked it out from the last
                //select statememt.
                let sql = """select "User"."FirstName","User"."LastName","User"."Email","User"."Phone" from "User" join "Student"
                             on "User"."Id" = "Student"."UserId" where "Student"."SchoolId" =
                             (select "School"."Id" from "School" where "School"."UserId" =
                             (select "User"."Id" from "User" where "User"."Email" = @Email));"""
                let result = conn
                             |> dapper_map_param_query<Domain.Student> sql (Map["Email", tutor_email])
                             |> Seq.toList
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.register_punters (punter : Models.Punter) : Task<Result<bool, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """insert into "Punter"("Email") values(@Email)"""
                if conn.Execute(cmd, punter) = 1 then  
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
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """select exists(select 1 from "School" where "UserId" = @UserId)"""
                let exists = conn
                             |> dapper_map_param_query<bool> cmd (Map ["UserId", user_id])
                             |> Seq.head
                return Ok exists
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        
        member this.user_from_email (email : string) : Task<Result<Models.User, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """select * from "User" where "User"."Email" = @Email"""
                let result = conn
                                |> dapper_map_param_query<Models.User> cmd (Map ["Email", email])
                                |> Seq.head
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }

        member this.school_from_email (email : string) : Task<Result<SchoolResponse, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """select "Name","Info","Subjects","Location" from "School" where "School"."UserId" = (select "Id" from "User" where "Email" = @Email)"""
                let result = conn
                                |> dapper_map_param_query<Models.SchoolFromEmail> cmd (Map ["Email", email])
                                |> Seq.head
                                |> (fun (x : Models.SchoolFromEmail) -> {SchoolResponse.Info = x.Info; SchoolResponse.Subjects = x.Subjects
                                                                         SchoolResponse.SchoolName = x.SchoolName;
                                                                         SchoolResponse.Location = x.Location; Error = None })
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        
        member this.update_school_by_user_id (school : Models.School) : Task<Result<bool, string>> = task {

            try 
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """update "School" set "Name" = @Name, "Principal" = @Principal where "UserId" = @UserId"""
                if conn.Execute(cmd, school) = 1 then  
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
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """insert into "School"("Name","UserId", "Principal") values(@Name,@UserId,@Principal)"""
                if conn.Execute(cmd, school) = 1 then  
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
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select "Name", "Principal" from "School" where "UserId" = @UserId"""
                let result = conn.QueryFirst<Models.School>(sql, {Models.School.init with Models.School.UserId = user_id})
                return Ok result
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
        
        //aproving an enrolment means adding a user to the school and then removing the request.
        member this.approve_enrol_request (tutor_email : string) (student_email : string) : Task<Result<unit, string>> = task {
            try 
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select "Id" from "User" where "User"."Email" = @Email"""
                let student_id = conn
                                |> dapper_map_param_query<int> sql (Map ["Email", student_email])
                                |> Seq.head
                //we have the id of the student - lets add the student to the tutor's school
                let sql = """select "Id" from "School" where "School"."UserId" = (select "Id" from "User" where "User"."Email" = @Email)"""
                let tutor_id = conn
                               |> dapper_map_param_query<int> sql (Map ["Email", tutor_email])
                               |> Seq.head
                
                let cmd = """insert into "Student"("UserId", "SchoolId") values(@UserId,@SchoolId)"""
                let m = (Map ["UserId", student_id; "SchoolId", tutor_id])
                if dapper_map_param_exec cmd m conn = 1 then  
                    //delete student from pending table
                    let cmd = """delete from "Pending" where "UserId" = @UserId"""
                    let m = (Map ["UserId", student_id])
                    if dapper_map_param_exec cmd m conn = 1 then  
                        return Ok ()
                    else
                        return Error ("Failed to delete user. sql is \"" + cmd + "\"")
                else
                    return Error ("Failed to insert user. sql is \"" + cmd + "\"")
                    
            with
            | :? Npgsql.PostgresException as e ->
                return Error e.MessageText
            |  e ->
                return Error e.Message
        }
