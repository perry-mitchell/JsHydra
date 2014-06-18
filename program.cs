using System;
using System.Net;

namespace JsHydra {

	class Program {

		public static void Main(string[] args) {
			if (args.Length > 0) {
				Console.WriteLine("jsHydra - v0.1");
				boot(args[0]);
			} else {
				Console.WriteLine("A single argument, the JavaScript URL, is required..");
			}
		}

		protected static void boot(string javascriptURL) {
			Console.WriteLine("");
			Console.WriteLine("Downloading target script: " + javascriptURL);
			string source = "";
			using (WebClient client = new WebClient()) {
				source = client.DownloadString(javascriptURL);
			}
			Console.WriteLine("Retrieved source: " + source.Length + " characters received");

			Dresser d = new Dresser(source);
			d.prepareSource();
			Console.WriteLine("Booting server: " + "localhost:80000");
			HydraServer server = new HydraServer(d);
			server.run();
			Console.WriteLine("Server running. Press any key to stop...");
			Console.ReadKey();
			server.stop();
		}

	}

}