module Client.Style

open Browser.Types
open CustomColours
open Domain
open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fable.Core.JsInterop
open Fable.Import
open Elmish.Navigation
open Fulma

let [<Literal>] ESC_KEY = 27.
let [<Literal>] ENTER_KEY = 13.

let goToUrl (e: MouseEvent) =
    e.preventDefault()
    let href = !!e.target?href
    Browser.Dom.console.info ("href = " + href)
    Navigation.newUrl href |> List.map (fun f -> f ignore) |> ignore

(*for documentation about these react properties and styles see for example
https://facebook.github.io/react-native/docs/layout-props#flexdirection*)
let viewLink page description =
    a [
        Style [
            CSSProp.Padding "0 20px"
            CSSProp.TextDecorationLine "underline"
            CSSProp.FontSize 25
        ]
        Href (Pages.to_path page)
        OnClick goToUrl
    ] [ str description]


let centerStyle direction =
    Style [ CSSProp.Display DisplayOptions.Flex
            FlexDirection direction
            AlignItems AlignItemsOptions.Center
            JustifyContent "center"
            Padding "20px 0"
    ]

let button dispatch msg text props = 
    Button.button (List.append [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg)) ] props)
        [ str text ]

let button_enabled dispatch msg text enable =
    button dispatch msg text [Button.Disabled (not enable)]

let loading_view = 
    div [ HTMLAttr.Class "lds-grid" ] [ 
        div [] []
        div [] []
        div [] []
        div [] []
        div [] []
        div [] []
        div [] []
        div [] []
        div [] []
    ]

let onEnter msg dispatch =
    function
    | (ev:KeyboardEvent) when ev.keyCode = ENTER_KEY ->
        ev.preventDefault()
        dispatch msg
    | _ -> ()
    |> OnKeyDown

let private help text = 
       Help.help [ Help.Color IsTitanError
                   Help.Modifiers 
                        [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                        [ str text ]
                        
let make_help (code : APICode) (error : APIError) = 
    List.fold2 (fun acc the_code msg ->
         if code = the_code then List.append acc [(help msg)] else acc)
         [] error.Codes error.Messages


let radio_buttons label yes_string no_string value dispatch msg =
    Field.div [ ] [
        Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str label ] ]
        Control.div [ ]

            [ Radio.radio [ ]
                [ Radio.input [ Radio.Input.Name label
                                Radio.Input.Props [ OnChange (fun ev -> dispatch msg)
                                                    Checked value
                                                    Disabled false ] ]
                  str yes_string ]
              Radio.radio [ ]
                [ Radio.input [ Radio.Input.Name label
                                Radio.Input.Props [ OnChange (fun ev -> dispatch msg)
                                                    Disabled false
                                                    Checked (not value) ] ]
                  str no_string ] ] 
    ]

//a read only input field
let input_field_ro label text =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str label ] ]
          Control.div [ ]
            [ Input.text 
                [ Input.Value text
                  Input.IsReadOnly true ]]]]

                    
let input_field_without_error label text on_change =
    [ Field.div [ ] 
        [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str label ] ]
          Control.div [ ]
            [ Input.text 
                [ Input.Value text
                  Input.OnChange on_change ] ]] ]

let input_field_with_error (error : APIError) (code : APICode) (label: string) (text : string) on_change  =
    [ Field.div [ ]
        (List.append 
            [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str label ] ]
              Control.div [ ]
                [ Input.text 
                    [ Input.Value text
                      Input.OnChange on_change ] ] ]
            (make_help code error))]

let input_field (error : APIError option) (code : APICode) =
    match error with
    | Some error -> input_field_with_error error code
    | _ -> input_field_without_error
    
let text_area_without_error (label: string) (text : string) on_change  =
    Field.div [Field.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left)] ] [
        Label.label [] [ str label ]
        Control.div [ ] [
            Textarea.textarea [ Textarea.Value text
                                Textarea.OnChange on_change ] [ ]
        ]
    ]


let notification (code : APICode) (error : APIError option) =
    
    match error with
    | Some e ->
        Browser.Dom.console.warn ("notification: " + List.head (e.Messages))
        if List.contains code e.Codes then
            let zipped = List.zip e.Codes e.Messages
            Notification.notification [ Notification.Color IsTitanError ] [
                yield! [for (c,m) in zipped do yield (if c = code then str m else nothing)]
            ]
            
        else
            nothing
    | _ -> nothing

let checkbox text ticked dispatch msg = 
    Field.div [ ]
        [ Control.div [ ]
            [ Checkbox.checkbox [  ]
                [ Checkbox.input [ Common.Props [ 
                    Checked ticked
                    OnChange (fun ev -> dispatch msg)] ]
                  str text ] ] ]