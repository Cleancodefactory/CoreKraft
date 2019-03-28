<!-- header
{			
	"title": "CoreKraft Loaders explained",
	"keywords": [ "introduction", "overview", "history", "nature", "BindKraft", "Server-side", "micro-service", "net core", "plug-ins", "data", "sql", "json" ]
}
-->
# Test link to readme.md 
[a relative link](../../README.md)
[a relative link](Installation/installation.md)

# Behind the CoreKraft (Server side) model #

The CoreKraft model is constructing its output (Response) to suit the client side framework needs. To achieve this we keep the structure defined in a 'NodesDefinition'.   
The NodesDefinition is a simple JSON file that contains separate Nodes, organized in a tree-like manner. Each node can return 'Data', 'Views' and 'Lookups' (all together, or any of them). Nodes can also be configured to execute an external .dll called 'CustomPlugin'.  

CustomPlugins and their purpose and usage will be explained in another article. Every Node can be configured to use a different data source, defined in a 'datapluginname' property. We are supporting inline statements (sql or json), or files where more complex JSON or Sql queries can be written. One of the benefits of supporting files is that you can easily write and test your Sql queries in any external editor.  

Any NodesDefinition must have a base (Root) node, and you can define the structure inside its children. Here is a sample of what the NodesDefinition may look like:
```json
    {
		"nodesdefinition": {
			Root:{
				"lookups": [],
				"views": [],
				"customplugins": [],
				"children": [
					{
						"nodekey": "node1",
						"datapluginname": "Sqlserver",
						"islist": 1,
						"read": {
							"select": {
							  "sql": "select something from sometable"
							}
						},
						"write": {
							"insert": {},
							"update": {
							  "sql": "update sometable set somecolumn = @somecolumn where something = @something;"
							},
							"delete": {}
						},
						"lookups": [],
						"views": [],
						"children": [
							{
								"nodekey": "childnode1",
								...
							}
						]
					}
				]
			}
		}
	}
```

### Overview of the CoreCraft Server implementation ###
It is important to say that you can call the entire definition, or a specific (Startup) Node. What does this mean? Well this means that every Node can be configured and used as a 'Startup', and the benefit of this is that you can use different portions of the data to create different views. A more detailed explanation on this will be given in another article. Below you can find the description of the basic System Plugins that we have provided so far.


- **Data Iterator**  
Its purpose is to circle from the 'Startup' node, deep through its children recursively, to the very end of the chain, and to initialize and trigger the implementation of the registered Data Loaders.  
The provided DataIterator is the System default one, but the user can create their own, and can register and call it as an alternative if needed.


- **Data Loaders**  
The System provides a default Data Loaders implementation for database and file storage. Currently supported: Microsoft Sql Server, SqLite, JSON (will be extended in future versions). The User can create their own data loaders, and register them and use them for various data sources.


- **ViewLoader**
The System provides a default plain HTML ViewLoader (support for the razor view engine is planned for future versions).  
The user can create their own view loaders, and can register and call them as an alternative if needed.


- **LookupsLoader**
The Lookups (also known as nomenclatures), are basically lists of data that is used to populate any kind of lookup controls in the view.   
BindKraft (Client Side) for now provides Dropdown and Multiselect controls for this purpose. Because of its purpose, the Lookups data differs from the Data, and it is not treated as a tree, but a flat list structure.  
Also the Lookups data is cached on the Client (you can find more details in the Client-side documentation)
The System provides basic LookupLoaders for Sql Sever, SQLite, and plain JSON data (will be extended in future versions).  
The user can create their own view loaders, and can register and call them as an alternative if needed.


The user can create their own plugins of any kind, and register them in the 'appsettings.json' file. Here is an example:
```json
	"NodesDataLoader": [
		{
			"Name": "MyLoaderName",
			"ImplementationAsString": "Ccf.Ck.CoreWeb.System.Plugin.Loader.SqlServer.SqlServerImp, Ccf.Ck.CoreWeb.System.Plugin.Loader.SqlServer",
			"InterfaceAsString": "Ccf.Ck.CoreWeb.Plugin.Contracts.IDataLoaderPlugin, Ccf.Ck.CoreWeb.Plugin.Contracts",
			"Default": true,
			"CustomSettings": {
			"ConnectionString": "Data Source=MyServer;Initial Catalog=MyDB"
			}
		}
	]
```

[Back to README](../../../README.md)