using System;

// ReSharper disable PossibleUnintendedReferenceComparison

namespace FortuneVoronoi {

    internal abstract class VNode {
        private VNode _left, _right;

        public VNode Left {
            get { return _left; }
            set {
                _left = value;
                value.Parent = this;
            }
        }

        public VNode Right {
            get { return _right; }
            set {
                _right = value;
                value.Parent = this;
            }
        }

        public VNode Parent { get; set; }

        public void Replace(VNode childOld, VNode childNew) {
            if (Left == childOld)
                Left = childNew;
            else if (Right == childOld)
                Right = childNew;
            else throw new Exception("Child not found!");
            childOld.Parent = null;
        }

        public static VDataNode FirstDataNode(VNode root) {
            var c = root;
            while (c.Left != null)
                c = c.Left;
            return (VDataNode)c;
        }

        public static VDataNode LeftDataNode(VDataNode current) {
            VNode c = current;
            //1. Up
            do {
                if (c.Parent == null)
                    return null;
                if (c.Parent.Left == c) {
                    c = c.Parent;
                } else {
                    c = c.Parent;
                    break;
                }
            } while (true);
            //2. One Left
            c = c.Left;
            //3. Down
            while (c.Right != null)
                c = c.Right;
            return (VDataNode)c; // Cast statt 'as' damit eine Exception kommt
        }

        public static VDataNode RightDataNode(VDataNode current) {
            VNode c = current;
            //1. Up
            do {
                if (c.Parent == null)
                    return null;
                if (c.Parent.Right == c) {
                    c = c.Parent;
                } else {
                    c = c.Parent;
                    break;
                }
            } while (true);
            //2. One Right
            c = c.Right;
            //3. Down
            while (c.Left != null)
                c = c.Left;
            return (VDataNode)c; // Cast statt 'as' damit eine Exception kommt
        }

        public static VEdgeNode EdgeToRightDataNode(VDataNode current) {
            VNode c = current;
            //1. Up
            do {
                if (c.Parent == null)
                    throw new Exception("No Left Leaf found!");
                if (c.Parent.Right == c) {
                    c = c.Parent;
                    continue;
                }
                c = c.Parent;
                break;
            } while (true);
            return (VEdgeNode)c;
        }

        public static VDataNode FindDataNode(VNode root, double ys, double x) {
            var c = root;
            do {
                var node = c as VDataNode;
                if (node != null)
                    return node;
                c = ((VEdgeNode)c).Cut(ys, x) < 0 ? c.Left : c.Right;
            } while (true);
        }

        /// <summary>
        /// Will return the new root (unchanged except in start-up)
        /// </summary>
        public static VNode ProcessDataEvent(VDataEvent e, VNode root, VoronoiGraph vg, double ys, out VDataNode[] circleCheckList) {
            if (root == null) {
                root = new VDataNode(e.DataPoint);
                circleCheckList = new[] { (VDataNode)root };
                return root;
            }
            //1. Find the node to be replaced
            VNode c = FindDataNode(root, ys, e.DataPoint[0]);
            //2. Create the subtree (ONE Edge, but two VEdgeNodes)
            var ve = new VoronoiEdge {
                LeftData = ((VDataNode)c).DataPoint,
                RightData = e.DataPoint,
                VVertexA = Fortune.VvUnkown,
                VVertexB = Fortune.VvUnkown
            };
            vg.Edges.Add(ve);

            VNode subRoot;
            if (Math.Abs(ve.LeftData[1] - ve.RightData[1]) < 1e-10) {
                if (ve.LeftData[0] < ve.RightData[0]) {
                    subRoot = new VEdgeNode(ve, false) {
                        Left = new VDataNode(ve.LeftData),
                        Right = new VDataNode(ve.RightData)
                    };
                } else {
                    subRoot = new VEdgeNode(ve, true) {
                        Left = new VDataNode(ve.RightData),
                        Right = new VDataNode(ve.LeftData)
                    };
                }
                circleCheckList = new[] { (VDataNode)subRoot.Left, (VDataNode)subRoot.Right };
            } else {
                subRoot = new VEdgeNode(ve, false) {
                    Left = new VDataNode(ve.LeftData),
                    Right = new VEdgeNode(ve, true) {
                        Left = new VDataNode(ve.RightData),
                        Right = new VDataNode(ve.LeftData)
                    }
                };
                circleCheckList = new[] { (VDataNode)subRoot.Left, (VDataNode)subRoot.Right.Left, (VDataNode)subRoot.Right.Right };
            }

            //3. Apply subtree
            if (c.Parent == null)
                return subRoot;
            c.Parent.Replace(c, subRoot);
            return root;
        }

