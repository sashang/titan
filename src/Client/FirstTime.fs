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
                [ Modal.Card.title [ ] [ FHR.str "Choose your character" ] ]
              Modal.Card.body [ ]
                //The Checkradio.Name property is used to identify radio buttons in the same 
                //group.
                [ Checkradio.radio [ Checkradio.Name "character" ] [ str "Pupil" ]
                  Checkradio.radio [ Checkradio.Name "character" ] [ str "Teacher" ] ]
              Modal.Card.foot [ ]
                [ Button.button
                    [ Button.Color IsSuccess ]
                    [ FHR.str "Continue" ] ] ] ]