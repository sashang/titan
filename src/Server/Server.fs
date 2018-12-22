open Database
open Domain
open FSharp.Control.Tasks
open FluentMigrator.Runner
open FluentMigrator.Runner.Initialization
open Giraffe
open Giraffe.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.Extensions.Identity
open Microsoft.EntityFrameworkCore
open Saturn
open Shared
open System
open System.IO
open System.Security.Claims
open ValueDeclarations

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

type StartupOptions = {
    google_id : string option
    google_secret : string option
    enable_test_user : bool
}


let print_user_details : HttpHandler =
    fun next ctx ->
        ctx.User.Claims |> Seq.iter (fun claim ->
            if claim.Issuer = "Google" && (claim.Type = ClaimTypes.Name || claim.Type = ClaimTypes.Email) then
                printfn "%s" claim.Value)
        next ctx

let auth_google = pipeline {
    requires_authentication (Giraffe.Auth.challenge "Google")
    plug print_user_details
}

let auth_null : HttpHandler = 
    fun next ctx ->
         next ctx

let logout = pipeline {
    sign_off "Cookies"
}

let logged_in_view = router {
    pipe_through auth_google

}
let default_view = router {
    get "/" (fun next ctx -> task {
        return! next ctx
    })
}

let validate_user startup_options next (ctx : HttpContext) = task {
        let! login = ctx.BindJsonAsync<Domain.Login>()
        return!
            match login.is_valid() with
            | true  ->
                //let data = Auth.createUserData login
                //ctx.WriteJsonAsync data
                ctx.WriteJsonAsync login
            | false -> RequestErrors.UNAUTHORIZED "Bearer" "" (sprintf "User '%s' can't be logged in." login.username) next ctx
    }

let titan_api (startup_options : StartupOptions) =  router {
    get "/user-credentials" (fun next ctx -> task {
        let name = ctx.User.Claims |> Seq.filter (fun claim -> claim.Type = ClaimTypes.Name) |> Seq.head
        return! json { user_name = name.Value } next ctx
    })
    post "/login" (validate_user startup_options)
    post "/sign-up" (SignUp.sign_up_user)
    post "/create-school" CreateSchool.create_school
}

let web_app (startup_options : StartupOptions) =
    router {
        pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
        pipe_through (match startup_options.google_id, startup_options.google_secret with
                      | Some id, Some secret -> auth_google
                      | _ -> auth_null )
        forward "/api" (titan_api startup_options)
    }

let configure_services (services:IServiceCollection) =
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings) |> ignore
    services.AddSingleton<IDatabase>(Database()) |> ignore
    services.AddEntityFrameworkNpgsql() |> ignore
    services.AddDbContext<IdentityDbContext<IdentityUser>>(
        fun options ->
            //options.UseInMemoryDatabase("NameOfDatabase") |> ignore
            options.UseNpgsql(PG_DEV_CON) |> ignore
        ) |> ignore


    services.AddFluentMigratorCore()
            .ConfigureRunner(fun rb ->
                rb.AddPostgres()
                  .WithGlobalConnectionString(PG_DEV_CON)
                  .ScanIn(typeof<TitanMigrations.AddSchool>.Assembly).For.Migrations() |> ignore)
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore) |> ignore

    services.AddIdentity<IdentityUser, IdentityRole>(
        fun options ->
            // Password settings
            options.Password.RequireDigit   <- true
            options.Password.RequiredLength <- 8
            options.Password.RequireNonAlphanumeric <- false
            options.Password.RequireUppercase <- true
            options.Password.RequireLowercase <- false

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan  <- TimeSpan.FromMinutes 30.0
            options.Lockout.MaxFailedAccessAttempts <- 10

            // User settings
            options.User.RequireUniqueEmail <- true
        )
        .AddEntityFrameworkStores<IdentityDbContext<IdentityUser>>()
        .AddDefaultTokenProviders() |> ignore
    
    services.BuildServiceProvider(false)
            .GetRequiredService<IMigrationRunner>()
            .MigrateUp() |> ignore
    services

let get_env_var var =
    match Environment.GetEnvironmentVariable(var) with
    | null -> None
    | value -> Some value

let configure_cors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let app (startup_options : StartupOptions) = 
    match startup_options.google_id, startup_options.google_secret with 
    | Some id, Some secret ->
        application {
            url ("http://0.0.0.0:" + port.ToString() + "/")
            use_router (web_app startup_options)
            memory_cache
            use_static publicPath
            service_config configure_services
            use_gzip
            use_google_oauth id secret "/oauth_callback_google" [] 
            use_cors "CORSPolicy" configure_cors
        }
    | _ ->
        application {
            url ("http://0.0.0.0:" + port.ToString() + "/")
            use_router (web_app startup_options)
            memory_cache
            use_static publicPath
            service_config configure_services
            use_gzip
        }

let startup_options = {
    google_id = get_env_var "TITAN_GOOGLE_ID"
    google_secret = get_env_var "TITAN_GOOGLE_SECRET"
    enable_test_user = (get_env_var "TITAN_ENABLE_TEST_USER" = Some "yes")
}
run (app startup_options)
