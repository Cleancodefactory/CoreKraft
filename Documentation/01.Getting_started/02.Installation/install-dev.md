# Install, try and develop

CoreKraft should be considered an `engine` that runs `projects` (sometimes called workspaces because of the way BindKraft sees the client).

That is why one needs to install CoreKraft and at least one project in order to have an functional setup for debugging and experimenting.

We will note project specific settings or actions further in the document.

## Prerequisites

Before dealing with CoreKraft download and install NET 6 from [Microsoft](https://dotnet.microsoft.com/)

For the `project` it is likely that you will need to compile SCSS (Sass). We use a theming technique based on it in many projects.

    To check if your project is using ThemeStyles - check if a directory with that name exists in Modules directory.
    If it does you will need to build the stylesheets before running the project. 

### Sass themes (if needed)

_Skip this section if no ThemeStyles is used in the project or the CSS is pre-built._

Make sure you have Node.js (12 or greater will be ok) and npm - https://nodejs.org/
Make sure you have sass compiler or install it with npm globally:

```
npm install -g sass
```

sass version should be 1.50 or above for the ThemeStyles module sass based themes.

    ThemeStyles is a CoreKraft module containing templates for controls and windows included often in
    CoreKraft/BindKraft projects. It is not integral part of either CoreKraft or BindKraft, but a styling
    technique.

    Compiling the themes can be done by running the:

    .\CssCompile.bat 
    
    in the root directory of the project (see further below)


## Building the CoreKraft

Clone the repository or download the sources. Clone or unpack in a directory (we will call it CoreKraft root directory here)

The CK root directory contains the CoreKraft.sln Visual Studio solution file. It can be opened with Visual Studio 2022.

**Building**

No matter how it is build it is most convenient to create a launchSettings.json file enabling to start CoreKraft for debugging with the `project` of choice. Check later in the document for example launchSettings.json

### with Visual Studio 

Open the solution and just build/rebuild it. For debugging later choose as start project: **Ccf.Ck.Launchers.Main** and preferably create the launchSettings.json file before running it (see below for details)

### on the Command line

To build on the command line open Cmd or Powershell, go to the CoreKraft root directory and run this command

```
dotnet build CoreKraft.sln

```

## The CoreKraft runtime

The runtime is the project that forms the CoreKraft process and (starting from the root CoreKraft directory) this is in: `{CoreKraft directory}\Src\Launchers\Main` and the project file is named Ccf.Ck.Launchers.Main.csproj

This is also the place where one can take care for making the usage more convenient in two ways:

### When server debugging is not necessary

In `{CoreKraft directory}\Src\Launchers\Main` there is a .cmd file setenv.cmd - run it in place to register an environment variable pointing to the CoreKraft directory. It will be available for the Windows user profile from this point on. If you move CoreKraft elsewhere you will need to rerun it.

This is useful when you want to run projects with CoreKraft. This can be done like:

```
dotnet run --project %COREKRAFT_PATH%\Ccf.Ck.Launchers.Main.csproj -- C:\Projects\SomeProject
```

The %COREKRAFT_PATH% is the path to the CoreKraft runtime and the setenv.cmd mentioned above sets it. So once you tun it you do not need to write the full path every time/ The switch --project is for the dotnet runtime and for NET CoreKraft is a project (do not mistake this with the projects executed by CoreKraft).

After the -- all the parameters are sent to CoreKraft and it recognizes only one - the path to the CoreKraft project directory. The directory must have an appsettings.json file (depending on the desired environment it can be for example appsettings.Development.json for development environment).

For debugging and in general during the development process the "Development" should be chosen. CoreKraft behaves differently in production and development and production is not convenient for debugging (even more for the BindKraft and CSS side of things).

Dotnet supports other deployment options, but for further deployment through a front WEB server (like IIS) keeping everything as projects is most convenient.

### When debugging of CoreKraft is required

In this case one would want to run CoreKraft directly from Visual Studio.

Here comes the aforementioned `launchSettings.json`. You may have one in the `{CoreKraft directory}\Src\Launchers\Main\Properties` directory or you might need to create it from scratch. Whatever the case you need a launch profile in it that will define how to run it and run CoreKraft with a project (workspace). The profile part that you need will look something like this:

```Json
"profiles": {
    "CoreKraft profile": {
      "commandName": "Project",
      "commandLineArgs": "C:\\Projects\\Project1",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }
```

The name of the profile does not matter (it is "CoreKraft profile" in the example), you can actually add profiles that will launch as many different projects with CoreKRaft as needed, just name them differently.

The `commandLineArguments` have to contain the path to the CoreKraft project you want to start.

As mentioned above the ASPNETCORE_ENVIRONMENT defines in which environment to start and for development (including debugging) "Development" should be chosen.

The `applicationUrl` defines where the WEB server will listen - both address, protocol and port. CoreKraft requires HTTPS and will try to redirect to the HTTPS version of the URL if invoked with regular HTTP non-secured protocol. So, if you want to listen on both HTTP and HTTPS make sure you have a pair with HTTP and HTTPS versions of the URL ( well in development environment HTTP wont be useful).

You can also instruct Visual Studio to launch a browser together with CoreKraft, but you can navigate manually in the browser, of course.

After you have the launchSettings.json VisualStudio will list all the profiles from it in the run drop down in its toolbar. From there you can select the one running the project you want to start and run it with or without debugging.

Besides debugging CoreKraft itself, you need the same setup if you have specific plugins for it in your project and you want to debug them. See more about the projects in [CoreKraft project](install-projects.md) and further.