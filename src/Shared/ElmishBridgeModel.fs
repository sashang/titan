module ElmishBridgeModel

type TestMessages =
    | Msg1 of string

type ServerMsg = unit
type ClientMsg = TheClientMsg of TestMessages

let endpoint = "/socket"
