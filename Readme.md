# Titan

## Requirements
The SAFE stack for F# web development.
https://safe-stack.github.io/

Install that using the link above. Then see the next section to build and run.

Then install the following from the root of the source code
```
yarn install node-sass --dev
yarn install bulma --dev
```

## Build

Ensure you have the dotnet sdk 2.1.500 installed. I've found that exporting the
following variables will ensure a headache free existance:

```
export DOTNET_ROOT=<path to dotnet root>
export PATH="<path to dotnet root>:$PATH"
```

Then cd into the directory you downloaded this source code and run:

```
fake build
```

To run it:
```
fake build --target run
```

### Note about .NET SDK and runtime versions

* There's no correlation between the runtime version number and the sdk version number
  For example 2.1.402 SDK uses the 2.1.4 runtime. 2.1.403 uses the 2.1.5 runtime. Confused yet?
  There's more....
* The version numbering isn't lexicographically ordered. After years of reading
  version numbers as strings I expected that 2.1.4 came after 2.1.300. This is not true.
  2.1.300 was released after 2.1.4.
* Check this link for reference https://dotnet.microsoft.com/download/dotnet-core/2.1

## Running

The command above should start both the client and the server and web browser
should open pointing to the client side port. If not point the browser at
`http://localhost:8080`. This is the client side interface, which is written
in F# and transpiled to Javascript. It's implemented as a single page app
which means that the Javascript for all the pages of this site is loaded when
the user's browser makes the first request. Subsequent pages don't need to
make requests to the server. Requests to the server are only made for specfic
data that resides on the database or for authentication with other
authentication providers like Google.

The server side code runs on port `http://localhost:8085`. This is all in F#
on top of ASP.NET Core.

## Development

### VSCode

It's easiest to use VSCode with the ionide plugin for fsharp installed. It
gives good code completion (better than the Vim plugin).

```
export DOTNET_ROOT=<path to dotnet root>
export PATH="<path to dotnet root>:$PATH"
code
```

If you start it without the DOTNET_ROOT path set to the path to the SDK in
use for development, it will find whatever SDK is on your system and it
searches from a set of common paths. This SDK may not be in the one you
intend to use, so best to start `code` with the right path set in the
DOTNET_ROOT environment variable.

### Debugging

It's worth installing Redux DevTools for Chrome as an extension. Then you can
view the messages in the Chrome debugger (hit F12).

### Environment Variables

#### Bash
```
export ASPNETCORE_ENVIRONMENT="Development"
```
#### Fish (friendly interactive shell)
```
set -x ASPNETCORE_ENVIRONMENT Development
```

#### Google authentication
This is all in a state of flux so may or not may not work. Set the following
environment variables:

##### Bash:
```
export TITAN_GOOGLE_ID="client id"
export TITAN_GOOGLE_SECRET="client secret"
```

##### Windows Powershell:
```
$Env:TITAN_GOOLGE_ID="client id"
$Env:TITAN_GOOGLE_SECRET="client secret"
```

