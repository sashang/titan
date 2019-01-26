module Server.TitanOpenTok

open OpenTokSDK
open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.Extensions.Logging
open Npgsql
open System.Data.SqlClient
open System.Collections.Generic
open System.Dynamic
open System.Threading.Tasks

type ITitanOpenTok =

    abstract member create_session: Task<Result<unit, string>>
    

type TitanOpenTok(key:int, secret:string) =
    member this.open_tok = OpenTok(key, secret)
    member this.session_id = ""
    
    interface ITitanOpenTok with
        member this.create_session : Task<Result<unit, string>> = task {
            this.session_id = this.open_tok.CreateSession()
         //   return Ok ()
        }
            
