open Database
open Domain
open FSharp.Control.Tasks
open FluentMigrator.Runner
open FluentMigrator.Runner.Initialization
open Giraffe
open Giraffe.Serialization
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Identity.UI.Services
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.Extensions.Identity
open Microsoft.EntityFrameworkCore
open Microsoft.IdentityModel.Tokens
open Saturn
open Saturn.Auth
open Shared
open System
open System.Collections.Generic
open System.IdentityModel.Tokens.Jwt
open System.IO
open System.Security.Claims
open System.Text

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

let json_auth_fail_message : HttpHandler =
    fun next ctx ->
        ctx.WriteJsonAsync "api 403"

let auth_google = pipeline {
    requires_authentication (Giraffe.Auth.challenge "Google")
    plug print_user_details
}

let auth_null : HttpHandler = 
    fun next ctx ->
         next ctx

let sign_out (next : HttpFunc) (ctx : HttpContext) = task {
    return! ctx.WriteJsonAsync
        { SignOutResult.code = [SignOutCode.Success]
          SignOutResult.message = [] }
}

let sign_out_pipeline = pipeline {
    sign_off "Identity.Application"
}

let sign_out_router = router {
    pipe_through sign_out_pipeline
    post "/sign-out" sign_out
}

let logged_in_view = router {
    pipe_through auth_google

}
let default_view = router {
    get "/" (fun next ctx -> task {
        return! next ctx
    })
}

let secret = "spadR2dre#u-ruBrE@TepA&*Uf@U"
let issuer = "saturnframework.io"

let generate_token username =
    let claims = [|
        Claim(JwtRegisteredClaimNames.UniqueName, username);
        Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]
    claims
    |> generateJWT (secret, SecurityAlgorithms.HmacSha256) issuer (DateTime.UtcNow.AddHours(1.0))


let print_claims (claims : TitanClaim list) (logger : ILogger<Debug.DebugLogger>) =
    claims |>
    List.map (fun x -> "type = " + x.Type + " value = " + x.Value) |>
    List.iter (fun x ->  logger.LogWarning(x))

let validate_user (next : HttpFunc) (ctx : HttpContext) = task {
    try
        let! login = ctx.BindJsonAsync<Domain.Login>()
        let sign_in_manager = ctx.GetService<SignInManager<IdentityUser>>()
        let! result = sign_in_manager.PasswordSignInAsync(login.username, login.password, true, false)
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation("attempting to login user " + login.username)
        match result.Succeeded with
        | true ->
            let token = generate_token login.username
            return! ctx.WriteJsonAsync
                { LoginResult.code = [LoginCode.Success]
                  LoginResult.message = []
                  token = token
                  username = login.username }
        | _ -> 
            let msg = sprintf "Failed to login user '%s'" login.username
            logger.LogWarning(msg)
            return! ctx.WriteJsonAsync
                { LoginResult.code = [LoginCode.Failure]
                  LoginResult.message = [msg]
                  token = ""
                  username = "" }
    with ex ->
        return! failwith ("exception: could not validate user: " + ex.Message)
}

let handleGetSecured =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let username = ctx.User.FindFirst ClaimTypes.Name
        let role = ctx.User.FindFirst "TitanRole"
        json ("User " + username.Value + " has titan role " + role.Value) next ctx

///endpoints that require authorization to reach
let secure_router = router {
    pipe_through (Auth.requireAuthentication JWT)
    //pipe_through (pipeline { requires_authentication (json_auth_fail_message)})
    get "/validate" handleGetSecured
    get "/load-school" API.load_school
    post "/create-school" API.create_school
}

let titan_api =  router {
    not_found_handler (text "resource not found")
    get "/user-claims" (fun next ctx -> task {
        let claims = ctx.User.Claims |>
                     List.ofSeq |>
                     List.map (fun (c : Claim) -> {TitanClaim.Value = c.Value; TitanClaim.Type = c.Type})
        print_claims claims |> ignore
        return! ctx.WriteJsonAsync { TitanClaims.Claims = claims }
    })
    post "/login" validate_user
    post "/sign-up" API.sign_up_user
    forward "/sign-out" sign_out_router
    forward "/secure" secure_router
}

///Define the pipeline that http request headers will see
let api_pipeline = pipeline {
    //Ensure that the Accept: application/json header is present. This is probably just for 
    //good practice. I don't fully understand why somoene would want this. Things work without it.
    //Just seems like a way for the api to deny clients who don't accept json, and for clients to tell
    //api's that they accept json.
//    plug acceptJson 
    set_header "x-pipeline-type" "Api"
}
let web_app = router {
    pipe_through api_pipeline
    forward "/api" titan_api
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
            options.Password.RequiredLength <- 4
            options.Password.RequireNonAlphanumeric <- false
            options.Password.RequireUppercase <- false
            options.Password.RequireLowercase <- false

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan  <- TimeSpan.FromMinutes 30.0
            options.Lockout.MaxFailedAccessAttempts <- 10

            // User settings
            options.User.RequireUniqueEmail <- true
        )
        .AddEntityFrameworkStores<IdentityDbContext<IdentityUser>>()
        .AddDefaultTokenProviders() |> ignore

    //apparently putting this in a scope is the thing to do with asp.net...
    let scope = services.BuildServiceProvider(false).CreateScope()
    scope.ServiceProvider.GetRequiredService<IMigrationRunner>() .MigrateUp() |> ignore
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

let configure_logging (builder : ILoggingBuilder) =
    builder.AddConsole()
        .AddDebug() |> ignore

(*
let configure_app (builder : IApplicationBuilder) =
    builder.UseAuthentication()
    *)

let app (startup_options : StartupOptions) = 
    match startup_options.google_id, startup_options.google_secret with 
    | Some id, Some secret ->
        application {
            url ("http://0.0.0.0:" + port.ToString() + "/")
            use_router web_app
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
            use_router web_app
            memory_cache
            use_static publicPath
            service_config configure_services
            //app_config configure_app
            use_jwt_authentication secret issuer
            //use_cookies_authentication "lambdafactory.io"
            logging configure_logging
            use_gzip
        }

let startup_options = {
    google_id = get_env_var "TITAN_GOOGLE_ID"
    google_secret = get_env_var "TITAN_GOOGLE_SECRET"
    enable_test_user = (get_env_var "TITAN_ENABLE_TEST_USER" = Some "yes")
}
run (app startup_options)
