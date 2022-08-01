# ActionQuery (AQ) in CoreKraft

CoreKraft includes support for a very simple scripting handled by plugins. It is based on [ActionQuery](https://github.com/Cleancodefactory/ActionQuery) library and is available for NodeSet plugins implemented for specific projects as well.

The support for AQ is in the form of a host class any plugin can create and execute scripts against it. Additionally there are default libraries of functions and additional libraries (files and images functions for example) which can be included in the host and made available to the scrips when necessary.  See details on how to use implement hosting in [Hosting ActionQuery in CoreKraft](AQHosting.md).

Ready-to-use `Scripter` and `NodeScripter` nodeset plugins are provided for use whenever AQ scripting is enough (i.e. there is no need of a custom plugin to provide additional features). See [Using scripter plugins](ScripterPlugin.md) for how-to instructions.

## Quick links

* [Default libraries](DefaultLibraries.md) - Libraries always available to `Scripter` and `NodeScripter` plugins.

* Built in libraries (included when configured): [basicimage](Libs/basicimage.md), [basicweb](Libs/basicweb.md), [files](Libs/files.md), [internalcalls](Libs/internalcalls.md)

* [Useful scripts and examples](UsefulScripts.md)

* [MetaInfo access](ExecutionMetaInfo/ExecutionMetaInfo.md)

## What is ActionQuery and what is its purpose

TODO:

Read more [here](AQPurpose.md).

## ActionQuery syntax

The ActionQuery language is a minimal algorithmic language that requires almost no learning and can be hosted easily by .NET classes. Its syntax is explained below, everything else is functions provided to the ActionQuery processor by the host or from attached libraries. In both cases all the functions available in the language are just methods implemented on a class.

A few minimal extensions to the syntax are planned, but other than that the very idea of ActionQuery is to provide minimalistic environment for automation based on already existing functions provided from outside.

### Overview

ActionQuery does not support definition of functions/procedures. It is a plain text and is executed from top to bottom. 

Everything returns a result. Literals are obviously their values (e.g. a string literal 'abc' returns a string containing 'abc'), the built-in operators look syntactically like functions and behave the same way - they return values. Thus the basic unit of the language is a statement returning a result. The entire program can be viewed as a list of statements passed as arguments to an imaginary function. There are no unary or binary operators in ActionQuery, instead everything is a function.

### Formal syntax

```ebnf

program = [ statement {"," statement } ] ; The program can be empty

statement = literal | node_parameter | function_call | operator_call ;

literal = number | string | boolean | null ;

node_parameter = identifier ;

identifier = alpha { alphanumeric | "_" } ;

function_call = [ identifier ] "(" program ")" ;

operator_call = ( "if" "(" statement "," statement [ "," statement ] ")" ) |
                ( "while" "(" statement "," statement ")" ) ;

number = integer | float ; These are self describing - no exponents are supported for floats

string = "'" { character } "'" ; ' is escaped with \', \n \t \r \" are recognized

boolean = "true" | "false" ;

null = "null" ;

```

#### operators

**`if (condition, then_statement [, else_statement ])`** - depending on the `condition` executes `then_statement` or `else_statement`.

The condition is treated as truthy or falsy value according to the usual expectations.

The returned result is the return result of the executed statement (then_statement or else_statement). If else_statement is omitted and the condition is falsy `null` is returned by the if operator.

Examples:

All the examples use an imaginary function Output that outputs its argument value somewhere where we can see it. This function DOES NOT EXIST in the libraries of CoreKraft it is for example purposes only!

```
Output ( if ( 1 , 'hello', 'bye') )
```
This will output 'hello'

**`while (condition, statement)`** - will execute the statement continuously while the condition is truthy. Will return null no matter how many times the statement is executed.

For example:

```
while ( 1, Output('hello'))
```

will output 'hello' forever.

to do something sensible with while most often variables are needed. So, if we assume the variables library is loaded we can do something like this.

```
Set('a', 5),
while ( 
    Set('a', Sub(Get('a'), 1)),
    Output ('hello')
)
```

This will output 'hello' 4 times.

**`compound statement`** - This is not actually an operator, it is a syntax enabling multiple simple statements to be executed one after another.

Example:

```
(
    Output('hello'),
    Set('a', 5),
    if (
        Greater(Get('a'), 3),
        Output('a is greater than 3')
    ),
    SomeFunction()
)
```

All these will be executed in the order they appear, but what about the result of the entire compound statement?

**The compound statements return the result of the last statement they contain.** - in the example this will be the result of SomeFunction().

### Functions

ActionQuery does not have any functions built into it. All the functions that enable the script to do something useful have to be supplied by the host. CoreKraft provides a number of function libraries - see [Scripter](ScriperPlugin.md) and [Hosting ActionQuery in plugins](AQHosting.md) for more information how to use the libraries.

## Libraries supplied by CoreKraft

[basicimage](Libs/basicimage.md)

[basicweb](Libs/basicweb.md)

[files](Libs/files.md)

[internalcalls](Libs/internalcalls.md)

To use any or all of the libraries specify their names (see above) as a comma separated list in the plugin configuration in configuration.json. E.g.

```JSON

        {
              "Name": "Scripter",
              "ImplementationAsString": "Ccf.Ck.SysPlugins.Data.Scripter.ScripterImp, Ccf.Ck.SysPlugins.Data.Scripter",
              "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.IDataLoaderPlugin, Ccf.Ck.SysPlugins.Interfaces",
              "Default": true,
              "CustomSettings": {
                "libraries": "basicweb,files,internalcalls,basicimage",
                ... other settings ...
              }
        }

```

The same syntax applies to NodeScripter too.

There must be no spaces in the string!

## Useful resolvers for AC scripts

CurrentData - TODO: describe it.
## Using the ActionQuery support in externally written plugins.

TODO:


