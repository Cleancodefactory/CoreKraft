# Nodeset syntax

The Nodesets are defined in a first level subdirectory under the `Nodesets` directory of a `Module`.

```
Workspace
    Modules
        MyModule
            ... 
            Nodesets
            ...
```

Each Nodeset occupies a separate directory. Nodesets cannot be nested.

## Structure of a Nodeset

The Nodeset consists of at least a definition JSON file named `Definition.json`. The Nodeset can also contain a number of additional files refered in its Definition.json file using the `loadquery` or `file` values in action definitions (such as Select, Insert, Update and so on). When a file is referenced inside the definition the path to the file is assumed to start at the directory of the Nodeset (examples can be seen later in the document).

To help the reader understand let us say that the aim is to keep all the resources needed for the Nodeset definition in a single directory tree not mixing them with other assets. Thus the definition refers to other files as needed. All refered files can be specified inline, but this is usually extremely inconvenient. A good and frequent example is a long SQL query which will require writing it on a single line inside the JSON file if external file is not used. This affects also any kind of script or similar content processed by various plugins and is almost always specified as a file (using loadquery).

## The structure of the definition.

The definition of a Nodeset specifies a tree of nodes that starts from a single root node. In practice the root node is rarely used as part of the so defined service, instead it is most often just a holder for any number of child nodes which are the actual start points of the services defined by the nodeset.

... TODO CONTINUE THE JOKE

### Custom plugins

```JSON
"customplugins": [
    {
        "custompluginname": "MyPlugin",
        "afternodeaction": true
    },
    {
        "custompluginname": "MyPlugin1",
        "beforenodeaction": true
    },
    {
        "custompluginname": "MyPlugin1",
        "afternodechildren": true
    }
    ,
    {
        "custompluginname": "MyPlugin1",
        "beforenode": true
    }
            
]
```

The points in the execution process in which a custom plugin can be invoked are:

- beforenode - Before any node processing occurs. 

    For the read actions this is not only before the any results are generated, but even before start cycling over the parent results. The parent results are visible to the `beforenode` plugin as Results in its context. It is possible to add some results, just keep in mind that these are parent results and not results generated from the current node.

    For write operations the `beforenode` plugins see list of the results coming for write before any of them is processed. Useful actions at this point can be changing their states, adding some implicit records for store and so on.

    The context available to these plugins is:

    ```csharp
        TO DO

    ```

    `beforenode` plugins are most often used together with Prepare operations.

- beforenodeaction - before the operation performed by the data loader of the node.

    The context available in this phase contains the same properties and methods as the data loader's context. Thus the execution is almost the same as if the same code is implemented as data loader, the only notable difference at that point will be that the Results will be empty for the first custom plugin. If multiple custom plugins are registered to execute before node action the results will be empty for the first one and then will depend on what each plugin does.

    - These plugins can add results before the data loader produces any. This rarely makes sense for read actions, but can be very useful for writes when implicitly generated data has to be sent for actual writing for instance.

... TODO CONTINUE ...

### Prepare operation

For read actions: 

```JSON
"read"{
    "prepare": {
        "loadquery": "somefile.sql"
    }
}
"write"{
    "prepare": {
        "loadquery": "somefile.sql"
    }
}
```

The prepare operations are performed by the data loader of the node.