var LightBox_AssignReasonCodePopWindow = "";
var LightBox_SimulateDowntimePopWindow = "";
var timedCountTimer;
var isReload = true; //when ispostback,isReload=false
var commentDefaultString = "Operator puts in comment here.";
var lightWindowWidth = 700;

$(document).ready(function () {

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


    setDefaultStyles();

    LightBox_AssignReasonCodePopWindow = $("#AssignReasonCodePopWindow").html();
    $("#AssignReasonCodePopWindow").remove();

    $('#create_new_event').click(function () {
        $.fn.jmodal({
            initWidth: lightWindowWidth,
            title: 'Create New Event',
            content: LightBox_AssignReasonCodePopWindow,
            buttonText: 'Cancel',
            okEvent: function (e) { }
        });

        $('#start_date_time').val((new Date()).format('MM/dd/yyyy HH:mmt'));
        $('#for_Occurences').css('display', 'block');
        $('#Occurences').css('display', 'block').val(1);

        $("#btn_next_select_reason_code").unbind('click');
        $('#btn_next_select_reason_code').click(function () {
            if (checkStep0()) {
                ShowAssignReasonCodeStep(1);
            }
        });

        $("#btnSave").unbind('click');
        $('#btnSave').click(function () {
            var reason, comment, reasonid, startdatetime, minutes, line, Occurences;
            comment = $('#comment').val();
            if (comment == commentDefaultString) {
                comment = '';
            }

            reasonid = getReasonId();
            reason = getReasonPath();
            startdatetime = $('#start_date_time').val();
            minutes = $('#minutes').val();

            line = $.getUrlVar('line');

            if (line == undefined || line == '' || line == null)
                line = '1';

            Occurences = $('#Occurences').val();

            $.post("Service.ashx?op=addnewevent", "reasonId=" + reasonid + "&reasonCode=" + UrlEncode(reason) + "&comment=" + UrlEncode(comment) + "&startdatetime=" + UrlEncode(startdatetime) + "&minutes=" + UrlEncode(minutes) + "&line=" + UrlEncode(line) + "&Occurences=" + UrlEncode(Occurences), function (data) {
                if (data.toString().toLowerCase() == 'true') {
                    closeLightBox(); //close light box
                    LoadGridData(); //update the grid data
                }
                else {
                    alert('save failed.please try again.\n' + data.toString());
                }
            });
        });

        $('#comment').val(commentDefaultString);
        $("#comment").unbind('focus');
        $('#comment').focus(function () {
            if ($('#comment').val() == '' || $('#comment').val() == commentDefaultString) {
                $('#comment').val('');
            }
            $('#comment').css('color', '#000');
        });

        $("#comment").unbind('blur');
        $('#comment').blur(function () {
            if ($('#comment').val() == '' || $('#comment').val() == commentDefaultString) {
                $('#comment').val(commentDefaultString);
                $('#comment').css('color', '#ccc');
            }

        });

        ShowAssignReasonCodeStep(0);
    });

    $('#f_del_btn').click(function () {
        delRows();
    });

    $('#f_go_btn').click(function () {
        LoadGridData();
    });

    isReload = false;
    LoadGridData(); //load grid data;

    setInterval("LoadGridData()", 3000); //每分钟更新一次

    $(document).resize(function () {
        setGridHeader();
    });
});

function getReasonId() {
    var reasonid;
    if ($('[name="level3"][checked!=""]').length == 0) {
        if ($('[name="level2"][checked!=""]').length > 0) {
            reasonid = $('[name="level2"][checked!=""]').attr("reasonId");
        } else {
            reasonid = $('[name="level1"][checked!=""]').attr("reasonId");
        }
    } else {
        reasonid = $('[name="level3"][checked!=""]').attr("reasonId");
    }
    return reasonid;
}

