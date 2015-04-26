using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VirusFactory.Model.Geography {
	public static class AStar//<T>
	{
		public static Path<T> FindPath<T>(
			T start,
			T destination,
			Func<T, T, double> distance,
			Func<T, double> estimate)
			where T : IHasNeighbors<T>, ICoordinate {
			//
			var closed = new HashSet<T>();
			var queue = new PriorityQueue<double, Path<T>>();
			queue.Enqueue(0, new Path<T>(start));
			while (!queue.IsEmpty) {
				var path = queue.Dequeue();
				if (closed.Contains(path.LastStep))
					continue;
				if (path.LastStep.Equals(destination))
					return path;
				closed.Add(path.LastStep);
				foreach (var n in path.LastStep.Neighbors)
				{
					var d = distance(path.LastStep, n);
					var newPath = path.AddStep(n, d);
					queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
				}
			}

			return null;
		}
	}

	public interface IHasNeighbors<out TN> {
		IEnumerable<TN> Neighbors { get; }
	}

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

	public class Path<T> : IEnumerable<T> where T : ICoordinate {
		public T LastStep { get; }
		public Path<T> PreviousSteps { get; }
		public double TotalCost { get; }

		private Path(T lastStep, Path<T> previousSteps, double totalCost) {
			LastStep = lastStep;
			PreviousSteps = previousSteps;
			TotalCost = totalCost;
		}

		public Path(T start) : this(start, null, 0) {
		}

		public Path<T> AddStep(T step, double stepCost) {
			return new Path<T>(step, this, TotalCost + stepCost);
		}

		public IEnumerator<T> GetEnumerator() {
			for (var p = this; p != null; p = p.PreviousSteps)
				yield return p.LastStep;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	}
}
