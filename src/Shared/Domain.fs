/// Domain model shared between client and server.
namespace Domain

///Flexible API error codes. Interpreted by the client side code depending on context.
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
    | Location
    | TitanOpenTok

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

    static member titan_open_tok msg =
        {Codes = [APICode.TitanOpenTok]; Messages = [msg]}

[<CLIMutable>]
type OpenTokInfo = 
    { SessionId : string
      Token : string
      Key : string }
    static member init = {SessionId = ""; Token = ""; Key = ""}

[<CLIMutable>]
type OTIResponse =
    { Info : OpenTokInfo option
      Error : APIError option}
    static member init = { Info = None; Error = None}


[<CLIMutable>]
type Session =
    { Username : string
      Token : string }
      static member init =
        {Username = ""; Token = ""}

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

///some api handler that uses an email address as a parameter
/// got tired of writign types for each request that used an email.
[<CLIMutable>]
type EmailRequest =
    { Email : string }

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
      Subjects : string
      Location : string
      Info : string
      Error : APIError option }
    static member init = {SchoolName = ""; Subjects = ""; Info = ""; Location = ""; Error = None}

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
      Phone : string
      Email : string }

      static member init =
        {FirstName = ""; LastName = ""; Email = ""; Phone = ""}

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
    static member init = {Error = None; Students = []}
    static member db_error msg = {Students = []; Error = Some (APIError.db msg) } 

[<CLIMutableAttribute>]
type AddStudentSchool =
    { Error : APIError option }


/// Students who have applied to enrol and are waiting for approval
[<CLIMutableAttribute>]
type PendingResult =
    { Error : APIError option
      Students : Student list }
    static member init = {Error = None; Students = []}

//assume student is logged in when enrolling so we have the user info (name, email etc... already in the claims)
[<CLIMutableAttribute>]
type EnrolRequest =
    { SchoolName : string }
    static member init = {SchoolName = ""}
//no enrol result other than APIError


[<CLIMutableAttribute>]
type ApprovePendingRequest =
    { Email : string }
    static member init = {Email = ""}
    static member of_student (student : Student) = {Email = student.Email}


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
      Info : string
      Location : string
      Subjects : string
      SchoolName : string }
    static member init = {FirstName = ""; LastName = ""; SchoolName = ""; Info = ""; Subjects = ""; Location = ""}
    member this.is_valid = this.FirstName <> "" && this.LastName <> "" && this.SchoolName <> "" && this.Location <> ""
    

[<CLIMutable>]
type School =
    { FirstName : string
      LastName : string
      Info : string
      Subjects : string
      Location : string
      SchoolName : string
      Email : string }
    static member init first last sn info subjects location email =
        {FirstName = first; LastName = last; SchoolName = sn; Info = info; Subjects = subjects; Location = location; Email = email}
    
[<CLIMutable>]
type GetAllSchoolsResult =
    { Schools : School list
      Error : APIError option}
    
    static member init = {Schools = []; Error = None} 
    static member db_error msg = {Schools = []; Error = Some (APIError.db msg) } 


[<CLIMutable>]
type DismissStudentRequest =
    { Email : string }


