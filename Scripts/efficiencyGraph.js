(function () {

    var root = this;

    // var $ = root.$;

    var _ = root._;

    var Backbone = root.Backbone;

    var EGraph = root.EGraph = {};

    var Line = EGraph.Line = 'Splenda1-1';
    var CycleLine = EGraph.CycleLine = 'Splenda1';
    var Now = new Date();
    var StartTime = EGraph.StartTime = moment().subtract('days', 1);
    var EndTime = EGraph.EndTime = moment();
    var Group = EGraph.Group = 'hours';
    var ScheduledMinutes = EGraph.ScheduledMinutes = 0;

    var Charts = EGraph.Charts = [];


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

    var CaseCounts = new CaseCountCollection;
    var CycleCounts = new CaseCountCollection;

    var init = EGraph.init = function (eleId) {

        eleId = eleId || 'effGraph';

        if (Backbone && Handlebars) {
            $('#startDate').val(EGraph.StartTime.format('MM/DD/YYYY'));
            $('#endDate').val(EGraph.EndTime.format('MM/DD/YYYY'));
            $('#startDate').datetimepicker();
            $('#endDate').datetimepicker();

            $('#btnEnter').click(function(){
                EGraph.StartTime = moment($('#startDate').val());
                EGraph.EndTime = moment($('#endDate').val());

                draw(eleId);
            });

            draw(eleId);

        }
    }

    var draw = EGraph.draw = function(eleId){
        getCaseCounts(true, function(caseCounts){
            getCycleCounts(true, function(cycleCounts){
                calculateCases(caseCounts, cycleCounts, function(calculatedCases){
                    updateEfficiencyGraph(eleId, calculatedCases);
                });
            })
        })
    }

    var calculateCases = EGraph.calculateCases = function(CaseCounts, CycleCounts, callback){
        if(CaseCounts && CycleCounts){

            var results = {
                waste: 0,
                efficiency: 0,
                downtime: 0
            };

            if(CaseCounts.length > 0 && CycleCounts.length > 0){

                getTotalScheduledMinutes(function(scheduledMinutes){

                    var cyclesPerMinute = 72;
                    var machineSpeed = 3100;

                    var firstCaseCount = _.first(CaseCounts) || CaseCounts.first();
                    var lastCaseCount = _.last(CaseCounts) || CaseCounts.last();

                    var firstCycleCount = _.first(CycleCounts) || CycleCounts.first();
                    var lastCycleCount = _.last(CycleCounts) || CycleCounts.last();

                    var sd = firstCaseCount['EventStart'] || firstCaseCount.get('EventStart');

                    sd = moment(sd);

                    var ed = lastCaseCount['EventStop'] || lastCaseCount.get('EventStop');

                    ed = moment(ed);

                    var diffMin = EGraph.EndTime.diff(EGraph.StartTime, 'minutes');//ed.diff(sd, 'minutes');

                    var minutes =  diffMin - scheduledMinutes;

                    var theoriticalCases = 3.3 * minutes;

                    console.log('Total Minutes = ' + diffMin + ' - ' + scheduledMinutes);

                    $('#minutes').text(minutes);
                    $('#theoCases').text(theoriticalCases);
                    $('#machineSpeed').text(machineSpeed);

                    var firstCount = firstCaseCount['CaseCount'] || firstCaseCount.get('CaseCount');
                    var lastCount = lastCaseCount['CaseCount'] || lastCaseCount.get('CaseCount');

                    var actualCases = lastCount - firstCount;

                    $('#actCases').text(actualCases);

                    console.log('Actual Cases [' + actualCases + ']: ' + lastCount + ' - ' + firstCount);

                    var startCycle = firstCycleCount['CaseCount'] || firstCycleCount.get('CaseCount');
                    var stopCycle = lastCycleCount['CaseCount'] || lastCycleCount.get('CaseCount');

                    if(stopCycle == startCycle)
                        stopCycle++;

                    $('#startCycle').text(startCycle);
                    $('#endCycle').text(stopCycle);

                    var waste = ((stopCycle-startCycle) * cyclesPerMinute)-(actualCases * 1000);

                    $('#waste').text(waste);

                    console.log('Waste [' + waste + '] : ((' + stopCycle + '-' + startCycle + ') * ' + cyclesPerMinute + ') - (' + actualCases + '* 1000)');

                    var efficiency = actualCases/theoriticalCases;


                    console.log('Efficiency [' + efficiency + '] : ' + actualCases + '/' + theoriticalCases);


                    var wastePercent = (waste/((stopCycle-startCycle)*cyclesPerMinute));


                    console.log('Waste% [' + wastePercent + '] : (' + waste + '/' + stopCycle + '-' + startCycle + ')*' + cyclesPerMinute);

                    var downtime = 1-efficiency-wastePercent;


                    var theorDowntimeMinutes = minutes * downtime;

                    $('#theorDowntimeMinutes').text(theorDowntimeMinutes.toFixed(2));

                    wastePercent = (wastePercent * 100).toFixed(2);
                    efficiency = (efficiency * 100).toFixed(2);
                    downtime = (downtime * 100).toFixed(2);

                    $('#wastePercent').text(wastePercent + '%');
                    $('#efficiency').text(efficiency + '%');
                    $('#downtime').text(downtime + '%');

                    if(!isFinite(wastePercent))
                        console.log(wastePercent);

                    results = {
                        waste: wastePercent,
                        efficiency: efficiency,
                        downtime: downtime
                    };

                    if(callback)
                        callback(results);

                });

            }

            return results;
        }
    }

    var getCycleCounts = EGraph.getCycleCounts = function (fetch, callback) {
        if (fetch) {
            CycleCounts.fetch({
                data: {
                    op: 'casecount',
                    sd: moment(EGraph.StartTime).format('M/D/YYYY H:m:s a'),
                    ed: moment(EGraph.EndTime).format('M/D/YYYY H:m:s a'),
                    line: EGraph.CycleLine
                    //group: EGraph.Group
                },
                success: function () {
                    if(callback)
                        callback(CycleCounts);
                }
            });

        } else {
            if (callback)
                callback(CycleCounts);

            return CycleCounts;
        }
    }

    var getCaseCounts = EGraph.getCaseCount = function (fetch, callback) {
        if (fetch) {
            CaseCounts.fetch({
                data: {
                    op: 'casecount',
                    sd: moment(EGraph.StartTime).format('M/D/YYYY H:m:s a'),
                    ed: moment(EGraph.EndTime).format('M/D/YYYY H:m:s a'),
                    line: EGraph.Line
                    //group: EGraph.Group
                },
                success: function () {
                    if(callback)
                        callback(CaseCounts);

                    console.log(CaseCounts);
                }
            });

        } else {
            if (callback)
                callback(CaseCounts);
            return CaseCounts;
        }
    }

    var getTotalScheduledMinutes = EGraph.getTotalSecheduledMinutes = function(callback){
        $.get('Service.ashx', {
            op: 'scheduledminutes',
            sd: moment(EGraph.StartTime).format('M/D/YYYY H:m:s a'),
            ed: moment(EGraph.EndTime).format('M/D/YYYY H:m:s a'),
            line: EGraph.CycleLine
        }, function(data){
           if(!isNaN(data)){
               EGraph.ScheduledMinutes = data;


               if(callback){
                   callback(data);
               }

           }
        });
    }

    var getTemplateAjax = EGraph.getTemplateAjax = function (path, callback) {
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

    var getBarTemplate = EGraph.getBarTemplate = function (callback) {
        getTemplateAjax('Templates/Bar.html', callback);
    }

    var createBarGraphObject = function () {

        var data = {
            title: 'Efficiency ',
            xTitle: 'Hour',
            yTitle: 'Percent',
            width: 900,
            height: 800,
            prefix: '',
            scale: '1,%',
            interval: '5',
            yAxisMax: 100,
            yAxisMin: 0,
            series: [
                {
                    text: 'Efficiency',
                    datapoints: []
                },
                {
                    text: 'Waste %',
                    datapoints: []
                },
                {
                    text: 'Downtime',
                    datapoints: []
                }
            ]
        };

        return data;

    }

    var updateEfficiencyGraph = EGraph.updateEfficiencyGraph = function(id, counts){

        var vChart;

        if(id && counts){
            getBarTemplate(function(template){

                if (EGraph.Charts[id] == undefined) {
                    vChart = EGraph.Charts[id] = new Visifire('SL.Visifire.Charts.xap', 'efficiencyGraph', 900, 800, "Transparent", true);
                }
                else {
                    vChart = EGraph.Charts[id];
                }

                var xmlCounts = createBarGraphObject();

                var text = 'Splenda1';

                xmlCounts.series[0].datapoints.push({
                    label: text,
                    value: counts.efficiency,
                    color: 'Green'
                });

                xmlCounts.series[1].datapoints.push({
                    label: text,
                    value: counts.waste,
                    color: 'Blue'
                });

                xmlCounts.series[2].datapoints.push({
                    label: text,
                    value: counts.downtime,
                    color: 'Red'
                });

                var data = template(xmlCounts);

                vChart.setDataXml(data);

                vChart.render(id);

            });

        }
    }


}).call(this);