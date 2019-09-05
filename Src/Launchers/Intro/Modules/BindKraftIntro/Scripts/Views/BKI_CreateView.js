<<<<<<< HEAD
﻿function BKI_CreateView() {
    GenericViewBaseEx.apply(this, arguments);
}

BKI_CreateView.Inherit(GenericViewBaseEx, "BKI_CreateView");
BKI_CreateView.ImplementProperty("title", new InitializeStringParameter("Current view title", "Create Example"));

BKI_CreateView.prototype.finalinit = function () {
    var state = this.get_dataContext().directData.state;
    if (state && state == "2") {
        this.set_title("Edit Example");
        this.updateTargets();
    }
};

BKI_CreateView.prototype.onTemplateSelect = function (sender) {
    var type = sender.get_item().Type;
    switch (type) {
        case 1: return sender.getTemplateByKey("html");
        case 2: return sender.getTemplateByKey("js");
        case 3: return sender.getTemplateByKey("json");
        case 4: return sender.getTemplateByKey("doc");
        default: alert("Error on selecting template.");
    }
};

BKI_CreateView.prototype.onDeleteSource = function (e, dc, b) {
    var example = this.get_data();
    var index = -1;
    for (var i = 0; i < example.Sources.Entries.length; i++) {
        entry = example.Sources.Entries[i]
        if (entry.Content == dc.Content && entry.Type == dc.Type && entry.EntryName == dc.EntryName) {
            index = i;
            break;
        }
    }
    if (index != -1) {
        example.Sources.Entries.splice(index, 1);
        this.set_data(example);
        this.updateTargets("sources");
    } else {
        alert("Error on removing source entry.");
    }
};

BKI_CreateView.prototype.onAddSource = function (e, dc, b) {
    var example = this.get_data();
    switch (b.bindingParameter) {
        case "1":
            example.Sources.Entries.unshift({ Content: "", Type: 1, EntryName: "" });
            break;
        case "2":
            example.Sources.Entries.unshift({ Content: "", Type: 2, EntryName: "" });
            break;
        case "3":
            example.Sources.Entries.unshift({ Content: "", Type: 3, EntryName: "" });
            break;
        default: return alert("Error on adding source entry.");
    }
    this.set_data(example);
    this.updateTargets("sources");
};

BKI_CreateView.prototype.onValidateUserData = function (e, dc, b) {
    var vr = this.validate();
    if (vr != ValidationResultEnum.correct) {
        return false;
    } else {
        return true;
    }
=======
﻿function BKI_CreateView() {
    GenericViewBaseEx.apply(this, arguments);
}

BKI_CreateView.Inherit(GenericViewBaseEx, "BKI_CreateView");
BKI_CreateView.ImplementProperty("title", new InitializeStringParameter("Current view title", "Create Example"));

BKI_CreateView.prototype.finalinit = function () {
    var state = this.get_dataContext().directData.state;
    if (state && state == "2") {
        this.set_title("Edit Example");
        this.updateTargets();
    }
};

BKI_CreateView.prototype.onTemplateSelect = function (sender) {
    var type = sender.get_item().Type;
    switch (type) {
        case 1: return sender.getTemplateByKey("html");
        case 2: return sender.getTemplateByKey("js");
        case 3: return sender.getTemplateByKey("json");
        case 4: return sender.getTemplateByKey("doc");
        default: alert("Error on selecting template.");
    }
};

BKI_CreateView.prototype.onDeleteSource = function (e, dc, b) {
    var example = this.get_data();
    var index = -1;
    for (var i = 0; i < example.Sources.Entries.length; i++) {
        entry = example.Sources.Entries[i]
        if (entry.Content == dc.Content && entry.Type == dc.Type && entry.EntryName == dc.EntryName) {
            index = i;
            break;
        }
    }
    if (index != -1) {
        example.Sources.Entries.splice(index, 1);
        this.set_data(example);
        this.updateTargets("sources");
    } else {
        alert("Error on removing source entry.");
    }
};

BKI_CreateView.prototype.onAddSource = function (e, dc, b) {
    var example = this.get_data();
    switch (b.bindingParameter) {
        case "1":
            example.Sources.Entries.unshift({ Content: "", Type: 1, EntryName: "" });
            break;
        case "2":
            example.Sources.Entries.unshift({ Content: "", Type: 2, EntryName: "" });
            break;
        case "3":
            example.Sources.Entries.unshift({ Content: "", Type: 3, EntryName: "" });
            break;
        default: return alert("Error on adding source entry.");
    }
    this.set_data(example);
    this.updateTargets("sources");
};

BKI_CreateView.prototype.onValidateUserData = function (e, dc, b) {
    var vr = this.validate();
    if (vr != ValidationResultEnum.correct) {
        return false;
    } else {
        return true;
    }
>>>>>>> develop
};