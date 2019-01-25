namespace TitanMigrations

type User = 
    { Email : string
      FirstName : string
      Phone : string
      LastName : string }

type TitanClaim =
    { UserId : int
      Type : string
      Value : string }
      
type School =
    { UserId : int
      Info : string
      Subjects : string
      Name : string }
    
type Student =
    { UserId : int
      SchoolId : int }
      
