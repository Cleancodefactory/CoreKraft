<!-- header
{
    "title": "CallDateLoader System Plugin",
    "description": "CallDataLoader overview, configuration and how to use.",
    "keywords": [ "CoreKraft", "SysPlugins" ]
}
-->

# CallDataLoader

## Overview
CallDataLoader system plugin executes internally plugins and returns the result of their actions. It executes plugins with the same operation it was called. For example if CallDataLoader is called in read operation it will execute internally the plugin also in read operation.

## Configuration

Add to the `NodesDataIterator` section in `Configuration.json` the configuration:
```
{
    "Name": "CallDataLoader",
    "ImplementationAsString": "Ccf.Ck.SysPlugins.Data.Call.CallDataLoaderImp, Ccf.Ck.SysPlugins.Data.Call",
    "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.IDataLoaderPlugin, Ccf.Ck.SysPlugins.Interfaces",
    "Default": true,
    "CustomSettings": {
    "ModuleKey": "module key",
    "NodesetKey": "nodeset key",
    "NodepathKey": "nodepath key"
    }
}
```
Where:
 * The value of __ModuleKey__ identifies a module in the parameters section of a node.
 * The value of __NodesetKey__ identifies a nodeset in the parameters section of a node.
 * The value of __NodepathKey__ identifies a nodepath in the parameters section of a node.

## How to use
Create a new node in a definition.json. For example:
 ```
 {
    "nodekey": "the name of the node key",
    "datapluginname": "CallDataLoader",
    "executionorder": index of execution order,
    "islist": 0 or 1,
    "parameters": [
        {
            "name": "module key",
            "Expression": "the module name"
        },
        {
            "name": "nodeset key",
            "Expression": "the nodeset name"
        },
        {
            "name": "nodepath key",
            "Expression": "the nodekey name"
        }
    ],
    "read": {
        "select":{
            "query":""
        }
    }
}
 ```
 Where:
 * __datapluginname__ should be the same as the name in the `Configuration.json`.
 * The parameters section must contains at least three parameter objects:
    * parameter object where its name property is equal to the entered ModuleKey value in the __Configuration.json__ and its Expression property is equal to name of the module where the node is located.
    * parameter object where its name property is equal to the entered NodesetKey value in the __Configuration.json__ and its Expression property is equal to the a nodeset name where the node is located.
    * parameter object where its name property is equal to the entered NodepathKey value in the __Configuration.json__ and its Expression property is equal to the nodepath name of the node to execute.
 * the query value can contain additional parameter in the format `key1=value1&key2=value2...`

 [Back to README](../../../README.md)