The values for those variables you can get by creating an application for use
with Google+
(https://console.cloud.google.com/apis/library/plus.googleapis.com). If those
environment variables are not set the server will not start.

### Database

Replaced Postgres with Azure Sql Server. This works out cheaper per month.
Postgres's basic plan was something like $35 but with Azure Sql Server it's
$6. The 'downside' is it is cloud first, so that means I can't install it
locally which means I had to setup and Azure account, setup a firewall rule
to let traffic from my external ip through to it.

#### Tools
mssql. This gives you command line access to the database in azure.

```
mssql -s titan-sql-server.database.windows.net -u <username> -p <password> -d titan-dev -e
```
username and password are setup when you create the database in the Azure portal.

#### Connection string

Use this something like this. You'll need your own account in Azure.
```
Server=tcp:titan-sql-server.database.windows.net,1433;Initial Catalog=titan-dev;Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```



#### Migrations

Migrations are handled using Fluentmigrator.

* Listing migrations
```
mono ./packages/FluentMigrator.Console/net461/any/Migrate.exe -c
"Host=localhost;Database=titan_dev;Username=titan_dev;Password=1234"
--provider Postgres -a src/Server/bin/Debug/netcoreapp2.1/Server.dll
--task listmigrations
```
* Migrating down
```
mono ./packages/FluentMigrator.Console/net461/any/Migrate.exe -c
"Host=localhost;Database=titan_dev;Username=titan_dev;Password=1234"
--provider Postgres -a src/Server/bin/Debug/netcoreapp2.1/Server.dll
--task migrate:down
```
* Migrating up
```
mono ./packages/FluentMigrator.Console/net461/any/Migrate.exe -c
"Host=localhost;Database=titan_dev;Username=titan_dev;Password=1234"
--provider Postgres -a src/Server/bin/Debug/netcoreapp2.1/Server.dll
--task migrate:up
```

* Additionally for development you can run the development migration using the --profile option
```
mono ./packages/FluentMigrator.Console/net461/any/Migrate.exe -c
"Host=localhost;Database=titan_dev;Username=titan_dev;Password=1234"
--provider Postgres -a src/Server/bin/Debug/netcoreapp2.1/Server.dll
--task migrate:up --profile Development
```
This will run the migration in Migrations/Development.fs


### Using Paket

Paket is a package manager for .NET packages. It's used in this project to
add/remove .NET assmeblies that this project depends on.

#### Adding a dependency

```
.paket/paket.exe add Chiron --version 7.0.0-beta-180105 --project src/Server/Server.fsproj -g Server
```

#### Removing a dependency

```
.paket/paket.exe remove Chiron
```

Once added or removed the files `paket.dependencies`, `paket.lock` and
`src/Server/Server/paket.references` are modified. These changes need to be
committed.

### Paket/.NET weirdmess

Most of the problems I've had stemmed from the fucking toolchain. Paket will sometime
throw out weird bugs. This section lists them and work arounds.

#### Error in Paket.Restore.targets

An error inside a file that is autogenerated by toolchain itself. WTF?!!. Pure garbage.

```
Microsoft (R) Build Engine version 15.7.179.6572 for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  Paket version 5.181.1
  The last restore is still up to date. Nothing left to do.
  Performance:
   - Runtime: 47 milliseconds
/home/sashan/code/titan/.paket/Paket.Restore.targets(147,9): error MSB4184: The expression ""System.String[]".GetValue(5)" cannot be evaluated. Index was outside the bounds of the array. [/home/sashan/code/titan/src/Server/Server.fsproj]

Build FAILED.

/home/sashan/code/titan/.paket/Paket.Restore.targets(147,9): error MSB4184: The expression ""System.String[]".GetValue(5)" cannot be evaluated. Index was outside the bounds of the array. [/home/sashan/code/titan/src/Server/Server.fsproj]
    0 Warning(s)
    1 Error(s)

Time Elapsed 00:00:02.24
sashan@deimos titan/src/Server master 1☠  > rm -rf bin/ obj/
sashan@deimos titan/src/Server master ☺ >
```

To get around this one `rm -rf bin/ obj/` directories and run `dotnet build`
again.

#### Stackoverflow in paket.

Here's another bug that crashes paket with a stack overflow: https://github.com/sashang/paket-bug

Workaround is to remove the paket.lock file and run `paket install`.

#### Updating assemblies breaks your code

When learning how to use this, most examples refer to assemblies in
paket.dependencies without including the version numbers of the assembly to use.
Running `paket update` will update those assemblies which are not pinned to a
version. This has burnt me numerous times. An assembly is updated, and the one
in my cache is 6 months old. I run `paket update` and it breaks my code because
it pulls in the latest version of that assembly. I've started pinning all
assemblies to the version number so I don't have to deal with this problem.


## The client

### Parent - Child relationships

React programs follow a parent child relationship. Components receive data in
one direction only (Reacts one way binding:
https://reactjs.org/docs/thinking-in-react.html). Similarly Elmish pushes
messages down from the main program. Messages triggered by a react element in
a child component is routed through the top level Elmish runtime first, then
the main top level component of the client (Root in this case) before making
it's way down to the component that triggered the message. Along the way down
this chain you (probably shouldn't [1]) be altering the model of the
components the message passes through until you get to the component the
message was intended for. So what is a child component then? It is any
component whose `update` function we call from the parent because only that
component knows how to change it's model. We can't do it from the parent when
processing a message.

[1] I say "probably should't" but there's no hard and fast rule against this.

### Walkthrough of the sign out process
This describes the flow of messages under elmish. Again it can be can be
confusing how the flow *really* works and it's advised to read
https://elmish.github.io/elmish/parent-child.html.

1. The user clicks the Sign Out button
2. A message ClickSignOut is dispatched. This message goes straight to the toplevel Elmish message pump.
3. The message ClickSignOut comes back to the application at the toplevel update function Root.update
4. Root.update routes it to SignOut.update
5. SignOut.update calls the function that tells the server to sign out.
6. The sign_out function makes a POST request to the api on the server
7. The return from the request can either be a Success or Failure message. We'll only describe the Success flow here for the sake of brevity.
8. The Success message is dispatched to the top level elmish message pump.
9. The Success message is routed via Root.update to SignOut.update
10. SignOut.update handles the success message. In this case it tells the browser to go to the home page url.

Things to notice:
1. There are 2 messages eventually sent when the user clicks the button, ClickSignOut and SignOutSuccess.
2. Messages come from the top and are routed down. They pass through from parent to child.
3. The Root component doesn't sign out the user. It doesn't make a request to the server to sign out. This is
delegated to the SignOut module.





## Notes about the stack

Notes and hints I need to remember because I keep forgetting.

### Fulma

Fulma is an F# wrapper around the Bulma CSS framework. It follows the React
DSL for React components. This means that each element, like a button, has 2
list arguments. The 1st is a list of properties (props) the the 2nd is the
content (children) of the element. You can access any CSS property from here,
or any property that is part of React, and in doing so modify the behaviour
of the component. For example to add an event handler to a button one adds a
callback to the OnClick handler in the 1st list.

```
Button.button [ Button.OnClick (fun _ -> (dispatch msg)) ] [ str "ClickMe" ]
```

To access the CSS specific properties use
```
Button.button [ Button.Props [ Props.Style Props.CSSProps.<property> "<value>" ] ] [ str "ClickMe" ]
```
### Colour scheme

The colours have been choosed to harmonise. To this I went to canva.com and
they have a good article (https://www.canva.com/learn/brand-color-palette/)
that got me started. However when playing with the canva website to try and
modify the pallete but keep harmony, it didn't give me the tools I needed.
It's too dumbed down. So I went here
(https://www.sessions.edu/color-calculator/) to pick more colors and did some
research on colour theory.

## Deploying on azure

Using a docker image but the fake build.fsx should handle all of that

```
fake build --target Bundle
```

Test with docker locally
```
docker run --rm -it -net=host  sashang/titan
```

If it's ok then push to dockerhub
```
docker push sashang/titan
```

The login to Azure and create a Web App. Under `OS` select Linux, under `Publish` select Docker image
Then click configure container and setup the path to dockerhub where azure will fetch the container

Then click `Create`



