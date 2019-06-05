module TitanOpenTok

open OpenTokCore
open Domain
open FSharp.Control.Tasks.ContextInsensitive
open System.Collections.Generic
open System
open System.Threading.Tasks

type ITitanOpenTok =

    abstract member get_token : string -> Task<Result<OpenTokInfo, string>>
    

type TitanOpenTok(key:int, secret:string) =
    let tutor_email_to_session = new Dictionary<string, OpenTokCore.Session>()
    member this.open_tok = OpenTok(key, secret)

    interface ITitanOpenTok with

        member this.get_token tutor_email :  Task<Result<OpenTokInfo, string>> = task {
            match tutor_email_to_session.ContainsKey(tutor_email) with
            | false ->
                let! new_session = this.open_tok.CreateSession("", OpenTokCore.MediaMode.RELAYED, OpenTokCore.ArchiveMode.MANUAL)
                tutor_email_to_session.Add(tutor_email, new_session)
                let in_five_hours = (DateTime.UtcNow.Add(TimeSpan.FromHours(5.0)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds
                let token = this.open_tok.GenerateToken(new_session.Id, OpenTokCore.Role.MODERATOR, in_five_hours, "name=test")
                let oti = {SessionId = new_session.Id; Token = token; Key = new_session.ApiKey.ToString() }
                return Ok oti
            | true ->
                let session = tutor_email_to_session.[tutor_email]
                let in_five_hours = (DateTime.UtcNow.Add(TimeSpan.FromHours(5.0)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds
                let token = this.open_tok.GenerateToken(session.Id, OpenTokCore.Role.MODERATOR, in_five_hours, "name=test")
                return Ok {SessionId = session.Id; Token = token; Key = session.ApiKey.ToString() }
        }

            
