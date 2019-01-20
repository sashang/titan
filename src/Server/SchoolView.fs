module SchoolView

open Giraffe.GiraffeViewEngine
open Giraffe.GiraffeViewEngine

let view =

    html [] [
        head [] [
            title [] [ str "Giraffe Sample" ]
        ]
        body [] [
            h1 [] [ str "I |> F#" ]
            p [ _class "some-css-class"; _id "someId" ] [
                str "Hello World"
            ]
        ]
    ]
