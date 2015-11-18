var getUser = function (fn) {

    $.post('Service.ashx?op=getuser', function (user) {

        if (fn) {
            fn(user);
        }

    }, 'json');
};