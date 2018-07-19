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

let viewLink page description =
  R.a [ Style [ Padding "0 20px" ]
        Href (Pages.toPath page)
        OnClick goToUrl]
      [ R.str description]

let centerStyle direction =
    Style [ Display "flex"
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

let client_header =
    Container.container [ Container.IsFluid ] [
        Section.section [ ] [
            Level.level [ ] [
                Level.left [ ] [
                    Level.item [ ] [
                        Heading.h1 [
                            Heading.Is3
                            Heading.Modifiers [
                                Modifier.TextColor IsBlack
                            ]
                        ] [ R.str "The New Kid" ]
                    ]
                ]
                Level.right [ ] [
                    Level.item [ ] [
                        Text.span [
                            Modifiers [
                                Modifier.TextSize (Screen.All, TextSize.Is3)
                                Modifier.TextColor IsLink
                            ]
                        ] [
                            R.a [
                                Href "#how_it_works"
                            ] [ R.str "How it Works" ]
                        ]
                    ]
                    Level.item [ ] [
                        viewLink Pages.Login "Login"
                    ]
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
