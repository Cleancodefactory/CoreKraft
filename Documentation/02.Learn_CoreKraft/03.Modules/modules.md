<!-- header
{
    "title": "CoreKraft modules",
    "keywords": [ "modules", "separation", "component", "apps" ]
}
-->
# CoreKraft modules

## What are modules?

The main structural unit in a CoreKraft project is the **module**. Modules are meant to provide clear separation of the assets of different entities in a Kraft based system.

A module can relate to any entity of a Kraft system, depending on the programmer's choice. For example, there can be:
* App modules: They serve to isolate the assets for a BindKraft App.
* Service and component modules: These can be used in many Apps, and are common for all running entities in the BindKraft session.
...

Any request to the CoreKraft server must access a specific module. This rule is obligatory. Modules are identified by the request URL, in the following manner:
```uri
.../<module name>/<nodedefskey>/<nodekeys>
```

## Structure

Modules have their own directories. In a module directory, there are several sub directories, which follow a strict naming convention.

There are several subdirectories, which serve as containers for various types of assets: *Images*, *Css*, *Views*, *Templates*, *Scripts* and *Data*. Additionally, there is the *CustomPlugins* directory, which is meant to hold custom plugin implementations, and the *Localization* directory, which should hold configuration files, for the different supported locals.

```
Directory: Module1
    File: Dependency.json

    Directory: TreeNodes
        ...
    Directory: Localization
        File: Localization.en.js
        File: Localization.bg.js
    Directory: Images
        ...
    Directory: Css
        File: Module.dep (obligatory)
        ...
    Directory: Scripts
        File: Module.dep (obligatory)
        ...
    Directory: Views
        ...
    Directory: Templates
        ...
    Directory: Data
        File: Module.json (obligatory)
        ...
    Directory: CustomPlugins
        ...
```

Each module directory must contain a *NodeSets* subdirectory, which holds all the node definitions, in the following way:
```
 Directory: Module1
     ...
     Directory: NodeSets
         Directory: NodeSets1
            File: Definition.json
         Directory: NodeSets2
           File: Definition.json
        ...
```

Each folder inside *NodeSets*, represents a single node definitions file. The folder name is the same as the **nodedefskey**, which is specified in the URL, upon request.

[Back to README](../../../README.md)