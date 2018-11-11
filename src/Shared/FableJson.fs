/// Functions to serialize and deserialize JSON, with client side support.
module FableJson

open Newtonsoft.Json

// The Fable.JsonConverter serializes F# types so they can be deserialized on the
// client side by Fable into full type instances, see http://fable.io/blog/Introducing-0-7.html#JSON-Serialization
// The converter includes a cache to improve serialization performance. Because of this,
// it's better to keep a single instance during the server lifetime.
let private jsonConverter = Fable.JsonConverter() :> JsonConverter
let to_json value =
    JsonConvert.SerializeObject(value, [|jsonConverter|])
let from_json<'a> (json:string) : 'a =
    JsonConvert.DeserializeObject<'a>(json, [|jsonConverter|])