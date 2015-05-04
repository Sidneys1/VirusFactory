using System;
using System.Collections;
using System.Linq;

// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace FortuneVoronoi {

    public abstract class Fortune {
        public static readonly Vector VvInfinite = new Vector(double.PositiveInfinity, double.PositiveInfinity);
        public static readonly Vector VvUnkown = new Vector(double.NaN, double.NaN);

        internal static double ParabolicCut(double x1, double y1, double x2, double y2, double ys) {
            //			y1=-y1;
            //			y2=-y2;
            //			ys=-ys;
            //
            if (Math.Abs(x1 - x2) < 1e-10 && Math.Abs(y1 - y2) < 1e-10) {
                //				if(y1>y2)
                //					return double.PositiveInfinity;
                //				if(y1<y2)
                //					return double.NegativeInfinity;
                //				return x;
                throw new Exception("Identical datapoints are not allowed!");
            }

            if (Math.Abs(y1 - ys) < 1e-10 && Math.Abs(y2 - ys) < 1e-10)
                return (x1 + x2) / 2;
            if (Math.Abs(y1 - ys) < 1e-10)
                return x1;
            if (Math.Abs(y2 - ys) < 1e-10)
                return x2;
            var a1 = 1 / (2 * (y1 - ys));
            var a2 = 1 / (2 * (y2 - ys));
            if (Math.Abs(a1 - a2) < 1e-10)
                return (x1 + x2) / 2;
            var xs1 = 0.5 / (2 * a1 - 2 * a2) * (4 * a1 * x1 - 4 * a2 * x2 + 2 * Math.Sqrt(-8 * a1 * x1 * a2 * x2 - 2 * a1 * y1 + 2 * a1 * y2 + 4 * a1 * a2 * x2 * x2 + 2 * a2 * y1 + 4 * a2 * a1 * x1 * x1 - 2 * a2 * y2));
            var xs2 = 0.5 / (2 * a1 - 2 * a2) * (4 * a1 * x1 - 4 * a2 * x2 - 2 * Math.Sqrt(-8 * a1 * x1 * a2 * x2 - 2 * a1 * y1 + 2 * a1 * y2 + 4 * a1 * a2 * x2 * x2 + 2 * a2 * y1 + 4 * a2 * a1 * x1 * x1 - 2 * a2 * y2));
            xs1 = Math.Round(xs1, 10);
            xs2 = Math.Round(xs2, 10);
            if (!(xs1 > xs2)) return y1 >= y2 ? xs2 : xs1;
            var h = xs1;
            xs1 = xs2;
            xs2 = h;
            return y1 >= y2 ? xs2 : xs1;
        }

        internal static Vector CircumCircleCenter(Vector a, Vector b, Vector c) {
            if (a == b || b == c || a == c)
                throw new Exception("Need three different points!");
            var tx = (a[0] + c[0]) / 2;
            var ty = (a[1] + c[1]) / 2;

            var vx = (b[0] + c[0]) / 2;
            var vy = (b[1] + c[1]) / 2;

            double ux, uy, wx, wy;

            if (a[0] == c[0]) {
                ux = 1;
                uy = 0;
            } else {
                ux = (c[1] - a[1]) / (a[0] - c[0]);
                uy = 1;
            }

            if (b[0] == c[0]) {
                wx = -1;
                wy = 0;
            } else {
                wx = (b[1] - c[1]) / (b[0] - c[0]);
                wy = -1;
            }

            var alpha = (wy * (vx - tx) - wx * (vy - ty)) / (ux * wy - wx * uy);

            return new Vector(tx + alpha * ux, ty + alpha * uy);
        }

        public static VoronoiGraph ComputeVoronoiGraph(IEnumerable datapoints) {
            var pq = new BinaryPriorityQueue();
            var currentCircles = new Hashtable();
            var vg = new VoronoiGraph();
            VNode rootNode = null;
            foreach (Vector v in datapoints) {
                pq.Push(new VDataEvent(v));
            }
            while (pq.Count > 0) {
                var ve = pq.Pop() as VEvent;
                VDataNode[] circleCheckList;
                if (ve is VDataEvent) {
                    rootNode = VNode.ProcessDataEvent(ve as VDataEvent, rootNode, vg, ve.Y, out circleCheckList);
                } else {
                    var @event = ve as VCircleEvent;
                    if (@event != null) {
                        currentCircles.Remove(@event.NodeN);
                        if (!@event.Valid)
                            continue;
                        rootNode = VNode.ProcessCircleEvent(@event, rootNode, vg, ve.Y, out circleCheckList);
                    } else throw new Exception($"Got event of type {ve?.GetType()}!");
                }
                foreach (var vd in circleCheckList) {
                    if (currentCircles.ContainsKey(vd)) {
                        ((VCircleEvent)currentCircles[vd]).Valid = false;
                        currentCircles.Remove(vd);
                    }
                    var vce = VNode.CircleCheckDataNode(vd, ve.Y);
                    if (vce == null) continue;
                    pq.Push(vce);
                    currentCircles[vd] = vce;
                }
                var dataEvent = ve as VDataEvent;
                if (dataEvent == null) continue;
                var dp = dataEvent.DataPoint;
                foreach (var vce in currentCircles.Values.Cast<VCircleEvent>().Where(vce => MathTools.Dist(dp[0], dp[1], vce.Center[0], vce.Center[1]) < vce.Y - vce.Center[1] && Math.Abs(MathTools.Dist(dp[0], dp[1], vce.Center[0], vce.Center[1]) - (vce.Y - vce.Center[1])) > 1e-10)) {
                    vce.Valid = false;
                }
            }
            VNode.CleanUpTree(rootNode);
            foreach (var ve in vg.Edges.Where(ve => !ve.Done).Where(ve => ve.VVertexB == VvUnkown)) {
                ve.AddVertex(VvInfinite);
                if (!(Math.Abs(ve.LeftData[1] - ve.RightData[1]) < 1e-10) || !(ve.LeftData[0] < ve.RightData[0])) continue;
                var T = ve.LeftData;
                ve.LeftData = ve.RightData;
                ve.RightData = T;
            }

            var minuteEdges = new ArrayList();
            foreach (var ve in vg.Edges.Where(ve => !ve.IsPartlyInfinite && ve.VVertexA == ve.VVertexB)) {
                minuteEdges.Add(ve);
                // prevent rounding errors from expanding to holes
                foreach (var ve2 in vg.Edges) {
                    if (ve2.VVertexA == ve.VVertexA)
                        ve2.VVertexA = ve.VVertexA;
                    if (ve2.VVertexB == ve.VVertexA)
                        ve2.VVertexB = ve.VVertexA;
                }
            }
            foreach (VoronoiEdge ve in minuteEdges)
                vg.Edges.Remove(ve);

            return vg;
        }

        public static VoronoiGraph FilterVg(VoronoiGraph vg, double minLeftRightDist) {
            var vgErg = new VoronoiGraph();
            foreach (var ve in vg.Edges.Where(ve => Math.Sqrt(Vector.Dist(ve.LeftData, ve.RightData)) >= minLeftRightDist)) {
                vgErg.Edges.Add(ve);
            }
            foreach (var ve in vgErg.Edges) {
                vgErg.Vertizes.Add(ve.VVertexA);
                vgErg.Vertizes.Add(ve.VVertexB);
            }
            return vgErg;
        }
    }
}