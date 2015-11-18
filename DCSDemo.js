var LightBox_AssignReasonCodePopWindow = "";
var LightBox_SimulateDowntimePopWindow = "";
var LightBox_SplitDowntimesPopWindow = "";
var timedCountTimer;
var isReload = true; //when ispostback,isReload=false
var commentDefaultString = "Operator puts in comment here.";
var lightWindowWidth = 700;
var Line = 'company-demo';
var isChangeOver = false;
var tpId = 0;
var op1Id = 0;
var op2Id = 0;
var OptionInfo = {};
var filteredId = 0;
var wasPlannedWarned = false;

//alert("new version");
var EventTimer = {
    dtId: 0,
    started: false,
    startTime: null,
    interval: null,
    hours: 0,
    minutes: 0,
    seconds: 0,
    start: function (sd) {

        if (sd) {

            this.startTime = moment(sd);


            if (this.started == true) {
                this.started = false;

                clearInterval(this.interval);
            }

            if (this.started == false) {
                var timer = this;

                this.interval = setInterval(function () {

                    var date = moment();

                    this.hours = date.diff(timer.startTime, 'hours');

                    this.minutes = date.diff(timer.startTime, 'minutes');

                    this.seconds = (((date.diff(timer.startTime, 'seconds') % 31536000) % 86400) % 3600) % 60;

                    if (this.seconds <= 30)
                        this.seconds = this.seconds + 30;
                    else
                        this.seconds = this.seconds - 30;

                    if (this.seconds == 60)
                        this.seconds = 0;

                    //console.log(date.diff(timer.startTime, 'seconds') + ' - ' + (this.minutes * 60));

                    $("#minutes").val(minutes);

                    $('#lblTimer').text(hours + ':' + minutes + ':' + seconds);

                }, 1000);

            }

        }



    },
    stop: function () {
        this.started = false;

        clearInterval(this.interval);
    },
    tick: function () {

    }

};

