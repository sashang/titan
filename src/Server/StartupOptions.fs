module StartupOptions

type RecStartupOptions = {
    GoogleId : string option
    GoogleSecret : string option
    EnableTestUser : bool
    JWTSecret : string
    JWTIssuer : string
}


type IStartupOptions =
    abstract member JWTSecret : string with get
    abstract member JWTIssuer : string with get

type StartupOptions(options : RecStartupOptions) =
    member private this.Options = options

    interface IStartupOptions with

        member this.JWTSecret = this.Options.JWTSecret
        member this.JWTIssuer = this.Options.JWTIssuer