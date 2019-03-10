module OpenTokReactApp

open Fable.Core
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import.React
open Fable.Core.JsInterop

type Properties =
    | Name of string
    | Width of string
    | Height of string
    | PublishVideo of bool


module Streams =
    type OTStreamsProps =
        | Session of obj

    let streams (props : OTStreamsProps list) (elems : ReactElement list) : ReactElement =
        ofImport "OTStreams" "opentok-react" (keyValueList CaseRules.LowerFirst props) elems

module Publisher = 
    type Props =
        | OTProps of Properties list

    let publisher (props : Props list) (elems : ReactElement list) : ReactElement =
        ofImport "OTPublisher" "opentok-react" (keyValueList CaseRules.LowerFirst props) elems

    let inline Props (css: Properties list): Props =
        !!("properties", keyValueList CaseRules.LowerFirst css)

module Subscriber =
    type Props =
        | OTProps of Properties list
        | OnSubscribe of (unit -> unit)

    let subscriber (props : Props list) (elems : ReactElement list) : ReactElement =
        ofImport "OTSubscriber" "opentok-react" (keyValueList CaseRules.LowerFirst props) elems

    let inline OTProps (css: Properties list): Props =
        !!("properties", keyValueList CaseRules.LowerFirst css)

module Session = 
    type Props =
        | ApiKey of string
        | Token of string
        | SessionId of string
        | Height of string

    let session (props : Props list) (elems : ReactElement list) : ReactElement =
        ofImport "OTSession" "opentok-react" (keyValueList CaseRules.LowerFirst props) elems

module StudentSubscriber =
    type Props =
        | OTProps of Properties list
        | TutorEmail of string

    let comp (props : Props list) (elems : ReactElement list) : ReactElement =
        ofImport "default" "./StudentSubscriber.js" (keyValueList CaseRules.LowerFirst props) elems


