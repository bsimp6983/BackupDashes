var firstLoad = true;
var autoCalculate = false;
var Line;
var currentTab = 0;

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

var _dataRange={
    startDate:function()
    {
        return $('#startdate').val();
        

    },
    endDate:function()
    {        
        return ed=$('#enddate').val();
        /*
        var as;
        if($.trim(ed).length>0)
        {
            ed=new Date(Date.parse(ed.replace(/-/g,"/")));
            ed=new Date(ed.getFullYear(),ed.getMonth(),ed.getDate(), ed.getHours(), ed.getMinutes(), ed.getSeconds(), ed.getMilliseconds());
            ed=ed.format('MM/dd/yyyy hh:mm');

        }
        
        return ed;
        */
    }
};

var _firstReportConfig = {
    startDate: _dataRange.startDate(),
    endDate: _dataRange.endDate(),
    level1: '',
    isEvents: true,
    type: 'day',
    day_goal: 10,
    week_goal: 70,
    month_goal: 300,
    year_goal: 3600,
    c_day_goal: 5,
    c_week_goal: 35,
    c_month_goal: 150,
    c_year_goal: 1800,
    goal: function() {
        switch (this.type.toLowerCase()) {
            case 'day':
                return this.isEvents?this.day_goal:this.c_day_goal;
            case 'week':
                return this.isEvents ? this.week_goal : this.c_week_goal;
            case 'month':
                return this.isEvents ? this.month_goal : this.c_month_goal;
            case 'year':
                return this.isEvents ? this.year_goal : this.c_year_goal;
            default:
                return this.isEvents ? this.day_goal : this.c_day_goal;
        }
    }
};

var _secondReportConfig = {
    startDate: _dataRange.startDate(),
    endDate: _dataRange.endDate(),
    level1: '',
    level3: '',
    levelid: 0,
    type: 'day',
    day_goal: _firstReportConfig.day_goal,
    week_goal: _firstReportConfig.week_goal,
    month_goal: _firstReportConfig.month_goal,
    year_goal: _firstReportConfig.year_goal,
    c_day_goal: _firstReportConfig.c_day_goal,
    c_week_goal: _firstReportConfig.c_week_goal,
    c_month_goal: _firstReportConfig.c_month_goal,
    c_year_goal: _firstReportConfig.c_year_goal,
    goal: function(isEvents) {
        if(isEvents==null || isEvents==undefined || isEvents=='')isEvents=true;
        switch (this.type.toLowerCase()) {
            case 'day':
                return isEvents ? this.day_goal : this.c_day_goal;
            case 'week':
                return isEvents ? this.week_goal : this.c_week_goal;
            case 'month':
                return isEvents ? this.month_goal : this.c_month_goal;
            case 'year':
                return isEvents ? this.year_goal : this.c_year_goal;
            default:
                return isEvents ? this.day_goal : this.c_day_goal;
        }
    }
    , refresh: function() {
        this.day_goal= _firstReportConfig.day_goal;
        this.week_goal= _firstReportConfig.week_goal;
        this.month_goal= _firstReportConfig.month_goal;
        this.year_goal= _firstReportConfig.year_goal;
        this.c_day_goal=_firstReportConfig.c_day_goal;
        this.c_week_goal= _firstReportConfig.c_week_goal;
        this.c_month_goal= _firstReportConfig.c_month_goal;
        this.c_year_goal = _firstReportConfig.c_year_goal;
    }
};



var _thirdReportConfig = {
    startDate: _dataRange.startDate(),
    endDate: _dataRange.endDate(),
    level1: '',
    level3: '',
    levelid: 0,
    type: 'day',
    day_goal: _firstReportConfig.day_goal,
    week_goal: _firstReportConfig.week_goal,
    month_goal: _firstReportConfig.month_goal,
    year_goal: _firstReportConfig.year_goal,
    c_day_goal: _firstReportConfig.c_day_goal,
    c_week_goal: _firstReportConfig.c_week_goal,
    c_month_goal: _firstReportConfig.c_month_goal,
    c_year_goal: _firstReportConfig.c_year_goal,
    goal: function(isEvents) {
        if(isEvents==null || isEvents==undefined || isEvents=='')isEvents=true;
        switch (this.type.toLowerCase()) {
            case 'day':
                return isEvents ? this.day_goal : this.c_day_goal;
            case 'week':
                return isEvents ? this.week_goal : this.c_week_goal;
            case 'month':
                return isEvents ? this.month_goal : this.c_month_goal;
            case 'year':
                return isEvents ? this.year_goal : this.c_year_goal;
            default:
                return isEvents ? this.day_goal : this.c_day_goal;
        }
    }
    , refresh: function() {
        this.day_goal= _firstReportConfig.day_goal;
        this.week_goal= _firstReportConfig.week_goal;
        this.month_goal= _firstReportConfig.month_goal;
        this.year_goal= _firstReportConfig.year_goal;
        this.c_day_goal=_firstReportConfig.c_day_goal;
        this.c_week_goal= _firstReportConfig.c_week_goal;
        this.c_month_goal= _firstReportConfig.c_month_goal;
        this.c_year_goal = _firstReportConfig.c_year_goal;
    }
};


var _fourReportConfig = {
    startDate: '',
    endDate: '',
    level1: '',
    level3: '',
    levelid: 0,
    level3_1: '',
    levelid_1: 0,
    isEvents: true,
    type: 'day',
    day_goal: _firstReportConfig.day_goal,
    week_goal: _firstReportConfig.week_goal,
    month_goal: _firstReportConfig.month_goal,
    year_goal: _firstReportConfig.year_goal,
    c_day_goal: _firstReportConfig.c_day_goal,
    c_week_goal: _firstReportConfig.c_week_goal,
    c_month_goal: _firstReportConfig.c_month_goal,
    c_year_goal: _firstReportConfig.c_year_goal,
    goal: function(isEvents) {
        if(isEvents==null || isEvents==undefined || isEvents=='')isEvents=true;
        switch (this.type.toLowerCase()) {
            case 'day':
                return isEvents ? this.day_goal : this.c_day_goal;
            case 'week':
                return isEvents ? this.week_goal : this.c_week_goal;
            case 'month':
                return isEvents ? this.month_goal : this.c_month_goal;
            case 'year':
                return isEvents ? this.year_goal : this.c_year_goal;
            default:
                return isEvents ? this.day_goal : this.c_day_goal;
        }
    }
    , refresh: function() {
        this.day_goal= _firstReportConfig.day_goal;
        this.week_goal= _firstReportConfig.week_goal;
        this.month_goal= _firstReportConfig.month_goal;
        this.year_goal= _firstReportConfig.year_goal;
        this.c_day_goal=_firstReportConfig.c_day_goal;
        this.c_week_goal= _firstReportConfig.c_week_goal;
        this.c_month_goal= _firstReportConfig.c_month_goal;
        this.c_year_goal = _firstReportConfig.c_year_goal;
    }
};

var _fiveReportConfig={
    isEvents: true,
    type: 'day'
};
var _Charts={
    HOME:{
        Top5DowntimeEvents:{
            Name:'mychart1',
            Chart:null
        },
        Top5OccuringEvents:{
            Name:'mychart2',
            Chart:null
        },
        LossBuckets:{
            Name:'mychart3',
            Chart:null
        },
        DowntimeActualVsGoal:
        {
            Name:'mychart4',
            Chart:null
        },
        EfficiencyChart:{
            Name:'mychart20',
            Chart:null
        }
    },
    Top5DowntimeEvents:{
        LossBuckets:{
            Name:"mychart5",
            Chart:null
        },
        Top5DowntimeEvents:{
            Name:"mychart6",
            Chart:null
        },
        DowntimeActualVsGoal:{
            Name:"mychart7",
            Chart:null
        }
        
    },
    Top5OccuringEvents:{
        LossBuckets:{
            Name:"mychart8",
            Chart:null
        },
        Top5OccuringEvents:{
            Name:"mychart9",
            Chart:null
        },
        OccuringActualVsGoal:{
            Name:"mychart10",
            Chart:null
        }
    },
    HistoricalDetail:{
        LossBuckets:{
            Name:"mychart11",
            Chart:null
        },
        Top5DowntimeEvents:{
            Name:"mychart12",
            Chart:null
        },
        Top5OccuringEvents:{
            Name:"mychart13",
            Chart:null
        },
        DowntimeActualVsGoal:{
            Name:"mychart14",
            Chart:null
        },
        DowntimeHistory:{
            Name:"mychart15",
            Chart:null
        },
        OccurrenceHistory:{
            Name:"mychart16",
            Chart:null
        }
    },
    HiddenReasons:{
        Top5DowntmeEvents:{
            Chat:"mychart17",
            Chart:null
        },
        Top5OccuringEvents:{
            Chat:"mychart18",
            Chart:null
        },
        ActualVs:{
            Chat:"mychart19",
            Chart:null
        }
    }
};


