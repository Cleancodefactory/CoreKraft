<<<<<<< HEAD
﻿# Notes about the node processing implemented in this project

## Problematic stuff

How to describe problems and suggestions:
Becasuse this is a very specific project it seems to be best to specify ways to find the respective code. It is not always possible to point it out, so 
    the identification is done best by specifying mode, phase etc. So, the recomendations is to compose your captions as 
```
(mode | phase)[, plugin | methods | region ...] - Problem title.
```

### Write mode, custom plugins - setting new records in the result during execution.
First of all this will collide with the running enumeration, second even this is somehow solved it may impact the iterations and invoke wrong number of executions.

### Write mode, UpdateResults - seems pointless and is probably caused by already obsoleted stuff.
The assumptions look strange, I wonder if this has any purpose anymore.

### Synchronize scoped context - needs defaiult implementation
Seems mostly copy and paste, but we need to ensure provider= works reliably with Core and create alternativ way for connection creation if not.

## Ideas with no current implementation

=======
﻿# Notes about the node processing implemented in this project

## Problematic stuff

How to describe problems and suggestions:
Becasuse this is a very specific project it seems to be best to specify ways to find the respective code. It is not always possible to point it out, so 
    the identification is done best by specifying mode, phase etc. So, the recomendations is to compose your captions as 
```
(mode | phase)[, plugin | methods | region ...] - Problem title.
```

### Write mode, custom plugins - setting new records in the result during execution.
First of all this will collide with the running enumeration, second even this is somehow solved it may impact the iterations and invoke wrong number of executions.

### Write mode, UpdateResults - seems pointless and is probably caused by already obsoleted stuff.
The assumptions look strange, I wonder if this has any purpose anymore.

### Synchronize scoped context - needs defaiult implementation
Seems mostly copy and paste, but we need to ensure provider= works reliably with Core and create alternativ way for connection creation if not.

## Ideas with no current implementation

>>>>>>> develop
