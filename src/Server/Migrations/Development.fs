namespace TitanMigrations

open FluentMigrator

[<Profile("Development")>]
type Development() =
    inherit Migration()

    override this.Up() =    
        //make some teachers
        this.Insert.IntoTable("User")
            .Row({Email = "mary@gmail.com"; GivenName = "Mary"; Surname = "Poppins"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 3; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("User")
            .Row({Email = "charles@xmansion.com"; GivenName = "Charles"; Surname = "Xavier"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 4; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("User")
            .Row({Email = "darth@deathstar.com"; GivenName = "Darth"; Surname = "Vader"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 5; Type = "IsTutor"; Value = "yes"}) |> ignore

        //make some students
        this.Insert.IntoTable("User")
            .Row({Email = "scott@xmansion.com"; GivenName = "Scott"; Surname = "Summers"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 6; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("User")
            .Row({Email = "albert@princeton.com"; GivenName = "Albert"; Surname = "Einstein"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 7; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("User")
            .Row({Email = "ada@computer.com"; GivenName = "Ada"; Surname = "Lovelace"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 8; Type = "IsStudent"; Value = "yes"}) |> ignore

    override this.Down() =
        ()