        public static VNode ProcessCircleEvent(VCircleEvent e, VNode root, VoronoiGraph vg, double ys, out VDataNode[] circleCheckList) {
            VEdgeNode eo;
            var b = e.NodeN;
            var a = LeftDataNode(b);
            var c = RightDataNode(b);
            if (a == null || b.Parent == null || c == null || a.DataPoint != e.NodeL.DataPoint || c.DataPoint != e.NodeR.DataPoint) {
                circleCheckList = new VDataNode[] { };
                return root; // Abbruch da sich der Graph verändert hat
            }
            var eu = (VEdgeNode)b.Parent;
            circleCheckList = new[] { a, c };
            //1. Create the new Vertex
            var vNew = new Vector(e.Center[0], e.Center[1]);
            //			VNew[0] = Fortune.ParabolicCut(a.DataPoint[0],a.DataPoint[1],c.DataPoint[0],c.DataPoint[1],ys);
            //			VNew[1] = (ys + a.DataPoint[1])/2 - 1/(2*(ys-a.DataPoint[1]))*(VNew[0]-a.DataPoint[0])*(VNew[0]-a.DataPoint[0]);
            vg.Vertizes.Add(vNew);
            //2. Find out if a or c are in a distand part of the tree (the other is then b's sibling) and assign the new vertex
            if (eu.Left == b) // c is sibling
            {
                eo = EdgeToRightDataNode(a);

                // replace eu by eu's Right
                eu.Parent.Replace(eu, eu.Right);
            } else // a is sibling
              {
                eo = EdgeToRightDataNode(b);

                // replace eu by eu's Left
                eu.Parent.Replace(eu, eu.Left);
            }
            eu.Edge.AddVertex(vNew);
            //			///////////////////// uncertain
            //			if(eo==eu)
            //				return Root;
            //			/////////////////////

            //complete & cleanup eo
            eo.Edge.AddVertex(vNew);
            //while(eo.Edge.VVertexB == Fortune.VVUnkown)
            //{
            //    eo.Flipped = !eo.Flipped;
            //    eo.Edge.AddVertex(Fortune.VVInfinite);
            //}
            //if(eo.Flipped)
            //{
            //    Vector T = eo.Edge.LeftData;
            //    eo.Edge.LeftData = eo.Edge.RightData;
            //    eo.Edge.RightData = T;
            //}

            //2. Replace eo by new Edge
            var ve = new VoronoiEdge {
                LeftData = a.DataPoint,
                RightData = c.DataPoint
            };
            ve.AddVertex(vNew);
            vg.Edges.Add(ve);

            var ven = new VEdgeNode(ve, false) {
                Left = eo.Left,
                Right = eo.Right
            };
            if (eo.Parent == null)
                return ven;
            eo.Parent.Replace(eo, ven);
            return root;
        }

        public static VCircleEvent CircleCheckDataNode(VDataNode n, double ys) {
            var l = LeftDataNode(n);
            var r = RightDataNode(n);
            if (l == null || r == null || l.DataPoint == r.DataPoint || l.DataPoint == n.DataPoint || n.DataPoint == r.DataPoint)
                return null;
            if (MathTools.ccw(l.DataPoint[0], l.DataPoint[1], n.DataPoint[0], n.DataPoint[1], r.DataPoint[0], r.DataPoint[1], false) <= 0)
                return null;
            var center = Fortune.CircumCircleCenter(l.DataPoint, n.DataPoint, r.DataPoint);
            var vc = new VCircleEvent {
                NodeN = n,
                NodeL = l,
                NodeR = r,
                Center = center,
                Valid = true
            };
            if (vc.Y > ys || Math.Abs(vc.Y - ys) < 1e-10)
                return vc;
            return null;
        }

        public static void CleanUpTree(VNode root) {
            while (true) {
                if (root is VDataNode)
                    return;
                var ve = root as VEdgeNode;
                while (ve?.Edge.VVertexB == Fortune.VvUnkown) {
                    ve?.Edge.AddVertex(Fortune.VvInfinite);
                    //				VE.Flipped = !VE.Flipped;
                }
                if (ve != null && ve.Flipped) {
                    var T = ve.Edge.LeftData;
                    ve.Edge.LeftData = ve.Edge.RightData;
                    ve.Edge.RightData = T;
                }
                if (ve != null) ve.Edge.Done = true;
                CleanUpTree(root.Left);
                root = root.Right;
            }
        }
    }
}