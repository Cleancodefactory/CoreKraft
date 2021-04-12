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

## Standard libraries

The standard libraries contain both general purpose function and functions that perform actions through the context. The latter will often act differently or will not be available for all contexts. Check any notes in the list to make sure what the function does and if it is available.

### Common functions - available for all contexts

__Add( arg {, arg} )__ - Returns the sum of all the arguments.

> If called without arguments will return `null`.

> If there is even one `double` value among the arguments they will be summed after attempting to convert each of them to `double`. Otherwise they will be summed (and converted) as `int`.

> Conversion exceptions are possible. If you want to ignore exceptions see TryAdd.

.......... to be continued ...............

