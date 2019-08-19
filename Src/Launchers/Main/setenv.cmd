@echo off
setx COREKRAFT_PATH %cd%
echo Environment variable COREKRAFT_PATH has been set to %cd%
echo for the current user.
echo .
echo you can run projects by using command like this one
echo dotnet run --project %%COREKRAFT_PATH%%\Ccf.Ck.Launchers.Main.csproj -- %%cd%%
echo executed in the directory containing your appsettings json file(s).
PAUSE

