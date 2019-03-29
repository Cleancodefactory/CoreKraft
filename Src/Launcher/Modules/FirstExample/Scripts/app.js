function StudentApp() {
    AppBaseEx.apply(this, arguments);
    this.setFinalAuthority(true);
}

StudentApp.Inherit(AppBaseEx, "StudentApp");
StudentApp.Implement(IPlatformUtilityImpl, "student");

StudentApp.prototype.provideAsServices = ["StudentApp"];
StudentApp.prototype.get_caption = function () {
    return "StudentApp";
}

StudentApp.registerShellCommand("StudentApp", "callook", function (args) {
    Shell.launchApp("StudentApp");
}, "Ultimate");

StudentApp.prototype.appinitialize = function (callback, args) {
    var singleWnd = new SimpleViewWindow(
        WindowStyleFlags.visible | WindowStyleFlags.parentnotify | WindowStyleFlags.adjustclient | WindowStyleFlags.sizable,
        this,
        new TemplateConnector("bindkraftstyles/window-mainwindow"),
        new Rect(200, 200, 800, 600),
        {
            url: this.moduleUrl("read", "main", "student")
        });

    this.placeWindow(singleWnd);

    this.mainWindow = singleWnd;
    return undefined;
}
StudentApp.prototype.run = function () { };
StudentApp.prototype.appshutdown = function () {
    jbTrace.log("The ModularExampleApp is shutting down");
    AppBase.prototype.appshutdown.apply(this, arguments);
}