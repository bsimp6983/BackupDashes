<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Timeline.aspx.cs" Inherits="DowntimeCollection_Demo.Timeline" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Styles/demo_table.css" rel="stylesheet" type="text/css" />
    <link href="Styles/demo_page.css" rel="stylesheet" type="text/css" />
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <link href="Scripts/timeline.css" rel="Stylesheet" type="text/css" />

    <script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="Scripts/underscore-min.js" type="text/javascript"></script>
    <script src="Scripts/backbone-min.js" type="text/javascript"></script>
    <script src="Scripts/handlebars.js" type="text/javascript"></script>
    <script src="Scripts/Visifire.js" type="text/javascript"></script>
    <script src="Scripts/moment.min.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    <script src="Scripts/timeline.js" type="text/javascript"></script>
    <script src="Scripts/jquery.qtip-1.0.0-rc3.min.js" type="text/javascript"></script>
    <script src="Scripts/dttimeline.js" type="text/javascript"></script>
    
    <style type="text/css">
        body {font: 10pt arial;}

        div.timeline-event {
            border: none;
            background: none;
            border-radius: 0;
        }
        div.timeline-event-selected {
            border: none;
            background: none;
        }
        div.timeline-event-content {
            margin: 0;
        }
        div.timeline-event-range {
            border: none;
            border-radius: 0;
            height: 100px;
            width: 100%;
            position: relative;
            overflow: visible;
        }
        div.bar {
            position: absolute;
            bottom: 0;
            left: 0;
            width: 100%;
            text-align: center;
            color: white;
            /* height and color is set for each individual bar */
        }
        div.requirement {
            position: absolute;
            bottom: 0;
            left: 0;
            width: 100%;
            border-top: 2px solid gray;
            background: #e5e5e5;
            opacity: 0.5;
        }
    </style>
    <script type="text/javascript">

        $(document).ready(function () {
            DTTimeline.init();

        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="timeline">
    
    </div>
    </form>
</body>
</html>
