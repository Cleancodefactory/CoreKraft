function Student() {
    GenericViewBaseEx.apply(this, arguments);
}

Student.Inherit(GenericViewBaseEx, "Student");
Student.Implement(IPlatformUtilityImpl, "FirstExample"); //provides moduleUrl
Student.ImplementActiveProperty("students", new InitializeArray());


Student.prototype.getStudents = function (e, dc, b) {
    this.ajaxPostXml(
        this.moduleUrl("read", "main", "students"), {}, function (result) {
            this.set_students(result.data);
        }
    );
};

Student.prototype.insertStudent = function (e, dc, b) {
    this.ajaxPostXml(
        this.moduleUrl("write", "main", "students"), { state: 1 }, function (result) {

        }
    );
};

Student.prototype.updateStudent = function (e, dc, b) {
    this.ajaxPostXml(
        this.moduleUrl("write", "main", "students"), { state: 2 }, function (result) {

        }
    );
};

Student.prototype.deleteStudent = function (e, dc, b) {
    this.ajaxPostXml(
        this.moduleUrl("write", "main", "students"), { state: 3 }, function (result) {

        }
    );
};