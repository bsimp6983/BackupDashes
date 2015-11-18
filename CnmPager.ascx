<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CnmPager.ascx.cs" Inherits="DowntimeCollection_Demo.CnmPager" %>

<div class="pager">
    records:<asp:Literal ID="Literal1" Text='<%# ItemCount %>' runat="server"></asp:Literal>,
    page:
    <asp:Literal ID="PageIndexLiteral" Text='<%# PageIndex + 1 %>' runat="server"></asp:Literal>
    /
    <asp:Literal ID="PageCountLiteral" Text='<%# PageCount %>' runat="server"></asp:Literal>,<asp:HyperLink
        ID="First" NavigateUrl='<%# string.Format(UrlFormat, 0) %>' Enabled='<%# PageIndex > 0 %>'
        runat="server">First</asp:HyperLink>
    <asp:HyperLink ID="Prev" NavigateUrl='<%# string.Format(UrlFormat, Math.Min(PageIndex, PageCount) - 1) %>'
        Enabled='<%# PageIndex > 0 %>' runat="server">Prev</asp:HyperLink>
    <asp:HyperLink ID="Next" NavigateUrl='<%# string.Format(UrlFormat, PageIndex + 1) %>'
        Enabled='<%# PageIndex < PageCount - 1 %>' runat="server">Next</asp:HyperLink>
    <asp:HyperLink ID="Last" NavigateUrl='<%# string.Format(UrlFormat, PageCount - 1) %>'
        Enabled='<%# PageIndex < PageCount - 1 %>' runat="server">Last</asp:HyperLink>
</div>
