module CustomColours

open Fulma
// By using inline the caller will be replaced directly by the function body
let inline IsTitanPrimary<'a> = IsCustomColor "custom-titan-primary"
let inline IsTitanSecondary<'a> = IsCustomColor "custom-titan-secondary"
let inline IsTitanInfo<'a> = IsCustomColor "custom-titan-info"
let inline IsTitanInfo1<'a> = IsCustomColor "custom-titan-info1"
let inline IsTitanInfo2<'a> = IsCustomColor "custom-titan-info2"
let inline IsTitanSuccess<'a> = IsCustomColor "custom-titan-success"
let inline IsTitanError<'a> = IsCustomColor "custom-titan-error"