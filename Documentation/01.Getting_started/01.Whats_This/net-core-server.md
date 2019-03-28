<!-- header
{
    "title": "Micro-Services Plugin-oriented Framework",
    "keywords":  [ "introduction", "overview", "history", "nature", "BindKraft", "Server-side", "micro-service", "net core" ]
}
-->

> Less is more

## Basic principles ##
There are a few pillars in software development which we believe are essential:
1. Object Oriented Programming
2. The ability to create reusable components
3. Run as much logic through a single path of execution

## Our approach ##
Following these principles, we have implemented Kraft. Kraft consists of two parts: the client (we refer to it as BindKraft) and the server (we refer to it as CoreKraft because it is implemented in .NET Core). Both BindKraft and CoreKraft create and use OOP, reusable components and loose coupling.

## CoreKraft ##
CoreKraft is one possible implementation of the server logic. We have started with .NET Core but will deliver other implementations (e.g. NodeJs and/or PHP). The general principles will remain untouched.

### Microservices ###
While there is no standard, formal definition of microservices, there are certain characteristics that help us identify the style. Essentially, the microservice architecture is a method for developing software applications as a suite of independently deployable, small, modular services in which each service runs a unique process and communicates through a well-defined, lightweight mechanism to serve a business goal. The communication among various devices and the microservices framework can be dependent on requirements, but the default implementation uses JSON and in the next stage HTTP/REST with JSON.
In large projects with multiple teams, this separation enables them to be more productive. Complete freedom, however, would end up in chaos. That’s why the infrastructure should define interfaces for communicating with these loosely coupled microservices.
In a nutshell, CoreKraft is a microservices framework where every node is independently addressable through URL, the business logic components are independently deployable, and the default data transferring standard is JSON.

### Restful ###
REST-compliant Web services allow requesting systems to access and manipulate textual representations of Web resources using a uniform and predefined set of stateless operations. This provides interoperability between computer systems on the Internet. The key words here are stateless operations and between computer systems on the Internet. In our case, we provide one system, which covers the client- and server-side.
On an abstract conceptual level, we are treating BindKraft and CoreKraft as one. Why is it smarter this way? If we focus exclusively on the client, the problem with the Entity state will be delegated to the mid-level programmer who must manually manage it. Besides the possible bugs, the implementation will contain tons of plain vanilla plumbing code.
Kraft deals with the Entity state naturally. Another example of a system tracking the state is the Entity-Framework.

### Strongly typed models ###
Strongly typed models is an easy to grasp, easy to use concept. They are not bad per se, but there are other ways to deal with a lot of models. The models in Kraft are the JSON entities! BindKraft binds its views and controls to these JSON models and CoreKraft processes them, and vice versa. Nobody limits you not to use strongly typed models, but you can do better. Let your database create these entities, manipulate them if needed and send them back. When you have complex business logic, please use strongly typed models but let the system deal with the rest of your CRUD operations (e.g. in 70% of the cases in one regular application, the developers are copying data from one model into the other in vain).

### Native support for plug-ins ###
CoreKraft has native support for plug-ins and custom service implementations. Both are injected into a dependency injection container and are accessible in the custom code. Even the core system is using the same mechanism.

[Back to README](../../../README.md)
