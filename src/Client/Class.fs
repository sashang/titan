/// Represents a class that the pupils attend via a remote connection
module Class

open System

/// Information about the class
[<RequireQualifiedAccess>]
type Info = {
    date : DateTimeOffset
    pupils : Pupil.Info list
}
