function BindKraftIntroApp() {
    AppBase.apply(this, arguments);
    this.windowManager = new BKI_WindowManager();
}
BindKraftIntroApp.Inherit(AppBase, "BindKraftIntroApp");
BindKraftIntroApp.Implement(IPlatformUtilityImpl, "bindkraftintro");

BindKraftIntroApp.Implement(ISupportsEnvironmentContextImpl);
BindKraftIntroApp.Implement(ISupportsCommandContextImpl, "single");
BindKraftIntroApp.Implement(ISupportsCommandRegisterExDefImpl, []);
BindKraftIntroApp.ImplementActiveProperty("currentExample", new InitializeObject("Current example", null));
BindKraftIntroApp.ImplementProperty("sections", new InitializeArray("Array holding the sections", []));
BindKraftIntroApp.ImplementProperty("isAdmin", new InitializeBooleanParameter("To indicate if user is admin", false));
BindKraftIntroApp.ImplementProperty("md", new InitializeStringParameter("Current example documentation", ""));
BindKraftIntroApp.ImplementProperty("loggedUser", new InitializeStringParameter("Currently logged in user", ""));

BindKraftIntroApp.prototype.provideAsServices = ["BindKraftIntroApp"];

BindKraftIntroApp.registerShellCommand("bindkraftintro", null, function () {
    if (Shell) Shell.launchApp("BindKraftIntroApp");
}, "BindKraftIntroApp");

BindKraftIntroApp.prototype.get_caption = function () { return "BindKraftIntroApp"; }
BindKraftIntroApp.prototype.run = function (args) { /* The function is override and must be implemented even empty. (See IAppBase) */ }

BindKraftIntroApp.prototype.rootWindow = null;
BindKraftIntroApp.prototype.appMainWindow = null;
BindKraftIntroApp.prototype.secondarySplitter = null;

BindKraftIntroApp.prototype.get_validatortemplate = function () {
    return ITemplateSourceImpl
        .GetGlobalTemplate(ITemplateSourceImpl.ParseTemplateName("bindkraftstyles/control-validator"));
};

BindKraftIntroApp.prototype.appinitialize = function (callback, args) {
    this.ajaxPostXml(this.moduleUrl("read", "main", "getroles"),
        null,
        function (result) {
            if (result.status.issuccessful) {
                if (result.data.userinroles.includes("administrator") || result.data.userinroles.includes("manager")) {
                    this.set_isAdmin(true);
                }
                this.set_loggedUser(result.data.userid);
                this.initMainWindows();
            }
        });
    BaseObject.callCallback(callback, true);
};

BindKraftIntroApp.prototype.initMainWindows = function () {
    this.rootWindow = Shell.createStdAppWindow();

    this.rootWindow = new MainWindow(
        new TemplateConnector('bindkraftstyles/window-simplewindow-empty'),
        WindowStyleFlags.fillparent | WindowStyleFlags.visible | WindowStyleFlags.adjustclient
    );
    this.placeWindow(this.rootWindow);
    this.appMainWindow = this.windowManager.createTouchSplitterWindow(
        {
            leftthreshold: 15,
            rightthreshold: 85,
            initial: 15,
            resizable: false
        },
        this.rootWindow,
        'bindkraftintro/window-touchsplitterwindow-static-main'
    );
    var url = this.moduleUrl("read", "main", "menu") + "?operation=nav";
    var appMenu = this.windowManager.createAppMenuWindow(url, this.rootWindow);
    var initAppBody = new SimpleViewWindow(
        WindowStyleFlags.fillparent | WindowStyleFlags.visible | WindowStyleFlags.adjustclient | WindowStyleFlags.parentnotify,
        new TemplateConnector("bindkraftstyles/window-simplewindow-empty"),
        {
            url: this.moduleUrl("read", "main", "initview")
        });

    this.appMainWindow.setLeft(appMenu);
    this.appMainWindow.setRight(initAppBody);

    this.popups = new PopDialogManager();

    var selectSection = new PopDialog(
        this.rootWindow,
        {
            url: this.moduleUrl("read", "main", "selectSection"),
            placement: {
                position: PopUpsPositionEnum.center,
                size: { w: 350, h: 350 }
            },
            template: new TemplateConnector("bindkraftstyles/window-simplewindow")
        }
    );

    this.popups.registerPopUp("selectSection", selectSection);

    this.popups.set_popupsPolicy(function (mng, openingPopUpKey) {
        var popup = mng.$popups[openingPopUpKey];
        if (popup.isOpened) {
            popup.closeDialog();
        }

        return true;
    });
};

