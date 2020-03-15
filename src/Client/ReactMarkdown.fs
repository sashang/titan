///binding to js react-markdown
module ReactMarkdown

open Fable.Core
open Fable.React
open Fable.Core.JsInterop


type ReactMarkdownProps =
    | Source of string

let reactMarkdown (props : ReactMarkdownProps list) (elems : ReactElement list) : ReactElement =
    ofImport "default" "react-markdown" (keyValueList CaseRules.LowerFirst props) elems