function getReasonPath() {
    var l1, l2, l3
    l1 = $('[name="level1"][checked!=""]').val();
    l2 = $('[name="level2"][checked!=""]').val();
    l3 = $('[name="level3"][checked!=""]').val();

    if ($('[name="level3"][checked!=""]').length == 0) {
        if ($('[name="level2"][checked!=""]').length > 0) {
            return l1+ " > "+l2;
        } else {
            return l1;
        }
    } else {
        return l1 + " > " + l2 + " > " + l3;
    }
}
function LoadGridData() {
    var line = $.getUrlVar('line');

    if (line == undefined)
        line = '';

    $("#tipBox").css("display", "none");
    $('#DataGrid').attr("disabled", "disabled");
    $('#DataGridPanel').append('<div id="_loading" style="width:50px;height:50px;position:absolute;margin:200px 400px;z-index:999;">Loading...</div>');
    $.post("Service.ashx?op=loadgriddata", { startDate: $('#f_startdate').val(), endDate: $('#f_enddate').val(), line: line }, function (data) {
        $('#_loading').remove();
        var checked = getMultipleCheckedIds();
        var tb = $('#DataGrid');
        tb.attr("disabled", "");
        var row, rownum, bgcolor;
        rownum = 0;
        tb.empty();
        tb.append('<tr style="display:none;"><td class="Headings" width="130">Event Start</td><td class="Headings" width="70">Minutes</td><td class="Headings">Reason Code</td></tr>');
        $.each(data, function () {
            var ischecked = "";

            if (checked.indexOf(this.ID) != -1)
                ischecked = "checked = \"checked\"";


            if (rownum % 2 == 1) {
                bgcolor = "#ffffff";
            }
            else {
                bgcolor = "#CDCDCD";
            }
            row = "<tr>";
            row += "<td class=\"col1\">" + this.EventStartDisplay + "</td>"; //(new Date(this.EventStart)).format("MM/dd/yyyy h:mm t")
            row += "<td class=\"col2\">" + secondToString(this.Minutes * 60) + "</td>";
            if ($.trim(this.ReasonCode).length > 0) {
                row += "<td class=\"col3\"><input type=\"checkbox\" name=\"record_chk\" "+ ischecked + " class=\"reasoncheckbox\" value=\"" + this.ID + "\" /> <a href=\"javascript:AssignReasonCode(" + this.ID + ")\">" + this.ReasonCode + "</a></td>";
            }
            else {
                $("#tipBox").css("display", "block");
                row += "<td class=\"col3\"><input type=\"checkbox\" name=\"record_chk\" " + ischecked +" class=\"reasoncheckbox\" value=\"" + this.ID + "\" /> <input type='button' id='assign_reason_code' value='' onclick='AssignReasonCode(" + this.ID + ")' /></td>";
            }
            row += "</tr>";
            tb.append(row);
            rownum++;
        });

        $('.reasoncheckbox').unbind('click');
        $('.reasoncheckbox').click(function () {
            /*if ($(this).attr('checked')) {
            $(this).prev().attr("disabled", "disabled");
            } else {
            $(this).prev().removeAttr("disabled");
            }*/
            getMultipleCheckedIds();
        });

        if (data.length < 9) {
            for (var i = data.length; i < 9; i++) {
                row = "<tr>";
                row += "<td class=\"col1\">&nbsp;</td>";
                row += "<td class=\"col2\">&nbsp;</td>";
                row += "<td class=\"col3\">&nbsp;</td>"
                row += "</tr>";
                tb.append(row);
            }
        }


        if ($('#DataGridPanel').height() > 436) {
            $('#DataGridPanel').css("height", "436px");
        }
        $('#DataGridPanel').jScrollPane(
						{
						    showArrows: false,
						    scrollbarWidth: 35,
						    reinitialiseOnImageLoad: true
						}
					);

        if (!isReload) {
            /*if ($('#DataGridPanel')[0].scrollTo != null) {
            $('#DataGridPanel')[0].scrollTo($('#DataGridPanel').height());
            }*/
        }

        if ($('#DataGridPanel')[0].scrollTo) {
           $('#DataGridPanel')[0].scrollTo($('#DataGridPanel').height());
        }


        setGridHeader();
        isReload = true;
    }, "json");
}

function getMultipleCheckedIds() {
    var result = "";
    $('.reasoncheckbox:checked').each(function () {
        if (result.length > 0) {
            result += "," + $(this).val();
        } else {
            result += $(this).val();
        }
    });
    return result;
}


function checkAllIds(check) {
    $('.reasoncheckbox').each(function () {
        if(check)
            $(this).attr('checked', 'checked');
        else
            $(this).removeAttr('checked');
    });
}

