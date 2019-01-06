module Home

open Fulma
open Fable.Helpers.React
open CustomColours

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
                        [ Text.div [ Common.Props [ Props.Style [ Props.CSSProp.FontFamily "'Montserrat', sans-serif" ] ] ] [ str "and rediscover motivation." ] ] ]
              Column.column [] 
                [ Image.image 
                    [ Image.IsSquare ] 
                    [ img [ Props.Src "Images/teacher.png" ] ] ] ] ]
let view =
    [ Section.section [ Section.Modifiers [ ] ]
        [ first_impression ]
      Section.section [ Section.Modifiers [ Modifier.BackgroundColor IsTitanSecondary ] ] [ ] ]
