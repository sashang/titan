/// I have no idea where these functions should go...
module Homeless

//make a list that breaks the original list into one that contains 4 elem list
//of the original value
let list_x x input =
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
