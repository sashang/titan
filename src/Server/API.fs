module API

open Database
open Domain
open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System.Security.Claims
open System.Net.Mail
open System


let enrol (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let! enrol_request = ctx.BindJsonAsync<Domain.EnrolRequest>()
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("processing enrol request")
        let db = ctx.GetService<IDatabase>()
        let student_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let! result = db.insert_enrol_request student_email enrol_request.SchoolName 
        match result with
        | Ok () ->
            return! ctx.WriteJsonAsync None
        | Error msg ->
            logger.LogWarning msg
            //clean up this error
            return! if msg.Contains("duplicate") then 
                       ctx.WriteJsonAsync (APIError.init [APICode.Email] ["Duplicate email address"] )
                    else 
                        ctx.WriteJsonAsync (APIError.db msg)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
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
        let api_error = {DismissPendingResult.Error = Some (APIError.init [APICode.Database] [e])}
        return! ctx.WriteJsonAsync api_error
    | Ok _ ->
        return! ctx.WriteJsonAsync {DismissPendingResult.Error = None}
}

let approve_enrolment_request (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let db = ctx.GetService<IDatabase>()
        let tutor_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let! student = ctx.BindJsonAsync<Domain.ApprovePendingRequest>()
        let! result = db.approve_enrol_request tutor_email student.Email
        match result with
        | Ok () ->
            return! ctx.WriteJsonAsync ()
            
        | Error message ->
            logger.LogWarning("Could not enrol student")
            return! ctx.WriteJsonAsync (APIError.db message)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let get_pending (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let db = ctx.GetService<IDatabase>()
        let tutor_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let! result = db.query_pending tutor_email
        match result with
        | Ok students ->
            return! ctx.WriteJsonAsync 
                {PendingResult.Error = None;
                 PendingResult.Students = students
                                          |> List.map (fun x -> {FirstName = x.FirstName; LastName = x.LastName;
                                                                 Phone = x.Phone; Email = x.Email})}
        | Error err ->
            logger.LogInformation("Could not get students")
            return! ctx.WriteJsonAsync 
                {PendingResult.Error = None
                 Students = []}
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let register_tutor (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let! registration = ctx.BindJsonAsync<Domain.TutorRegister>()
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let db = ctx.GetService<IDatabase>()
        let! result = db.insert_tutor registration.FirstName registration.LastName registration.SchoolName registration.Email 
        match result with
        | Ok () ->
            return! ctx.WriteJsonAsync None
        | Error msg ->
            logger.LogWarning msg
            //clean up this error
            return! if msg.Contains("duplicate") then 
                       ctx.WriteJsonAsync (APIError.init [APICode.Email] ["Duplicate email address"] )
                    else 
                        ctx.WriteJsonAsync (APIError.db msg)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let register_student (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let! registration = ctx.BindJsonAsync<Domain.StudentRegister>()
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let db = ctx.GetService<IDatabase>()
        let! result = db.insert_student registration.FirstName registration.LastName registration.Email 
        match result with
        | Ok () ->
            return! ctx.WriteJsonAsync None
        | Error msg ->
            logger.LogWarning msg
            //clean up this error
            return! if msg.Contains("duplicate") then 
                       ctx.WriteJsonAsync (APIError.init [APICode.Email] ["Duplicate email address"] )
                    else 
                        ctx.WriteJsonAsync (APIError.db msg)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
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
        return! ctx.WriteJsonAsync {AddStudentSchool.Error = None}
    | Error e ->
        logger.LogWarning e
        return! ctx.WriteJsonAsync {AddStudentSchool.Error = Some (APIError.init [APICode.Database] [e])}

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

let get_name  (next :HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    logger.LogInformation("called get_name")
    let db_service = ctx.GetService<IDatabase>()
    let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
    let! result = db_service.user_from_email user_email
    match result with
    | Ok user ->
        let the_school = {UserResponse.FirstName = user.FirstName;
                          UserResponse.LastName = user.LastName;
                          UserResponse.Error = None}
        return! ctx.WriteJsonAsync the_school
                                   
    | Error message ->
        logger.LogWarning("Failed to load school: " + message)
        return! ctx.WriteJsonAsync {UserResponse.FirstName = ""
                                    UserResponse.LastName = ""
                                    UserResponse.Error = Some(APIError.init [APICode.Database] [message])}
}

/// Load the user's school.
let load_school (next :HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    logger.LogInformation("called load_school")
    let db_service = ctx.GetService<IDatabase>()
    let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
    let! result = db_service.school_from_email user_email
    match result with
    | Ok db_school ->
        let the_school = {Domain.SchoolResponse.SchoolName = db_school.Name; Error = None}
        return! ctx.WriteJsonAsync the_school
                                   
    | Error message ->
        logger.LogWarning("Failed to load school: " + message)
        return! ctx.WriteJsonAsync {SchoolResponse.SchoolName = "";
                                    Error = Some (APIError.init [APICode.Database] [message])}

}

let load_user (next :HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    logger.LogInformation("called load_user")
    let db_service = ctx.GetService<IDatabase>()
    let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
    let! result = db_service.user_from_email user_email
    match result with
    | Ok user_details ->
        let the_school = {UserResponse.FirstName = user_details.FirstName;
                          UserResponse.LastName = user_details.LastName;
                          Error = None}
        return! ctx.WriteJsonAsync the_school
                                   
    | Error message ->
        logger.LogWarning("Failed to load school: " + message)
        return! ctx.WriteJsonAsync UserResponse.init
}

let save_tutor (next :HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("called save_tutor")
        let! data = ctx.BindJsonAsync<Domain.SaveRequest>()
        let db_service = ctx.GetService<IDatabase>()
        let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let! result = db_service.update_user data.FirstName data.LastName user_email
        match result with
        | Ok () ->
            let! result = db_service.update_school_name data.SchoolName user_email
            match result with
            | Ok () -> return! ctx.WriteJsonAsync None
            | Error message -> return! ctx.WriteJsonAsync (APIError.init [APICode.Database] [message])
        | Error message ->
            return! ctx.WriteJsonAsync (APIError.db message)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let get_all_schools (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("called get_all_schools")
        let db_service = ctx.GetService<IDatabase>()
        let! result = db_service.get_school_view
        match result with
        | Ok schooltutor ->
            //bit of a mess...should rethink how we do this so we don't need to convert rfom one type to the other.
            let mapped_schools = schooltutor |> List.map (fun st -> {School.LastName = st.LastName; School.FirstName = st.FirstName;
                                                School.SchoolName = st.SchoolName})
            return! ctx.WriteJsonAsync {GetAllSchoolsResult.init with Schools = mapped_schools }
        | Error message ->
            return! ctx.WriteJsonAsync (GetAllSchoolsResult.db_error message)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let get_all_students (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("called get_all_students")
        let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let db_service = ctx.GetService<IDatabase>()
        let! result = db_service.query_students user_email
        match result with
        | Ok students ->
            //bit of a mess...should rethink how we do this so we don't need to convert rfom one type to the other.
            return! ctx.WriteJsonAsync {GetAllStudentsResult.init with Students = students }
        | Error message ->
            return! ctx.WriteJsonAsync (GetAllStudentsResult.db_error message)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let dismiss_student (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("called dismiss_student")
        let! data = ctx.BindJsonAsync<DismissStudentRequest>()
        let tutor_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let db_service = ctx.GetService<IDatabase>()
        let! result = db_service.delete_student_from_school tutor_email data.Email
        match result with
        | Ok students ->
            return! ctx.WriteJsonAsync ()
        | Error message ->
            return! ctx.WriteJsonAsync (APIError.db message)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}
