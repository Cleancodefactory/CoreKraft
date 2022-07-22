# Built-in parameter resolvers

In the NodeSet files (Definition.json in a sub-directory of NodeSets module directory) one can specify parameters at each `node` or even separate set of parameters for `read` and `write` operations done by the node. Additionally parameters can be specified at root level of the NodeSet and they will be applicable in any node in the NodeSet at any depth. For more information see the CoreKraft documentation and tutorials.

Here we describe the built-in `resolvers` which can be used in the parameter expressions. An example parameter definition:
```JSON
    "parameters": [
        {
            "name": "param1",
            "Expression": "GetFrom('client', name)"
        },
        {
            "name": "param2",
            "Expression": "GetFrom('current,parent', 'id')"
        }
        // And more entries like these
    ]
```

The `Expression` entry specifies how to obtain and calculate the value for the parameter. Check the rest of the documentation for more information how they are consumed by various plugins. For example in an SQL query executed by one of the database plugins the parameter usage will look like this:

```SQL
SELECT * FROM SOME_TABLE WHERE ID=@id AND SOME_FIELD LIKE '%' || @param1 || '%';
```

## The language of the `Expression`

The `resolver expressions` are a very simplistic miniature language with a extremely small set of basic rules and support for functions we call `Resolvers`. The resolvers that can be used are configurable and their numbers can be extended with plugins containing more of them. In this document we discuss the built-in resolvers inside the CoreKraft itself.

## The expression syntax

The EBNF syntax:

```EBNF

Expr = "name" | Resolver | Number | String | Boolean | "null" | Parameter ;

Resolver = Identifier "(" (  [ Expr { "," Expr } ] ")" ;

Number = [ "-" | "+" ] { Digit } [ "." { Digit } ] ;

Digit = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;

Letter = "A" | "B" | "C" | "D" | "E" | "F" | "G"
       | "H" | "I" | "J" | "K" | "L" | "M" | "N"
       | "O" | "P" | "Q" | "R" | "S" | "T" | "U"
       | "V" | "W" | "X" | "Y" | "Z" | "a" | "b"
       | "c" | "d" | "e" | "f" | "g" | "h" | "i"
       | "j" | "k" | "l" | "m" | "n" | "o" | "p"
       | "q" | "r" | "s" | "t" | "u" | "v" | "w"
       | "x" | "y" | "z" ;

String = "'" [ { Character } ] "'" ;

Boolean = "false" | "true" ;

Parameter = IdentifierString ;

Identifier = IdentifierString ;

IdentifierString = ( Letter | "_" ) [ { Letter | "_" | Digit | "." } ] ;

```

This basically boils down to a syntax consisting of function calls with parameters which can in turn be function calls. The functions are the `resolvers` we describe below and there are some possible literals, namely `numbers`, `strings` and `booleans`, some special keywords like `name` and `null` and the `Parameter` keywords. The `resolvers` have exact number of arguments and variable number of arguments is not currently supported by the resolver expression engine.

`name` - Acts as a string that is exactly the name of the parameter. In the example above `name` will resolve to "param1" in the first expression.

`null` - Denotes a null value literal.

`Parameter` - This is the name of another parameter from the list of the configured parameters. So, the expression defining one parameter can use the other parameters - their expressions by simply placing the name of the parameter. An example:

```JSON
    "parameters": [
        {
            "name": "param1",
            "Expression": "GetFrom('client', name)"
        },
        {
            "name": "param2",
            "Expression": "Concat(param1,'%')"
        }
    ]
```

In this example param2 is param1 concatenated with "%" character which could be useful in LIKE conditions in SQL for example.

In current CoreKraft versions the recursion level allowed for using the `Parameter` syntax is **limited to only one level**. This means that if in the example above a param3 tries to use param2 in a similar way, this will violate the limitation and will cause an exception. This limitation is imposed for a reason, its purpose is to prevent the temptation to use the parameter expressions as a programming language. They are pre-compiled and fast enough for what they do, but making too complex expressions and causing recursion can lead to uncontrollable and hard to calculate complexity. One should not forget that they are intended mainly as the means to fetch the data needed by the plugins, best example are the DB plugins.

Where the initial data comes from? Notice the `GetFrom` resolver, it fetches a value from the context collections which represent the data in the current request (be it an HTTP one or not.). In the end of the document there is more detailed description of the available collections and what they correspond to.

## The built in resolvers

### GetFrom(sources, paramname)

**sources** - A string specifying comma separated list of sources to check.

**paramname** - The name to look for.

Gets a value from the standard sources. When a list of sources is specified, checks them in the order specified until a parameter with the `paramname` is found or returns null if the list is exhausted.

The type of the returned value will depend on the source's capability to return differently typed values. See also the `CastAs` resolver below for the cases where explicit type casting is needed.

**Standard sources:**

`current` - (write). Represents the object processed by the node. 

`input` - (write). Alias for current.

