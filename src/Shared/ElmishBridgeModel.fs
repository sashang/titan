module ElmishBridgeModel

//messages from the client to the server
type ServerMsg =
    | TutorGoLive //the tutor has started streaming
    | TutorStopLive //tutor has stopped streaming

//messages from the server to the client
type ClientMsg =
    | ClientTutorGoLive
    | ClientTutorStopLive
    | TestMessage

type Model =
    | User of string

let endpoint = "/socket"