$(document).ready(function() {
    Line = $.getUrlVar('line');
    
    if (Line == undefined || Line == '' || Line == null)
        Line = "company-demo";
        
    document.title += ' | ' + Line;
    $('.datepicker').each(function () {
        var dtp = $(this).datetimepicker();

        $(this).datetimepicker('setDate', (new Date()) );

    });

     $("#home-events-grid").tablesorter({
        sortInitialOrder: 'desc',
        sortedList: [
            [1,1]
        ]
     }); 

    PFChart.init({
        initBtns: false,
        graph: false,
        timeline: true,
        line: Line
    });

    $('#clientLines').val(Line);

    $('#clientLines').bind('change', function () {
        var line = $(this).find(':selected').val();

        if (line && line != Line) {
            location = 'DCSDashboard.aspx?line=' + line;
        }
    });
    
    $('#btnGo').click(function(){
        autoCalculate = true;
        initReports();

        PFChart.StartTime = moment($('#startdate').val());
        PFChart.EndTime = moment($('#enddate').val());

        var sd = moment($('#startdate').val());
        var ed = moment($('#enddate').val());

        var tmpDate = ed.clone();

        PFChart.createTimeLine(tmpDate.subtract('days', 1), ed);

    });
    
    $('#fake-body').corner();
    

    //setInterval('updateDock()', 20);
    
    $('#btnUI').click(function()
    {
        if($('body').css('background-image')!='none')
        {
            showPrintUI();
        }else{
            showDefaultUI();
        }
    });

    $('#radShowBasic').click(function () {
        switchEventTableView(false);
    });

    $('#radShowAdvance').click(function () {
        switchEventTableView(true);
    });

    $('#txtEventSearch').keyup(function (e) {
        var searc = $(e.target).val();

        $('#home-events-grid .cumulative').each(function () {
            var txt = $(this).text().replace('%', '');

            if (!isNaN(searc) && searc) {
                if (!isNaN(txt)) {
                    txt = parseFloat(txt);

                    if (txt < searc) {
                        $(this).parent().show();
                    } else {
                        $(this).parent().hide();
                    }
                }
            }
            else {
                $(this).parent().show();
            }
        });
    });
    
    var nd=new Date();
    nd=new Date(nd.getFullYear(),nd.getMonth(),nd.getDate()-7);
    $('#startdate').val(nd.format('MM/dd/yyyy hh:mm'));
    
    //$('#enddate').val((new Date()).format('MM/dd/yyyy hh:mm'));
    
    initDock();
    bindTabItemEvents();
    
    GetRedLines();

    initReports();

    $('#DowntimeActualVsGoal input[type="button"].cmd').click(function() {
        _firstReportConfig.type = $(this).attr('reportType');
        DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1, _firstReportConfig.isEvents, _firstReportConfig.goal(), _firstReportConfig.type);
    });

    $('#DowntimeActualVsGoal input[type="radio"][name="DowntimeActualVsGoal_Switchto"]').click(function() {
        _firstReportConfig.isEvents = (parseInt($(this).val()) == 1)
        DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1, _firstReportConfig.isEvents, _firstReportConfig.goal(), _firstReportConfig.type);
    });

    $('#DowntimeActualVsGoal input[type="button"].setting').click(function() {
        settingVoid();
    });

    $('#TI_Top5DowntimeEvents_DowntimeActualVsGoal input[type="button"].cmd').click(function() {
        _secondReportConfig.type = $(this).attr('reportType');
        _secondReportConfig.refresh();
        Top5DowntimeEvents_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _secondReportConfig.level1, _secondReportConfig.goal(), _secondReportConfig.type, _secondReportConfig.levelid, _secondReportConfig.level3);
    });

    $('#TI_Top5DowntimeEvents_DowntimeActualVsGoal input[type="button"].setting').click(function() {
        settingVoid();
    });
    
    $('#TI_Top5DowntimeEvents_Comments input[type="button"].xls').click(function(){
        window.open("ExportXLS.aspx?op=dcd_Top5DowntimeEvents_Comments&startdate=" + UrlEncode(_dataRange.startDate()) + "&enddate=" + UrlEncode(_dataRange.endDate()) + "&level1=" + UrlEncode(_secondReportConfig.level1) + "&level3=" + UrlEncode(_secondReportConfig.level3) + "&level3_id=" + _secondReportConfig.levelid + "&line=" + Line);
    });
    
    
    
    $('#Top5OccuringEvents_OccuringActualVsGoal input[type="button"].cmd').click(function() {
        _thirdReportConfig.type = $(this).attr('reportType');
        _thirdReportConfig.refresh();
        Top5OccuringEvents_OccuringActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _thirdReportConfig.level1, _thirdReportConfig.goal(), _thirdReportConfig.type, _thirdReportConfig.levelid, _thirdReportConfig.level3);
    });

    $('#Top5OccuringEvents_OccuringActualVsGoal input[type="button"].setting').click(function() {
        settingVoid();
    });
    
    $('#Top5OccuringEvents_Comments input[type="button"].xls').click(function(){
        window.open("ExportXLS.aspx?op=dcd_Top5DowntimeEvents_Comments&startdate=" + UrlEncode(_dataRange.startDate()) + "&enddate=" + UrlEncode(_dataRange.endDate()) + "&level1=" + UrlEncode(_thirdReportConfig.level1) + "&level3=" + UrlEncode(_thirdReportConfig.level3) + "&level3_id=" + _thirdReportConfig.levelid + "&line=" + Line);
    });
    
    $('#HistoricalDetail_DowntimeActualVsGoal input[type="button"].cmd').click(function() {
        _fourReportConfig.type = $(this).attr('reportType');
        HistoricalDetail_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _fourReportConfig.level1, _fourReportConfig.isEvents, _fourReportConfig.goal(), _fourReportConfig.type);
    });
    
    $('#TI_HistoricalDetail input[type="button"].xls').click(function(){
    
        window.open("ExportXLS.aspx?op=dcd_Events&startdate=" + UrlEncode(_dataRange.startDate()) + "&enddate=" + UrlEncode(_dataRange.endDate()) + "&line=" + Line);
    });

    $('.efficiencytoxls').click(function(){
            window.open("ExportXLS.aspx?op=dcd_Efficiency&startdate=" + UrlEncode(_dataRange.startDate()) + "&enddate=" + UrlEncode(_dataRange.endDate()) + "&line=" + Line);

    });
    $('#HistoricalDetail_DowntimeActualVsGoal input[type="radio"][name="HistoricalDetail_DowntimeActualVsGoal_Switchto"]').click(function() {
        _fourReportConfig.isEvents = (parseInt($(this).val()) == 1)
        HistoricalDetail_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _fourReportConfig.level1, _fourReportConfig.isEvents, _fourReportConfig.goal(), _fourReportConfig.type);
    });

    $('#HistoricalDetail_DowntimeActualVsGoal input[type="button"].setting').click(function() {
        settingVoid();
    });
    
    
    $('#HistoricalDetail_DowntimeHistory input[type="button"].cmd').click(function() {
        HistoricalDetail_DowntimeHistory(_dataRange.startDate(),_dataRange.endDate(), _fourReportConfig.levelid, $(this).attr('reportType'));
    });
    
    $('#HistoricalDetail_OccurrenceHistory input[type="button"].cmd').click(function() {
        HistoricalDetail_OccurrenceHistory(_dataRange.startDate(),_dataRange.endDate(), _fourReportConfig.levelid_1, $(this).attr('reportType'));
    });
    
    $('#TI_HiddenReasons_actualAs input[type="button"].cmd').click(function() {
        _fiveReportConfig.type = $(this).attr('reportType');
        Hidden_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(),"",_fiveReportConfig.isEvents,0,_fiveReportConfig.type);
     });

    $('#TI_HiddenReasons_actualAs input[type="radio"][name="TI_HiddenReasons_actualAs_Switchto"]').click(function() {
        _fiveReportConfig.isEvents = (parseInt($(this).val()) == 1)
        Hidden_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(),"",_fiveReportConfig.isEvents,0,_fiveReportConfig.type);
    });

    $('.setgoals').click(function() { GetRedLines(); });

    $('.day').removeClass('day').val('Day');
    $('.week').removeClass('week').val('Week');
    $('.month').removeClass('month').val('Month');
    $('.year').removeClass('year').val('Year');
    $('.setgoals').removeClass('setgoals').val('Set Goals');

    //EfficiencyChart,LossBuckets,lossListBtn,efficiencyChartBtn
    $('#lossListBtn,#efficiencyChartBtn').click(function(){
         var sd=_dataRange.startDate();
        var ed=_dataRange.endDate();
        if($(this).attr('id')=='lossListBtn'){
            $(this).attr('disabled','disabled');
            $('#efficiencyChartBtn').removeAttr('disabled');
            $('#EfficiencyChart').hide();
            $('.eftype').hide();
            $('#LossBuckets').show();
            $('#Top5DowntimeEvents,#Top5OccuringEvents,#DowntimeActualVsGoal,#DowntimeActualVsGoal_rows').show();
            LossBuckets(sd,ed);
        }else{
            $(this).attr('disabled','disabled');
            $('#lossListBtn').removeAttr('disabled');
            $('#EfficiencyChart').show();
            $('.eftype').show();
            $('#LossBuckets').hide();
            $('#Top5DowntimeEvents,#Top5OccuringEvents,#DowntimeActualVsGoal,#DowntimeActualVsGoal_rows').hide();
            EfficiencyChart(sd,ed);
        }
    });

    $('.eftype').click(function(){
        var curef=$(this);
        //$(this).attr('disabled','disabled');
        var sd=_dataRange.startDate();
        var ed=_dataRange.endDate();
        autoCalculate = false;
        EfficiencyChart(sd,ed);
    });

    $('#efNumeratorDenominator').click(function(){
        alert('numerator:'+ef_numerator+',denominator:'+ef_denominator);
    });

    $('#efficiencyChartBtn').click();

    //efficiencyChartSetting
    $('#efficiencyChartSettingBtn').click(function(){
        $.post("DCSDashboard.aspx","op=getEfficiencyChartSetting&line=" + Line,function(data){
            var Mark_v=data.Mark;
            var MaxCaseCount_v=data.MaxCaseCount;
            $.fn.jmodal({
		        initWidth: 400,
		        title: 'Efficiency Chart Setting',
		        content: "<table style='width:30px;margin:0px auto;'><tr><td>Mark:</td><td><input type='text' id='a_Mark' value='"+Mark_v+"' style='width:100px' /></td></tr><tr><td>MaxCaseCount:</td><td><input type='text' value='"+MaxCaseCount_v+"' id='a_MaxCaseCount'  style='width:100px' />&nbsp;&nbsp;<input type='button' id='efSettingBtn' value='Save' /></td></tr></table>",
		        buttonText: 'Cancel',
		        okEvent: function(e) { }
	        });

            $('#efSettingBtn').unbind('click');
            $('#efSettingBtn').click(function(){
                var u_mk,u_mc;
                u_mk=$('#a_Mark').val();
                u_mc=$('#a_MaxCaseCount').val();
                if(isNaN(u_mk) || $.trim(u_mk).length==0){
                    alert('Mark must be decimal');
                    $('#a_Mark')[0].focus();
                    return false;
                }

                if(isNaN(u_mc) || $.trim(u_mc).length==0){
                    alert('MaxCaseCount must be decimal');
                    $('#a_MaxCaseCount')[0].focus();
                    return false;
                }

                $.post("DCSDashboard.aspx",{op:"saveEfficiencyChartSetting",Mark:u_mk,MaxCaseCount:u_mc, line: Line},function(result){
                    if(result=="true"){
                        var sd=_dataRange.startDate();
                        var ed=_dataRange.endDate();
                        EfficiencyChart(sd,ed);
                        $("#jmodal-bottom-okbutton").click();
                    }else{
                        alert('save setting error,please try again.');
                    }
                });
            });
        },"json");
    });

    //auto refresh EfficiencyChart
    var interval=1000*60*15;
    setInterval(function(){
        var sd=_dataRange.startDate();
        var ed=_dataRange.endDate();
        //autoCalculate = false;
        EfficiencyChart(sd,ed);
    },interval);

    setInterval("getEfficiency()",60*1000*5);

    /*setInterval(function(){
        var sd=_dataRange.startDate();
        var ed=_dataRange.endDate();
        EfficiencyChart(sd,ed);
    },60*1000*60);*/

    $('input[type=radio][name=showAs]').click(function(){
        getEfficiency();
        var sd=_dataRange.startDate();
        var ed=_dataRange.endDate();
        EfficiencyChart(sd,ed);
    });
});

