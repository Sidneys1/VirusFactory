namespace FortuneVoronoi {
	internal class VDataNode : VNode
	{
		public VDataNode(Vector dp)
		{
			DataPoint = dp;
		}
		public Vector DataPoint;
	}
}