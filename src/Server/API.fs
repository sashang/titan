module API


open Database
open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open System.Security.Claims

let private role_to_string = function
| Some TitanRole.Admin -> "admin"
| Some TitanRole.Student -> "student"
| Some TitanRole.Principal -> "principal"
| None -> "unknown"

let sign_up_user (next : HttpFunc) (ctx : HttpContext) = task {
    //translate the string code generated by Identity into an enum
    let of_code (id_error_code : string) = 
        if id_error_code.Contains "Password" then
            SignUpCode.BadPassword
        else if id_error_code.Contains "User" then
            SignUpCode.BadUsername
        else if id_error_code.Contains "Email" then
            SignUpCode.BadEmail
        else
            //we don't have a mapping for this Identity error
            SignUpCode.UnknownIdentityError

    let of_id_errors (errors : seq<IdentityError>) =
        Seq.fold (fun acc (err : IdentityError) ->
                    {SignUpResult.code = List.append acc.code [of_code err.Code]; SignUpResult.message = List.append acc.message [err.Description] })
                    {SignUpResult.code = []; SignUpResult.message = [] } errors

    let! login = ctx.BindJsonAsync<Domain.SignUp>()
    let user = IdentityUser(UserName = login.username, Email = login.email)
    let user_manager = ctx.GetService<UserManager<IdentityUser>>()
    let sign_in_manager = ctx.GetService<SignInManager<IdentityUser>>()
    let! id_result = user_manager.CreateAsync(user, login.password)
    match id_result.Succeeded with
    | false ->
        printfn "Failed to create user"
        return! ctx.WriteJsonAsync (of_id_errors id_result.Errors)
    | true -> 
        let claim = Claim("TitanRole", role_to_string login.role)
        let! add_claim_result = user_manager.AddClaimAsync(user, claim)
        if add_claim_result.Succeeded then
            do! sign_in_manager.SignInAsync(user, false)
            return! ctx.WriteJsonAsync {SignUpResult.code = []; SignUpResult.message = []}
        else
            return! ctx.WriteJsonAsync {SignUpResult.code = [SignUpCode.DatabaseError]; SignUpResult.message = ["failed to add claim"]}

}
let create_school (next : HttpFunc) (ctx : HttpContext) = task {
    let db_service = ctx.GetService<IDatabase>()
    let! school = ctx.BindJsonAsync<Domain.School>()
    let! result = db_service.insert_school school
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    match result with
    | Ok _ ->
        return! ctx.WriteJsonAsync {CreateSchoolResult.Codes = [CreateSchoolCode.Success]; CreateSchoolResult.Messages = [""]}
    | Error message ->
        logger.LogWarning("Failed to create school")
        return! ctx.WriteJsonAsync {CreateSchoolResult.Codes = [CreateSchoolCode.DatabaseError]; CreateSchoolResult.Messages = [message]}
}