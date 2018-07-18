module Client.Home

open Fulma
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props

open Style

let view () =
    Container.container [ Container.IsFluid ]
        [ Section.section [ ]
            [ Level.level [ ]
                [ Level.left [ ]
                    [ Level.item [ ]
                        [ Heading.h1 [ Heading.Is3
                                       Heading.Modifiers [ Modifier.TextColor IsBlack ] ]
                                       [ str "The New Kid" ] ] ]
                  Level.right [ ]
                    [ Level.item [ ]
                        [ Text.span [ Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is3)
                                                  Modifier.TextColor IsLink ] ] [ a [ Href "#how_it_works" ] [ str "How it Works" ] ] ]
                      Level.item [ ]
                        [ viewLink Pages.Login "Login" ] ] ] ]
          Section.section [ ]
              [ Hero.hero [ Hero.IsBold
                            Hero.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Container.container [ Container.IsFluid ]
                        [ Heading.h1 [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ]
                            [ str "Need to grow your classroom?" ]
                          Heading.h3 [ Heading.Modifiers [ Modifier.TextColor IsBlack ] ]
                            [ str "Add students to your class virtually" ] ] ] ] ]