BindKraftIntroApp.prototype.changeBodyView = function (exampleName, category) {
    this.createSecondarySplitter();
    this.ajaxPostXml(this.moduleUrl("read", "dbnodeset", "loadfiles") + "?operation=view&cat=" + category + "&exampleName=" + exampleName, {}, function (result) {
        if (result.data != null && result.status.issuccessful) {
            this.runExample(result.data.intro, category);
        } else {
            var errorPage = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "errorpage"), result.status.messages);
            this.secondarySplitter.setLeft(errorPage);
        }
    });
};

BindKraftIntroApp.prototype.runExample = function (example, category) {
    this.set_currentExample(example);
    var res = this.get_currentExample();
    if (category == "ForReview") {
        res.forReview = true;
    }
    var dData = {};
    var view = [];
    this.menuArray = [];
    if (res.Sources) {
        for (var i = 0; i < res.Sources.Entries.length; i++) {
            var currentContent = res.Sources.Entries[i].Content;
            switch (res.Sources.Entries[i].Type) {
                case 1:
                    view.push(currentContent);
                    this.addSourceFiles(res.Sources.Entries[i]);
                    break;
                case 2:
                    this.addSourceFiles(res.Sources.Entries[i], true);
                    break;
                case 3:
                    dData = JSON.parse(currentContent);
                    this.addSourceFiles(res.Sources.Entries[i]);
                    break;
                case 4:
                    this.set_md(marked(currentContent));
                    break;
                default:
            }
        }
    }

    var exampleMenu = this.windowManager.createSimpleViewWindow(
        this.moduleUrl("read", "main", "examplemenu"),
        {
            menu: this.menuArray,
            example: res
        },
        "bindkraftintro/window-simplewindow-empty");

    this.secondarySplitter.setRight(exampleMenu);

    this.createNewAppBody();
    var right = this.windowManager.createSimpleViewWindowViewString(view[0], dData);
    this.newAppBody.setRight(right);
};

BindKraftIntroApp.prototype.openCreateView = function (dc) {
    if (dc) {
        for (var i = 0; i < dc.Sources.Entries.length; i++) {
            var name = dc.Sources.Entries[i].EntryName;
            dc.Sources.Entries[i].EntryName = name.substring(0, name.indexOf("."));
        }
        if (!dc.state) {
            dc.state = "2";
        }
        this.set_currentExample(dc);
    } else {
        this.set_currentExample({
            Id: "", Caption: null, Description: null, LaunchSpec: null, OrderIdx: 1, Author: this.get_loggedUser(), Sources: {
                Entries: [{
                    Content: "# TODO:" + String.fromCharCode(13, 10) + String.fromCharCode(13, 10) + "### add documentation for this", Type: 4, EntryName: null
                }]
            }
        });
    }
    var createView = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "createview"), this.get_currentExample());
    var createControls = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "controlsview"), this.get_currentExample(), "bindkraftintro/window-simplewindow-empty");
    this.createSecondarySplitter();
    this.secondarySplitter.setLeft(createView);
    this.secondarySplitter.setRight(createControls);
};

