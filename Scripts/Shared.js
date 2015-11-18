$.extend({
    getUrlVars: function () {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    },
    getUrlVar: function (name) {
        return $.getUrlVars()[name];
    }
});

$(document).ready(function () {
    var Line = $.getUrlVar('line');

    if (Line == undefined)
        Line = '';

    $('body').append('<div id="toprightnav" style="font-size:13px;height:auto;padding:10px;margin:0px;width:150px; position:absolute;right:0px; ">' +
                            '<a href="Logout.aspx" style="width: 100%; font-size:13px!important;">Logout</a>' +
                            '<a href="DCSDemoReasonCodes.aspx?line=' + Line + '" style="margin-top:20px;font-size:13px!important;"><b>Step 1:</b>Create Reason Codes</a>' +
                            '<a href="DCSDemo.aspx?line=' + Line + '" style="margin-top:20px;font-size:13px!important;"><b>Step 2:</b>Enter Downtime Events</a>' +
                            '<a href="http://dashboard.thrivemes.com?line=' + Line + '" style="margin-top:20px;font-size:13px!important;"><b>Step 3:</b>Analyze Data in Dashboard</a>' +
                    '</div>');

    floatingMenu.add('toprightnav', {
        targetRight: 10,
        targetTop: 100,
        prohibitXMovement: false,
        snap: false
    });

    $('#toprightnav > a').button();
    $('#toprightnav > a > span').css('font-size', '13px');
});