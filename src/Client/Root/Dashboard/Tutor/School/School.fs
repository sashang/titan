module School

open Client.Shared
open Client.Style
open CustomColours
open Domain
open Elmish
open Fable.React
open Fable.Import
open Fable.React.Props
open Fetch
open Fulma
open ModifiedFableFetch
open Thoth.Json
type TF = Thoth.Fetch.Fetch

exception SaveEx of APIError
exception LoadSchoolEx of APIError
exception LoadUserEx of APIError

type AzureMapsSummary = 
    { Query : string
      QueryType : string
      QueryTime: int
      NumResults : int
      Offset : int
      TotalResults : int
      FuzzyLevel : int }

    static member decoder : Decoder<AzureMapsSummary> =
        Decode.object
            (fun get ->
                { Query = get.Required.Field "query" Decode.string
                  QueryType = get.Required.Field "queryType" Decode.string
                  QueryTime = get.Required.Field "queryTime" Decode.int
                  NumResults = get.Required.Field "numResults" Decode.int
                  Offset = get.Required.Field "offset" Decode.int
                  TotalResults = get.Required.Field "totalResults" Decode.int
                  FuzzyLevel = get.Required.Field "fuzzyLevel" Decode.int })

type AzureMapsAddress = 
    { MunicipalitySubdivision : string option
      Municipality : string
      CountrySecondarySubdivision : string option
      CountrySubdivision : string
      CountryCode : string
      Country : string
      CountryCodeISO3 : string
      FreeformAddress : string }

    static member decoder : Decoder<AzureMapsAddress> =
        Decode.object
            (fun get ->
                { MunicipalitySubdivision = get.Optional.Field "municipalitySubdivision" Decode.string
                  Municipality = get.Required.Field "municipality" Decode.string
                  CountrySecondarySubdivision = get.Optional.Field "countrySecondarySubdivision" Decode.string
                  CountrySubdivision = get.Required.Field "countrySubdivision" Decode.string
                  CountryCode = get.Required.Field  "countryCode" Decode.string
                  Country = get.Required.Field  "country" Decode.string
                  CountryCodeISO3 = get.Required.Field "countryCodeISO3" Decode.string
                  FreeformAddress = get.Required.Field "freeformAddress" Decode.string })
                  
type AzureMapsResult =
    { Type : string 
      Address : AzureMapsAddress }
    static member decoder : Decoder<AzureMapsResult> =
        Decode.object
            (fun get ->
                { Type = get.Required.Field "type" Decode.string
                  Address = get.Required.Field "address" AzureMapsAddress.decoder })


type AzureMapsResponse = 
    { Summary : AzureMapsSummary
      Results : AzureMapsResult list }

    static member decoder : Decoder<AzureMapsResponse> =
        Decode.object
            (fun get ->
                { Summary = get.Required.Field "summary" AzureMapsSummary.decoder
                  Results = get.Required.Field "results" (Decode.list AzureMapsResult.decoder) })

type MapsInfo =
    { Text : string
      Suggestions : string list }
    static member init =
        {Text = ""; Suggestions = []}

type Model =
    { SchoolName : string
      FirstName : string
      Subjects : string
      Location : MapsInfo
      LastName : string
      UserLoadState : LoadingState
      SchoolLoadState : LoadingState
      AzureMapsClientId : string
      AzureMapsPKey : string
      Info : string
      Error : APIError option}

type Msg =
    | SetSchoolName of string
    | SetLocation of string
    | SetFirstName of string
    | SetLastName of string
    | SetInfo of string
    | ClickSave
    | SaveSuccess of unit
    | Success of SchoolResponse
    | GetAzureMapsKeys of Domain.AzureMapsKeys
    | LoadUserSuccess of UserResponse
    | AMSearch of AzureMapsResponse
    | ClickSuggestion of string
    | Failure of exn

