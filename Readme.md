# Titan

## Requirements
The SAFE stack for F# web development.
https://safe-stack.github.io/

Install that using the link above. Then see the next section to build and run.

## Build

Ensure you have the dotnet sdk 2.1.500 installed. I've found that exporting the
following variables will ensure a headache free existance:

```
export DOTNET_ROOT=<path to dotnet root>
export PATH="<path to dotnet root>:$PATH"
```

Then cd into the directory you downloaded this source code and run:

```
fake build --target run
```

Then to build the custom stylesheet
```
yarn install node-sass --dev
yarn install bulma --dev
yarn run css-build
```
Changes to `mystyles.scss` aren't picked up by the watch variables in the
fake process, so if you change anything here remember to restart fake.

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
authetication providers like Google.

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

Using Postgres at the moment. The filesystem database is gone.

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