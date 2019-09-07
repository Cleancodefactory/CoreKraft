<!-- header
{
    "title": "Server core details",
    "keywords": [ "introduction", "overview", "history", "nature", "BindKraft", "Server-side", "micro-service", "net core", "plug-ins" ]
}
-->
# Micro-Services Plugin-oriented Framework #

> Less is more

## Plugins-Overview ##
- ***Registration***:
The registration is done through Dependency Injection. The registered implementation and its life cycle will be cached as Type on multiple levels and therefore the creation of instances is fast.
- ****Availability****:
In the CoreKraft execution pipeline the developers have access to the registered plugin instances. It is quite handy to use the DI container as factory and not think about lifetimes.
- ***Access to parameters***
All plugins have access to the parameters through the interfaces they implement. The parameters are separated clearly in different groups and collected from different sources.
- ***Access to parent data***:
The Nodes-Data-Plugins have access to the data from their parent node. This is helpful especially in the custom plugins if they want to manipulate it. Another case is when the child node uses a value (e.g. database key) from the parent to limit the selection. Good example of this is when you want to return properly grouped car manufacturers with all their models (e.g. BMW -> Model 3, 5, 7...).

## Available Plugin types ##

### System-Plugins ###
What are System-Plugins? 
These plugins are registered in the appsettings.json and have to implement the ISystemPlugin interface.
Who is allowed to override the existing plugins? Everyone.
Good examples for such plugins are: ViewLoader, LookupLoader or the ResourceLoader. They deal with the loading of different resources.

### Nodes-Data-Iterator-Plugins ###
These plugins are registered in the appsettings.json and have to implement the IDataIteratorPlugin interface.
Who is allowed to override the existing plugins? Everyone.
The main task of theses plugins is to iterate through the data nodes and call the registered Nodes-Data-Plugins (e.g. see below). The core system comes with a default implementation for such an iterator which will move from parent to children and will call the actual registered data loaders.

### Nodes-Data-Plugins ###
These plugins are registered in the appsettings.json and have to implement the IDataLoaderPlugin interface.
Who is allowed to override the existing plugins? Everyone.
The main task of these plugins is to get the data from different sources. Usually you will have one loader for one type of data access: SQL-Server loader for SQL-Server, and SQLite loader for SQLite. The list can grow with the requirements.
The idea is that you have relatively small work entities which are configured and are responsible for addressing only one data source. Data source is used here non-restrictively and includes Web-Services as well.

### Custom-Plugins ###
These plugins are supported and implemented by the developers using CoreKraft. They have to implement the ICustomPlugin interface. They are declared in the DataNodes-Structure (e.g. the JSON description of the nodes) and have access to the request parameters, server parameters, and very importantly to the retrieved data of their parent node. This is pretty handy if you have to check the data, or make some manipulations on it.

[Back to README](../../../README.md)