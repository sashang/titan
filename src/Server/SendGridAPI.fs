module SendGridAPI


type ISendGridAPI =
    abstract member getKey : string

type SendGridAPI(key : string) =
    member this.key = key

    interface ISendGridAPI with
        member this.getKey = this.key