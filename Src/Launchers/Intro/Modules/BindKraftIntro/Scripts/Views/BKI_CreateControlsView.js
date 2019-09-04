function BKI_CreateControlsView() {
    GenericViewBaseEx.apply(this, arguments);
}

BKI_CreateControlsView.Inherit(GenericViewBaseEx, "BKI_CreateControlsView");
BKI_CreateControlsView.ImplementProperty("errorMessage", new InitializeStringParameter("To hold the error msg", null));
BKI_CreateControlsView.prototype.app = null;

BKI_CreateControlsView.prototype.finalinit = function () {
    if (this.app == null) {
        this.app = this.findService("BindKraftIntroApp");
    }
};

BKI_CreateControlsView.prototype.addFileExtensions = function (example) {
    for (var i = 0; i < example.length; i++) {
        switch (example[i].Type) {
            case 1:
                if (!example[i].EntryName.includes(".html")) {

                    example[i].EntryName = example[i].EntryName + ".html";
                }
                break;
            case 2:
                if (!example[i].EntryName.includes(".js")) {

                    example[i].EntryName = example[i].EntryName + ".js";
                }
                break;
            case 3:
                if (!example[i].EntryName.includes(".json")) {

                    example[i].EntryName = example[i].EntryName + ".json";
                }
                break;
            case 4:
                if (!example[i].EntryName.includes(".md")) {

                    example[i].EntryName = example[i].EntryName + ".md";
                }
                break;
            default: return false;
        }
    }
    return true;
};

BKI_CreateControlsView.prototype.validateExample = function (example) {
    var htmls = example.Sources.Entries.filter(this.app.isView);
    var hasHtml = false;
    for (var i = 0; i < htmls.length; i++) {
        if (htmls[i].Content != "") {
            hasHtml = true;
            break;
        }
    }
    if (!hasHtml) {
        this.set_errorMessage("Can not run an example without html.");
        return false;
    }

    var valid = this.app.secondarySplitter.$leftWindow.currentView.onValidateUserData();
    if (!valid) {
        this.set_errorMessage("Please fill all required filds.");
        return false;
    }

    if (!this.addFileExtensions(example.Sources.Entries)) {
        this.set_errorMessage("Unknown file type.");
        return false;
    }
    return true;
};

BKI_CreateControlsView.prototype.onRunExample = function (e, dc, b) {
    if (!this.validateExample(dc)) {
        alert(this.get_errorMessage());
        return;
    }
    this.app.runExample(dc);
};