`filter` - (read). Represents the JSON posted to CoreKraft. When this is an http request this will be JSON body of a POST request (these are the typical requests sent by BindKraft apps).

`data` - (read and write). The same as filter, but available in both read and write operations.

`parent` - (read and write). Represents the object processed by the parent node of the current node. Typical usages of this would be when one creates a child node that extracts data related to the data processed/fetched by the parent node. In case of databases this is often the primary key field of the "parent" record.

`parents` - (read and write). Like parent, but represents the combined result of all the parents down the NodeSet tree. This source gets used not so often, because it makes sense only in NodeSets with depth of at least 3 which are rarely an everyday matter.

`client` - (read and write). In HTTP requests this represents the query string. In other requests it depends on the implementation, but it is always something that plays a similar role.

`server` - (read and write). A number of server defined variables.

_**Currently**_ there is no separate source for a typical form post x-www-form-urlencoded, nor for multipart/form-data, because they are internally represented as a posted JSON which can be addressed with the existing sources. This means that the CoreKraft abstracts all the post and post-like requests containing named values the same way.

_**Currently**_ there is no representation of headers of HTTP requests and according to the CoreKraft's concept there should never be one. If headers or any header-like data has to reach the NodeSet the `processor` that accepts the request must copy it into one or more of the other collections. However, we noticed the need for another abstract source that represents data considered as metadata/OOB data or something similar and this can be introduced in near future.

_**Currently**_ multiple values attached to the same name are not supported for HTTP requests and if support is introduced it will take the form of an array (as if a value in JSON post is an array). Processing arrays is supported by some resolvers, but is hard to provide full general support without introducing disproportionally complex additions and we are working on finding the right way to do this without making writing CoreKraft NodeSets harder.


### NavGetFrom(source, dottedpath)

Advanced version of GetFrom ...

### CurrentData()

Returns the data for the current node (write operations only) as a dictionary. Most useful for ActionQuery scripts which need to access the whole record.

### GetHostingUrl()

Returns the hosting URL from the appsettins.

### RequestType()

Returns the current request/call type. One of the ECallType values as integers: 

0 - Web request
1 - Direct call
2 - Signal
3 - Service call (also known as indirect call)

### RequestProcessor()

Returns a string identifying which processor/handler created the request/call. Processors/handlers construct the processing context being executed. Each processor deals with specific concerns like different manners of decoding input data, batching multiple requests and so on.

### RequestTask()

Arbitrary string marking the purpose/role of the current request. By default set to "webrequest" and "call" (for direct calls). Do not use the default values to identify the type of request - use RequestType() instead. This value is intended for marking hte request explicitly by the code that creates the request. It might happen in processors of WEB requests in future, but currently it is mainly designed for direct calls and scheduled service calls. For example certain requests can be marked as "import" which marking will be accessible for scripts and even SQL and allow conditional logic enabling the same nodeset to work correctly for requests sent by UI and requests sent by import task.

### NewGuid()

Generates and returns a new Guid as string.

### GetUserId()

If there is a logged on user, returns the user id provided by the authorization server (see authentication and authorization in CoreKraft). If there is no logged on user the resolver returns null. This resolver is used extensively in Nodesets where the performed operations differ for logged on and anonymous users.

### GetUserEmail()

If there is a logged on user, returns the email provided by the authorization server, otherwise returns null.

### HasRoleName(role)

Returns a 0 | 1 result indicating if the logged on user has the `role`.

**role** - a string specifying the role name. These are the roles given by the authorization server. The actual role names depend on the configuration of the authorization server, however by default it supports "`user`", "`manager`" and "`administrator`" assignable to individual users. Real world usage showed that they are rarely extended and only the default roles are used in the vast majority of cases. There is a logical reason for this trend - most application need more than just roles and typically assign user rights in application specific manner. As CoreKraft typically serves multiple applications at once and behind the same authorization server, the roles from the authorization server are typically convenient for initial user assignment, identifying admins and little else. In real applications the users are usually recognized by their ID, recorded by the application with some minimal initial settings and further details are left to the managers/admins of the specific application. I.e. the authorization server's user id and roles are useful for identification, but not much useful for detailed authorization and especially in the cases when many applications use the same authorization and seek a common and minimal set of general roles.

### Or(a,b)

Returns 0 | 1 result indicating if the any one of the two arguments is true like.

**a**, **b** - Arguments to check.
### Concat(a, b)

Converts the two arguments to string (if they are not strings already) and returns a string of them both concatenated one after the other.

**a**, **b** - The arguments to concatenate.

### Replace(a, b, c)

Returns a string `a` with all occurrences of `b` replaced with `c`.

The arguments `a` and `b` are not converted to string - they have to be strings. The argument **c** is converted to string.

_This behavior is chosen because when replacing substrings the conversion of the base string and substring being searched often requires specific conversion._

### Coalesce(a, b)

