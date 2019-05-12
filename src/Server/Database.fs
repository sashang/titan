/// Functions for managing the database.
module Database



open Dapper
open Domain
open FSharp.Control.Tasks.ContextInsensitive
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
    abstract member has_claim: string -> string -> Task<Result<bool, string>>

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

    abstract member update_user_claim : string -> string -> string -> Task<Result<unit, string>>

    abstract member insert_user_claim : string -> string -> string -> Task<Result<unit, string>>
    
    abstract member get_enrolled_schools : string -> Task<Result<School list, string>>

    abstract member get_unenrolled_schools : string -> Task<Result<School list, string>>

    //get the unapproved users for the titan(titan is our code word for admin) user
    abstract member get_unapproved_users_for_titan : unit -> Task<Result<Domain.UsersForTitanResponse, string>>

    //get all the users
    abstract member get_users_for_titan : unit -> Task<Result<Domain.UsersForTitanResponse, string>>

    ///get the pending schools (i.e. the schools that the student has requested enrolement in) given a student's email.
    abstract member get_pending_schools : string -> Task<Result<Domain.SchoolsResponse, string>>

    
type Database(c : string) = 
    member this.connection = c

    interface IDatabase with
        member this.get_pending_schools (email : string) : Task<Result<Domain.SchoolsResponse, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                //the order the table fields appear in the sql select has to match the order in the Models.SchoolTutor
                //this sql statement needs a comment. We want the tutors info and the school. We're given the email of the student.
                //Pending table links to User via UserId, School links to Pending via SchoolId
                //Tutor links with School via SchoolId and links with User via UserId.
                //Then we want to filter all of that based on the students email.
                let sql = """select "School"."Name","User"."FirstName","User"."LastName","School"."Info",
                             "School"."Subjects", "School"."Location", "User"."Email" from "User"
                             join "Pending" on "User"."Id" = "Pending"."UserId"
                             join "School" on "School"."Id" = "Pending"."SchoolId"
                             join "Tutor" on "Tutor"."UserId" = "User"."Id" where "User"."Email" = @Email;"""
                
                let result = conn
                             |> dapper_map_param_query<Models.SchoolTutor> sql (Map["Email", email])
                             |> Seq.toList
                             |> List.map (fun x -> School.init x.FirstName x.LastName x.SchoolName x.Info x.Subjects x.Location x.Email)
                return Ok {Schools = result; Error = None}
            with
            |  e ->
                return Error e.Message
        }

        member this.get_unapproved_users_for_titan () : Task<Result<Domain.UsersForTitanResponse, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select "User"."FirstName","User"."LastName","User"."Email",
                             "TitanClaims"."Type","TitanClaims"."Value" from "User"
                             join "TitanClaims" on "TitanClaims"."UserId" = "User"."Id"
                             where "User"."Id" not in
                             (select "User"."Id" from "User" join "TitanClaims" as "TC" on "User"."Id" = "TC"."UserId"
                             where "TC"."Type" = 'IsApproved' and "TC"."Value" = 'true');"""
                let result =
                    conn
                    |> dapper_query<Models.UserForTitan> sql
                    |> Seq.toList
                    |> List.groupBy (fun x -> x.Email)
                    |> List.map (fun (key, values) -> 
                        List.fold (fun (state : Domain.UserForTitan) (x : Models.UserForTitan) ->
                            {state with FirstName = x.FirstName; LastName = x.LastName; Email = x.Email;
                                        IsApproved = state.IsApproved || (x.Type = "IsApproved" && x.Value = "true");
                                        IsTitan = state.IsTitan || (x.Type = "IsTitan" && x.Value = "true");
                                        IsTutor = state.IsTutor || (x.Type = "IsTutor" && x.Value = "true");
                                        IsStudent = state.IsStudent || (x.Type = "IsStudent" && x.Value = "true")}) Domain.UserForTitan.init values)
                return Ok {Users = result; Error = None}
            with
            |  e ->
                return Error e.Message
        }

        member this.get_users_for_titan () : Task<Result<Domain.UsersForTitanResponse, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select "User"."FirstName","User"."LastName","User"."Email",
                             "TitanClaims"."Type","TitanClaims"."Value" from "User" join "TitanClaims" on "User"."Id" = "TitanClaims"."UserId";"""
                let result =
                    conn
                    |> dapper_query<Models.UserForTitan> sql
                    |> Seq.toList
                    |> List.groupBy (fun x -> x.Email)
                    |> List.map (fun (key, values) -> 
                        List.fold (fun (state : Domain.UserForTitan) (x : Models.UserForTitan) ->
                            {state with FirstName = x.FirstName; LastName = x.LastName; Email = x.Email;
                                        IsApproved = state.IsApproved || (x.Type = "IsApproved" && x.Value = "true");
                                        IsTitan = state.IsTitan || (x.Type = "IsTitan" && x.Value = "true");
                                        IsTutor = state.IsTutor || (x.Type = "IsTutor" && x.Value = "true");
                                        IsStudent = state.IsStudent || (x.Type = "IsStudent" && x.Value = "true")}) Domain.UserForTitan.init values)
                return Ok {Users = result; Error = None}
            with
            |  e ->
                return Error e.Message
        }

        member this.get_enrolled_schools student_email : Task<Result<School list, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select "TS"."Name","TU"."FirstName","TU"."LastName","TS"."Info",
                             "TS"."Subjects", "TS"."Location", "TU"."Email"
                             from "User" as "TU" join "Tutor" on "Tutor"."UserId" = "TU"."Id"
                             join "School" as "TS" on "TS"."Id" = "Tutor"."SchoolId"
                             join "Student" on "Student"."SchoolId" = "Tutor"."SchoolId"
                             join "User" as "SU" on "SU"."Id" = "Student"."UserId"
                             where "SU"."Email" = @Email;"""
                let result = conn
                             |> dapper_map_param_query<Models.SchoolTutor> sql (Map["Email", student_email])
                             |> Seq.toList
                             |> List.map (fun x -> School.init x.FirstName x.LastName x.SchoolName x.Info x.Subjects x.Location x.Email)
                return Ok result
            with
            |  e ->
                return Error e.Message
        }

        member this.delete_student_from_school tutor_email student_email : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """delete from "Student" where "Student"."UserId" =
                             (select "Id" from "User" where "User"."Email" = @StudentEmail) and
                             "Student"."SchoolId" = (select "SchoolId" from "Tutor" where "Tutor"."UserId" =
                             (select "User"."Id" from "User" where "User"."Email" = @TutorEmail))"""
                let m = (Map ["StudentEmail", student_email; "TutorEmail", tutor_email])
                if dapper_map_param_exec sql m conn = 1 then  
                    return Ok ()
                else 
                    return Error ("Did not insert the expected number of records. sql is \"" + sql + "\"")
            with
            |  e ->
                return Error e.Message
        }

        member this.get_unenrolled_schools student_email : Task<Result<School list, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                //tutor must be approved for their school to be valid so we check for it in the sql.
                //The sql builds up a table of schools and tutors of those schools. then using a left join links with the students.
                //This creates a new dynamic table and missing entries will be null where the school has no students or the email of the student is not the 
                //email in the parameter passed to the sql. In those cases the student is not enrolled in the school because their email
                //is not the same as the email referenced by the query.
                let sql = """select "School"."Name","TU"."FirstName","TU"."LastName","School"."Info","School"."Subjects", "School"."Location", "TU"."Email"
                             from "School" join "Tutor" on "School"."Id" = "Tutor"."SchoolId"
                             join "User" as "TU" on "TU"."Id" = "Tutor"."UserId"
                             join "TitanClaims" on "TU"."Id" = "TitanClaims"."UserId" and "TitanClaims"."Type" = 'IsApproved' and "TitanClaims"."Value" = 'true'
                             where "School"."Id" not in 
                             (select "Student"."SchoolId" from Student join "User" on "User"."Id" = "Student"."UserId" where "User"."Email" = @StudentEmail
                              union select "Pending"."SchoolId" from "Pending" join "User" on "User"."Id" = "Pending"."UserId" where "User"."Email" = @StudentEmail);
                             """
                let m = (Map ["StudentEmail", student_email])
                let result = conn
                             |> dapper_map_param_query<Models.SchoolTutor> sql m
                             |> Seq.toList
                             |> List.map (fun x -> School.init x.FirstName x.LastName x.SchoolName x.Info x.Subjects x.Location x.Email)
                return Ok result
            with
            |  e ->
                return Error e.Message
        }
        
        member this.get_school_view : Task<Result<School list, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                //tutor must be approved for their school to be valid so we check for it in the sql.
                let sql = """select "School"."Name","User"."FirstName","User"."LastName","School"."Info","School"."Subjects", "School"."Location", "User"."Email"
                             from Tutor join "School" on "School"."Id" = "Tutor"."SchoolId"
                             join "User" on "User"."Id" = "Tutor"."UserId"
                             join "TitanClaims" on "User"."Id" = "TitanClaims"."UserId" where "TitanClaims"."Type" = 'IsApproved' and "TitanClaims"."Value" = 'true';"""
                let result = conn
                             |> dapper_query<Models.SchoolTutor> sql
                             |> Seq.toList
                             |> List.map (fun x -> School.init x.FirstName x.LastName x.SchoolName x.Info x.Subjects x.Location x.Email)
                return Ok result
            with
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
            |  e ->
                return Error e.Message
        }
        member this.handle_save_request tutor_email request : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """update "School" set "Name" = @Name, "Info" = @Info, "Subjects" = @Subjects, "Location" = @Location
                             from "School" join "Tutor" on "Tutor"."SchoolId" = "School"."Id"
                             join "User" on "Tutor"."UserId" = "User"."Id"
                             where "User"."Email" = @Email;"""
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
            |  e ->
                return Error e.Message
        }
        
        member this.insert_user_claim email claim value : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """insert into "TitanClaims" ("UserId", "Type", "Value") values 
                             ((select "User"."Id" from "User" where "User"."Email" = @Email), @Type, @Value)"""
                let m = (Map ["Email", email; "Type", claim; "Value", value])
                if dapper_map_param_exec cmd m conn = 1 then  
                    return Ok ()
                else 
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
            |  e ->
                return Error e.Message
        }

        member this.update_user_claim email claim value : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """update "TitanClaims" set "TitanClaims"."Value" = @Value from "User"
                             join "TitanClaims" on "User"."Id" = "TitanClaims"."UserId" where
                             "User"."Email" = @Email and "TitanClaims"."Type" = @Type"""
                let m = (Map ["Email", email; "Type", claim; "Value", value])
                if dapper_map_param_exec cmd m conn = 1 then  
                    return Ok ()
                else 
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
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
            |  e ->
                return Error e.Message
        }

        member this.has_claim email claim : Task<Result<bool, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """select case when exists (select "User"."Id" from "User"
                             join "TitanClaims" on "User"."Id" = "TitanClaims"."UserId"
                             where "TitanClaims"."Type" = @Claim and "User"."Email" = @Email) then cast(1 as bit) else cast(0 as bit) end"""
                let exists = conn
                             |> dapper_map_param_query<bool> cmd (Map ["Email", email; "Claim", claim])
                             |> Seq.head
                return (if exists then Ok true else Ok false)
            with
            |  e ->
                return Error e.Message
        }

        member this.insert_tutor first last schoolname email : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """begin transaction
                                declare @UserId as int;
                                declare @SchoolId as int;
                                insert into "User" ("FirstName","LastName","Email", "Phone") VALUES (@FirstName, @LastName, @Email, @Phone);
                                select @UserId = scope_identity();
                                insert into "School" ("Name","Info","Subjects","Location") VALUES (@SchoolName, @Info, @Subjects, @Location);
                                select @SchoolId = scope_identity();
                                insert into "TitanClaims" ("UserId","Type","Value") VALUES (@UserId, 'IsTutor', 'true');
                                insert into "Tutor" ("UserId","SchoolId") VALUES (@UserId, @SchoolId);
                             commit"""
                let m = (Map ["Email", email; "FirstName", first; "LastName", last; "Phone", ""; "SchoolName", schoolname;
                              "Info", ""; "Subjects", ""; "Location", ""])

                if dapper_map_param_exec cmd m conn <> 0 then  
                   return Ok ()
                else
                   return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
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
            |  e ->
                return Error e.Message
        }

        member this.delete_pending_for_tutor student_email tutor_email : Task<Result<unit, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """delete from "Pending" where "Pending"."UserId" = (select "Id" from "User" where "User"."Email" = @StudentEmail)
                             and "Pending"."SchoolId" = (select "SchoolId" from "Tutor" where "Tutor"."UserId" = (select "Id" from "User" where "User"."Email" = @TutorEmail));"""
                let m = (Map ["StudentEmail", student_email; "TutorEmail", tutor_email])
                if dapper_map_param_exec cmd m conn = 1 then  
                    return Ok ()
                else
                    return Error ("Did not insert the expected number of records. sql is \"" + cmd + "\"")
            with
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
            |  e ->
                return Error e.Message
        }

        member this.query_students tutor_email : Task<Result<Domain.Student list, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let sql = """select "User"."FirstName","User"."LastName","User"."Email","User"."Phone" from "User"
                             join Student on "Student"."UserId" = "User"."Id"
                             join School on "School"."Id" = "Student"."SchoolId"
                             join "Tutor" on "Tutor"."SchoolId" = "School"."Id"
                             join "User" as "TU" on "Tutor"."UserId" = "TU"."Id"
                             where "TU"."Email" = @Email;"""
                let result = conn
                             |> dapper_map_param_query<Domain.Student> sql (Map["Email", tutor_email])
                             |> Seq.toList
                return Ok result
            with
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
            |  e ->
                return Error e.Message
        }

        member this.school_from_email (email : string) : Task<Result<SchoolResponse, string>> = task {
            try
                use conn = new SqlConnection(this.connection)
                conn.Open()
                let cmd = """select "Name","Info","Subjects","Location" from "School" join "Tutor" on "Tutor"."SchoolId" = "School"."Id" where "Tutor"."UserId" = (select "Id" from "User" where "Email" = @Email)"""
                let result = conn
                                |> dapper_map_param_query<Models.SchoolFromEmail> cmd (Map ["Email", email])
                                |> Seq.head
                                |> (fun (x : Models.SchoolFromEmail) -> {SchoolResponse.Info = x.Info; SchoolResponse.Subjects = x.Subjects
                                                                         SchoolResponse.SchoolName = x.SchoolName;
                                                                         SchoolResponse.Location = x.Location; Error = None })
                return Ok result
            with
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
                let sql = """select "School"."Id" from "School"
                             join "Tutor" on "Tutor"."SchoolId" = "School"."Id"
                             join "User" on "User"."Id" = "Tutor"."UserId" where "User"."Email" = @Email"""
                let school_id = conn
                               |> dapper_map_param_query<int> sql (Map ["Email", tutor_email])
                               |> Seq.head
                
                let cmd = """begin transaction
                                insert into "Student"("UserId", "SchoolId") values(@UserId,@SchoolId)
                                delete from "Pending" where "UserId" = @UserID
                              commit"""
                let m = (Map ["UserId", student_id; "SchoolId", school_id])
                if dapper_map_param_exec cmd m conn = 2 then  
                    return Ok ()
                else
                    return Error ("Failed to insert user. sql is \"" + cmd + "\"")
                    
            with
            |  e ->
                return Error e.Message
        }
