module TitanOpenTok

open OpenTokCore
open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.Extensions.Logging
open System.Data.SqlClient
open System.Collections.Generic
open System
open System.Dynamic
open System.Threading.Tasks

type ITitanOpenTok =

    abstract member get_token : unit -> Task<Result<OpenTokInfo, string>>
    

type TitanOpenTok(key:int, secret:string) =
    let mutable (session : OpenTokCore.Session option) = None
    member this.open_tok = OpenTok(key, secret)

    interface ITitanOpenTok with

        member this.get_token () :  Task<Result<OpenTokInfo, string>> = task {
            match session with
            | None ->
                let! new_session = this.open_tok.CreateSession("", OpenTokCore.MediaMode.RELAYED, OpenTokCore.ArchiveMode.MANUAL)
                let in_one_hour = (DateTime.UtcNow.Add(TimeSpan.FromHours(5.0)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds
                let token = this.open_tok.GenerateToken(new_session.Id, OpenTokCore.Role.MODERATOR, in_one_hour, "name=test")
                session <- Some new_session
                return Ok {SessionId = new_session.Id; Token = token; Key = new_session.ApiKey.ToString() }
            | Some session ->
                let in_one_hour = (DateTime.UtcNow.Add(TimeSpan.FromHours(5.0)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds
                let token = this.open_tok.GenerateToken(session.Id, OpenTokCore.Role.MODERATOR, in_one_hour, "name=test")
                return Ok {SessionId = session.Id; Token = token; Key = session.ApiKey.ToString() }
        }

            
