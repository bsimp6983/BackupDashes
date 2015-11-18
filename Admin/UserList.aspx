<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserList.aspx.cs" Inherits="DowntimeCollection_Demo.Admin.UserList" %>


<%@ Register src="../CnmPager.ascx" tagname="CnmPager" tagprefix="uc1" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="../plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <link href="../Styles/demo_table.css" rel="Stylesheet" type="text/css" />
    <link href="../Styles/demo_page.css" rel="Stylesheet" type="text/css" />
    <script src="../scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="../plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    <script src="../scripts/jquery.jmodal.js" type="text/javascript"></script>
    <script src="../Scripts/jquery-ui-timepicker-addon.js" type="text/javascript"></script>
    <script src="../Scripts/jquery.dataTables.js" type="text/javascript"></script>
    <script language="javascript">
            /* Custom filtering function which will filter data in column four between two values */

        $(document).ready(function () {
            var setting = {
                "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                    return nRow;
                },
                "sPaginationType": "full_numbers",
                "bLengthChange": true,
                "bFilter": true,
                "bSort": true,
                "bInfo": false,
                "bAutoWidth": false,
                "iDisplayLength": 100,
                "bProcessing": false,
                "bStateSave": false,
                "bInitialised": false,
                "iDisplayStart": 0,
            };

            $('.datepicker').datetimepicker();
            var oTable = $("#GridView1").prepend($("<thead></thead>").append($(this).find("tr:first"))).dataTable(setting);
            $('#GridView1').css('width', '100%');

           // $('#filterTxt').keypress( function(){ console.log('hi'); oTable.fnDraw(); } );
        });

        function changePassword(userName) {
            $.fn.jmodal({
                initWidth: 400,
                title: 'Change Password',
                content: "<table style='width:30px;margin:0px auto;'><tr><td>New Password:</td><td><input type='password' id='password' style='width:100px' /></td></tr><tr><td>ConfirmPassword:</td><td><input type='password' id='repassword'  style='width:100px' /></td></tr><tr><td><input type='button'onclick='savePassword('" + userName + "')' value='Save' /></td></tr></table>",
                buttonText: 'Cancel',
                okEvent: function (e) { }
            });
        }

        function savePassword(userName) {
            if ($('#password').val() != $('#repassword').val() || $.trim($('#repassword').val()).length == 0) {
                alert("two password not same.");
                return false;
            }
            $.post("UserList.aspx", { op: "changePassword", password: $('#repassword').val() }, function (result) {
                if (result == 'true') {
                    $("#jmodal-bottom-okbutton").click();
                } else {
                    alert('changing password error.');
                }
            });
        }
    </script>
