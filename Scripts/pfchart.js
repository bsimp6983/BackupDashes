(function () {

    var root = this;

    // var $ = root.$;

    var _ = root._;

    var Backbone = root.Backbone;

    var PFChart = root.PFChart = {};

    var init = PFChart.init = function (opts, callback) {
        var Line = PFChart.Line = 'company-demo';
        var Now = new Date();
        var StartTime = PFChart.StartTime = moment().subtract('days', 1);
        var EndTime = PFChart.EndTime = moment();
        var Group = PFChart.Group = 'hours';
        var Timeline = PFChart.Timeline = null;
        var CurrentTab = PFChart.CurrentTab = 0;
        var initBtns = true;

        if (!opts)
            opts = {
                initBtns: true,
                graph: true,
                timeline: true,
                line: Line
            };


        initBtns = opts.initBtns;

        Line = PFChart.Line = opts.line;

        if (Backbone) {

            //Line = 'Comfort_Bath_1A'.toLowerCase();

            var Downtime = Backbone.Model.extend({
            });

            var DowntimeCollection = Backbone.Collection.extend({
                model: Downtime,
                url: 'Service.ashx'
            });

            var CaseCount = Backbone.Model.extend({
                defaults: {
                    Id: 0,
                    EventStart: '',
                    EventStop: '',
                    Client: 'sage',
                    Line: Line,
                    CaseCount: 0
                }
            });

            var CaseCountCollection = Backbone.Collection.extend({
                model: CaseCount,
                url: 'Service.ashx'
                /*
                comparator: function (item) {
                return new Date(item.get("EventStop"));
                }
                */
            });

            var Line = Backbone.Model.extend({

            });

            var LineCollection = Backbone.Collection.extend({
                model: Line,
                url: 'Service.ashx'
            });

            var Lines = new LineCollection;


            var Downtimes = new DowntimeCollection;
            var TimeLineDownTimes = new DowntimeCollection;

            var CaseCounts = new CaseCountCollection;

            var Charts = PFChart.Charts = [];

            if (initBtns == true) {
                $('.group').change(function () {
                    var value = $(this).val();

                    PFChart.Group = value;

                    updatePerformanceGraph(true);

                });

                $('#slLines').change(function () {
                    PFChart.Line = $(this).val();
                });


                $('#txtStartDate').datepicker();
                $('#txtEndDate').datepicker();

                $('#txtPerfStartDate').datepicker();
                $('#txtPerfEndDate').datepicker();

                $('#txtStartDate').val(moment(PFChart.StartTime).format('M/D/YYYY'));
                $('#txtEndDate').val(moment(PFChart.EndTime).format('M/D/YYYY'));

                $('#txtPerfStartDate').val(moment(PFChart.StartTime).format('M/D/YYYY'));
                $('#txtPerfEndDate').val(moment(PFChart.EndTime).format('M/D/YYYY'));

                $('#btnEnter').click(function () {
                    PFChart.StartTime = moment($('#txtStartDate').val());
                    PFChart.EndTime = moment($('#txtEndDate').val());

                    updatePerformanceGraph(true, function () {
                        //createTimeLine();
                    });

                });

                $('#btnPerfEnter').click(function () {
                    var sd = moment($('#txtPerfStartDate').val());
                    var ed = moment($('#txtPerfEndDate').val());

                    var tmpDate = ed.clone();

                    if (tmpDate.subtract('days', 7).toDate() <= sd.toDate())
                        createTimeLine(sd, ed);
                    else
                        alert('You can only view 7 days at a time');
                });
            }

            var getGroupStartDate = function (group) {
                var startDate;
                switch (group.toLowerCase()) {

                    case "hours":
                        {
                            startDate = PFChart.EndTime.subtract('hours', 48);
                        }
                        break;
                    case "days":
                        {
                            startDate = PFChart.EndTime.subtract('days', 7);
                        }
                        break;
                    case "weeks":
                        {
                            startDate = PFChart.EndTime.subtract('weeks', 52);
                        }
                        break;
                    case "months":
                        {
                            startDate = PFChart.EndTime.subtract('months', 36);
                        }
                        break;
                    case "years":
                        {
                            startDate = PFChart.StartDate;
                        }
                        break;
                }

                return startDate;

            }

            var filterByGroup = PFChart.filterByGroup = function (group, callback) {
                group = group || PFChart.Group;

                if (group && CaseCounts.length > 0 && Downtimes.count > 0) {

                    Downtimes = new DowntimeCollection(Downtimes.filter(function (downtime) {
                        return dateIsGreaterThan(startDate, downtime.get('EventStart'));
                    }));

                    CaseCounts = new CaseCountCollection(CaseCounts.filter(function (casecount) {
                        return dateIsGreaterThan(startDate, casecount.get('EventStop'));
                    }));

                    if (callback)
                        callback();
                }

            }

            var getDowntimes = PFChart.getDowntimes = function (fetch, options) {

                options = options || { callback: null };

                var callback = options.callback;
                var group = options.group || PFChart.Group;

                var st = options.startdate || PFChart.StartTime;
                var et = options.enddate || PFChart.EndTime;

                st = moment(st);
                et = moment(et);

                if (fetch) {
                    Downtimes.fetch({
                        data: {
                            op: 'downtime',
                            sd: moment(PFChart.StartTime).format('M/D/YYYY H:m:s a'),
                            ed: moment(PFChart.EndTime).format('M/D/YYYY H:m:s a'),
                            line: PFChart.Line,
                            group: group
                        },
                        success: function () {
                            if (callback)
                                callback(Downtimes);
                        }
                    });
                } else {

                    if (callback)
                        callback(Downtimes);
                }

                return Downtimes;
            }

            var createTimeLine = PFChart.createTimeLine = function (startDate, endDate) {

                var timeLineData = [];

                var style = 'height:100px;' +
                            'background-color: red;';

                var lastEventStop;
                $('#lblMessage').text('Loading...');

                TimeLineDownTimes.fetch({
                    data: {
                        op: 'downtime',
                        sd: moment(startDate).format('M/D/YYYY H:m:s a'),
                        ed: moment(endDate).format('M/D/YYYY H:m:s a'),
                        line: PFChart.Line,
                        group: 'timeline'
                    },
                    success: function () {
                        TimeLineDownTimes.each(function (downtime) {
                            var actual = '<div class="bar downtime" style="' + style + '" ' +
                                        ' title="' + (downtime.get('ReasonCode') || '') + '">' + downtime.get('ReasonCode') || '' + ' </div>';

                            if (lastEventStop) {

                                timeLineData.push({
                                    'group': '',
                                    'start': lastEventStop,
                                    'end': new Date(downtime.get('EventStart')),
                                    'content': '<div class="bar" style="height: 100px; background-color: green;"> </div>'
                                });

                            }

                            timeLineData.push({
                                'group': '',
                                'start': new Date(downtime.get('EventStart')),
                                'end': new Date(downtime.get('EventStop')),
                                'content': actual
                            });

                            lastEventStop = new Date(downtime.get('EventStop'));
                        });

                        var options = {
                            "width": "100%",
                            "height": "200px",
                            "style": "box" // optional
                        };

                        if (PFChart.Timeline == null || PFChart.Timeline == undefined)
                            PFChart.Timeline = new links.Timeline(document.getElementById('timeline'));
                        else
                            PFChart.Timeline.deleteAllItems();

                        // Draw our timeline with the created data and options
                        PFChart.Timeline.draw(timeLineData, options);
                        $('.downtime').qtip({
                            content: {
                                text: false // Use each elements title attribute
                            },
                            position: {
                                corner: {
                                    target: 'topMiddle',
                                    tooltip: 'center'
                                }
                            },
                            style: 'cream' // Give it some style
                        });

                        $('#lblMessage').text('');

                    }
                });

            }

            var getCaseCounts = PFChart.getCaseCount = function (fetch, callback) {
                if (fetch) {

                    CaseCounts.fetch({
                        data: {
                            op: 'casecount',
                            sd: moment(PFChart.StartTime).format('M/D/YYYY H:m:s a'),
                            ed: moment(PFChart.EndTime).format('M/D/YYYY H:m:s a'),
                            line: PFChart.Line,
                            group: PFChart.Group
                        },
                        success: function () {
                            if (callback)
                                callback(CaseCounts);
                        }
                    });

                } else {
                    if (callback)
                        callback(CaseCounts);

                    return CaseCounts;
                }


            }

            var getLines = PFChart.getLines = function (callback) {

                Lines.fetch({
                    data: {
                        op: 'lines'
                    },
                    success: function () {
                        if (callback)
                            callback(Lines);
                    }
                });

                return Lines;
            }

            var populateLineSelect = PFChart.populateLineSelect = function (callback) {
                $('#slLines option').empty();

                getLines(function (Lines) {
                    var counter = 0;
                    Lines.each(function (line) {

                        $('#slLines').append('<option value="' + line.get('Line') + '" >' + line.get('Line') + '</option>');

                        counter++;

                        if (counter == Lines.length - 1) {
                            if (callback)
                                callback();
                        }

                    });

                });

            }

            var getTemplateAjax = PFChart.getTemplateAjax = function (path, callback) {
                var source;
                var template;

                $.ajax({
                    url: path,
                    cache: true,
                    success: function (data) {
                        source = data;
                        template = Handlebars.compile(source);

                        if (callback)
                            callback(template);

                        return template;
                    }
                });
            }

            var getStackedTemplate = PFChart.getStackedTemplate = function (callback) {
                getTemplateAjax('Templates/Stacked.html', callback);
            }

            var dateIsGreaterThan = function (sd, ed) {
                sd = moment(sd);
                ed = moment(ed);

                if (sd.toDate() > ed.toDate())
                    return true;
                else
                    return false;

                /*
                if (sd.year() > ed.year())
                return true;
                else if (sd.year() > ed.year() && sd.month() > ed.month())
                return true;
                else if (sd.year() > ed.year() && sd.month() > ed.month() && sd.date() > ed.date())
                return true;
                else if (sd.year() > ed.year() && sd.month() > ed.month() && sd.date() > ed.date() && sd.hours() > ed.hours())
                return true;
                else if (sd.year() > ed.year() && sd.month() > ed.month() && sd.date() > ed.date() && sd.hours() > ed.hours() && sd.minutes() > ed.minutes())
                return true;
                else if (sd.year() > ed.year() && sd.month() > ed.month() && sd.date() > ed.date() && sd.hours() > ed.hours() && sd.minutes() > ed.minutes() && sd.seconds() > ed.seconds())
                return true;
                else
                return false;
                */

            }

            var totalDowntimeForHour = function (start, stop, callback) {

                var total = 0;

                start = moment(start);
                stop = moment(stop);

                if (start.toDate() > stop.toDate()) {
                    var tmp = start;

                    start = stop;
                    stop = tmp;

                }

                Downtimes.each(function (dt) {

                    var s = moment(dt.get('EventStart'));
                    var e = moment(dt.get('EventStop'));

                    if (dateIsGreaterThan(s, start) && !dateIsGreaterThan(s, stop)) {

                        var t = parseFloat(dt.get('Minutes'));

                        if (t < 0)
                            t *= -1;

                        if (dateIsGreaterThan(e, stop)) {
                            var diff = e.diff(stop, 'minutes');

                            if (diff < 0)
                                diff *= -1;

                            //console.log(e.format('M/D/YYYY H:mm:s a') + ' ' + stop.format('M/D/YYYY H:mm:s a'));

                            t -= diff;

                        }

                        total += t;

                    }
                    else if (dateIsGreaterThan(e, start) && !dateIsGreaterThan(e, stop)) {
                        var diff = e.diff(start, 'minutes');

                        if (diff < 0)
                            diff *= -1;

                        var t = (parseFloat(dt.get('Minutes')) - diff);

                        if (t < 0)
                            t *= -1;

                        if (dateIsGreaterThan(e, stop)) {

                            diff = e.diff(stop, 'minutes');

                            if (diff < 0)
                                diff *= -1;

                            t -= diff;
                        }

                        total += t;
                        //console.log(t);

                    }
                });

                return total;
            }

            var getPerformance = PFChart.getPerformance = function (ccArray, callback) {
                if (ccArray.length > 0) {

                    var grpThroughCases = _.groupBy(ccArray, function (item) {
                        return item.get('Throughput');
                    }); //Group cases by Throughput

                    var lastCC;

                    var results = [];

                    // console.log(grpThroughCases);
                    var date = null;
                    _.each(grpThroughCases, function (caseArray) {

                        var result = {
                            uptime: 0,
                            downtime: 0,
                            performance: 0,
                            hours: 0
                        };

                        var CaseCounts = new CaseCountCollection(caseArray);

                        var firstCC = CaseCounts.first();
                        var lastCC = CaseCounts.last();

                        var throughput = parseFloat(lastCC.get('Throughput')); //All cases in this group share throughput

                        if (throughput == 0)
                            throughput++;

                        var ST = moment(lastCC.get('EventStop'));
                        var ED = moment(firstCC.get('EventStop'));

                        if (date == null)
                            date = ED.toDate();

                        //console.log(CaseCounts);

                        var totalMinutes = ST.diff(ED, 'minutes');
                        var totalHours = ST.diff(ED, 'hours');

                        /*
                        Need to break it down to the minute. If change over is at 9:20, then use the amount of minutes of that case count for the hour. 
                        Also need to calculate the weight of each Throughput. If one is happening 90% of the day, it needs to be bigger than the 10%. Can't average the total out. 


                        */

                        if (totalHours < 0)
                            totalHours *= -1;

                        if (totalMinutes < 0)
                            totalMinutes *= -1;


                        if (totalHours == 0)
                            totalHours = 1;

                        if (totalMinutes == 0)
                            totalMinutes = 1;


                        var totalThroughput = (totalMinutes / 5) * ((throughput * totalHours) / 12)//Total minutes / 5 * Throughput every 5mins

                        var totalCases = lastCC.get('CaseCount') - firstCC.get('CaseCount');

                        if (totalCases < 0)
                            totalCases *= -1;

                        var totalEst = lastCC.get('Throughput');

                        var value = 0;

                        var totalDowntime = totalDowntimeForHour(firstCC.get('EventStop'), lastCC.get('EventStop'));

                        var dtCases = (totalThroughput / totalMinutes) * totalDowntime;


                        var caseLoss = totalThroughput - (totalCases + dtCases);


                        var uptimePercent = totalCases / (totalCases + dtCases + caseLoss);


                        var downtimePercent = dtCases / (totalCases + dtCases + caseLoss);


                        var totalPerformanceLoss = caseLoss / (totalCases + dtCases + caseLoss);

                        /*
                        console.log('Total Minutes: ' + totalMinutes);
                        console.log('Total Hours: ' + totalHours);
                        console.log('Calc: (' + totalMinutes + ' / 5 * ((' + throughput * totalHours + ') / 12)');
                        console.log('Throughput: ' + throughput);
                        console.log('Total Throughput: ' + totalThroughput);
                        console.log('Dt Case: ' + dtCases);
                        console.log('Total downtime: ' + totalDowntime);
                        console.log('Total Cases: ' + totalCases);
                        console.log('Case Loss Performance: ' + caseLoss);
                        console.log('Uptime Percent: ' + uptimePercent);
                        console.log('Downtime Percent: ' + downtimePercent);
                        console.log('Performance: ' + totalPerformanceLoss);
                        */

                        result.uptime = uptimePercent;
                        result.downtime = downtimePercent;
                        result.performance = totalPerformanceLoss;
                        result.hours = totalHours;

                        //console.log(result);

                        results.push(result);


                    });

                    var totalHours = 0;

                    var totalResult = {
                        uptime: 0,
                        downtime: 0,
                        performance: 0,
                        date: date
                    }

                    //console.log(results);

                    _.each(results, function (result) {//Total all hours

                        totalHours += result.hours;

                    });

                    _.each(results, function (result) {

                        var percentOfTotal = result.hours / totalHours;

                        totalResult.uptime += result.uptime * percentOfTotal;
                        totalResult.downtime += result.downtime * percentOfTotal;
                        totalResult.performance += result.performance * percentOfTotal;

                    });

                    totalResult.uptime = totalResult.uptime * 100;
                    totalResult.downtime = totalResult.downtime * 100;
                    totalResult.performance = totalResult.performance * 100;


                    if (callback)
                        callback(totalResult);

                    return totalResult;
                }

                return 0;


            }


            var updatePerformanceGraph = PFChart.updatePerformanceGraph = function (fetch, callback) {

                // fetch = false; //Because I can't get updated data yet

                var vChart;
                var id = 'perfChart';

                if (Charts[id] == undefined) {
                    vChart = Charts[id] = new Visifire('SL.Visifire.Charts.xap', 'performanceChart', 900, 800, "Transparent", true);
                }
                else {
                    vChart = Charts[id];
                }

                $('#lblMessage').text('Loading...');
                getCaseCounts(fetch, function (CaseCounts) {

                    if (CaseCounts) {
                        if (CaseCounts.length == 0)
                            $('#lblMessage').text('');

                        if (CaseCounts.length > 0) {

                            getDowntimes(fetch,
						        {

						            startdate: CaseCounts.first().get('EventStop'),
						            enddate: CaseCounts.last().get('EventStop'),
						            callback: function (Downtimes) {
						                getStackedTemplate(function (template) {

						                    //var data = template(//convertJsonToVisifire(createStackedGraphObject());

						                    var obj = createStackedGraphObject();

						                    var group = PFChart.Group;

						                    obj.title += PFChart.Line;

						                    //var ST = moment(PFChart.StartTime).clone();
						                    //var ET = moment(PFChart.EndTime).clone();
						                    //var diff = ET.diff(ST, group);

						                    //console.log('Group: ' + group);

						                    //console.log('Diff: ' + diff);

						                    //Group by date based on group

						                    var ccArray = CaseCounts.groupBy(function (item) {
						                        var d = moment(item.get('EventStop'));
						                        return d.format('M/DD/YYYY H');
						                        /*
						                        switch (group) {
						                        case 'hours':
						                        return d.format('M/DD/YYYY H');
						                        /*
						                        case 'weeks':
						                        return d.format('w');
						                        case 'months':
						                        return d.format('YYYY M');
						                        case 'years':
						                        return d.format('YYYY');
						                        }
						                        */

						                    });

						                    var calculatedValues = [];

						                    //console.log(ccArray);

						                    _.each(ccArray, function (recs) {

						                        /*
						                        var ccArray = CaseCounts.filter(function (item) {
						                        var d = moment(item.get('EventStop'));

						                        switch (group) {
						                        case 'hours':
						                        return (d.date() == ST.date() && d.year() == ST.year() && d.month() == ST.month() && d.hours() == ST.hours());
						                        case 'days':
						                        return (d.date() == ST.date() && d.year() == ST.year() && d.month() == ST.month());
						                        case 'weeks':
						                        return (d.year() == ST.year() && d.month() == ST.month() && d.format('w') == ST.format('w'));
						                        case 'months':
						                        return (d.year() == ST.year() && d.month() == ST.month());
						                        case 'years':
						                        return (d.year() == ST.year());
						                        }

						                        });
						                        */

						                        var value = getPerformance(recs);

						                        calculatedValues.push(value);

						                        /*
						                        if (recs.length > 0) {

						                        //series == stacked data
						                        var text = '';

						                        var ST = recs[0];

						                        ST = moment(ST.get('EventStop'));

						                        if (group == 'hours') {
						                        text = ST.format('hh:00 a');
						                        obj.xTitle = 'Hours';

						                        }
						                        else if (group == 'days') {
						                        text = ST.format('M/DD/YYYY');
						                        obj.xTitle = 'Days';
						                        }
						                        else if (group == 'months') {
						                        text = ST.format('M/YYYY');
						                        obj.xTitle = 'Months';
						                        }
						                        else if (group == 'weeks') {
						                        text = ST.format('w');
						                        obj.xTitle = 'Weeks';
						                        }
						                        else if (group == 'years') {
						                        text = ST.format('YYYY');
						                        obj.xTitle = 'Years';

						                        console.log(value);
						                        }

						                        obj.series[0].datapoints.push({
						                        label: text,
						                        value: value.uptime,
						                        color: 'Green'
						                        });

						                        obj.series[1].datapoints.push({
						                        label: text,
						                        value: value.downtime,
						                        color: 'Red'
						                        });

						                        obj.series[2].datapoints.push({
						                        label: text,
						                        value: value.performance,
						                        color: 'Blue'
						                        });

						                        }

						                        */

						                    });

						                    var results = _.groupBy(calculatedValues, function (item) {
						                        var et = item['date'];

						                        if (et) {
						                            var d = moment(et);

						                            switch (group) {
						                                case 'hours':
						                                    return d.format('M/DD/YYYY H');
						                                case 'days':
						                                    return d.format('M/DD/YYYY');
						                                case 'weeks':
						                                    return d.format('w');
						                                case 'months':
						                                    return d.format('YYYY M');
						                                case 'years':
						                                    return d.format('YYYY');
						                            }
						                        }

						                    });

						                    _.forEach(results, function (value) {

						                        var text = '';

						                        var first = _.first(value);

						                        var ST = moment(first['date']);
						                        var Uptime = 0;
						                        var Downtime = 0;
						                        var Performance = 0;

						                        _.forEach(value, function (v) {
						                            Uptime += v.uptime;
						                            Downtime += v.downtime;
						                            Performance += v.performance;
						                        });

						                        Uptime = Uptime / value.length;
						                        Downtime = Downtime / value.length;
						                        Performance = Performance / value.length;


						                        if (group == 'hours') {
						                            text = ST.format('hh:00 a');
						                            obj.xTitle = 'Hours';

						                        }
						                        else if (group == 'days') {
						                            text = ST.format('M/DD/YYYY');
						                            obj.xTitle = 'Days';
						                        }
						                        else if (group == 'months') {
						                            text = ST.format('M/YYYY');
						                            obj.xTitle = 'Months';
						                        }
						                        else if (group == 'weeks') {
						                            text = ST.format('w');
						                            obj.xTitle = 'Weeks';
						                        }
						                        else if (group == 'years') {
						                            text = ST.format('YYYY');
						                            obj.xTitle = 'Years';

						                        }

						                        obj.series[0].datapoints.push({
						                            label: text,
						                            value: Uptime,
						                            color: 'Green'
						                        });

						                        obj.series[1].datapoints.push({
						                            label: text,
						                            value: Downtime,
						                            color: 'Red'
						                        });

						                        obj.series[2].datapoints.push({
						                            label: text,
						                            value: Performance,
						                            color: 'Blue'
						                        });

						                    });

						                    var data = template(obj);

						                    vChart.setDataXml(data);

						                    vChart.render(id);

						                    if (callback)
						                        callback();

						                    $('#lblMessage').text('');

						                });
						            }
						        });
                        }
                    }


                });


            }

            var createStackedGraphObject = function () {

                var data = {
                    title: 'Performance ',
                    xTitle: 'Hour',
                    yTitle: 'Performance',
                    width: 900,
                    height: 800,
                    prefix: '',
                    scale: '1,%',
                    interval: '5',
                    yAxisMax: 100,
                    yAxisMin: 0,
                    series: [
                        {
                            text: 'Uptime',
                            datapoints: []
                        },
                        {
                            text: 'Downtime',
                            datapoints: []
                        },
                        {
                            text: 'Performance',
                            datapoints: []
                        }
                    ]
                };

                return data;

            }

            var stackedGraphTemplate = '<vc:Chart  xmlns:vc="clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts" Width="{{width}}" Height="{{height}}"'
                    + ' Theme="Theme1" BorderBrush="Gray" AnimatedUpdate="true" CornerRadius="7" ShadowEnabled="true" '
                    + ' Padding="6,8,8,10"> '
                    + '<vc:Chart.Titles> '
                    + '    <vc:Title Text="{{title}}"/> '
                    + '</vc:Chart.Titles> '
                    + '<vc:Chart.AxesX> '
                    + '    <vc:Axis Title="{{xTitle}}" /> '
                    + '</vc:Chart.AxesX> '
                    + '<vc:Chart.AxesY> '
                    + '    <vc:Axis Title="{{yTitle}}" Prefix="{{prefix}}" ScalingSet="{{scale}}" Interval="{{interval}}"/> '
                    + '</vc:Chart.AxesY> '
                    + '<vc:Chart.PlotArea> '
                    + '    <vc:PlotArea ShadowEnabled="false"/> '
                    + '</vc:Chart.PlotArea> '
                    + '<vc:Chart.Legends> '
                      + '  <vc:Legend BorderColor="#dbf2f2" BorderThickness="0.5" CornerRadius="2"> '
                         + '   <vc:Legend.Background> '
                            + '    <LinearGradientBrush EndPoint="1,1" StartPoint="0,1"> '
                               + '     <GradientStop Color="#f9f8f8" Offset="0.1"/> '
                                  + '  <GradientStop Color="#f1fafa" Offset="0.4"/> '
                                  + '  <GradientStop Color="#fcfefe" Offset="1"/> '
                             + '   </LinearGradientBrush> '
                         + '   </vc:Legend.Background> '
                      + '  </vc:Legend> '
                    + ' </vc:Chart.Legends> '
                    + '<vc:Chart.Series> '
                       + ' {{#each series}} '
                         + '   <vc:DataSeries LegendText="{{text}}" RenderAs="StackedColumn" > '
                            + '    <vc:DataSeries.DataPoints> '
                                 + '   {{#each datapoints}} '
                                 + '       <vc:DataPoint AxisXLabel="{{label}}" YValue="{{value}}"/> '
                                 + '   {{/each}} '
                               + ' </vc:DataSeries.DataPoints>  '
                         + '   </vc:DataSeries>  '
                     + '   {{/each}} '
                  + '  </vc:Chart.Series> '
               + ' </vc:Chart>';

            var convertJsonToVisifire = PFChart.convertJsonToVisifireStacked = function (json) {
                var template = Handlebars.compile(stackedGraphTemplate);

                var html = template(json);

                return html;
            }

            populateLineSelect(function () {
                $('#slLines').change();

                updatePerformanceGraph(false);
            });

            updatePerformanceGraph(true, callback);
        }

    }

}).call(this);