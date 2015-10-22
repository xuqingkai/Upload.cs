using System;
namespace SH
{
	/// <summary>
	/// 上传文件
	/// </summary>
	public class Upload
	{
		/// <summary>
		/// 保存目录
		/// </summary>
		private string _dir = "/Upload";
		/// <summary>
		/// 设置保存目录
		/// </summary>
		/// <param name="dir">目录名，默认/Upload</param>
		/// <returns></returns>
		public Upload Dir(object dir) { _dir = dir + ""; return this; }

		/// <summary>
		/// 允许的扩展名
		/// </summary>
		private string _ext = "swf,flv,mp3,mp4,gif,jpg,png,bmp,doc,docx,xls,xlsx,ppt,pptx,zip,rar";
		/// <summary>
		/// 允许的扩展名
		/// </summary>
		/// <param name="ext">文件扩展名，如：swf,flv,mp3,wmv,gif,jpg,png,bmp,doc,docx,xls,xlsx,ppt,pptx,zip,rar</param>
		/// <returns></returns>
		public Upload Ext(string ext) { _ext = ext; return this; }

		/// <summary>
		/// 上传大小限制
		/// </summary>
		private int _max = 0;
		/// <summary>
		/// 设置上传大小限制,单位KB
		/// </summary>
		/// <param name="max">单位KB</param>
		/// <returns></returns>
		public Upload Max(int max) { _max = max; return this; }

		/// <summary>
		/// 生成文件名
		/// </summary>
		private string _fileName = "0";
		/// <summary>
		/// 设置生成的文件名
		/// </summary>
		/// <param name="fileName">新文件名，不要带后缀，0自动，1为原样，或直接自定义</param>
		/// <returns></returns>
		public Upload FileName(object fileName) { _fileName = fileName + ""; return this; }

		/// <summary>
		/// 已上传的文件列表
		/// </summary>
		public System.Collections.ArrayList List = new System.Collections.ArrayList();

		/// <summary>
		/// 结果错误号，0为正确，-1为未上传，>0为错误
		/// </summary>
		public int ErrorID = -1;
		/// <summary>
		/// 结果错误描述
		/// </summary>
		public string ErrorMessage = null;
		/// <summary>
		/// 文件地址，相对根目录
		/// </summary>
		public string Url { get { return List.Count > 0 ? List[0] + "" : ""; } }

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="dir">保存目录</param>
		/// <param name="ext">允许扩展名</param>
		/// <param name="max">最大上传大小</param>
		/// <param name="fileName">文件名</param>
		public Upload(object dir = null, string ext = null, int max = 0, object fileName = null)
		{
			string dirName = dir + "";
			if (dirName.Length == 0) { dir = System.Configuration.ConfigurationManager.AppSettings["SH.Upload.Dir"] + ""; }
			if (dirName.Length > 0) { _dir = dirName; }

			ext = ext + "";
			if (ext.Length == 0) { ext = System.Configuration.ConfigurationManager.AppSettings["SH.Upload.Ext"] + ""; }
			if (ext.Length > 0) { _ext = ext; }

			if (max == 0) { Int32.TryParse(System.Configuration.ConfigurationManager.AppSettings["SH.Upload.Max"] + "", out max); }
			if (max > 0) { _max = max; }

			_fileName = fileName + "";
		}

