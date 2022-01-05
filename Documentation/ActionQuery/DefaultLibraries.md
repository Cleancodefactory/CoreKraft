# Standard libraries for ActionQuery in CoreKraft

There are 3 libraries currently loaded when ActionQuery host is created (can be suppressed if necessary).

```C#
var host = new ActionQueryHost<IDataLoaderReadContext>(IDataLoaderReadContext context,bool withoutlibs);

```

The first argument can be any of the `IDataLoaderReadContext`/`IDataLoaderWriteContext` or `INodePluginReadContext`/`INodePluginWriteContext`.

The second argument `withoutlibs` is optional and if `true` is passed the libraries described here are **`not available`**. This may be needed in some specific cases when a plugin wants to enable very specific scripts to work only with its own features without access to anything else.

The default libraries described further in this document are 3:

- `Variables library` - access to variables available for the running script and its host (same for all contexts)

- `Default libraries` - Currently a single general purpose set of functions for all kinds of operations: arithmetic, strings, dictionaries, lists and so on. In future additional libraries may be added. (same for all contexts)

- `Nodeset library` - Contains functions for querying and changing node data in Nodeset execution current node. (there are some minor differences between scripts running in DataLoader context and CustomPlugin context.)


## Variables library (Will be replaced by in-language feature soon)

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

**Equal(arg1,arg2)** - Compares the two arguments.
`arg1`, `arg2` are treated as follows:

- If any of the two is null `false` is returned.
- If any of the arguments is `float` or `double`, they are converted to `double` and compared.
- If any of the arguments is integer (any size, signed or unsigned), they are converted to int64 and compared.
- The arguments are converted to strings and compared - case sensitive.

**Greater(arg1, arg2)** - Compares the arguments and if arg1 is greater than arg2, returns true, otherwise returns false. See **Equal** for how the arguments are compared.

**Lower(arg1, arg2)** - Compares the arguments and if arg1 is smaller than arg2, returns true, otherwise returns false. See **Equal** for how the arguments are compared.

**Or([arg1 [, arg2 [, arg3 ... ]]])** - Returns true if any of the arguments is a truthy value, otherwise returns false.

**And([arg1 [, arg2 [,arg3 ...]]])** - Returns true if all of the arguments are  truthy values, otherwise returns false.

**Not(arg1)** - Inverts the truthiness of the argument. Returns a boolean result.

**IsNull(arg1)** - Returns true if the argument is null, otherwise returns false.

**NotNull(arg1)** - Returns true if the argument is not null, otherwise returns false.

**Slice(string, start [,end])** - Returns a string containing the characters from the original `string` starting at `start` and ending at `end` (not including the end). If end is omitted, returns to the end of the string.
                    
**Length(string | List | Dict)** - Depending on the type of the argument, returns the length of the string, list or dictionary.

**Replace(string, findwhat, replacewith)** - Replaces all the occurrences of `findwhat` in the `string` with the string passed in `replacewidth`.

**RegexReplace(string, pattern, replacewith)** - Replaces substrings matching the `pattern` in the `string` with the `replacewith`. The pattern uses the C# regular expression syntax. All arguments are converted to strings if they are other types. `Returns`: the resulting string.

**Split(string [, separator])** - Splits the string using the specified separator or using "," if it is omitted. The result is an AC list (see List functions).

**Trim(string)** - Trims the string and returns the resulting string.

**EncodeBase64(arg)** - Converts the to string and encodes it to base64 which is returned as string.

**DecodeBase64(arg)** - Converts the argument to string and decodes it from base64 to UTF8 string.

**MD5(arg)** - Converts the argument to string, then treating it as UTF8 string converts it to bytes. The hash is calculated from those bytes and returned as string containing the resulting has in bytes listed in hexdecimal (two digits per byte).

#### List functions

The default library supports work with lists which are internally represented as `List<ParameterResolverValue>` classes. This means that any lists received from outside (from node parameters for example) must be converted to internal lists (we will call them AC lists below) before the other functions can be used on them.

There are two kinds of AC lists which are completely interchangeable for internal use, but impact when data created internally has to be send outside after conversion with `ToNodesetData`. The need comes from the way CoreKraft processes NodeSets. This is explained in detail in the ToNodesetData function below.

**List([arg1 [,arg2 [,arg3 ...]]])** - Creates and AC list with the arguments added as items in the list. With no arguments it will create an empty list. `Returns`: the created AC list.

**ValueList([arg1 [,arg2 [,arg3 ...]]])** - The same as `List`, but the created AC list is marked as one containing values. See ToNodesetData for details.

**ConsumeOne(list)** - Works with AC lists and with `Queue<ParameterResolverValue>`. The latter are not directly supported by the default library, but can be produced by functions in a hosting plugin or another library.

When the argument is an AC list removes the last argument of the list and returns it. If the list is empty returns null. Can be used efficiently in `while` cycles to consume an entire list.

When used with queue, dequeues one element and returns it. If the queue is empty returns null. Can be used efficiently in `while` cycles.

**ListAdd(list[, arg2 [,arg3 [,arg4 ...]]])** - Adds items at the end of an AC list. `Returns`: the AC list.

**ListGet(list,index)** - Gets the element ar `index` and returns it. If the `index` is out of range returns `null`.

