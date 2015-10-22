<%@ Page Language="C#" %>
<!DOCTYPE html>
<html>
<head>
	<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
	<title>Upload.cs</title>
</head>
<body>
<%=new SH.Upload().Post("")%>
<form method="post" action="?" enctype="multipart/form-data">
<input type="file" name="file" /><br />
<input type="submit" value="submit" />
</form>

</body>
</html>
