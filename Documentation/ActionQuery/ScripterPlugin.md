# Scripter plugin

Any CoreKraft module can register one or more instances of this plugin in its `Configuration.json` file. Then AQ scripts can be used at every node that is using one of the registered plugin instances.

## Register in Configuration.json

```JSON
{
  "KraftModuleConfigurationSettings": {
    "NodeSetSettings": {
      "SourceLoaderMapping": {
        "NodesDataIterator": {
          // other registrations
          "NodesDataLoader": [
              // Other registrations

              {
              "Name": "Scripter",
              "ImplementationAsString": "Ccf.Ck.SysPlugins.Data.Scripter.ScripterImp, Ccf.Ck.SysPlugins.Data.Scripter",
              "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.IDataLoaderPlugin, Ccf.Ck.SysPlugins.Interfaces",
              "Default": true,
              "CustomSettings": {
                "libraries": "-=librarylist=-"
                // Any other settings you want to consume in the AQ scripts through CSetting function
              }
            }

            // Other registrations
          ]
          // The file continues - skipping for brevity.
```

Each Scripter instance can be configured with a list of internally provided by CoreKraft additional libraries. The names are separated with commas and should not contain spaces. The [default libraries](DefaultLibraries.md) are always loaded. 

The libraries currently available inside CoreKraft are:

**basicimage** - Basic image processing. 

**basicweb** - Basic WEB requests.

**files** - File manipulation library

**internalcalls** - Internal calls across the CoreKraft instance.

So for example we can set in CustomSettings `"libraries": "basicweb,internalcalls"` and this will make the functions in these two libraries available in the AQ scripts in nodes using the Scripter plugin.

The libraries are split into default libraries and others for a reason. You may have noticed that all the CoreKraft AQ libraries look like ones that provide access to the "outer" world. This makes them potentially dangerous in cases where the scripts should not be able to access anything outside.