function calculateKPI(startdate, enddate, fn) {
    $.get("Service.ashx", "op=kpi&sd=" + UrlEncode(startdate) + "&ed=" + UrlEncode(enddate) + "&line=" + UrlEncode(Line), function (data) {
        
        
        $('#totalDowntime').text(data.Downtime.toFixed(2));
        $('#mtbf').text(data.MTBF.toFixed(2));
        $('#rf').text(data.RF.toFixed(2));
        $('#uf').text(data.UF.toFixed(2));


        if (fn)
            fn(data);

    });
}

function switchEventTableView(showAdvance) {
    var tbl = $('#home-events-grid');

    if (showAdvance) {
        tbl.addClass('show-advance');

        $('.advanced').css('display', '');

    } else {
        tbl.removeClass('show-advance');

        $('.advanced').css('display', 'none');
    }
}

function getEfficiencyChartType(){
        var s="the recent 12 hours";
        $('.eftype').each(function(){
            if($(this).attr('disabled')){
                s=$(this).attr('ctype');
                return ;
            }
        });
        return s;
}
function updateDock()
{
    $('#dock').attr('ascrollTop',$("body").get(0).scrollTop);
     $('#dock').attr('aoffsetTop',$("body").get(0).offsetTop);
     $('#dock').attr('aclientHeight',$("body").get(0).clientHeight);
     $('#dock').attr('screenTop',$(document).scrollTop());
     
    $('#dock').css('position','absolute').css("top",($(window).height()+$(document).scrollTop() - $('#dock').height()-10)+'px');
}

function showPrintUI()
{
    $('body').css('background-image','none');
    $('#header').css('display','none');
    $('#dock').css('display','none');
    $('#DowntimeActualVsGoal_rows').css('height','auto');
    $('#scroll-body').css('overflow-y','').css('overflow-x','').css('overflow','');
    setTimeout("$(window).get(0).print()",700);
}

function showDefaultUI()
{
    $('body').css('background','url(images/rp-fxed-bg.jpg) fixed center top no-repeat');
    $('#header').css('display','block');
    $('#dock').css('display','block');
    $('#DowntimeActualVsGoal_rows').css('height','350px');
    $('#scroll-body').css('overflow-y','auto');
    initDock();
}

function initReports()
{
    var sd=_dataRange.startDate();
    var ed=_dataRange.endDate();   
    
    getTotalDowntime(sd,ed);

	if(currentTab == 0){
        loadTab1(sd, ed);
	}
	else if(currentTab == 1){
        loadTab2(sd, ed);
	}
	else if(currentTab == 2){
        loadTab3(sd, ed);
	}
	else if(currentTab == 3){
        loadTab4(sd, ed);
	}
	else if(currentTab == 4){
        loadTab5(sd, ed);
	}

    _secondReportConfig.levelid = 0;
    _thirdReportConfig.levelid = 0;
    _fourReportConfig.levelid = 0;

    calculateKPI(sd, ed);
}

function getTotalDowntime(startdate, enddate)
{
    if(this.getTotalDowntimeAjax){
       if(this.lastStartDate && this.lastEndDate){
            if(this.lastStartDate == startdate && this.lastEndDate == enddate)
                return;
       }

       this.getTotalDowntimeAjax.abort();
       this.getTotalDowntimeAjax = null;
    }

     this.getTotalDowntimeAjax = $.post("DCSDashboard.aspx", "op=TotalDowntime&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&line=" + Line, function(data) {
        $('#Total-Downtime label:first').text(data);
     });

    this.lastStartDate = startdate;
    this.lastEndDate = enddate;
}

function tryRemoveOldChart(obj){
    if(!obj)return;
    if(!obj.Name)return;
    $('#'+obj.Name).remove();
}

function loadTab1(sd, ed){
	
	if($('#lossListBtn').attr('disabled')){
		LossBuckets(sd,ed);
        $('#lblLoadingEffChart').hide();
	}else{
		EfficiencyChart(sd,ed);
		getEfficiency();
	}

    Top5DowntimeEvents(sd,ed, _firstReportConfig.level1);
    Top5OccuringEvents(sd,ed, _firstReportConfig.level1);
    DowntimeActualVsGoal(sd,ed, _firstReportConfig.level1,_firstReportConfig.isEvents,_firstReportConfig.goal(),_firstReportConfig.type);
    GetEventRows(sd,ed,_firstReportConfig.level1);
}

function loadTab2(sd, ed){
	Top5DowntimeEvents_LossBuckets(sd,ed);
    Top5DowntimeEvents_Top5DowntimeEvents(sd,ed, _secondReportConfig.level1);
}

function loadTab3(sd, ed){
    Top5OccuringEvents_LossBuckets(sd,ed);
    Top5OccuringEvents_Top5OccuringEvents(sd,ed, _thirdReportConfig.level1);
}

function loadTab4(sd, ed){
    HistoricalDetail_LossBuckets(sd,ed);
    HistoricalDetail_Top5DowntimeEvents(sd,ed, _fourReportConfig.level1);
    HistoricalDetail_Top5OccuringEvents(sd,ed, _fourReportConfig.level1);
    HistoricalDetail_DowntimeActualVsGoal(sd,ed, _fourReportConfig.level1,_fourReportConfig.isEvents,_fourReportConfig.goal(),_fourReportConfig.type);
}

function loadTab5(sd, ed){
	Hidden_top5downtime(sd,ed,"");
	Hidden_top5occuring(sd,ed,"");
	Hidden_DowntimeActualVsGoal(sd,ed,"",_fiveReportConfig.isEvents,0,_fiveReportConfig.type);
	Hidden_GetEventRows(sd,ed,"");	
}

function LossBuckets(startdate, enddate) {
    var width = 900, height = 300;

    if(this.getLossBucketsAjax){
        this.getLossBucketsAjax.abort();
        this.getLossBucketsAjax = null;
    }

    this.getLossBucketsAjax = $.post("DCSDashboard.aspx", "op=LossBuckets&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
        
             var vChart =null;
             tryRemoveOldChart(_Charts.HOME.LossBuckets);
             var vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.HOME.LossBuckets.Name, width, height, "Transparent", true);
            
            vChart.setDataXml(data);
            vChart.preLoad = function(args) {
                var chart = args[0];
                if(chart.Series.length==0)return ;
                chart.Series[0].MouseLeftButtonUp = function(dataPoint) {
                    _firstReportConfig.startDate = startdate;
                    _firstReportConfig.endDate = enddate;
                    _firstReportConfig.level1 = dataPoint.AxisXLabel;

                    loadTab1(startdate, enddate);
                };
            }
            
                    
            vChart.loaded=function(args){
            
                tryRemoveOldChart(_Charts.HOME.EfficiencyChart);
                //tryRemoveOldChart(_Charts.HOME.Top5DowntimeEvents);
                //tryRemoveOldChart(_Charts.HOME.Top5OccuringEvents);

                var chart = args[0];
                if(chart.Series[0]==null || chart.Series[0].DataPoints==null || chart.Series[0].DataPoints.length==0)return ;
                
                _firstReportConfig.startDate = startdate;
                _firstReportConfig.endDate = enddate;
                _firstReportConfig.level1 = "";//chart.Series[0].DataPoints[0].AxisXLabel;
             };

            vChart.render($('#LossBuckets .report').get(0));
             _Charts.HOME.LossBuckets.Chart=vChart;

    });
}

function getEfficiencyChartShowAs() {
    return $('input[type=radio][name=showAs]:checked').val();
}

var effChartLoading = false;
var effChartLastType = null;
var effChartLastShowAs = null;

function EfficiencyChart(startdate,enddate){
    var width = 700, height = 300;

    if(effChartLoading == true || (getEfficiencyChartType() == effChartLastType && getEfficiencyChartShowAs() == effChartLastShowAs))
        return;

    $('#lblLoadingEffChart').show();

    effChartLoading = true;
    effChartLastLastType = getEfficiencyChartType();
    effChartLastShowAs = getEfficiencyChartShowAs();

    if(this.getEffChart){//Efficiency always grabs the last 12hrs. No need to re-grab and interupt the last one. 
        this.getEffChart.abort();
        this.getEffChart = null;
    }

    this.getEffChart = $.post("DCSDashboard.aspx", "op=getLeft12HoursCountCase&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate)+"&type="+effChartLastLastType + "&width=" + width + "&height=" + height+"&clientTime="+UrlEncode((new Date()).format('yyyy/MM/dd HH:mm:ss'))+ "&showas=" + effChartLastShowAs + "&ac=" + autoCalculate + "&line=" + Line, function(data) {
        
             var vChart =null;
             tryRemoveOldChart(_Charts.HOME.EfficiencyChart);
             var vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.HOME.EfficiencyChart.Name, width, height, "Transparent", true);

            
            vChart.setDataXml(data);
            

            vChart.render($('#EfficiencyChart .report').get(0));
             _Charts.HOME.EfficiencyChart.Chart=vChart;

             effChartLoading = false;

            $('#lblLoadingEffChart').hide();
            this.getEffChart = null;
    });
}
var ef_numerator=0;
var ef_denominator=0;
function getEfficiency(){

    if(this.getEffAjax){
        return;
    }

    this.getEffAjax = $.post("DCSDashboard.aspx", {op:"getEfficiency",clientTime:(new Date()).format('yyyy/MM/dd HH:mm:ss'), line: Line},function(data){
        $('#runningEfficiency').text(data.RunningEfficiency);
        $('#yesterdayEffiency').text(data.YesterdayEffiency);
        ef_numerator=data.Numerator;
        ef_denominator=data.Denominator;
        this.getEffAjax = null;
        /*
        if (prent2num(data.RunningEfficiency) < data.Mask) {
            $('#EfficiencyChart>div:first').css('background-image', 'url(images/Efficiency-r.jpg)');
        } else {
            $('#EfficiencyChart>div:first').css('background-image', 'url(images/Efficiency.jpg)');
        }
        */

    },"json");
}

