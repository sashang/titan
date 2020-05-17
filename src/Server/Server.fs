module Server

open AzureMaps
open Database
open Domain
open FSharp.Control.Tasks
open FluentMigrator.Runner
open Giraffe
open Giraffe.Serialization
open Homeless
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.IdentityModel.Tokens
open Microsoft.AspNetCore.Authentication
open Saturn
open Saturn.Auth
open System
open System.IdentityModel.Tokens.Jwt
open System.IO
open System.Net
open System.Security.Claims
open System.Security.Cryptography.X509Certificates
open Thoth.Json.Net
open Thoth.Json.Giraffe
open TitanOpenTok
open TokBoxCB
open UAParser

type AspNetCoreGoogleOpts = Microsoft.AspNetCore.Authentication.Google.GoogleOptions

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

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
        { SignOutResult.Code = SignOutCode.Success
          SignOutResult.Message = "Successfully signed out" }
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
        let logger = ctx.GetLogger<Debug.DebugLoggerProvider>()
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
        let logger = ctx.GetLogger<Debug.DebugLoggerProvider>()
        logger.LogInformation ("no session")
        return! failwith ("no session for user" )
}

let render_school_view (next : HttpFunc) (ctx : HttpContext) = task {
    try
        let logger = ctx.GetLogger<Debug.DebugLoggerProvider>()
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
    get "/load-school" API.load_school
    get "/load-user" API.load_user
    get "/get-all-schools" API.get_all_schools
    get "/get-pending-schools" API.get_pending_schools
    get "/get-all-students" API.get_all_students
    get "/get-pending" API.get_pending
    get "/get-session-id" API.get_session_id
    get "/get-enrolled-schools" API.get_enrolled_schools
    get "/get-unenrolled-schools" API.get_unenrolled_schools
    get "/get-users-for-titan" API.get_approved_users_for_titan
    get "/get-unapproved-users-for-titan" API.get_unapproved_users_for_titan
    get "/get-azure-maps-keys" API.get_azure_maps_keys

    //get "/signin-google" (redirectTo false "/api/secure")
    post "/enrol-student" API.enrol
    post "/register-punter" API.register_punter
    post "/register-tutor" API.register_tutor
    post "/register-student" API.register_student
    post "/approve-pending" API.approve_enrolment_request
    post "/dismiss-student" API.dismiss_student
    post "/dismiss-pending" API.dismiss_pending
    post "/save-tutor" API.save_tutor
    post "/student-get-session" API.get_session_id_for_student
    post "/update-user-approval" API.update_user_claims
    post "/delete-user-titan" API.delete_user_titan

    //calback for tokbox to tell us about sessions starting/ending etc.
    post "/tokbox-cb" TokBoxCB.callback
    post "/tokbox-find-by-name" TokBoxCB.find_by_name
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


let check_same_site (context : HttpContext) (options : CookieOptions) =
    if (options.SameSite = SameSiteMode.None) then
        let user_agent = context.Request.Headers.["User-Agent"].ToString()
        let parser = Parser.GetDefault()
        let client_info = parser.Parse(user_agent)
        let ua = client_info.UA
        let logger = context.GetLogger<Debug.DebugLoggerProvider>()
    //    options.SameSite <- SameSiteMode.None
     //   options.Secure <- true
        logger.LogInformation(ua.Family)
        logger.LogInformation(ua.Major)
        if (not(ua.Family = "Chrome" && ua.Major = "80")) then
            logger.LogInformation("User agent does not support samesite")
            options.SameSite <- SameSiteMode.Unspecified


let configure_services startup_options (services:IServiceCollection) =
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings) |> ignore
    //services.AddSingleton<IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer) |> ignore
    services.AddSingleton<IDatabase>(Database(startup_options.ConnectionString)) |> ignore
    services.AddSingleton<ITitanOpenTok>(TitanOpenTok(startup_options.OpenTokKey, startup_options.OpenTokSecret)) |> ignore
    services.AddSingleton<IAzureMaps>(AzureMaps(startup_options.AzureMapsClientId, startup_options.AzureMapsPrimaryKey)) |> ignore

    services.AddFluentMigratorCore()
            .ConfigureRunner(fun rb ->
                rb.AddSqlServer2016()
                  .WithGlobalConnectionString(startup_options.ConnectionString)
                  .ScanIn(typeof<TitanMigrations.Initial>.Assembly)
                  .ScanIn(typeof<TitanMigrations.FixForeignKeys>.Assembly)
                  .For.Migrations() |> ignore)
            .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore) |> ignore

    services.Configure<CookiePolicyOptions>(fun (options : CookiePolicyOptions) ->
        options.MinimumSameSitePolicy <- SameSiteMode.Unspecified) |> ignore
