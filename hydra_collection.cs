using System;
using System.Collections.Generic;

namespace JsHydra {
	
	class HydraCollection {

		private int totalItems;
		private int hydraID;
		private Dictionary<int, List<int>> items;
		private DateTime created;

		public HydraCollection(int hID, int total) {
			hydraID = hID;
			totalItems = total;
			items = new Dictionary<int, List<int>>();
			created = DateTime.Now;
		}

		public void addItem(int pointID, int time) {
			if (items.ContainsKey(pointID)) {
				items[pointID].Add(time);
			} else {
				List<int> times = new List<int>();
				times.Add(time);
				items.Add(pointID, times);
			}
		}

		public DateTime getCreatedDate() {
			return created;
		}

		public int getID() {
			return hydraID;
		}

		public int getTotalItems() {
			return totalItems;
		}

		public int getTotalItemsHit() {
			int hit = 0;
			for (int num = 1; num <= totalItems; num += 1) {
				if (items.ContainsKey(num)) {
					hit += 1;
				}
			}
			return hit;
		}

	}

}