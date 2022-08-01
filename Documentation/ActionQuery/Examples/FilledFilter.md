# Filled filter results (DDD examples)

## Aggregate approach

In aggregate approach we usually create a sub-node tree that extracts and saves all (or almost all) the data related to a central entity. This is usually done by entity's database ID (or similar key for non-database storage). This leaves the question about filtered searches - what they should return?

A common DDD (Domain Driven Development) practice is to call such filters (among other things) - behaviors. So they are typically implemented as separate sub-nodes and return either only sets of ID-s of the found entities or ID-s and a few common fields only. 

This achieves two goals - fast way to create brief lists (with a few general fields) and way to find the ID-s of the matched entities and then flesh them out by passing them to the full extractor that works with ID-s only. 

Let us call  the sub-node set responsible for the full data - `full` standing for the _full data_ and lets call the behaviors that perform searches - `filters` (can be one ore many depending on what is more productive depending on the underlying storage).

**So, the obvious question here is the fleshing out of the searches - how to do it and is it there a standard way to do it?**

Obviously a script can be added in the `filter's` starting node to do this, but this will make the _fleshing out_ integral part of the search which will require us to pass some parameter to enable or disable this part and this is harder to track during the development than a separate node that calls the `filter` and then _fleshes it out_. This approach will leave more easily usable as separate options both the brief results returned by the filter and the fleshed out results passed additionally through the `full` data sub-nodes.

So here is a script that does that. The assumptions are discussed after the example:

```AQL

$search(CallRead(
    'mymodule/aggregate_product/filter.by_letter',
    filterpost,
    filterget
    )
),
$idlist(ValueList()),
while($one(ConsumeOne($search)),
    (
        ListAdd($idlist, DictGet($one, 'productid'))
    )
),
RemoveAllResults(), # We no longer need the brief results - we will replace them with the full ones.
AddResult(
    ToNodesetData(
        CallRead('mymodule/aggregate_product/full',Dict('productid', $idlist))  
    )
)

```

What are the assumptions?

- The nodeset name is `aggregate_product`.

- It has an empty root node.

- It has a 1-level node named `full` ( aggregate_product/full ) which is the starting element of a sub-node set responsible to fetch all the data for the product by single `id` or list of `id`-s. _It is also responsible for writing, but this is outside of the scope of the current topic._

- We also have a filter node named `by_letter` under an empty node `filter` where we place various filters (the path to it is /filter.by_letter). Imagine some filter finding the products with names starting with a given letter. It could have additional filter conditions - it is not important for our purposes.

- We have another filter with whatever name, but for clarity lets name it `by_name_filled` (i.e. /filter/by_letter_filled).

- The script above is the main script in `by_letter_filled`.

In the nodeset definition's `Definition.json` the `by_letter_filled` node will look like this:

```JSON
    // .....
    {
        "nodekey": "by_letter_filled", 
        "islist": 1,
        "datapluginname": "Scripter",
        "read": {
            "parameters": [
                {"name": "filterpost", "Expression": "CombineSources('filter')"},
                {"name": "filterget", "Expression": "CombineSources('client')"}
            ],
            "select": {
                "loadquery": "aq//filter/by_letter/full.aql-main"
            }
        }                           
    } 
    // .....
```

The path to the script is arbitrary, of course. Important is that the `datapluginname` refers to a plugin configuration (see blow) that uses the `Scripter` plugin. In other words this node will execute script only and will not dig directly in a database.

Special attention deserve the parameters. They both use the `CombineSources` resolver. This built-in resolver function combines the specified collections into one. In our case we do not combine more than one collection, but the fact that the resolver creates a copy internally usable as a single parameter is what we need.

> Inside the script `filterpost` and `filterget` are passed to the `CallRead`. The effect is that the called node (the actual filter - `by_letter`) receives them as post and get parameters which will be the case if `by_letter` is called from a browser for example. So we ar just transferring the parameters assuming the `by_letter` node will be also defined to be called from a browser as well. Of course, this is very general, the node can be specifically designed to use only post or only get parameters and then we can just `CombineSources('filter,client')` them and pass only one collection, but if we aim at usefulness of both `by_letter` and `by_letter_filled` this is the safest bet that we can put in place and not think more about it.

