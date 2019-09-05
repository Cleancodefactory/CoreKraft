<<<<<<< HEAD
﻿function BKI_Menu() {
    Base.apply(this, arguments);
}

BKI_Menu.Inherit(Base, "BKI_Menu");
BKI_Menu.Implement(IUIControl);
BKI_Menu.Implement(ITemplateSourceImpl, new Defaults("templateName", "bindkraftintro/menu-template"));
BKI_Menu.$defaults = {
    templateName: "bindkraftintro/menu-template"
};

BKI_Menu.ImplementProperty("itemsdata", new InitializeParameter("Data used to build the menu and its functionality.", null));

BKI_Menu.prototype.init = function () {
    $$(this.root).first().empty().append(this.get_template());
};

BKI_Menu.prototype.OnBeforeDataContextChanged = function () {
    var service = this.findService("BindKraftIntroApp");
    if (service.get_isAdmin()) {
        this.child('adminSwitcher')[0].activeClass.ToggleTemplate();
    }
    if (this.get_data()) {
        this.$itemsdata = this.get_data().intro.Sections;
        this.updateSources();
        this.updateTargets();
    }
    var sections = [];
    for (var i = 0; i < this.$itemsdata.length; i++) {
        if (this.$itemsdata[i].Id != "ForReview") {
            sections.push({ name: this.$itemsdata[i].Id });
        }
    }
    service.set_sections(sections);
};

BKI_Menu.prototype.onCreate = function (e, dc, b) {
    var service = this.findService("BindKraftIntroApp");
    service.openCreateView();
};

BKI_Menu.prototype.onOpenAdminView = function (e, dc, b) {
    var service = this.findService("BindKraftIntroApp");
    service.openAdminView();
};
=======
﻿function BKI_Menu() {
    Base.apply(this, arguments);
}

BKI_Menu.Inherit(Base, "BKI_Menu");
BKI_Menu.Implement(IUIControl);
BKI_Menu.Implement(ITemplateSourceImpl, new Defaults("templateName", "bindkraftintro/menu-template"));
BKI_Menu.$defaults = {
    templateName: "bindkraftintro/menu-template"
};

BKI_Menu.ImplementProperty("itemsdata", new InitializeParameter("Data used to build the menu and its functionality.", null));

BKI_Menu.prototype.init = function () {
    $$(this.root).first().empty().append(this.get_template());
};

BKI_Menu.prototype.OnBeforeDataContextChanged = function () {
    var service = this.findService("BindKraftIntroApp");
    if (service.get_isAdmin()) {
        this.child('adminSwitcher')[0].activeClass.ToggleTemplate();
    }
    if (this.get_data()) {
        this.$itemsdata = this.get_data().intro.Sections;
        this.updateSources();
        this.updateTargets();
    }
    var sections = [];
    for (var i = 0; i < this.$itemsdata.length; i++) {
        if (this.$itemsdata[i].Id != "ForReview") {
            sections.push({ name: this.$itemsdata[i].Id });
        }
    }
    service.set_sections(sections);
};

BKI_Menu.prototype.onCreate = function (e, dc, b) {
    var service = this.findService("BindKraftIntroApp");
    service.openCreateView();
};

BKI_Menu.prototype.onOpenAdminView = function (e, dc, b) {
    var service = this.findService("BindKraftIntroApp");
    service.openAdminView();
};
>>>>>>> develop
