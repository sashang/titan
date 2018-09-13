# Titan

## Requirements
The SAFE stack for F# web development.
https://safe-stack.github.io/

Install that using the link above. Then see the next section to build and run.

## Build

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
on top of ASP.NET Core. You'll need the following environment variables set

### Bash:
```
export GOOGLE_ID="client id"
export GOOGLE_SECRET="client secret"
```

### Windows Powershell:
```
$Env:GOOLGE_ID="client id"
$Env:GOOGLE_SECRET="client secret"
```

The values for those variables you can get by creating an application for use with Google+ (https://console.cloud.google.com/apis/library/plus.googleapis.com). If those environment variables
are not set the server will not start.