function prent2num(v) {
    v = v.toString().replace('%');
    return parseFloat(v);

}

function Top5DowntimeEvents(startdate, enddate,level1) {
    var width = 450, height = 250;

    if(this.getTop5DTEventsAjax){
        this.getTop5DTEventsAjax.abort();
        this.getTop5DTEventsAjax = null;
    }

    this.getTop5DTEventsAjax = $.post("DCSDashboard.aspx", "op=Top5DowntimeEvents&isHome=true&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {

             var vChart =null;
             tryRemoveOldChart(_Charts.HOME.Top5DowntimeEvents);
             var vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.HOME.Top5DowntimeEvents.Name, width, height, "Transparent", true);

            vChart.setDataXml(data);
            
            vChart.preLoad = function(args)
            {   
                var chart = args[0];
                chart.Titles[0].MouseLeftButtonUp = function(title)
                {
                    $('#Top5DowntimeEventsHref').click();
                }
            }

            vChart.render($('#Top5DowntimeEvents .report').get(0));
             _Charts.HOME.Top5DowntimeEvents.Chart=vChart;

    });
}


function Top5OccuringEvents(startdate, enddate, level1) {
    var width = 450, height = 250;

    if(this.getTop5OCCEventsAjax){
        this.getTop5OCCEventsAjax.abort();
        this.getTop5OCCEventsAjax = null;
    }

    this.getTop5OCCEventsAjax = $.post("DCSDashboard.aspx", "op=Top5OccuringEvents&isHome=true&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {

             var vChart =null;
             tryRemoveOldChart(_Charts.HOME.Top5OccuringEvents);
             var vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.HOME.Top5OccuringEvents.Name, width, height, "Transparent", true);

                 
            vChart.setDataXml(data);
            vChart.preLoad = function(args)
            {   
                var chart = args[0];
                chart.Titles[0].MouseLeftButtonUp = function(title)
                {
                    $('#Top5OccuringEventsHref').click();
                }
            }
            vChart.render($('#Top5OccuringEvents .report').get(0));
            _Charts.HOME.Top5OccuringEvents.Chart=vChart;

    });
}


function DowntimeActualVsGoal(startdate, enddate, level1,isEvents,goal, type) {
    var width = 450, height = 300;

    if(this.getDTActualVsGoalAjax){
        this.getDTActualVsGoalAjax.abort();
        this.getDTActualVsGoalAjax = null;
    }

    this.getDTActualVsGoalAjax = $.post("DCSDashboard.aspx", "op=DowntimeActualVsGoal&isHome=true&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&goal=" + goal + "&type=" + UrlEncode(type.toString()) + "&isEvents=" + isEvents + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
             var vChart =null;
             tryRemoveOldChart(_Charts.HOME.DowntimeActualVsGoal);
             var vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.HOME.DowntimeActualVsGoal.Name, width, height, "Transparent", true);

            
            
            vChart.setDataXml(data);
            vChart.preLoad = function(args)
            {   
                var chart = args[0];
                chart.Titles[0].MouseLeftButtonUp = function(title)
                {
                    $('#HistoricalDetailHref').click();
                }
            }
            vChart.render($('#DowntimeActualVsGoal .report').get(0));
            _Charts.HOME.DowntimeActualVsGoal.Chart=vChart;
    });
}

function GetEventRows(startdate,enddate,level1)
{
    var tb=$('#home-events-grid');
    var showAdvance = $('#radShowAdvance').is(':checked');//tb.hasClass('show-advance');

     var width = 450, height = 300;

    if(this.getEventRowAjax){
        this.getEventRowAjax.abort();
        this.getEventRowAjax = null;
    }

    this.getEventRowAjax = $.post("DCSDashboard.aspx", "op=GetEventRows&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1)+ "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
       tb.find('tbody tr').remove();
       $(data).each(function(i,item){
            tb.find('tbody').append('<tr class="data">'+
                            '<td>'+item.Minutes+'</td>'+
                            '<td>'+item.Occurrences+'</td>'+
                            '<td style="text-align:left;">'+item.Path+'</td>'+

                            '<td style="text-align:left; display: none;" class="advanced">' + item.Duration + '</td>' +
                            '<td style="text-align:left; display: none;" class="advanced">' + item.Frequency + '</td>' +
                            '<td style="text-align:left; display: none;" class="advanced">' + item.Average + '</td>' +
                            '<td style="text-align:left; display: none;" class="advanced cumulative">' + item.Cumulative + '</td>' +
                      '</tr>');

            switchEventTableView(showAdvance);
       });

       tb.trigger("update"); 
    },"json");
}

function Top5DowntimeEvents_LossBuckets(startdate, enddate) {
    var width = 900, height = 300;

    if(this.getTop5DTEvents_LossBuckAjax){
        this.getTop5DTEvents_LossBuckAjax.abort();
        this.getTop5DTEvents_LossBuckAjax = null;
    }

    this.getTop5DTEvents_LossBuckAjax = $.post("DCSDashboard.aspx", "op=LossBuckets&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
             var vChart =null;
             tryRemoveOldChart(_Charts.Top5DowntimeEvents.LossBuckets);
             tryRemoveOldChart(_Charts.Top5DowntimeEvents.DowntimeActualVsGoal);
             tryRemoveOldChart(_Charts.Top5DowntimeEvents.Top5DowntimeEvents);
             vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.Top5DowntimeEvents.LossBuckets.Name, width, height,"Transparent",true);

            
            vChart.setDataXml(data);
            vChart.preLoad = function(args) {
                var chart = args[0];
                if(chart.Series.length==0)return ;
                chart.Series[0].MouseLeftButtonUp = function(dataPoint) {
                    _secondReportConfig.startDate = startdate;
                    _secondReportConfig.endDate = enddate;
                    _secondReportConfig.level1 = dataPoint.AxisXLabel;
                    loadTab2(startdate, enddate);
                };
                
            }
            
            vChart.loaded=function(args){
               try{
                    var chart = args[0];
                    if(chart.Series[0]==null || chart.Series[0].DataPoints==null || chart.Series[0].DataPoints.length==0)return ;
                    _secondReportConfig.startDate = startdate;
                    _secondReportConfig.endDate = enddate;
                    _secondReportConfig.level1 = "";//chart.Series[0].DataPoints[0].AxisXLabel;
               }catch(ex)
               {
                    alert(ex);
               }
             };

            vChart.render($('#TI_Top5DowntimeEvents_LossBuckets .report').get(0));
            _Charts.Top5DowntimeEvents.LossBuckets.Chart=vChart;
    });
}

function Top5DowntimeEvents_Top5DowntimeEvents(startdate, enddate, level1) {
    var width = 900, height = 300;

    if(this.getTop5DTEventEventsAjax){
        this.getTop5DTEventEventsAjax.abort();
        this.getTop5DTEventEventsAjax = null;
    }

    this.getTop5DTEventEventsAjax = $.post("DCSDashboard.aspx", "op=Top5DowntimeEvents_Top5DowntimeEvents&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {

            var vChart = null;
            tryRemoveOldChart(_Charts.Top5DowntimeEvents.Top5DowntimeEvents);
            vChart=new Visifire('SL.Visifire.Charts.xap', _Charts.Top5DowntimeEvents.Top5DowntimeEvents.Name, width, height, "Transparent", true);

            
            vChart.setDataXml(data);
            vChart.preLoad = function(args) {
                try{
                var chart = args[0];
                if(chart.Series.length==0)return ;
                chart.Series[0].MouseLeftButtonUp = function(dataPoint) {
                    _secondReportConfig.levelid = parseInt(dataPoint.LegendText);
                    _secondReportConfig.level3 = dataPoint.AxisXLabel;
                    _secondReportConfig.refresh();
                    Top5DowntimeEvents_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _secondReportConfig.level1, _secondReportConfig.goal(), _secondReportConfig.type, _secondReportConfig.levelid, _secondReportConfig.level3);
                    Top5DowntimeEvents_Comments(_dataRange.startDate(),_dataRange.endDate(), _secondReportConfig.level1, _secondReportConfig.levelid, _secondReportConfig.level3);
                };
            }catch(err){
                    alert(err);
                }
            };
           
           vChart.loaded=function(args){
               try{
                    if(args==null || args.length==0)return ;
                    
                    var chart = args[0];
                    if(chart.Series[0]==null || chart.Series[0].DataPoints==null || chart.Series[0].DataPoints.length==0)return ;
                    _secondReportConfig.levelid = 0;//parseInt(chart.Series[0].DataPoints[0].LegendText);
                    _secondReportConfig.level3 = chart.Series[0].DataPoints[0].AxisXLabel;
                    _secondReportConfig.refresh();
                    Top5DowntimeEvents_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _secondReportConfig.level1, _secondReportConfig.goal(), _secondReportConfig.type, _secondReportConfig.levelid, _secondReportConfig.level3);
                    Top5DowntimeEvents_Comments(_dataRange.startDate(),_dataRange.endDate(), _secondReportConfig.level1, _secondReportConfig.levelid, _secondReportConfig.level3);
                }catch(err)
                {
                    alert(err);
                }
            };
            vChart.render($('#TI_Top5DowntimeEvents_Top5DowntimeEvents .report').get(0));
            _Charts.Top5DowntimeEvents.Top5DowntimeEvents.Chart=vChart;
       
    });
}


function Top5DowntimeEvents_DowntimeActualVsGoal(startdate, enddate, level1, goal,type,levelid,level3) {
    var width = 900, height = 300;

    if(this.getTop5DTEventsDTActualVsGoalAjax){
        this.getTop5DTEventsDTActualVsGoalAjax.abort();
        this.getTop5DTEventsDTActualVsGoalAjax = null;
    }

    this.getTop5DTEventsDTActualVsGoalAjax = $.post("DCSDashboard.aspx", "op=Top5DowntimeEvents_DowntimeActualVsGoal&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&type=" + UrlEncode(type) + "&level1=" + UrlEncode(level1) + "&level3=" + UrlEncode(level3) + "&level3_id=" + levelid + "&goal=" + goal + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {

            var vChart =null;
            tryRemoveOldChart(_Charts.Top5DowntimeEvents.DowntimeActualVsGoal);
            vChart=new Visifire('SL.Visifire.Charts.xap', _Charts.Top5DowntimeEvents.DowntimeActualVsGoal.Name, width, height, "Transparent", true);

            vChart.setDataXml(data);
            vChart.render($('#TI_Top5DowntimeEvents_DowntimeActualVsGoal .report').get(0));
            _Charts.Top5DowntimeEvents.DowntimeActualVsGoal.Chart=vChart;

    });
}

