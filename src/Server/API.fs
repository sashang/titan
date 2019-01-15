module API

open Database
open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open System.Security.Claims
open System.Net.Mail
open System

let private role_to_string = function
| Some TitanRole.Admin -> "admin"
| Some TitanRole.Student -> "student"
| Some TitanRole.Principal -> "principal"
| None -> "unknown"

let enrol (next : HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    try
        let! info = ctx.BindJsonAsync<Domain.Enrol>()
        let _ = MailAddress(info.Email)//test that the email is valid.
        let db = ctx.GetService<IDatabase>()
        let m = { Models.Student.init with Email = info.Email; LastName = info.LastName; FirstName = info.FirstName }
        logger.LogInformation("enrolling " + m.FirstName + " in school " + info.SchoolName)
        let! result = db.insert_pending m info.SchoolName
        match result with
        | Ok () ->
            return! ctx.WriteJsonAsync {EnrolResult.Error = None}
        | Error e -> 
            logger.LogInformation("Enrolment failed: " + e)
            let api_error = {EnrolResult.Error = Some (APIError.init [APICode.DatabaseError] [e])}
            return! ctx.WriteJsonAsync api_error
    with
        | :? FormatException as e ->
            logger.LogInformation("enrolee has bad email")
            let api_error = {EnrolResult.Error = Some (APIError.init [APICode.Failure] [e.Message])}
            return! ctx.WriteJsonAsync api_error
        | :? ArgumentException as e ->
            logger.LogInformation("enrolee has bad email")
            let api_error = {EnrolResult.Error = Some (APIError.init [APICode.Failure] [e.Message])}
            return! ctx.WriteJsonAsync api_error
        | e ->
            logger.LogInformation("failed to enrol")
            let api_error = {EnrolResult.Error = Some (APIError.init [APICode.Failure] [e.Message])}
            return! ctx.WriteJsonAsync api_error
}

let get_schools (next : HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    logger.LogInformation("get schools")
    let db = ctx.GetService<IDatabase>()
    let! result = db.query_all_schools
    match result with
    | Ok schools ->
        return! ctx.WriteJsonAsync (schools |> List.map (fun x -> {School.Name = x.Name; School.Principal = x.Principal}))
    | Error err ->
        logger.LogInformation("Could not get students: " + err)
        return! ctx.WriteJsonAsync []
}

let dismiss_pending (next :HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    let db = ctx.GetService<IDatabase>()
    let! info = ctx.BindJsonAsync<DismissPendingRequest>()
    //get the user id of the tutor
    let user_id = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let! result = db.delete_pending_for_tutor info.Email user_id
    match result with
    | Error e ->
        logger.LogInformation("failed to delete pending student: " + e)
        let api_error = {DismissPendingResult.Error = Some (APIError.init [APICode.DatabaseError] [e])}
        return! ctx.WriteJsonAsync api_error
    | Ok _ ->
        return! ctx.WriteJsonAsync {DismissPendingResult.Error = None}
}

let approve_pending (next :HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    let db = ctx.GetService<IDatabase>()
    let! info = ctx.BindJsonAsync<Domain.ApprovePendingRequest>()
    //get the user id of the tutor
    let user_id = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let! result = db.insert_student {Models.Student.init with Email = info.Email; FirstName = info.FirstName; LastName = info.LastName}
    match result with
    | Error e ->
        logger.LogInformation("failed to insert pending student: " + e)
        let api_error = {ApprovePendingResult.Error = Some (APIError.init [APICode.DatabaseError] [e])}
        return! ctx.WriteJsonAsync api_error
    | Ok _ ->
        let! result = db.insert_student_school info.Email user_id
        match result with
        | Error e ->
            logger.LogInformation("failed to insert new student: " + e)
            let api_error = {ApprovePendingResult.Error = Some (APIError.init [APICode.DatabaseError] [e])}
            return! ctx.WriteJsonAsync api_error
        | Ok _ ->
            let! result = db.delete_pending info.Email
            match result with
            | Error e ->
                logger.LogInformation("failed delete pending student: " + e)
                let api_error = {ApprovePendingResult.Error = Some (APIError.init [APICode.DatabaseError] [e])}
                return! ctx.WriteJsonAsync api_error
            | Ok _ ->
                return! ctx.WriteJsonAsync {ApprovePendingResult.Error = None}
}

let get_pending (next : HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    let db = ctx.GetService<IDatabase>()
    let! result = db.query_pending
    match result with
    | Ok students ->
        return! ctx.WriteJsonAsync 
            {GetAllStudentsResult.Codes = [APICode.Success]
             GetAllStudentsResult.Messages = []
             Students = students |> List.map (fun x -> {FirstName = x.FirstName; LastName = x.LastName; Email = x.Email})}
    | Error err ->
        logger.LogInformation("Could not get students")
        return! ctx.WriteJsonAsync 
            {GetAllStudentsResult.Codes = [APICode.DatabaseError]
             GetAllStudentsResult.Messages = ["Failed to get students from database"]
             Students = []}
}

let add_student_to_school (next : HttpFunc) (ctx : HttpContext) = task {
    let! student = ctx.BindJsonAsync<Domain.Student>()
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    let db = ctx.GetService<IDatabase>()
    //get the user id of the tutor
    let user_id = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let! result = db.insert_student_school student.Email user_id
    match result with
    | Ok () ->
        return! ctx.WriteJsonAsync {AddStudentSchool.Codes = [APICode.Success];
            AddStudentSchool.Messages = [""]}
    | Error e ->
        logger.LogWarning e
        return! ctx.WriteJsonAsync {AddStudentSchool.Codes = [APICode.DatabaseError]; AddStudentSchool.Messages = [e]}

}
 
