<<<<<<< HEAD
﻿function BKI_MenuItem() {
    Base.apply(this, arguments);
}

BKI_MenuItem.Inherit(Base, "BKI_MenuItem");
BKI_MenuItem.Implement(IUIControl);
BKI_MenuItem.Implement(ITemplateSourceImpl, new Defaults("templateName", "bindkraftintro/menu-item-template"));
BKI_MenuItem.$defaults = {
    templateName: "bindkraftintro/menu-item-template"
};

//Main Properties
BKI_MenuItem.ImplementProperty("currentCategory", new InitializeStringParameter("The last category selected", "Undefined"));
BKI_MenuItem.ImplementProperty("caption", new InitializeStringParameter("The caption of the item.", "Undefined"));
BKI_MenuItem.ImplementProperty("name", new InitializeStringParameter("The caption of the item.", "Undefined"));
BKI_MenuItem.ImplementProperty("svgpath", new InitializeStringParameter("The path of the item's svg.", null));
BKI_MenuItem.ImplementActiveProperty("children", new InitializeParameter("Children information(caption, click window, ...etc.).", null), null, null, "changeTemplate");

BKI_MenuItem.prototype.OnDataContextChanged = function () {
};

BKI_MenuItem.prototype.rawImagePathConverter = {
    ToTarget: function ToTarget(v) {
        var service = this.findService("BindKraftIntroApp");
        return service.resourceUrl("$images", v);
    },
    FromTarget: function FromTarget() { } // do nothing
};

BKI_MenuItem.prototype.init = function () {
    $$(this.root).first().empty().append(this.get_template());
};

BKI_MenuItem.prototype.finalinit = function () {
};

BKI_MenuItem.prototype.changeTemplate = function () {
    //Please refactor that at some point
    if (this.get_children() && this.get_children().length > 0) {
        this.child('switcher')[0].activeClass.ToggleTemplate();
    }
};

BKI_MenuItem.prototype.onClick = function (ev, dc, handler) {
    if (dc && dc.Id) {
        if (ev.target) {
            $('.bki-current-menu-item').removeClass('bki-current-menu-item');
            $(ev.target).addClass('bki-current-menu-item');
        }
        var category = "";
        if (this.findParent('menurow')) {
            category = this.findParent('menurow').get_data().Id;
        }        
        if (dc.Id && dc.Id.length > 0 && category && category.length > 0) {
            
            this.SwitchBodyWindow(dc.Id, category);
        }
    }
};

BKI_MenuItem.prototype.SwitchBodyWindow = function (exampleName, category) {
    var service = this.findService("BindKraftIntroApp");
    service.changeBodyView(exampleName, category);
};

=======
﻿function BKI_MenuItem() {
    Base.apply(this, arguments);
}

BKI_MenuItem.Inherit(Base, "BKI_MenuItem");
BKI_MenuItem.Implement(IUIControl);
BKI_MenuItem.Implement(ITemplateSourceImpl, new Defaults("templateName", "bindkraftintro/menu-item-template"));
BKI_MenuItem.$defaults = {
    templateName: "bindkraftintro/menu-item-template"
};

//Main Properties
BKI_MenuItem.ImplementProperty("currentCategory", new InitializeStringParameter("The last category selected", "Undefined"));
BKI_MenuItem.ImplementProperty("caption", new InitializeStringParameter("The caption of the item.", "Undefined"));
BKI_MenuItem.ImplementProperty("name", new InitializeStringParameter("The caption of the item.", "Undefined"));
BKI_MenuItem.ImplementProperty("svgpath", new InitializeStringParameter("The path of the item's svg.", null));
BKI_MenuItem.ImplementActiveProperty("children", new InitializeParameter("Children information(caption, click window, ...etc.).", null), null, null, "changeTemplate");

BKI_MenuItem.prototype.OnDataContextChanged = function () {
};

BKI_MenuItem.prototype.rawImagePathConverter = {
    ToTarget: function ToTarget(v) {
        var service = this.findService("BindKraftIntroApp");
        return service.resourceUrl("$images", v);
    },
    FromTarget: function FromTarget() { } // do nothing
};

BKI_MenuItem.prototype.init = function () {
    $$(this.root).first().empty().append(this.get_template());
};

BKI_MenuItem.prototype.finalinit = function () {
};

BKI_MenuItem.prototype.changeTemplate = function () {
    //Please refactor that at some point
    if (this.get_children() && this.get_children().length > 0) {
        this.child('switcher')[0].activeClass.ToggleTemplate();
    }
};

BKI_MenuItem.prototype.onClick = function (ev, dc, handler) {
    if (dc && dc.Id) {
        if (ev.target) {
            $('.bki-current-menu-item').removeClass('bki-current-menu-item');
            $(ev.target).addClass('bki-current-menu-item');
        }
        var category = "";
        if (this.findParent('menurow')) {
            category = this.findParent('menurow').get_data().Id;
        }        
        if (dc.Id && dc.Id.length > 0 && category && category.length > 0) {
            
            this.SwitchBodyWindow(dc.Id, category);
        }
    }
};

BKI_MenuItem.prototype.SwitchBodyWindow = function (exampleName, category) {
    var service = this.findService("BindKraftIntroApp");
    service.changeBodyView(exampleName, category);
};

>>>>>>> develop
