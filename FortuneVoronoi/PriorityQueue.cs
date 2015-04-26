using System;
using System.Collections;

namespace FortuneVoronoi
{
	public interface IPriorityQueue : ICloneable, IList
	{
		int Push(object o);
		object Pop();
		object Peek();
		void Update(int i);
	}
	public class BinaryPriorityQueue : IPriorityQueue
	{
		protected ArrayList InnerList = new ArrayList();
		protected IComparer Comparer;

		#region contructors
		public BinaryPriorityQueue() : this(System.Collections.Comparer.Default)
		{}
		public BinaryPriorityQueue(IComparer c)
		{
			Comparer = c;
		}
		public BinaryPriorityQueue(int c) : this(System.Collections.Comparer.Default,c)
		{}
		public BinaryPriorityQueue(IComparer c, int capacity)
		{
			Comparer = c;
			InnerList.Capacity = capacity;
		}

		protected BinaryPriorityQueue(ArrayList core, IComparer comp, bool copy)
		{
			if(copy)
				InnerList = core.Clone() as ArrayList;
			else
				InnerList = core;
			Comparer = comp;
		}

		#endregion
		protected void SwitchElements(int i, int j)
		{
			var h = InnerList[i];
			InnerList[i] = InnerList[j];
			InnerList[j] = h;
		}

		protected virtual int OnCompare(int i, int j)
		{
			return Comparer.Compare(InnerList[i],InnerList[j]);
		}

		#region public methods
		/// <summary>
		/// Push an object onto the PQ
		/// </summary>
		/// <param name="o">The new object</param>
		/// <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
		public int Push(object o)
		{
			var p = InnerList.Count;
			InnerList.Add(o); // E[p] = O
			do
			{
				if(p==0)
					break;
				var p2 = (p-1)/2;
				if(OnCompare(p,p2)<0)
				{
					SwitchElements(p,p2);
					p = p2;
				}
				else
					break;
			}while(true);
			return p;
		}

		/// <summary>
		/// Get the smallest object and remove it.
		/// </summary>
		/// <returns>The smallest object</returns>
		public object Pop()
		{
			var result = InnerList[0];
			var p = 0;
			InnerList[0] = InnerList[InnerList.Count-1];
			InnerList.RemoveAt(InnerList.Count-1);
			do
			{
				var pn = p;
				var p1 = 2*p+1;
				var p2 = 2*p+2;
				if(InnerList.Count>p1 && OnCompare(p,p1)>0) // links kleiner
					p = p1;
				if(InnerList.Count>p2 && OnCompare(p,p2)>0) // rechts noch kleiner
					p = p2;
				
				if(p==pn)
					break;
				SwitchElements(p,pn);
			}while(true);
			return result;
		}

		/// <summary>
		/// Notify the PQ that the object at position i has changed
		/// and the PQ needs to restore order.
		/// Since you dont have access to any indexes (except by using the
		/// explicit IList.this) you should not call this function without knowing exactly
		/// what you do.
		/// </summary>
		/// <param name="i">The index of the changed object.</param>
		public void Update(int i)
		{
			var p = i;
			int p2;
			do	// aufsteigen
			{
				if(p==0)
					break;
				p2 = (p-1)/2;
				if(OnCompare(p,p2)<0)
				{
					SwitchElements(p,p2);
					p = p2;
				}
				else
					break;
			}while(true);
			if(p<i)
				return;
			do	   // absteigen
			{
				var pn = p;
				var p1 = 2*p+1;
				p2 = 2*p+2;
				if(InnerList.Count>p1 && OnCompare(p,p1)>0) // links kleiner
					p = p1;
				if(InnerList.Count>p2 && OnCompare(p,p2)>0) // rechts noch kleiner
					p = p2;
				
				if(p==pn)
					break;
				SwitchElements(p,pn);
			}while(true);
		}

		/// <summary>
		/// Get the smallest object without removing it.
		/// </summary>
		/// <returns>The smallest object</returns>
		public object Peek()
		{
			return InnerList.Count>0 ? InnerList[0] : null;
		}

		public bool Contains(object value)
		{
			return InnerList.Contains(value);
		}

		public void Clear()
		{
			InnerList.Clear();
		}

		public int Count => InnerList.Count;

		IEnumerator IEnumerable.GetEnumerator()
		{
			return InnerList.GetEnumerator();
		}

		public void CopyTo(Array array, int index)
		{
			InnerList.CopyTo(array,index);
		}

		public object Clone()
		{
			return new BinaryPriorityQueue(InnerList,Comparer,true);	
		}

		public bool IsSynchronized => InnerList.IsSynchronized;

		public object SyncRoot => this;

		#endregion
		#region explicit implementation
		bool IList.IsReadOnly => false;

		object IList.this[int index]
		{
			get
			{
				return InnerList[index];
			}
			set
			{
				InnerList[index] = value;
				Update(index);
			}
		}

		int IList.Add(object o)
		{
			return Push(o);
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException();
		}

		int IList.IndexOf(object value)
		{
			throw new NotSupportedException();
		}

		bool IList.IsFixedSize => false;

		public static BinaryPriorityQueue Syncronized(BinaryPriorityQueue p)
		{
			return new BinaryPriorityQueue(ArrayList.Synchronized(p.InnerList),p.Comparer,false);
		}
		public static BinaryPriorityQueue ReadOnly(BinaryPriorityQueue p)
		{
			return new BinaryPriorityQueue(ArrayList.ReadOnly(p.InnerList),p.Comparer,false);
		}
		#endregion
	}
}
