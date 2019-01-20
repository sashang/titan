module Server


open Database
open Domain
open FSharp.Control.Tasks
open FluentMigrator.Runner
open Giraffe
open Giraffe.Serialization
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration  
open Microsoft.IdentityModel.Tokens
open Saturn
open Saturn.Auth
open System
open System.IdentityModel.Tokens.Jwt
open System.IO
open System.Security.Claims

open Thoth.Json.Net

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us


type RecStartupOptions = {
    JWTSecret : string 
    JWTIssuer : string 
    ConnectionString : string
    GoogleClientId : string
    GoogleSecret : string
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


let auth_null : HttpHandler =
    fun next ctx ->
         next ctx

let sign_out (next : HttpFunc) (ctx : HttpContext) = task {
    return! ctx.WriteJsonAsync
        { SignOutResult.code = [SignOutCode.Success]
          SignOutResult.message = [] }
}

let sign_out_pipeline = pipeline {
    sign_off "Cookies"
}

let sign_out_router = router {
    pipe_through sign_out_pipeline
    get "/sign-out" sign_out
}

let auth_google = pipeline {
    //plug (enableCors CORS.defaultCORSConfig)
    requires_authentication (Giraffe.Auth.challenge "Google")
    //plug print_user_details
}

let logged_in = router {
    pipe_through auth_google
}

let is_tutor = pipeline {
    plug logged_in
    requires_role "Tutor" json_auth_fail_message
}

let default_view = router {
    get "/" (fun next ctx -> task {
        return! next ctx
    })
}
let generate_token secret issuer (ctx : HttpContext) = task {
    let user = ctx.User
    let db = ctx.GetService<IDatabase>()
    let given_name = user.FindFirst(ClaimTypes.GivenName).Value
    let surname = user.FindFirst(ClaimTypes.Surname).Value
    let email = user.FindFirst(ClaimTypes.Email).Value
    let! result = db.query_claims email
    match result with
    | Error message -> return failwith message //no claims no token
    | Ok titan_claims ->
        let claims = [| for claim in titan_claims do 
                           yield Claim(claim.Type, claim.Value) |] 
        return 
            [| Claim(JwtRegisteredClaimNames.Email, email);
               Claim(JwtRegisteredClaimNames.GivenName, given_name);
               Claim(JwtRegisteredClaimNames.FamilyName, surname);
               Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]
            |> Array.append claims
            |> generateJWT (secret, SecurityAlgorithms.HmacSha256) issuer (DateTime.UtcNow.AddHours(1.0))
}

let check_session (next : HttpFunc) (ctx : HttpContext) = task {
    try
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let config = ctx.GetService<IConfiguration>()
        logger.LogInformation ("checking if user is authenticated")
        if ctx.User.Identity.IsAuthenticated then
            let name = ctx.User.Identity.Name
            let auth_type = ctx.User.Identity.AuthenticationType
            logger.LogInformation ("name = " + name + " auth type = " + auth_type)
            ctx.User.Claims 
            |> Seq.map (fun claim -> "type = " + claim.Type + " value = " + claim.Value)
            |> Seq.iter (fun message -> logger.LogInformation (message)) |> ignore
            let! token =  generate_token config.["JWTSecret"] config.["JWTIssuer"] ctx
            logger.LogInformation ("Generated token = " + token)
            return! ctx.WriteJsonAsync {Session.init with Token = token; Username = name}
        else
            return! RequestErrors.UNAUTHORIZED "Bearer" "" ("no user logged in") next ctx
    with ex ->
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        logger.LogInformation ("no session")
        return! failwith ("no session for user" )
}

let render_school_view (next : HttpFunc) (ctx : HttpContext) = task {
    try
        let logger = ctx.GetLogger<Debug.DebugLogger>()
        let config = ctx.GetService<IConfiguration>()
        let db = ctx.GetService<IDatabase>()
        let! result = db.get_school_view
        match result with
        | Ok schools ->
            return! (htmlView (SchoolView.view schools)) next ctx
        | Error message ->
            return! (htmlString message) next ctx
                
    with ex ->
        return! failwith ("COuld not render the school view" )
}


let handleGetSecured =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let username = ctx.User.FindFirst ClaimTypes.Name
        let role = ctx.User.FindFirst "TitanRole"
        json ("User " + username.Value + " has titan role " + role.Value) next ctx

///endpoints that require authorization to reach
let secure_router = router {
    pipe_through logged_in
    not_found_handler (redirectTo false "/")
}

let api_pipeline = pipeline {
    plug acceptJson 
    set_header "x-pipeline-type" "Api"
}
let titan_api =  router {
    not_found_handler (json "resource not found")
    //pipe_through api_pipeline
    post "/enrol" API.enrol
    get "/load-school" API.load_school
    get "/load-user" API.load_user
    //get "/signin-google" (redirectTo false "/api/secure")
    post "/register-punter" API.register_punter
    post "/register-tutor" API.register_tutor
    post "/register-student" API.register_student
    post "/save-tutor" API.save_tutor
}

///Define the pipeline that http request headers will see
let defaultView = router {
    printfn "default view"
    get "/" (json "root")
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")
    get "/schools.html" render_school_view
}

let browser = pipeline {
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}
let browser_router = router {
    not_found_handler (redirectTo false "/") //Use the default 404 webpage
    pipe_through browser //Use the default browser pipeline
    forward "/signin-google" secure_router
    forward "" defaultView //Use the default view
}

let web_app = router {
    get "/check-session" check_session
    forward "/sign-out" sign_out_router
    forward "/api" titan_api
    forward "" browser_router
}

let configure_services startup_options (services:IServiceCollection) =
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings) |> ignore
    services.AddSingleton<IDatabase>(Database(startup_options.ConnectionString)) |> ignore

    services.AddFluentMigratorCore()
            .ConfigureRunner(fun rb ->
                rb.AddSqlServer2016()
                  .WithGlobalConnectionString(startup_options.ConnectionString)
                  .ScanIn(typeof<TitanMigrations.Initial>.Assembly).For.Migrations() |> ignore)
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore) |> ignore

    //apparently putting this in a scope is the thing to do with asp.net...
    let scope = services.BuildServiceProvider(false).CreateScope()
    scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp() |> ignore
    //return the list of servicesk
    services

let get_env_var var =
    match Environment.GetEnvironmentVariable(var) with
    | null -> None
    | value -> Some value

let configure_logging (builder : ILoggingBuilder) =
    builder.AddConsole()
        .AddDebug() |> ignore
let configure_host (builder : IWebHostBuilder) =
    //turns out if you pass an anonymous function to a function that expects an Action<...> or
    //Func<...> the type inference will work out the inner types....so you don't need to specify them.
    let settings_file =
        match get_env_var "ASPNETCORE_ENVIRONMENT" with
        | None -> "appsettings.json"
        | Some e -> "appsettings."+e+".json"
    builder.ConfigureAppConfiguration((fun ctx builder -> builder.AddJsonFile(settings_file) |> ignore))

let app (startup_options : RecStartupOptions) =
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router web_app
        memory_cache
        use_static publicPath
        service_config (configure_services startup_options)
        host_config configure_host
        use_google_oauth startup_options.GoogleClientId startup_options.GoogleSecret "/oauth_callback_google" ["language", "urn:google:language"]
        logging configure_logging
        use_gzip
    }

let settings = System.IO.File.ReadAllText("appsettings.json")
let decoder = Decode.Auto.generateDecoder<RecStartupOptions>()
let result = Decode.fromString decoder settings
match result with
| Ok startup_options -> run (app startup_options)
| Error e -> failwith ("failed to read appsettings.json: " + e)