(*     services.Configure<CookiePolicyOptions>(fun (options : CookiePolicyOptions) ->
        options.MinimumSameSitePolicy <- SameSiteMode.Unspecified
        options.OnAppendCookie <- (fun cookie_context -> check_same_site cookie_context.Context cookie_context.CookieOptions)
        options.OnDeleteCookie <- (fun cookie_context -> check_same_site cookie_context.Context cookie_context.CookieOptions)) |> ignore *)

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


let configure_host (settings_file : string) (builder : IWebHostBuilder) =
    //turns out if you pass an anonymous function to a function that expects an Action<...> or
    //Func<...> the type inference will work out the inner types....so you don't need to specify them.
    builder.ConfigureAppConfiguration((fun ctx builder -> builder.AddJsonFile(settings_file).Build() |> ignore) ) |> ignore
        // .ConfigureKestrel(fun ctx opt ->
        //     let certificate_file = (ctx.Configuration.["certificateSettings:filename"])
        //     let certificate_password = (ctx.Configuration.["certificateSettings:password"])
        //     let certificate = new X509Certificate2(certificate_file, certificate_password)
        //     opt.AddServerHeader <- false
        //     opt.Listen(IPAddress.Loopback, 4431, (fun opt -> opt.UseHttps(certificate) |> ignore)))
        //  .UseUrls("https://localhost:4431") |> ignore
    builder

let configure_app (settings_file : string) (builder : IApplicationBuilder) =
    builder.UseCookiePolicy() |> ignore //before anything that uses cookies, like UseAuthentication
    builder.UseAuthentication() |> ignore
    builder


let app settings_file (startup_options : RecStartupOptions) =
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router web_app
        memory_cache
        use_static publicPath
        service_config (configure_services startup_options)
        host_config (configure_host settings_file)
        app_config (configure_app settings_file)

        use_google_oauth_with_config (fun (opt:AspNetCoreGoogleOpts) ->
            opt.ClientSecret <- startup_options.GoogleSecret
            opt.ClientId <- startup_options.GoogleClientId
            opt.CallbackPath <- PathString("/oauth_callback_google")
            opt.UserInformationEndpoint <- "https://www.googleapis.com/oauth2/v2/userinfo"
            opt.ClaimActions.Clear()
            opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id")
            opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "name")
            opt.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name")
            opt.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name")
            opt.ClaimActions.MapJsonKey("urn:google:profile", "link")
            opt.ClaimActions.MapJsonKey(ClaimTypes.Email, "email"))
            // opt. startup_options.GoogleClientId startup_options.GoogleSecret "/oauth_callback_google" ["language", "urn:google:language"]
        logging configure_logging
        use_gzip
    }

let settings_file =
    match get_env_var "ASPNETCORE_ENVIRONMENT" with
        | None -> "appsettings.json"
        | Some e -> "appsettings."+e+".json"
let settings = System.IO.File.ReadAllText(settings_file)
let decoder = Decode.Auto.generateDecoder<RecStartupOptions>()
let result = Decode.fromString decoder settings
match result with
    | Ok startup_options -> run (app settings_file startup_options)
    | Error e -> failwith ("failed to read appsettings.json: " + e)
