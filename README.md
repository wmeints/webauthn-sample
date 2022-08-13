# WebAuthn sample

This sample demonstrates how to integrate WebAuthn with FIDO2 keys into ASP.NET Core. Please read the rest of this README 
to get a full picture of what's in the sample and how the various components work together.

## System requirements

* .NET SDK 6.0
* Latest LTS release Node 

## Getting started

You can run this sample as a docker container using the following steps:

* `git clone https://github.com/wmeints/webauthn-sample/`

After cloning the repository, create a file `.env` with the following content:

```text
DB_PASSWORD=SomethingStr0ng!
```

You can choose any password you like. Once you have the password set, perform the following steps from the root of the project directory:

* `docker-compose up -d`
* `cd src/WebAuthnSample`
* `dotnet user-secrets set "ConnectionStrings:DefaultDatabase" "data source=127.0.0.1;initial catalog=webauthn;user id=sa;password=<your-password>"`

Make sure the password in the last step matches the password that was stored in the `.env` file. Now execute the following
command from `src/WebAuthnSample` directory:

* `dotnet run`

Open the browser to `https://localhost:7140` follow the on-screen instructions.

## Documentation

### Project structure

The project is made out of two parts:

* `src/WebAuthnSample` - Contains the server-side application.
* `src/WebAuthnSample/Client` - Contains the client-side scripts.

The server-side components are written in ASP.NET Core 6.
For the client-side components I've used webpack with React. 

#### The client-side scripts

The client-side scripts use React. I've made a setup where each page gets its own dedicated script.

* `src/WebAuthnSample/Pages/Login.cshtml` - Uses `src/WebAuthnSample/wwwroot/js/authentication.js`.
* `src/WebAuthnSample/Pages/Register.cshtml` - Uses `src/WebAuthnSample/wwwroot/js/registration.js`.

The registration script is compiled from `src/WebAuthnSample/Client/registration/index.tsx` and related files.
The authentication script is compiled from `src/WebAuthnSample/Client/authentication/index.tsx` and related files.

For performance reasons, I've split the shared scripts into `src/WebAuthnSample/wwwroot/shared.js`. This script
is contains react, react-dom, and the sources from `src/WebAuthnSample/Client/shared/`.

#### The server-side components

The server-side part follows this layout:

* `src/WebAuthnSample/Controllers` - Contains two API controllers for registration and authentication.
* `src/WebAuthnSample/Services` - Contains business logic to implement the registration and authentication.
* `src/WebAuthnSample/Models` - Contains models used in the application.
* `src/WebAuthnSample/Data` - Contains persistence logic for the application.
* `src/WebAuthnSample/Forms` - Contains the forms used in the controllers. 
* `src/WebAuthnSample/Pages` - Contains the pages for the application.
