using System;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable PossibleUnintendedReferenceComparison

namespace FortuneVoronoi {
	public class VoronoiEdge
	{
		internal bool Done;
		public Vector RightData, LeftData;
		public Vector VVertexA = Fortune.VvUnkown, VVertexB = Fortune.VvUnkown;
		public void AddVertex(Vector v)
		{
			if(VVertexA == Fortune.VvUnkown)
				VVertexA = v;
			else if(VVertexB == Fortune.VvUnkown)
				VVertexB = v;
			else throw new Exception("Tried to add third vertex!");
		}
		public bool IsInfinite => VVertexA == Fortune.VvInfinite && VVertexB == Fortune.VvInfinite;
		public bool IsPartlyInfinite => VVertexA == Fortune.VvInfinite || VVertexB == Fortune.VvInfinite;

		public Vector FixedPoint
		{
			get
			{
				if(IsInfinite)
					return 0.5 * (LeftData+RightData);
				return VVertexA != Fortune.VvInfinite ? VVertexA : VVertexB;
			}
		}
		public Vector DirectionVector
		{
			get
			{
				if(!IsPartlyInfinite)
					return (VVertexB-VVertexA)*(1.0/Math.Sqrt(Vector.Dist(VVertexA,VVertexB)));
				if(LeftData[0]==RightData[0])
				{
					return LeftData[1]<RightData[1] ? new Vector(-1,0) : new Vector(1,0);
				}
				var erg = new Vector(-(RightData[1]-LeftData[1])/(RightData[0]-LeftData[0]),1);
				if(RightData[0]<LeftData[0])
					erg.Multiply(-1);
				erg.Multiply(1.0/Math.Sqrt(erg.SquaredLength));
				return erg;
			}
		}
		public double Length => IsPartlyInfinite ? double.PositiveInfinity : Math.Sqrt(Vector.Dist(VVertexA,VVertexB));
	}
}