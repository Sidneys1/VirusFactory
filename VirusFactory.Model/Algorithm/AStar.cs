using System;
using System.Collections.Generic;
using VirusFactory.Model.Collections;
using VirusFactory.Model.Interface;

namespace VirusFactory.Model.Algorithm {
	public static class AStar {
		public static Path<T> FindPath<T>(T start, T destination, Func<T, T, double> distance, Func<T, double> estimate) where T : IHasNeighbors<T>, ICoordinate {
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
				foreach (var n in path.LastStep.Neighbors) {
					var d = distance(path.LastStep, n);
					var newPath = path.AddStep(n, d);
					queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
				}
			}

			return null;
		}
	}
}
