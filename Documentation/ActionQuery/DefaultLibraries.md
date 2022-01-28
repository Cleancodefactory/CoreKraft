# Standard libraries for ActionQuery in CoreKraft

There are 3 libraries currently loaded when ActionQuery host is created (can be suppressed if necessary). When hosting the ActionQuery in a plugin:

```C#
var host = new ActionQueryHost<IDataLoaderReadContext>(IDataLoaderReadContext context,bool withoutlibs);

```

The first argument can be any of the `IDataLoaderReadContext`/`IDataLoaderWriteContext` or `INodePluginReadContext`/`INodePluginWriteContext`.

The second argument `withoutlibs` is optional and if `true` is passed the libraries described here are **`not available`** except the variables library but it will be accessible only by the variable set/get syntax (`$varname(value)` - sets variable `$varname` - gets variable).

This may be needed in some specific cases when a plugin wants to enable very specific scripts to work only with its own features without access to anything else. See [hosting ActionQuery](AQHosting.md) for more details.

The default libraries described further in this document are 3:

- `Variables library` - access to variables available for the running script and its host (same for all contexts). This library is always included, but when the host is created without default libraries, the functions of the library are not available, while the variables remain accessible through the Action Query variable set / get syntax (`$varname(value)` - sets variable).

- `Default libraries` - Currently a single general purpose set of functions for all kinds of operations: arithmetic, strings, dictionaries, lists and so on. In future additional libraries may be added. The scripts can hold in values and variables anything, but the Dict and List types are manageable by this library. Most functions from optional libraries work with them when possible.

- `Nodeset library` - Contains functions for querying and changing node data in Nodeset execution current node. (there are some minor differences between scripts running in DataLoader context and CustomPlugin context.)


## Variables library (Get/Set are equivalent to the set get variable syntax)

_This library is available in all contexts without any context specific differences._

### Functions

**Get(varname)** 

Returns the value of the `varname`.

varname - a string, the name of the variable.

returns the value of the variable or `null` if the variable is not defined.

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

From ActionQuery v1.1 the dedicated variable access syntax accesses the same variables as Set/Get/Inc/Dec functions. E.g. Set('a',123) will do the same as $a(123), also $i(0),Inc('i') will work with the same variable and after Inc it will be equal to 1 in this example.

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

**IsList(arg)** - Checks if arg is a List created by List or ValueList or returned from another function. `Returns`: `true` or `false`.

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

**IsDict(arg)** - Checks if arg is a Dict created by Dict or returned from another function. `Returns`: `true` or `false`.

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

**NodePath()** - returns the full path of the node from the current nodeset. The result is a string containg the node names in the path separated with `.`

**NodeKey()** - Returns the key name of the current node.

**Action()** - returns the action: `read` or `write` as string.

**Operation()** - returns the operation under which the script is executed. Applies to both data loader and node scripts. The returned value is string and can be one of these `select`, `insert`, `update`, `delete`.

**AddResult( `dict | ( key, value [key, value [, key, value ...]])` )** - Works only in read actions. Creates a new result (resulting row). After it until AddResult is called again SetResult works on the recently added result. Can be called without arguments to create an empty result.

> `dict` - A Dict obtained/created previously. All the Dict values are assigned to the result under the same keys.

> `key, value [key, value [, key, value ...]]` - key, value pairs to assign to the result the values under corresponding keys in the result.

**HasResults()** - returns `true` or `false` depending on if any result exists. In write actions always returns `true`.

**SetResult(`dict | ( key, value [key, value [, key, value ...]])`)** - Sets the result/current result/row. In read actions a result have exist (in DataLoader scripts use AddResult, In Node scripts the node should have produced at least one result otherwise SetResult will fail. To avoid that check ResultsCount or HasResults first).

> `dict` - A Dict obtained/created previously. All the Dict values are assigned to the result under the same keys.

> `key, value [key, value [, key, value ...]]` - key, value pairs to assign to the result the values under corresponding keys in the result.

> In both cases existing values with the same keys will be overwritten.

**ResultsCount()** - returns the number of result dictionaries in read actions and always 1 in write actions.

**GetResult(index)** - In read actions gets result specified by `index`. index must be between >=0 and < ResultsCount(). The index can be also omitted and then the last (current) result will be returned as Dict. In write actions always returns the only result (any arguments are ignored). The return value is a Dict with copy of the result and not the result itself. (see Dictionary functions above)

**RemoveResult(index)** - Removes result specified by `index`. `index` must be between >=0 and < ResultsCount(). In write actions throws an exception.

**GetAllResults()** - in read actions returns List of Dict objects (see List and Dict above) which are copies of all the results accumulated so far in the current node. In write actions returns a Dict with the current row.

**ModifyResult(index, ( dict | ( key, value [, key, value ...]))** - Works only in read actions. Modifies the result indicated by `index`, by setting the specified values one by one or by using dictionary in the same fashion like SetResult. As in write actions there is a single current row, which is also the result, this function throws an exception in order to stimulate usage of SetResult in these cases.

**ClearResultExcept(`dict | ([key [, key ...]])`)**

**CSetting(`name [,default]`)**

**CSettingLoader(`name [, default]`)**

**ModuleName()** - Returns module name

**ModulePath(`[combinepath]`)** - Returns the physical module path of the current module. If argument is passed combines them. This function allows easy construction of physical path to parts of the module. For example to get the physical path of the module's Data directory use `ModulePath('Data')`.

#### Result state functions

Most of these functions set the state property of the current result. To avoid the need to know the values indicating the states, there are separate functions for each state. Setting the states is most obvious when using SQL and DB plugins. If a script does that job think about your script in terms of a database operation - i.e. what kind of operation it implements if you want to describe it as a replacement of an SQL query.

**ResetResultState()** - Resets the state of the current result to unchanged.

**SetResultDeleted()** - Sets the state of the current result to deleted. If you want to impact the current execution process this should be set in a node script executed in `beforenodeaction` phase. 

**SetResultNew()**

**SetResultUpdated()**

**GetStatePropertyName**


