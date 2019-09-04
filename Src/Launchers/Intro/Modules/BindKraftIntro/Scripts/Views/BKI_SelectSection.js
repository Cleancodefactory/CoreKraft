function BKI_SelectSection() {
    GenericViewBaseEx.apply(this, arguments);
}

BKI_SelectSection.Inherit(GenericViewBaseEx, "BKI_SelectSection");
BKI_SelectSection.Implement(IDialogView);
BKI_SelectSection.ImplementActiveProperty("section", new InitializeObject("Name of the selected section", null));
BKI_SelectSection.ImplementActiveProperty("orderIdx", new InitializeNumericParameter("Difficulty level for example", 0));

BKI_SelectSection.prototype.get_caption = function () {
    return "Select Section";
};

BKI_SelectSection.prototype.InitWorkData = function (workdata) {
    this.set_data(workdata);
};

BKI_SelectSection.prototype.onSave = function () {
    var section = this.get_section();
    if (!this.onValidateUserData()) {
        return;
    }
    if (!section || section.name == "") {
        alert("Please select a section.");
        return;
    }
    section.orderIdx = Number.parseInt(this.get_orderIdx());
    this.completeDialog(true, section);
};

BKI_SelectSection.prototype.onCancel = function () {
    this.completeDialog(false);
};

BKI_SelectSection.prototype.onValidateUserData = function (e, dc, b) {
    var vr = this.validate();
    if (vr != ValidationResultEnum.correct) {
        return false;
    } else {
        return true;
    }
};