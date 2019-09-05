<<<<<<< HEAD
﻿function BKI_AdminView() {
    GenericViewBaseEx.apply(this, arguments);
}

BKI_AdminView.Inherit(GenericViewBaseEx, "BKI_AdminView");

BKI_AdminView.prototype.onDeleteAll = function (ev, dc, binding) {
    var service = this.findService("BindKraftIntroApp");
    service.deleteAllMarkedDelete();
}

=======
﻿function BKI_AdminView() {
    GenericViewBaseEx.apply(this, arguments);
}

BKI_AdminView.Inherit(GenericViewBaseEx, "BKI_AdminView");

BKI_AdminView.prototype.onDeleteAll = function (ev, dc, binding) {
    var service = this.findService("BindKraftIntroApp");
    service.deleteAllMarkedDelete();
}

>>>>>>> develop