		/// <summary>
		/// 上传处理，
		/// </summary>
		/// <param name="fieldName">前台上传标签name值</param>
		/// <returns></returns>
		public Upload Post(string fieldName = "file")
		{
			if (System.Web.HttpContext.Current.Request.HttpMethod != "POST") { return this; }
			
			string dir = _dir;

			string ext = (_ext + "").Replace(".", "").Trim(',');
			if (ext.Length == 0) { ext = "media,image,file"; }
			ext = ext.Replace("media", "audio,video,flash");
			ext = ext.Replace("audio", "mp3");
			ext = ext.Replace("video", "mp4");
			ext = ext.Replace("flash", "swf,flv");
			ext = ext.Replace("image", "gif,jpg,png,bmp");
			ext = ext.Replace("file", "doc,docx,xls,xlsx,ppt,pptx,zip,rar");
			ext = "," + ext + ',';

			int max = _max * 1024;

			string fileName = _fileName + "";
			if (fileName.Length == 0 || fileName == "0") { fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff"); }

			System.Web.HttpFileCollection files = System.Web.HttpContext.Current.Request.Files;
			if(files.Count < 1){ErrorID = 1; ErrorMessage = "请选择文件"; return this;}
			for (int i = 0; i < files.Count; i++)
			{
				if (files.AllKeys[i].Length == 0 || files.AllKeys[i].ToLower() != fieldName.ToLower()) { continue; }
				System.Web.HttpPostedFile file = files[i];
				if (file.InputStream.Length == 0) { ErrorID = 1; ErrorMessage = "请选择文件"; break; }

				if (file.InputStream.Length > max && max > 0) { ErrorID = 2; ErrorMessage = file.FileName + "文件大小超过限制"; break; }

				string fileExt = System.IO.Path.GetExtension(file.FileName).ToLower() + "";
				if (fileExt.Length == 0 || ext.Contains("," + fileExt.Substring(1) + ",") == false)
				{
					ErrorID = 3; ErrorMessage = file.FileName + "文件类型不支持"; break;
				}
				dir = dir.Replace("{date}", "{yyyy}/{MM}/{dd}");
				dir = dir.Replace("{yyyy}", DateTime.Now.ToString("yyyy"));
				dir = dir.Replace("{MM}", DateTime.Now.ToString("MM"));
				dir = dir.Replace("{dd}", DateTime.Now.ToString("dd"));
				dir = dir.TrimEnd('/');
				string path = "";
				foreach (string v in dir.Split('/'))
				{
					path += v + "/";
					if (path.Length > 1 && v.Length > 0)
					{
						string realPath = System.Web.HttpContext.Current.Server.MapPath(path);
						if (!System.IO.Directory.Exists(realPath)) { System.IO.Directory.CreateDirectory(realPath); }
					}
				}
				string url = dir + "/";

				if(fileName == "1"){
					string tempFileName = file.FileName.Replace("\\", "/");
					if (tempFileName.Contains("/")) { tempFileName.Substring(tempFileName.LastIndexOf("/") + 1); }
					if (tempFileName.Contains(".")) { tempFileName.Substring(0, tempFileName.LastIndexOf(".")); }
					url = url + tempFileName;
				}
				else
				{
					url = url + fileName + i;
				}
				url = url + fileExt;
				if (!url.StartsWith("/"))
				{
					string requestPath = System.Web.HttpContext.Current.Request.Path;
					requestPath = requestPath.Substring(0, requestPath.LastIndexOf("/"));
					while (url.StartsWith("../"))
					{
						requestPath = requestPath.Substring(0, requestPath.LastIndexOf("/"));
						url = ("|" + url).Replace("|../", "");

					}
					url = requestPath + "/" + url;
				}
				List.Add(url);
				path = System.Web.HttpContext.Current.Server.MapPath(url);
				file.SaveAs(path);				
			}
			return this;
		}

		/// <summary>
		/// 清空本对象已上传的所有文件
		/// </summary>
		/// <param name="content">输出内容</param>
		public void Clear()
		{
			foreach (string file in List) { System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath(file)); }
			List.Clear();
		}

		/// <summary>
		/// 所有文件
		/// </summary>
		/// <param name="dir">保存目录</param>
		/// <param name="max">最大上传大小</param>
		/// <param name="fileName">文件名</param>
		/// <returns></returns>
		public static Upload All(object dir = null, int max = 100 * 1024 * 1024, object fileName = null)
		{
			return new Upload(dir, null, max, fileName);
		}

		/// <summary>
		/// 媒体文件，Flash+音视频
		/// </summary>
		/// <param name="dir">保存目录</param>
		/// <param name="max">最大上传大小</param>
		/// <param name="fileName">文件名</param>
		/// <returns></returns>
		public static Upload Media(object dir = null, int max = 100 * 1024 * 1024, object fileName = null)
		{
			return new Upload(dir, "media", max, fileName);
		}

		/// <summary>
		/// 音频文件
		/// </summary>
		/// <param name="dir">保存目录</param>
		/// <param name="max">最大上传大小</param>
		/// <param name="fileName">文件名</param>
		/// <returns></returns>
		public static Upload Audio(object dir = null, int max = 5 * 1024 * 1024, object fileName = null)
		{
			return new Upload(dir, "audio", max, fileName);
		}

