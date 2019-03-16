module TitanOpenTokBinding

open Fable.Core
open Fable.Helpers.React
open Fable.Import.React
open Fable.Core.JsInterop

type TutorPublisherProps =
    | ApiKey of string
    | Token of string
    | Session of string
    | TutorEmail of string

let TutorPublisher (props : TutorPublisherProps list) (elems : ReactElement list) : ReactElement =
    ofImport "default" "./TutorPublisher.js" (keyValueList CaseRules.LowerFirst props) elems

type TutorSubscriberProps =
    | ApiKey of string
    | Token of string
    | Session of string

let TutorSubscriber (props : TutorSubscriberProps list) (elems : ReactElement list) : ReactElement =
    ofImport "default" "./TutorSubscriber.js" (keyValueList CaseRules.LowerFirst props) elems
