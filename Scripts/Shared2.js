/*
    For Line Efficiency

*/

$(document).ready(function () {
    $('body').append('<div id="toprightnav" style="font-size:13px;height:auto;padding:10px;margin:0px;width:150px; position:absolute;right:0px; ">' +
                            '<a href="Logout.aspx" style="width: 100%; font-size:13px!important;">Logout</a>' +
                            '<a href="DCSDemoReasonCodes.aspx" style="margin-top:20px;font-size:13px!important;"><b>Step 1:</b>Create Reason Codes</a>' +
                            '<a href="DCSDemo.aspx" style="margin-top:20px;font-size:13px!important;"><b>Step 2:</b>Enter Downtime Events</a>' +
                            '<a href="http://dashboard.thrivemes.com" style="margin-top:20px;font-size:13px!important;"><b>Step 3:</b>Analyze Data in Dashboard</a>' +
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