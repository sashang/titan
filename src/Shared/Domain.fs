/// Domain model shared between client and server.
namespace Domain

// Json web token type.
type JWT = string

type APICode = 
    | Success = 0
    | Failure = 1
    | DatabaseError = 2
    | FetchError = 2

// Login credentials.
[<CLIMutable>] //needed for BindJsonAync to work
type Login =
    { username : string
      password : string }

    member this.is_valid() =
        not (this.username <> "test@test"  || this.password <> "test")

type LoginCode = Success = 0 | Failure = 1


[<CLIMutable>]
type TitanClaim =
    { Type : string
      Value : string }

type TitanClaims = 
    { Claims : TitanClaim list}

[<CLIMutable>]
type Session = 
    { username : string
      token : string }

[<CLIMutable>]
type LoginResult =
    { code : LoginCode list
      message : string list
      token : string
      username : string }

type SignUpCode = Success = 0 | BadPassword = 1 | BadUsername = 2 | BadEmail = 3
                  | DatabaseError = 4 | UnknownIdentityError = 5

type TitanRole =
    | Student
    | Principal
    | Admin

[<CLIMutable>] //needed for BindJsonAync to work
///The data transmitted with the sign up request
type SignUp =
    { username : string
      email : string
      password : string
      role : TitanRole option }

type SignOutCode =
    | Success = 0
    | Failure = 1

/// Result of the sign-up action.
[<CLIMutable>]
type SignUpResult =
    { code : SignUpCode list
      message : string list }

[<CLIMutable>]
type SignOutResult =
    { code : SignOutCode list
      message : string list }

///Return codes to the create school request.
type CreateSchoolCode = 
    Success = 0 | SchoolNameInUse = 1 | DatabaseError = 2
    | Unknown = 3 | FetchError = 4

///The data submitted with the create school request
[<CLIMutable>]
type School =
    { Name : string
      Principal : string }

///The result of submitting the create school request when the teacher
///first creates the school
[<CLIMutable>]
type CreateSchoolResult =
    { Codes : CreateSchoolCode list
      Messages : string list }

type LoadSchoolCode =
    Success = 0 | DatabaseError = 1 | FetchError = 2 | NoSchool = 3

[<CLIMutable>]
type LoadSchoolResult =
    { Codes : LoadSchoolCode list
      /// The error messages if there are error codes in Codes
      Messages : string list
      /// Valid if the first code in Codes is Success
      TheSchool : School}

[<CLIMutable>]
type BetaRegistration =
    { Email : string }

type BetaRegistrationCode =
    | Success = 0 | Failure = 1 | BadEmail = 2 | DatabaseError = 3

[<CLIMutable>]
type BetaRegistrationResult =
    { Codes : BetaRegistrationCode list
      Messages : string list }

[<CLIMutable>]
type Student =
    { FirstName : string
      LastName : string
      Email : string }

      static member init =
        {FirstName = ""; LastName = ""; Email = ""}


[<CLIMutable>]
type GetAllStudentsResult =
    { Codes : APICode list
      Messages : string list
      Students : Student list}

[<CLIMutableAttribute>]
type AddStudentSchool = 
    { Codes : APICode list
      Messages : string list }


/// Students who have applied to enrol and are waiting for approval
[<CLIMutableAttribute>]
type PendingResult = 
    { Codes : APICode list
      Messages : string list
      Students : Student list }

[<CLIMutableAttribute>]
type Enrol =
    { FirstName : string 
      LastName : string 
      Email : string
      SchoolName : string }
    static member init = {FirstName = ""; LastName = ""; Email = ""; SchoolName = ""}


[<CLIMutableAttribute>]
type EnrolResult = 
    { Codes : APICode list
      Messages : string list }

