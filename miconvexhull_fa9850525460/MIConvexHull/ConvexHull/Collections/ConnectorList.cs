namespace MIConvexHull.ConvexHull.Collections {
	/// <summary>
	/// Connector list.
	/// </summary>
	sealed class ConnectorList
	{
		FaceConnector _last;

		/// <summary>
		/// Get the first element.
		/// </summary>
		public FaceConnector First { get; private set; }

/*
		/// <summary>
		/// Adds the element to the beginning.
		/// </summary>
		/// <param name="connector"></param>
		void AddFirst(FaceConnector connector)
		{
			First.Previous = connector;
			connector.Next = First;
			First = connector;
		}
*/

		/// <summary>
		/// Adds a face to the list.
		/// </summary>
		/// <param name="element"></param>
		public void Add(FaceConnector element)
		{
			if (_last != null)
			{
				_last.Next = element;
			}
			element.Previous = _last;
			_last = element;
			if (First == null)
			{
				First = element;
			}
		}

		/// <summary>
		/// Removes the element from the list.
		/// </summary>
		/// <param name="connector"></param>
		public void Remove(FaceConnector connector)
		{
			if (connector.Previous != null)
			{
				connector.Previous.Next = connector.Next;
			}
			else if (/*first == face*/ connector.Previous == null)
			{
				First = connector.Next;
			}

			if (connector.Next != null)
			{
				connector.Next.Previous = connector.Previous;
			}
			else if (/*last == face*/ connector.Next == null)
			{
				_last = connector.Previous;
			}

			connector.Next = null;
			connector.Previous = null;
		}
	}
}