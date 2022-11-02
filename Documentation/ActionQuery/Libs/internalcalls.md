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