function delRows() {
    var ids = "";
    $('input[type=checkbox][name=record_chk]:checked').each(function () {
        ids +=(ids==''?'':',')+ $(this).val();
    });

    if (ids.length == 0) {
        return;
    }

    if (!confirm("Are you sure you want to delete the selected records?")) {
        return;
    }

    $('#f_del_btn').attr('disabled', 'disabled');
    $.post('Service.ashx?op=delRecords', { ids: ids }, function (data) {
        if (data == 'true' || data === 'true' || data === true) {
            LoadGridData();
        } else {
            alert(data);
        }

        $('#f_del_btn').removeAttr('disabled');
    });
}

function setDefaultStyles() {
    $(':input[type=text]').each(function() {
        $(this).addClass('inputDefaultStyle');
    });
}

function setGridHeader() {
    //$("#header").width($('#DataGrid').width());
}

function InArray(ar, value, index) {
    if (ar.length < (index + 1)) return false;
    return $.trim(ar[index].toString()).toLowerCase() == $.trim(value.toString()).toLowerCase()
}

function loadLevel0() {
    return true;
}

function loadLevel1() {
    $.post("Service.ashx?op=level1", "", function(data) {
        var rg = $("#step1");
        rg.empty();
        var i = 0;
        var orgarr = $('#orgValue').val().split(' > ');
        $.each(data, function() {
        rg.append('<label><input type="radio" name="level1" reasonId="' + this.ID + '" value="' + this.Level1 + '" id="level1_' + i + '" ' + (InArray(orgarr, this.Level1, 0) ? 'checked="checked"' : '') + ' onclick="ShowAssignReasonCodeStep(2)"  />' + this.Level1 + '</label>');
            i++;
        });

        if (rg.find("input").length == 0) {
            alert('level1 is empty.');
        }
    }, "json");
}

function loadLevel2(level1) {
    $.post("Service.ashx?op=level2", "level1=" + level1, function(data) {
        var rg = $("#step2");
        rg.empty();
        var i = 0;
        var orgarr = $('#orgValue').val().split(' > ');

        $.each(data, function() {
        rg.append('<label><input type="radio" name="level2" reasonId="' + this.ID + '" value="' + this.Level2 + '" id="level2_' + i + '" ' + (InArray(orgarr, this.Level2, 1) ? 'checked="checked"' : '') + '  onclick="ShowAssignReasonCodeStep(3);" />' + this.Level2 + '</label>');
            i++;
        });

        if (rg.find("input").length == 0) {
            alert('level1 is empty.');
        }

        rg.append('<input type="hidden" value="'+$('[name="level1"][checked!=""]').val()+'" id="preLevel1" />');
    }, "json");
}

function loadLevel3(level1, level2) {
    $.post("Service.ashx?op=level3", "level1=" + level1 + "&level2=" + level2, function(data) {
        var rg = $("#step3");
        rg.empty();
        var i = 0;
        var orgarr = $('#orgValue').val().split(' > ');
        $.each(data, function() {
            rg.append('<label><input type="radio" name="level3" reasonId="' + this.ID + '" value="' + this.Level3 + '" id="level3_' + i + '" ' + (InArray(orgarr, this.Level3, 2) ? 'checked="checked"' : '') + '  onclick="ShowAssignReasonCodeStep(4)" />' + this.Level3 + '</label>');
            i++;
        });

        if (rg.find("input").length == 0) {
            alert('level1 is empty.');
        }

        rg.append('<input type="hidden" value="' + $('[name="level2"][checked!=""]').val() + '" id="preLevel2" />');
    }, "json");
}

