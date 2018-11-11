/// Login web part and functions for API web part request authorisation with JWT.
module ServerCode.Auth

open Domain
open FSharp.Control.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open RequestErrors
open System

let createUserData (login : Domain.Login) =
    {
        UserName = login.username
        Token    =
            ServerCode.JsonWebToken.encode (
                { UserName = login.password } : ServerTypes.UserRights
            )
    }

/// Authenticates a user and returns a token in the HTTP body.
let login : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! login = ctx.BindJsonAsync<Domain.Login>()
            return!
                match login.is_valid() with
                | true  ->
                    let data = createUserData login
                    ctx.WriteJsonAsync data
                | false -> RequestErrors.UNAUTHORIZED "Bearer" "" (sprintf "User '%s' can't be logged in." login.username) next ctx
        }

let private missingToken = RequestErrors.BAD_REQUEST "Request doesn't contain a JSON Web Token"
let private invalidToken = RequestErrors.FORBIDDEN "Accessing this API is not allowed"

/// Checks if the HTTP request has a valid JWT token for API.
/// On success it will invoke the given `f` function by passing in the valid token.
let requiresJwtTokenForAPI f : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        (match ctx.TryGetRequestHeader "Authorization" with
        | Some authHeader ->
            let jwt = authHeader.Replace("Bearer ", "")
            match JsonWebToken.isValid jwt with
            | Some token -> f token
            | None -> invalidToken
        | None -> missingToken) next ctx
