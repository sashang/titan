module Maybe

type MaybeBuilder() =
    member __.Bind(x, f) =
        match x with
        | Some(x) -> f(x)
        | _ -> None
    member __.Return(x) =
        Some x
    member __.Zero(x) =
        Some x

let maybe = MaybeBuilder()