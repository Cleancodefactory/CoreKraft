<<<<<<< HEAD
﻿function BKI_EditFileView() {
    GenericViewBaseEx.apply(this, arguments);
}

BKI_EditFileView.Inherit(GenericViewBaseEx, "BKI_EditFileView");


BKI_EditFileView.prototype.OnDataContextChanged = function () {
}

BKI_EditFileView.prototype.onChangeSource = function (ev, dc, binding) {
    this.updateSources();
    var service = this.findService("BindKraftIntroApp");
    service.changeCurrentExampleSource(dc);
=======
﻿function BKI_EditFileView() {
    GenericViewBaseEx.apply(this, arguments);
}

BKI_EditFileView.Inherit(GenericViewBaseEx, "BKI_EditFileView");


BKI_EditFileView.prototype.OnDataContextChanged = function () {
}

BKI_EditFileView.prototype.onChangeSource = function (ev, dc, binding) {
    this.updateSources();
    var service = this.findService("BindKraftIntroApp");
    service.changeCurrentExampleSource(dc);
>>>>>>> develop
}