function AssignReasonCode(id) {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        dataType: "json",
        url: "Service.ashx",
        data: { op: "getevent", id: id },
        error: function (xml) { alert('get event data failed.'); },
        timeout: 30,
        success: function (result) {
            if (result.length == 0) {
                alert('not found event data');
                return;
            }


            $.fn.jmodal({
                initWidth: lightWindowWidth,
                title: 'Event List',
                content: LightBox_AssignReasonCodePopWindow,
                buttonText: 'Cancel',
                okEvent: function (e) { }
            });

            var orgvalue, comment, startdate, minutes, line;
            orgvalue = result.ReasonCode;
            comment = result.Comment;
            startdate = (new Date(result.EventStart));
            minutes = result.Minutes;
            line = result.Line;
            $('#for_Occurences').css('display', 'none');
            $('#Occurences').css('display', 'none').val(1);
            $('#id').val(id); // set id value
            $('#orgValue').val(orgvalue); //set original step value
            if ($.trim(comment).length > 0) {
                $('#comment').val(comment); //set original comment value
                $('#comment').css("color", "#000000");
            }

            //$('#start_date_time').val(startdate.format('MM/dd/yyyy HH:mmt'));
            $('#start_date_time').val(result.EventStartDisplay);

            $('#minutes').val(minutes);

            $('#line').val(line);
            if ($('#line').val() != line && $.trim(line).length > 0) {
                $('#line').append('<option value="' + line + '">' + line + '</option>');
                $('#line').val(line);
            }

            $("#btn_next_select_reason_code").unbind('click');
            $('#btn_next_select_reason_code').click(function () {
                if (checkStep0()) {
                    ShowAssignReasonCodeStep(1);
                }
            });

            $('#btnSave').unbind('click');
            $('#btnSave').click(function () {
                if (checkStep0()) {
                    ShowAssignReasonCodeStep(1);
                }
                var reason, comment, reasonid, startdatetime, minutes, line
                comment = $('#comment').val();
                if (comment == commentDefaultString) {
                    comment = '';
                }

                reasonid = getReasonId();
                reason = getReasonPath();
                startdatetime = $('#start_date_time').val();
                //Set to not change minutes
                minutes = 'tv'; //$('#minutes').val();

                var line = $.getUrlVar('line');

                if (line == undefined || line == null || line == '')
                    line = '1';

                var postIds = getMultipleCheckedIds();
                if (postIds.length == 0) postIds = $('#id').val();


                $.post("Service.ashx?op=save", "id=" + postIds + "&editId=" + $('#id').val() + "&reasonId=" + reasonid + "&reasonCode=" + UrlEncode(reason) + "&comment=" + UrlEncode(comment) + "&startdatetime=" + UrlEncode(startdatetime) + "&minutes=" + UrlEncode(minutes) + "&line=" + UrlEncode(line), function (data) {
                    if (data.toString().toLowerCase() == 'true') {
                        //alert('update has been successfully.');
                        closeLightBox();
                        LoadGridData(); //update the grid data
                        checkAllIds(false);
                    }
                    else {
                        alert('update failed.please try again.');
                    }
                });
            });

            $('#comment').unbind('focus');
            $('#comment').focus(function () {
                if ($('#comment').val() == '' || $('#comment').val() == 'Operator puts in comment here.') {
                    $('#comment').val('');
                }
                $('#comment').css('color', '#000');
            });

            $('#comment').unbind('blur');
            $('#comment').blur(function () {
                if ($('#comment').val() == '' || $('#comment').val() == 'Operator puts in comment here.') {
                    $('#comment').val('Operator puts in comment here.');
                    $('#comment').css('color', '#ccc');
                }

            });
            //ShowAssignReasonCodeStep(0);//默认step
            ShowAssignReasonCodeStep(1);

        }
    });
    
    
}

function showAssignReasonCodeStepTitle(num, dataSourceLength) {
    var l1, l2, l3;
    l1 = $('[name=level1][checked!=""]');
    l2 = $('[name=level2][checked!=""]');
    l3 = $('[name=level3][checked!=""]');


    switch (num) {
        case 0:
            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > Date Time');
            break;
        case 1:
            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(0)">Date Time</a> > 1st Level');
            break;
        case 2:
            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(1)">' + l1.val() + '</a> > 2nd Level');

            break;
        case 3:
            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(1)">' + l1.val() + '</a> > <a href="javascript:ShowAssignReasonCodeStep(2)">' + l2.val() + '</a> > 3rd Level');
            break;
        case 4:
            if (l2.length == 0) {
                setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(1)">' + l1.val() + '</a> > Comment');
                break;
            }
            if (l3.length == 0) {
                setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(1)">' + l1.val() + '</a> > <a href="javascript:ShowAssignReasonCodeStep(2)">' + l2.val() + '</a> > Comment');
                break;
            }

            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(1)">' + l1.val() + '</a> > <a href="javascript:ShowAssignReasonCodeStep(2)">' + l2.val() + '</a> > <a href="javascript:ShowAssignReasonCodeStep(3)">' + l3.val() + '</a> > Comment');

            break;
    }
}