var Top5DowntimeEvents_Comments_TB = null;
function Top5DowntimeEvents_Comments(startdate, enddate, level1, levelid, level3) {
    var tab = $('#TI_Top5DowntimeEvents_Comments table:first');
    if($.trim(level3).length>0){
        $(tab).find('caption').html('Comments for '+level3);
    }else if($.trim(level3).length==0 && $.trim(level1).length>0)
    {
        $(tab).find('caption').html('Comments for '+level1);
    }else{
        $(tab).find('caption').html('Comments');
    }

    if(this.getTop5DTEventsCommentsAjax){
        this.getTop5DTEventsCommentsAjax.abort();
        this.getTop5DTEventsCommentsAjax = null;
    }
        
    this.getTop5DTEventsCommentsAjax = $.post("DCSDashboard.aspx", "op=Top5DowntimeEvents_Comments&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&level3=" + UrlEncode(level3) + "&level3_id=" + levelid + "&line=" + Line, function(data) {      
       
        if (data) {
            if (Top5DowntimeEvents_Comments_TB != null) {
                Top5DowntimeEvents_Comments_TB.fnClearTable();
            }
            $.each($(data), function() {
                if(Top5DowntimeEvents_Comments_TB ==null)
                {
                    var html = '';
                    if ($(tab).find("tr").length == 0) {
                        html = "<thead><tr bgcolor='#F1DEB8' height='30'>";
                        html += "<th width='100'>Data</th>";
                        html += "<th>Comment</th>";
                        html += "</tr></thead>";
                        $(tab).append(html);
                    }

                    html = "<tr height='22'>";
                    html += "<td>" + this.EventStartTimeStr + "</td>";
                    html += "<td>" + this.Comment + "</td>";
                    html += "</tr>";
                    $(tab).append(html);
                }else{
                    Top5DowntimeEvents_Comments_TB.fnAddData([this.EventStartTimeStr, this.Comment]);
                }
            });


            var setting = {
                "fnRowCallback": function(nRow, aData, iDisplayIndex) {
                    return nRow;
                },
                "sPaginationType": "full_numbers",
                "bLengthChange": false,
                "bFilter": false,
                "bSort": false,
                "bInfo": false,
                "bAutoWidth": false,
                "iDisplayLength": 5,
                "bProcessing": false,
                "bStateSave": false,
                "bInitialised": false,
                "iDisplayStart":0,
                "aoColumns": [null,null],
            };
            if (Top5DowntimeEvents_Comments_TB != null) {
                Top5DowntimeEvents_Comments_TB.fnDraw(false);
            } else {
                Top5DowntimeEvents_Comments_TB = $(tab).dataTable(setting);
            }
        }

    }, 'json');
}

function Top5OccuringEvents_LossBuckets(startdate, enddate) {
    var width = 900, height = 300;

    if(this.getTop50OccurEvents_LossBucketAjax){
        this.getTop50OccurEvents_LossBucketAjax.abort();
        this.getTop50OccurEvents_LossBucketAjax = null;
    }

    this.getTop50OccurEvents_LossBucketAjax = $.post("DCSDashboard.aspx", "op=LossBuckets_Occuring&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
         var vChart =null;
         tryRemoveOldChart(_Charts.Top5OccuringEvents.LossBuckets);
         tryRemoveOldChart(_Charts.Top5OccuringEvents.OccuringActualVsGoal);
         tryRemoveOldChart(_Charts.Top5OccuringEvents.Top5OccuringEvents);
         vChart=new Visifire('SL.Visifire.Charts.xap', _Charts.Top5OccuringEvents.LossBuckets.Name, width, height, "Transparent", true);

             
        vChart.setDataXml(data);
        vChart.preLoad = function(args) {
            var chart = args[0];
            if(chart.Series.length==0)return ;
            chart.Series[0].MouseLeftButtonUp = function(dataPoint) {
                _thirdReportConfig.startDate = startdate;
                _thirdReportConfig.endDate = enddate;
                _thirdReportConfig.level1 = dataPoint.AxisXLabel;
            };
            
        }
        
        vChart.loaded=function(args){
           try{
                var chart = args[0];
                if(chart.Series[0]==null || chart.Series[0].DataPoints==null || chart.Series[0].DataPoints.length==0)return ;
                _thirdReportConfig.startDate = startdate;
                _thirdReportConfig.endDate = enddate;
                _thirdReportConfig.level1 = "";//chart.Series[0].DataPoints[0].AxisXLabel;
                Top5OccuringEvents_Top5OccuringEvents(_dataRange.startDate(),_dataRange.endDate(), _thirdReportConfig.level1);
           }catch(ex)
           {
                alert(ex);
           }
         };

        vChart.render($('#TI_Top5OccuringEvents_LossBuckets .report').get(0));
        _Charts.Top5OccuringEvents.LossBuckets.Chart=vChart;
    });
}



function Top5OccuringEvents_Top5OccuringEvents(startdate, enddate, level1) {
    var width = 900, height = 300;

    if(this.getTop50OccurEventsTop50Ajax){
        this.getTop50OccurEventsTop50Ajax.abort();
        this.getTop50OccurEventsTop50Ajax = null;
    }

    this.getTop50OccurEventsTop50Ajax = $.post("DCSDashboard.aspx", "op=Top5OccuringEvents_Top5OccuringEvents&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
         var vChart =null;
         tryRemoveOldChart(_Charts.Top5OccuringEvents.Top5OccuringEvents);
        vChart=new Visifire('SL.Visifire.Charts.xap', _Charts.Top5OccuringEvents.Top5OccuringEvents.Name, width, height, "Transparent", true);
         
        vChart.setDataXml(data);
        vChart.preLoad = function(args) {
            var chart = args[0];
            if(chart.Series.length==0)return ;
            chart.Series[0].MouseLeftButtonUp = function(dataPoint) {
                _thirdReportConfig.levelid = parseInt(dataPoint.LegendText);
                _thirdReportConfig.level3 = dataPoint.AxisXLabel;
                _thirdReportConfig.refresh();
                Top5OccuringEvents_OccuringActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _thirdReportConfig.level1, _thirdReportConfig.goal(), _thirdReportConfig.type, _thirdReportConfig.levelid, _thirdReportConfig.level3);
                Top5OccuringEvents_Comments(_dataRange.startDate(),_dataRange.endDate(), _thirdReportConfig.level1, _thirdReportConfig.levelid, _thirdReportConfig.level3);
            };
        };
       
       vChart.loaded=function(args){
           try{
                if(args==null || args.length==0)return ;
                
                var chart = args[0];
                if(chart.Series[0]==null || chart.Series[0].DataPoints==null || chart.Series[0].DataPoints.length==0)return ;
                _thirdReportConfig.levelid = 0;//parseInt(chart.Series[0].DataPoints[0].LegendText);
                _thirdReportConfig.level3 = chart.Series[0].DataPoints[0].AxisXLabel;
                _thirdReportConfig.refresh();
                Top5OccuringEvents_OccuringActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _thirdReportConfig.level1, _thirdReportConfig.goal(), _thirdReportConfig.type, _thirdReportConfig.levelid, _thirdReportConfig.level3);
                Top5OccuringEvents_Comments(_dataRange.startDate(),_dataRange.endDate(), _thirdReportConfig.level1, _thirdReportConfig.levelid, _thirdReportConfig.level3);
            }catch(err)
            {
                alert(err);
            }
        };
        vChart.render($('#Top5OccuringEvents_Top5OccuringEvents .report').get(0));
        _Charts.Top5OccuringEvents.Top5OccuringEvents.Chart=vChart;
    });
}

function Top5OccuringEvents_OccuringActualVsGoal(startdate, enddate, level1, goal,type,levelid,level3) {
    var width = 900, height = 300;

    if(this.getTop5OccuringEvents_OccuringActualVsGoalAjax){
        this.getTop5OccuringEvents_OccuringActualVsGoalAjax.abort();
        this.getTop5OccuringEvents_OccuringActualVsGoalAjax = null;
    }

    this.getTop5OccuringEvents_OccuringActualVsGoalAjax = $.post("DCSDashboard.aspx", "op=Top5OccuringEvents_OccuringActualVsGoal&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&type=" + UrlEncode(type) + "&level1=" + UrlEncode(level1) + "&level3=" + UrlEncode(level3) + "&level3_id=" + levelid + "&goal=" + goal + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
        var vChart =null;
        tryRemoveOldChart(_Charts.Top5OccuringEvents.OccuringActualVsGoal);
        vChart=new Visifire('SL.Visifire.Charts.xap', _Charts.Top5OccuringEvents.OccuringActualVsGoal.Name, width, height, "Transparent", true);

        vChart.setDataXml(data);
        vChart.render($('#Top5OccuringEvents_OccuringActualVsGoal .report').get(0));
        _Charts.Top5OccuringEvents.OccuringActualVsGoal.Chart=vChart;
    });
}

var Top5OccuringEvents_Comments_TB = null;
function Top5OccuringEvents_Comments(startdate, enddate, level1, levelid, level3) {
    var tab = $('#Top5OccuringEvents_Comments table:first');
    if($.trim(level3).length>0){
        $(tab).find('caption').html('Comments for '+level3);
    }else if($.trim(level3).length==0 && $.trim(level1).length>0)
    {
        $(tab).find('caption').html('Comments for '+level1);
    }else{
        $(tab).find('caption').html('Comments');
    }

    if(this.getTop5OccuringEvents_CommentsAjax){
        this.getTop5OccuringEvents_CommentsAjax.abort();
        this.getTop5OccuringEvents_CommentsAjax = null;
    }
    
    this.getTop5OccuringEvents_CommentsAjax = $.post("DCSDashboard.aspx", "op=Top5OccuringEvents_Comments&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&level3=" + UrlEncode(level3) + "&level3_id=" + levelid + "&line=" + Line, function(data) {
        if (data) {
            if (Top5OccuringEvents_Comments_TB != null) {
                Top5OccuringEvents_Comments_TB.fnClearTable();
            }
            $.each($(data), function() {
                if(Top5OccuringEvents_Comments_TB ==null)
                {
                    var html = '';
                    if ($(tab).find("tr").length == 0) {
                        html = "<thead><tr bgcolor='#F1DEB8' height='30'>";
                        html += "<th width='100'>Data</th>";
                        html += "<th>Comment</th>";
                        html += "</tr></thead>";
                        $(tab).append(html);
                    }

                    html = "<tr height='22'>";
                    html += "<td>" + this.EventStartTimeStr + "</td>";
                    html += "<td>" + this.Comment + "</td>";
                    html += "</tr>";
                    $(tab).append(html);
                }else{
                    Top5OccuringEvents_Comments_TB.fnAddData([this.EventStartTimeStr, this.Comment]);
                }
            });


            var setting = {
                "fnRowCallback": function(nRow, aData, iDisplayIndex) {
                    return nRow;
                },
                "sPaginationType": "full_numbers",
                "bLengthChange": false,
                "bFilter": false,
                "bSort": false,
                "bInfo": false,
                "bAutoWidth": false,
                "iDisplayLength": 5,
                "bProcessing": false,
                "bStateSave": false,
                "bInitialised": false,
                "iDisplayStart":0,
                "aoColumns": [null,null],
            };
            if (Top5OccuringEvents_Comments_TB != null) {
                Top5OccuringEvents_Comments_TB.fnDraw(false);
            } else {
                Top5OccuringEvents_Comments_TB = $(tab).dataTable(setting);
            }
        }

    }, "json");
}

