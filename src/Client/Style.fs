module Client.Style

open Domain
open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open Elmish.Browser.Navigation
open Fulma
open Fulma.Extensions
module R = Fable.Helpers.React
open CustomColours

let goToUrl (e: React.MouseEvent) =
    e.preventDefault()
    let href = !!e.target?href
    Browser.console.info ("href = " + href)
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore

(*for documentation about these react properties and styles see for example
https://facebook.github.io/react-native/docs/layout-props#flexdirection*)
let viewLink page description =
    R.a [
        Style [
            CSSProp.Padding "0 20px"
            CSSProp.TextDecorationLine "underline"
            CSSProp.FontSize 25
        ]
        Href (Pages.to_path page)
        OnClick goToUrl
    ] [ R.str description]


let centerStyle direction =
    Style [ CSSProp.Display "flex"
            FlexDirection direction
            AlignItems "center"
            JustifyContent "center"
            Padding "20px 0"
    ]

let button dispatch msg text props = 
    Button.button (List.append [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg)) ] props)
        [ R.str text ]

let button_enabled dispatch msg text enable =
    button dispatch msg text [Button.Disabled (not enable)]

let onEnter msg dispatch =
    function
    | (ev:React.KeyboardEvent) when ev.keyCode = Keyboard.Codes.enter ->
        ev.preventDefault()
        dispatch msg
    | _ -> ()
    |> OnKeyDown

let private help text = 
       Help.help [ Help.Color IsTitanError
                   Help.Modifiers 
                        [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                        [ R.str text ]
let private make_help (code : APICode) (error : APIError) = 
    List.fold2 (fun acc the_code msg ->
         if code = the_code then List.append acc [(help msg)] else acc)
         [] error.Codes error.Messages

//a read only input field
let input_field_ro label text =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ R.str label ] ]
          Control.div [ ]
            [ Input.text 
                [ Input.Value text
                  Input.IsReadOnly true ]]]]

                    
let input_field_without_error label text on_change =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ R.str label ] ]
          Control.div [ ]
            [ Input.text 
                [ Input.Value text
                  Input.OnChange on_change ] ]] ]

let input_field_with_error (error : APIError) (code : APICode) (label: string) (text : string) on_change  =
    [ Field.div [ ]
        (List.append 
            [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ R.str label ] ]
              Control.div [ ]
                [ Input.text 
                    [ Input.Value text
                      Input.OnChange on_change ] ] ]
              (make_help code error))]

let input_field (error : APIError option) (code : APICode) =
    match error with
    | Some error -> input_field_with_error error code
    | _ -> input_field_without_error
        

let notification (code : APICode) (error : APIError option) =
    
    match error with
    | Some e ->
        if List.contains code e.Codes then
            let zipped = List.zip e.Codes e.Messages
            Notification.notification [ Notification.Color IsTitanError ]
              [ for (c,m) in zipped do yield (if c = code then R.str m else R.nothing) ]
        else
            R.nothing
    | _ -> R.nothing

let checkbox text ticked dispatch msg = 
    Field.div [ ]
        [ Control.div [ ]
            [ Checkbox.checkbox [  ]
                [ Checkbox.input [ Common.Props [ 
                    Checked ticked
                    OnChange (fun ev -> dispatch msg)] ]
                  R.str text ] ] ]