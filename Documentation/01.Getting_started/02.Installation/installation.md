# Installation guide

Working locally with CoreKraft and deploying to a server typically rise different concerns and fulfil different needs. In development one needs a trusted instance of CoreKraft to run the projects on which the developer works. Even if more than one version (or branch) of CoreKraft is needed for some reason or another, for a developer it is still handy to have CoreKraft installed separately from the projects.

In production deployment the need to make sure that updates of CoreKraft will not break something in the project makes it preferable to deploy both together - project and CoreKraft. It is, of course, possible to still use one CoreKraft instance to run multiple projects, but the appropriate devops organization must be established first as well as pre-deployment testing against the CoreKraft version that will drive the projects being deployed. This is an organizational matter, so all the deployment oriented documentation will assume installation of both CoreKraft and project, changing that is trivial, but we do not want to tempt others to go that way before they prepare their development process for it.

[Installation for development](install-dev.md) - how to setup CoreKraft and some project(s) to run with it for development purposes on the local computer.

[Back to README.md](../../../README.md) 