let private load_school () = promise {
    let request = make_get 
    let decoder = Decode.Auto.generateDecoder<SchoolResponse>()
    let! response = TF.tryFetchAs("/api/load-school", decoder, request)
    match response with
    | Ok result ->
        match result.Error with
        | None -> 
            return result
        | Some api_error ->
            return raise (LoadSchoolEx api_error)
    | Error e ->
        return failwith "no school"
}

let private get_azure_maps_keys () = promise {
    let request = make_get 
    let decoder = Decode.Auto.generateDecoder<Domain.AzureMapsKeys>()
    let! response = TF.tryFetchAs("/api/get-azure-maps-keys", decoder, request)
    match response with
    | Ok keys ->
        Browser.Dom.console.info("Got azure client id = " +  keys.ClientId);
        Browser.Dom.console.info("Got azure pkey = " +  keys.PKey);
        return keys
    | Error message ->
        return failwith "no azure maps keys"
}


let private azure_maps_search (sub_key, query) = promise {
    let request =
        [ RequestProperties.Method HttpMethod.GET ] 
    let decoder = AzureMapsResponse.decoder
    let country_set ="&countrySet=AU,NZ"
    let api_version ="&api-version=1.0"
    let type_ahead ="&typeahead=true"
    let fetch_url = "https://atlas.microsoft.com/search/address/json?subscription-key=" + sub_key + api_version + country_set + type_ahead + "&query=" + query
    //let fetch_url = "https://atlas.microsoft.com/search/address/json"
    let! response = TF.tryFetchAs(fetch_url, decoder, request)
    match response with
    | Ok keys ->
        Browser.Dom.console.info("Got azure maps response");
        Browser.Dom.console.info("num_results: " + keys.Summary.NumResults.ToString());
        return keys
    | Error message ->
        Browser.Dom.console.error("Failed azure maps response: "+ message);
        return failwith message
}

let private load_user () = promise {
    let request = make_get 
    let decoder = Decode.Auto.generateDecoder<UserResponse>()
    let! response = TF.tryFetchAs("/api/load-user", decoder, request)
    match response with
    | Ok result ->
        match result.Error with
        | None -> 
            return result
        | Some api_error ->
            return raise (LoadUserEx api_error)
    | Error e ->
        return failwith "no user details"
}

let private save (data : SaveRequest) = promise {
    let request = make_post 6 data
    let decoder = Decode.Auto.generateDecoder<APIError option>()
    let! response = TF.tryFetchAs("/api/save-tutor", decoder, request)
    match response with
    | Ok result ->
        Browser.Dom.console.info("Saved successful")
    | Error e ->
        Browser.Dom.console.warn("Failed to save")

    return map_api_error_result response SaveEx
}

let init () : Model*Cmd<Msg> =
    {SchoolName = ""; Error = None; Subjects = ""; UserLoadState = Loading; SchoolLoadState = Loading;
     FirstName = ""; LastName = ""; Info = ""; Location = MapsInfo.init; AzureMapsClientId = ""; AzureMapsPKey = ""},
     Cmd.batch [Cmd.OfPromise.either load_school () Success Failure
                Cmd.OfPromise.either load_user () LoadUserSuccess Failure
                Cmd.OfPromise.either get_azure_maps_keys () GetAzureMapsKeys Failure]


let private of_api_error (result : APIError) =
    List.reduce (fun acc the_message -> acc + " " + the_message) result.Messages
        
let private of_load_school_result (code : APICode) (result : APIError) =
    List.fold2
        (fun acc the_code the_message -> if code = the_code then acc + " " + the_message else acc)
        "" result.Codes result.Messages

let private std_label text = 
    Label.label 
        [ Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
        [ str text ]

let private make_error (result : APIError option) =
    match result with
    | Some error ->
        Browser.Dom.console.error(sprintf "Making error message: %s" (of_api_error error))
        Message.message [ Message.Color IsTitanError
                          Message.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [
            Message.header [ ] [
                str "Error"
            ]
            Message.body [   ] [ 
                Help.help [
                    Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is5) ]
                ] [
                    str (of_api_error error)
                ]
            ]
        ]
    | _ ->  nothing

