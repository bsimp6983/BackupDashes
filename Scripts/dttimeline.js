(function () {

    var root = this;

    // var $ = root.$;

    var _ = root._;

    var Backbone = root.Backbone;

    var DTTimeline = root.DTTimeline = {};

    var Downtime = Backbone.Model.extend({
    });

    var DowntimeCollection = Backbone.Collection.extend({
        model: Downtime,
        url: 'Service.ashx'
    });

    var TimeLineDownTimes = new DowntimeCollection;

    var init = DTTimeline.init = function (Downtimes, element) {
        var Line = DTTimeline.Line = 'company-demo';
        var Now = new Date();
        var StartTime = DTTimeline.StartTime = moment().subtract('days', 1);
        var EndTime = DTTimeline.EndTime = moment();
        var Group = DTTimeline.Group = 'hours';

        if (Backbone) {

            createTimeLine(StartTime, EndTime);
        }

    }

    var createTimeLine = DTTimeline.createTimeLine = function (startDate, endDate) {

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
                line: 'Comfort_Bath_1A',
                group: 'timeline'
            },
            success: function () {
                TimeLineDownTimes.each(function (downtime) {
                    var actual = '<div class="bar downtime" style="' + style + '" ' +
                                        ' title="' + (downtime.get('ReasonCode') || '') + '">' + downtime.get('ReasonCode') || '' + ' </div>';

                    if (lastEventStop) {

                        timeLineData.push({
                            'group': 'Downtime Events',
                            'start': lastEventStop,
                            'end': new Date(downtime.get('EventStart')),
                            'content': '<div class="bar" style="height: 100px; background-color: green;"> </div>',
                            'id': downtime.get('Id')
                        });

                    }

                    timeLineData.push({
                        'group': 'Downtime Events',
                        'start': new Date(downtime.get('EventStart')),
                        'end': new Date(downtime.get('EventStop')),
                        'content': actual,
                        'id': downtime.get('Id')
                    });

                    lastEventStop = new Date(downtime.get('EventStop'));
                });

                function getSelectedRow() {
                    var row = undefined;
                    var sel = DTTimeline.Timeline.getSelection();
                    if (sel.length) {
                        if (sel[0].row != undefined) {
                            row = sel[0].row;
                        }
                    }
                    return row;
                }

                var options = {
                    "width": "100%",
                    "height": "200px",
                    "editable": true,
                    "style": "box" // optional
                };

                var onEdit = function (event) {
                    var row = getSelectedRow();

                    /*
                    document.getElementById("info").innerHTML += "item " + row + " edit<br>";
                    var content = data.getValue(row, 2);
                    var newContent = prompt("Enter content", content);
                    if (newContent != undefined) {
                    data.setValue(row, 2, newContent);
                    }
                    */

                    var splitAmount = prompt('Split Amount', 0);

                    if (!isNaN(splitAmount)) {
                        var events = timeLineData;

                        if (events[row] && splitAmount > 0) {
                            var event = events[row];

                            var start = moment(event['start']);
                            var end = moment(event['end']);

                            var minutes = end.diff(start, 'minutes');
                            var days = end.diff(start, 'days');

                            var amount = 0;

                            if (days == 0 && minutes > 0) {
                                amount = minutes / splitAmount;
                            }
                            else if (days > 0) {
                                amount = days / splitAmount;
                            }

                            if (amount > 0) {

                                var original = _.clone(event);

                                if (days == 0) {

                                    var amounts = amount.toString().split('.');

                                    var min = parseInt(amounts[0]);
                                    var sec = parseInt(amounts[1]);

                                    if (sec > 0) {
                                        sec = (sec * 0.1) * 60;
                                    }

                                    var moStart = moment(start);
                                    var minutes = moStart.minutes();
                                    var seconds = moStart.seconds();

                                    minutes += min;
                                    seconds += sec;

                                    var end = new Date(event['start']);

                                    end = new Date(end.setMinutes(48));
                                    // end = new Date(end.setSeconds(seconds));



                                    event['end'] = end;

                                    console.log(minutes);
                                    console.log(event);
                                    console.log(original);

                                }
                                else if (days > 0) {
                                    event['end'] = moment(start).add('days', amount).toDate();
                                }

                                timeLineData[row] = event;

                                var lastEvent = event;

                                for (var x = 1; x < splitAmount; x++) {
                                    var currentEvent = _.clone(lastEvent);

                                    currentEvent['start'] = currentEvent['end'];


                                    if (days == 0) {
                                        currentEvent['end'] = moment(currentEvent['start']).add('minutes', amount).toDate();
                                    }
                                    else if (days > 0) {
                                        currentEvent['end'] = moment(currentEvent['start']).add('days', amount).toDate();
                                    }

                                    lastEvent = currentEvent;
                                }
                            }

                        }
                    }

                    DTTimeline.Timeline.redraw();
                };

                if (DTTimeline.Timeline == null || DTTimeline.Timeline == undefined) {

                    DTTimeline.Timeline = new links.Timeline(document.getElementById('timeline'));
                }
                else
                    DTTimeline.Timeline.deleteAllItems();

                links.events.addListener(DTTimeline.Timeline, 'edit', onEdit);

                // Draw our timeline with the created data and options
                DTTimeline.Timeline.draw(timeLineData, options);

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

}).call(this);