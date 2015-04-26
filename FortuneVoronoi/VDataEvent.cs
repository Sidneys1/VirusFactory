namespace FortuneVoronoi {
	internal class VDataEvent : VEvent
	{
		public Vector DataPoint;
		public VDataEvent(Vector dp)
		{
			DataPoint = dp;
		}
		public override double Y => DataPoint[1];

		public override double X => DataPoint[0];
	}
}