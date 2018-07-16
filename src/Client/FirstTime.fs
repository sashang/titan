module Client.FirstTime

open Fulma
open Fulma.Extensions
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props

module FHR = Fable.Helpers.React
module FHRP = Fable.Helpers.React.Props

[<RequireQualifiedAccess>]
type Model = { pupil : bool; teacher : bool }

type Msg =
    | ClickContinue
    | ClickBackground
    | TogglePupil
    | ToggleTeacher

let update (msg : Msg) (model : Model) : Model*Cmd<Msg> =
    match msg with
    | ClickContinue->
        model, Cmd.none
    | ClickBackground ->
        model, Cmd.none
    | TogglePupil ->
        { pupil = true; teacher = false}, Cmd.none
    | ToggleTeacher ->
        { pupil = false; teacher = true}, Cmd.none
        
let view (model : Model) dispatch =
    Modal.modal [ Modal.IsActive true ]
        [ Modal.background [ Props [ OnClick (fun _ -> (dispatch ClickBackground) ) ] ] [ ]
          Modal.Card.card [ ]
            [ Modal.Card.head [ ]
                [ Modal.Card.title [ ] [ FHR.str "Choose your character" ] ]
              Modal.Card.body [ ]
                //The Checkradio.Name property is used to identify radio buttons in the same 
                //group.
                [ Checkradio.radio 
                    [ Checkradio.Name "character"
                      Checkradio.Checked model.pupil                       
                      Checkradio.OnChange (fun _ -> dispatch TogglePupil) ] [ str "Pupil" ]
                  Checkradio.radio 
                    [ Checkradio.Name "character"
                      Checkradio.Checked model.teacher 
                      Checkradio.OnChange (fun _ -> dispatch ToggleTeacher) ] [ str "Teacher" ] ]
              Modal.Card.foot [ ]
                [ Button.button
                    [ Button.Color IsSuccess
                      Button.OnClick (fun _ -> (dispatch ClickContinue)) ]
                    [ FHR.str "Continue" ] ] ] ]