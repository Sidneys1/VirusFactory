namespace MIConvexHull.ConvexHull.Collections {
	/// <summary>
	/// A priority based linked list.
	/// </summary>
	sealed class FaceList
	{
		ConvexFaceInternal _last;
        
		/// <summary>
		/// Get the first element.
		/// </summary>
		public ConvexFaceInternal First { get; private set; }

		/// <summary>
		/// Adds the element to the beginning.
		/// </summary>
		/// <param name="face"></param>
		void AddFirst(ConvexFaceInternal face)
		{
			face.InList = true;
			First.Previous = face;
			face.Next = First;
			First = face;
		}

		/// <summary>
		/// Adds a face to the list.
		/// </summary>
		/// <param name="face"></param>
		public void Add(ConvexFaceInternal face)
		{
			if (face.InList)
			{
				if (First.VerticesBeyond.Count >= face.VerticesBeyond.Count) return;
				Remove(face);
				AddFirst(face);
				return;
			}

			face.InList = true;

			if (First != null && First.VerticesBeyond.Count < face.VerticesBeyond.Count)
			{
				First.Previous = face;
				face.Next = First;
				First = face;
			}
			else
			{
				if (_last != null)
				{
					_last.Next = face;
				}
				face.Previous = _last;
				_last = face;
				if (First == null)
				{
					First = face;
				}
			}
		}

		/// <summary>
		/// Removes the element from the list.
		/// </summary>
		/// <param name="face"></param>
		public void Remove(ConvexFaceInternal face)
		{
			if (!face.InList) return;

			face.InList = false;

			if (face.Previous != null)
			{
				face.Previous.Next = face.Next;
			}
			else if (/*first == face*/ face.Previous == null)
			{
				First = face.Next;
			}

			if (face.Next != null)
			{
				face.Next.Previous = face.Previous;
			}
			else if (/*last == face*/ face.Next == null)
			{
				_last = face.Previous;
			}

			face.Next = null;
			face.Previous = null;
		}
	}
}