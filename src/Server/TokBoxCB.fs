module TokBoxCB

open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System
open System.Collections.Generic
open Thoth.Json.Net

type TokBoxConnection =
    { Id : string
      CreatedAt : uint64
      Data : string }

    static member decoder : Decode.Decoder<TokBoxConnection> =
        Decode.object
            (fun get ->
                { Id = get.Required.Field "id" Decode.string
                  CreatedAt = get.Required.Field "createdAt" Decode.uint64
                  Data = get.Required.Field "data" Decode.string })

type TokBoxSession =
    { SessionId : string
      ProjectId : string
      Event : string
      Timestamp : uint64 
      Connection : TokBoxConnection }

    static member decoder : Decode.Decoder<TokBoxSession> =
        Decode.object
            (fun get ->
                { SessionId = get.Required.Field "sessionId" Decode.string
                  ProjectId = get.Required.Field "projectId" Decode.string
                  Event = get.Required.Field "event" Decode.string
                  Timestamp = get.Required.Field "timestamp" Decode.uint64
                  Connection = get.Required.Field "connection" TokBoxConnection.decoder })

let sessions = new HashSet<string>()

let callback (next : HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLogger>()
    logger.LogInformation("called TokBoxCB.callback")
    let! body = ctx.ReadBodyFromRequestAsync()
    logger.LogInformation body
    //we always have to return success to tokbox when we handle a callback regardless
    //of the result of the handling of the callback. If we don't return success
    //they increment a counter on their end and stop calling back once that counter 
    //reaches a limit.
    ctx.SetStatusCode 200
    let result = Decode.fromString TokBoxSession.decoder body
    match result with
    | Ok data ->
        logger.LogInformation (sprintf "SessionID = %s" data.SessionId)
        if data.Event = "connectionCreated" then
            sessions.Add(data.SessionId) |> ignore
            return! text "Ok" next ctx
        else if data.Event = "connectionDestroyed" then
            sessions.Remove(data.SessionId) |> ignore
            return! text "Ok" next ctx
        else
            return! text "Ok" next ctx

    | Error _ -> 
        logger.LogWarning ("Failed to decode tokbox callback json")
        return! text "Ok" next ctx
}