function HistoricalDetail_LossBuckets(startdate, enddate) {
    HistoricalDetail_OccurrenceHistory(startdate,enddate,-1);
    var width = 900, height = 300;

    if(this.getHistoricalDetail_LossBucketsAjax){
        this.getHistoricalDetail_LossBucketsAjax.abort();
        this.getHistoricalDetail_LossBucketsAjax = null;
    }

    this.getHistoricalDetail_LossBucketsAjax = $.post("DCSDashboard.aspx", "op=LossBuckets&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
        var vChart=null;
        tryRemoveOldChart(_Charts.HistoricalDetail.LossBuckets);
        tryRemoveOldChart(_Charts.HistoricalDetail.DowntimeActualVsGoal);
        tryRemoveOldChart(_Charts.HistoricalDetail.DowntimeHistory);
        tryRemoveOldChart(_Charts.HistoricalDetail.OccurrenceHistory);
        tryRemoveOldChart(_Charts.HistoricalDetail.Top5DowntimeEvents);
        tryRemoveOldChart(_Charts.HistoricalDetail.Top5OccuringEvents);
        vChart = new Visifire('SL.Visifire.Charts.xap',_Charts.HistoricalDetail.LossBuckets.Name , width, height,"Transparent",true);
        
        vChart.setDataXml(data);
        vChart.preLoad = function(args) {
            var chart = args[0];
            if(chart.Series.length==0)return ;
            chart.Series[0].MouseLeftButtonUp = function(dataPoint) {
                _fourReportConfig.startDate = startdate;
                _fourReportConfig.endDate = enddate;
                _fourReportConfig.level1 = dataPoint.AxisXLabel;
                loadTab4(startdate, enddate);
            };
        }
        
                
        vChart.loaded=function(args){
            var chart = args[0];
            if(chart.Series[0]==null || chart.Series[0].DataPoints==null || chart.Series[0].DataPoints.length==0)return ;
            
            _fourReportConfig.startDate = startdate;
            _fourReportConfig.endDate = enddate;
            _fourReportConfig.level1 = "";//chart.Series[0].DataPoints[0].AxisXLabel;
         };

        vChart.render($('#HistoricalDetail_LossBuckets .report').get(0));
        _Charts.HistoricalDetail.LossBuckets.Chart=vChart;
    });

    
    PFChart.createTimeLine(moment(enddate).subtract('days', 1), moment(enddate));

}

function HistoricalDetail_Top5DowntimeEvents(startdate, enddate,level1) {
    var width = 450, height = 250;

    if(this.getHistoricalDetail_Top5DowntimeEventsAjax){
        this.getHistoricalDetail_Top5DowntimeEventsAjax.abort();
        this.getHistoricalDetail_Top5DowntimeEventsAjax = null;
    }

    this.getHistoricalDetail_Top5DowntimeEventsAjax = $.post("DCSDashboard.aspx", "op=Top5DowntimeEvents&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
        var vChart=null;
        tryRemoveOldChart(_Charts.HistoricalDetail.Top5DowntimeEvents);
        vChart = new Visifire('SL.Visifire.Charts.xap',_Charts.HistoricalDetail.Top5DowntimeEvents.Name , width, height,"Transparent",true);
                
        vChart.setDataXml(data);
        
        vChart.preLoad = function(args) {
            var chart = args[0];
            if(chart.Series.length==0)return ;
            chart.Series[0].MouseLeftButtonUp = function(dataPoint) {
                if(dataPoint==null)return ;
                _fourReportConfig.startDate = startdate;
                _fourReportConfig.endDate = enddate;
                _fourReportConfig.level3 = dataPoint.AxisXLabel;
                _fourReportConfig.levelid=parseInt(dataPoint.LegendText);
                HistoricalDetail_DowntimeHistory(_dataRange.startDate(),_dataRange.endDate(),_fourReportConfig.levelid,"day");
                //HistoricalDetail_OccurrenceHistory(_dataRange.startDate(),_dataRange.endDate(),0);
            };
        }
        
        vChart.loaded=function(args){
            var chart = args[0];
            if(chart.Series[0]==null || chart.Series[0].DataPoints==null || chart.Series[0].DataPoints.length==0)return ;
            
            _fourReportConfig.startDate = startdate;
            _fourReportConfig.endDate = enddate;
            _fourReportConfig.level3 = "";//chart.Series[0].DataPoints[0].AxisXLabel;
            _fourReportConfig.levelid=0;//parseInt(chart.Series[0].DataPoints[0].LegendText);
            HistoricalDetail_DowntimeHistory(_dataRange.startDate(),_dataRange.endDate(),_fourReportConfig.levelid,"day");
         };
        
        vChart.render($('#HistoricalDetail_Top5DowntimeEvents .report').get(0));
        _Charts.HistoricalDetail.Top5DowntimeEvents.Chart=vChart;
    });
}


function HistoricalDetail_Top5OccuringEvents(startdate, enddate, level1) {
    var width = 450, height = 250;

    if(this.getHistoricalDetail_Top5OccuringEventsAjax){
        this.getHistoricalDetail_Top5OccuringEventsAjax.abort();
        this.getHistoricalDetail_Top5OccuringEventsAjax = null;
    }

    this.getHistoricalDetail_Top5OccuringEventsAjax = $.post("DCSDashboard.aspx", "op=Top5OccuringEvents&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
        var vChart=null;
        tryRemoveOldChart(_Charts.HistoricalDetail.Top5OccuringEvents);
        vChart = new Visifire('SL.Visifire.Charts.xap',_Charts.HistoricalDetail.Top5OccuringEvents.Name , width, height,"Transparent",true);

                
        vChart.setDataXml(data);
        
        vChart.preLoad = function(args) {
            var chart = args[0];
            if(chart.Series.length==0)return ;
            chart.Series[0].MouseLeftButtonUp = function(dataPoint) {
                if(dataPoint==null)return ;
                _fourReportConfig.startDate = startdate;
                _fourReportConfig.endDate = enddate;
                _fourReportConfig.level3_1 = dataPoint.AxisXLabel;
                _fourReportConfig.levelid_1=parseInt(dataPoint.LegendText);
                HistoricalDetail_OccurrenceHistory(_dataRange.startDate(),_dataRange.endDate(),_fourReportConfig.levelid_1,"day");
                //HistoricalDetail_OccurrenceHistory(_dataRange.startDate(),_dataRange.endDate(),0);
            };
        }
        
        vChart.loaded=function(args){
            var chart = args[0];
            if(chart.Series[0]==null || chart.Series[0].DataPoints==null || chart.Series[0].DataPoints.length==0)return ;
            
            _fourReportConfig.startDate = startdate;
            _fourReportConfig.endDate = enddate;
            _fourReportConfig.level3_1 = "";//chart.Series[0].DataPoints[0].AxisXLabel;
            _fourReportConfig.levelid_1=0;//parseInt(chart.Series[0].DataPoints[0].LegendText);
            HistoricalDetail_OccurrenceHistory(_dataRange.startDate(),_dataRange.endDate(),_fourReportConfig.levelid_1,"day");
         };
         
        vChart.render($('#HistoricalDetail_Top5OccuringEvents .report').get(0));
        _Charts.HistoricalDetail.Top5OccuringEvents.Chart=vChart;
    });
}


function HistoricalDetail_DowntimeActualVsGoal(startdate, enddate, level1,isEvents,goal, type) {
    var width = 900, height = 300;

    if(this.getHistoricalDetail_DowntimeActualVsGoalAjax){
        this.getHistoricalDetail_DowntimeActualVsGoalAjax.abort();
        this.getHistoricalDetail_DowntimeActualVsGoalAjax = null;
    }

    this.getHistoricalDetail_DowntimeActualVsGoalAjax = $.post("DCSDashboard.aspx", "op=DowntimeActualVsGoal&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&goal=" + goal + "&type=" + UrlEncode(type.toString()) + "&isEvents=" + isEvents + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
       var vChart=null;
       tryRemoveOldChart(_Charts.HistoricalDetail.DowntimeActualVsGoal);
       vChart = new Visifire('SL.Visifire.Charts.xap',_Charts.HistoricalDetail.DowntimeActualVsGoal.Name , width, height,"Transparent",true);

                
        vChart.setDataXml(data);
        vChart.render($('#HistoricalDetail_DowntimeActualVsGoal .report').get(0));
        _Charts.HistoricalDetail.DowntimeActualVsGoal.Chart=vChart;
    });
}

function HistoricalDetail_DowntimeHistory(startdate, enddate, levelid,type) {
    var width = 450, height = 400;

    if(this.getHistoricalDetail_DowntimeHistoryAjax){
        this.getHistoricalDetail_DowntimeHistoryAjax.abort();
        this.getHistoricalDetail_DowntimeHistoryAjax = null;
    }

    this.getHistoricalDetail_DowntimeHistoryAjax = $.post("DCSDashboard.aspx", "op=HistoricalDetail_DowntimeHistory&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate)+ "&level3=" + UrlEncode(_fourReportConfig.level3) + "&level3_id=" + levelid + "&width=" + width + "&height=" + height+"&type="+type + "&line=" + Line, function(data) {
      var vChart=null;
      tryRemoveOldChart(_Charts.HistoricalDetail.DowntimeHistory);
      vChart = new Visifire('SL.Visifire.Charts.xap',_Charts.HistoricalDetail.DowntimeHistory.Name , width, height,"Transparent",true);
       
        vChart.setDataXml(data);
        vChart.render($('#HistoricalDetail_DowntimeHistory .report').get(0));
        _Charts.HistoricalDetail.DowntimeHistory.Chart=vChart;
    });
}

