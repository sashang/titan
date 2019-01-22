module SchoolView

open Giraffe.GiraffeViewEngine

 
type Model =
    {SchoolName : string
     FirstName : string
     LastName : string}
     
    static member init first last school =
        {SchoolName = school; FirstName = first; LastName = last}
    
let navbar_end =
    div [ _class "navbar-end" ] [
        div [ _class "navbar-item" ] [
            button [ _class "button is-custom-titan-info"
                     _onclick "window.location.href='/#login'"] [
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
    div [_class "box" ] [
        div [_class "card-header has-background-custom-titan-secondary"] [
            p [ _class "card-header-title has-text-white" ] [ str s.SchoolName ]
        ]
        div [_class "card-content"] [
            div [_class "columns"] [
                div [_class "column is-2"] [
                    h3 [_class "subtitle" ] [ str "Tutor" ]
                ]
                div [_class "column"] [
                    h4 [ ] [str s.FirstName; str " "; str s.LastName ]
                ]
            ]
            div [_class "columns"] [
                div [_class "column is-2"] [
                    h3 [ _class "subtitle"] [ str "Location" ]
                ]
                div [_class "column is-2"] [
                    h4 [ ] [ ]
                ]
            ]
            div [_class "columns"] [
                div [_class "column is-2"] [
                    label [_class "subtitle" ] [ str "Description" ]
                ]
                div [_class "content"] [
                    p [ ] [ str "asdsadasd" ]
                ]
            ]
        ]
        div [ _class "card-footer" ] [
            button [ _class "button is-custom-titan-info is-small"; _onclick "window.location.href='/signin-google'" ] [
                str "Enrol"
            ]
        ]
    ]

let private single_row (schools : Model list) =
    [for s in schools do yield school_tile s]

let private school_view_body (schools : Model list) =
    let sublists = Seq.chunkBySize 4 schools
    div [_class "container"] [
        yield! [ for s in sublists do yield! single_row (List.ofArray s) ]
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
    
