<!--TOOLBAR_EXEMPT-->
<%@ Page Language="C#"%>

<%
Response.RedirectLocation = Response.ApplyAppPathModifier("default.mspx");
Response.StatusCode = 301;
%>