BindKraftIntroApp.prototype.openAdminView = function (dc) {
    if (this.get_isAdmin()) { //temp
        this.ajaxPostXml(this.moduleUrl("read", "dbnodeset", "getdeletedfiles") + "?operation=admin", {}, function (result) {
            if (result.data != null && result.status.issuccessful) {
                //
            }
            else {
                var errorPage = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "errorpage"), result.status.messages);
                this.secondarySplitter.setLeft(errorPage);
            }
        });
        this.createSecondarySplitter();
        var view = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "adminview"), {});
        this.secondarySplitter.setLeft(view);
        this.secondarySplitter.setRight(null);
    }
};

BindKraftIntroApp.prototype.addSourceFiles = function (filesToAdd, isJs) {
    this.menuArray.push(filesToAdd);
    if (isJs) {
        var loader = new LoadableScript(filesToAdd.Content);
        loader.load();
    }
};

BindKraftIntroApp.prototype.changeCentralWindowData = function (fileObj) {
    var nodekey = "contentviewer";
    this.createNewAppBody();
    switch (fileObj.Type) {
        case 1:
            this.newAppBody.setRight(this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", nodekey), { source: fileObj.Content, brush: "htmlbrush" }));
            break;
        case 2:
            this.newAppBody.setRight(this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", nodekey), { source: fileObj.Content, brush: "jsBrush" }));
            break;
        case 3:
            this.newAppBody.setRight(this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", nodekey), { source: fileObj.Content, brush: "jsonBrush" }));
            break;
        //case 4:
        //    nodekey = "docviewer";
        //    var md = marked(fileObj.Content);
        //    this.newAppBody.setRight(this.windowManager.createSimpleViewWindowViewString(md, { source: md }));
        //    break;
        default:
            break;
    }
};

BindKraftIntroApp.prototype.changeCurrentExampleSource = function (dc) {
    var currentExample = this.get_currentExample();
    for (var i = 0; i < currentExample.Sources.Entries.length; i++) {
        if (dc.EntryName == currentExample.Sources.Entries[i].EntryName) {
            currentExample.Sources.Entries[i].Content = dc.Content;
            return;
        }
    }
    //switch (dc.Type) {
    //    case 1:
    //    case 2:
    //    case 4:
    //        for (var i = 0; i < currentExample.Sources.Entries.length; i++) {
    //            if (dc.EntryName == currentExample.Sources.Entries[i].EntryName) {
    //                currentExample.Sources.Entries[i].Content = dc.Content;
    //                return;
    //            }
    //        }
    //        break;
    //    case 3:
    //        for (var i = 0; i < currentExample.Sources.Entries.length; i++) {
    //            if (dc.EntryName == currentExample.Sources.Entries[i].EntryName) {
    //                currentExample.Sources.Entries[i].Content = dc.Content;
    //                return;
    //            }
    //        }
    //        break;
    //    default:
    //}
};

BindKraftIntroApp.prototype.refreshExample = function (dc) {
    var view = dc.menu.find(this.isView).Content;
    var data;
    try {
        data = JSON.parse(dc.menu.find(this.isData).Content);
    } catch (e) {
        data = {};
    }
    this.createSecondarySplitter();
    this.createNewAppBody();
    this.newAppBody.setRight(this.windowManager.createSimpleViewWindowViewString(view, data));
};

BindKraftIntroApp.prototype.insertExample = function (dc) {
    this.ajaxPostXml(this.moduleUrl("write", "dbnodeset", "savefiles"), { example: this.get_currentExample(), state: dc }, // + "?provider=file"
        function (result) {
            if (result.data != null && result.status.issuccessful) {
                var url = this.moduleUrl("read", "main", "menu") + "?operation=nav";
                var appMenu = this.windowManager.createAppMenuWindow(url, this.rootWindow);
                this.appMainWindow.setLeft(appMenu);
                var example = this.get_currentExample();
                if (example.state && example.state == "2") {
                    this.runExample(this.get_currentExample());
                } else {
                    example.state = "2";
                    this.runExample(this.get_currentExample(), "ForReview");
                }
            } else {
                var errorPage = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "errorpage"), result.status.messages);
                this.secondarySplitter.setLeft(errorPage);
                this.secondarySplitter.setRight(null);
            }
        });
};

BindKraftIntroApp.prototype.deleteExample = function (dc) {
    this.ajaxPostXml(this.moduleUrl("write", "dbnodeset", "deletefiles"), { example: this.get_currentExample(), state: "3" }, // + "?provider=file"
        function (result) {
            if (result.status.issuccessful) {
                var url = this.moduleUrl("read", "main", "menu") + "?operation=nav";
                var appMenu = this.windowManager.createAppMenuWindow(url, this.rootWindow);
                this.appMainWindow.setLeft(appMenu);
                this.secondarySplitter.setRight(null);
                this.secondarySplitter.setLeft(null);
            } else {
                var errorPage = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "errorpage"), result.status.messages);
                this.secondarySplitter.setLeft(errorPage);
                this.secondarySplitter.setRight(null);
            }
        });
};

