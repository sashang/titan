module Home

open Domain
open Elmish
open Elmish.Browser.Navigation
open Fulma
open Fable.Helpers.React
open CustomColours
open ValueDeclarations

type Msg =
    | SetEmail of string

type Model =
    { Email : string 
      BetaRegisrationResult : BetaRegistrationResult option}
//what the user sees 1st time
let first_impression =
    Container.container [ Container.IsFullHD ] 
        [ Columns.columns [ ] 
            [ Column.column []  
                  [ Heading.h1 
                        [ Heading.Modifiers [ Modifier.TextColor IsBlack
                                              Modifier.TextTransform TextTransform.Capitalized
                                              Modifier.TextWeight TextWeight.Bold
                                              Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] 
                        [ Text.div [ ] [ str "Connect" ] ]
                    Heading.h3 
                        [ Heading.Modifiers [ Modifier.TextColor IsGrey
                                              Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                        [ Text.div [ Common.Props [ Props.Style [ Props.CSSProp.FontFamily "'Montserrat', sans-serif" ] ] ] [ str "and rediscover teaching." ] ] ]
              Column.column [] 
                [ Image.image 
                    [ Image.IsSquare ] 
                    [ img [ Props.Src "Images/teacher.png" ] ] ] ] ]

let target_audience =
    Container.container 
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
        [ Columns.columns [ ]
            [ Column.column [ ]
                    [ Box.box' [ Common.Props [ Props.Style [ Props.CSSProp.Height "100%" ] ] ]
                        [ Heading.h3 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] 
                            [ str "For Tutors" ]
                          Text.div 
                            [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                            [ str "Powerful tools to manage your students, schedule classes, and live stream lessons" ] ] ]
              Column.column [ ]
                    [ Box.box' [ Common.Props [ Props.Style [ Props.CSSProp.Height "100%" ] ] ]
                        [ Heading.h3 [ Heading.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ]
                            [ str "For Students" ]
                          Text.div 
                            [ Common.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                            [ str "Connect with tutors who know your curriculum." ] ] ] ] ]

let testimonials =
    Container.container 
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
        [ Heading.h1
            [ Heading.Modifiers [ Modifier.TextColor IsBlack
                                  Modifier.TextWeight TextWeight.Bold ] ]
            [ str "Why tutors love us" ]
          Columns.columns [ Columns.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
            [ Column.column [ Column.Width (Screen.All, Column.IsHalf) ]
                [ Box.box' [] 
                    [ Text.div 
                        [ Common.Modifiers
                            [ Modifier.TextTransform TextTransform.Italic
                              Modifier.TextSize (Screen.All, TextSize.Is4) ] ]
                        [ str (MAIN_NAME + " has allowed me to increase the number of students attending my class") ] ] ] ] ]

let beta_program model dispatch =
    Container.container
        [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
        [ Columns.columns [ Columns.IsCentered; Columns.IsGapless ]
            [ Column.column [ Column.Width (Screen.All, Column.Is6) ]
                [ Box.box' [ ]
                    [ Input.text
                        [ Input.Type Input.Email
                          Input.Placeholder "Email"
                          Input.Props [ Props.OnChange (fun ev -> dispatch (SetEmail ev.Value)) ] ] ] ]
              Column.column [ Column.Width (Screen.All, Column.Is3) ]
                [ Button.button 
                    [ Button.Props [ Props.Style [ Props.CSSProp.Margin "20px" ] ] ]
                    [ str "Register" ] ] ] ]
let footer = 
    Footer.footer [ Common.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
        [ Container.container [ ]
            [ ] ]

let init () =
    {Email = ""; BetaRegisrationResult = None }

let update  (model : Model) (msg : Msg): Model*Cmd<Msg> =
    match msg with
    | SetEmail email ->
        { model with Email = email}, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    [ Section.section 
        [ Section.Modifiers [ ] ]
        [ first_impression ]
      Section.section 
        [ Section.Modifiers [ Modifier.BackgroundColor IsTitanPrimary ] ]
        [ target_audience ]
      Section.section 
        [ Section.Modifiers [ ] ]
        [ beta_program model dispatch ] ]
