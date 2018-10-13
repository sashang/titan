open Database
open Giraffe
open Giraffe.Serialization
open FileSystemDatabase
open Maybe
open Microsoft.Extensions.DependencyInjection
open Saturn
open ServerCode
open Shared
open System
open System.IO
open System.Security.Claims

let publicPath = Path.GetFullPath "../Client/public"
let port = 8085us

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

let auth_null = pipeline {
    plug (fun next ctx -> next ctx)
}

let logout = pipeline {
    sign_off "Cookies"
}

let logged_in_view = router {
    pipe_through auth_google

    get "/user-credentials" (fun next ctx -> task {
        let name = ctx.User.Claims |> Seq.filter (fun claim -> claim.Type = ClaimTypes.Name) |> Seq.head
        return! json { user_name = name.Value } next ctx
    })
}
let default_view = router {
    get "/" (fun next ctx -> task {
        return! next ctx
    })
}

let titan_api (db : IDatabaseFunctions) =  router {
    get "/schools" (fun next ctx -> task {
        let! schools = db.load_schools
        return! ctx.WriteJsonAsync schools
    })
}

let google_web_app use_fs_db =
    let db = Database.get_database DatabaseType.FileSystem 
    router {
        pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
        pipe_through auth_google
        forward "/api" (titan_api db)
    }

let web_app use_fs_db =
    let db = Database.get_database DatabaseType.FileSystem 
    router {    
        pipe_through (pipeline { set_header "x-pipeline-type" "Api" })
        forward "/api" (titan_api db)
    }

let configureSerialization (services:IServiceCollection) =
    let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
    fableJsonSettings.Converters.Add(Fable.JsonConverter())
    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings)

let get_env_var var =
    match Environment.GetEnvironmentVariable(var) with
    | null -> None
    | value -> Some value

let app google_id google_secret use_fs_db =
    match google_id, google_secret with
    | Some id, Some secret -> application {
            url ("http://0.0.0.0:" + port.ToString() + "/")
            use_router (google_web_app use_fs_db)
            memory_cache
            use_static publicPath
            service_config configureSerialization
            use_gzip
            use_google_oauth id secret "/oauth_callback_google" []
        }
    | _, _ -> application {
            url ("http://0.0.0.0:" + port.ToString() + "/")
            use_router (web_app use_fs_db)
            memory_cache
            use_static publicPath
            service_config configureSerialization
            use_gzip
        }


let google_id = get_env_var "TITAN_GOOGLE_ID"
let google_secret = get_env_var "TITAN_GOOGLE_SECRET"
let use_filesystem_db = (get_env_var "TITAN_USE_FILESYSTEM_DB" = Some "yes")
run (app google_id google_secret use_filesystem_db)
