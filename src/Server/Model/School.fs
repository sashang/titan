module Models

//databse model of the school table.
[<CLIMutable>]
type School =
    { Id : int32
      UserId : string
      Name : string 
      Principal : string }

let default_school = {Id = 0; UserId = ""; Name = ""; Principal = ""}
