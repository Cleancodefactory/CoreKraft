# Resolver libraries

Libraries containing resolvers are now supported (from March 2023) by CoreKRaft.

A library of resolver is loaded in similar way any plugins for CoreKraft are loaded:

- The library is loaded per module. I.e. it has to be configured in the Configuration.json if each module where it is used in some nodesets.

- The library is a class in an assembly. The same assembly may contain multiple classes containing resolvers. The methods that are resolvers are exported using internally initialized configuration that maps alias names to actual methods, only the configured aliases are exported and usable.

Currently the configuration cannot be modified (even partially) in the configuration.json of the module. Some options for disabling individual resolvers is planned for a future version of CK, but this is low priority feature, because it is not critical by any measure.

## Configure resolver library

Sample configuration with the unrelated parts of the file omitted:

```JSON

{
  "KraftModuleConfigurationSettings": {
    "NodeSetSettings": {
      "SourceLoaderMapping": {
        "NodesDataIterator": {
          //... omitted - other sections
          "ParameterResolvers":[
            {
              "Name": "PrefixName",
              "ImplementationAsString": "Ck.SysPlugins.LibX.Resolvers.ResolversClass, Ck.SysPlugins.LibX",
              "InterfaceAsString": "Ccf.Ck.SysPlugins.Support.ParameterExpression.Interfaces.IParameterResolversSource, Ccf.Ck.SysPlugins.Support.ParameterExpression",
              "CustomSettings": {
                // ... none are currently used by default
              }
            }
          ],
          //... omited - other sections
          // close all the brackets
```

The resolver libraries require one object per library in the section `ParameterResolvers`.

`Name` - is optional, but strongly recommended. If Name is specified any resolver in the library will be accessible as `PrefixName.ResolverName([args])`. So you can define the prefix the library is using ad they can be different in each module (probably  a good idea would be to use the same prefix everywhere).

`ImplementationAsString` - Ths specifies the two most important things

    - The class that implements the library

    - The assembly where this class is defined.

`InterfaceAsString` - While in future this may be optional, it is required for now, It specifies the interface that has to be implemented by each library and the assembly where the interface is defined. This is well-known for each kind of plugin and must contain exactly what is in the examples. This is also the reason why this setting may become optional in future.

Currently no other settings are supported for resolver libraries, but in future partial override of the internal settings could be.

## Implementation (C#)

A C# library project is required and at least one class in it that inherits from `ResolverSet`.

```CSharp
public class MyResloverLib : ParameterResolverSet
{
    public DbNameResolverImp() : base(new ResolverSet
    {
        Resolvers = new List<Resolver>()
        {
            new Resolver()
                {
                        Alias = "MyResolver",
                        Arguments = 0,
                        Name = "MyResolverImplementation"
                }
        }
    })
    {}

    public ParameterResolverValue MyResolverImplementation(IParameterResolverContext ctx, IList<ParameterResolverValue> args)
    {
        return new ParameterResolverValue("Hello World!");
    }
}

```

In the above example just one simple resolver is implemented. The classes that implement libraries (`also called resolver sets`) have to pass some configuration to the base constructor and has to have parameter-less constructor. So, the above approach is recommended.

    ResolverSet has the property Resolvers which is a List of Resolver class objects. This is the only important part of the configuration and each Resolver in the list defines 3 properties:

- `Alias` - the name under which the Resolver will be available (with prefix as specified in the configuration)

- `Arguments` - the number of arguments accepted by the Resolver. The number of arguments is exact and no optional arguments are allowed.

- `Name` - the name of the method exposed as a Resolver.


The Method(s) that implement resolvers in natural form expect two arguments:

- `Context` - the context `IParameterREsolverContext` which is always available and is needed if the particular resolver needs to access any information about the execution context and related data,

- `args` - IList of ParameterResolverValue List of the arguments. 

- The return value of the resolver is ParameterResolverValue too.

Other forms are also possible (if one makes implementation more convenient - it can be used). However, the natural form above can be always used in implementation, others are just alternative forms:

