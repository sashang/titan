///binding to js react-markdown
module ReactMarkdown

open Fable.Core
open Fable.Helpers.React
open Fable.Import.React
open Fable.Core.JsInterop
open Fable.Helpers.React.Props
open Fulma


type ReactMarkdownProps =
    | Source of string

let reactMarkdown (props : ReactMarkdownProps list) (elems : ReactElement list) : ReactElement =
    ofImport "default" "react-markdown" (keyValueList CaseRules.LowerFirst props) elems