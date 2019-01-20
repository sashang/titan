namespace TitanMigrations

type User = 
    { Email : string
      FirstName : string
      LastName : string }

type TitanClaim =
    { UserId : int
      Type : string
      Value : string }
      
type School =
    { UserId : int
      Name : string }
      
