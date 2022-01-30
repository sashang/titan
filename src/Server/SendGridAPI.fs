module SendGridAPI

open FSharp.Control.Tasks
open SendGrid
open SendGrid.Helpers.Mail
open System
open System.Collections.Generic


type ISendGridAPI =
    abstract member welcome_student : string -> string -> Threading.Tasks.Task<Response>

type SendGridAPI(key : string) =
    member this.key = key

    interface ISendGridAPI with

        member this.welcome_student (destination : string) (name : string) = task {
            let client = SendGridClient(this.key)
            let from = EmailAddress("sashang@tewtin.com", "Sashan Govender")
            let dest = EmailAddress(destination, name)
            let template_id = "d-a8721564f3bd4021a34aedfe2dd3b098" //this value is read from the sendgrid admin interface
            //let template_data = """{"from":{"email":"sashan@tewtin.com"},"personalizations":[{"to":[{"email":"sashang@gmail.com"}],"dynamic_template_data":{"name":"Sashan"}}],"template_id":"d-a8721564f3bd4021a34aedfe2dd3b098"}"""
            let template_data = new Dictionary<string,string>()
            template_data.Add("name", name)
            let email = MailHelper.CreateSingleTemplateEmail(from, dest, template_id, template_data)
            return! client.SendEmailAsync(email)
        }
