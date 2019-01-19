module Models

[<CLIMutable>]
type TitanClaims =
    { Id : int32
      UserId : int32
      Type : string
      Value : string }

      static member init = {Id = 0; UserId = 0; Type = ""; Value = ""}

//databse model of the school table.
[<CLIMutable>]
type School =
    { Id : int32
      UserId : string
      Name : string }

let init = {Id = 0; UserId = ""; Name = ""}

[<CLIMutable>]
type Punter =
    { Id : int32
      Email : string }
let default_punter = {Id = 0; Email = ""}

[<RequireQualifiedAccessAttribute>]
[<CLIMutable>]
type Student =
    { Id : int32
      FirstName : string
      Email : string
      LastName : string }
    static member init = {Id = 0; FirstName = ""; LastName = ""; Email = ""}