$(document).ready(function () {

    Line = $.getUrlVar('line');

    if (Line == undefined || Line == '' || Line == null)
        Line = 'company-demo';

    setDefaultStyles();

    document.title += ' | ' + Line;

    $('#f_startdate').datetimepicker();
    $('#f_enddate').datetimepicker();

    LightBox_AssignReasonCodePopWindow = $("#AssignReasonCodePopWindow").html();
    $("#AssignReasonCodePopWindow").remove();

    LightBox_SplitDowntimesPopWindow = $("#SplitDowntimesPopWindow").html();
    $("#SplitDowntimesPopWindow").remove();

    $('#drpLights').change(function () {

        TurnLights($(this).val());

    });

    $('#f_search_btn').click(function () {
        var dtId = prompt("Search for Downtime ID:", '');
        filteredId = dtId;

        var dataGrid = $('#DataGrid');

        dataGrid.find('.col1').each(function () {
            var id = $(this).data('id');

            if (dtId > 0) {
                if (id == dtId) {
                    $(this).parent().show();
                } else {
                    $(this).parent().hide();
                }
            }
            else if (dtId == '') {
                $(this).parent().show();
            }
        });
    });

    $('#create_new_event').click(function () {

        $.fn.jmodal({
            initWidth: lightWindowWidth,
            title: 'Create New Event',
            content: LightBox_AssignReasonCodePopWindow,
            buttonText: 'Cancel',
            okEvent: function (e) { }
        });

        $('#start_date_time').datetimepicker();

        $('#startTimer').css('display', '');

        var startTime = new Date();

        $('#start_date_time').val(moment().format('M/D/YYYY hh:mm A'));

        $('#startTimer').click(function () {
            var minutes = 0;
            var dt = null;

            $(this).attr('disabled', 'disabled');

            CreateDTWithNoStop($('#start_date_time').val(), function (newDt) {
                if (newDt.ID) {
                    dt = newDt;

                    closeLightBox();

                    //EventTimer.start(dt.EventStart);
                }
            });

            if (dt)
                minutes = dt.Minutes;
        });

        $('#for_Occurences').css('display', 'block');
        $('#Occurences').css('display', 'block').val(1);

        //$('#startTimer').attr('disabled', 'disabled');

        //$('#start_date_time').val((new Date(dt.EventStart)).format('MM/dd/yyyy HH:mmt'));

        $("#btn_next_select_reason_code").unbind('click');
        $('#btn_next_select_reason_code').click(function () {
            if (checkStep0()) {

                ShowAssignReasonCodeStep(false, 1);
            }
        });

        populateDropboxes();

        /*
        getLatestThroughput(function (d) {
        populateDropboxes(d);
        });
        */

        $("#btnSave").unbind('click');
        $('#btnSave').click(function () {
            var reason, comment, reasonid, startdatetime, minutes, line, Occurences, throughput, option1, option2;
            comment = $('#comment').val();
            if (comment == commentDefaultString) {
                comment = '';
            }

            reasonid = getReasonId();
            reason = getReasonPath();

            minutes = $('#minutes').val();

            throughput = $('#slThroughput').val();
            option1 = $('#slOption1').val();
            option2 = $('#slOption2').val();

            if (throughput == undefined)
                throughput = -1;

            if (option1 == undefined)
                option1 = -1;

            if (option2 == undefined)
                option2 = -1;

            line = $.getUrlVar('line');

            if (line == undefined || line == '' || line == null)
                line = 'company-demo';

            Occurences = $('#Occurences').val();
            startdatetime = $('#start_date_time').val();

            var save = true;

            for (var x = 0; x < OptionInfo.length; x++) {
                var opt = OptionInfo[x];

                if (opt.Number == 1 && opt.IsRequired && (option1 <= 0 || !option1)) {
                    alert('Need to select ' + opt.Name);
                    save = false;
                    break;
                }
                else if (opt.Number == 2 && opt.IsRequired && (option2 <= 0 || !option2)) {
                    alert('Need to select ' + opt.Name);
                    save = false;
                    break;
                }
            }

            if (save) {
                $.post("Service.ashx?op=addnewevent", "reasonId=" + reasonid + "&reasonCode=" + UrlEncode(reason) + "&comment=" + UrlEncode(comment) + "&startdatetime=" + UrlEncode(startdatetime) + "&minutes=" + UrlEncode(minutes) + "&line=" + UrlEncode(line) + "&Occurences=" + UrlEncode(Occurences) + "&throughput=" + UrlEncode(throughput) + "&option1=" + UrlEncode(option1) + "&option2=" + UrlEncode(option2), function (data) {
                    if (data.toString().toLowerCase() == 'true') {
                        closeLightBox(); //close light box
                        LoadGridData(); //update the grid data
                    }
                    else {
                        alert('save failed.please try again.\n' + data.toString());
                    }
                });
            }
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

        ShowAssignReasonCodeStep(false, 0);
    });

    $('#f_del_btn').click(function () {
        delRows();
    });

    $('#f_mrg_btn').click(function () {
        mrgRows();
    });

    $('#f_go_btn').click(function () {
        LoadGridData();
    });

    $('#f_splt_btn').click(function() {
        splitrows();
    });

    isReload = false;
    LoadGridData(); //load grid data;

    setInterval("LoadGridData()", 1000 * 120); //Originally 3000, then 30,000

    $(document).resize(function () {
        setGridHeader();
    });
});

function GetLatestDTWithNoStop(cb) {
    $.get('Service.ashx', { op: 'GetLatestDTWithNoStop', line: Line }, function (response) {
        if (cb) {
            cb(response);
        }
    });
}

function CreateDTWithNoStop(sd, cb) {
    if (sd) {
        $.get('Service.ashx', { op: 'CreateDTWithNoStop', line: Line, sd: sd }, function (response) {
            if (cb) {
                cb(response);
            }
        });

    }
}

function StopEventTimer(id, es) {
    $.get('Service.ashx', { op: 'UpdateDTWithNoStop', id: id, es: es }, function (response) {
        if (cb) {
            cb(response);
        }
    });
}

function getOptionInfo(callback) {
    $.get('Service.ashx', { op: 'GetOptionInfo', showdisabled: false, getchildren: true }, function (data) {
        callback(data);
    });

}

function getOption(number, callback) {
    $.post('Service.ashx', { op: 'GetOption', number: number }, function (data) {
        callback(data);

    });

}

function getLatestThroughput(callback) {
    $.get('Service.ashx', { op: 'getLatestThroughput' }, function (data) {
        if (callback)
            callback(data);

        return data;
    });

}

function getReasonId() {
    var reasonid;
    if ($('[name="level3"]input:checked').length == 0) {
        if ($('[name="level2"]input:checked').length > 0) {
            reasonid = $('[name="level2"]input:checked').attr("reasonId");
        } else {
            reasonid = $('[name="level1"]input:checked').attr("reasonId");
        }
    } else {
        reasonid = $('[name="level3"]input:checked').attr("reasonId");
    }
    return reasonid;
}

function getReasonPath() {
    var l1, l2, l3
    l1 = $('[name="level1"]input:checked').val();
    l2 = $('[name="level2"]input:checked').val();
    l3 = $('[name="level3"]input:checked').val();

    if ($('[name="level3"]input:checked').length == 0) {
        if ($('[name="level2"]input:checked').length > 0) {
            return l1 + " > " + l2;
        } else {
            return l1;
        }
    } else {
        return l1 + " > " + l2 + " > " + l3;
    }
}

function TurnLights(val, callback) {
    var on = false;

    if (val.toLowerCase() == 'on')
        on = true;

    $.post('Service.ashx', { op: 'turnlights', line: Line, on: on }, function () {
        if (callback) {
            callback();
        }

    });

}

function changeLightsOnLabel(status) {
    if (status.toLowerCase() == 'true') {
        $('#drpLights').val('On');
    } else {
        $('#drpLights').val('Off');
    }
}

function LoadGridData() {
    var line = $.getUrlVar('line');

    if (line == undefined || line == '' || line == null)
        line = 'company-demo';

    //var scroll = $('#DataGridPanel').scrollTop();

    $("#tipBox").css("display", "none");
    //$('#DataGrid').attr("disabled", "disabled");
    $('#DataGridPanel').append('<div id="_loading" style="width:50px;height:50px;position:absolute;margin:200px 400px;z-index:999;">Loading...</div>');
    $.post("Service.ashx?op=loadgriddata", { startDate: $('#f_startdate').val(), endDate: $('#f_enddate').val(), line: line }, function (data) {
        $('#_loading').remove();
        var checked = getMultipleCheckedIds();
        var tb = $('#DataGrid').clone(true);
        //tb.attr("disabled", "");
        var row, rownum, bgcolor, r;
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

            if (filteredId == this.ID)
                row = "<tr>";
            else if (filteredId > 0)
                row = "<tr style='display: none'>";
            else
                row = "<tr>";


            //if(this.Minutes == 1.14)
            //    console.log(secondToString(this.Minutes * 60));

            row += "<td class=\"col1 tooltip\" title='Unique ID: " + this.ID + "' data-id='" + this.ID + "'>" + this.EventStartDisplay + "</td>"; //(new Date(this.EventStart)).format("MM/dd/yyyy h:mm t")
            row += "<td class=\"col2\">" + secondToString(this.Minutes * 60) + "</td>";
            if ($.trim(this.ReasonCode).length > 0) {
                row += "<td class=\"col3\"><input type=\"checkbox\" name=\"record_chk\" " + ischecked + " class=\"reasoncheckbox\" value=\"" + this.ID + "\" /> <a href=\"javascript:AssignReasonCode(" + this.ID + ")\">" + this.ReasonCode + "</a></td>";
            }
            else {
                $("#tipBox").css("display", "block");
                row += "<td class=\"col3\"><input type=\"checkbox\" name=\"record_chk\" " + ischecked + " class=\"reasoncheckbox\" value=\"" + this.ID + "\" /> <input type='button' id='assign_reason_code' value='' onclick='AssignReasonCode(" + this.ID + ")' /></td>";
            }
            row += "</tr>";
            tb.append(row);
            rownum++;
        });

        $('#DataGrid').replaceWith(tb);

        $('.reasoncheckbox').unbind('click');
        $('.reasoncheckbox').click(function () {
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
        var pane = $('#DataGridPanel').jScrollPane(
						{
						    showArrows: false,
						    scrollbarWidth: 35,
						    reinitialiseOnImageLoad: true,
						    mouseWheelSpeed: 50
						}
					);

        var paneApi = pane.data('jsp');

        if (!isReload) {
            if ($('#DataGridPanel')[0].scrollTo != null) {
                $('#DataGridPanel')[0].scrollTo($('#DataGridPanel').height());
            }
            else if (paneApi.scrollTo != null) {
                paneApi.scrollToY($('#DataGrid').height());
            }
        }

        setGridHeader();
        isReload = true;

        $('.tooltip').tooltip({
            track: true
        });

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
        if (check)
            $(this).attr('checked', 'checked');
        else
            $(this).removeAttr('checked');
    });
}

function delRows() {
    checkPassword(function (passed) {

        if (passed == true) {
            var ids = "";
            $('input[type=checkbox][name=record_chk]:checked').each(function () {
                ids += (ids == '' ? '' : ',') + $(this).val();
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
        } else {
            alert('Incorrect password');
        }

    });
}

function mrgRows() {

    var ids = "";
    $('input[type=checkbox][name=record_chk]:checked').each(function () {
        ids += (ids == '' ? '' : ',') + $(this).val();
    });

    if ($('input[type=checkbox][name=record_chk]:checked').length < 2) {
        alert("You must select at least two records to merge");
        return;
    }

    if (!confirm("Are you sure you want to merge the selected records?")) {
        return;
    }

    $('#f_mrg_btn').attr('disabled', 'disabled');
    $.post('Service.ashx?op=mergeRecords', { ids: ids }, function (data) {
        /*
        if (data == 'true' || data === 'true' || data === true) {
        LoadGridData();
        } else {
        alert(data);
        }*/

        alert("Sum is " + data);
        LoadGridData();
        $('#f_mrg_btn').removeAttr('disabled');
    });

}

function splitrows() {
    var rows = $('input[type=checkbox][name=record_chk]:checked');
    var checkedCount = rows.length;

    if (checkedCount > 1) {
        alert("You must select at most one record to split");
        return;
    }

    if (checkedCount == 0) {
        alert("You must select one record to split");
        return;
    }

    $.fn.jmodal({
        initWidth: lightWindowWidth,
        title: 'Select Split Times',
        content: LightBox_SplitDowntimesPopWindow,
        buttonText: 'Cancel',
        okEvent: function (e) { }
    });


    var selectedRow = rows.first().parent().parent();
    var time = selectedRow.find('.col2').text();
    var downtimeId = selectedRow.find('.col1').data('id');

    var timeSeconds = getSecondsFromTime(time);

    var timeOriginal = $('#splitdt-time-original');
    timeOriginal.val(time);

    var timeNew = $('#splitdt-time-new');

    timeOriginal.change(function () { updateTime(timeSeconds, timeOriginal, timeNew); });
    $('#splitdt-save-button').click(function () {
        $('#splitdt-save-button').off();

        $.post('Service.ashx?op=splitdowntime&downtimeId=' + downtimeId + "&minutes=" + getSecondsFromTime($("#splitdt-time-new").val()) / 60, {}, function () {
            LoadGridData();
            closeLightBox();
        });


    });
}

function getSecondsFromTime(time) {
    var timeParts = time.split(':');

    var hours = parseInt(timeParts[0]);
    var minutes = parseInt(timeParts[1]);
    var seconds = parseInt(timeParts[2]);

    return seconds + (minutes * 60) + (hours * 3600);
}

function updateTime(totalSeconds, oldTimeElement, newTimeElement) {
    var oldSeconds = getSecondsFromTime($(oldTimeElement).val());
    var newSeconds = totalSeconds - oldSeconds;

    if (totalSeconds < oldSeconds) {
        alert("Sorry, you can't increase the downtime while splitting it");
        return;
    }

    var newTimeHours = Math.floor(newSeconds / 3600);
    var newTimeMinutes = Math.floor((newSeconds - newTimeHours * 3600) / 60);
    var newTimeSeconds = newSeconds - newTimeHours * 3600 - newTimeMinutes * 60;

    if (newTimeHours < 10) newTimeHours = "0" + newTimeHours;
    if (newTimeMinutes < 10) newTimeMinutes = "0" + newTimeMinutes;
    if (newTimeSeconds < 10) newTimeSeconds = "0" + newTimeSeconds;

    newTimeElement.val(newTimeHours + ":" + newTimeMinutes + ":" + newTimeSeconds);
}

function setDefaultStyles() {
    $(':input[type=text]').each(function () {
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
    if (Line == null || Line == '' || Line == undefined)
        Line = 'company-demo';

    wasPlannedWarned = false;

    $.post("Service.ashx?op=level1", "line=" + Line, function (data) {
        var rg = $("#step1");
        rg.empty();
        var i = 0;
        var orgarr = $('#orgValue').val().split(' > ');
        $.each(data, function () {
            rg.append('<label><input type="radio" name="level1" ischangeover="' + this.IsChangeOver + '" reasonId="' + this.ID + '" value="' + this.Level1 + '" id="level1_' + i + '" ' + (InArray(orgarr, this.Level1, 0) ? 'checked="checked"' : '') + ' onclick="ShowAssignReasonCodeStep(' + this.IsChangeOver + ', 2, ' + ((this.IsPlanned && this.Duration > 0) ? true : false) + ')" data-duration="' + this.Duration + '" />' + this.Level1 + '</label>');
            i++;
        });

        if (rg.find("input").length == 0) {
            alert('level1 is empty.');
        }
    }, "json");
}

function loadLevel2(level1) {
    $.post("Service.ashx?op=level2", "level1=" + level1 + "&line=" + Line, function (data) {
        var rg = $("#step2");
        rg.empty();
        var i = 0;
        var orgarr = $('#orgValue').val().split(' > ');

        $.each(data, function () {
            rg.append('<label><input type="radio" name="level2" ischangeover="' + this.IsChangeOver + '" reasonId="' + this.ID + '" value="' + this.Level2 + '" id="level2_' + i + '" ' + (InArray(orgarr, this.Level2, 1) ? 'checked="checked"' : '') + '  onclick="ShowAssignReasonCodeStep(' + this.IsChangeOver + ', 3, ' + ((this.IsPlanned && this.Duration > 0) ? true : false) + ');" data-duration="' + this.Duration + '" />' + this.Level2 + '</label>');
            i++;
        });

        if (rg.find("input").length == 0) {
            alert('level1 is empty.');
        }

        rg.append('<input type="hidden" value="' + $('[name="level1"]input:checked').val() + '" id="preLevel1" />');
    }, "json");
}

function loadLevel3(level1, level2) {
    $.post("Service.ashx?op=level3", "level1=" + level1 + "&level2=" + level2 + "&line=" + Line, function (data) {
        var rg = $("#step3");
        rg.empty();
        var i = 0;
        var orgarr = $('#orgValue').val().split(' > ');
        $.each(data, function () {
            rg.append('<label><input type="radio" name="level3" ischangeover="' + this.IsChangeOver + '" reasonId="' + this.ID + '" value="' + this.Level3 + '" id="level3_' + i + '" ' + (InArray(orgarr, this.Level3, 2) ? 'checked="checked"' : '') + '  onclick="ShowAssignReasonCodeStep(' + this.IsChangeOver + ', 4, ' + ((this.IsPlanned && this.Duration > 0) ? true : false) + ')" data-duration="' + this.Duration + '" />' + this.Level3 + '</label>');
            i++;
        });

        if (rg.find("input").length == 0) {
            alert('level1 is empty.');
        }

        rg.append('<input type="hidden" value="' + $('[name="level2"]input:checked').val() + '" id="preLevel2" />');
    }, "json");
}

function AssignReasonCode(id) {
    $.ajax({
        async: false,
        cache: false,
        type: "POST",
        dataType: "json",
        url: "Service.ashx",
        data: { op: "getevent", id: id, line: Line },
        error: function (xml) { alert('get event data failed.'); },
        timeout: 30,
        success: function (result) {
            if (result.length == 0) {
                alert('not found event data');
                return;
            }

            EventTimer.stop();

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

            var postIds = getMultipleCheckedIds();

            if (postIds) {
                if (postIds.split(',').length > 1) {
                    $('#minutes').attr('disabled', 'disabled');
                }
                else {
                    $('#minutes').removeAttr('disabled');
                }
            }
            else {
                $('#minutes').removeAttr('disabled');
            }

            $('#for_Occurences').css('display', 'none');
            $('#Occurences').css('display', 'none').val(1);
            $('#id').val(id); // set id value
            $('#orgValue').val(orgvalue); //set original step value
            if ($.trim(comment).length > 0) {
                $('#comment').val(comment); //set original comment value
                $('#comment').css("color", "#000000");
            }

            $('#startTimer').hide();
            $('#lblTimer').text('');

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
                    ShowAssignReasonCodeStep(false, 1);
                }
            });


            //$('#slThroughput').val(result.Throughput);
            //$('#slOption1').val(result.Option1);
            //$('#slOption2').val(result.Option2);

            var tp = result.Throughput;
            var op1 = result.Option1;
            var op2 = result.Option2;

            if (tp == undefined)
                tp = '';

            if (op1 == undefined)
                op1 = '';

            if (op2 == undefined)
                op2 = '';

            populateDropboxes(tp, op1, op2);

            $('#btnSave').unbind('click');
            $('#btnSave').click(function () {
                if (!checkStep0()) {
                    ShowAssignReasonCodeStep(false, 1);
                }

                var reason, comment, reasonid, startdatetime, minutes, line, throughput, option1, option2;
                comment = $('#comment').val();
                if (comment == commentDefaultString) {
                    comment = '';
                }

                reasonid = getReasonId();
                reason = getReasonPath();
                startdatetime = $('#start_date_time').val();
                //Set to not change minutes
                minutes = $('#minutes').val();

                throughput = $('#slThroughput').val();
                option1 = $('#slOption1').val();
                option2 = $('#slOption2').val();

                if (throughput == undefined)
                    throughput = -1;

                if (option1 == undefined)
                    option1 = -1;

                if (option2 == undefined)
                    option2 = -1;

                var save = true;

                for (var x = 0; x < OptionInfo.length; x++) {
                    var opt = OptionInfo[x];

                    if (opt.Number == 1 && opt.IsRequired && (option1 <= 0 || !option1)) {
                        alert('Need to select ' + opt.Name);
                        save = false;
                        break;
                    }
                    else if (opt.Number == 2 && opt.IsRequired && (option2 <= 0 || !option2)) {
                        alert('Need to select ' + opt.Name);
                        save = false;
                        break;
                    }
                }

                if (save) {


                    var line = $.getUrlVar('line');

                    if (line == undefined || line == null || line == '')
                        line = 'company-demo';

                    postIds = getMultipleCheckedIds();
                    if (postIds.length == 0) postIds = $('#id').val();

                    $.post("Service.ashx?op=save", "id=" + postIds + "&editId=" + $('#id').val() + "&reasonId=" + reasonid + "&reasonCode=" + UrlEncode(reason) + "&comment=" + UrlEncode(comment) + "&startdatetime=" + UrlEncode(startdatetime) + "&enddatetime=" + moment().format('M/D/YYYY H:mm:ss') + "&minutes=" + UrlEncode(minutes) + "&line=" + UrlEncode(line) + "&throughput=" + UrlEncode(throughput) + "&option1=" + UrlEncode(option1) + "&option2=" + UrlEncode(option2), function (data) {
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
                }
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
            ShowAssignReasonCodeStep(false, 1);

        }
    });


}

function populateOption1Dropbox(value, callback) {

    $('#slOption1').css('display', '');

    $('#lblOption1').css('display', '');

    getOption(1, function (data) {
        var oldVal = $('#slOption1').val();

        $('#slOption1').empty();
        $('#slOption1').append('<option value="0"> </option>');

        var counter = 0;

        $(data).each(function () {
            $('#slOption1').append('<option value="' + this.Id + '">' + this.Description + '</option>');

            if (counter == data.length - 1) {

                if (value)
                    $('#slOption1').val(value);

                if (callback)
                    callback();
            }
            else
                counter++;

        });

    });

}

function populateOption2Dropbox(value, callback) {

    $('#slOption2').css('display', '');

    $('#lblOption2').css('display', '');

    getOption(2, function (data) {
        var oldVal = $('#slOption2').val();

        $('#slOption2').empty();

        $('#slOption2').append('<option value="0"> </option>');

        var counter = 0;

        $(data).each(function () {
            $('#slOption2').append('<option value="' + this.Id + '">' + this.Description + '</option>');

            if (counter == data.length - 1) {

                if (value)
                    $('#slOption2').val(value);

                if (callback)
                    callback();
            }
            else
                counter++;
        });

    });

}

function populateThroughputDropbox(value, callback) {

    $('#slThroughput').css('display', '');

    $('#lblThroughput').css('display', '');

    getThroughPuts(function (data) {
        $('#slThroughput').empty();

        var counter = 0;

        $(data).each(function () {
            $('#slThroughput').append('<option value="' + this.Id + '">' + this.Name + ' | ' + this.Description + '</option>');

            if (counter == data.length - 1) {
                if (value)
                    $('#slThroughput').val(value);

                if (callback)
                    callback();
            }
            else
                counter++;
        });

    });

}

function populateDropboxes(tpVal, op1Val, op2Val, callback) {

    //populateOption1Dropbox(op2Val, populateOption2Dropbox(op1Val, populateThroughputDropbox(tpVal, callback)));

    //Hide by default
    $('#slOption1').hide();
    $('#slOption2').hide();
    $('#lblOption1').hide();
    $('#lblOption2').hide();

    getOptionInfo(function (optInfo) {
        if (optInfo) {
            OptionInfo = optInfo;

            $(optInfo).each(function () {

                var number = this.Number;
                var name = this.Name;

                var slId = '#slOption' + number;
                var lblId = '#lblOption' + number;

                var sl = $(slId);
                var lbl = $(lblId);

                lbl.text(name + ':');

                var value = op1Val;

                if (number == 2)
                    value = op2Val;

                if (this.Enabled) {
                    sl.css('display', '');

                    lbl.css('display', '');

                    var counter = 0;

                    var options = this.Options;
                    sl.empty();
                    sl.append('<option value="0"> </option>');

                    $(options).each(function () {

                        sl.append('<option value="' + this.Id + '">' + this.Name + '</option>');


                        if (counter == options.length - 1) {
                            if (value)
                                sl.val(value);
                        }
                        else
                            counter++;

                    });

                }
                else {
                    sl.css('display', 'none');

                    lbl.css('display', 'none');
                }

            });
        }

        populateThroughputDropbox(tpVal, function () {
            if (callback) {
                callback();
            }
        })
    });

}

function showAssignReasonCodeStepTitle(num, dataSourceLength) {
    var l1, l2, l3;
    l1 = $('[name=level1]input:checked');
    l2 = $('[name=level2]input:checked');
    l3 = $('[name=level3]input:checked');


    switch (num) {
        case 0:
            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > Date Time');
            break;
        case 1:
            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 0)">Date Time</a> > 1st Level');
            break;
        case 2:
            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 1)">' + l1.val() + '</a> > 2nd Level');

            break;
        case 3:
            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 1)">' + l1.val() + '</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 2)">' + l2.val() + '</a> > 3rd Level');
            break;
        case 4:
            if (l2.length == 0) {
                setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 1)">' + l1.val() + '</a> > Comment');
                break;
            }
            if (l3.length == 0) {
                setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 1)">' + l1.val() + '</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 2)">' + l2.val() + '</a> > Comment');
                break;
            }

            setLightBoxTitle('<a href="#" onclick="closeLightBox()">Event List</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 0)">Date Time</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 1)">' + l1.val() + '</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 2)">' + l2.val() + '</a> > <a href="javascript:ShowAssignReasonCodeStep(false, 3)">' + l3.val() + '</a> > Comment');

            break;
    }
}

function ShowAssignReasonCodeStep(ischangeover, num, isPlanned) {
    isChangeOver = ischangeover;

    if (!isPlanned && num == 1) {
        wasPlannedWarned = false;
    }
    else if (isPlanned && !wasPlannedWarned) {

        var con = confirm('Selecting this will split the Event into separate events');

        if (!con) {
            wasPlannedWarned = false;
            return;
        }

        wasPlannedWarned = true;
    }

    var dataSourceLength = 0;
    if (num == 2 || num == 3) {
        $.ajax({
            async: false,
            cache: false,
            type: "POST",
            dataType: "html",
            url: "Service.ashx",
            data: { op: "getLevelDataSourceLength", level1: $('[name="level1"]input:checked').val(), level2: (num == 3 ? $('[name="level2"]input:checked').val() : ''), department: $('#lineDepartment').val(), line: Line },
            error: function (xml) { alert('get level data source length failed.'); dataSourceLength = 0; },
            timeout: 30,
            success: function (result) { dataSourceLength = parseInt(result); }
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
                l1 = $('[name="level1"]input:checked');
                if (l1.length > 0) {
                    l2 = $('[name="level2"]input:checked');
                    if (l2.length == 0 || $('#preLevel1').val() != l1.val()) {
                        loadLevel2(l1.val());
                    }
                }
                else {
                    alert('your must select 1st Level.');
                    ShowAssignReasonCodeStep(false, 1);
                    return;
                }
            } else {
                $('#step2').empty();
                $('#step3').empty();
                ShowAssignReasonCodeStep(isChangeOver, 4);
                return;
            }
            break;
        case 3:
            if (dataSourceLength > 0) {
                var l1, l2, l3;

                l1 = $('[name=level1]input:checked');
                l2 = $('[name=level2]input:checked');

                if (l1.length == 0) {
                    alert('your must select 1st Level.');
                    ShowAssignReasonCodeStep(false, 1);
                    return;
                }

                if (l2.length == 0) {
                    alert('your must select 2nd Level.');
                    ShowAssignReasonCodeStep(false, 2);
                    return;
                }

                l3 = $('[name="level3"]input:checked');
                if (l3.length == 0 || $('#preLevel2').val() != l2.val()) {
                    loadLevel3(l1.val(), l2.val());
                }
            } else {
                $('#step3').empty();
                ShowAssignReasonCodeStep(isChangeOver, 4);
                return;
            }
            break;
        case 4:
            var l1, l2, l3;
            l1 = $('[name=level1]input:checked');
            l2 = $('[name=level2]input:checked');
            l3 = $('[name=level3]input:checked');

            if (l1.length == 0) {
                alert('your must select 1st Level.');
                ShowAssignReasonCodeStep(false, 1);
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

    hideOptions();


    for (var i = 0; i <= 4; i++) {
        if (num != i) {
            $("#step" + i).css("display", "none");
        }
        else {
            $("#step" + i).css("display", "block");
        }
    }
}

function hideOptions() {

    if (isChangeOver == true) {
        $('#lblThroughput').css('display', '');
        $('#slThroughput').css('display', '');



    }
    else {
        $('#lblThroughput').css('display', 'none');
        $('#slThroughput').css('display', 'none');

    }

    if (OptionInfo && OptionInfo.length > 0) {
        //$('#lblOption2').css('display', 'none');

        var enabledOptions = {
            1: false,
            2: false
        };

        for (var index in OptionInfo) {
            var optInfo = OptionInfo[index];

            if (optInfo.Enabled) {
                var num = parseInt(optInfo.Number);
                enabledOptions[num] = true;
            }
        }

        for (var optNumber in enabledOptions) {
            var enabled = enabledOptions[optNumber];

            if (enabled) {
                $('#slOption' + optNumber).css('display', '');
                $('#lblOption' + optNumber).css('display', '');
            } else {
                $('#slOption' + optNumber).css('display', 'none');
                $('#lblOption' + optNumber).css('display', 'none');
            }
        }


    } else {
        $('#lblOption1').css('display', 'none');
        $('#lblOption2').css('display', 'none');

        $('#slOption1').css('display', 'none');
        $('#slOption2').css('display', 'none');

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
        okEvent: function (e) { }
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
    $.post("Service.ashx?op=stop", "start=" + st + "&stop=" + et, function (date) {
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

Date.prototype.format = function (format) {
    if (Object.prototype.toString.call(this) === "[object Date]") {
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

Number.prototype.toFixed = function (d) {
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


    if (isNaN($('#minutes').val()) || $.trim($('#minutes').val()).length == 0) {
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
