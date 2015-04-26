using System.Collections.Generic;
using VirusFactory.Model.Interface;

namespace VirusFactory.Model.Geography
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Adds or Updates the dictionary to include the destination and its associated cost and complete path (and param arrays make paths easier to work with)
		/// </summary>

		public static void Set<T>(this Dictionary<T, KeyValuePair<double, LinkedList<Connection<T>>>> dictionary, T destination, double cost, params Connection<T>[] paths) where T : ICoordinate
		{
			var completePath = paths == null ? new LinkedList<Connection<T>>() : new LinkedList<Connection<T>>(paths);
			dictionary[destination] = new KeyValuePair<double, LinkedList<Connection<T>>>(cost, completePath);
		}

	}
}