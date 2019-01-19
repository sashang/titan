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

let words size message =
    R.span [ Style [ FontSize (size |> sprintf "%dpx") ] ] [ R.str message ]

let buttonLink cssClass onClick elements =
    R.a [ ClassName cssClass
          OnClick (fun _ -> onClick())
          OnTouchStart (fun _ -> onClick())
          Style [ Cursor "pointer" ] ] elements

let button dispatch msg text = 
    Button.button [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg))
    ] [ R.str text ]

 



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
let input_field_ro label text code error =
    [ Field.div [ ] 
        (List.append 
            [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ R.str label ] ]
              Control.div [ ]
                [ Input.text 
                    [ Input.Value text
                      Input.IsReadOnly true ]]]
              (make_help code error))]

let input_field label text on_change =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ R.str label ] ]
          Control.div [ ]
            [ Input.text 
                [ Input.Value text
                  Input.OnChange on_change ] ]] ]

let input_field_with_error (label: string) (text : string) on_change (code : APICode) (error : APIError) =
    [ Field.div [ ]
        (List.append 
            [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ R.str label ] ]
              Control.div [ ]
                [ Input.text 
                    [ Input.Value text
                      Input.OnChange on_change ] ] ]
              (make_help code error))]

let notification (code : APICode) (error : APIError) =
    let zipped = List.zip error.Codes error.Messages

    if List.contains code error.Codes then
        Notification.notification [ Notification.Color IsTitanError ]
          [ for (c,m) in zipped do yield (if c = code then R.str m else R.nothing) ]
    else
        R.nothing