</head>
<body>
<div><a href="TvDemoControl.aspx" style="float: left;">Tv Demo Control</a><br /></div>
    <form id="form1" runat="server">
        <div>
            <!-- <asp:TextBox ID="filterTxt" runat="server"></asp:TextBox> -->

            <h1>
                User List</h1>
            <asp:GridView ID="GridView1" runat="server" DataSource='<%# Users %>' AutoGenerateColumns="False"
                OnRowCommand="GridView1_RowCommand" EnableModelValidation="True">
                <Columns>
                    <asp:TemplateField HeaderText="User Name" SortExpression="UserName">
                        <ItemTemplate>
                            <asp:Label ID="UserNameLabel" runat="server" Text='<%# Bind("UserName") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:Label ID="UserNameLabel" runat="server" Text='<%# Bind("UserName") %>'></asp:Label>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Creation Date">
                        <ItemTemplate>
                            <asp:Label ID="CreationDateLabel" runat="server" Text='<%# Eval("CreationDate",@"{0:MM/dd/yyyy HH:mm}") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:Label ID="CreationDateLabel" runat="server" Text='<%# Eval("CreationDate",@"{0:MM/dd/yyyy HH:mm}") %>'></asp:Label>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Expire Date">
                        <ItemTemplate>
                            <asp:Label ID="ExpireDateLabel" runat="server" Text='<%# Eval("Expire Date",@"{0:MM/dd/yyyy HH:mm}") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="ExpireDateTextBox" CssClass="datepicker" runat="server" Text='<%# Eval("Expire Date",@"{0:MM/dd/yyyy HH:mm}") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Last Login Date">
                        <ItemTemplate>
                            <asp:Label ID="LastLoginDateLabel" runat="server" Text='<%# Eval("Last Login",@"{0:MM/dd/yyyy HH:mm}") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:Label ID="LastLoginTextBox" runat="server" Text='<%# Eval("Last Login",@"{0:MM/dd/yyyy HH:mm}") %>'></asp:Label>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Shift Start">
                        <ItemTemplate>
                            <asp:Label ID="ShiftStartLabel" runat="server" Text='<%# Eval("ShiftStart",@"{0:HH:mm}") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="ShiftStartTextBox" CssClass="datepicker" runat="server" Text='<%# Eval("ShiftStart",@"{0:HH:mm}") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Name">
                        <ItemTemplate>
                            <asp:Label ID="NameLabel" runat="server" Text='<%# Eval("Name") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="NameTextBox" runat="server" Text='<%# Eval("Name") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Phone">
                        <ItemTemplate>
                            <asp:Label ID="PhoneLabel" runat="server" Text='<%# Eval("Phone") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="PhoneTextBox" runat="server" Text='<%# Eval("Phone") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Email">
                        <ItemTemplate>
                            <asp:HyperLink ID="EmailLink" runat="server" 
                                NavigateUrl='<%# Eval("Email", "mailto:{0}") %>' Target="_blank" 
                                Text='<%# Eval("Email") %>'></asp:HyperLink>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="EmailTextBox" runat="server" Text='<%# Eval("Email") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Password">
                        <ItemTemplate>
                             <%# Eval("Password") %>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="PasswordTextBox" runat="server" Text='<%# Eval("Password") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Admin Password">
                        <ItemTemplate>
                             <%# Eval("AdminPassword") %>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="AdminPasswordTextBox" runat="server" Text='<%# Eval("AdminPassword") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="IsApproved">
                        <ItemTemplate>
                            <%#Eval("IsApproved")%>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:CheckBox id="CheckBoxIsApproved" Checked='<%# Eval("IsApproved") %>'  runat="server"></asp:CheckBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Eff Chart Enabled">
                        <ItemTemplate>
                            <%#Eval("Eff Chart Enabled")%>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:CheckBox id="EffChartEnabled" Checked='<%# Eval("Eff Chart Enabled") %>'  runat="server"></asp:CheckBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Estimated Output">
                        <ItemTemplate>
                            <%#Eval("EstimatedOutput")%>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox id="EstimatedOutputTextBox" Text='<%# Eval("EstimatedOutput") %>'  runat="server"></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="DeleteButton" runat="server" CommandArgument='<%# Eval("UserName") %>'
                                Visible='<%# "admin" != ((string)Eval("UserName")).ToLower() %>'
                                OnClientClick='<%#  Eval("UserName", "return confirm(\"are you sure you want to delete this user?\\n{0}\");") %>'
                                CommandName="delete user"  Text="Delete"/>

                                 &nbsp;<asp:Button ID="EditButton" runat="server" 
                                CommandArgument='<%# Eval("UserName") %>' CommandName="edit user" 
                                Visible='<%# "admin" != ((string)Eval("UserName")).ToLower() %>' 
                                Text="Edit" />
                            <asp:HyperLink ID="ModifPasswordLink" runat="server" NavigateUrl="javascript:changePassword('admin');" Visible="false">Change Passowrd</asp:HyperLink>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:Button ID="SaveButton" runat="server" CommandArgument='<%# Eval("UserName") %>'
                               CommandName="save"  Text="Save"/>

                                 &nbsp;<asp:Button ID="CancelButton" runat="server" CommandArgument='<%# Eval("UserName") %>'
                               CommandName="cn_cancel"  Text="Cancel"/>
                        </EditItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <br />
        </div>
    </form>
</body>
</html>
