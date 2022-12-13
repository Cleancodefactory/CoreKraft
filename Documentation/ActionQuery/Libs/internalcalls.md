# Internal CoreKraft calls (`internalcalls`)

### CallRead(address[, data[, clientdata[, runas]]])

    Executes read action on the node in the nodeset specified by the address and returns the result"

### CallNew(address[, data[, clientdata[, runas]]])

    Executes new action on the node in the nodeset specified by the address and returns the result

### CallWrite(address[, data[, clientdata[, runas]]])

    Executes write action on the node in the nodeset specified by the address and returns the result. Make sure you set the state of the data (or parts of it) to insert, update or delete.

### ScheduleCallRead(address[, data[, clientdata[, runas]]])

    Schedules read action on the node in the nodeset specified by the address and returns the result

### ScheduleCallWrite(address[, data[, clientdata[, runas]]])

    Schedules write action on the node in the nodeset specified by the address and returns the result

## All the functions above take the same arguments:

`address` - Address of the module, nodeset and node in it to call/schedule for call. The syntax is simple `<modulename>/<nodesetname>/<startnode>.<subnode>.<sub-subnode>`

`data` - Data which will be available through the execution of the call as DATA (the same way as if JSON is posted through a normal WEB request). Dictionary should be preferred on root level.

`clientdata` - (Optional) Same as data but this one will be visible during the call as client data (which usually corresponds to query string parameters).

`runas` - (Optional) string - a name of a built-in user, listed in the active appsettings.json of the project (workspace). These users are mostly intended for work that would be described by most people as "system tasks", e.g. imports, regular tasks like archiving, operating on existing data etc. These are mostly used with the SchedueCallXXX methods where the execution will actually occur after the initial request has finished. In those cases and in some other situations where other nodesets have to be called using the user who initiates the call/scheduling of a task is impractical or problematic. 


### ScheduledCallStatus

    Attempts to obtain the scheduiling status of a scheduled task

### ScheduledCallResult

    Attempts to obtain the result of a finished scheduled task

**Added on 13 December 2022**

New methods concerning ScheduleCallRead and ScheduleCallWrite were added. They set library **`state`** applied to any consequent `ScheduleCallRead` or `ScheduleCallWrite` call performed to the end of the current script. These methods define callbacks for the scheduled calls. This technique is rarely used, but sometimes such callbacks are necessary - for example when the scheduled calls perform long lasting operations which should be recorded/logged in a separate database that keeps tracks when they were executed and with what results. In general the callbacks enable in first place tracking the scheduled calls (tasks) without forcing the programmer to implement in them such a functionality. Of course other uses are also possible and legal.

All callbacks are executed over the same thread where the scheduled call is executed. This means they will consume time otherwise used by the scheduled calls. This is necessary because of the main purpose of this feature - call tracking.

All the functions accept the same arguments, but the callbacks are executed in different moments of time.

### OnSchedule 

Declares a callback called after scheduling a call. Any call scheduling after this point will execute that callback when scheduled.

### OnStart

Declares a callback called  just before the scheduled call is actually executed - i.e. when it is picked up from the scheduled calls queue.

### OnFinish

Declares a callback called when the scheduled call is finished.

**The arguments of OnSchedule, OnStart and OnFinish:**

All 3 functions accept:

`address` - a string specifying the address of node to call using the usual syntax: `modulename/nodeset/mode[.node2[.node3]]`. If null is passed the corresponding callback is removed and not applied to the following `ScheduleCallRead` and `ScheduleCallWrite` calls.

`write` - Boolean, optional. If true the callback will be executed as write operation. If omitted or false the callback will be executed as read operation.

`runas` - String, optional. The built-in user on behalf which the callback will be executed. If never set the callback will be anonymous. Most often any scheduled calls and related callbacks call nodesets restricted to built-in users. This depends on the nodeset design, but the common practice is to not leave them without security requirements in order to prevent them from being called from the WEB or even called internally by mistake.

**The callbacks:**

The input data of the callbacks is a dictionary with elements named 'input' for the input model of the call for which the callback will be executed and 'return' for the return model. 

All callbacks get as 'input', but only OnFinish has non-null 'return' value.

The values follow the InputModel and ReturnModel types in CoreKraft, but are converted to dictionaries with similar structure and part of the members are not included. Any changes in InputModel will not be applied to the original InputModel - the callbacks receive copies.

_Further changes and additions can be expected for this feature!_