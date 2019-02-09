module DevMigration

open Homeless
open TitanMigrations
open FluentMigrator
open FluentMigrator.Runner
open FluentMigrator.Runner.Initialization
open Microsoft.Extensions.DependencyInjection
open Thoth.Json.Net
open System
open System.Linq

[<Migration(20190209L)>]
type Development() =
    inherit Migration()

    override this.Up() =    
        let lorem = """Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur ut ipsum eu dui sagittis rutrum.
                       Nullam vel lacus vel elit sollicitudin pretium. Quisque scelerisque vitae libero sed maximus.
                       Nam at nisi laoreet, ultricies nulla non, elementum enim. Integer vulputate eleifend lorem eget fermentum.
                       Nullam id lacus quis enim auctor euismod vel a nisi. Integer congue mauris neque, at scelerisque augue dictum ac.
                       Praesent tempus dolor eleifend tortor volutpat lacinia. In nec finibus felis. Vestibulum maximus
                       ligula vel bibendum aliquam. Nulla facilisi."""
        let subjects1 = "Mathematics,Chemistry"
        let subjects2 = "English,VCE,English Literature"
        let subjects3 = "Physics"
        //make some teachers
        this.Insert.IntoTable("User")
            .Row({Email = "charles@xmansion.com"; FirstName = "Charles"; LastName = "Xavier"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 2; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 2; Name = "Charles' School for the Gifted"; Info = lorem; Subjects = subjects1; Location = "USA"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "darth@deathstar.com"; FirstName = "Darth"; LastName = "Vader"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 3; Name = "Deathstar"; Info = lorem; Subjects = subjects1; Location = "Outer Space"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 3; Type = "IsTutor"; Value = "yes"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "hobbit@hobbiton.com"; FirstName = "John"; LastName = "Tolkien"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 4; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 4; Name = "The Shire"; Info = lorem; Subjects = subjects3; Location = "Hobbitton"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "william@theglobe.com"; FirstName = "William"; LastName = "Shakespeare"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 5; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 5; Name = "The Globe"; Info = lorem; Subjects = subjects3; Location = "London"}) |> ignore

        this.Insert.IntoTable("User")
            .Row({Email = "marie@radium.com"; FirstName = "Marie"; LastName = "Curie"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 6; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 6; Name = "Radium"; Info = lorem; Subjects = subjects3; Location = "Paris"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "emily@bronte.com"; FirstName = "Emily"; LastName = "Bronte"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 7; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 7; Name = "The Heights"; Info = lorem; Subjects = subjects3; Location = "England"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "rajas@english.com"; FirstName = "Rajas"; LastName = "Govender"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 8; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 8; Name = "English Achievers"; Info = lorem; Subjects = subjects3; Location = "Melbourne, Australia"}) |> ignore
            
        this.Insert.IntoTable("User")
            .Row({Email = "kurt@logically.com"; FirstName = "Kurt"; LastName = "Goedel"; Phone = "2928092"}) |> ignore
        this.Insert.IntoTable("TitanClaims")
            .Row({UserId = 9; Type = "IsTutor"; Value = "yes"}) |> ignore
        this.Insert.IntoTable("School")
            .Row({UserId = 9; Name = "Logically"; Info = lorem; Subjects = subjects3; Location = "Princeton"}) |> ignore
            
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


let create_services connection_string =
        // let builder = NpgsqlConnectionStringBuilder(connection_string)
        // builder.SslMode <- SslMode.Require
        // builder.TrustServerCertificate <- true
        // builder.UseSslStream <- true
        // let connection = new NpgsqlConnection(builder.ConnectionString)
        // connection.ProvideClientCertificatesCallback <- (fun certs -> certs.Add(new X509Certificate2(cert)) |> ignore)
        // connection
        ServiceCollection()
            // Add common FluentMigrator services
            .AddFluentMigratorCore()
            .ConfigureRunner(
                (fun (rb : IMigrationRunnerBuilder) -> rb.AddPostgres()
                                                         .WithGlobalConnectionString(connection_string)
                                                         .ScanIn(typeof<Development>.Assembly).For.Migrations() |> ignore))
            // Enable logging to console in the FluentMigrator way
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
            // Build the service provider
            .BuildServiceProvider(false)

let settings = System.IO.File.ReadAllText("../appsettings.Development.json")
let decoder = Decode.Auto.generateDecoder<RecStartupOptions>()
let result = Decode.fromString decoder settings
match result with
| Ok startup_options ->
    create_services startup_options.ConnectionString |> ignore
| Error e -> failwith ("failed to read appsettings.json: " + e) |> ignore
