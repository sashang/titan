# Titan

## Requirements
The SAFE stack for F# web development.
https://safe-stack.github.io/

Install that using the link above. Then see the next section to build and run.

## Build

Ensure you have the dotnet sdk 2.1.3 installed. I've found that exporting the
following variables will ensure a headache free existance:

```
export DOTNET_ROOT=<path to dotnet root>
export PATH="<path to dotnet root>:$PATH"
```

Then cd into the directory you downloaded this source code and run:

```
fake build --target run
```

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

## Google authentication
This is all in a state of flux so may or not may not work. Set the following
environment variables: 

### Bash:
```
export TITAN_GOOGLE_ID="client id"
export TITAN_GOOGLE_SECRET="client secret"
```

### Windows Powershell:
```
$Env:TITAN_GOOLGE_ID="client id"
$Env:TITAN_GOOGLE_SECRET="client secret"
```

The values for those variables you can get by creating an application for use
with Google+
(https://console.cloud.google.com/apis/library/plus.googleapis.com). If those
environment variables are not set the server will not start.

## Filesystem Database

The filesystem database is intended for testing the server side API. It is
simply a collection of JSON files stored on the filesystem. To enable it set the
following environment variable

### Bash:
```
export TITAN_FILESYSTEM_DB="yes"
```

### Windows Powershell:
```
$Env:TITAN_FILESYSTEM_DB="yes"
```

Currently other databases are not supported so it's recommended to enabled this.

## Using Paket

Paket is a package manager for .NET packages. It's used in this project to
add/remove .NET assmeblies that this project depends on.

### Adding a dependency

```
.paket/paket.exe add Chiron --version 7.0.0-beta-180105 --project src/Server/Server.fsproj -g Server
```

### Removing a dependency

```
.paket/paket.exe remove Chiron
```

Once added or removed the files `paket.dependencies`, `paket.lock` and
`src/Server/Server/paket.references` are modified. These changes need to be
committed.