The `by_letter` node must return at least the Id-s of the found products (assume they are named `productid`), but there is no problem including a couple of more fields as long as they are easy to extract and make the node usable in places where we do not need the full data about the product.

Yet, in `by_letter_filled` we read this, use the id-s only and resolve them to the full fledged product entities from the `full` node. Then we return those to the caller.

**What is achieved this way?**

We reuse to sub-nodesets - one that is designed to get the full data about a product and one designed for some king of filtering. Neither of them has to deal with the problems of the other. In the case of the filter (by_letter) we limit ourselves to just a few field (optionally), but we do not need to create sub-entities in depth. E.g. imagine a complex tree of related stuff that has to come with the product data, the filter digs only in SQL (for example) with part of it - as much as needed for the filtering, but not for the extraction. The `full` node and its sub-nodes deal with the depth and constructing the entire data tree describing a product fully.

There is some impact over the performance of course, but it is minimal compared to the effort needed to support multiple filters combined with fetching data tree describing the product as well. Any change in the database (or service) will cause the need of more changes than this approach and do not forget the implementations can spread throughout the application much more and bugs caused by forgetting to update some of them will happen sooner or later.

So, we achieve uniformity of the structure of the fetched data and ability to support structure changes with less effort. As a bonus we can still make filter return directly usable data if brief results can be of use somewhere.

**The plugin Configuration.json:**

```JSON
    {
        "Name": "Scripter",
        "ImplementationAsString": "Ccf.Ck.SysPlugins.Data.Scripter.ScripterImp, Ccf.Ck.SysPlugins.Data.Scripter",
        "InterfaceAsString": "Ccf.Ck.SysPlugins.Interfaces.IDataLoaderPlugin, Ccf.Ck.SysPlugins.Interfaces",
        "Default": true,
        "CustomSettings": {
        "libraries": "internalcalls"
        }
    }

```

One can be override this in appsettings if convenient, of course, but the need will depend on the custom settings. Here we just include the `internalcalls` library because we need it for those `CallRead` calls. Other custom settings can be used by the script itself, but it will depend on some specific decisions made for reasons not related to our topic.

**Can we reuse the script in many places where this is needed and the pattern is followed?**

Yes we can! To do that we have to parameterize:

- the path to the `full` node. 

- the path to the filter node. 

- the name of the ID in the filter output.

The configuration is obviously not the way to go - it applies to all usages of the plugin (Scripter in this case). In our case this will apply to all AQL scripts written with this plugin - not useful.

We need to specify the above settings in each case differently, so we needs nodeset parameters. Here is reworked node and then script.


```JSON
    // .....
    {
        "nodekey": "by_letter_filled", 
        "islist": 1,
        "datapluginname": "Scripter",
        "read": {
            "parameters": [
                {"name": "filterpost", "Expression": "CombineSources('filter')"},
                {"name": "filterget", "Expression": "CombineSources('client')"},
                {"name": "filter_path", "Expression": "'mymodule/aggregate_product/filter.by_letter'"},
                {"name": "full_path", "Expression": "'mymodule/aggregate_product/full'"},
                {"name": "id_fieldname", "Expression": "'productid'"}
            ],
            "select": {
                "loadquery": "aq//filter/by_letter/full.aql-main"
            }
        }                           
    } 
    // .....
```

and the script

```AQL

$search(CallRead(
    filter_path,
    filterpost,
    filterget
    )
),
$idlist(ValueList()),
while($one(ConsumeOne($search)),
    (
        ListAdd($idlist, DictGet($one, id_fieldname))
    )
),
RemoveAllResults(), # We no longer need the brief results - we will replace them with the full ones.
AddResult(
    ToNodesetData(
        CallRead(full_path,Dict(id_fieldname, $idlist))  
    )
)

```

The parameterization can be a little different, of course, but this illustrates the process.