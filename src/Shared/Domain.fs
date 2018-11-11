/// Domain model shared between client and server.
namespace Domain

open System

// Json web token type.
type JWT = string

// Login credentials.
type Login =
    { username   : string
      password   : string }
    member this.is_valid() =
        not (this.username <> "test@test"  || this.password <> "test")

type UserData =
  { UserName : string
    Token    : JWT }

type Pupil = {
    uuid : string
}

type Class = {
    uuid : string
    date : string
}

type School = {
    uuid : string
    name : string
    classes : Class list
    pupils : Pupil list
}

type Schools = {
    schools : School list
}