function HistoricalDetail_OccurrenceHistory(startdate, enddate, levelid,type) {
    var width = 450, height = 400;

    if(this.getHistoricalDetail_OccurrenceHistoryAjax){
        this.getHistoricalDetail_OccurrenceHistoryAjax.abort();
        this.getHistoricalDetail_OccurrenceHistoryAjax = null;
    }

    this.getHistoricalDetail_OccurrenceHistoryAjax = $.post("DCSDashboard.aspx", "op=HistoricalDetail_OccurrenceHistory&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate)+ "&level3=" + UrlEncode(_fourReportConfig.level3_1) + "&level3_id=" + levelid + "&width=" + width + "&height=" + height+"&type="+type + "&line=" + Line, function(data) {
      var vChart=null;
      tryRemoveOldChart(_Charts.HistoricalDetail.OccurrenceHistory);
      vChart = new Visifire('SL.Visifire.Charts.xap',_Charts.HistoricalDetail.OccurrenceHistory.Name , width, height,"Transparent",true);

                
        vChart.setDataXml(data);
        vChart.render($('#HistoricalDetail_OccurrenceHistory .report').get(0));
        _Charts.HistoricalDetail.OccurrenceHistory.Chart=vChart;
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



function bindTabItemEvents() {
    $('#dock a[tabitem]').each(function(i, item) {
        $(item).click(function() {
            $('.TabItem').each(function() {
                if ($(this).attr('id') != $(item).attr('tabitem')) {
                    $(this).hide();
                } else {
                        var sd=_dataRange.startDate();
                        var ed=_dataRange.endDate();
                        $(this).show();
                    switch($(item).attr('tabitem')){
                        case "TI_Home":
                            {
                                currentTab = 0;
                            }
                            break;
                        case "TI_Top5DowntimeEvents":
                        {
                            currentTab = 1;
                        }
                            break;
                        case "TI_Top5OccuringEvents":
                        {
                            currentTab = 2;
                        }
                            break;
                        case "TI_HistoricalDetail":
                        {
                            currentTab = 3;
                        }
                            break;
                        case "TI_HiddenReasons":
                        {
                            currentTab = 4;
                        }
                            break;
                    }                    
                }

                initReports();
            });
            
           return false;
        });
        
        if (i == 0) $(this).click();//show home;
    });
}

function initDock() {

    //Transition you want :)
    var easing_type = 'easeOutBounce';

    //The default height for the dock (on mouse out)
    var default_dock_height = 82;
    var offset_top=20;

    //Expanded height, the height of the dock on mouse over, you have to set it in CSS
    var expanded_dock_height = $('#dock').height();

    //Fake body height
    var body_height = $(window).height() - default_dock_height-offset_top;

    //Set the size of #fake_body
    $('#scroll-body').height(body_height);
    $('#scroll-body').css('overflow-x','hidden').css('overflow-y','auto');
    //Set the CSS attribute for #dock
    $('#dock').css({ 'height': default_dock_height, 'position': 'absolute', 'top': body_height });

    //In case the user resize the browser, we will need to recalculate the height and top for #fake_body and #dock
    $(window).unbind('resize');
    $(window).resize(function() {

        //Grab the updated height/top
        updated_height = $(window).height() - default_dock_height-offset_top;

        //Set the updated height for #fake_body and top for #dock
        $('#scroll-body').height(updated_height);
        $('#dock').css({ 'top': updated_height });
    });

    //Recalculate default body height (always get the latest height), in case user has resized the window
    body_height = $(window).height() - default_dock_height-offset_top;

    //Animate the height change, set the height to default_dock-height and set the top value as well
    $('#dock').animate({ 'height': default_dock_height, 'top': body_height }, { queue: false, duration: 800, easing: easing_type });
}

function settingVoid() {
    $('#settingHref').click();
    /*
    $.fn.jmodal({
        initWidth: 500,
        docHeight: 300,
        title: 'Create New Event',
        content: '<fieldset align="center" style="width: 200px;float:left;"><legend>Downtimes</legend><label>Day:</label><input type="text" id="Goal_Day" /><br /><label>Week:</label><input type="text" id="Goal_Week" /><br /><label>Month:</label><input type="text" id="Goal_Month" /><br /><label>Year:</label><input type="text" id="Goal_Year" /></fieldset> <fieldset align="center" style="width: 200px;float:right;"><legend>Occurences</legend><label>Day:</label><input type="text" id="c_Goal_Day" /><br /><label>Week:</label><input type="text" id="c_Goal_Week" /><br /><label>Month:</label><input type="text" id="c_Goal_Month" /><br /><label>Year:</label><input type="text" id="c_Goal_Year" /></fieldset>  <input type="button" value="SET" id="Goal_SET" />',
        buttonText: 'Cancel',
        contentCss: {
            'height': 'auto',
            'background-color': '#F5FFFA',
            'padding': '20px 10px',
            'overflow': 'auto'
        },
        okEvent: function(e) { }
    });
    $('#Goal_Day').val(_firstReportConfig.day_goal);
    $('#Goal_Week').val(_firstReportConfig.week_goal);
    $('#Goal_Month').val(_firstReportConfig.month_goal);
    $('#Goal_Year').val(_firstReportConfig.year_goal);

    $('#c_Goal_Day').val(_firstReportConfig.c_day_goal);
    $('#c_Goal_Week').val(_firstReportConfig.c_week_goal);
    $('#c_Goal_Month').val(_firstReportConfig.c_month_goal);
    $('#c_Goal_Year').val(_firstReportConfig.c_year_goal);

    $('#Goal_SET').unbind('click').click(function() {
        var d = $('#Goal_Day').val(), w = $('#Goal_Week').val(), m = $('#Goal_Month').val(), y = $('#Goal_Year').val();
        var cd = $('#c_Goal_Day').val(), cw = $('#c_Goal_Week').val(), cm = $('#c_Goal_Month').val(), cy = $('#c_Goal_Year').val();
        if (isNaN(d) || $.trim(d).length == 0 || parseInt(d) <= 0) {
            alert('Goal of Downtime day must great than 0');
            $('#Goal_Day').get(0).select();
            $('#Goal_Day').get(0).focus();
            return false;
        }

        if (isNaN(w) || $.trim(w).length == 0 || parseInt(w) <= 0) {
            alert('Goal of Downtime week must great than 0');
            $('#Goal_Week').get(0).select();
            $('#Goal_Week').get(0).focus();
            return false;
        }

        if (isNaN(m) || $.trim(m).length == 0 || parseInt(m) <= 0) {
            alert('Goal of Downtime month must great than 0');
            $('#Goal_Month').get(0).select();
            $('#Goal_Month').get(0).focus();
            return false;
        }

        if (isNaN(y) || $.trim(y).length == 0 || parseInt(y) <= 0) {
            alert('Goal of Downtime year must great than 0');
            $('#Goal_Year').get(0).select();
            $('#Goal_Year').get(0).focus();
            return false;
        }



        if (isNaN(cd) || $.trim(cd).length == 0 || parseInt(cd) <= 0) {
            alert('Goal of Occurences day must great than 0');
            $('#c_Goal_Day').get(0).select();
            $('#c_Goal_Day').get(0).focus();
            return false;
        }

        if (isNaN(cw) || $.trim(cw).length == 0 || parseInt(cw) <= 0) {
            alert('Goal of Occurences week must great than 0');
            $('#c_Goal_Week').get(0).select();
            $('#c_Goal_Week').get(0).focus();
            return false;
        }

        if (isNaN(cm) || $.trim(cm).length == 0 || parseInt(cm) <= 0) {
            alert('Goal of Occurences month must great than 0');
            $('#c_Goal_Month').get(0).select();
            $('#c_Goal_Month').get(0).focus();
            return false;
        }

        if (isNaN(cy) || $.trim(cy).length == 0 || parseInt(cy) <= 0) {
            alert('Goal of Occurences year must great than 0');
            $('#c_Goal_Year').get(0).select();
            $('#c_Goal_Year').get(0).focus();
            return false;
        }

        _firstReportConfig.day_goal = parseInt(d);
        _firstReportConfig.week_goal = parseInt(w);
        _firstReportConfig.month_goal = parseInt(m);
        _firstReportConfig.year_goal = parseInt(y);

        _firstReportConfig.c_day_goal = parseInt(cd);
        _firstReportConfig.c_week_goal = parseInt(cw);
        _firstReportConfig.c_month_goal = parseInt(cm);
        _firstReportConfig.c_year_goal = parseInt(cy);

        $('#jmodal-bottom-okbutton').click();
        //update the report
        _secondReportConfig.refresh();
        _thirdReportConfig.refresh();
        _fourReportConfig.refresh();
        Top5DowntimeEvents_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _secondReportConfig.level1, _secondReportConfig.goal(), _secondReportConfig.type, _secondReportConfig.levelid, _secondReportConfig.level3);
        DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1, _firstReportConfig.isEvents, _firstReportConfig.goal(), _firstReportConfig.type);
        HistoricalDetail_DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _fourReportConfig.level1,_fourReportConfig.isEvents,_fourReportConfig.goal(),_fourReportConfig.type);
    });
    */
}


var redLinesTable;

function GetRedLines() {
    $('#setRedLines').unbind('click').click(function(){Add();});

    if(this.GetRedLinesAjax){
        this.GetRedLinesAjax.abort();
        this.GetRedLinesAjax = null;
    }

	this.GetRedLinesAjax = $.post("DCSDashboard.aspx", "op=RedLineList&line=" + Line, function(data) {
		if (data) {
			var tab = $('#redLineListTable');
			if(redLinesTable!=null)
			{
			    redLinesTable.fnClearTable();
			}else{
			    html = "<thead><tr bgcolor='#F1DEB8' height='30'>";
			    html = html + "<th width='180'>Start Time</th>";
			    html = html + "<th width='180'>End Time</th>";
			    html = html + "<th width='180'>Downtime</th>";
			    html = html + "<th width='180'>Occurences</th>";
			    html = html + "<th>Edit</th>";
			    html += "</tr></thead>";
			    $(tab).append(html);
			}
		
			$.each(data, function() {
			    
			    if(redLinesTable!=null)
			    {
			        redLinesTable.fnAddData([this.StartTimeStr, this.EndTimeStr,this.Downtime,this.Occuring,"<a href='#' onclick='Edit(this," + this.Id + ")'>Edit</a> <a href='#' onclick='Delete(" + this.Id + ")'>Delete</a>"]);
			    }else{
				    var html = "<tr height='22'>";

				    html = html + "<td>" + this.StartTimeStr + "</td>";
				    html = html + "<td>" + this.EndTimeStr + "</td>";
				    html = html + "<td>" + this.Downtime + "</td>";
                    html = html + "<td>" + this.Occuring + "</td>";

				    html = html + "<td><a href='#' onclick='Edit(this," + this.Id + ")'>Edit</a> <a href='#' onclick='Delete(" + this.Id + ")'>Delete</a></td>";


				    html += "</tr>";
				    $(tab).append(html);
				}
			});

            if(redLinesTable==null){
			    redLinesTable = $('#redLineListTable').dataTable({
				    "bPaginate":false,
				    "sPaginationType": "full_numbers",
				    "bLengthChange": false,
				    "bFilter": false,
				    "bSort": false,
				    "bInfo": false,
				    "bAutoWidth": false
			    });
			}else{
			    redLinesTable.fnDraw(false);
			}
		}

	}, "json");
}

function Edit(sender, id) {
	var startTime, endTime, Downtime,Occuring;

	startTime = $(sender).parent().parent().children("td:eq(0)").text();
	endTime = $(sender).parent().parent().children("td:eq(1)").text();
	Downtime = $(sender).parent().parent().children("td:eq(2)").text();
	Occuring = $(sender).parent().parent().children("td:eq(3)").text();
	
	$.fn.jmodal({
		initWidth: 400,
		title: 'Edit Goal',
		content: "<table style='width:30px;margin:0px auto;'><tr><td>StartTime:</td><td><input type='text' id='a_starttime' value='"+startTime+"' style='width:100px' /></td></tr><tr><td>EndTime:</td><td><input type='text' value='"+endTime+"' id='a_endtime'  style='width:100px' /></td></tr><tr><td>Downtime:</td><td><input type='text' value='"+Downtime+"' id='a_Downtime' style='width:100px'/></td></tr><tr><td>Occurences:</td><td><input type='text' value='"+Occuring+"' id='a_Occuring' style='width:100px'/>&nbsp;&nbsp;<input type='button'onclick='SaveEdit(" + id + ",this)' value='Save' /></td></tr></table>",
		buttonText: 'Cancel',
		okEvent: function(e) { }
	});
	
	$('#a_starttime').datepicker();
	$('#a_endtime').datepicker();

}

function CancelEdit(id,sender,success) {
	GetRedLines();
	$("#jmodal-bottom-okbutton").click();
}

function SaveEdit(id,sender) {
	var startTime, endTime, Downtime,Occuring;

	startTime = $("#a_starttime").val();
	endTime = $("#a_endtime").val();
	Downtime = $("#a_Downtime").val();
    Occuring= $("#a_Occuring").val();

	if ($.trim(startTime).length == 0) {
		$("#a_starttime").get(0).focus();
		alert('please input a date');
		return false;
	}

	if ($.trim(endTime).length == 0) {
		$("#a_endtime").get(0).focus();
		alert('please input a date');
		return false;
	}

	if (isNaN(Downtime) || $.trim(Downtime).length == 0) {
		$("#a_Downtime").get(0).focus();
		alert('please input a numeric');
		return false;
	}
	
	if (isNaN(Occuring) || $.trim(Occuring).length == 0) {
		$("#a_Occuring").get(0).focus();
		alert('please input a numeric');
		return false;
	}



	$.post("DCSDashboard.aspx", "op=updateredline&startDate=" + UrlEncode(startTime) + "&endDate=" + UrlEncode(endTime) + "&Downtime=" + Downtime+ "&Occuring=" + Occuring + "&id=" + id, function(data) {
		if (data == "1") {
			alert("Update successfully");
			CancelEdit(id,sender, true);
		} else {
			alert(data);
		}
	});
}

function Add() {
	$.fn.jmodal({
		initWidth: 400,
		title: 'Add New Goal',
		content: "<table style='width:100%;margin:0px auto;'><tr><td>StartTime:</td><td><input type='text' id='a_starttime' style='width:100px' /></td></tr><tr><td>EndTime:</td><td><input type='text' id='a_endtime'  style='width:100px' /></td></tr><tr><td>Downtime:</td><td><input type='text' id='a_Downtime' style='width:100px'/></td></tr><tr><td>Occurences:</td><td><input type='text' id='a_Occuring' style='width:100px'/>&nbsp;&nbsp;<input type='button'onclick='SaveAdd(this)' value='Save' /></td></tr></table>",
		buttonText: 'Cancel',
		okEvent: function(e) { }
	});
	
	$('#a_starttime').datepicker();
	$('#a_endtime').datepicker();
}

function SaveAdd(sender) {
	var startTime, endTime, Downtime,Occuring;

	startTime = $("#a_starttime").val();
	endTime = $("#a_endtime").val();
	Downtime = $("#a_Downtime").val();
    Occuring= $("#a_Occuring").val();

	if ($.trim(startTime).length == 0) {
		$("#a_starttime").get(0).focus();
		alert('please input a date');
		return false;
	}

	if ($.trim(endTime).length == 0) {
		$("#a_endtime").get(0).focus();
		alert('please input a date');
		return false;
	}

	if (isNaN(Downtime) || $.trim(Downtime).length == 0) {
		$("#a_Downtime").get(0).focus();
		alert('please input a numeric');
		return false;
	}
	
	if (isNaN(Occuring) || $.trim(Occuring).length == 0) {
		$("#a_Occuring").get(0).focus();
		alert('please input a numeric');
		return false;
	}


	$.post("DCSDashboard.aspx", "op=addredline&startDate=" + UrlEncode(startTime) + "&endDate=" + UrlEncode(endTime) + "&Downtime=" + Downtime+ "&Occuring=" + Occuring + "&line=" + Line, function(data) {
		if (data == "1") {
			alert("Insert successfully");
			$("#jmodal-bottom-okbutton").click();
			GetRedLines();
		} else {
			alert(data);
		}
	});
}

function CancelAdd(sender) {
	$("#jmodal-bottom-okbutton").click();
}

function Delete(id) {
	if (!confirm('Do you want to delete this record?')) {
		return;
	}
	
	$.post("DCSDashboard.aspx", "op=deleteredline&id=" + id + "&line=" + Line, function(data) {
		if (data == "1") {
			alert("Delete successfully");
			GetRedLines();
		} else {
			alert(data);
		}
	});
}

function Hidden_top5downtime(startdate, enddate, level1)
{
    var width = 450, height = 250;

    if(this.getHidden_top5downtimeAjax){
        this.getHidden_top5downtimeAjax.abort();
        this.getHidden_top5downtimeAjax = null;
    }

    this.getHidden_top5downtimeAjax = $.post("DCSDashboard.aspx", "op=Hidden_top5downtime&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {

             var vChart =null;
             tryRemoveOldChart(_Charts.HiddenReasons.Top5DowntmeEvents);
             var vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.HiddenReasons.Top5DowntmeEvents.Name, width, height, "Transparent", true);

                 
            vChart.setDataXml(data);
            vChart.render($('#TI_HiddenReasons_top5downtime .report').get(0));
            _Charts.HiddenReasons.Top5DowntmeEvents.Chart=vChart;

    });
}

