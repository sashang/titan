module TutorSessionMap

open Domain
open FSharp.Control.Tasks.ContextInsensitive
open Elmish.Bridge
open ElmishBridgeModel
open System.Collections.Generic
open System.Dynamic
open System.Threading.Tasks

type Name = string
type SessionId = string

//interface session map. interfaces don't have a constructor
type ISessionMap =
    abstract member add_session : Name -> SessionId -> unit
    abstract member remove_session : Name -> unit
    abstract member has_session : Name -> bool


//class that implements interface above. classes have a constructor,
//the parantheses after the class name indicate this.
type TutorSessionMap(add_session) =
    let dict_name_to_session = new Dictionary<Name, SessionId>()
    let eh_add_session = add_session 
    interface ISessionMap with
        member this.add_session name id =
            dict_name_to_session.Add(name, id)
            eh_add_session id //call the event handler when we add a session

        member this.remove_session name =
            dict_name_to_session.Remove(name) |> ignore

        member this.has_session name =
            dict_name_to_session.ContainsKey(name)
