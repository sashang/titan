module FileSystemDatabase

open System.IO
open Domain
open Newtonsoft.Json

let load_schools =
    let fi = FileInfo("./json/schools.json")
    File.ReadAllText(fi.FullName)
    |> FableJson.from_json<Schools>

let add_user username email password =
    if File.Exists("./json/login.json") then
        let fi = FileInfo("./json/login.json")
        let text = File.ReadAllText(fi.FullName)
        let login_list = FableJson.from_json<SignUp list> text
        if List.exists (fun x -> x.username = username) login_list then
            false
        else
            let new_list = List.append login_list [{SignUp.username = username; SignUp.password = password; SignUp.email = email}]
            let json = FableJson.to_json new_list
            File.WriteAllText("./json/login.json", json)
            true
    else
        let initial_user = {SignUp.username = username; SignUp.password = password; SignUp.email = email}
        let json = FableJson.to_json [initial_user]
        File.WriteAllText("./json/login.json", json)
        true