function Hidden_top5occuring(startdate, enddate, level1)
{
    var width = 450, height = 250;

    if(this.getHidden_top5occuringAjax){
        this.getHidden_top5occuringAjax.abort();
        this.getHidden_top5occuringAjax = null;
    } 

    this.getHidden_top5occuringAjax= $.post("DCSDashboard.aspx", "op=Hidden_top5occuring&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {

             var vChart =null;
             tryRemoveOldChart(_Charts.HiddenReasons.Top5OccuringEvents);
             var vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.HiddenReasons.Top5OccuringEvents.Name, width, height, "Transparent", true);
                 
            vChart.setDataXml(data);
            vChart.render($('#TI_HiddenReasons_top5occuring .report').get(0));
            _Charts.HiddenReasons.Top5OccuringEvents.Chart=vChart;

    });
}

function Hidden_DowntimeActualVsGoal(startdate, enddate, level1,isEvents,goal, type) {
    var width = 450, height = 300;

    if(this.getHidden_DowntimeActualVsGoalAjax){
        this.getHidden_DowntimeActualVsGoalAjax.abort();
        this.getHidden_DowntimeActualVsGoalAjax = null;
    } 

    this.getHidden_DowntimeActualVsGoalAjax = $.post("DCSDashboard.aspx", "op=Hidden_DowntimeActualVsGoal&isHome=true&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1) + "&goal=" + goal + "&type=" + UrlEncode(type.toString()) + "&isEvents=" + isEvents + "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
             var vChart =null;
             tryRemoveOldChart(_Charts.HiddenReasons.ActualVs);
             var vChart = new Visifire('SL.Visifire.Charts.xap', _Charts.HiddenReasons.ActualVs.Name, width, height, "Transparent", true);
            
            vChart.setDataXml(data);
            vChart.render($('#TI_HiddenReasons_actualAs .report').get(0));
            _Charts.HiddenReasons.ActualVs.Chart=vChart;
    });
}

function Hidden_GetEventRows(startdate,enddate,level1)
{
    var tb=$('#hidden-events-grid');
     var width = 450, height = 300;

    if(this.getHidden_GetEventRowsAjax){
        this.getHidden_GetEventRowsAjax.abort();
        this.getHidden_GetEventRowsAjax = null;
    } 

    this.getHidden_GetEventRowsAjax = $.post("DCSDashboard.aspx", "op=Hidden_GetEventRows&startdate=" + UrlEncode(startdate) + "&enddate=" + UrlEncode(enddate) + "&level1=" + UrlEncode(level1)+ "&width=" + width + "&height=" + height + "&line=" + Line, function(data) {
       tb.find('tbody tr').remove();
       $(data).each(function(i,item){
            tb.find('tbody').append('<tr class="data">'+
                            '<td>'+item.Minutes+'</td>'+
                            '<td>'+item.Occurrences+'</td>'+
                            '<td style="text-align:left;">'+item.Path+'</td>'+
                      '</tr>');
       });
    },"json");
}