using System;

namespace FortuneVoronoi {
	internal class VCircleEvent : VEvent
	{
		public VDataNode NodeN, NodeL, NodeR;
		public Vector Center;
		public override double Y => Math.Round(Center[1]+MathTools.Dist(NodeN.DataPoint[0],NodeN.DataPoint[1],Center[0],Center[1]),10);

		public override double X => Center[0];

		public bool Valid = true;
	}
}