module Client.Shared

open Domain
open Fable.Core.JsInterop
open Thoth.Json

type LoadingState =
    | Loading
    | Loaded

/// Claim info that's shared between pages on the client side.
[<CLIMutable>]
type TitanClaim = 
    { Surname : string 
      GivenName : string
      Email : string
      IsTitan : bool 
      IsStudent : bool 
      IsTutor : bool}
    static member init = 
      { Surname = ""
        GivenName = ""
        Email = ""
        IsTitan = false
        IsTutor = false
        IsStudent = false }

    static member decoder : Decode.Decoder<TitanClaim> =
        Decode.object
            (fun get -> 
                { Surname = get.Required.Field "family_name" Decode.string
                  GivenName= get.Required.Field "given_name" Decode.string
                  Email = get.Required.Field "email" Decode.string
                  IsTutor =  get.Optional.Field "IsTutor" Decode.string = Some "true"
                  IsStudent = get.Optional.Field "IsStudent" Decode.string = Some "true"
                  IsTitan = get.Optional.Field "IsTitan" Decode.string = Some "true" })
    member this.is_first_time = not (this.IsStudent || this.IsTitan || this.IsTutor)

module OpenTokJSInterop =

    let init_session (key:string) (session_id:string) : obj =
        import "init_session" "./custom.js"

    let init_pub (div_id : string) (res : string) : obj =
        import "init_pub" "./custom.js"

    let connect_session_with_pub (session:obj) (publisher:obj) (token:string) : unit =
        import "connect_session_with_pub" "./custom.js"

    let disconnect (session : obj) : unit =
        import "disconnect" "./custom.js"

    let connect (session : obj) (token : obj) : unit =
        import "connect" "./custom.js"

    let connect_session_with_sub (session:obj) (token:obj) : unit =
        import "connect_session_with_sub" "./custom.js"

    let on_streamcreate_subscribe (session:obj) (width : int) (height : int) : unit =
        import "on_streamcreate_subscribe" "./custom.js"
