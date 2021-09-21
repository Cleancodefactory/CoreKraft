# Hosting ActionQuery in a CoreKraft plugin.

_Lets start by reiterating the fact_ that most often hosting AQ in a CoreKraft plugin will be overkill. This is completely justified in plugins that are or can be implemented as set of granular actions/functions and having a script enables you to make them work in different combinations. Seen from another viewpoint - these will be plugins which without ActionQuery will be recompiled constantly, plugins that will be cloned into a number of variations and so on. So, instead of implementing all the work one can split it into functions and let ActionQuery scripts vary the way they are used.

For the mainstream cases using the Scripter plugin, with the necessary libraries CoreKraft provides for ActionQuery, will allow any programmer to create "smart" nodes in Nodesets and handle most possible non-trivial tasks.

## Implementing ActionQuery support in a CoreKraft plugin

We will talk about DataLoader plugins here, but the Node plugins are not much different and we will point what is done differently in the end of the document.

If we have the plugin ready, how the usage in a nodeset will look like?

### Configuring ActionQuery script in a Nodeset node.

The node will look normally:

```JSON
 ... nodes ...
    {
        "nodekey": "somenode",
        "datapluginname": "MyPlugin",
        "islist": 1,
        "Trace": true,
        "write": {
            "parameters":[
                {"name": "myparam", "Expression":"GetFrom('current',name)" }
            ],
            "insert": {
                "loadquery": "ac/myaqscript.ac"
            }
        }	
    }
... continues ...

```    

In the example above we have a rather partially defined node, but for the sake of pointing the ActionQuery usage this is enough. Note the `loadquery` property in the insert section. This can be used also in select, update and delete of course.

The `loadquery` property defines path to a file relative to the nodeset directory (the one where the Definition.json resides.). This file will be read when the nodeset is loaded and its text content will be set to the `query` property of the operation (insert in this case).

So, the same effect can be achieved if you specify the script inline:

```JSON
    . . .
    "insert": {
        "query": "Set('a', myparam), SetResult('x', Get('a'))"
    }
    . . .
```
This will be very inconvenient for almost any useful AQ script, so the first option is almost always preferred.

The ActionQuery script is just text, so it is possible to devise some custom manner to obtain it - for example one can obtain it from a database or somewhere else. In case you need this, you may also need some configuration settings to help you with the matter - use the custom settings in Configuration.json to inform your plugin what it needs to know.

### Obtaining the script source from the plugin

One can use different base classes when implementing a plugin, but no matter the approach TODO ....