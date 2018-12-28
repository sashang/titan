module Client.NewTeacher

open Domain
open Fulma
open Elmish
open Elmish.Browser.Navigation
open Fable.Helpers.React
open Fable.Import
open Style
open Fable.PowerPack
open ModifiedFableFetch
open Thoth.Json

type Model = {
    school_name : string
    teacher_name : string
    create_school_result : CreateSchoolResult option
}

type Msg =
| SetSchoolName of string
| SetTeacherName of string
| Submit
| Response of CreateSchoolResult
| SubmissionFailure of exn
| SignOutMsg of SignOut.Msg


let submit (school : Domain.CreateSchool) = promise {
    let body = Encode.Auto.toString(2, school)
    let! response = post_record "/api/create-school" body []
    let decoder = Decode.Auto.generateDecoder<Domain.CreateSchoolResult>()
    let! text = response.text()
    let result = Decode.fromString decoder text
    match result with
    | Ok create_school_result -> return create_school_result
    | Error e -> return failwithf "fail: %s" e
}

let init () =
    { school_name = ""; teacher_name = ""; create_school_result = None }

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    | SetSchoolName name ->
        {model with school_name = name}, Cmd.none
    | SetTeacherName name ->
        {model with teacher_name = name}, Cmd.none
    | Submit ->
        model, Cmd.ofPromise submit
            { CreateSchool.Name =  model.school_name;
              CreateSchool.Principal = model.teacher_name } Response SubmissionFailure
    | Response result ->
        match result.code with
        | [] ->
            Browser.console.info ("Sign up success ")
            model, Navigation.newUrl  (Client.Pages.to_path Client.Pages.AddClass)
        | _ ->
            Browser.console.warn ("Create school failed")
            { model with create_school_result = Some result }, Cmd.none
    | SubmissionFailure err ->
        Browser.console.info ("Failed to create school: " + err.Message)
        model, Cmd.none
    | SignOutMsg msg ->
        let cmd' = SignOut.update msg
        model, Cmd.map SignOutMsg cmd'

let on_school_change dispatch =
    fun (ev : React.FormEvent) ->
        let sn = ev.Value
        dispatch (SetSchoolName sn)

let on_teacher_change dispatch =
    fun (ev : React.FormEvent) ->
        let tn = ev.Value
        dispatch (SetTeacherName tn)

let on_submit dispatch =
    dispatch Submit

let input_field field_name description on_change =
    Field.div [ ] [
        Label.label [ ] [
            words 20 field_name
        ]
        Control.div [ ] [
            Input.text [
                Input.Size IsLarge
                Input.Color IsPrimary
                Input.Placeholder description
                Input.OnChange on_change
            ]
        ]
    ]

let view model (dispatch : Msg -> unit) session =
    
    ///create a  single string from the list of strings in the result
    let of_create_school_result (code : CreateSchoolCode) (result : CreateSchoolResult) =
        List.fold2 (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc) "" result.code result.message

    ///display error messages based on the result code code and to the user
    let make_error_user_message (code: CreateSchoolCode) =
        match model.create_school_result with
        | Some result ->
            match List.contains code result.code with
            | true ->
                Help.help [
                    Help.Color IsDanger
                    Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
                ] [
                    str (of_create_school_result code result)
                ]
            | false -> nothing
        | _ ->  nothing

    Hero.hero [
        Hero.IsBold
        Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ]
        Hero.Color IsWhite
        Hero.IsFullHeight
    ] [
        Hero.head [ ] [
            client_header (SignOutMsg >> dispatch) session
        ]
        Hero.body [ ] [
            Container.container [
                Container.Modifiers [
                    Modifier.TextAlignment (Screen.All, TextAlignment.Centered)
                ]
            ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is4)
                    Column.Offset (Screen.All, Column.Is4)
                ] [
                    Heading.h3 [
                        Heading.Modifiers [ Modifier.TextColor IsGrey ]
                    ] [
                        str "Welcome"
                    ]
                    Box.box' [
                        Modifiers [
                            Modifier.TextAlignment (Screen.All, TextAlignment.Left)
                        ]
                    ] [
                        input_field "School Name" "Give your school a name" (on_school_change dispatch)
                        make_error_user_message CreateSchoolCode.SchoolNameInUse
                        input_field "Your Name" "Your name" (on_teacher_change dispatch)
                        Button.button [
                            Button.Color IsPrimary
                            Button.IsFullWidth
                            Button.CustomClass "is-large is-block"
                            Button.OnClick (fun _ -> on_submit dispatch)
                        ] [
                            str "Submit"
                        ]
                        make_error_user_message CreateSchoolCode.DatabaseError
                        make_error_user_message CreateSchoolCode.Unknown
                    ]
                ]
            ]
        ]
    ]
