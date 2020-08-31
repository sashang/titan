module ElmishBridgeModel

type LiveState =
    | On
    | Off

type Tutor = {
    email : string
    students : string list
}

type Student = {
    email : string
    tutor : string
}

type Titan = {
    email : string
}

type User =
    | Tutor
    | Student
    | Titan

type Model = User option

//messages from the client to the server
type ServerMsg =
    | TutorGoLive //the tutor has started streaming
    | TutorStopLive //tutor has stopped streaming
    | StudentRequestLiveState //student wants to know if tutor has started streaming
    | TutorLiveState of LiveState
    | ClientIs of User

//messages from the server to the client
type ClientMsg =
    | ClientTutorGoLive
    | ClientTutorStopLive
    | ClientStudentRequestLiveState

    //message to client requesting information about the client
    //sent on init of the server the client responds with info
    //about itself. 
    | ClientInitialize 

let endpoint = "/socket"
