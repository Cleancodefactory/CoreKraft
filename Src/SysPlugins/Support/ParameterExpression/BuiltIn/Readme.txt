This folder contains built in Parameter Resolver sets and Parameter Validator sets. They are loaded internally (unless disabled), their configuration uses the same models as the external plugin
sets, but is hard coded into this assembly.

It is recommended to keep all the built-in resolvers and validators - this will give you better and easier portability across different implementations of Kraft Servers, which are obliged to implement the same built-in 
resolvers to the level of the standard they implement.