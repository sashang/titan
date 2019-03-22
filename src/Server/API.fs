module API

open AzureMaps
open Database
open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System.Security.Claims
open System.Net.Mail
open System
open TitanOpenTok


let get_azure_maps_keys (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("getting azure maps keys")
        let az_keys = ctx.GetService<IAzureMaps>()
        let result = az_keys.get_azure_maps_keys
        return! ctx.WriteJsonAsync result
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let get_enrolled_schools (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("getting enrolled schools")
        let student_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let db = ctx.GetService<IDatabase>()
        let! result = db.get_enrolled_schools student_email
        match result with
        | Ok schools ->
            return! ctx.WriteJsonAsync {GetAllSchoolsResult.init with Schools = schools ; Error = None}
        | Error msg ->
            logger.LogError("Failed to get session info from opentok")
            return! ctx.WriteJsonAsync ({Info = None; Error = Some (APIError.titan_open_tok msg)})
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

///student request to get session id. Note that the json must container the
///email of the tutor since that is what links the session id.
let get_session_id_for_student (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let! request = ctx.BindJsonAsync<EmailRequest>()
        let student_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        logger.LogInformation(sprintf "get_session_id_for_student: student with email %s joining tutor with email %s " student_email request.Email)
        let titan_open_tok = ctx.GetService<ITitanOpenTok>()
        let! result = titan_open_tok.get_token request.Email
        match result with
        | Ok tok_info ->
            return! ctx.WriteJsonAsync {Info = Some tok_info; Error = None}
        | Error msg ->
            logger.LogError("Failed to get session info from opentok")
            return! ctx.WriteJsonAsync ({Info = None; Error = Some (APIError.titan_open_tok msg)})
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}
/// tutor request to go live
let get_session_id (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let email = ctx.User.FindFirst(ClaimTypes.Email).Value
        logger.LogInformation(sprintf "get_session_id: tutor with email %s" email)
        let titan_open_tok = ctx.GetService<ITitanOpenTok>()
        let! result = titan_open_tok.get_token email
        match result with
        | Ok tok_info ->
            return! ctx.WriteJsonAsync {Info = Some tok_info; Error = None}
        | Error msg ->
            logger.LogError("Failed to get session info from opentok")
            return! ctx.WriteJsonAsync ({Info = None; Error = Some (APIError.titan_open_tok msg)})
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

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
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let db = ctx.GetService<IDatabase>()
        let! info = ctx.BindJsonAsync<DismissPendingRequest>()
        //get the user id of the tutor
        let tutor_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let! result = db.delete_pending_for_tutor info.Email tutor_email
        match result with
        | Error e ->
            logger.LogInformation("failed to delete pending student: " + e)
            let api_error = {DismissPendingResult.Error = Some (APIError.init [APICode.Database] [e])}
            return! ctx.WriteJsonAsync api_error
        | Ok _ ->
            return! ctx.WriteJsonAsync {DismissPendingResult.Error = None}
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
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
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("called load_school")
        let db_service = ctx.GetService<IDatabase>()
        let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let! result = db_service.school_from_email user_email
        match result with
        | Ok school ->
            return! ctx.WriteJsonAsync school
                                       
        | Error message ->
            logger.LogWarning("Failed to load school: " + message)
            return! ctx.WriteJsonAsync {SchoolResponse.init with Error = (Some (APIError.db message))}
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
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
        let! data = ctx.BindJsonAsync<SaveRequest>()
        let db_service = ctx.GetService<IDatabase>()
        let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let! result = db_service.handle_save_request user_email data
        match result with
        | Ok () ->
            return! ctx.WriteJsonAsync None
        | Error message ->
            return! ctx.WriteJsonAsync (APIError.db message)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let get_pending_schools (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("called get_pending_schools")
        let db_service = ctx.GetService<IDatabase>()
        let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let! result = db_service.get_pending_schools user_email
        match result with
        | Ok schools ->
            return! ctx.WriteJsonAsync schools
        | Error message ->
            return! ctx.WriteJsonAsync (SchoolsResponse.db_error message)
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
        | Ok schools ->
            return! ctx.WriteJsonAsync {GetAllSchoolsResult.init with Schools = schools }
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

///update the user and its claims
let update_user_approval (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("called update_user_claims")
        let titan_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let db_service = ctx.GetService<IDatabase>()
        logger.LogInformation("titan is " + titan_email)
        let! is_titan = db_service.has_claim titan_email "IsTitan"
        let! data = ctx.BindJsonAsync<UserForTitan>()
        match is_titan with
        | Ok _ ->
            let! result = db_service.has_claim data.Email "IsApproved"
            match result with
            | Ok true ->
                //update the claim
                logger.LogInformation("Updating existing claim for " + data.Email)
                let! result = db_service.update_user_claim data.Email "IsApproved" (if data.IsApproved then "true" else "false")
                match result with
                | Ok response ->
                    return! ctx.WriteJsonAsync None
                | Error message ->
                    return! ctx.WriteJsonAsync (APIError.db message)
            | Ok false ->
                //insert the claim
                logger.LogInformation("Inserting new claim for " + data.Email)
                let! result = db_service.insert_user_claim data.Email "IsApproved" (if data.IsApproved then "true" else "false")
                match result with
                | Ok response ->
                    return! ctx.WriteJsonAsync None
                | Error message ->
                    return! ctx.WriteJsonAsync (APIError.db message)
            | Error msg ->
                logger.LogInformation("has_claim failed for " + data.Email)
                return! ctx.WriteJsonAsync (APIError.db msg)
        | Error msg ->
            logger.LogError("Failed to update claim for " + data.Email)
            return! ctx.WriteJsonAsync (APIError.db msg)
    else
        return! ctx.WriteJsonAsync APIError.unauthorized
}

let get_users_for_titan (next : HttpFunc) (ctx : HttpContext) = task {
    if ctx.User.Identity.IsAuthenticated then
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("called get_users_for_titan")
        let user_email = ctx.User.FindFirst(ClaimTypes.Email).Value
        let db_service = ctx.GetService<IDatabase>()
        let! is_titan = db_service.has_claim user_email "IsTitan"
        match is_titan with
        | Ok _ ->
            let! result = db_service.get_users_for_titan ()
            match result with
            | Ok response ->
                return! ctx.WriteJsonAsync response
            | Error message ->
                return! ctx.WriteJsonAsync (UsersForTitanResponse.db_error message)
        | Error _ ->
            return! ctx.WriteJsonAsync APIError.unauthorized
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
