/// I have no idea where these functions should go...
module Homeless

// record that maps to the contents of appsettings.json
type RecStartupOptions = {
    JWTSecret : string 
    JWTIssuer : string 
    ConnectionString : string
    GoogleClientId : string
    GoogleSecret : string
    OpenTokSecret : string
    AzureMapsClientId : string
    AzureMapsPrimaryKey : string
    OpenTokKey : int
    SendGridAPIKey : string
}
//make a list that breaks the original list into one that contains x elem list
//of the original value
let chunk x input =
    input
    |> List.fold
         (fun (count,result) value ->
            match result with
            | [[]] -> count+1,[[value]]
            | inner::[] ->
                match count, inner with
                | count, ys when count = x -> 0,[ys;[value]]
                | _, ys -> count+1,[List.append ys [value]] 
            | _ -> failwith "Invalid initial state. Should be (0,[[]])") (0,[[]])
    |> (fun (count, result) -> result) //only interested in the result which is a list of lists
    
    
