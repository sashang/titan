module Client.Main

open Client.Pages
open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Import
open Fable.Import.Browser
open Fable.PowerPack
open Shared

let secret = "spadR2dre#u-ruBrE@TepA&*Uf@U"
let issuer = "saturnframework.io"

let handleNotFound (model: SinglePageState) =
    Browser.console.error("Error parsing url: " + Browser.window.location.href)
    ( model, Navigation.modifyUrl (Client.Pages.to_path Client.Pages.PageType.Login) )

/// The navigation logic of the application given a page identity parsed from the .../#info
/// information in the URL.
let url_update (result : Client.Pages.PageType option) (model : SinglePageState) =
    match result with
    | None ->
        handleNotFound model

    | Some Client.Pages.PageType.Home ->
        { model with page = HomeModel; session = None }, Cmd.none

    | Some Client.Pages.PageType.FirstTime ->
        { model with page = FirstTimeModel (FirstTime.init ()); session = None }, Cmd.none

    | Some Client.Pages.PageType.Login ->
        { model with page = LoginModel Login.init; session = None }, Cmd.none

    | Some Client.Pages.PageType.NewTeacher ->
        { model with page = NewTeacherModel (NewTeacher.init ()); session = None }, Cmd.none

    | Some Client.Pages.PageType.NewStudent ->
        { model with page = NewPupilModel; session = None }, Cmd.none
    
    | Some Client.Pages.PageType.HowItWorks ->
        { model with page = HowItWorksModel; session = None }, Cmd.none

    | Some Client.Pages.PageType.MainSchool ->
        {model with page = MainSchoolModel (MainSchool.init "" "" []); session = None}, Cmd.none

    | Some AddClass ->
        { model with page = AddClassModel (AddClass.init ()); session = None }, Cmd.none
    
    | Some SignUp ->
        { model with page = SignUpModel (SignUp.init ()); session = None }, Cmd.none
        
let init _ : SinglePageState * Cmd<Msg> =
    {page = HomeModel; session = None}, Cmd.none

let print_claims (claims : TitanClaim list) =
    claims |>
    List.map (fun x -> "type = " + x.Type + " value = " + x.Value) |>
    List.iter (fun x -> Browser.console.info x)
    

(*
    have a look at the parent-child description at
    https://elmish.github.io/elmish/parent-child.html to understand how update messages
    propagate from the child to parent. It's more subtle than it appears from surface.
*)
let update (msg : Msg) (sps : SinglePageState) : SinglePageState * Cmd<Msg> =
    match msg, sps with    
    | SignUpMsg msg, {page = SignUpModel model} ->
        let model', cmd = SignUp.update msg model
        {sps with page = SignUpModel model'}, Cmd.map SignUpMsg cmd

    | LoginMsg msg, {page = LoginModel login_model; session = None} ->
        match msg with
        //hijack the login response since this contains the session token.
        //normally we don't hijack a message since it violates the elm conceptual flow - the child
        //and only yhe child should know what to do with a message inteded for it it but in thst
        //case we need to store the session info in the toplevel
        | Login.Msg.Response session ->
            Browser.console.debug ("hijacking login response ")
            (*
            let jwt_handler = JwtSecurityTokenHandler()
            let tok_params = TokenValidationParameters()
            let claims, validated_token = jwt_handler.ValidateToken(session.token, tok_params)
            let role = claims.FindFirst "TitanRole"
            Browser.console.info ("has role = " + role.Value)*)
            let next_state = {sps with  page = MainSchoolModel (MainSchool.init "" "" []); session = Some session}
            //let login_model', cmd = Login.update msg login_model
            //{ sps with page = LoginModel login_model'; session = Some session }, Cmd.map LoginMsg cmd
            //next_state, Navigation.newUrl (Client.Pages.to_path Client.Pages.MainSchool)
            next_state, Cmd.none 

        //in this case pass the message through
        | _ ->
            let login_model', cmd = Login.update msg login_model
            { sps with page = LoginModel login_model'}, Cmd.map LoginMsg cmd

    //any page with no session is not allowed to be accesses
    | _, {session = None} -> 
        Browser.console.info ("No authorization to access this page")
        sps, Cmd.none

    //from this point all pages have a session
    | FirstTimeMsg msg, {page = FirstTimeModel ft_model}  ->
        let ft_model', cmd = FirstTime.update msg ft_model
        { sps with page = FirstTimeModel ft_model' }, Cmd.map FirstTimeMsg cmd

    | NewTeacherMsg msg, {page = NewTeacherModel nt_model} ->
        let nt_model', cmd = NewTeacher.update msg nt_model
        {sps with page = NewTeacherModel nt_model' }, Cmd.map NewTeacherMsg cmd

    | MainSchoolMsg msg, {page = MainSchoolModel main_school_model; session = Some session} ->
        let main_school_model', cmd = MainSchool.update msg main_school_model session
        {sps with page = MainSchoolModel main_school_model'}, Cmd.map MainSchoolMsg cmd

    | AddClassMsg msg, {page = AddClassModel model} ->
        let model', cmd = AddClass.update msg model
        {sps with page = AddClassModel model'}, Cmd.map AddClassMsg cmd

    //don't care
    | _, _ -> sps, Cmd.none


let show = function
| Some x -> string x
| None -> "Loading..."

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

let view model dispatch =
    view_page model dispatch 

Program.mkProgram init update view
|> Program.toNavigable Client.Pages.url_parser url_update
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
