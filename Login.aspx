<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="DowntimeCollection_Demo.Login" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Downtime Collection Solutions &raquo; Contact</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link rel='index' title='Downtime Collection Solutions' href='http://legacy.downtimecollectionsolutions.com/' />
    <style type="text/css" media="print">#wpadminbar { display:none; }</style>
    <style type="text/css">
	    html { margin-top: 28px !important; }
	    * html body { margin-top: 28px !important; }
    </style>
<!-- Google Analytics Tracking by Google Analyticator 6.1.3: http://ronaldheft.com/code/analyticator/ -->
<script type="text/javascript">
	var analyticsFileTypes = [''];
	var analyticsEventTracking = 'enabled';
</script>
<script type="text/javascript">
	var _gaq = _gaq || [];
	_gaq.push(['_setAccount', 'UA-5441212-2']);
	_gaq.push(['_trackPageview']);

	(function() {
		var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
		ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
		var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
	})();

</script>
    <link rel="stylesheet" href="http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/style.css" type="text/css" media="screen" />
	
	<!-- for lightbox for Screenshots -->
	<link rel="stylesheet" href="http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/css/lightbox.css" type="text/css" media="screen" />
	<script src="http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/js/lightbox.js" type="text/javascript"></script>
	<script src="http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/js/prototype.js" type="text/javascript"></script>
	<script src="http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/js/scriptaculous.js?load=effects,builder" type="text/javascript"></script>
	
	<!-- for popup "Try it Free!" button -->

	<script src="http://jqueryjs.googlecode.com/files/jquery-1.2.6.min.js" type="text/javascript"></script>
	<script src="http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/js/popup.js" type="text/javascript"></script>
    <script type="text/javascript">
        function createNewAccount(formContainer) {
            var $ = jQuery;
            var username = $(formContainer + ' form input[name="username"]').val();
            var password = $(formContainer + ' form input[name="password"]').val();
            var confirmpassword = $(formContainer + ' form input[name="confirmpassword"]').val();
            var email = $(formContainer + ' form input[name="email"]').val();
            var name = $(formContainer + ' form input[name="name"]').val();
            var phone = $(formContainer + ' form input[name="phone"]').val();

            if ($.trim(username) == '') {
                $(formContainer + ' form input[name="username"]').addClass('error').get(0).focus();
                return false;
            } else {
                $(formContainer + ' form input[name="username"]').removeClass('error');
            }

            if ($.trim(password) == '') {
                $(formContainer + ' form input[name="password"]').addClass('error').get(0).focus();
                return false;
            } else {
                $(formContainer + ' form input[name="password"]').removeClass('error');
            }

            if ($.trim(name) == '') {
                $(formContainer + ' form input[name="name"]').addClass('error').get(0).focus();
                return false;
            } else {
                $(formContainer + ' form input[name="name"]').removeClass('error');
            }

            if ($.trim(email) == '') {
                $(formContainer + ' form input[name="email"]').addClass('error').get(0).focus();
                return false;
            } else {
                $(formContainer + ' form input[name="email"]').removeClass('error');
            }

            $.ajax({
                url: "http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/remote-reg.php",
                async: true,
                data: {
                    op: 'reg-from-wp',
                    username: username,
                    password: password,
                    email: email,
                    phone: phone,
                    name: name
                },
                dataType: "html",
                error: function(e) {
                    alert("error:" + e);
                },
                success: function(result) {
                    if (result.toLowerCase() == 'true') {
                        location.href = 'http://legacy.downtimecollectionsolutions.com/index.php/tour/37-revision-2/';
                        $('.jqiclose').click();
                    } else {
                        alert(result);
                    }
                }
            });
        }
    </script>
</head>
<body>
<div id="top_container">
	<div class="top">
    	<a href="http://legacy.downtimecollectionsolutions.com" title="Downtime Collection Solutions"><img src="http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/images/logo.png" alt="Downtime Collection Solutions" class="logo" /></a>
    </div>
</div>
<div id="header_container2">
	<div class="bg2">
    </div>
</div>
    <div id="main">
	<div class="container">         
											                                                        
                                                                                    
                                                                                    
                                                        
                                                                    <div id="post-10" class="post-10 page type-page status-publish hentry">

                                        
																				<h2 class="entry-title"><a href="Login.aspx" >Login</a></h2>
										                                        
                                        
                                                                                                                <div class="entry-content">
                                            <p><img src="http://legacy.downtimecollectionsolutions.com/wp-content/uploads/2011/05/downtime.png" alt="downtime" title="downtime" width="399" height="379" class="alignright pushup" />
											
											<form id="form1" runat="server">
												<div id="loginspacer">
												
													<asp:Login ID="Login1" runat="server" LoggedIn="LoggedIn"></asp:Login>
                                                    <asp:Label ID="lblError" runat="server" ForeColor="Red"></asp:Label>
												</div>
												</form>
											
											
											</p>
                                                                                    </div><!-- .entry-content -->
                                                            
   
                                    </div><!-- #post-## -->
                            
                                                        
                                                                		    </div>
</div><div id="footer_container">
	<div class="content">
    	<a href="http://legacy.downtimecollectionsolutions.com" title="Downtime Collection Solutions"><img src="http://legacy.downtimecollectionsolutions.com/wp-content/themes/DowntimeCollectionSolutions/images/footer_logo.png" class="logo" alt="Downtime Collection Solutions" /></a>        
        
		<div id="footernav">
			&copy;2011 Downtime Collection Solutions | (567) 686-1040
		</div>

        <a href="http://www.infostreamusa.com" target="_blank" class="design">Website Design Toledo by InfoStream Solutions<img src="http://legacy.downtimecollectionsolutions.com/web-design-images/web-design-toledo-infostream-logo.png" alt="Web Design Toledo" /></a>
    </div>
</div>
</body>
</html>