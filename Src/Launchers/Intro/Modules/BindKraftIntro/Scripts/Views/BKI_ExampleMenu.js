function BKI_ExampleMenu() {
    GenericViewBaseEx.apply(this, arguments);
}


BKI_ExampleMenu.Inherit(GenericViewBaseEx, "BKI_ExampleMenu");

BKI_ExampleMenu.prototype.OnDataContextChanged = function () {
};

BKI_ExampleMenu.prototype.onTemplateSelect = function (sender) {
    var item = sender.get_item().example;
    var service = this.findService("BindKraftIntroApp");
    if (service.get_isAdmin() && item.forReview) {
        return sender.getTemplateByKey("adminReview");
    } else if (service.get_isAdmin()) {
        return sender.getTemplateByKey("admin");
    } else if (item.state && item.state == "1") {
        return sender.getTemplateByKey("userCreate");
    } else {
        return sender.getTemplateByKey("user");
    }
};

BKI_ExampleMenu.prototype.onFileSelect = function (ev, dc, binding) {
    var service = this.findService("BindKraftIntroApp");
    $$('.bk-margin-large.bki-current-menu-item').removeClasses('bki-current-menu-item');
    $$(ev.currentTarget).first().classes('bki-current-menu-item');
    service.changeCentralWindowData(dc);
};

BKI_ExampleMenu.prototype.onRunExample = function (ev, dc, binding) {
    var service = this.findService("BindKraftIntroApp");
    service.refreshExample(dc);
    $$('.bk-margin-large.bki-current-menu-item').removeClasses('bki-current-menu-item');
};

BKI_ExampleMenu.prototype.onSaveExample = function (ev, dc, binding) {
    var service = this.findService("BindKraftIntroApp");
    service.insertExample(dc.example.state);
};

BKI_ExampleMenu.prototype.onDeleteExample = function (ev, dc, binding) {
    var service = this.findService("BindKraftIntroApp");
    service.deleteExample(dc);
};


BKI_ExampleMenu.prototype.onEditExample = function (ev, dc, binding) {
    var service = this.findService("BindKraftIntroApp");
    service.openCreateView(dc.example);
};

BKI_ExampleMenu.prototype.onApproveExample = function (ev, dc, binding) {
    var service = this.findService("BindKraftIntroApp");
    service.openSelectSection();
};