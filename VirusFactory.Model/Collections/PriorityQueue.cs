using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VirusFactory.Model.Collections
{
	class PriorityQueue<TP, TV> : IEnumerable<TV> {
		private readonly SortedDictionary<TP, Queue<TV>> _list = new SortedDictionary<TP, Queue<TV>>();

		public void Enqueue(TP priority, TV value) {
			Queue<TV> q;
			if (!_list.TryGetValue(priority, out q)) {
				q = new Queue<TV>();
				_list.Add(priority, q);
			}
			q.Enqueue(value);
		}

		public TV Dequeue() {
			var pair = _list.First();
			var v = pair.Value.Dequeue();
			if (pair.Value.Count == 0)
				_list.Remove(pair.Key);
			return v;
		}

		public TV Peek() {
			var pair = _list.First();
			return pair.Value.Dequeue();
		}

		public bool IsEmpty => !_list.Any();

		public IEnumerator<TV> GetEnumerator() {
			return ((IEnumerable<TV>)_list).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}