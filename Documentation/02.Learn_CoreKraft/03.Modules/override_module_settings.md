<!-- header
{
    "title": "Override settings of CoreKraft modules",
    "keywords": [ "modules", "override", "app.settings", "apps" ]
}
-->
### Introduction ###
CoreKraft is a component-oriented system. The components in its context are called modules. These modules are self sufficient and contain everything needed to supply a functioning feature (e.g. html views, css, javascript and C# code). The C# code is represented by other compiled sub-components called Node-Plugins or Loaders. Usually these plugins are written in a general manner and are used in different modules by referencing the compiled lib. Obviously, they must be configured differently. Until now CoreKraft supported in place configuration settings which was problematic in complex deployment scenarios. The new feature gives opportunity to provide external configuration settings for the Node-Plugins (Loaders).


## The default behavior:
 - The dependencies of the module are read from Dependency.json file. (Example of Dependency.json file below)
```json
{
  "name": "Board",
  "version": "1.0.0",
  "description": "Description of the project",
  "keywords": [],
  "author": "Clean Code Factory",
  "license": "MIT License",
  "dependencies": {
    "PlatformUtility": "^1.0.0"
    ...
  },
  "signals": [
    ...
  ]
}
```
 - Configuration settings of the module are read from the Configuration.json file. (Example of Configuration.json file bellow)
```json
 {
  "KraftModuleConfigurationSettings": {
    "NodeSetSettings": {
      "SourceLoaderMapping": {
        "NodesDataIterator": {
          "NodesDataLoader": [
            {
              "Name": "JsonData",
              "ImplementationAsString": "Ccf.Ck.SysPlugins.Data.Json.JsonDataImp, Ccf.Ck.SysPlugins.Data.Json",
              "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.IDataLoaderPlugin, Ccf.Ck.SysPlugins.Interfaces",
              "Default": true,
              "CustomSettings": {
                BasePath: @wwwroot@/Custom/Definitions/@nodename@/
              }
            },
            {
              "Name": "SqlServerData",
              "ImplementationAsString": "Ccf.Ck.SysPlugins.Data.Db.ADO.GenericSQLServer, Ccf.Ck.SysPlugins.Data.Db.ADO",
              "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.IDataLoaderPlugin, Ccf.Ck.SysPlugins.Interfaces",
              "Default": true,
              "CustomSettings": {
                ConnectionString: Server=ConnectionString
              }
            },
            {
              "Name": "SqLiteData",
              "ImplementationAsString": "Ccf.Ck.SysPlugins.Data.Db.ADO.GenericSQLite, Ccf.Ck.SysPlugins.Data.Db.ADO",
              "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.IDataLoaderPlugin, Ccf.Ck.SysPlugins.Interfaces",
              "Default": true,
              "CustomSettings": {
                ConnectionString: Data Source=@ConnectionString
              }
            }
          ]
        },
        "ViewLoader": [
          {
            "Name": "HtmlViewLoader",
            "ImplementationAsString": "Ccf.Ck.SysPlugins.Views.Html.HtmlViewImp, Ccf.Ck.SysPlugins.Views.Html",
            "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.ISystemPlugin, Ccf.Ck.SysPlugins.Interfaces",
            "Default": true,
            "CustomSettings": {
              ViewPath: @custom-view-path
            }
          }
        ],
        "LookupLoader": [
        ],
        "ResourceLoader": [
        ]
      }
    }
  }
}

```
 - The application settings are read from the appsettings.json file. Here the default values for each module are loaded. (Example of appsettings.json file bellow)
```json
{
  "KraftGlobalConfigurationSettings": {
    "GeneralSettings": {
      "EnableOptimization": false,
      "ModulesRootFolders": [
        "@contentroot@/Modules/",
        "@contentroot@/bin/Debug/netcoreapp2.2/Modules/"
      ],
      "DefaultStartModule": "BindKraftIntro",
      "Theme": "Basic",
      "KraftUrlSegment": "node",
      "KraftUrlCssJsSegment": "res",
      "KraftUrlResourceSegment": "raw",
      "KraftUrlModuleImages": "images",
      "KraftUrlModulePublic": "public",
      "KraftRequestFlagsKey": "sysrequestcontent",
    
      "HostingServiceSettings": [
        {
          "IntervalInMinutes": 0,
          "Signals": [
            "UpdateTenant"
          ]
        }
      ],
      "SignalSettings": {
        "OnSystemStartup": [
          "OnSystemStartup"
        ],
        "OnSystemShutdown": []
      },
      "SignalRSettings": {
        "UseSignalR": false,
        "HubImplementationAsString": "",
        "HubRoute": "/hub"
      }
    }
  }
}
```
# Changing the default settings of a loader or a collection of loaders for a module:
In order to override the default settings for each plugin you should add a section (OverrideModuleSettings) in the corresponding appsettings.json file.
In the section you should provide an array of objects. Each object contains ModuleName, Collection of Loaders where you should provide LoaderName and 
CustomSettings object that will override the default behavior of the module's loader. 
(See the example bellow)
```json
{
  "KraftGlobalConfigurationSettings": {
    "GeneralSettings": {
      "EnableOptimization": false,
      "ModulesRootFolders": [
        "@contentroot@/Modules/"
      ],
      "DefaultStartModule": "KraftApps_Launcher",
      "Theme": "Basic",
      "KraftUrlSegment": "node",
      "KraftUrlCssJsSegment": "res",
      "KraftUrlResourceSegment": "raw",
      "KraftUrlModuleImages": "images",
      "KraftUrlModulePublic": "public",
      "KraftRequestFlagsKey": "sysrequestcontent",
     
    OverrideModuleSettings: 
    [
      {
        "ModuleName": "Board",
        "Loaders": [
          {
            "LoaderName": "SqLite",
            "CustomSettings": {
              "ConnectionString": "Data Source=@connectionStringData",
              "NoCache": false
            }
          }
        ]
      }
    ]
  }
}
```

[Back to README](../../../README.md)