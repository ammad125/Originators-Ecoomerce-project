//Validations
debugger
$(function () {
    var $regForm = $("#userform");
    $.validator.addMethod("email", function (value, element) {
        return this.optional(element) || /^[a-zA-Z0-9._-]+@[a-zA-Z0-9-]+\.[a-zA-Z.]{2,5}$/i.test(value);
    }, "Email Address is invalid: Please enter a valid email address.");

    $.validator.addMethod("password", function (value, element) {
        return this.optional(element) || /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,16}$/i.test(value);
    }, "Passwords are 8-16 characters with uppercase letters, lowercase letters and at least one number.");

    $.validator.addMethod("noSpace", function (value, element) {
        return value === '' || value.trim().length !== 0
    }, "Spaces are not Allowed");

    if ($regForm.length) {
        $regForm.validate({
            rules: {
                FullName: {
                    required: true,
                    noSpace: true
                },
                username: {
                    required: true,
                    noSpace: true
                },
                email: {
                    required: true,
                    email: true
                },
                password: {
                    required: true
                },
                conf_password: {
                    required: true,
                    equalTo: '#password'
                }
            },
            messages: {
                FullName: {
                    required: 'FullName field is Required'
                },
                username: {
                    required: 'Username field is Required'
                },
                email: {
                    required: 'Email field is Required',
                    email: 'Please Enter a Valid Email'
                },
                password: {
                    required: 'Password field is Required'
                },
                conf_password: {
                    required: 'Confirm Password field is Required',
                    equalTo: 'Password & Confirm Password Must Be Same'
                }
            },
            errorPlacement: function (error, element) {
                if (element.is(":radio")) {
                    error.appendTo(element.parents(".gender"));
                }
                else if (element.is(".checkbox")) {
                    error.appendTo(element.parents(".hobbies"));
                }
                else {
                    error.insertAfter(element);
                }
            }
        })
    }
});