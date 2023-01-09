<!-- vscode-markdown-toc -->
* 1. [Structure of a Nodeset](#structure-of-a-nodeset)
* 2. [The structure of the definition.](#the-structure-of-the-definition.)
    * 2.1. [Custom plugins](#custom-plugins)
    * 2.2. [Prepare operation](#prepare-operation)
    * 2.3. [Conditional break/continue while executing nodeset](#conditional-break/continue-while-executing-nodeset)
        * 2.3.1. [Node level ContinueIf/BreakIf](#node-level-continueif/breakif)
        * 2.3.2. [Read/Write level ContinueIf/BreakIf](#read/write-level-continueif/breakif)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->




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

##  1. <a name='structure-of-a-nodeset'></a>Structure of a Nodeset

The Nodeset consists of at least a definition JSON file named `Definition.json`. The Nodeset can also contain a number of additional files refered in its Definition.json file using the `loadquery` or `file` values in action definitions (such as Select, Insert, Update and so on). When a file is referenced inside the definition the path to the file is assumed to start at the directory of the Nodeset (examples can be seen later in the document).

To help the reader understand let us say that the aim is to keep all the resources needed for the Nodeset definition in a single directory tree not mixing them with other assets. Thus the definition refers to other files as needed. All refered files can be specified inline, but this is usually extremely inconvenient. A good and frequent example is a long SQL query which will require writing it on a single line inside the JSON file if external file is not used. This affects also any kind of script or similar content processed by various plugins and is almost always specified as a file (using loadquery).

##  2. <a name='the-structure-of-the-definition.'></a>The structure of the definition.

The definition of a Nodeset specifies a tree of nodes that starts from a single root node. In practice the root node is rarely used as part of the so defined service, instead it is most often just a holder for any number of child nodes which are the actual start points of the services defined by the nodeset.

... TODO CONTINUE THE JOKE

###  2.1. <a name='custom-plugins'></a>Custom plugins

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

###  2.2. <a name='prepare-operation'></a>Prepare operation

For read and/or actions: 

```JSON
"read"{
    "prepare": {
        "loadquery": "somefile.sql"
    },
    "select": { ... }
}
"write"{
    "prepare": {
        "loadquery": "somefile.sql"
    },
    "insert": { ... },
    "update": { ... },
    "delete": { ... }
}
```

The prepare operations are performed by the data loader of the node if it supports them. ADO based plugins (all DB plugins) support `Prepare` for instance and they are the most obvious case where such operations are needed sometimes.

> One of the best examples for usage of `Prepare` is the case when one wants to remove all relevant records from database table and then (re)create those that should exist. For simple relation tables this is always a tempting solution, but without `prepare` the only reasonable way to do it is to perform the removal in the parent node as part of the work it does.

> The problem with such a solution is that the parent node take care for two separate concerns and also the construct can be executed only from the parent and executing the node that removes and creates the relations is impossible to call separately. Prepare enables this mass removal to be part of the node that (re)creates the records - just execute the removal query before the other work. So, in real life you will likely have a DELETE statement in `prepare` and then insert will be executed.

`Prepare` has some specifics - it does not receive the same data as the other operations. This means that neither GetFrom('current',name) will not work in write operations and some other resolvers will also have problems. This is a new feature and specialized resolvers will be added to enable more flexible solutions. For now GetFrom('parent',name) and GetFrom('data') or GetFrom('client') are the most useful resolvers. Still there are limitations and issuing correct exceptions is not yet fully implemented. Usage of `Prepare` in read operations is rarely useful and its support for resolvers is minimal compared to write operations. With read operations in `Prepare` query only parameters obtained from `data`, `client` and built-in data (like GetUserId and similar) should be used at this time. Future improvements will enable more parameters to be obtained.

Let's try to illustrate the typical case described above.

The data being written looks something like this:

```Javascript
{
    id: 1,
    name: "Item name",
    related: [
        { relatedid: 3},
        { relatedid: 5}
    ]
}

```

To keep things brief ... TODO: continue 

###  2.3. <a name='conditional-break/continue-while-executing-nodeset'></a>Conditional break/continue while executing nodeset

As we already noted nodesets enable the programmer to request a whole tree of data with a single request by starting at some node of the nodeset and continue down the tree until it finished either because there are no more child nodes to execute or because no data is available for the further child nodes (either data to store or data to fetch).

This is obviously a base on which one can plan some parts of a nodeset in such a way that the actual processing will stop at certain point. By design and as common practice this is driven by the data. However this kind of usage may become quite complicated, requiring you to add ActionQuery scripts (or full plugins) that create/not create/remove the data needed to drive the processing in the right direction. 

The conditional break/continue options enables an easy way to control the execution of the nodeset. Still, both approaches have their place and it depends on the scenario which one of them or both should be used.

Syntax example:

```JSON
{
    "nodekey": "child1",
    "IsList": 0,
	"ContinueIf": "b",
	"parameters": [
		{ "name": "x", "Expression":"GetFrom('filter,client', name)"},
		{ "name": "y", "Expression":"GetFrom('filter,client', name)"},
		{ "name": "b", "Expression":"GetFrom('filter,client', name)"}
	],
    "read": {
        "parameters": [
            { "name": "c", "Expression":"Equal(GetFrom('parent', name),'ccc2')"}
        ],
        "ContinueIf": "c",
        "select": {
            "query": "... some query ..."
        }
    }
}
```

We are going to use the above example to explain the `ContinueIf` and `BreakIf` parameters.

The exact query and even the data plugin forming the node are not important for our purposes right now.

In the place of `ContinueIf` parameter above we can place `BreakIf` to achieve the reverse functionality.

####  2.3.1. <a name='node-level-continueif/breakif'></a>Node level ContinueIf/BreakIf

These are specified outside `read` or `write` sections and their value is a string, specifying the name of a parameter available at that level. The parameters in the read/write sections **will not be available for them**, also attempts to access parent values will cause exception.

The parameter specified is resolved and if its value is truthy:
- `ContinueIf` will allow the node to be executed, if it is falsy the node will not be processed.
- `BreakIf` will act in reverse - allowing the node to execute if the value of the parameter is falsy and will break the execution if it is truthy.

**In the result the property corresponding to the node will be missing for `read operations` and will not store and update the data in write operations.**

Special attention requires the write operations, because they will send to the nodeset a tree which can be partially processed depending on the ContinueId/BreakId options used in one or more nodes. This is the intended effect - to be able to skip parts of the data processing conditionally. The client application can send the data blindly and count on the options to limit the processing to certain depths or it can also cut the data that should not be processed and send only what's needed. Of course something in the middle is also possible and through `ContinueIf`/`BreakIf` the actions can be balanced between optimization and flexibility.

Obviously it is not always possible to know for the client what will and what will not be processed on the server side - it may depend on the logic running on the server. The very need for this feature came about from the wish to avoid forcing the client to know what exactly happens on the server in order to pass the right pieces of data. Through these options the client can be completely or partially unaware and leave to the server make the decisions.

Scenarios in which this is likely to be needed can be UI that shows a big data tree read from the nodeset, but capable of updating it piece by piece. Without break/continue features this will be limited to pieces starting from a point in the tree, but always finishing with its leaves. While possible to cut the unnecessary data in order to instruct the nodeset to process the sub-tree only "up to that point", this requires the client to know what happens on the server. With the continue/break options the client has the option to send a chunk of the tree, but the nodeset to decide to store it up to the point determined by the specified conditions in some nodes.

It is true that this may stimulate developers to send more data than actually needed, but leaving the server to decide means the solution can be more flexible and needing none or at least much less synchronization between client and server.

####  2.3.2. <a name='read/write-level-continueif/breakif'></a>Read/Write level ContinueIf/BreakIf

These act in similar fashion like the others, but they are executed during the normal node processing and thus have access to any parameter, including parent generated/updated values.

They are checked in each node iteration, which means:

- For read - they can stop/allow producing data for each result of the parent node.
- For write they can allow/disallow processing of each individual result/object.

What this means in practice:

> Read - imagine the parent node generates 10 results (IsList is 1). Some of them can be skipped by a child node - i.e. no further data extracted for some of them.

> Write - from an array of results (IsList is 1) some will be stored, others will not, but not because of the state of the particular result, but based on custom conditions.


TODO: Some concrete examples will help novices to learn useful CoreKraft techniques and how they can be simplified or extended with break/continue features.

