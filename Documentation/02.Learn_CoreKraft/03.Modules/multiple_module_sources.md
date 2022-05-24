<!-- header
{
    "title": "Multiple module sources",
    "keywords": [ "modules", "override", "app.settings", "roots", "sources" ]
}
-->
# How modules are loaded from multiple sources #
For introduction about CoreKraft modules please visit: [Modules](/Documentation/02.Learn_CoreKraft/03.Modules/modules.md)

CoreKraft can load its modules from multiple sources. This is configured in the appsettings.[Development/Production].json
```json
{
  {
  "KraftGlobalConfigurationSettings": {
    "GeneralSettings": {
      "EnableOptimization": false,
      "DefaultStartModule": "Start_Launcher",
      "ModulesRootFolders": ["@contentroot@/Modules/", "@contentroot@/Modules2/","%KRAFTCLIENTTOOLS%"]
      ...
    }
}
```

The idea is that you tell the framework where your modules are. The references in the "DefaultStartModule" will build the dependency tree and will load only the modules which are part of it. Not all modules from these sources will be loaded in memory but the directly or indirectly referenced ones. 
From version 5.0 of the framework the loading process is as following:
1. Check all subdirectories of the configured "ModulesRootFolders" and collect only valid CoreKraft modules.
2. Start from the "DefaultStartModule"'s references and build the dependency tree (tree shaking).
3. Load all really referenced modules after the tree shaking in memory and boot the system.

## Replacements and reserved words in the paths
You have noticed "@contentroot@" special word. During booting it will be replaced with the actual content root value. This is handy to not hard-code the absolute paths and reduce friction during deployment.
In the above example "%KRAFTCLIENTTOOLS%" will be replaced with environment variable's value with the same name.
![Environment variables](/Documentation/Images/Environment_Variables.png)

## Dependencies and Optional Dependencies
 The dependencies of the module are read from Dependency.json file. (Example of Dependency.json file below)

```json
 {
  "name": "Lively_Launcher",
  "version": "1.0.0",
  "description": "",
  "keywords": [],
  "author": "Clean Code Factory",
  "license": "MIT License",
  "dependencies": {
    "BindKraft": "^1.0.0",
    "PlatformUtility": "^1.0.0",
    "Lively": "^1.0.0",
    "EasyCms": "^0.0.1",
    "CanvasDrawing": "^1.0.0",
    "ConferenceHall": "^0.0.1",
    "ThemeStyles": "^1.0.0"
  },
  "optionalDependencies": { <<<<<<<<<<<<<<<<<<<<<<<<<<<< new setting
    "KraftClientTools": "^1.0.0"
  },
  "signals": [],
  "release": [
    {
      "version": "1.0.0",
      "info": "Initial creation"
    }
  ]
}
```
The "optionalDependencies" section contains modules which will be loaded if found in "ModulesRootFolders" and referenced in section "DefaultStartModule". If not found or not referenced they won't be loaded. Version compatibility will be checked. The only difference to "dependencies" section is that the system won't trigger an exception.


[Back to README](../../../README.md)