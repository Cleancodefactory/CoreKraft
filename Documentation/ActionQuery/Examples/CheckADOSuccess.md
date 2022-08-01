# Check if the database operation was successful.

## The problem

Some database operations have to be written in cumbersome way or require additional code inside the database in order to throw an exception when they fail. In many cases there are not enough other reasons to justify the effort to write procedure instead of simple query or add a trigger just to be able to cause exception when insert fails for instance.

There can be more reasons to do the necessary additional work, but more often such reasons do not exist or are unimportant enough to make you look for simple way that involves less work.

## The basis for the solution

ADO.NET actually returns information about the executed command. What we are interested in is the `rows affected` value. So, for example, if we have conditional insert statement, instead of trying more complicated solutions on the database side, we can simply check if (enough) rows were affected by the database command.

Of course this is important for write operations. Usually our problem is irrelevant to read operations, because it is typically enough to have or not have results in that case.

## CoreKraft features we need to use.

We need access to the execution meta information. CoreKraft collects some mandatory (minimal) information about the execution of the nodeset and potentially a lot more (when configured so). We need only two pieces from the mandatory meta information:

- The state of the objects passing through the node in question. We have to check if all is ok on objects that are actually processed and not on objects that passed through because they have no state or have state `unchanged`.

- The `rowsaffected` value returned by ADO after executing the statement(s) for this node.

TO BE CONTINUED ...