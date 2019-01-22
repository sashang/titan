module Student.Dashboard

open Domain
open Elmish
open Elmish.Browser
open Elmish.Browser.Navigation
open Elmish.React
open Fable.Helpers.React.Props
open Fulma
open Fable.Helpers.React


type Model =
    { FirstName : string
      LastName : string
      Phone : string
      Email : string }

type Msg = Submit

let init = {FirstName = ""; LastName = ""; Phone = ""; Email =""}, Cmd.none

let update model msg = model, Cmd.none

let view model dispatch =
    str "nothing here"
