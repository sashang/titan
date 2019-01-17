namespace TitanMigrations

open FluentMigrator

[<Profile("Development")>]
type Development() =
    inherit Migration()

    override this.Up() =    
        //make some teachers
        this.Insert.IntoTable("User")
            .Row({Email = "mary@gmail.com"; FirstName = "Mary"; LastName = "Poppins"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 3; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("User")
            .Row({Email = "charles@xmansion.com"; FirstName = "Charles"; LastName = "Xavier"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 4; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("User")
            .Row({Email = "darth@deathstar.com"; FirstName = "Darth"; LastName = "Vader"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 5; Type = "IsTutor"; Value = "yes"}) |> ignore

        //make some students
        this.Insert.IntoTable("User")
            .Row({Email = "scott@xmansion.com"; FirstName = "Scott"; LastName = "Summers"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 6; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("User")
            .Row({Email = "albert@princeton.com"; FirstName = "Albert"; LastName = "Einstein"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 7; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("User")
            .Row({Email = "ada@computer.com"; FirstName = "Ada"; LastName = "Lovelace"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 8; Type = "IsStudent"; Value = "yes"}) |> ignore

    override this.Down() =
        ()