let get_all_students (next : HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    let db = ctx.GetService<IDatabase>()
    let! result = db.query_all_students
    match result with
    | Ok students ->
        return! ctx.WriteJsonAsync 
            {GetAllStudentsResult.Codes = [APICode.Success]
             GetAllStudentsResult.Messages = []
             Students = students |> List.map (fun x -> {FirstName = x.FirstName; LastName = x.LastName; Email = x.Email})}
    | Error err ->
        logger.LogInformation("Could not get students")
        return! ctx.WriteJsonAsync 
            {GetAllStudentsResult.Codes = [APICode.DatabaseError]
             GetAllStudentsResult.Messages = ["Failed to get students from database"]
             Students = []}
}

let register_punter (next : HttpFunc) (ctx : HttpContext) = task {
    let! punter = ctx.BindJsonAsync<Domain.BetaRegistration>()
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    let db = ctx.GetService<IDatabase>()
    try
        let address = MailAddress(punter.Email)//test that the email is valid.
        let! result = db.register_punters {Models.default_punter with Models.Punter.Email = punter.Email}
        match result with
        | Ok true  -> 
            return! ctx.WriteJsonAsync {BetaRegistrationResult.Codes = [BetaRegistrationCode.Success]; BetaRegistrationResult.Messages = []}
        | Ok false  -> 
            return! ctx.WriteJsonAsync
                {BetaRegistrationResult.Codes = [BetaRegistrationCode.DatabaseError]; BetaRegistrationResult.Messages = ["database error"]}
        | Error e -> 
            return! ctx.WriteJsonAsync 
                {BetaRegistrationResult.Codes = [BetaRegistrationCode.DatabaseError]; BetaRegistrationResult.Messages = [e]}
    with
    | :? FormatException as e ->
        logger.LogInformation("punter had bad email")
        return! ctx.WriteJsonAsync 
            {BetaRegistrationResult.Codes = [BetaRegistrationCode.BadEmail]
             BetaRegistrationResult.Messages = [e.Message]}
    | :? ArgumentException as e ->
        logger.LogInformation("punter had bad email")
        return! ctx.WriteJsonAsync
            {BetaRegistrationResult.Codes = [BetaRegistrationCode.BadEmail]
             BetaRegistrationResult.Messages = [e.Message]}
    | e ->
        logger.LogInformation("failed to register punter")
        return! ctx.WriteJsonAsync
            {BetaRegistrationResult.Codes = [BetaRegistrationCode.Failure]
             BetaRegistrationResult.Messages = [e.Message]}
}

let create_school (next : HttpFunc) (ctx : HttpContext) = task {
    let db_service = ctx.GetService<IDatabase>()
    let! school = ctx.BindJsonAsync<Domain.School>()
    //get the user id the asp.net way....
    let user_id = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let! exists = db_service.user_has_school user_id
    let school_info = {Models.default_school with Models.School.Principal = school.Principal
                                                  Models.School.Name = school.Name; Models.School.UserId = user_id}

    let! result =
        match exists with
        | Ok false ->
            db_service.insert_school school_info    
        | Ok true ->
            db_service.update_school_by_user_id school_info
        | Error error -> task { return Error error }


    let logger = ctx.GetLogger<Debug.DebugLogger>()
    match result with
    | Ok _ ->
        return! ctx.WriteJsonAsync {CreateSchoolResult.Codes = [CreateSchoolCode.Success]; CreateSchoolResult.Messages = [""]}
    | Error message ->
        logger.LogWarning("Failed to create school: " + message)
        return! ctx.WriteJsonAsync {CreateSchoolResult.Codes = [CreateSchoolCode.DatabaseError]; CreateSchoolResult.Messages = [message]}
}

/// Load the user's school.
let load_school (next :HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    let db_service = ctx.GetService<IDatabase>()
    let user_id = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let! result = db_service.user_has_school user_id
    match result with
        //has a school so look it up and return it.
        | Ok true ->
            let! result = db_service.school_from_user_id user_id
            match result with
            | Ok db_school ->
                let the_school = {Domain.School.Name = db_school.Name; Domain.School.Principal = db_school.Principal}
                return! ctx.WriteJsonAsync {LoadSchoolResult.Codes = [LoadSchoolCode.Success]
                                            LoadSchoolResult.Messages = [""]; TheSchool = the_school}
            | Error message ->
                logger.LogWarning("Failed to load school: " + message)
                return! ctx.WriteJsonAsync {LoadSchoolResult.Codes = [LoadSchoolCode.DatabaseError]
                                            LoadSchoolResult.Messages = [message]
                                            TheSchool = {Domain.School.Principal = ""; Domain.School.Name = ""}}
        //no school
        | Ok false -> 
            return! ctx.WriteJsonAsync {LoadSchoolResult.Codes = [LoadSchoolCode.NoSchool]
                                        LoadSchoolResult.Messages = ["No school associated with user."]
                                        TheSchool = {Domain.School.Principal = ""; Domain.School.Name = ""}}
        //something bad happened
        | Error error ->
            logger.LogWarning("Failed to check if user is associated with a school: " + error)
            return! ctx.WriteJsonAsync {LoadSchoolResult.Codes = [LoadSchoolCode.DatabaseError]
                                        LoadSchoolResult.Messages = [error]
                                        TheSchool = {Domain.School.Principal = ""; Domain.School.Name = ""}}

}

