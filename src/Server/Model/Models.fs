module Models

 //Dapper uses reflection when reading the names here to match what
 //we used in the sql string. This means the parameter names in the
 //constructor below must match the names use in the sql string, so we use
 //Pascal case in teh constructor...
[<RequireQualifiedAccessAttribute>]
type SchoolTutor(Name:string, FirstName:string, LastName:string, Info:string, Subjects:string, Location:string)=
    member this.SchoolName = Name
    member this.FirstName = FirstName
    member this.LastName = LastName
    member this.Info = Info
    member this.Location = Location
    member this.Subjects = Subjects
    
[<RequireQualifiedAccessAttribute>]
type PendingStudent(FirstName:string, LastName:string, Phone:string, Email:string)=
    member this.Phone = Phone
    member this.Email = Email
    member this.FirstName = FirstName
    member this.LastName = LastName


[<RequireQualifiedAccessAttribute>]
type SchoolFromEmail(Name:string, Info:string, Subjects:string, Location:string)=
    member this.SchoolName = Name
    member this.Info = Info
    member this.Subjects = Subjects
    member this.Location = Location
    
[<RequireQualifiedAccessAttribute>]
[<CLIMutable>]
type TitanClaims =
    { Id : int32
      UserId : int32
      Type : string
      Value : string }

      static member init = {Id = 0; UserId = 0; Type = ""; Value = ""}

[<RequireQualifiedAccessAttribute>]
[<CLIMutable>]
type User =
    { Id : int32
      FirstName : string
      Email : string
      LastName : string }

    static member init = {Id = 0; FirstName = ""; LastName = ""; Email = ""}

//databse model of the school table.
[<CLIMutable>]
[<RequireQualifiedAccessAttribute>]
type School =
    { Id : int32
      UserId : string
      Name : string }

    static member init = {Id = 0; UserId = ""; Name = ""}

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
      Phone : string
      LastName : string }
    static member init = {Id = 0; FirstName = ""; LastName = ""; Email = ""; Phone = ""}
