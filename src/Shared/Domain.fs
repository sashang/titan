/// Domain model shared between client and server.
namespace Domain

[<RequireQualifiedAccess>]
type APICode =
    | Failure
    | Database
    | Fetch
    | Unauthorized
    | SchoolName
    | Email
    | FirstName
    | LastName
    | NoSchool

[<CLIMutableAttribute>]
type APIError =
    { Codes : APICode list
      Messages : string list }
    static member init_empty = {Codes = []; Messages = []}

    static member init codes messages =
        {Codes = codes; Messages = messages}
    static member unauthorized =
        {Codes = [APICode.Unauthorized]; Messages = ["You are not authorized"]}
    static member db msg =
        {Codes = [APICode.Database]; Messages = [msg]}

// Login credentials.
[<CLIMutable>] //needed for BindJsonAync to work
type Login =
    { username : string
      password : string }

    member this.is_valid() =
        not (this.username <> "test@test"  || this.password <> "test")

type LoginCode = Success = 0 | Failure = 1


type CallBackCode =
    | CBLogin

[<CLIMutable>]
type CallBack =
    { Code : CallBackCode }

[<CLIMutable>]
type Session =
    { Username : string
      Token : string }
      static member init =
        {Username = ""; Token = ""}


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

[<CLIMutable>]
type SchoolRequest =
    { Email : string }

[<CLIMutable>]
type SchoolResponse =
    { SchoolName : string
      Error : APIError option }

[<CLIMutable>]
type LoadSchoolResult =
    { SchoolName : string
      FirstName : string
      LastName : string
      Error : APIError }

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
type UserResponse =
    { FirstName : string
      LastName : string
      Error : APIError option }
    static member init =
        {FirstName = ""; LastName = ""; Error = None}

[<CLIMutable>]
type GetAllStudentsResult =
    { Error : APIError option
      Students : Student list}

[<CLIMutableAttribute>]
type AddStudentSchool =
    { Error : APIError option }


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
    { Error : APIError option}


[<CLIMutableAttribute>]
type ApprovePendingRequest =
    { Email : string
      FirstName : string
      LastName : string }
    static member init = {FirstName = ""; LastName = ""; Email = ""}
    static member of_student (student : Student) = {FirstName = student.FirstName; LastName = student.LastName; Email = student.Email}

[<CLIMutableAttribute>]
type ApprovePendingResult =
    { Error : APIError option}

type DismissPendingRequest =
    { Email : string
      FirstName : string
      LastName : string }
    static member init = {FirstName = ""; LastName = ""; Email = ""}
    static member of_student (student : Student) = {FirstName = student.FirstName; LastName = student.LastName; Email = student.Email}

[<CLIMutableAttribute>]
type DismissPendingResult =
    { Error : APIError option}

[<CLIMutable>]
type StudentRegister =
    { FirstName : string
      LastName : string
      Email : string }
    static member init = {FirstName = ""; LastName = ""; Email = "" }
    member this.is_valid = (this.FirstName <> "" && this.LastName <> "" && this.Email <> "")

[<CLIMutable>]
type TutorRegister =
    { FirstName : string
      LastName : string
      Email : string
      SchoolName : string }
    static member init = {FirstName = ""; LastName = ""; Email = ""; SchoolName = ""}
    member this.is_valid = this.FirstName <> "" && this.LastName <> "" && this.Email <> "" && this.SchoolName <> ""

[<CLIMutable>]
type SaveRequest =
    { FirstName : string
      LastName : string
      SchoolName : string }
    static member init = {FirstName = ""; LastName = ""; SchoolName = ""}
    member this.is_valid = this.FirstName <> "" && this.LastName <> "" && this.SchoolName <> ""
    

[<CLIMutable>]
type School =
    { FirstName : string
      LastName : string
      SchoolName : string }
    static member init first last sn = {FirstName = first; LastName = last; SchoolName = sn}
    member this.is_valid = this.FirstName <> "" && this.LastName <> "" && this.SchoolName <> ""
    
[<CLIMutable>]
type GetAllSchoolsResult =
    { Schools : School list
      Error : APIError option}
    
    static member init = {Schools = []; Error = None} 
    static member db_error msg = {Schools = []; Error = Some (APIError.db msg) } 
    member this.is_valid = this.Schools |> List.exists (fun x -> not x.is_valid)


