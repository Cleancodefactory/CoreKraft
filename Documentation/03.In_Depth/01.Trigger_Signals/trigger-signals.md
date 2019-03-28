<!-- header
{
  "title": "Trigger signals for the back-end",
	"description": "How to trigger signals for the back-end to react on environmental changes",
  "keywords": [ "signal", "mass calling nodes" ]
}
-->
# Signals in the context of CoreKraft #
## What are signals ##
CoreKraft (the server part of BindKraft) represents a coherent model of entities called `Modules`. A `module` defines a set of resources and actions. One `module` should cover a narrow piece of functionality and cover it good. A basic part of a `module` is the Nodeset, a hierarchical tree of nodes and on each, a different action could be executed. The node in the nodeset defines routing as well. The generated result is returned as JSON.

Let’s dissect a signal definition from `<ModuleName>Dependency.json`:
``` 
...
"signals": [
    {
      "key": "DeleteUser",
      "nodeset": "deleteuser",
      "nodepath": "tenant",
	    "maintenance": false
    },
    {
      "key": "UpdateTenant",
      "nodeset": "boardtenant",
      "nodepath": "activity",
	    "maintenance": true
    }
  ],
... 
```

The signals section defines one or multiple signals. Each is described by key, nodeset and nodepath. All rules for calling nodes from the browser apply with one exception: if the loadertype is not set the server sets a default to DataLoader.

So, what the heck when we can call the nodes directly?

The signals are meant to represent a kind of interface to the nodesets and nodes. Additionally, if a signal with the same key is defined in all modules you can call it for all of them.

## Phases to call a signal ## 

### OnSystemStartup or OnSystemShutdown ###
```
...
"SignalSettings": {
  "OnSystemStartup": [ "OnSystemStartup" ],
  "OnSystemShutdown": []
}
...
```

### From the build-in service ###
```
...
"HostingServiceSettings": {
  "Interval": 0,
  "Signals": [ "UpdateTenant" ]
},
...
```

### From the client ###
Example system configuration with:
a)	Server: `myserver.com`
b)	CoreKraft-Configuration entry point: `node`

Regular node calls:
```
(www)myserver.com/node/<read/write>/nodesetname/nodepath1.nodepath2?sysrequestcontent=ffff
```

Signal node calls:
Calling the signal with key `updatetenant` in module `board`:
```
(www)myserver.com/node/<read/write>/signal/board/updatetenant/
```

Calling the signal with key `updatetenant` over all currently included modules:
```
(www)myserver.com/node/<read/write>/signal/null/updatetenant/
```

Explanation:
The system will define a hard-coded section `signal`. The further segments are reserved for the module name and signal key.

[Back to README](../../../README.md)

