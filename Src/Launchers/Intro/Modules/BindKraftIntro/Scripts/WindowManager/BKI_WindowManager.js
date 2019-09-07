function BKI_WindowManager() {
    BaseObject.apply(this, arguments);
}

BKI_WindowManager.Inherit(BaseObject, "BKI_WindowManager");


BKI_WindowManager.prototype.createAppMenuWindow = function (url, parent) {
    var svw = new SimpleViewWindow(
        WindowStyleFlags.fillparent | WindowStyleFlags.visible | WindowStyleFlags.adjustclient,
        new TemplateConnector("bindkraftstyles/window-simplewindow-empty"),
        parent,
        {
            url: url
        });
    return svw;
}

BKI_WindowManager.prototype.createSimpleViewWindow = function (url, paramData, templateName) {
    if (!templateName) templateName = "bindkraftstyles/window-simplewindow-empty";
    var svw = new SimpleViewWindow(
        WindowStyleFlags.visible | WindowStyleFlags.adjustclient | WindowStyleFlags.parentnotify | WindowStyleFlags.fillparent,
        new TemplateConnector(templateName),
        {
            url: url,
            directData: paramData
        });
    return svw;
}

BKI_WindowManager.prototype.createSimpleViewWindowViewString = function (viewString, paramData) {
    var svw = new SimpleViewWindow(
        WindowStyleFlags.visible | WindowStyleFlags.adjustclient | WindowStyleFlags.parentnotify | WindowStyleFlags.fillparent,
        new TemplateConnector("bindkraftstyles/window-simplewindow-empty"),
        {
            view: viewString,
            directData: paramData 
        });
    return svw;
}

BKI_WindowManager.prototype.createTouchSplitterWindow = function (sizeParamObj, parent, templateName) {
    var splitter = new TouchSplitterWindow(
        WindowStyleFlags.fillparent | WindowStyleFlags.adjustclient | WindowStyleFlags.visible,
        new TemplateConnector(templateName),
        sizeParamObj,
        parent
    );
    return splitter;
}