function ShowAssignReasonCodeStep(num) {

    var dataSourceLength = 0;
    if (num == 2 || num == 3) 
    {
        $.ajax({
            async: false,
            cache: false,
            type: "POST",
            dataType: "html",
            url: "Service.ashx",
            data: { op: "getLevelDataSourceLength", level1: $('[name="level1"][checked!=""]').val(), level2: (num==3?$('[name="level2"][checked!=""]').val():''), department: $('#lineDepartment').val() },
            error: function(xml) { alert('get level data source length failed.'); dataSourceLength = 0; },
            timeout: 30,
            success: function(result) { dataSourceLength = parseInt(result);}
        });

    }
    

    
    switch (num) {
        case 0:
            loadLevel0();
            break;
        case 1:

            if ($("[name='level1'][checked!='']").size() == 0) {
                loadLevel1();
            }
            break;
        case 2:
            if (dataSourceLength > 0) {
                var l1, l2;
                l1 = $('[name="level1"][checked!=""]');
                if (l1.length > 0) {
                    l2 = $('[name="level2"][checked!=""]');
                    if (l2.length == 0 || $('#preLevel1').val() != l1.val()) {
                        loadLevel2(l1.val());
                    }
                }
                else {
                    alert('your must select 1st Level.');
                    ShowAssignReasonCodeStep(1);
                    return;
                }
            } else {
                $('#step2').empty();
                $('#step3').empty();
                ShowAssignReasonCodeStep(4);
                return;
            }
            break;
        case 3:
            if (dataSourceLength > 0) {
                var l1, l2, l3;

                l1 = $('[name=level1][checked!=""]');
                l2 = $('[name=level2][checked!=""]');

                if (l1.length == 0) {
                    alert('your must select 1st Level.');
                    ShowAssignReasonCodeStep(1);
                    return;
                }

                if (l2.length == 0) {
                    alert('your must select 2nd Level.');
                    ShowAssignReasonCodeStep(2);
                    return;
                }

                l3 = $('[name="level3"][checked!=""]');
                if (l3.length == 0 || $('#preLevel2').val() != l2.val()) {
                    loadLevel3(l1.val(), l2.val());
                }
            } else {
                $('#step3').empty();
                ShowAssignReasonCodeStep(4);
                return;
            }
            break;
        case 4:
            var l1, l2, l3;
            l1 = $('[name=level1][checked!=""]');
            l2 = $('[name=level2][checked!=""]');
            l3 = $('[name=level3][checked!=""]');

            if (l1.length == 0) {
                alert('your must select 1st Level.');
                ShowAssignReasonCodeStep(1);
                return;
            }

//            if (l2.length == 0) {
//                alert('your must select 2nd Level.');
//                ShowAssignReasonCodeStep(2);
//                return;
//            }

//            if (l3.length == 0) {
//                alert('your must select 3rd Level.');
//                ShowAssignReasonCodeStep(3);
//                return;
//            }


            break;
    }

    showAssignReasonCodeStepTitle(num);


    for (var i = 0; i <= 4; i++) {
        if (num != i) {
            $("#step" + i).css("display", "none");
        }
        else {
            $("#step" + i).css("display", "block");
        }
    }
}

function setLightBoxTitle(html) {
    $("#jmodal-container-title").html(html);
}

function closeLightBox() {
    $("#jmodal-bottom-okbutton").click();
}

function SimulateDowntime() {
    $.fn.jmodal({
        initWidth: 300,
        title: 'Downtime Simulation',
        content: LightBox_SimulateDowntimePopWindow,
        buttonText: 'Cancel',
        okEvent: function(e) { }
    });

    $("#btnStart").attr("disabled", "");
    $("#btnStop").attr("disabled", "disabled");
}
function Start() {
    $("#StartTime").val((new Date()).format("MM/dd/yyyy HH:mm:ss"));
    $("#btnStart").attr("disabled", "disabled");
    $("#btnStop").attr("disabled", "");
    timedCount();
}
function Stop() {
    clearTimeout(timedCountTimer);
    $('#secondCount').val(0);
    $('#btnStop').val("Save");
    var st = $("#StartTime").val();
    var et = (new Date()).format("MM/dd/yyyy HH:mm:ss");
    $.post("Service.ashx?op=stop", "start=" + st + "&stop=" + et, function(date) {
        if (date.toString().toLowerCase() == 'true') {
            //alert('insert row has been successfully.');
            closeLightBox();
            isReload = false;
            LoadGridData(); //update the grid data
        } else {
            alert('insert row failed,please try again.');
        }
    });
}

