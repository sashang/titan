# Titan

## Introduction

### A note about the naming
Titan is the name of this project (a webapp) associated with this git repo. I
built to help tutors deliver lessons remotely and manage their classes. The
provides admin tools for the teacher to manage their students, generate reports
and manage assignments. It also provides video conferencing facilities.

Tewtin is the customer facing name for the product. It encompasses all services that
might be needed to deliver the entire service to the customer. This means that
one can create another project, call it deimos, host it in git, and then place
it under the umbrella name of Tewtin. For example if I was to create another 
service that provided an API for taking payments, I might name the repository
deimos and put all the code related to it there. 

### In case of my passing away into the grave or wherever.

This is a list of services you will need to maintain (or contact to close down
if you don't feel like running this anymore).

The following are associated with `sashang@gmail.com`
* Lastpass (password manager contains all of the passwords to the services
    below)
* https://github.com/sashang/titan (this is private so not publically visible)


The following accounts are associated with the username `sashang@tewtin.com`
* GSuite
* Azure
* Google analytics
* Google credentials (oauth secret and key)
* Tokbox (api key and secret)
* Godaddy


If you want to keep running this then talk to https://compositional-it.com/.
They're an IT consulting firm that will understand the tech stack used.

## Requirements
The SAFE stack for F# web development. Follow the instructions there to install
EVERYTHING you need (.net sdk etc...)
https://safe-stack.github.io/


Do a git clone to get the source code.
```
git clone git@github.com:sashang/titan.git
```

Then install the following from the root of the source code
```
yarn install node-sass --dev
yarn install bulma --dev
```
:
## Build

I use Fish as my shell so all shell commands below assumes this.

Ensure you have the dotnet sdk 3.1.100 installed. I've found that exporting the
following variables will ensure a headache free existance:

```
set -x DOTNET_ROOT $HOME/code/dotnet/3.1.100
set -x ASPNETCORE_ENVIRONMENT Development
```

Then cd into the directory you downloaded this source code and run:

```
fake build
```

This will build it. If there are any errors fix them and then try again.

Once errors are fixed, run it:
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
* Check this link for reference https://dotnet.microsoft.com/download/dotnet-core/2.2 and to download the .NET core sdk.

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

If you start it without the DOTNET_ROOT path set to the path to the SDK in
use for development, it will find whatever SDK is on your system and it
searches from a set of common paths. This SDK may not be in the one you
intend to use, so best to start `code` with the right path set in the
DOTNET_ROOT environment variable.

```
set -x DOTNET_ROOT <path to sdk>
code <path to source code>
```

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
Goto https://console.cloud.google.com/apis/credentials and select titan-231208
to see the client id and secret. This allows you to authenticate with Google
when logging in. These values should never be committed to the git repo (TODO:
find a way to handle the secrets better)

They are used in appsettings.json or appsettings.Development.json.

```
    "GoogleClientId" : <secret>,
    "GoogleSecret" : <secret>
```

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

Example: Inserting a new student in a school
```
mssql> insert into "Student" (SchoolId,"UserId") values ('1','141');
```

#### Connection string

Use this something like this. You'll need your own account in Azure.
```
Server=tcp:titan-sql-server.database.windows.net,1433;Initial Catalog=titan-dev;Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```



#### Migrations

* _This is broken at the moment_...

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

### Troubleshooting fable

Fable is the compiler that translates F# to JS using Babel (Fable and Babel rhyme).

Source .fs files need to be under the project root.
Having them outside the root will trip up the fable compiler.

When upgrading .NET packages in the Client with paket you might get dependency errors.
Basically one package depends on a version of THoth.Json for example, that's ahead of what was originally there.
So Thoth.Json is upgraded.
But this breaks the existing stuff.
Solution is to search for a version that is compatible with both dependant packages and pin THoth.Json in the paket.dependencies.

```
group Client
    source https://api.nuget.org/v3/index.json
    framework: netstandard2.0
    storage: none

    nuget Fable.Browser.Url
    nuget Fable.Browser.Websocket
    nuget Fable.Core ~> 3
    nuget Fable.Elmish ~> 3
    nuget Fable.Elmish.Debugger ~> 3
    nuget Fable.Elmish.HMR ~> 4
    nuget Fable.Elmish.React ~> 3
    nuget Fable.SimpleJson 3.7
    nuget Thoth.Fetch ~> 1
    nuget Fable.React ~> 5
    nuget Fulma ~> 2
    nuget Fable.FontAwesome.Free ~> 2
    nuget Thoth.Json 3.1
```

Here Thoth.Json is included explictly even though it's not a direct dependency.

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

## Deploying production on Azure

Using a docker image but the fake build.fsx should handle all of that
Make sure your appsettings.json file is correct. appsettings.json should not be
committed to the repo as it contains passwords. It also contains the connection
string the +production database+. So when developing locally *do not* use
appsettings.json. Use appsetting.Development.json instead.

```
source <path to .net sdk>
set -e ASPNETCORE_ENVIRONMENT
fake build --target Docker
```

Test with docker locally
```
docker run --rm -it --net=host  sashang/titan
```

If it's ok then push to dockerhub
```
docker push sashang/titan
```

### Intial setup

This only needs to be done once.
The login to Azure and create a Web App. Under `OS` select Linux, under
`Publish` select Docker image Then click configure container and setup the
path to dockerhub where azure will fetch the container. Enable a webhook so
that future pushes to dockerhub will call azure back and tell it a new image
is available. Azure will then go ahead and reload the image.

Then click `Create`


## Azure architecture

Azure is used to host tewtin.com. There are 2 major services running here. One
is the database, the other is the App Service that runs .NET Core. Most of the
setup is faily trivial, and point and click. There is a webhook to docker hub
that dockerhub uses to callback azure when `docker push sashang/titan`
completes.

## Troubleshooting

### Getting the logs
```
lftp -u 'tewtin\$tewtin',2Bk0Lvdtg2EpzXtae9pa0498ynP1FP0mXH2hCjepvJKzm9JlGBmhLstkrMl7 ftp://waws-prod-mwh-007.ftp.azurewebsites.windows.net/site/wwwroot
```

Then cd to the root (cd \)
```
 cd /
lftp tewtin\$tewtin@waws-prod-mwh-007.ftp.azurewebsites.windows.net:/> ls
02-11-19  10:40AM       <DIR>          .mono
07-18-19  02:41PM       <DIR>          ASP.NET
07-31-19  04:19AM       <DIR>          LogFiles
07-18-19  02:41PM       <DIR>          site
lftp tewtin\$tewtin@waws-prod-mwh-007.ftp.azurewebsites.windows.net:/> cd LogFiles/
lftp tewtin\$tewtin@waws-prod-mwh-007.ftp.azurewebsites.windows.net:/LogFiles> ls
07-07-19  07:28PM                 7123 2019_07_07_RD00155DA0177E_docker.log
07-23-19  12:17AM               197625 2019_07_22_RD00155DA0177E_default_docker.log
07-24-19  01:50AM              2040767 2019_07_23_RD00155DA0177E_default_docker.log
07-24-19  11:59PM              1188340 2019_07_24_RD00155DA0177E_default_docker.log
07-25-19  09:10AM              2098660 2019_07_25_RD00155DA0177E_default_docker.1.log
07-26-19  12:12AM               456792 2019_07_25_RD00155DA0177E_default_docker.log
07-27-19  12:56AM               264332 2019_07_26_RD00155DA0177E_default_docker.log
07-28-19  12:54AM               114737 2019_07_27_RD00155DA0177E_default_docker.log
07-29-19  12:03AM               193224 2019_07_28_RD00155DA0177E_default_docker.log
07-29-19  10:33PM               218130 2019_07_29_RD00155DA0177E_default_docker.log
07-31-19  12:15AM              1927251 2019_07_30_RD00155DA0177E_default_docker.log
07-31-19  12:15AM              1397436 2019_07_31_RD00155DA0177E_default_docker.log
04-21-19  03:46AM       <DIR>          kudu
02-14-19  12:00AM       <DIR>          webssh
07-18-19  02:40PM                   18 __lastCheckTime.txt
lftp tewtin\$tewtin@waws-prod-mwh-007.ftp.azurewebsites.windows.net:/LogFiles>
```