		/// <summary>
		/// 视频文件
		/// </summary>
		/// <param name="dir">保存目录</param>
		/// <param name="max">最大上传大小</param>
		/// <param name="fileName">文件名</param>
		/// <returns></returns>
		public static Upload Video(object dir = null, int max = 100 * 1024 * 1024, object fileName = null)
		{
			return new Upload(dir, "video", max, fileName);
		}

		/// <summary>
		/// Flash文件
		/// </summary>
		/// <param name="dir">保存目录</param>
		/// <param name="max">最大上传大小</param>
		/// <param name="fileName">文件名</param>
		/// <returns></returns>
		public static Upload Flash(object dir = null, int max = 20 * 1024 * 1024, object fileName = null)
		{
			return new Upload(dir, "flash", max, fileName);
		}

		/// <summary>
		/// 图片文件
		/// </summary>
		/// <param name="dir">保存目录</param>
		/// <param name="max">最大上传大小</param>
		/// <param name="fileName">文件名</param>
		/// <returns></returns>
		public static Upload Image(object dir = null, int max = 2 * 1024 * 1024, object fileName = null)
		{
			return new Upload(dir, "image", max, fileName);
		}

		/// <summary>
		/// 文档文件
		/// </summary>
		/// <param name="dir">保存目录</param>
		/// <param name="max">最大上传大小</param>
		/// <param name="fileName">文件名</param>
		/// <returns></returns>
		public static Upload File(object dir = null, int max = 10 * 1024 * 1024, object fileName = null)
		{
			return new Upload(dir, "file", max, fileName);
		}

		/// <summary>
		/// KindEditor接口
		/// </summary>
		/// <param name="path">保存路径</param>
		/// <param name="max">上传大小限制，单位K</param>
		public static string KindEditor(string path = "../attached", int max = 10 * 1000)
		{
			//文件保存目录路径
			string dir = System.Web.HttpContext.Current.Request.QueryString["dir"] + "";
			if (dir.Length == 0) { dir = "image"; }
			if (!"image,flash,media,file".Contains(dir)) { Debug("目录名不正确"); }
			path = path.TrimEnd('/') + "/" + dir;

			System.Web.HttpContext.Current.Response.Clear();
			Upload upload = new Upload(path).Post("imgFile");
			if (upload.ErrorID == 0)
			{
				System.Web.HttpContext.Current.Response.Write("{\"error\":0,\"url\":\"" + upload.Url + "\"}");
			}
			else
			{
				System.Web.HttpContext.Current.Response.Write("{\"error\":1,\"message\":\"" + upload.ErrorMessage + "\"}");
			}
			System.Web.HttpContext.Current.Response.End();
			return null;
		}

		/// <summary>
		/// JSON
		/// </summary>
		public void Json()
		{
			string list = ""; foreach (string file in List) { list += ",\"" + file + "\""; }
			System.Web.HttpContext.Current.Response.Clear();
			System.Web.HttpContext.Current.Response.AddHeader("Content-Type", "text/html; charset=UTF-8");
			System.Web.HttpContext.Current.Response.Write("{\"ret\":" + ErrorID + ",\"message\":\"" + ErrorMessage + "\",\"result\":[" + list.Trim(',') + "]}");
			System.Web.HttpContext.Current.Response.End();
		}

		/// <summary>
		/// 调试
		/// </summary>
		/// <param name="content">内容</param>
		/// <param name="name">生成文件名</param>
		/// <returns></returns>
		public static string Debug(object content, object name = null)
		{
			
			if (content == null)
			{
				byte[] bytes = new byte[System.Web.HttpContext.Current.Request.InputStream.Length];
				System.Web.HttpContext.Current.Request.InputStream.Read(bytes, 0, bytes.Length);
				content = System.Text.Encoding.GetEncoding(contentType).GetString(bytes);
			}
			if (name == null)
			{
				System.Web.HttpContext.Current.Response.Clear();
				System.Web.HttpContext.Current.Response.Write(content);
				System.Web.HttpContext.Current.Response.End();
			}
			else
			{
				string filename = name + "";
				if (filename.Length == 0) { filename = System.DateTime.Now.ToString("yyyyMMddHHmmssffffff"); }
				if (!filename.Contains(".")) { filename += ".txt"; }
				filename = System.Web.HttpContext.Current.Server.MapPath(filename);
				System.IO.File.AppendAllText(filename, content + "\n\n", System.Text.Encoding.UTF8);
			}
			return null;
		}


	}
}