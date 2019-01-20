module SchoolView

open Giraffe.GiraffeViewEngine
open Giraffe.GiraffeViewEngine

 //Dapper uses reflection when reading the names here to match what
 //we used in the sql string. This means the parameter names in the
 //constructor below must match the names use in the sql string...       
type Model(Name:string, FirstName:string, LastName:string)=
    member this.SchoolName = Name
    member this.FirstName = FirstName
    member this.LastName = LastName
    
let navbar_end =
    div [ _class "navbar-end" ] [
        div [ _class "navbar-item" ] [
            button [ _class "button is-custom-titan-info"; _href "/index.html" ] [
                str "Login"
            ]
        ]
    ]
    
let navbar_brand =
    div [ _class "navbar-brand" ] [
        div [ _class "navbar-item" ] [
            h3 [ _class "subtitle is-3 has-text-white" ] [
                str "tewtin"
            ]
        ]
        div [ _class "navbar-item" ] [
            h3 [ _class "subtitle is-5 has-text-white" ] [
                str "putting tutors first"
            ]
        ]
    ]
let navbar =
    nav [ _class "navbar has-background-custom-titan-primary" ] [
        div [ _class "container" ] [
            navbar_brand
            navbar_end
        ]
    ]

let school_tile (s : Model) =
    div [_class "tile is-3"] [ //grid is 12 columns, is-3 means use 3 of them 
        str s.SchoolName
    ]

let private school_view_body (schools : Model list) =
    section [] [
        div [_class "container is-fullhd"] [
            div [_class "tile is-ancestor"] [
                yield! [ for s in schools do
                            yield school_tile s ] 
            ]
        ]
    ]
    
let view (schools : Model list) =

    html [] [
        head [] [
            title [] [ str "tewtin" ]
            meta [ _charset "utf-8"; _name "viewport"; _content "width=device-width, initial-scale=1" ] 
            link [_rel "stylesheet"; _href "https://use.fontawesome.com/releases/v5.6.3/css/all.css";
                  _integrity "sha384-UHRtZLI+pbxtHCWp1t77Bi1L4ZtiqrqD80Kn4Z8NTSRyMA2Fd33n5dQ8lWUE00s/"; _crossorigin "anonymous"]
            link [_rel "stylesheet"; _href "css/mystyles.css"] //must come berfore material design theme below otherwise it will override material design theme-->
            link [_rel "stylesheet"; _href "https://unpkg.com/bulmaswatch/materia/bulmaswatch.min.css"]
            link [_href "https://fonts.googleapis.com/css?family=Montserrat:500|Roboto"; _rel "stylesheet"]
        ]
        body [] [
            section [ _class "hero is-white has-text-centered" ] [
                div [ _class "hero-head" ] [
                    navbar
                ]
                div [ _class "hero-body" ] [
                    school_view_body schools
                ]
                div [ _class "hero-foot" ] [
                    div [ _class "footer has-background-custom-titan-primary" ] [
                        div [_class "container" ] []
                    ]
                ]
            ]
        ]
    ]
    
