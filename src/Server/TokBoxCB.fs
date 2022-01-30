module TokBoxCB

open Domain
open FSharp.Control.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System
open System.Collections.Generic
open Thoth.Json.Net
open TutorSessionMap

type TokBoxConnection =
    { Id : string
      CreatedAt : uint64
      Data : string }

    static member init =
        {Id = ""; CreatedAt = 0UL; Data = "" }

    static member decoder : Decoder<TokBoxConnection> =
        Decode.object
            (fun get ->
                { Id = get.Required.Field "id" Decode.string
                  CreatedAt = get.Required.Field "createdAt" Decode.uint64
                  Data = get.Required.Field "data" Decode.string })

type TokBoxStream =
    { Id : string
      Connection : TokBoxConnection
      CreatedAt : uint64
      Name : string
      VideoType : string }

    static member decoder : Decoder<TokBoxStream> =
        Decode.object
            (fun get ->
                { Id = get.Required.Field "id" Decode.string
                  Connection = get.Required.Field "connection" TokBoxConnection.decoder
                  CreatedAt = get.Required.Field "createdAt" Decode.uint64
                  Name = get.Required.Field "name" Decode.string
                  VideoType = get.Required.Field "videoType" Decode.string })

type TokBoxSession =
    { SessionId : string
      ProjectId : string
      Event : string
      Timestamp : uint64
      Connection : TokBoxConnection option
      Stream : TokBoxStream option }

    static member decoder : Decoder<TokBoxSession> =
        Decode.object
            (fun get ->
                { SessionId = get.Required.Field "sessionId" Decode.string
                  ProjectId = get.Required.Field "projectId" Decode.string
                  Event = get.Required.Field "event" Decode.string
                  Timestamp = get.Required.Field "timestamp" Decode.uint64
                  Connection = get.Optional.Field "connection" TokBoxConnection.decoder
                  Stream = get.Optional.Field "stream" TokBoxStream.decoder })

type Name = string
type SessionId = string

let callback (next : HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLoggerProvider>()
    logger.LogInformation("Called TokBoxCB.callback")
    let! body = ctx.ReadBodyFromRequestAsync()
    //we always have to return success to tokbox when we handle a callback regardless
    //of the result of the handling of the callback. If we don't return success
    //they increment a counter on their end and stop calling back once that counter
    //reaches a limit.
    ctx.SetStatusCode 200
    let session_map = ctx.GetService<ISessionMap>()
    try
        let result = Decode.fromString TokBoxSession.decoder body
        match result with
        | Ok data ->
            logger.LogInformation (sprintf "SessionID = %s" data.SessionId)
            if data.Event = "connectionCreated" then
                return! text "Ok" next ctx
            else if data.Event = "connectionDestroyed" then
                return! text "Ok" next ctx
            else if data.Event = "streamCreated" then
                match data.Stream with
                | Some stream ->
                    session_map.add_session stream.Name data.SessionId
                    return! text "Ok" next ctx
                | None ->
                    return! text "Ok" next ctx
            else if data.Event = "streamDestroyed" then
                match data.Stream with
                | Some stream ->
                    session_map.remove_session(stream.Name) |> ignore
                    return! text "Ok" next ctx
                | None ->
                    return! text "Ok" next ctx
            else
                return! text "Ok" next ctx

        | Error message ->
            logger.LogWarning ("Failed to decode tokbox callback json")
            return! text message next ctx
    with
    | ex ->
        logger.LogWarning ex.Message
        return! text ex.Message next ctx
}

let find_by_name (next : HttpFunc) (ctx : HttpContext) = task {
    let logger = ctx.GetLogger<Debug.DebugLoggerProvider>()
    logger.LogInformation("Called TokBoxCB.find_by_name")
    let! email = ctx.BindJsonAsync<Domain.EmailRequest>()
    logger.LogInformation(email.Email)
    return! text "Dummy value" next ctx
}
