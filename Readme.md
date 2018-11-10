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

## Debugging

It's worth installing Redux DevTools for Chrome as an extension. Then you can
view the messages in the Chrome debugger (hit F12).

## Environment Variables

### Google authentication
This is all in a state of flux so may or not may not work. Set the following
environment variables: 

#### Bash:
```
export TITAN_GOOGLE_ID="client id"
export TITAN_GOOGLE_SECRET="client secret"
```

#### Windows Powershell:
```
$Env:TITAN_GOOLGE_ID="client id"
$Env:TITAN_GOOGLE_SECRET="client secret"
```

The values for those variables you can get by creating an application for use
with Google+
(https://console.cloud.google.com/apis/library/plus.googleapis.com). If those
environment variables are not set the server will not start.

### Filesystem Database

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

## Paket/.NET weirdmess

Most of the problems I've had stemmed from the fucking toolchain. Paket will sometime
throw out weird bugs. This section lists them and work arounds.

### Error in Paket.Restore.targets

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

### Stackoverflow in paket.

Here's another bug that crashes paket with a stack overflow: https://github.com/sashang/paket-bug

Workaround is to remove the paket.lock file and run `paket install`.

### Updating assemblies breaks your code

When learning how to use this, most examples refer to assemblies in
paket.dependencies without including the version numbers of the assembly to use.
Running `paket update` will update those assemblies which are not pinned to a
version. This has burnt me numerous times. An assembly is updated, and the one
in my cache is 6 months old. I run `paket update` and it breaks my code because
it pulls in the latest version of that assembly. I've started pinning all
assemblies to the version number so I don't have to deal with this problem.


