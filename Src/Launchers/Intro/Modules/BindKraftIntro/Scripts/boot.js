(function () {
    var reg = Registers.getRegister("bootfs");

    if (!reg) {
        Registers.Default().addRegister(new MemoryFSDirectory("bootfs"));
    }

    System.BootFS().writeScript("system/startapps", "launchapp BindKraftIntroApp dropcontext");
    System.BootFS().writeMasterBoot("startshell createworkspace 'bindkraftstyles/window-workspacewindow-simple' initculture 'en' initframework gcall 'system/startapps'");
})();