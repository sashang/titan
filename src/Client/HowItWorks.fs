module Client.HowItWorks

open Fulma
open Fulma.Extensions
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props

type Model = None

type Msg = None

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    model, Cmd.none

let view () =
    Hero.hero
        [ Hero.Color IsSuccess
          Hero.IsFullHeight ]
        [ Hero.body [ ]
            [ Container.container
                [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ Content.content [ ]
                    [ h1 [ ] [ str "How It Works" ] ] ] ] ]

