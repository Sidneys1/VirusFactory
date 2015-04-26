using System;

namespace FortuneVoronoi {
	internal abstract class VEvent : IComparable
	{
		public abstract double Y {get;}
		public abstract double X {get;}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			if(!(obj is VEvent))
				throw new ArgumentException("obj not VEvent!");
			var i = Y.CompareTo(((VEvent)obj).Y);
			return i!=0 ? i : X.CompareTo(((VEvent)obj).X);
		}

		#endregion
	}
}