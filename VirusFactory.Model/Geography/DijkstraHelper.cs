using System;
using System.Collections.Generic;
using System.Linq;

namespace VirusFactory.Model.Geography
{
	public static class DijkstraHelper<T> where T : ICoordinate
	{
		private static readonly Dictionary<T,Dictionary<T, LinkedList<Connection<T>>>> CacheDictionary = new Dictionary<T, Dictionary<T, LinkedList<Connection<T>>>>();

		public static int DictionarySize => CacheDictionary.Sum(o => o.Value.Count);

		public static LinkedList<Connection<T>> CalculateShortestPathBetween(T source, T destination, IEnumerable<Connection<T>> paths)
		{
			if (!CacheDictionary.ContainsKey(source))
				CacheDictionary.Add(source, CalculateFrom(source, paths));
			
			return CacheDictionary[source][destination];
		}

		public static Dictionary<T, LinkedList<Connection<T>>> CalculateShortestFrom(T source, IEnumerable<Connection<T>> paths)
		{
			if (!CacheDictionary.ContainsKey(source))
				CacheDictionary.Add(source, CalculateFrom(source, paths));

			return CacheDictionary[source];
		}
		
		private static Dictionary<T, LinkedList<Connection<T>>> CalculateFrom(T source, IEnumerable<Connection<T>> paths)
		{
			// validate the paths
			var connections = paths as IList<Connection<T>> ?? paths.ToList();
			if (connections.Any(p => p.LocationA.Equals(p.LocationB)))
				throw new ArgumentException("No path can have the same source and destination");
			
			// keep track of the shortest paths identified thus far
			var shortestPaths = new Dictionary<T, KeyValuePair<double, LinkedList<Connection<T>>>>();

			// keep track of the locations which have been completely processed
			var locationsProcessed = new List<T>();
			
			// include all possible steps, with Int.MaxValue cost
			connections.SelectMany(p => new[] { p.LocationA, p.LocationB })				// union source and destinations
					.Distinct()														   // remove duplicates
					.ToList()														   // ToList exposes ForEach
					.ForEach(s => shortestPaths.Set(s, double.MaxValue, null));		   // add to ShortestPaths with MaxValue cost
			
			// update cost for self-to-self as 0; no path
			shortestPaths.Set(source, 0, null);
			
			// keep this cached
			var locationCount = shortestPaths.Keys.Count;

			while (locationsProcessed.Count < locationCount)
			{
				var locationToProcess = default(T);

				//Search for the nearest location that isn't handled
				foreach (var location in shortestPaths.OrderBy(p => p.Value.Key).Select(p => p.Key).ToList().Where(location => !locationsProcessed.Contains(location)))
				{
					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (shortestPaths[location].Key == double.MaxValue)
						return shortestPaths.ToDictionary(k => k.Key, v => v.Value.Value); //ShortestPaths[destination].Value;

					locationToProcess = location;
					break;
				}

				var selectedPaths = connections.Where(p => p.LocationA.Equals(locationToProcess));

				foreach (var path in selectedPaths.Where(path => shortestPaths[path.LocationB].Key > path.Length + shortestPaths[path.LocationA].Key))
				{
					shortestPaths.Set(
						path.LocationB,
						path.Length + shortestPaths[path.LocationA].Key,
						shortestPaths[path.LocationA].Value.Union(new[] { path }).ToArray());
				}
				
				//Add the location to the list of processed locations
				locationsProcessed.Add(locationToProcess);
			} // while
			
			return shortestPaths.ToDictionary(k => k.Key, v => v.Value.Value);
		}
	}
}
