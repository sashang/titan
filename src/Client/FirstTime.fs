module Client.FirstTime

open Fulma
open Fulma.Extensions
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props

module FHR = Fable.Helpers.React
module FHRP = Fable.Helpers.React.Props

[<RequireQualifiedAccess>]
type Model =
    | Tutor
    | Student
    | None

type Msg =
    | ClickContinue
    | ClickBackground

let view model dispatch =
    Modal.modal [ Modal.IsActive true ]
        [ Modal.background [ Props [ OnClick (fun _ -> (dispatch ClickBackground) ) ] ] [ ]
          Modal.Card.card [ ]
            [ Modal.Card.head [ ]
                [ Modal.Card.title [ ]
                    [ FHR.str "First time" ] ]
              Modal.Card.body [ ]
                [ FHR.str "something" ]
              Modal.Card.foot [ ]
                [ Button.button
                    [ Button.Color IsSuccess ]
                    [ FHR.str "Continue" ] ] ] ]
(*     Hero.hero
        [ Hero.Color IsSuccess
          Hero.IsFullHeight ]
        [ Hero.body []
            [ Container.container
                [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                [ Heading.h3
                    [ Heading.Modifiers [ Modifier.TextColor IsGrey ] ]
                    [ str "First time?" ] ] ] ] *)
