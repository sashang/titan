/// Domain model shared between client and server.
namespace Domain

open System

// Json web token type.
type JWT = string

// Login credentials.
[<CLIMutable>] //needed for BindJsonAync to work
type Login =
    { username   : string
      password   : string }
    member this.is_valid() =
        not (this.username <> "test@test"  || this.password <> "test")

type SignUpCode = Success = 0 | BadPassword = 1 | BadUsername = 2 | BadEmail = 3 | Unknown = 4
/// Result of the sign-up action.
[<CLIMutable>]
type SignUpResult =
    { code : SignUpCode list
      message : string list }

type UserData =
    { UserName : string
      Token    : JWT }

type Pupil =
    { uuid : string }

type Class =
    { uuid : string
      date : string }

type School =
    { uuid : string
      name : string
      classes : Class list
      pupils : Pupil list }

type Schools =
    { schools : School list }
