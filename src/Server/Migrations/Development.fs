namespace TitanMigrations

open FluentMigrator

[<Profile("Development")>]
type Development() =
    inherit Migration()

    override this.Up() =    
        //make some teachers
        this.Insert.IntoTable("User")
            .Row({Email = "charles@xmansion.com"; FirstName = "Charles"; LastName = "Xavier"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 2; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 2; Name = "Charles' School for the Gifted"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "darth@deathstar.com"; FirstName = "Darth"; LastName = "Vader"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 3; Name = "Deathstar"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 3; Type = "IsTutor"; Value = "yes"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "hobbit@hobbiton.com"; FirstName = "John"; LastName = "Tolkien"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 4; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 4; Name = "The Shire"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "william@theglobe.com"; FirstName = "William"; LastName = "Shakespeare"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 5; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 5; Name = "The Globe"}) |> ignore

        this.Insert.IntoTable("User")
            .Row({Email = "marie@radium.com"; FirstName = "Marie"; LastName = "Curie"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 6; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 6; Name = "Radium"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "emily@bronte.com"; FirstName = "Emily"; LastName = "Bronte"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 7; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 7; Name = "The Heights"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "rajas@english.com"; FirstName = "Rajas"; LastName = "Govender"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 8; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 8; Name = "English Achievers"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "kurt@logically.com"; FirstName = "Kurt"; LastName = "Goedel"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 9; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 9; Name = "Logically"}) |> ignore
            
        //make some students
        this.Insert.IntoTable("User")
            .Row({Email = "scott@xmansion.com"; FirstName = "Scott"; LastName = "Summers"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 10; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("Student")
            .Row({UserId = 10; SchoolId = 1}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "albert@princeton.com"; FirstName = "Albert"; LastName = "Einstein"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 11; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("Student")
            .Row({UserId = 11; SchoolId = 1}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "ada@computer.com"; FirstName = "Ada"; LastName = "Lovelace"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 12; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("Student")
            .Row({UserId = 12; SchoolId = 1}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "leonard@bridges.com"; FirstName = "Leonard"; LastName = "Euler"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 13; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("Student")
            .Row({UserId = 13; SchoolId = 2}) |> ignore

        this.Insert.IntoTable("User")
            .Row({Email = "carl@gauss.com"; FirstName = "Carl"; LastName = "Gauss"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 14; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("Student")
            .Row({UserId = 14; SchoolId = 2}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "ghengis@kahn.com"; FirstName = "Ghengis"; LastName = "Kahn"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 15; Type = "IsStudent"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("Student")
            .Row({UserId = 15; SchoolId = 3}) |> ignore
            
    override this.Down() =
        ()