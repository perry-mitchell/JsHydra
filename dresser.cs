using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JsHydra {

	class Dresser {

		protected string sourceCode;
		protected int pointCount;

		public Dresser(string jsSource) {
			sourceCode = jsSource;
			pointCount = 0;
		}

		public int getPointCount() {
			return pointCount;
		}

		public string getSourceCode() {
			return sourceCode;
		}

		private void injectCoveragePoints() {
			int currentID = 1;
			string finalised = "";
			string[] lines = sourceCode.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
			Regex rex = new Regex(@"(^function [_a-zA-Z0-9]+[ ]?\(|^[_a-zA-Z0-9]+[ ]?:[ ]?function[ ]?\()");
			bool addNext = false;
			foreach (string line in lines) {
				bool addPoint = false;
				if (addNext) {
					addPoint = true;
					addNext = false;
				}
				string testLine = line.Trim();
				if (rex.IsMatch(testLine)) {
					if (testLine[testLine.Length - 1] == '{') {
						addPoint = true;
					} else {
						addNext = true;
					}
				}
				finalised += line + "\n";
				if (addPoint) {
					finalised += "window.JsHydra.touchCoveragePoint(" + currentID + ");\n";
					currentID += 1;
					Console.Write(".");
				}
			}
			pointCount = currentID - 1;
			sourceCode = finalised;
			Console.WriteLine("");
		}

		public void prepareSource() {
			Console.WriteLine("Preparing source code:");
			Console.WriteLine(" - Injecting coverage points");
			this.injectCoveragePoints();
			Console.WriteLine(" - Injecting js-hydra");
			string hydraCode = File.ReadAllText("jsHydra.js");
			hydraCode = hydraCode.Replace("[server_url]", "http://localhost:9999/hydra/report/");
			sourceCode = hydraCode + sourceCode;
		}

	}

}