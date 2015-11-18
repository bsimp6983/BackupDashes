<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DigestEmails.aspx.cs" Inherits="DowntimeCollection_Demo.DigestEmails1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="//netdna.bootstrapcdn.com/twitter-bootstrap/2.3.1/css/bootstrap-combined.min.css" rel="stylesheet" />
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <link href="Styles/select2.css" rel="Stylesheet" type="text/css" />

    <!--[if IE 7]>
    <link type="text/css" href="css/jquery.ui.1.10.0.ie.css" rel="stylesheet" />
    <![endif]-->

    <!-- Le HTML5 shim, for IE6-8 support of HTML5 elements -->
    <!--[if lt IE 9]>
    <script src="http://html5shim.googlecode.com/svn/trunk/html5.js"></script>
    <![endif]-->

    <title></title>
</head>
<body ng-app="DigestEmails" ng-controller="ctrlBase">
    <p><span class="label label-important">In Beta</span></p>

    <button class="btn" ng-click="addEmail()">Add Email</button>
    
    <button class="btn" ng-click="save()">Save</button>

    <button class="btn" ng-click="sendEmail()" style="float: right;">Send Out Emails</button>

    <table class="table">
        <thead>
            <tr>
                <th> 
                    Email
                </th>
                <th>
                    Lines
                </th>
                <th>
                    Daily
                </th>
                <th>
                    Weekly
                </th>
                <th>
                
                </th>
            </tr>
        </thead>
        <tbody>
            <tr ng-repeat="email in emails">
                <td>
                    <input type="email" ng-model="email.Email" />
                </td>
                <td>
                    <select ng-model="email.Lines" multiple="multiple" style="width: 150px" ui-select2="">
                        <option ng-repeat="line in lines" value="{{line}}">{{line}}</option>                        
                    </select>
                </td>
                <td>
                    <input type="checkbox" ng-model="email.IsDaily" />
                </td>
                <td>
                    <input type="checkbox" ng-model="email.IsWeekly" />
                </td>
                <td>
                    <button ng-click="delete(email)" class="btn">Delete Email</button>
                </td>
            </tr>
        </tbody>
    </table>
    
    <script src="//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js"></script>
    <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.2/jquery-ui.min.js"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.0.6/angular.min.js"></script>
    <script src="Scripts/angular-ui.js"></script>   
    <script src="Scripts/angular-resource.min.js"></script>
    <script src="Scripts/digestemails.js"></script>
    <script src="Scripts/select2.min.js"></script>

</body>
</html>