Returns the first argument if it is not null, otherwise returns the second argument.

### NumAsText(a)

If the argument is numeric returns it as `content string`, in all other cases it returns the `content string` "**null**".

### CastAs(a, b)

Casts `b` to the type specified by `a`

**a** - a string specifying the type to which to convert the second argument. The supported type names are "`int`", "`uint`", "`double`" and "`string`".

**b** - the argument to convert.

### Add(a, b)

### Sub(a, b)

### AsContent(a)

### OrderByEntry(a, b, c)

### OrderBy(a), OrderBy2(a,b), OrderBy3(a, b,c)
### ApiTokenFromAuth(a)

### GetUserDetails(a)

### GetUserRoles()

### Skip()

### IntegerContent(a)

### Split(input, separator)

If the `input` is a string it will be split into array of strings separated by the specified by the `separator` argument separator. If the `input` is not a string it will be returned "AS-IS".

Useful for use together with `idlist` for instance. `idlist` will deal with dictionaries, lists and other enumerable parameters, containing numbers or strings, but will not know what to do with a plain string. Thus using something like `idlist(Split(input,','), null)` will accept either sets of numbers (the elements being numbers or strings) or plain strings containing lists of numbers separated by coma in this example.

### idlist(input,regex)

Produces a value marked as "content" (e.g. for injection in SQL for instance) containing the elements of the `input`. The `input` must be some enumerable - a dictionary, a collection. In case of dictionary only the values will be used. 

Example: Let us assume we call through AJAX and a Javascript array goes into the input. The idlist will take all the elements from the array and generate a string like below:

```
# input: [1,2,3]
idlist(input, null) # will produce 1,2,3

#input []
idlist(input, null) # will produce NULL

#input ['a','b','c']
idlist(input, null) # will produce NULL - see the comments below

#input ['a','b','c']
idlist(input, 'a|b|c') # will produce 'a','b','c' - see the comments below

#input 'a'
idlist(input, 'a|b|c') # will produce 'a'
```

* When elements are strings the `regex` argument must be supplied and the elements must match it.

* A single value as input will be treated as an array with a single element.

* To use as an input a string containing list of elements `idlist` must be used together with `Split` (see above)

### MetaADOResult(field[, depth])

Gets ADO meta information for the last database operation performed in the node specified (`depth`).

`field` - Specifies which field to extract. Can be: `rowsaffected`, `rows`, `fields`. 

_`rowsaffected`_ returns the reported rows affected (-1 on select, 0 or more on insert, update and delete statements). The value applies to the entire SQL batch.

_`rows`_ - returns the number of rows fetched for the last result set. A batch can return many result sets, this function provides access only to th last one.

_`fields`_ - The number of columns in the last result set.

`depth` - Extracts the meta info for the 
> 0 -current node (0) (_should not be used in before main action scripts_)

> 1 - extracts from the parent and so on. If the depth exceeds the execution chain null will be returned.

`rows` and `fields` will be available only if MetLogging contains the Basic flag.


### MetaNode(field, depth)

Extracts general information from the meta node

`field` - Specifies which field to extract. Can be: `name`, `step`, `executions`. 

`depth` - Extracts the meta info for the 

> 0 -current node (0) (_should not be used in before main action scripts_)

> 1 - extracts from the parent and so on. If the depth exceeds the execution chain null will be returned.


### MetaRoot(field)

Extracts general meta information from the current execution as whole. Some fields change during execution and may return different meta information depending on where and when they are fetched.
                
`field` - Specifies which field to extract. Can be: `flags`, `steps`, `basic`, `debug`, `trace`, `log`.

This one is mainly useful to determine if certain flag is set in the configuration when the parameter is used for something dependent on that information.

### Lower(a,b)

Compares `a` and `b` and returns `true` if a is lower than b. While it works with strings, comparison based on locale settings is not supported for now.

### Greater(a,b)

Compares `a` and `b` and returns `true` if a is greater than b. While it works with strings, comparison based on locale settings is not supported for now.

### Equal(a,b)

Compares `a` and `b` and returns `true` if a is greater than b. While it works with strings, the comparison will be exact.

### IfThenElse(condition, pos, neg)

**condition** - null or a numeric value. If it is true like hte resolver returns pos, otherwise neg.

**pos**, **neg** - The two possible returnees, they will be returned "as is" without type or flags changed.

### CheckedText(text, regexp)

Checks the `text` against the `regexp` and returns it as ParameterResolverValue marked as content (EResolverValueType.ContentType) if matched or null marked as invalid if not. The resulting value is handled typically as text insert (by DB plugins for example) allowing to use text inserted in an SQL statement without risk of security compromises. Other plugins can use the resulting value differently - please check first. While there are cases when this may be unavoidable mass usage of CheckedText is not recommended. It will protect against SQL injection (for example) only if the regular expression is carefully composed, massive usage will obviously rise the risk of checks that will miss something.