```CSharp
// With some arguments
public ParameterResolverValue MyResolver1(IParameterResolverContext ctx, ParameterResolverValue arg1, ParameterResolverValue arg2);

// With no arguments
public ParameterResolverValue MyResolver2(IParameterResolverContext ctx);

// If the context is not needed and no arguments are supported
public ParameterResolverValue MyResolver3()

```

## Why ParameterResolverValue ?

All resolvers us that ubiquitous type for arguments and return value. The reason for that is the role played by the resolvers: 

    A range of the built-in resolvers fetch some value from location defined in their arguments, but they give the programmer ability to compose a value by doing some calculations, string operations and so on. 

    What is produced by parameter expression using resolvers is a value that will be available for the Data Loader plugin forming a node. Depending on the specific data loader different operation is implemented over various kinds of data sources.

    So the parameters defined by the parameter's Expression setting are often thus prepared for the specific usage by the specific data loader.

### The needs of the specific data loaders

While these needs occur with variety of data loaders the database ones are still the best illustration what may be needed, why and how the resolvers help. So, we will be talking about ADO.NET based data loaders for relational databases, but you should not assume that the features we discuss here are specific only for them,

In some SQL databases one can pass as query/command parameters almost everything in others you wont be able to pass as command parameters things like table and field names needed by `order by` clause and sometime even parameters needed for paging (like offset and limit or equivalent). Another example would be usage of table typed parameters that may or may not be supported and even with support it may be undesirable to go that way and prefer a parameter which will create a simple IN clause. We can go further and point out that we can point at huge number of cases in which obtaining even table names for joins can help us create easier and more flexible solution.

Basically the above examples shows us that there is always some benefit in treating as parameters some elements of an SQL query even if they cannot natively be passed as such. CoreKraft's `ParameterResolverValue` type enables the value to be passed to the data loader with some optional information that gives the loader chance to create illusion that this is a parameter and not integral part of the query syntax. `ParameterResolverValue` can hold other usable information, but classification of the parameters is the most used feature - they can be normal values passed as command parameters, content parameters - these are not passed as command arguments, but are included as textual part of the query. Also the value can be marked as something to be ignored or as value which is unusable, because it was produced by expression that was wrong etc. Thus the data loaders can use this classification and decide how to use the value.

Imagine we have a weird query like:

```SQL

SELECT * FROM @table;

```

The table is obviously come from a parameter. If it is marked as `content` the parameter has to be replace with its value. The ADO based data loaders behave exactly like that. Among the built-in resolvers there are some that return values marked as `content` because they produce whole pieces of the query. This is quite obvious for order by composers, but is not so obvious in other cases. So, among the built in resolvers there are those that return values marked as `content` and those that mark explicitly value as `content`. The obvious downside is that some of these resolvers are SQL specific and some can be even specific for a database. 

Such needs are to be expected with data loaders that work with something that is done through a language. In other cases these markings are not needed or are used in a bit different manner - like data loaders dealing with WEB services for instance. Obviously a library of resolvers can be designed to work with a very specific data loader and built-in ones are for general or almost general cases.

## What resolvers to develop for your projects

1. Thanks to the option to create very specific resolvers for a project with already decided parameters one can create resolvers to generate certain SQL segments repeatedly used in the project'd queries. For example:

    - Generate data access control conditions to be included - they should have arguments giving the alias names of the tables to enable different aliasing in different queries,

    - Because other parameters can be used in resolver expressions one can define resolvers that determine if a given join or subquery is needed and another resolver that generates the necessary code if the other parameters meet some condition. This can be complicated, but compared with the need to have multiple queries for each possible case, it will be much easier.

    - Transformations and Calculations. Elements of the business logic can be implemented as resolvers if it cannot be implemented otherwise better. E.g. one often ha to decide between performing certain transformations in the database or outside. The storage can be managed through some WEB service which requires the data with the business logic applied,

    - Preparation of address/entry point and parameters. All the logic can be in resolvers and then used as parameters for data loader that consumes their product as ready-to-use requests/addresses/data block. Do not forget that parameter are not limited to simple types and may contain anything if the loader can work with it - avoiding this in typical RDBMS comes from their universalism which makes more practical to use simple solutions applicable for all systems and all case, but if custom solution is unavoidable then there is no reason to stick to context neutral solutions. So, a resolver can compose complex data structures if this work can help split the business logic in convenient way.



