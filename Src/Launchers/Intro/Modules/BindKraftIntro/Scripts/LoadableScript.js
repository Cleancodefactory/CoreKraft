function LoadableScript(scr) {
    BaseObject.apply(this, arguments);
    if (scr != null) {
        this.$script = scr + "";
    }
}
LoadableScript.Inherit(BaseObject, "LoadableScript");
LoadableScript.prototype.$script = null;
LoadableScript.prototype.get_script = function () {
    return this.$script;
};
LoadableScript.prototype.set_script = function (v) {
    this.$script = null;
    if (v != null) {
        this.$script = v + "";
    }
};
LoadableScript.prototype.$element = null; // Here will come reference to the <script> element
LoadableScript.prototype.load = function () {
    if (typeof this.$script != "string" && this.$script.length == 0) return false;
    var el = document.createElement("script");
    el.textContent = this.$script;
    document.head.appendChild(el);
    document.head.removeChild(el);
    return true;
};