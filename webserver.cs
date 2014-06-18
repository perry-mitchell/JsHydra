using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace JsHydra {

	class HydraServer {

		private Dresser dresser;
		private List<HydraCollection> collections;
		private string template;

		public HydraServer(Dresser d) : this(new string[]{"http://localhost:9999/hydra/"}) {
			dresser = d;
			collections = new List<HydraCollection>();
			template = File.ReadAllText("interface_template.html");
		}

		private readonly HttpListener _listener = new HttpListener();
		//private readonly Func<HttpListenerRequest, string> _responderMethod;
 
		public HydraServer(string[] prefixes)
		{
			if (!HttpListener.IsSupported) {
				throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
			}
 
			// URI prefixes are required, for example 
			// "http://localhost:8080/index/".
			if (prefixes == null || prefixes.Length == 0) {
				throw new ArgumentException("prefixes");
			}
 
			// A responder method is required
			/*if (method == null) {
				throw new ArgumentException("method");
			}*/
 
			foreach (string s in prefixes) {
				_listener.Prefixes.Add(s);
			}
 
			//_responderMethod = method;
			_listener.Start();
		}
 
		public void run() {
			ThreadPool.QueueUserWorkItem((o) => {
				Console.WriteLine("Webserver running...");
				try {
					while (_listener.IsListening) {
						ThreadPool.QueueUserWorkItem((c) => {
							var ctx = c as HttpListenerContext;
							try {
								//string rstr = _responderMethod(ctx.Request);
								string rstr = this.processData(ctx.Request);
								byte[] buf = Encoding.UTF8.GetBytes(rstr);
								if (rstr.Length <= 0) {
									ctx.Response.StatusCode = 302;
								}
								ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
								ctx.Response.ContentLength64 = buf.Length;
								ctx.Response.OutputStream.Write(buf, 0, buf.Length);
							} catch { } // suppress any exceptions
							finally	{
								// always close the stream
								ctx.Response.OutputStream.Close();
							}
						}, _listener.GetContext());
					}
				}
				catch { } // suppress any exceptions
			});
		}
 
		public void stop() {
			try {
				_listener.Stop();
			} catch {
				Console.WriteLine("Failed stopping server - error encountered");
			}
			try {
				_listener.Close();
			} catch {
				Console.WriteLine("Failed closing socket - error encountered");
			}
		}

		private string processData(HttpListenerRequest request) {
			string theURL = request.Url.OriginalString;
			theURL = theURL.ToLower();
			theURL = theURL.Replace("https://", "");
			theURL = theURL.Replace("http://", "");
			string[] uParts = theURL.Split('/');
			string output = "";
			//Console.WriteLine("Request for: " + theURL);
			if (uParts.Length >= 3) {
				if (uParts[2] == "source") {
					output = dresser.getSourceCode();
				} else if (uParts[2] == "report") {
					string text;
					using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))	{
						text = reader.ReadToEnd();
					}
					text = HttpUtility.UrlDecode(text);
					processReport(text);
				} else if (uParts[2] == "collections") {
					string collData = "<ul>\n";
					lock (collections) {
						foreach (HydraCollection cc in collections) {
							string created = cc.getCreatedDate().ToString("F");
							collData += "<li><a href=\"http://localhost:9999/hydra/collection/" + cc.getID() + "\">Collection #" + cc.getID() + "</a> (" + created + ")</li>\n";
						}
					}
					collData += "</ul>\n";
					output = template;
					output = output.Replace("[title]", "Hydra Collections");
					output = output.Replace("[content]", collData);
				} else if (uParts[2] == "collection") {
					if (uParts.Length > 3) {
						int collID = Convert.ToInt32(uParts[3]);
						HydraCollection c = null;
						lock (collections) {
							foreach (HydraCollection cc in collections) {
								if (cc.getID() == collID) {
									c = cc;
									break;
								}
							}
						}
						if (null != c) {
							string collData = "<div>\n";
							collData += "<p><a href=\"http://localhost:9999/hydra/collections/\">Back</a></p>\n";
							int hit = c.getTotalItemsHit();
							int total = c.getTotalItems();
							double ratio = ((double)hit / total);
							collData += "<p>Item hit-rate: " + hit + "/" + total + "<br />";
							collData += "Coverage: " + string.Format("{0:0.00%}", ratio) + "</p>\n";
							collData += "</div>\n";
							output = template;
							output = output.Replace("[title]", "Hydra Collection #" + collID);
							output = output.Replace("[content]", collData);
						}
					}
				}
			}
			return output;
		}

		private void processReport(string reportText) {
			string[] reportLines = reportText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
			lock (collections) {
				HydraCollection c;
				foreach (string rLine in reportLines) {
					string[] lineSections = rLine.Split('|');
					if (lineSections.Length == 3) {
						int pointID = Convert.ToInt32(lineSections[0]);
						int timeMS = Convert.ToInt32(lineSections[1]);
						int hydraID = Convert.ToInt32(lineSections[2]);
						bool foundCollection = false;
						//HydraCollection c;
						foreach (HydraCollection cc in collections) {
							if (cc.getID() == hydraID) {
								foundCollection = true;
								c = cc;
								break;
							}
						}
						if (foundCollection == false) {
							c = new HydraCollection(hydraID, dresser.getPointCount());
							collections.Add(c);
						}
						c.addItem(pointID, timeMS);
					}
				}
				int hit = c.getTotalItemsHit();
				int total = c.getTotalItems();
				double ratio = ((double)hit / total);
				Console.WriteLine("Collection #" + c.getID() + " coverage: " + string.Format("{0:0.00%}", ratio));
			}
		}

	}

}