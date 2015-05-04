using System;

namespace FortuneVoronoi {

    internal class VEdgeNode : VNode {

        public VEdgeNode(VoronoiEdge e, bool flipped) {
            Edge = e;
            Flipped = flipped;
        }

        public VoronoiEdge Edge;
        public bool Flipped;

        public double Cut(double ys, double x) {
            return !Flipped ? Math.Round(x - Fortune.ParabolicCut(Edge.LeftData[0], Edge.LeftData[1], Edge.RightData[0], Edge.RightData[1], ys), 10) : Math.Round(x - Fortune.ParabolicCut(Edge.RightData[0], Edge.RightData[1], Edge.LeftData[0], Edge.LeftData[1], ys), 10);
        }
    }
}