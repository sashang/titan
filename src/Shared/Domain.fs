/// Domain model shared between client and server.
namespace Domain

// Json web token type.
type JWT = string

// Login credentials.
[<CLIMutable>] //needed for BindJsonAync to work
type Login =
    { username : string
      password : string }
    member this.is_valid() =
        not (this.username <> "test@test"  || this.password <> "test")

type SignUpCode = Success = 0 | BadPassword = 1 | BadUsername = 2 | BadEmail = 3
                  | DatabaseError = 4 | Unknown = 5

type TitanRole = Pupil = 0 | Principal = 1 | Unknown = 2

[<CLIMutable>] //needed for BindJsonAync to work
type SignUp =
    { username : string
      email : string
      password : string
      role : TitanRole }

/// Result of the sign-up action.
[<CLIMutable>]
type SignUpResult =
    { code : SignUpCode list
      message : string list }

///Return codes to the create school request.
type CreateSchoolCode = 
    Success = 0 | SchoolNameInUse = 1 | DatabaseError = 2
    | Unknown = 3

///The data submitted with the create school request
[<CLIMutable>]
type CreateSchool =
    { Name : string
      Principal : string }

///The result of submitting the create school request when the teacher
///first creates the school
[<CLIMutable>]
type CreateSchoolResult =
    { code : CreateSchoolCode list
      message : string list }

type UserData =
    { UserName : string
      Token    : JWT }

type Pupil =
    { uuid : string }

type Class =
    { uuid : string
      date : string }