let private help_first_time_user (result : LoadSchoolResult option) =
    match result with
    | Some result ->
        match List.contains APICode.NoSchool result.Error.Codes with
        | true ->
            Help.help
                [ Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is6) 
                                   Modifier.TextAlignment (Screen.All, TextAlignment.Left)] ]
                [ str "Enter your name." ]
        | false -> std_label "Name"
    | _ -> std_label "Name"

let private school_name_help_first_time_user (result : LoadSchoolResult option) =
    match result with
    | Some result ->
        match List.contains APICode.NoSchool result.Error.Codes with
        | true ->
            Help.help
                [ Help.Modifiers [ Modifier.TextSize (Screen.All, TextSize.Is6)
                                   Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                [ str "Enter the name of your school." ]
        | false -> std_label "School Name"
    | _ -> std_label "School Name"

let private account_level =
    Level.level [ ] 
        [ Level.left [ ]
            [ Level.title [ Common.Modifiers [ Modifier.TextTransform TextTransform.UpperCase
                                               Modifier.TextSize (Screen.All, TextSize.Is5) ]
                            Common.Props [ Style [ CSSProp.FontFamily "'Montserrat', sans-serif" ]] ] [ str "Account" ] ] ]
    
let private image_holder url =
    [ Image.image [ Image.Is128x128 ]
        [ img [ Src url ] ] ]


let input_field_location (error : APIError option) (code : APICode) (label: string)
    (text : string) on_change  =
    [ Field.div [ ]
        (List.append 
            [ Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ]
                [ Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str label ] ]
              Control.div [ ]
                [ Input.text 
                    [ Input.Value text
                      Input.OnChange on_change ] ] ]
              (match error with
              |Some e -> Client.Style.make_help code e
              |None -> [] ))]

let input_autosuggest model dispatch =
    Field.div [ ] [
        Field.label [ Field.Label.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left) ] ] [
                      Field.p [ Field.Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str "Location" ] ]
        Control.div [ ] [
                Input.text [ Input.Value model.Location.Text; Input.OnChange (fun ev -> dispatch (SetLocation ev.Value))]
        ]
        Dropdown.dropdown [ Dropdown.IsActive (model.Location.Suggestions.IsEmpty = false)
                            Dropdown.Props [ Style [ Display DisplayOptions.Block ] ] ] [
            Dropdown.menu [ ] [
                Dropdown.content [ ] [
                    for suggestion in model.Location.Suggestions do
                        yield Dropdown.Item.a [ Dropdown.Item.Props  [ OnClick (fun ev -> dispatch (ClickSuggestion suggestion)) ] ] [
                                str suggestion 
                              ]
                ]
            ]
        ]
    ]


let school_content (model : Model) (dispatch : Msg->unit) = 
    [ Columns.columns [ ]
        [ Column.column [ ]
            [ yield! input_field model.Error APICode.FirstName "First Name" model.FirstName (fun e -> dispatch (SetFirstName e.Value))
              yield! input_field model.Error APICode.LastName "Last Name" model.LastName (fun e -> dispatch (SetLastName e.Value)) ]
          Column.column []
            [ yield! input_field model.Error APICode.SchoolName "School Name" model.SchoolName (fun e -> dispatch (SetSchoolName e.Value))
              yield input_autosuggest model dispatch ]]
              //yield! input_field_location model.Error APICode.Location "City or Suburb" model.Location (fun e -> dispatch (SetLocation e.Value)) ] ]
      Columns.columns [ ]
        [ Column.column []
            [ yield text_area_without_error "Info" model.Info (fun e -> dispatch (SetInfo e.Value)) ] ] ]


let private save_button dispatch msg text =
    Button.button [
        Button.Color IsTitanInfo
        Button.OnClick (fun _ -> (dispatch msg))
    ] [ str text ]

