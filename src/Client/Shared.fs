module Client.Shared

open Domain
open Thoth.Json

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
