module AzureMaps

open Domain


type IAzureMaps =

    abstract member get_azure_maps_keys :  Domain.AzureMapsKeys

type AzureMaps(client_id:string, pkey:string) =
    member this.client_id = client_id
    member this.pkey = pkey

    interface IAzureMaps with
        member this.get_azure_maps_keys =
            {ClientId = this.client_id; PKey = this.pkey}