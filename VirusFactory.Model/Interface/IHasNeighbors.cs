using System.Collections.Generic;

namespace VirusFactory.Model.Interface
{
	public interface IHasNeighbors<out TN> {
		IEnumerable<TN> Neighbors { get; }
	}
}