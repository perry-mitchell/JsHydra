(function() {
	window.JsHydra = (function() {

		var coverageBuffer = [];
		var started = new Date();
		var hydraID = Math.floor(Math.random() * 10000) + 1;
		var serverURL = "[server_url]";
		var maxSendLength = 10000;
		var pointMax = 100;

		var pointCountTrack = {};


		function post(path, params) {
			var xmlhttp = null,
				data = '';

			for (var key in params) {
				if (params.hasOwnProperty(key)) {
					data += '&' + encodeURIComponent(key) + "=" + encodeURIComponent(params[key]);
				}
			}

			data = data.substr(1); //Get rid of the first character.

			if (window.XMLHttpRequest) {
				xmlhttp = new XMLHttpRequest();
			} else {
				xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
			}

			xmlhttp.onreadystate = function() { };

			xmlhttp.open("POST", path, true);
			xmlhttp.send(data); //Send your data in the send method.
		}

		// start
		

		// run
		setInterval(function() {
			// check buffer
			var sendContent = "";
			while (coverageBuffer.length > 0) {
				var coverage = coverageBuffer.shift();
				sendContent += coverage.executionID + "|" + coverage.executed + "|" + hydraID + "\n";
				if (sendContent.length > maxSendLength) {
					break;
				}
			}
			console.log("contentlen", sendContent.length);
			if (sendContent.length > 0) {
				sendContent = "\n" + sendContent;
				post(serverURL, {
					content: sendContent
				});
			}
		}, 1000);

		return {

			getHydraID: function() {
				return hydraID;
			},

			log: function(text) {

			},

			touchCoveragePoint: function(cpid) {
				var cs = "point" + cpid;
				if (!pointCountTrack.hasOwnProperty(cs)) {
					pointCountTrack[cs] = 1;
				} else {
					pointCountTrack[cs] += 1;
				}
				if (pointCountTrack[cs] > pointMax) {
					return;
				}
				var exDate = new Date(),
					diff = new Date();
				diff.setTime(exDate.getTime() - started.getTime());
				var coverage = {
					executionID: cpid,
					executed: diff.getMilliseconds(),
					hydraID: hydraID
				};
				coverageBuffer.push(coverage);
			}

		};
	})();

	window.JsHydra.log("Hydra started, ID: " + window.JsHydra.getHydraID());

}());