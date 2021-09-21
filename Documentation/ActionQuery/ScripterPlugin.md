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

The `libraries` configuration option specifies list of optional libraries to load. They are implemented in CoreKraft, you do not need more plugins for them. One can list all of them, but it is recommended to configure only those that will be needed according to the planned role for the plugin configuration. For example if it will only deal with uploading and managing files locally, it is better to not include `basicweb` library, because this will enable the programmers who write scripts to make WEB requests and this will be more likely a source for mistakes, than something useful.

_Basically, it is strongly recommended_ to plan specific purpose for all the plugin configurations and configure the plugin (Any plugin not only the Scripter discussed here) for that purpose only - as close as possible. Universal plugin configurations may be unavoidable sometimes, but should not be used if other approach is possible.

The libraries currently available inside CoreKraft are:

**basicimage** - Basic image processing. 

**basicweb** - Basic WEB requests.

**files** - File manipulation library

**internalcalls** - Internal calls across the CoreKraft instance.

So for example we can set in CustomSettings `"libraries": "basicweb,internalcalls"` and this will make the functions in these two libraries available in the AQ scripts in nodes using the Scripter plugin.

The libraries are split into default libraries and others for a reason. You may have noticed that all the CoreKraft AQ libraries basically provide access to the "outer" world. This makes them potentially dangerous in cases where the scripts should not be able to access anything outside unless it is their reason for existence - so, do not open doors which you do not need.

### Additional settings

In the configuration entry for the Scripter plugin, one can specify any number of additional settings needed for the job the scripts should handle. These can be simple constants, file paths, connection strings, web addresses and so on. The `CSetting` function from the default libraries provides access to them.

For example if you have the above sample configuration modified like this:

```JSON
... omitted for brevity ...
    {
      "Name": "Scripter",
      "ImplementationAsString": "Ccf.Ck.SysPlugins.Data.Scripter.ScripterImp, Ccf.Ck.SysPlugins.Data.Scripter",
      "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.IDataLoaderPlugin, Ccf.Ck.SysPlugins.Interfaces",
      "Default": true,
      "CustomSettings": {
        "libraries": "-=librarylist=-",
        // Any other settings you want to consume in the AQ scripts through CSetting function
        "mysetting": "My value"
      }
    }
... omitted for brevity ...    
```
You can read it like this: 

```
CSetting('mysetting')
```

And if you want to have a fallback value if the setting is not present:

```
CSetting('mysetting', 'I am the fallback value')
```

## How to use the Scripter in a nodeset

Lets assume we have a configuration for a Scripter plugin. In the examples below we will assume that it is named `Scripter1`.

TODO: Continue
