module SendGridAPI

open FSharp.Control.Tasks.ContextInsensitive
open SendGrid
open SendGrid.Helpers.Mail
open System.Net.Mail
open System


let send_email_via_sendgrid (apiKey : string) (destination : string) (name : string) (topic : string) (msg : string) = task {
    let client = SendGridClient(apiKey)
    let from = EmailAddress("sashang@tewtin.com", "Sashan Govender")
    let dest = EmailAddress(destination, name)
    let email = MailHelper.CreateSingleEmail(from, dest, topic, msg, msg);
    return! client.SendEmailAsync(email)
}

type ISendGridAPI =
    abstract member get_key : string
    abstract member send_email : string -> string -> string -> string -> unit

type SendGridAPI(key : string) =
    member this.key = key

    interface ISendGridAPI with
        member this.get_key = this.key

        member this.send_email (email : string) (name : string) (topic : string) (msg : string) =
            let server = "smtp-relay.gmail.com" // ConfigurationManager.AppSettings.["mailserver"]
            let sender = "sashang@tewtin.com" // ConfigurationManager.AppSettings.["mailsender"]
            let password = "1TrM3LiX*87D" // ConfigurationManager.AppSettings.["mailpassword"] |> my-decrypt
            let port = 587
            let mail_message = new MailMessage(sender, email, topic, "Hi " + name + ", <br/><br/>\r\n\r\n" + msg)
            mail_message.IsBodyHtml <- true
            let client = new SmtpClient(server, port)
            client.EnableSsl <- true
            client.Credentials <- System.Net.NetworkCredential(sender, password)
            let observer (e : ComponentModel.AsyncCompletedEventArgs) = 
                let msg = e.UserState :?> MailMessage
                if e.Cancelled then
                    ("Mail message cancelled:\r\n" + msg.Subject) |> Console.WriteLine
                if not (isNull e.Error) then
                    ("Sending mail failed for message:\r\n" + msg.Subject + 
                        ", reason:\r\n" + e.Error.ToString()) |> Console.WriteLine
                if msg<>Unchecked.defaultof<MailMessage> then msg.Dispose()
                if client<>Unchecked.defaultof<SmtpClient> then client.Dispose()
            client.SendCompleted |> Observable.add(observer)
        (*     client.SendCompleted |> Observable.add(fun e -> 
                let msg = e.UserState :?> MailMessage
                if e.Cancelled then
                    ("Mail message cancelled:\r\n" + msg.Subject) |> Console.WriteLine
                if not (isNull e.Error) then
                    ("Sending mail failed for message:\r\n" + msg.Subject + 
                        ", reason:\r\n" + e.Error.ToString()) |> Console.WriteLine
                if msg<>Unchecked.defaultof<MailMessage> then msg.Dispose()
                if client<>Unchecked.defaultof<SmtpClient> then client.Dispose()
            ) *)
            // Maybe some System.Threading.Thread.Sleep to prevent mail-server hammering
            client.SendAsync(mail_message, mail_message)