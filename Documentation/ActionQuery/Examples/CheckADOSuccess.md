# Check if the database operation was successful.

## The problem

Some database operations have to be written in cumbersome way or require additional code inside the database in order to throw an exception when they fail. In many cases there are not enough other reasons to justify the effort to write procedure instead of simple query or add a trigger just to be able to cause exception when insert fails for instance.

There can be more reasons to do the necessary additional work, but more often such reasons do not exist or are unimportant enough to make you look for simple way that involves less work.

## The basis for the solution

ADO.NET actually returns information about the executed command. What we are interested in is the `rows affected` value. So, for example, if we have conditional insert statement, instead of trying more complicated solutions on the database side, we can simply check if (enough) rows were affected by the database command.

Of course this is important for write operations. Usually our problem is irrelevant to read operations, because it is typically enough to have or not have results in that case.

## CoreKraft features we need to use.

We need access to the **execution meta information**. CoreKraft collects some mandatory (minimal) information about the execution of the nodeset and potentially a lot more (when configured so). We need only two pieces from the mandatory meta information:

- The `state` of the objects passing through the node in question. We have to check if all is ok on objects that are actually processed and not on objects that passed through because they have no state or have state `unchanged`.

- The `rowsaffected` value returned by ADO after executing the statement(s) for this node.

## Brief example

We omit everything not related to our problem for brevity:

```JSON
// ................
{
    "nodekey": "somenode",
    "islist": 1, // This does not matter in our case
    // There could be a read section, but our case ia about write operations mostly
    "write": {
        "parameters": [
            // Omitted for brevity
        ],
        "insert": {
            "loadquery": "sql/full/insert.sql"
        },
        "update": {
            "loadquery": "sql/full/update.sql"
        },
        "delete": {
            "loadquery": "sql/full/delete.sql"
        },
        "customplugins": [
            {
                "custompluginname": "NodeScripter",
                "afternodeaction": true, // This is important
                "loadquery": "aql/full/check_success.aql-node"
            }
        ]
    }
}
// ................

```

Assume the statements are like their names suggest - insert, update, delete ones. The paths to the files is just an example one.

Now the `check_success.aql-node` script is our main feature here. 

It has to check if the operation actually did something:

```AQL
    if (RegexMatch(MetaNode('datastate'), '^1|2|3$'),
        if (Lower(MetaADOResult('rowsaffected'), 1),
            Throw('Write operation at level Company failed')
        )    
    )
```
Dissection of the code:

MetaNode extracts the _volatile_ value `datastate`. This value is actual only during the execution of the node. It can be used by the node plugins (custom plugins) on that node only. It contains the data state of the object (record) currently processed and is available only in write operations. This

**IMPORTANT**: As the state is set as string containing a single digit ("0" - unchanged, "1" - new, "2" - changed, "3" - for deletion) we then use `RegexMatch` to check if the data state involves processing. In cases in which not all operations are performed the expression may need to be changed. E.g. you may have an empty update operation which will invoke no changes. Depending on the design it may or may not be invoked - i.e. data with state `changed` may or may not pass through the node and be left "as-is". In those cases the check should not be performed, because it will treat this pass-through as an error - no rows will be affected.

And as hinted above for all the states involving actual processing we check if any rows are affected. If none at this means the statements did nothing and this is an error according to our assumptions.

**IMPORTANT**: Have in mind that we assume that the statements in question are written so that they have to effect changes on at least one row and failing to do so means wrong ID or wrong something else. We will discuss this a little bit further below. As said in the beginning this example is for those cases, there are certainly other ways to make sure database operations perform correctly.

## Conclusions so far

To make sure the write database operations succeed most often is equivalent to making sure they did something. This, of course, depend on how they are coded exactly, but it at least looks like the approach that needs the least code. However, inside the database there are no standard ways compatible with all databases that will as a minimum give you the chance to rise a flag in some field or even better to throw exception.

ADO exploits a feature the connectors to virtually all databases support - affected rows report in the result from the database. In CoreKraft we capture this and it can be used as almost universally compatible technique to check if something was done or not.

Still, we have to know if the processing is needed or not for the data being processed. Again, CoreKraft captures this temporarily during the processing of every object (record) in the data and we can access this information in order to know if we should check or not for affected rows.

What remains is a little idea what kind of SQL will work with this well. ADO deals with relational databases, so for other sources we will have to look elsewhere of course.

## The write database operations and their success/fail conditions.

TO BE CONTINUED ....