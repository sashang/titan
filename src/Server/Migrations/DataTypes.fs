namespace TitanMigrations

type User = 
    { Email : string
      GivenName : string
      Surname : string }

type TitanClaim =
    { UserId : int
      Type : string
      Value : string }
      