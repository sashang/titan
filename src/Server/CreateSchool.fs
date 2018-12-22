module CreateSchool

open Database
open Domain
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.AspNetCore.Identity

let create_school (next : HttpFunc) (ctx : HttpContext) = task {
    let db_service = ctx.GetService<IDatabase>()
    let! school = ctx.BindJsonAsync<Domain.CreateSchool>()
    let! result = db_service.insert_school school
    if result then
        return! ctx.WriteJsonAsync {CreateSchoolResult.code = []; CreateSchoolResult.message = []}
    else
        return! ctx.WriteJsonAsync {CreateSchoolResult.code = [CreateSchoolCode.DatabaseError]; CreateSchoolResult.message = ["Error writing user to database"]}
}