Date.prototype.format = function(format) {
    var o = {

        "M+": this.getMonth() + 1, //month   
        "d+": this.getDate(), //day
        "h+": (this.getHours() > 12 ? this.getHours() - 12 : this.getHours()), //hour
        "H+": this.getHours(), //hour   
        "m+": this.getMinutes(), //minute   
        "s+": this.getSeconds(), //second   
        "q+": Math.floor((this.getMonth() + 3) / 3), //quarter   
        "S": this.getMilliseconds(), //millisecond   
        "t": (this.getHours() <= 12 ? "AM" : "PM")
    }

    if (/(y+)/.test(format))
        format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(format))
        format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
    return format;
}

function secondToString(v) {
    v = v.toFixed();
    _m = v % 3600;
    s = '00' + (_m % 60).toFixed();

    if (_m > 0) {
        h = '00' + ((v - _m) / 3600);
    } else {
        h = '00' + (v / 3600);
    }
    m = '00' + (_m - s) / 60;
    return h.substring(h.length - 2) + ':' + m.substring(m.length - 2) + ':' + s.substring(s.length - 2);

}

Number.prototype.toFixed = function(d) {
    var s = this + ""; if (!d) d = 0;
    if (s.indexOf(".") == -1) s += "."; s += new Array(d + 1).join("0");
    if (new RegExp("^(-|\\+)?(\\d+(\\.\\d{0," + (d + 1) + "})?)\\d*$").test(s)) {
        var s = "0" + RegExp.$2, pm = RegExp.$1, a = RegExp.$3.length, b = true;
        if (a == d + 2) {
            a = s.match(/\d/g); if (parseInt(a[a.length - 1]) > 4) {
                for (var i = a.length - 2; i >= 0; i--) {
                    a[i] = parseInt(a[i]) + 1;
                    if (a[i] == 10) { a[i] = 0; b = i != 1; } else break;
                }
            }
            s = a.join("").replace(new RegExp("(\\d+)(\\d{" + d + "})\\d$"), "$1.$2");
        } if (b) s = s.substr(1); return (pm + s).replace(/\.$/, "");
    } return this + "";
};


function UrlEncode(str) {
    var ret = "";
    var strSpecial = "!\"#$%&'()*+,/:;<=>?[]^`{|}~%";
    for (var i = 0; i < str.length; i++) {
        var chr = str.charAt(i);
        var c = str2asc(chr);
        if (parseInt("0x" + c) > 0x7f) {
            ret += "%" + c.slice(0, 2) + "%" + c.slice(-2);
        } else {
            if (chr == " ")
                ret += "+";
            else if (strSpecial.indexOf(chr) != -1)
                ret += "%" + c.toString(16);
            else
                ret += chr;
        }
    }
    return ret;
}


function str2asc(ch) {
    var Strob = new String(ch);
    var sstr = "";
    for (var i = 0; i < Strob.length; i++) {
        sstr += new String(Strob.substr(i, 1)).charCodeAt(0).toString(16);
    }
    return sstr;

}


function timedCount() {
    var seconds = parseInt($('#secondCount').val());
    var str = secondToString(seconds);
    $("#timerSpan").val(str);
    seconds += 1;
    $('#secondCount').val(seconds);
    timedCountTimer = setTimeout("timedCount();", 1000)
}

function checkStep0() {
    if ($.trim($('#start_date_time').val()).length == 0) {
        alert('Start Date & Time is required.');
        alert($('#start_date_time').val());
        $('#start_date_time').get(0).focus();
        return false;
    }


    if (isNaN($('#minutes').val()) || $.trim($('#minutes').val()).length==0) {
        alert('Minutes is required and must be a decimal.');
        $('#minutes').get(0).select();
        $('#minutes').get(0).focus();
        return false;
    }


    if (isNaN($('#Occurences').val()) || $.trim($('#Occurences').val()).length == 0) {
        alert('Occurences is required and must be a number.');
        $('#Occurences').get(0).select();
        $('#Occurences').get(0).focus();
        return false;
    }

    var occurences = parseInt($('#Occurences').val());
    if (occurences <= 0) {
        alert('Occurences must be greater than 0.');
        $('#Occurences').get(0).select();
        $('#Occurences').get(0).focus();
        return false;
    }
    
    if ($.trim($('#line').val()).length == 0) {
        alert('Line is required.');
        $('#line').get(0).focus();
        return false;
    }

    return true;
}
