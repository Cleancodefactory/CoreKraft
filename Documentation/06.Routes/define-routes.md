<!-- header
{
    "title": "Define system routes",
    "description": "Define data routes in configuration.",
    "keywords": [ "Routes", "Data routes" ]
}
-->

# Define routes

## Overview
CoreKraft is customizable and the routes are defined in appsettings.[Development|Production]config. 

## Configuration

![Routes defined in configuration](/Documentation/Images/Routes_appsettings.png)

In configuration section **KraftGlobalConfigurationSettings:GeneralSettings** there are 6 settings which are directly or indirectly responsible for the routings:
**"KraftUrlSegment"**: main entry point of the whole application,
**"KraftUrlCssJsSegment"**: entry point for the resources (e.g. css and js) in production mode,
**"KraftUrlResourceSegment"**: entry point for the resources of each module,
**"KraftUrlModuleImages"**: name of the folder inside of a Kraft-Module for the images,
**"KraftUrlModulePublic"**: name of the folder inside of a Kraft-Module for other public assets,
**"KraftRequestFlagsKey"**: flags directing CoreKraft to return only types of data (e.g. data or views or all)


### KraftUrlSegment
CoreKraft defines an entry point and binds it to the modules' execution points. The modules in CoreKraft are containers for templates in form of html, css, js and execution code in form of nodesets. The value in the **KraftUrlSegment** defines the general entry point or virtual path for all modules. Afterwards the name of the module (in lower case) separates the sub parts of it. 
Example: 
```uri
https://domain.com/node/...
```

### KraftUrlCssJsSegment
In Production mode CoreKraft combines all resources of one type (css|js) respecting the dependency tree of referenced modules. It creates the dependency tree, orders the modules and walks through its *.dep files reading the referenced css or js files. It caches the collected content, calculates an ETag and applies some transformations (e.g. minifications or other optimizations) before it returns it to the client. This is done once during the application's start up process. The value in the **KraftUrlSegment** defines the virtual path under which these resources will be returned to the client.
Example: 
```uri
https://domain.com/res/css?SVdc-gniHURnRqaA7GrWpA
```

### KraftUrlResourceSegment
The value in **KraftUrlResourceSegment** defines a virtual path segment under **KraftUrlSegment** which is used for delivering of images and other public resources. You as a developer have the ability to use CDN (Content Delivery Network) for your static resources.

Example: 
```uri
https://domain.com/node/raw/modulename/images/image.png
```

### KraftUrlModuleImages
The value in **KraftUrlModuleImages** defines the physical path under a module where CoreKraft will search for the requested image. For more details please see [static resources.](/Documentation/02.Learn_CoreKraft/06.Routes/static-resources.md)

### KraftUrlModulePublic
The value in **KraftUrlModulePublic** defines the physical path under a module where CoreKraft will search for the requested asset. For more details please see [static resources.](/Documentation/02.Learn_CoreKraft/06.Routes/static-resources.md)

### KraftRequestFlagsKey
The value in **KraftRequestFlagsKey** defines the name of a GET parameter, the value of which CoreKraft will use to determine what kind of data has been requested. Its omission will default to data, which means that only data will be returned.
Possible values:
```javascript
        None = 0x0000,
        ViewLoader = 0x0001,
        ResourceLoader = 0x0002,
        LookupLoader = 0x0004,
        CustomPlugin = 0x0008,
        DataLoader = 0x0020,
        All = 0xFFFF
```     

```uri
Example: https://domain.com/node/modulename/nodesetname/nodekey?sysrequestcontent=20
```