let private go_live dispatch msg text =
    Button.button [
        Button.Color IsTitanSuccess
        Button.OnClick (fun _ -> (dispatch msg))
    ] [ str text ]


let view (model : Model) (dispatch : Msg -> unit) = 
    match model.SchoolLoadState, model.UserLoadState with
    | Loaded, Loaded ->
        div [ ] [
            account_level
            Card.card [ ] [
                Card.header [ ] [
                    Card.Header.title [ ] [ ]
                ]
                Card.content [ ]
                    [ yield! school_content model dispatch ] 
                Card.footer [ ] [
                    Level.level [] [
                        Level.left [ ] [
                            Level.item [] [
                                Card.Footer.div [ ] [
                                    save_button dispatch ClickSave "Save"
                                ]
                           ]
                        ]
                    ]
               ]
            ]
            make_error model.Error 
        ]
    | _, _ ->  Client.Style.loading_view

let update  (model : Model) (msg : Msg): Model*Cmd<Msg> =
    match msg with
    | ClickSave ->
        Browser.Dom.console.info("clicked save")
        let save_request = {SaveRequest.init with FirstName = model.FirstName
                                                  LastName = model.LastName; Info = model.Info; Subjects = model.Subjects
                                                  SchoolName = model.SchoolName; Location = model.Location.Text}
        model, Cmd.OfPromise.either save save_request SaveSuccess Failure
    | SaveSuccess () ->
        model, Cmd.none
    | SetFirstName name ->
        {model with FirstName = name}, Cmd.none
    | SetLocation location ->
        let cmd =
            if location.Length >=3 then
               Cmd.OfPromise.either azure_maps_search (model.AzureMapsPKey, model.Location.Text) AMSearch Failure
            else
                Cmd.none
        {model with Location = {model.Location with Text = location}}, cmd
    | SetLastName name ->
        {model with LastName = name}, Cmd.none
    | SetInfo info ->
        {model with Info = info}, Cmd.none
    | SetSchoolName name ->
        {model with SchoolName = name}, Cmd.none
    | Success result ->
        {model with SchoolLoadState = Loaded; SchoolName = result.SchoolName; Info = result.Info;
                    Subjects = result.Subjects; Location = {model.Location with Text = result.Location}}, Cmd.none
    | LoadUserSuccess result ->
        {model with UserLoadState = Loaded; FirstName = result.FirstName; LastName = result.LastName}, Cmd.none
    | GetAzureMapsKeys keys ->
        {model with AzureMapsClientId = keys.ClientId; AzureMapsPKey = keys.PKey}, Cmd.none
    | AMSearch am_response ->
        Browser.Dom.console.info("Got " + am_response.Summary.NumResults.ToString() + " results")
        try
            let get_suggestions =
                am_response.Results
                |> List.filter (fun (result : AzureMapsResult) -> result.Type = "Geography")
                |> List.map (fun x -> x.Address.FreeformAddress)

            {model with Location = {model.Location with Suggestions = get_suggestions}}, Cmd.none
        with
        | e ->
            Browser.Dom.console.info("No 'Geography' type in results")
            model, Cmd.none

    | ClickSuggestion suggestion ->
        //replace the text with the suggestion clicked and empty the list of suggestions, becuase once
        //the user has chosen the item they want we don't need to display the other options.
        {model with Location = {model.Location with Text = suggestion; Suggestions = [] }}, Cmd.none

    | Failure e ->
        match e with
        | :? SaveEx as ex ->
            Browser.Dom.console.warn("Failed to save")
            { model with Error = Some ex.Data0 }, Cmd.none
        | :? LoadUserEx as ex ->
            { model with Error = Some ex.Data0 }, Cmd.none
        | :? LoadSchoolEx as ex ->
            { model with Error = Some ex.Data0 }, Cmd.none
        | e ->
            { model with Error = Some { Codes = [APICode.Failure]; Messages = ["Unknown errror"] }}, Cmd.none