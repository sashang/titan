module FAQ

open Client.Shared
open CustomColours
open Domain
open Elmish
open Elmish.Browser.Navigation
open Fulma
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Thoth.Json
open ValueDeclarations


let view =
    Container.container [ Container.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ]
                          Container.IsFluid ] [ 
        Section.section [ Section.Modifiers [ ] ] [
            Columns.columns [] [
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo1 ] ] [ 
                        Heading.h2 [ ] [ str "Is this is a tutor student matchmaking service?" ] 
                    ]
                ]
                Column.column [ ] [
                    Heading.h2 [ ] [ ] 
                ]
            ]
            Columns.columns [] [
                Column.column [ ] [ ]
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo2 ]  ] [ 
                        Heading.h4 [ Heading.IsSubtitle ]
                            [ str "No, it's a platform for independent tutors to deliver lessons online and perform routine administration tasks.
                                   There are plenty of other platforms that match students and tutors. You can use this platform to find tutors and students
                                   but that's not its primary goal." ] 
                    ]
                ]
            ]
        ]
        Section.section [ Section.Modifiers [ ] ] [
            Columns.columns [] [
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo1 ]  ] [ 
                        Heading.h2 [ ] [ str "You expect me to pay for video conferencing when Skype is free?" ] 
                    ]
                ]
                Column.column [ ] [
                    Heading.h2 [ ] [ ] 
                ]
            ]
            Columns.columns [] [
                Column.column [ ] [ ]
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo2 ]  ] [ 
                        Heading.h4 [ Heading.IsSubtitle ] 
                            [ str "The tutors who will benefit the most from using this platform have a high volume of students. They don't want to be dialing in 3 or more students each
                                   hour for 5 hours. Tutor's with low volumes of students will not benefit much.
                                   However, if you want to scale to 100s of students then Tewtin will be able to deliver an interactive style broadcast.
                                   Imagine trying to manage 10 students with Skype in a single call?" ] 
                        Heading.h4 [ Heading.IsSubtitle ] 
                            [ str "Additionally Skype has problems with privacy. Your conversation over Skype is being listened
                                   to by 3rd parties and students targeted for advertising. We don't believe that
                                   education should be delivered this way. Parents are becoming more aware of the unexpected side effects
                                   of the advertising model that drives a lot of these free tools. They are concerned about the data footprint of their
                                   children, and seek to minimise this. Tewtin helps here because we do not advertise on behalf of
                                   3rd parties nor sell data to third parties." ] 
                    ]
                ]
            ]
        ]
        Section.section [ Section.Modifiers [ ] ] [
            Columns.columns [] [
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo1 ]  ] [ 
                        Heading.h2 [ ] [ str "What's the profile of your typical user?" ] 
                    ]
                ]
                Column.column [ ] [
                    Heading.h2 [ ] [ ] 
                ]
            ]
            Columns.columns [] [
                Column.column [ ] [ ]
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo2 ]  ] [ 
                        Heading.h4 [ Heading.IsSubtitle ] 
                            [ str "Our trial users are high volume independant tutors and their students. These tutors make tutoring a full time occupation and are tutoring 30 or more students a week.
                                   They run their classrooms out of their homes typically standing in front of a whiteboard, simulating a classroom environment." ] 
                    ]
                ]
            ]
        ]
        Section.section [ Section.Modifiers [ ] ] [
            Columns.columns [] [
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo1 ]  ] [ 
                        Heading.h2 [ ] [ str "Why high volume independent tutors?" ] 
                    ]
                ]
                Column.column [ ] [
                    Heading.h2 [ ] [ ] 
                ]
            ]
            Columns.columns [] [
                Column.column [ ] [ ]
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo2 ]  ] [ 
                        Heading.h4 [ Heading.IsSubtitle ] 
                            [ str "They are currently an underserved vertical market. There's nothing out there specifically
                                   designed to enable them take advantage of the scale that the internet can provide." ] 
                    ]
                ]
            ]
        ]
        Section.section [ Section.Modifiers [ ] ] [
            Columns.columns [] [
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo1 ]  ] [ 
                        Heading.h2 [ ] [ str "What's the business model?" ] 
                    ]
                ]
                Column.column [ ] [
                    Heading.h2 [ ] [ ] 
                ]
            ]
            Columns.columns [] [
                Column.column [ ] [ ]
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo2 ]  ] [ 
                        Heading.h4 [ Heading.IsSubtitle ] 
                            [ str "A monthly subscription model for the base service with pre-pay
                                   for video conferencing and other add on features. Pricing is yet to be determined." ] 
                    ]
                ]
            ]
        ]
        Section.section [ Section.Modifiers [ ] ] [
            Columns.columns [] [
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo1 ]  ] [ 
                        Heading.h2 [ ] [ str "Do you have a virtual whiteboard?" ] 
                    ]
                ]
                Column.column [ ] [
                    Heading.h2 [ ] [ ] 
                ]
            ]
            Columns.columns [] [
                Column.column [ ] [ ]
                Column.column [ ] [
                    Box.box' [ Common.Modifiers [ Modifier.BackgroundColor IsTitanInfo2 ]  ] [ 
                        Heading.h4 [ Heading.IsSubtitle ] 
                            [ str "No. Our typical user is already standing infront of a physical whiteboard.
                                   They aren't sitting at the computer delivering a lesson. We have plans to use
                                   augmented reality and AI to enhance this experience." ] 
                    ]
                ]
            ]
        ]
    ]
