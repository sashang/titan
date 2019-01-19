open Database
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open System.Security.Claims
open Microsoft.EntityFrameworkCore
open Models
open System.Threading.Tasks


let create_services : ServiceProvider =
    let services = ServiceCollection() :> IServiceCollection
    services.AddSingleton<IDatabase>(Database("Host=localhost;Database=titan_dev;Username=titan_dev;Password=1234")) |> ignore
    services.AddEntityFrameworkNpgsql() |> ignore
    services.AddDbContext<IdentityDbContext<IdentityUser>>(
        fun options ->
            options.UseNpgsql("Host=localhost;Database=titan_dev;Username=titan_dev;Password=1234") |> ignore
    ) |> ignore

    services.AddIdentity<IdentityUser, IdentityRole>(
        fun options ->
            // Password settings
            options.Password.RequireDigit   <- true
            options.Password.RequiredLength <- 4
            options.Password.RequireNonAlphanumeric <- false
            options.Password.RequireUppercase <- false
            options.Password.RequireLowercase <- false

            // User settings
            options.User.RequireUniqueEmail <- true
        )
        .AddEntityFrameworkStores<IdentityDbContext<IdentityUser>>()
        .AddDefaultTokenProviders() |> ignore
    
    services.BuildServiceProvider(false)



[<EntryPointAttribute>]
let main args = 
    let service = create_services
    let db = service.GetService<IDatabase>()
    let user_manager = service.GetService<UserManager<IdentityUser>>()
    let result = task {
        //add a tutor
        let user = IdentityUser(UserName = "xavier", Email = "xaviar@xmansion.com")
        let! _ = user_manager.CreateAsync(user, "1234")
        let! result = db.query_id "xavier"
        match result with
        | Ok user_id ->
            printfn "xavier user id = %s" user_id
            //add a school
            task {
                let! res = db.insert_school {Models.init with Name="xmansion"; UserId = user_id}
                match res with
                | Ok _ ->
                    printfn "add xmansion school"
                    return result
                | Error e ->
                    printfn "%s" e
                    return failwith e
            } |> ignore
             
        | Error message ->
            printfn "error: %s" message
            return failwith message

        db.insert_student "Scott" "Summers"  "cyclops@xmansion.com" |> ignore
        db.insert_student "Logan" "" "wolverine@xmansion.com" |> ignore
    }
    result.Wait()
    0