BindKraftIntroApp.prototype.deleteAllMarkedDelete = function (dc) {
    this.ajaxPostXml(this.moduleUrl("write", "dbnodeset", "harddeletefiles"), { state: "3" },
        function (result) {
            if (result.status.issuccessful) {
                //
            } else {
                var errorPage = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "errorpage"), result.status.messages);
                this.appMainWindow.setLeft(errorPage);
            }
        });
};

BindKraftIntroApp.prototype.approveExample = function (success, data) {
    var example = this.get_currentExample();
    example.OrderIdx = data.orderIdx;
    this.ajaxPostXml(this.moduleUrl("write", "dbnodeset", "approve"), { example: example, state: "2", sectionId: data.name }, // + "?provider=file"
        function (result) {
            if (result.status.issuccessful) {
                var url = this.moduleUrl("read", "main", "menu") + "?operation=nav";
                var appMenu = this.windowManager.createAppMenuWindow(url, this.rootWindow);
                this.appMainWindow.setLeft(appMenu);
                var example = this.get_currentExample();
                example.state = "2";
                example.forReview = false;
                this.runExample(this.get_currentExample(), data.name);
            } else {
                var errorPage = this.windowManager.createSimpleViewWindow(this.moduleUrl("read", "main", "errorpage"), result.status.messages);
                this.secondarySplitter.setLeft(errorPage);
                this.secondarySplitter.setRight(null);
            }
        });
};

BindKraftIntroApp.prototype.createSecondarySplitter = function () {
    if (!this.secondarySplitter) {
        this.secondarySplitter = this.windowManager.createTouchSplitterWindow(
            {
                leftthreshold: 80,
                rightthreshold: 20,
                initial: 20,
                resizable: false
            },
            this.appMainWindow.get_rightwindow(),
            'bindkraftintro/window-touchsplitterwindow-static-inner'
        );
        this.appMainWindow.setRight(this.secondarySplitter);
    }
};

BindKraftIntroApp.prototype.createNewAppBody = function () {
    if (!this.newAppBody) {
        this.newAppBody = this.windowManager.createTouchSplitterWindow(
            {
                leftthreshold: 50,
                rightthreshold: 50,
                initial: 15,
                resizable: true
            },
            this.secondarySplitter.get_leftwindow(),
            'bindkraftintro/window-touchsplitterwindow-static-inner-second'
        );
    }
    this.secondarySplitter.setLeft(this.newAppBody);
    this.newAppBody.setLeft(this.windowManager.createSimpleViewWindowViewString(this.get_md(), { source: this.get_md() }));
};

BindKraftIntroApp.prototype.openSelectSection = function () {
    var operation = this.popups.openPopUp("selectSection", { sections: this.get_sections() });
    operation.chunk(this.thisCall(this.approveExample));
};

BindKraftIntroApp.prototype.isView = function (obj) {
    return obj.Type == 1;
};

BindKraftIntroApp.prototype.isData = function (obj) {
    return obj.Type == 3;
};

BindKraftIntroApp.prototype.appshutdown = function () {
    jbTrace.log("BindKraftIntroApp is shutting down.");
    AppBase.prototype.appshutdown.apply(this, arguments);
};
