# Scripter plugin

Any CoreKraft module can register one or more instances of this plugin in its `Configuration.json` file. Then Action Query scripts can be used at every node that is using one of the registered plugin instances. 

## Register in Configuration.json

There are two options:

- DataLoader (main) plugin which allows AQ script to be used as a node main action (instead of ADO based SQL for example).
- Custom node plugin from which any number of instances can run before or after the main action of a node.

### Registering as (main action plugin) DataLoader.

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

The name "Scripter" here is arbitrary - one can name the registration freely and then use it by setting `datapluginname` to that name at nodeset or node level.

#### Libraries

The [default libraries](DefaultLibraries.md) are always loaded. 

Each Scripter instance can be configured with a list of internally provided by CoreKraft additional libraries by listing their names separated by commas (without spaces). 

The `libraries` configuration option specifies list of optional libraries to load. They are implemented in CoreKraft, you do not need more plugins for them. 

The libraries currently available inside CoreKraft are:

**basicimage** - Basic image processing. 

**basicweb** - Basic WEB requests.

**files** - File manipulation library

**internalcalls** - Internal calls across the CoreKraft instance.

So for example we can set in CustomSettings `"libraries": "basicweb,internalcalls"` and this will make the functions in these two libraries available in the AQ scripts in nodes using the Scripter plugin.

Using in a nodeset (example extract from a nodeset):

```JSON
 // ... 
 {
  "nodekey": "mynode",
  "islist": 0,
  "datapluginname": "Scripter"
  "read": {
    "parameters": [
      { "name": "id","Expression": "GetFrom('parent',name)"},
      { "name": "authuserid", "Expression": "GetUserId()" }
    ],
    "select": {
      "loadquery": "myscript.aq"
    }
  } 
  // ...
```

The code to be executed can be provided in-place as `"query"` property or (as in the example) in a separate file.

#### Additional settings

In the configuration entry for the Scripter plugin, one can specify any number of additional settings in the `CustomSettings` section. The `CSetting` function from the default libraries provides access to them. _Note that CoreKraft supports only strings in that section!_

For example :

```JSON
  //... omitted for brevity ...
    
      "CustomSettings": {
        "libraries": "-=librarylist=-",
        // Any other settings you want to consume in the AQ scripts through CSetting function
        "mysetting": "My value"
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

### Registering as custom (additional) node plugin.

The purpose of the "custom" plugins is to perform some actions before or after the node's main action. The custom plugins are usually different from the main action plugin. For example it is frequently useful to do something before executing an SQL and then do more after that when the node deals with data and resources tracked with entries in a database, but otherwise stored elsewhere. Action Query scripts are usually good enough for such scenarios and using registration of a Node scripter as custom plugin will provide the necessary options.

Example registration:
```JSON
// ................
"CustomPlugin": [
// ..... possibly other registrations
    {
      "Name": "NodeScript",
      "ImplementationAsString": "Ccf.Ck.NodePlugins.Scripter.NodeScripterImp, Ccf.Ck.NodePlugins.Scripter",
      "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.INodePlugin, Ccf.Ck.SysPlugins.Interfaces",
      "Default": true,
      "CustomSettings": {
        "libraries": "files,basicimage,internalcalls",
        // Some custom settings for use by the scripts through CSetting
        "uploaddir": "UploadedFiles/",
        "noimage": "assets/no-image-icon.svg"
      }
    }
// ..... possibly other registrations
]
// .............
```

Obviously the example hints at script dealing with some image files and is provided access to the relevant libraries . Virtually everything said about the registration as main action plugin applies to registration as custom node plugin - libraries, custom setting etc.

#### Usage in a nodeset

The custom node actions are configured separately for read and write operations in the corresponding node sections (`read` or `write`). They cannot be configured for specific actions only - i.e. currently you cannot register custom action for `insert` action only, instead it must be registered for `write` and check itself wat the exact action is in order to determine if it should do something or not. There are some plans to extend the configuration this way in near future, but the matter is not yet decided.


```JSON
"write":{
						"parameters":[
							// some parameters ...
						],
						"insert":{
							"loadquery": "1.sql"
						},
						"update":{
							"loadquery": "2.sql"
						},
						"delete":{
							"loadquery": "3.sql"
						},
            "customplugins": [
							{ 
                "custompluginname": "MyNodeScript",
								"executionorder": 0,
                "afternodeaction": true,
                "loadquery": "script1.aq"
              },
              { 
                "custompluginname": "FileScript",
								"executionorder": 1,
                "afternodeaction": true,
                "loadquery": "script2.aq"
              }
            ]
},
```

We assume both `MyNodeScript` and `FileScript` are registered as custom plugins along the lines of the previous example.

Like for the DataLoader (main action) plugins here the script can be in external file (loadquery) or inline (query). Obviously only very short scripts can be written inline.

There are 3 possible phases points in time in which to execute each of the custom node scripts. This is specified by setting the corresponding property to true:

`beforenodeaction` - Execute before the main action

`afternodeaction` - Execute after the main action

`afternodechildren` - Execute after the main action and all the child nodes are processed.
