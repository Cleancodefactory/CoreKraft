# Standard libraries for ActionQuery in CoreKraft

There are two libraries currently loaded when ActionQuery host is created without the option to exclude them.

```C#
var host = new ActionQueryHost<IDataLoaderReadContext>(IDataLoaderReadContext context,bool false);

```

The first argument can be any of the `IDataLoaderReadContext`/`IDataLoaderWriteContext` or `INodePluginReadContext`/`INodePluginWriteContext`.

The second argument is optional and if true is passed the libraries described here are **not available**.

The main library offers slightly different functionality depending on the context in order to offer what's actually needed in each case. 
The majority of the functions are the same in all cases - check the notes for each function for specifics. If not marked otherwise the
function is available in all contexts.

## Variables library

_This library is available in all contexts without any context specific differences._

### Functions

**Get(varname)** - Returns the value of the `varname`.

> varname - a string, the name of the variable.

> returns the value of the variable or `null` if the variable is not defined.

__Set(varname, value *{, varname, value })__ - Sets the `varname` to the specified `value`.

> varname - a string, the name of the variable.

> value - the value to set.

> Can accept multiple pairs of `varname`/`value` to set multiple variables at once.

> returns the number of variables set.

__Undefine(varname *{, varname})__ - Unsets/undefines the `varname`.

> varname  - a string, the name of the variable.

> Can accept multiple variable names to unset them all at once. Will not fail if a variable does not exist.

> returns the number of variables.

**Inc(varname)** - Increments a variable.

> varname - the name of the variable to increment

> returns the new value.

> The value of the variable is converted to `int` and then incremented. The new value is both returned and set back to the variable. If the variable contained another type it will be replaced with `int`.

**Dec(varname)** - Decrements a variable.

> varname - the name of the variable to decrement

> returns the new value.

> The value of the variable is converted to `int` and then decremented. The new value is both returned and set back to the variable. If the variable contained another type it will be replaced with `int`.

### Variables Library notes

**Inc** and **Dec** can be used in `while` and `if` constructs as a condition, when they reach 0 they become a falsy value with the corresponding effect on the constructs.

## Default libraries

The default libraries contain both general purpose function and functions that perform actions through the context (a node in a NodeSet). The latter will often act differently or will not be available for all contexts. Check any notes in the list to make sure what the function does and if it is available/usable in the particular context. So, default libraries are considered as consisting of basic and NodeSet oriented functions:

### Basic library

Provides basic general purpose functions for arithmetic, logical operations and string manipulation needed by most scripts. 

_The basic library cannot be loaded separately, it is loaded as part of the NodeSet plugins library_


__Add( arg {, arg} )__ - Returns the sum of all the arguments.

> If called without arguments will return `null`.

> If there is even one `double` value among the arguments they will be summed after attempting to convert each of them to `double`. Otherwise they will be summed (and converted) as `int`.

> Conversion exceptions are possible. If you want to ignore exceptions see TryAdd.

**TryAdd(arg {, arg} )** - Like Add, but will return null instead of throwing an exception if the conversion fails.

**Concat(arg {, arg })** - Returns the arguments turned to strings and concatenated in the order in which they appear.

    If any argument is null, it will be treated as an empty string.

**Cast(type, arg)** - Returns the `arg` casted to the type specified by `type`.

> **type** is a string and can be `string`, `bool`, `int`, `long` or `double`. If the conversion is impossible the result will be for string - empty string, bool - false, int, long and double - 0.

**GSetting(settingName)** - returns the value of a global setting. Only a small number of global settings can be queried. These are typically defined in the `appsettings*.json` file.

    Currently supported settings: EnvironmentName, ContentRootPath, ApplicationName, StartModule, ClientId
        
**Throw(text)** - Throws an exception with the given description.

> This function enables the AC script to bail out and cancel the node and NodeSet processing intentionally.

**IsEmpty(arg)** - Checks if a string is empty. Does not attempt to convert the argument to string, as result any non-string value will be considered to be empty.
                    
**TypeOf(arg)** - Returns the name of the type contained in the argument. Recognizes only these types:
> `null`, `string`, `int`, `long`, `double`, `short`, `byte`, `bool` and everything else is `unknown`. The numeric types are both - the signed and the unsigned versions (no distinction).

**IsNumeric(arg)** - Returns true if the argument contains a numeric value.

**Random([min [, max]])** - Generates a random integer. If min is specified the integer is at least min or greater. If also max is specified the generated number is equal or greater than min, but lower than max.

**Neg(arg)** - Returns the argument with inverted sign if the value is numeric and null otherwise. The numeric values are converted to double if they are float or double, to Int32 if they are integers shorter or same length as int, Int64 it the are longer. It is not recommended to use this with unsigned numbers, because of the chance to lose precision or convert them wrongly, usage with unsigned numbers should be done with care and will be safe only if they are smaller than the signed integer of the same size.

**Equal(arg1,arg2)**

**Greater(arg1, arg2)**

**Lower(arg1, arg2)**

**Or([arg1 [, arg2 [, arg3 ... ]]])**

**And([arg1 [, arg2 [,arg3 ...]]])**

**Slice(stting, start [,end])**
                    
**Length(string)**

**Replace(string, findwhat, replacewith)**

**RegexReplace(string, pattern, replacewith)**

**Split(string [, separator])**

**List([arg1 [,arg2 [,arg3 ...]]])**

**ConsumeOne(list)**

**ListAdd(list[, arg2 [,arg3 [,arg4 ...]]])**

**ListGet(list,index)**

**ListInsert(list, index, value)**

**ListRemove(list[ , index1 [,index2 [,index3]]])**

**ListSet(list, index, value)**

**ListClear(list)**

**AsList(value)**



### NodeSet plugins library

Contains the Basic library and additional set of functions enabling the scripts to implement useful functionality in the context of a NodeSet.

TODO:


