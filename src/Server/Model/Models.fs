module Models

//databse model of the school table.
[<CLIMutable>]
type School =
    { Id : int32
      UserId : string
      Name : string 
      Principal : string }

let default_school = {Id = 0; UserId = ""; Name = ""; Principal = ""}

[<CLIMutable>]
type Punter =
    { Id : int32
      Email : string }
let default_punter = {Id = 0; Email = ""}

[<CLIMutable>]
type Student =
    { Id : int32
      FirstName : string
      Email : string
      LastName : string }
    static member init = {Id = 0; FirstName = ""; LastName = ""; Email = ""}
