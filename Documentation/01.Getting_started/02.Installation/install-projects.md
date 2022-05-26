# CoreKraft project

Everything one does with CoreKraft with or without BindKraft involves projects or workspaces (alternative name coming from BindKraft).

The projects contain everything except the CoreKraft itself - from the services it will provide (mostly as nodesets), resources and scripts that need to be served to the client. The features CoreKraft offers are not exclusively for BindKraft based clients. In the real world CoreKraft projects can vary from data services providers to complete UI and services solutions. Obviously some client technologies will be able to benefit from CoreKraft for their UI, but others will have very different structure and lifecycle to seek integration beyond the data service part.

For these reasons the project structure and functionality is described below from the most independent parts to the more specialized features. This should also make it a bit easier to read as the specialization whatever it might be always comes from architectural and technological concerns which become less widely known as we dig deeper in the specific concepts and even more when we discuss their effect further down the line, for example on the UI. Here BindKraft can offer higher development productivity thanks to the conceptual integration with CoreKraft in exchange for the willingness of the developer to make an effort to think along the lines of the concepts rooted in CoreKraft and further developed on the client side.

## Structure

The CoreKraft project is a directory tree with several subdirectories intended for specific purposes. The project also contains in its root some settings and often build related files.

The first level will look something like this:

```
Modules
Src
wwwroot
appsettings.Development.json
appsettings.Production.json
nlog.config
# typical files not really part of the project, but commonly included
launch.bat
CssCompile.bat
```

