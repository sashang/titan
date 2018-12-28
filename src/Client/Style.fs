module Client.Style

open Fable.Helpers.React.Props
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open Elmish.Browser.Navigation
open Fulma
open Fulma.Extensions
module R = Fable.Helpers.React

let goToUrl (e: React.MouseEvent) =
    e.preventDefault()
    let href = !!e.target?href
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
        Button.Color IsPrimary
        Button.IsFullWidth
        Button.OnClick (fun _ -> (dispatch msg))
        Button.CustomClass "is-large is-block"
    ] [ R.str text ]

let client_header dispatch session =
      Section.section [ ] [
          Level.level [ ] [
              Level.left [ ] [
                  Level.item [ ] [
                      viewLink Pages.Home "The New Kid"
                  ]
              ]
              Level.right [ ] [
                  Level.item [ ] [
                      viewLink Pages.HowItWorks "How it Works"                      
                  ]
                  Level.item [ ] [
                      (match session with
                      | None -> viewLink Pages.Login "Sign In"
                      | Some session -> SignOut.view dispatch session)
                  ]
              ]
          ]
      ]

let onEnter msg dispatch =
    function
    | (ev:React.KeyboardEvent) when ev.keyCode = Keyboard.Codes.enter ->
        ev.preventDefault()
        dispatch msg
    | _ -> ()
    |> OnKeyDown