**ListInsert(list, index, value)** - inserts an element in the AC list at the specified index. Will fail only if the underlying List.insert method fails. Use it with the same assumptions. Returns: the AC list.

**ListRemove(list[ , index1 [,index2 [,index3 ...]]])** - removes the element(s) at the specified indexes. The indexes are the positions before doing any removal. Returns: the modified AC list.

**ListSet(list, index, value)** - sets an element in an AC list. The index must exist in the AC list or an exception will occur.

**ListClear(list)** - clears the AC list and returns it.

**AsList(value)** - Converts an externally obtained list-like object into AC list. When receiving enumerable values from a node parameter or from function calling something external they may contain all kinds of values and they need to be packed into `ParameterResolverValue` objects in order to be used internally. AsDict does exactly that.

This function can be also used to create a copy of an existing AC list. Just call AsList(`list`) and the result will be a copy of `list`.

This function accepts dictionaries as an argument. The produced AC list will contain only the values from the dictionary. The dictionary can be AC dictionary too.

**AsValueList(value)** - The same as `AsList`, but marks the list as list of values. See `ToNodesetData` for more details.

#### Dictionary functions

The default library supports work with dictionaries which internally are represented by `Dcitionary<string, ParameterResolverValue>` classes which we will call AC dictionaries where appropriate. Most functions work with this kind of dictionaries and you need to convert any dictionaries coming from the outside to the internally supported type. This is done with `AsDict` function described below.

This means that if an external parameter contains some dictionary data you have to pass it through `AsDict` in order to use functions like `DictGet`, `DictSet` and so on.

**Dict([key, value [, key, value [, key, value ...]]])** - Creates an AC dictionary. Can be used without arguments to create an empty one or with pairs of arguments to add elements on creation. The argument pairs are [`key`, `value`] which means that the key will be converted as string and will be the key of the next argument in the dictionary. This pattern is used in all the dictionary functions below where appropriate. `Returns`: the new AC dictionary.

**DictSet(dict, [key, value [, key, value [, key, value ...]]])** - sets elements of an AC dictionary. The argument `dict` is the dictionary, wile after it any number of pairs [`key`, `value`] can be specified. Each pair will set an element in the dictionary, if an element with that key does not exist it will be created, if it exists its value will be set with the new one. `Returns`: the same AC dictionary passed with the `dict` argument with the changes applied to it.

**DictGet(dict, key)** - Gets a value of an element in an AC dictionary. The `dict` is the dictionary to look into, the `key` is the key to look up. If the key is missing `null` wil be returned. `Returns`: the value of the key.

**DictClear(dict, key)** - Clears an AC dictionary And `returns` it.

**DictRemove(dict [, key [, key,[key ... ]]])** - Removes elements from an AC dictionary, The first argument is the AC dictionary, after it any number of keys can be specified. All the specified keys are removed from the dictionary. `Returns`: the dictionary.

**DictRemoveExcept(dict [, key [, key ...]])** - Clears all the values in the dictionary except those listed after the first argument, which is the dictionary to clear.

**AsDict(value [, value2])** - Converts external object to AC dictionary. Used most often to convert dictionaries obtained from node parameters into usable AC dictionaries. Can be used in two ways:

> **single argument**: The argument must be a dictionary-like value. Converts the external dictionary to AC dictionary. Used

> **two arguments**: Both arguments must be enumerable. The first one is expected to contain the keys and the second to contain the values for the AC dictionary that will be created from them. The arguments will be treated as parallel lists and if certain element of the first one (the one specifying the keys) contains a null element, the corresponding value will not be added in the AC dictionary because `null` is not usable as a key. 

`Returns`: The constructed AC dictionary.

The function does not throw exceptions for inappropriate arguments and will return an empty dictionary in such a case. To determine if it will work with certain arguments use `IsDictCompatible` with the same arguments in some kind of logical construct (`if` for example) and deal with the situation appropriately.

**IsDictCompatible(value [, value2])** - Checks if AsDict can succeed with the same arguments. See `AsDict` for more details about the arguments.

#### Combined Dictionary / List functions

**NavGet(dict | list [, key | index [, key | index ...]])**

**ToNodesetData(value)**

**ToGeneralData(value)**

**Error(code , text)**
**Error(text)**

**IsError(value)**

**ErrorText(error)**

**ErrorCode(error)**

### NodeSet library

Contains additional set of functions enabling the scripts to implement useful functionality in the context of the currently executing nod of the NodeSet.

Please check the contexts in which the functions are available. Some of them are applicable only in certain cases - e.g. result manipulation is different on read and on write. When a function is not available this will cause a corresponding error.

**NodePath()**

**NodeKey()**

**Action()**

**Operation()**

**AddResult( `dict | ( key, value [key, value [, key, value ...]])` )**

**HasResults()**

**SetResult(`dict | ( key, value [key, value [, key, value ...]])`)**

**ClearResultExcept(`dict | ([key [, key ...]])`)**

**CSetting(`name [,default]`)**

**CSettingLoader(`name [, default]`)**

**ModuleName()**

**ModulePath(`[combinepath]`)**

**ResetResultState()**

**SetResultDeleted()**

**SetResultNew()**

**SetResultUpdated()**


