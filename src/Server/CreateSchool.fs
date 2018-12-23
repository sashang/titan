module CreateSchool

open Database
open Domain
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.ContextInsensitive

let create_school (next : HttpFunc) (ctx : HttpContext) = task {
    let db_service = ctx.GetService<IDatabase>()
    let! school = ctx.BindJsonAsync<Domain.CreateSchool>()
    let! result = db_service.insert_school school
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    match result with
    | Ok _ ->
        return! ctx.WriteJsonAsync {CreateSchoolResult.code = []; CreateSchoolResult.message = []}
    | Error message ->
        logger.LogWarning("Failed to create school")
        return! ctx.WriteJsonAsync {CreateSchoolResult.code = [CreateSchoolCode.DatabaseError]; CreateSchoolResult.message = [message]}
}