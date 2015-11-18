var allowDrop = false;
var delids = ""; //被删除的id列表
var _ORG_TABLE_HTML = null;
var Line = '';

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
    var body = $('body');

    body.hide();

    getUser(function (user) {
        if (user) {
            console.log(user);
            checkPassword(function (passed) {
                if (passed == true) {
                    body.show();

                    init();

                    if (!user.hideHelper) {
                        $('#toprightnav').append('<a href="#" onclick="HelpPopup()" id="openHelpPopup" style="width:100%;margin-top:20px;">Help</a>');
                        $('#openHelpPopup').button();
                        $('#openHelpPopup span').css('font-size', '13px')
                                .css('font-weight', 'bold')
                                .css('color', 'red');

                        HelpPopup();
                    }

                    if (user.hidePanel) {
                        $('#toprightnav').hide();
                    }


                } else {
                    alert('Incorrect password');
                }

            });
        }
    });
});

function HelpPopup() {
    $("#dialog:ui-dialog").dialog("destroy");
    $("#dialog-confirm").dialog({
        resizable: false,
        height: "auto",
        width: 400,
        modal: true,
        buttons: {
            "More": function () {
                window.open("http://legacy.downtimecollectionsolutions.com/index.php/tour/reason-code-management/");
                $(this).dialog("close");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
}

function init() {
    $('#tab-container').tabs();

    Line = $.getUrlVar('line');

    if (Line == undefined)
        Line = '';

    document.title += ' | ' + Line;

    loadTreeTable();
    getOptionInfo();

    $('#btnSave').click(function () {
        if (haveChanges()) {
            saveData();
        } else {
            alert('No changes,not need update.');
        }
    });

    $('#btnReload').click(function () {
        if (haveChanges()) {
            if (confirm('There are some changes, continue without saving?')) {
                loadTreeTable();
            }
        } else {
            loadTreeTable();
        }
    });

    window.onbeforeunload = function () {
        if (haveChanges()) {
            return 'There are some changes, continue without saving?';
        }

        return;
    };

    $('#lkChangeOver').click(function () {
        getChangeOverData();
    });

    $('#btnTPAdd').click(function () {
        if ($(this).text() == 'Add')
            createThroughPut();
    });

    $("#btnOPAdd").click(function () {
        if ($(this).text() == 'Add')
            createOption();
    });

    $('#btnShowGlobal').click(function () {
        window.location = 'DCSDemoReasonCodes.aspx';
    });

    $('#btnSaveOptionInfo').click(function () {
        var op1Name = $('#opt1Name').val();
        var op1Required = $('#chkOpt1Required').is(':checked');
        var op1Enabled = $('#chkOpt1Enabled').is(':checked');

        var op2Name = $('#opt2Name').val();
        var op2Required = $('#chkOpt2Required').is(':checked');
        var op2Enabled = $('#chkOpt2Enabled').is(':checked');

        $.post('Service.ashx', { op: 'UpdateOptionInfo', opt1Name: op1Name, opt2Name: op2Name, opt1Required: op1Required, opt2Required: op2Required, opt1Enabled: op1Enabled, opt2Enabled: op2Enabled }, function (data) {
            if (data.error) {
                alert(data.error);
            }
        });

    });

    setInterval('getChangeOverData()', 1000 * 2);

}

function getOptionInfo(callback) {
    $.get('Service.ashx', { op: 'GetOptionInfo', getchildren: false, showdisabled: true }, function (data) {

        $(data).each(function () {
            var number = this.Number;

            if (number == 1) {
                $('#opt1Name').val(this.Name);
                $('#chkOpt1Required').attr('checked', (this.IsRequired ? 'checked' : ''));
                $('#chkOpt1Enabled').attr('checked', (this.Enabled ? 'checked' : ''));
            }
            else if (number == 2) {
                $('#opt2Name').val(this.Name);
                $('#chkOpt2Required').attr('checked', (this.IsRequired ? 'checked' : ''));
                $('#chkOpt2Enabled').attr('checked', (this.Enabled ? 'checked' : ''));
            }

        });

        if(callback)
            callback(data);
    });

}


function createThroughPut() {
    var name = $('#txtTPName').val();
    var desc = $('#txtTPDesc').val();
    var perHour = $('#txtTPPerHour').val();

    $.post('Service.ashx', { op: 'CreateThroughPut', name: name, description: desc, perhour: perHour }, function (data) {
        getThroughPuts();
    });

    getThroughPuts();
}
function createOption() {
    var name = $('#txtOPName').val();
    var desc = $('#txtOPDesc').val();
    var num = $('#slOPNum').val();

    $.post('Service.ashx', { op: 'CreateOption', name: name, description: desc, number: num }, function (data) {
                getOptions();
    });
    getOptions();
}



function updateThroughPut(id) {
    $('#txtTPName').val($('#t' + id).find('.name').text());
    $('#txtTPDesc').val($('#t' + id).find('.description').text());
    $('#txtTPPerHour').val($('#t' + id).find('.perhour').text());

    $('#btnTPAdd').text('Update');

    var updateTP = function () {
        var ans = confirm('Update ThroughPut?');

        if (ans) {
            var name = $('#txtTPName').val();
            var desc = $('#txtTPDesc').val();
            var perHour = $('#txtTPPerHour').val();

            $.post('Service.ashx', { op: 'UpdateThroughPut', id: id, name: name, description: desc, perhour: perHour }, function (data) {
                getThroughPuts();
            });
        }
        getThroughPuts();
        $('#btnTPAdd').text('Add').unbind('click', updateTP);
        $('#txtTPName').val('');
        $('#txtTPDesc').val('');
        $('#txtTPPerHour').val('');

    };


    $('#btnTPAdd').unbind('click.updateTP');
    
    $('#btnTPAdd').bind('click.updateTP', updateTP);

}

function updateOption(id) {
    $('#txtOPName').val($('#o' + id).find('.name').text());
    $('#txtOPDesc').val($('#o' + id).find('.description').text());
    $('#slOPNum').val($('#o' + id).find('.number').text());

    var updateOP = function () {
        var ans = confirm('Update Option?');

        if (ans) {
            var name = $('#txtOPName').val();
            var desc = $('#txtOPDesc').val();
            var number = $('#slOPNum').val();

            $.post('Service.ashx', { op: 'UpdateOption', id: id, name: name, description: desc, number: number }, function (data) {
                getOptions();
            });
        }
        getOptions();

        $('#btnOPAdd').text('Add').unbind('click', updateOP);

        $('#txtOPName').val('');
        $('#txtOPDesc').val('');
        $('#slOPNum').val(0);
    };


    $('#btnOPAdd').text('Add').unbind('click.updateOP'); //In case of previous bound
    $('#btnOPAdd').text('Update').bind('click.updateOP', updateOP);
}
function deleteThroughPut(id) {
    var ans = confirm('Are you sure you want to delete?');

    if (ans) {
        $.post('Service.ashx', { op: 'DeleteThroughPut', id: id }, function (data) {
            getThroughPuts();
        });
        getThroughPuts();
    }
}
function deleteOption(id) {
    var ans = confirm('Are you sure you want to delete?');

    if (ans) {
        $.post('Service.ashx', { op: 'DeleteOption', id: id }, function (data) {
            getOptions();
        });
        getOptions();
    }
}

function getChangeOverData() {
    getThroughPuts();
    getOptions();
    //getOption2();

}

function getThroughPuts() {
    $.post('Service.ashx', { op: 'GetThroughPuts' }, function (data) {
        $('#tblThroughputs tbody').empty();

        $(data).each(function () {
            var name = this.Name;
            var desc = this.Description;
            var perHour = this.PerHour;
            var id = this.Id;

            var row = '<tr id="t' + id + '">';
            row += '<td class="name">' + name + '</td>';
            row += '<td class="description">' + desc + '</td>';
            row += '<td class="perhour">' + perHour + '</td>';
            row += '<td><a href="#" onclick="updateThroughPut(' + id + ')">Edit</a></td>';
            row += '<td><a href="#" onclick="deleteThroughPut(' + id + ')">Delete</a></td>';
            row += '</tr>';

            $('#tblThroughputs').append(row);

            $('#' + id)

        });

    }, 'json');
}

function getOptions() {
    $.post('Service.ashx', { op: 'GetOptions' }, function (data) {
        $('#tblOption tbody').empty();

        $(data).each(function () {
            var name = this.Name;
            var desc = this.Description;
            var id = this.Id;
            var number = this.Number;

            var row = '<tr id="o' + id + '">';
            row += '<td class="name">' + name + '</td>';
            row += '<td class="description">' + desc + '</td>';
            row += '<td class="number">' + number + '</td>';
            row += '<td><a href="#" onclick="updateOption(' + id + ')">Edit</a></td>';
            row += '<td><a href="#" onclick="deleteOption(' + id + ')">Delete</a></td>';
            row += '</tr>';

            $('#tblOption').append(row);

        });



    });

}

function getOption2() {
    $.post('Service.ashx', { op: 'GetOption2' }, function (data) {
        $('#tblOption2 tbody').empty();

        $(data).each(function () {
            var name = this.Name;
            var desc = this.Description;

            var row = '<tr>';
            row += '<td>' + name + '</td>';
            row += '<td>' + desc + '</td>';
            row += '</tr>';

            $('#tblOption2').append(row);

        });


    });

}

function haveChanges() {
    if ($(_ORG_TABLE_HTML).find('tbody tr').length != $('#dnd-example').find('tbody tr').length) {
        return true;
    }

    var result = false;
    $(_ORG_TABLE_HTML).find('tbody tr').each(function (i, item) {
        if ($.trim($(item).find('td:last').text()) != $.trim($('#dnd-example').find('tbody tr:eq(' + i + ')').find('td:last').text())) {
            result = true;
            return;
        }

        if ($(item).find('input[type="checkbox"]').attr('checked') != $('#dnd-example').find('tbody tr:eq(' + i + ')').find('input[type="checkbox"]').attr('checked')) {
            result = true;
            return;
        }

        //if ($(item).find('.duration').text() != $('#dnd-example').find('tbody tr:eq(' + i + ')').find('.duration')) {
        //    result = true;
        //    return;
        //}

        if ($(item).find('.changeover').attr('checked') != $('#dnd-example').find('tbody tr:eq(' + i + ')').find('.changeover').attr('checked')) {
            result = true;
            return;
        }

        if ($(item).find('.planned').attr('data-duration') != $('#dnd-example').find('tbody tr:eq(' + i + ')').find('.planned').attr('data-duration')) {
            result = true;
            return;
        }
    });

    return result;
}

function loadTreeTable() {
    markTip("Loading...");
    $('#btnSave').attr('disabled', 'disabled');
    $('#btnReload').attr('disabled', 'disabled');

    if ($('#dnd-example').length > 0) $('#dnd-example').remove();

    var table = $('<table id="dnd-example" class="example treeTable"></table>');
    table.append('<thead><tr><th>Reason Code</th><th>Planned</th><th class="col2"></th><th>Is ChangeOver</th><th>Full Path</th></tr></thead>');
    table.append('<tr id="node-0" system_id="-1" isRoot="true">' +
                  '<td><span class="expander" style="margin-left: -19px; padding-left: 19px;"></span><span class="folder ui-draggable">Reason Codes</span></td>' +
                  '<td>&nbsp;</td>' +
                  '<td><span class="addClhildren"  onclick="addChildren(this,1);">Add children</span></td>' +
                  '<td>&nbsp;</td>' +
                  '<td>Reason Codes</td>' +
                  '</tr>');

    $.post("Service.ashx", "op=gettreetable&line=" + Line, function (data) {
        //debugger;
        var level1Array = new Array();
        var globel_id = 0;
        var original = jQuery.extend(true, [], data);

        var getLevel1Duration = function (level1) {
            for (var x = 0; x < original.length; x++) {
                if (original[x].Level1 == level1 && original[x].HideReasonInReports) {
                    return original[x].Duration;
                }
            }

            return 0;
        }

        var getLevel2Duration = function (level1, level2) {
            for (var x = 0; x < original.length; x++) {
                if (original[x].Level1 == level1 && original[x].Level2 == level2 && original[x].HideReasonInReports) {
                    return original[x].Duration;
                }
            }

            return 0;
        }

        var getLevel3Duration = function (level1, level2, level3) {
            for (var x = 0; x < original.length; x++) {
                if (original[x].Level1 == level1 && original[x].Level2 == level2 && original[x].Level3 == level3 && original[x].HideReasonInReports) {
                    return original[x].Duration;
                }
            }

            return 0;
        }

        //first
        $(
        data.GroupBy(function (o, i) {
            return o.Level1
        })).each(function (i, item) {
            globel_id++;

            item.Item.Duration = getLevel1Duration(item.Item.Level1);
            //debugger;
            table.append('<tr id="node-' + globel_id + '" system_id="' + item.Item.ID + '" class="child-of-node-0">' +
                              '<td><span class="expander" style="margin-left: -19px; padding-left: 19px;"></span><span class="folder ui-draggable">' + item.Item.Level1 + '</span></td>' +
                              '<td><input class="planned" type="checkbox" ' + (item.Item.HideReasonInReports ? ' checked="checked"' : '') + ' value="true" onclick="setReasonToPlanned(' + item.Item.ID + ', this)" data-Duration="' + item.Item.Duration + '" ' + (item.Item.Duration != 0 ? ' title="' + item.Item.Duration + ' minute(s)"' : '') + ' /></td>' +
                              '<td class="col2"><span class="addClhildren"  onclick="addChildren(this,2);">Add children</span></td>' +
                              '<td><input type="checkbox" ' + (item.Item.IsChangeOver ? ' checked="checked" ' : ' ')/* + (item.Item.Duration != 0 ? ' title="' + item.Item.Duration + ' minute(s)"' : '')*/ + ' value="true" class="changeover" /></td>' +
                              '<td>' + item.Item.Level1 + '</td>' +
                              '</tr>');
            var level1 = item.Item.Level1;
            var level1_id = globel_id;

            //second
            $(
                data.GroupBy(function (o, i) {
                    if (o.Level1 == level1) {
                        return o.Level2
                    }
                })).each(function (i2, item2) {
                    if ($.trim(item2.Item.Level2).length > 0) {
                        //debugger;
                        globel_id++;
                        item2.Item.Duration = getLevel2Duration(item2.Item.Level1, item2.Item.Level2);
                        table.append('<tr id="node-' + globel_id + '" system_id="' + item2.Item.ID + '" class="child-of-node-' + level1_id + '">' +
                                      '<td><span class="expander" style="margin-left: -19px; padding-left: 19px;"></span><span class="folder ui-draggable">' + item2.Item.Level2 + '</span></td>' +
                                      '<td><input class="planned" type="checkbox" ' + (item2.Item.HideReasonInReports ? ' checked="checked"' : '') + ' value="true" onclick="setReasonToPlanned(' + item2.Item.ID + ', this)" data-Duration="' + item2.Item.Duration + '" ' + (item2.Item.Duration != 0 ? ' title="' + item2.Item.Duration + ' minute(s)"' : '') + ' /></td>' +
                                      '<td class="col2"><span class="addClhildren"  onclick="addChildren(this,3);">Add children</span></td>' +
                                      '<td><input type="checkbox" ' + (item2.Item.IsChangeOver ? ' checked="checked" ' : ' ')/* + (item2.Item.Duration != 0 ? ' title="' + item2.Item.Duration + ' minute(s)"' : '')*/ + ' value="true" class="changeover" /></td>' +
                                      '<td>' + item2.Item.Level1 + '>' + item2.Item.Level2 + '</td>' +
                                      '</tr>');

                        var level2_id = globel_id;
                        var level2 = item2.Item.Level2;
                        //third
                        $(
                        data.GroupBy(function (o, i) {
                            if (o.Level1 == level1 && o.Level2 == level2) {
                                return o.Level3
                            }
                        })).each(function (i3, item3) {
                            if (($.trim(item3.Item.Level3).length > 0)) {
                                //debugger;
                                globel_id++;
                                item3.Item.Duration = getLevel3Duration(item3.Item.Level1, item3.Item.Level2, item3.Item.Level3);
                                table.append('<tr id="node-' + globel_id + '" system_id="' + item3.Item.ID + '" class="child-of-node-' + level2_id + '">' +
                                          '<td><span class="expander" style="margin-left: -19px; padding-left: 19px;"></span><span class="file ui-draggable">' + item3.Item.Level3 + '</span></td>' +
                                          '<td><input class="planned" type="checkbox" ' + (item3.Item.HideReasonInReports ? ' checked="checked"' : '') + ' value="true" onclick="setReasonToPlanned(' + item3.Item.ID + ', this)" data-Duration="' + item3.Item.Duration + '" ' + (item.Item.Duration != 0 ? ' title="' + item.Item.Duration + ' minute(s)"' : '') + ' /></td>' +
                                          '<td class="col2">&nbsp;</td>' +
                                          '<td><input type="checkbox" ' + (item3.Item.IsChangeOver ? ' checked="checked"' : '') /*+ (item3.Item.Duration != 0 ? ' title="' + item3.Item.Duration + ' minute(s)"' : '')*/ + ' value="true" class="changeover" /></td>' +
                                          '<td>' + item3.Item.Level1 + '>' + item3.Item.Level2 + '>' + item3.Item.Level3 + '</td>' +
                                          '</tr>');
                            }
                        });
                        if (table.find('tr:last').attr('id').replace('node-', '') == level2_id.toString()) {
                            $(table).find('#node-' + level2_id).removeClass('parent').find('td:first').find('span:last').removeClass('folder').addClass('file');
                        }

                    }

                });
            if (table.find('tr:last').attr('id').replace('node-', '') == level1_id.toString()) {
                $(table).find('#node-' + level1_id).removeClass('parent').find('td:first').find('span:last').removeClass('folder').addClass('file');
            }

        });

        removeMarkTip();
        $('#btnSave').removeAttr('disabled');
        $('#btnReload').removeAttr('disabled');

        $('div#container').empty().append(table);
        _ORG_TABLE_HTML = $(table).clone();
        // treeTable
        $("#dnd-example").treeTable({
            clickableNodeNames: false,
            expandable: true
        });

        $('table#dnd-example tbody tr:first').expand();

        if (allowDrop) {
            // Configure draggable nodes
            $("#dnd-example .file, #dnd-example .folder").draggable({
                helper: "clone",
                opacity: .75,
                refreshPositions: true, // Performance?
                revert: "invalid",
                revertDuration: 300,
                scroll: true
            });

            // Configure droppable rows
            $("#dnd-example .folder").each(function () {
                $(this).parents("tr").droppable({
                    accept: ".file, .folder",
                    drop: function (e, ui) {
                        // Call jQuery treeTable plugin to move the branch
                        //if (!carDrop($(ui.draggable).parents("tr"), this)) return;

                        $($(ui.draggable).parents("tr")).appendBranchTo(this);
                        refreshFullPath();
                    },
                    hoverClass: "accept",
                    over: function (e, ui) {
                        // Make the droppable branch expand when a draggable node is moved over it.
                        if (this.id != $(ui.draggable.parents("tr")[0]).id && !$(this).is(".expanded")) {
                            $(this).expand();
                        }
                    }
                });
            });
        }

        // Make visible that a row is clicked
        $("table#dnd-example tbody tr").mousedown(function () {
            $("tr.selected").removeClass("selected"); // Deselect currently selected rows
            $(this).addClass("selected");
        });

        // Make sure row is selected when span is clicked
        $("table#dnd-example tbody tr span").mousedown(function () {
            $($(this).parents("tr")[0]).trigger("mousedown");
        });

        bindEditEvent(); //bind edit events

        bindDeleteEvent(); //bind delete events

        bindCheckboxEvent(); //

    }, "json");
}

function setReasonToPlanned(id, element) {
    //return;//deprecrated for now
    //debugger;
    var isPlanned = element.checked;

    if (isPlanned) {
        debugger;
        var duration = prompt('Duration of Planned Downtime',$(element).attr("data-duration"));
        if (duration != null) {
            if (!isNaN(duration)) {
                var children = ".child-of-" + element.parentNode.parentNode.getAttribute("id");
                //$(element).data('Duration', duration).next().text('[' + duration + ' min]');
                $(element).attr("data-duration", duration);
                $(element).attr("title", duration + " minute(s)");
                $(children + " .planned").attr("data-duration", duration);
                $(children + " .planned").attr("title", duration + " minute(s)");
                return;
            }
            else {
                alert("You must enter a valid number");
                element.checked = false;
            }
        }
        else {
            element.checked = false;
        }
    }

    //$(element).attr("data-duration", 0);
}

function bindCheckboxEvent() {
    $("table#dnd-example input[type='checkbox']").click(function () {
        var checked = false;

        if ($(this).prop) {
            checked = $(this).prop('checked');
        }
        else {
            checked = $(this).is(':checked');
        }

        var pid = $(this).parent().parent().attr('id').replace('node-', '');

        if ($(this).attr('class') == 'changeover') {
            updateChangeChecked(pid, checked);
        }
        else
            updateChecked(pid, checked);
    });
}

function updateChecked(pid, checked) {
    $('.child-of-node-' + pid).each(function () {
        $(this).find('input[type="checkbox"]:first').attr('checked', checked);
        var cid = $(this).attr('id').replace('node-', '');
        updateChecked(cid, checked);

    });
}

function updateChangeChecked(pid, checked) {
    $('.child-of-node-' + pid).each(function () {
        $(this).find('.changeover').attr('checked', checked);
        cid = $(this).attr('id').replace('node-', '');
        updateChangeChecked(cid, checked);
    });
}


function saveData() {

    markTip("Updating...");
    var dd = getData();

    $.post('Service.ashx?op=saveall', "data=" + UrlEncode(getData()) + "&dels=" + UrlEncode(delids) + "&line=" + UrlEncode(Line), function (result) {
        if (result.toString() != "true") {
            alert(result);
        } else {
            loadTreeTable();
        }
        $('#btnSave').removeAttr('disabled');
        $('#btnReload').removeAttr('disabled');
        removeMarkTip();
    });

    $('#btnSave').attr('disabled', 'disabled');
    $('#btnReload').attr('disabled', 'disabled');


}

function getData() {
    var result = "[";
    $("#dnd-example .file").each(function (i, row) {
        var folder = $(row).parent('td').parent('tr');
        //debugger;
        if ($(folder).attr('isRoot') != 'true') {

            result += (i == 0 ? '' : ',') + '{"Path":"' + $.trim(folder.find('td:last').text()) + '","Id":' + folder.attr('system_id') + ',"HideReasonInReports":' + folder.find('input[type="checkbox"]:first').attr('checked') + ',"IsChangeOver":' + folder.find('.changeover').attr('checked') + ', "Line": "' + Line + '", "Duration": "' + (folder.find('input[type="checkbox"]:first').attr('data-duration') || 0) + '" }';
        }
    });
    result += "]";
    return result;
}

function isFolder(row) {
    var isFolder = false;
    var eles = $(row).find('td:first').find('span:last').attr('className');

    if(!eles)
        return;

    $(eles.split(' ')).each(function () {
        if (this == 'folder') {
            isFolder = true;
            return;
        }
    });
    return isFolder;
}

function carDrop(source, target) {
    var target_index = getItemDeep(target);
    if (target_index >= 3) return false;

    return (getItemDeep(source) + getItemDeepForChildren(target)) < 3;

}

function getItemDeep(item) {
    var i = 1;
    var pid = getParentId(item);
    while (pid > 0) {
        pid = getParentId($('#node-' + pid));
        i++;
    }
    return i;
}

function getItemDeepForChildren(item) {
    var deepArr = new Array();
    var deepRowArr = new Array();
    $('table tr').each(function (i, row) {
        var a = getItemChidrenDeep(item, deepRowArr);
        deepArr.push(a.Deep);
        deepRowArr.push(a.Row);
    });

    if (deepArr.length > 0) {
        deepArr = sort(deepArr);
        return deepArr[deepArr.length - 1];
    } else {
        return 0;
    }
}

function getItemChidrenDeep(item, arr) {
    var result = 0;
    var resultRow = null;
    var id = $(item).attr('id').replace('node-', '');

    $('table tr').each(function (i, row) {
        if (getParentId(row) == id && $.inArray(row, arr) < 0) {
            arr.push(row);
            result += getItemChidrenDeep(row, arr).Deep;
            resultRow = row;
            return;
        }
    });

    this.Deep = result;
    this.Row = result;
    return this;
}

var sort = function (array) {
    var temp;
    for (var i = 0; i < array.length - 1; i++) {
        for (var j = array.length - 1; j >= i; j--)
            if (array[j + 1] < array[j]) {
                temp = array[j + 1];
                array[j + 1] = array[j];
                array[j] = temp;
            }
}
return array;
};


function addChildren(eventTarget, newRowIndex) {
    var parent = $(eventTarget).parent('td').parent('tr');
    var parentId = $(parent).attr('id').replace('node-', '');
    var newId = $("table#dnd-example tbody tr").length + 1;
    while ($('#node-' + newId).length > 0) {
        newId++;
    }

    var item = $('<tr id="node-' + newId + '" class="child-of-node-' + parentId + '">' +
                          '<td><span class="expander" style="margin-left: -19px; padding-left: ' + (newRowIndex == 1 ? 20 : (newRowIndex == 2 ? 39 : 76)) + 'px;"></span><span class="file ui-draggable">&nbsp;</span></td>' +
                          '<td><input type="checkbox"  value="true" onclick="setReasonToPlanned(1, this)"/><span class="duration"></span></td>' +
                          '<td>' + (newRowIndex < 3 ? '<span class="addClhildren" onclick="addChildren(this,' + (newRowIndex + 1) + ');">Add children</span>' : '&nbsp;') + '</td>' +
                          '<td><input type="checkbox"  value="false" /></td>' +
                          '<td>&nbsp;</td>' +
                          '</tr>');

    $(item).find('td:first').find('span:last').unbind("dblclick");
    $(item).find('td:first').find('span:last').dblclick(function () {
        var input = $('<input type="text" style="width:150px" value="' + $.trim($(this).text()) + '" />');
        input.unbind('blur');
        input.blur(function () {
            var value = $(this).val();
            if ($.trim(value).length == 0) {
                $(this).parent('span').parent('td').parent('tr').remove(); //delete thie empty row
                if (!hasChildren(parent)) {
                    $(parent).removeClass("expanded").removeClass("collapsed").removeClass('parent').find('td:first').find('span.folder').removeClass('folder').removeClass('file').addClass('file');
                }
            } else {
                $(this).parent('span').text(value);
                //refreshItemFullPath(item);
                refreshFullPath();
                $(this).remove();
            }
        });
        $(this).empty();
        $(this).append(input);
        input.get(0).focus();
    });

    $(item).insertAfter(parent);

    $(item).find('td:first').find('span:last').dblclick(); //enter edit mode
    if (hasChildren(parent)) {
        $(parent).removeClass('parent').addClass('parent').find('td:first').find('span:last').removeClass('folder').addClass('folder').removeClass('file');
        $(parent).expand();
    }
    configNewRow(item);

}

/*
function bindDeleteEvent() {
    $("table#dnd-example tbody tr").each(function (i, item) {
        //$(item).unbind("keydown");
        $(item)[0].onkeydown = function (e) {
            if (!e) e = event;
            if (e.keyCode == 46) {
                if (isFolder($(item))) {
                    alert('folder can not be delete.');
                } else {
                    if (confirm('Are you sure you want to delete this row? After you click save the row will be permanently deleted.')) {
                        delids += (delids.length > 0 ? "," : "") + $(item).attr('system_id');
                        $(item).remove();
                        var parent = $('#node-' + getParentId(item));
                        if (!hasChildren(parent)) {
                            $(parent).removeClass("expanded").removeClass("collapsed").removeClass('parent').find('td:first').find('span.folder').removeClass('folder').removeClass('file').addClass('file');
                        }
                        refreshFullPath();
                    }
                }
            }
        };

    });
}
*/

function getCurrentTabIndex() {
    var currentTab = $('#tab-container').tabs('option', 'selected');

    if (!isNaN(currentTab)) {
        return currentTab;
    }
    else if (currentTab && currentTab.attr) {
        if (currentTab.attr('id') == 'lkChangeOver')
            return 1;
    }

    return 0;
}

function bindDeleteEvent() {


    $(document).unbind('keydown');
    $(document).keydown(function (e) {
        if (e.keyCode == 46) {

            if (getCurrentTabIndex() > 0)
                return;

            $("table#dnd-example tbody tr").each(function () {
                if ($(this).hasClass('selected')) {
                    var item = this;
                    if (isFolder($(item))) {
                        alert('folder can not be delete.');
                    } else {
                        if (confirm('Are you sure you want to delete this row? After you click save the row will be permanently deleted.')) {
                            delids += (delids.length > 0 ? "," : "") + $(item).attr('system_id');
                            $(item).remove();
                            var parent = $('#node-' + getParentId(item));
                            if (!hasChildren(parent)) {
                                $(parent).removeClass("expanded").removeClass("collapsed").removeClass('parent').find('td:first').find('span.folder').removeClass('folder').removeClass('file').addClass('file');
                            }
                            refreshFullPath();
                        }
                    }
                }

            });
        }

    });
}


function bindEditEvent() {
    $("table#dnd-example tbody tr").each(function (i, item) {
        if ($(item).attr('isRoot') != 'true') {
            $(item).find('td:first').find('span:last').attr('title', 'double click me to edit value');
            $(item).find('td:first').find('span:last').unbind("dblclick");
            $(item).find('td:first').find('span:last').dblclick(function () {
                var input = $('<input type="text" value="' + $.trim($(this).text()) + '" style="width:150px" />');
                input.unbind('blur');
                input.blur(function () {
                    var value = $.trim($(this).val());
                    if (value.length == 0) {
                        alert("reason code is required.");
                        $(this).get(0).focus();
                        return;
                    }
                    $(this).parent('span').text(value);
                    $(this).remove();
                    //refreshItemFullPath(item);
                    refreshFullPath();
                });
                $(this).empty();
                $(this).append(input);
                input.get(0).focus();
            });
        }
    });
}

function refreshItemFullPath(item) {
    var pid = getParentId(item);
    if (pid > 0) {

        var fillPath = $.trim($('#node-' + pid).find('td:first').find('span:last').text());
        while (pid > 0) {
            pid = getParentId($('#node-' + pid));
            if (pid <= 0) break;
            fillPath = $.trim($('#node-' + pid).find('td:first').find('span:last').text()) + ">" + fillPath;
        }

        fillPath += ">" + $.trim($(item).find('td:first').find('span:last').text());
        $(item).find('td:last').html(fillPath);
    } else {
        $(item).find('td:last').html($.trim($(item).find('td:first').find('span:last').text()));
    }
}

function refreshFullPath() {
    $("table#dnd-example tbody tr").each(function (i, item) {
        var pid = getParentId(item);
        if (pid > 0) {

            var fillPath = $.trim($('#node-' + pid).find('td:first').find('span:last').text());
            while (pid > 0) {
                pid = getParentId($('#node-' + pid));
                if (pid <= 0) break;
                fillPath = $.trim($('#node-' + pid).find('td:first').find('span:last').text()) + ">" + fillPath;
            }

            fillPath += ">" + $.trim($(item).find('td:first').find('span:last').text());
            $(item).find('td:last').html(fillPath);
        } else {
            $(item).find('td:last').html($.trim($(item).find('td:first').find('span:last').text()));
        }

        if (!hasChildren(item)) {
            $(item).removeClass('parent').find('td:first').find('span:last').removeClass('folder').removeClass('file').addClass('file');
        } else {
            $(item).removeClass('parent').addClass('parent').find('td:first').find('span:last').removeClass('folder').addClass('folder').removeClass('file');
        }
    });
}

function hasChildren(item) {
    var id = $(item).attr('id').replace('node-', '');


    return $('#dnd-example .child-of-node-' + id).length > 0;
}

function getParentId(row) {
    var result = -1;
    var eles = $(row).attr('className');

    if(!eles)
        return;

    $(eles.split(' ')).each(function (i, className) {
        if (className.toString().indexOf("child-of-node-") >= 0) {
            result = className.toString().replace("child-of-node-", "");
            return;
        }
    });

    return result;
}

function configNewRow(row) {
    if (allowDrop) {
        // Configure draggable nodes
        $(row).draggable({
            helper: "clone",
            opacity: .75,
            refreshPositions: true, // Performance?
            revert: "invalid",
            revertDuration: 300,
            scroll: true
        });
        // Configure droppable rows
        $(row).each(function () {
            $(this).parents("tr").droppable({
                accept: ".file, .folder",
                drop: function (e, ui) {
                    // Call jQuery treeTable plugin to move the branch
                    $($(ui.draggable).parents("tr")).appendBranchTo(this);
                    refreshFullPath();
                },
                hoverClass: "accept",
                over: function (e, ui) {
                    // Make the droppable branch expand when a draggable node is moved over it.
                    if (this.id != $(ui.draggable.parents("tr")[0]).id && !$(this).is(".expanded")) {
                        $(this).expand();
                    }
                }
            });
        });
    }

    // Make visible that a row is clicked
    $(row).mousedown(function () {
        $("tr.selected").removeClass("selected"); // Deselect currently selected rows
        $(this).addClass("selected");
    });

    // Make sure row is selected when span is clicked
    $($('row').find('span')).mousedown(function () {
        $($(this).parents("tr")[0]).trigger("mousedown");
    });
}


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

function markTip(s) {
    $('div:first').append('<div id="markTipDiv" style="position:absolute;top:50%; left:50%;background:#888888;padding:15px;">' + s + '</div>');
}

function removeMarkTip() {
    if ($('#markTipDiv').length > 0) {
        $('#markTipDiv').remove();
    }
}

Array.prototype.GroupBy = function (clause) {
    var newArray = new Array();
    if (typeof (clause) == "function") {
        for (var index = 0; index < this.length; index++) {

            var KeyValues = clause(this[index], index);
            if (KeyValues != null && KeyValues != undefined) {
                var isExsits = false;
                var item = this[index];

                for (var n = 0; n < newArray.length; n++) {
                    if (newArray[n].KeyValue == KeyValues) {
                        newArray[n].KeyCount = parseInt(newArray[n].KeyCount) + 1;
                        newArray[n].Item = this[index];
                        isExsits = true;
                    }
                }
                if (!isExsits) {
                    newArray.push({ KeyValue: KeyValues, KeyCount: 1, Item: item });
                }
            }

        }
    }
    return newArray;
};