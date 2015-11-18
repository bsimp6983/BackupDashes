var checkPassword = function (fn) {

    $.post('Service.ashx?op=getadminpassword', function (adminPassword) {
        var passed = false;

        if (adminPassword) {
            var password = prompt('Please enter in Admin Password', '');

            if (adminPassword === password)
                passed = true;
        } else {//None was set
            passed = true;
        }

        if (fn) {
            fn(passed);
        }

    }, 'text');
};