using MIConvexHull.ConvexHull.Collections;
using MIConvexHull.Triangulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MIConvexHull.ConvexHull.Algorithm {

    internal class ConvexHullInternal {
        private const int CONNECTOR_TABLE_SIZE = 2017;

        /// <summary>
        ///     Are we on a paraboloid?
        /// </summary>
        private readonly bool _isLifted;

        /// <summary>
        ///     Explained in ConvexHullComputationConfig.
        /// </summary>
        private readonly double _planeDistanceTolerance;

        /*
         * Representation of the input vertices.
         *
         * - In the algorithm, a vertex is represented by its index in the Vertices array.
         *   This makes the algorithm a lot faster (up to 30%) than using object reference everywhere.
         * - Positions are stored as a single array of values. Coordinates for vertex with index i
         *   are stored at indices <i * Dimension, (i + 1) * Dimension)
         * - VertexMarks are used by the algorithm to help identify a set of vertices that is "above" (or "beyond")
         *   a specific face.
         * - VertexAdded helps with handling of degenerate data to prevent duplicate addition of the same vertex to the hull.
         */
        private readonly IVertex[] _vertices;

        /// <summary>
        ///     Stores faces that are visible from the current vertex.
        /// </summary>
        private IndexBuffer _affectedFaceBuffer;

        /// <summary>
        ///     Used to determine which vertices are "above" (or "beyond") a face
        /// </summary>
        private IndexBuffer _beyondBuffer;

        /// <summary>
        ///     The centroid of the currently computed hull.
        /// </summary>
        private double[] _center;

        /// <summary>
        ///     Stores faces that form a "cone" created by adding new vertex.
        /// </summary>
        private SimpleList<DeferredFace> _coneFaceBuffer;

        /// <summary>
        ///     The connector table helps to determine the adjacency of convex faces.
        ///     Hashing is used instead of pairwise comparison. This significantly speeds up the computations,
        ///     especially for higher dimensions.
        /// </summary>
        private ConnectorList[] _connectorTable;

        /// <summary>
        ///     A list of faces that form the convex hull.
        /// </summary>
        private IndexBuffer _convexFaces;

        /// <summary>
        ///     A list of vertices that form the convex hull.
        /// </summary>
        private IndexBuffer _convexHull;

        /// <summary>
        ///     The vertex that is currently being processed.
        /// </summary>
        private int _currentVertex;

        /// <summary>
        ///     Used for VerticesBeyond for faces that are on the convex hull.
        /// </summary>
        private IndexBuffer _emptyBuffer;

        /// <summary>
        ///     A helper variable to help determine the index of the vertex that is furthest from the face that is currently being
        ///     processed.
        /// </summary>
        private int _furthestVertex;

        /// <summary>
        ///     Helper class for handling math related stuff.
        /// </summary>
        private MathHelper _mathHelper;

        /// <summary>
        ///     A helper variable to determine the furthest vertex for a particular convex face.
        /// </summary>
        private double _maxDistance;

        /// <summary>
        ///     Manages the memory allocations and storage of unused objects.
        ///     Saves the garbage collector a lot of work.
        /// </summary>
        private ObjectManager _objectManager;

        private double[] _positions;

        /// <summary>
        ///     Stores a list of "singular" (or "generate", "planar", etc.) vertices that cannot be part of the hull.
        /// </summary>
        private HashSet<int> _singularVertices;

        /// <summary>
        ///     Used to determine which faces need to be updated at each step of the algorithm.
        /// </summary>
        private IndexBuffer _traverseStack;

        /// <summary>
        ///     A list of faces that that are not a part of the final convex hull and still need to be processed.
        /// </summary>
        private FaceList _unprocessedFaces;

        /*
         * Helper arrays to store faces for adjacency update.
         * This is just to prevent unnecessary allocations.
         */
        private int[] _updateBuffer;
        private int[] _updateIndices;
        private bool[] _vertexAdded;
        private bool[] _vertexMarks;
        internal bool[] AffectedFaceFlags;

        /// <summary>
        ///     Corresponds to the dimension of the data.
        ///     When the "lifted" hull is computed, Dimension is automatically incremented by one.
        /// </summary>
        internal int Dimension;

        /*
         * The triangulation faces are represented in a single pool for objects that are being reused.
         * This allows for represent the faces as integers and significantly speeds up many computations.
         * - AffectedFaceFlags are used to mark affected faces/
         */
        internal ConvexFaceInternal[] FacePool;

        /// <summary>
        ///     Wraps the vertices and determines the dimension if it's unknown.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="lift"></param>
        /// <param name="config"></param>
        private ConvexHullInternal(IVertex[] vertices, bool lift, ConvexHullComputationConfig config) {
            if (config.PointTranslationType != PointTranslationType.None && config.PointTranslationGenerator == null) {
                throw new InvalidOperationException("PointTranslationGenerator cannot be null if PointTranslationType is enabled.");
            }

            _isLifted = lift;
            _vertices = vertices;
            _planeDistanceTolerance = config.PlaneDistanceTolerance;

            Dimension = DetermineDimension();
            if (Dimension < 2) throw new InvalidOperationException("Dimension of the input must be 2 or greater.");

            if (lift) Dimension++;
            InitializeData(config);
        }

        /// <summary>
        ///     Computes the Delaunay triangulation.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TCell"></typeparam>
        /// <param name="data"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        internal static TCell[] GetDelaunayTriangulation<TVertex, TCell>(IList<TVertex> data,
            TriangulationComputationConfig config)
            where TCell : TriangulationCell<TVertex, TCell>, new()
            where TVertex : IVertex {
            config = config ?? new TriangulationComputationConfig();

            var vertices = new IVertex[data.Count];
            for (var i = 0; i < data.Count; i++) vertices[i] = data[i];
            var ch = new ConvexHullInternal(vertices, true, config);
            ch.FindConvexHull();
            ch.PostProcessTriangulation(config);

            return ch.GetConvexFaces<TVertex, TCell>();
        }

        /// <summary>
        ///     Remove the upper faces from the hull.
        ///     Remove empty boundary cells if shifting was used.
        /// </summary>
        /// <param name="config"></param>
        private void PostProcessTriangulation(TriangulationComputationConfig config) {
            RemoveUpperFaces();
            if (config.PointTranslationType == PointTranslationType.TranslateInternal) {
                RemoveEmptyBoundaryCells(config.ZeroCellVolumeTolerance);
            }
        }

        /// <summary>
        ///     Removes up facing Tetrahedrons from the triangulation.
        /// </summary>
        private void RemoveUpperFaces() {
            var delaunayFaces = _convexFaces;
            var dimension = Dimension - 1;

            // Remove the "upper" faces
            for (var i = delaunayFaces.Count - 1; i >= 0; i--) {
                var candidateIndex = delaunayFaces[i];
                var candidate = FacePool[candidateIndex];
                if (!(candidate.Normal[dimension] >= 0.0)) continue;
                for (var fi = 0; fi < candidate.AdjacentFaces.Length; fi++) {
                    var af = candidate.AdjacentFaces[fi];
                    if (af < 0) continue;
                    var face = FacePool[af];
                    for (var j = 0; j < face.AdjacentFaces.Length; j++) {
                        if (face.AdjacentFaces[j] == candidateIndex) {
                            face.AdjacentFaces[j] = -1;
                        }
                    }
                }
                delaunayFaces[i] = delaunayFaces[delaunayFaces.Count - 1];
                delaunayFaces.Pop();
            }
        }

        /// <summary>
        ///     Removes the empty boundary cells that might have been created using PointTranslationType.TranslateInternal.
        /// </summary>
        /// <param name="tolerance"></param>
        private void RemoveEmptyBoundaryCells(double tolerance) {
            var faces = _convexFaces;
            var pool = FacePool;
            var dimension = Dimension - 1;

            var visited = new bool[pool.Length];
            var remove = new bool[pool.Length];
            var toTest = new IndexBuffer();

            for (var i = faces.Count - 1; i >= 0; i--) {
                var adj = pool[faces[i]].AdjacentFaces;
                if (adj.Any(t => t < 0)) {
                    toTest.Push(faces[i]);
                }
            }

            var buffer = new double[dimension][];
            for (var i = 0; i < dimension; i++) buffer[i] = new double[dimension];

            var simplexVolumeBuffer = new MathHelper.SimplexVolumeBuffer(dimension);
            while (toTest.Count > 0) {
                var top = toTest.Pop();
                var face = pool[top];
                visited[top] = true;

                if (!(MathHelper.GetSimplexVolume(face, _vertices, simplexVolumeBuffer) < tolerance)) continue;
                remove[top] = true;

                var adj = face.AdjacentFaces;
                foreach (var n in adj.Where(n => n >= 0 && !visited[n])) {
                    toTest.Push(n);
                }
            }

            for (var i = faces.Count - 1; i >= 0; i--) {
                if (!remove[faces[i]]) continue;
                var candidateIndex = faces[i];
                var candidate = pool[candidateIndex];
                for (var fi = 0; fi < candidate.AdjacentFaces.Length; fi++) {
                    var af = candidate.AdjacentFaces[fi];
                    if (af < 0) continue;
                    var face = pool[af];
                    for (var j = 0; j < face.AdjacentFaces.Length; j++) {
                        if (face.AdjacentFaces[j] == candidateIndex) {
                            face.AdjacentFaces[j] = -1;
                        }
                    }
                }

                faces[i] = faces[faces.Count - 1];
                faces.Pop();
            }
        }

        /// <summary>
        ///     This is called by the "ConvexHull" class.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig.GetDefault() is used.</param>
        /// <returns></returns>
        internal static ConvexHull<TVertex, TFace> GetConvexHull<TVertex, TFace>(IList<TVertex> data,
            ConvexHullComputationConfig config)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex {
            config = config ?? new ConvexHullComputationConfig();

            var vertices = new IVertex[data.Count];
            for (var i = 0; i < data.Count; i++) vertices[i] = data[i];
            var ch = new ConvexHullInternal(vertices, false, config);
            ch.FindConvexHull();

            var hull = new TVertex[ch._convexHull.Count];
            for (var i = 0; i < hull.Length; i++) {
                hull[i] = (TVertex)ch._vertices[ch._convexHull[i]];
            }

            return new ConvexHull<TVertex, TFace> { Points = hull, Faces = ch.GetConvexFaces<TVertex, TFace>() };
        }

        /// <summary>
        ///     Finds the convex hull and creates the TFace objects.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <returns></returns>
        private TFace[] GetConvexFaces<TVertex, TFace>()
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex {
            var faces = _convexFaces;
            var cellCount = faces.Count;
            var cells = new TFace[cellCount];

            for (var i = 0; i < cellCount; i++) {
                var face = FacePool[faces[i]];
                var vertices = new TVertex[Dimension];
                for (var j = 0; j < Dimension; j++) {
                    vertices[j] = (TVertex)_vertices[face.Vertices[j]];
                }

                cells[i] = new TFace {
                    Vertices = vertices,
                    Adjacency = new TFace[Dimension],
                    Normal = _isLifted ? null : face.Normal
                };
                face.Tag = i;
            }

            for (var i = 0; i < cellCount; i++) {
                var face = FacePool[faces[i]];
                var cell = cells[i];
                for (var j = 0; j < Dimension; j++) {
                    if (face.AdjacentFaces[j] < 0) continue;
                    cell.Adjacency[j] = cells[FacePool[face.AdjacentFaces[j]].Tag];
                }

                // Fix the vertex orientation.
                if (!face.IsNormalFlipped) continue;
                var tempVert = cell.Vertices[0];
                cell.Vertices[0] = cell.Vertices[Dimension - 1];
                cell.Vertices[Dimension - 1] = tempVert;

                var tempAdj = cell.Adjacency[0];
                cell.Adjacency[0] = cell.Adjacency[Dimension - 1];
                cell.Adjacency[Dimension - 1] = tempAdj;
            }

            return cells;
        }

        /// <summary>
        ///     Check the dimensionality of the input data.
        /// </summary>
        private int DetermineDimension() {
            var r = new Random();
            var vCount = _vertices.Length;
            var dimensions = new List<int>();
            for (var i = 0; i < 10; i++)
                dimensions.Add(_vertices[r.Next(vCount)].Position.Length);
            var dimension = dimensions.Min();
            if (dimension != dimensions.Max()) throw new ArgumentException("Invalid input data (non-uniform dimension).");
            return dimension;
        }

        /// <summary>
        ///     Create the first faces from (dimension + 1) vertices.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> CreateInitialHull() {
            var faces = new int[Dimension + 1];

            for (var i = 0; i < Dimension + 1; i++) {
                var vertices = new int[Dimension];
                for (int j = 0, k = 0; j <= Dimension; j++) {
                    if (i != j) vertices[k++] = _convexHull[j];
                }
                var newFace = FacePool[_objectManager.GetFace()];
                newFace.Vertices = vertices;
                Array.Sort(vertices);
                _mathHelper.CalculateFacePlane(newFace, _center);
                faces[i] = newFace.Index;
            }

            // update the adjacency (check all pairs of faces)
            for (var i = 0; i < Dimension; i++) {
                for (var j = i + 1; j < Dimension + 1; j++) UpdateAdjacency(FacePool[faces[i]], FacePool[faces[j]]);
            }

            return faces;
        }

        /// <summary>
        ///     Check if 2 faces are adjacent and if so, update their AdjacentFaces array.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        private void UpdateAdjacency(ConvexFaceInternal l, ConvexFaceInternal r) {
            var lv = l.Vertices;
            var rv = r.Vertices;
            int i;

            // reset marks on the 1st face
            for (i = 0; i < lv.Length; i++) _vertexMarks[lv[i]] = false;

            // mark all vertices on the 2nd face
            for (i = 0; i < rv.Length; i++) _vertexMarks[rv[i]] = true;

            // find the 1st false index
            for (i = 0; i < lv.Length; i++) if (!_vertexMarks[lv[i]]) break;

            // no vertex was marked
            if (i == Dimension) return;

            // check if only 1 vertex wasn't marked
            for (var j = i + 1; j < lv.Length; j++) if (!_vertexMarks[lv[j]]) return;

            // if we are here, the two faces share an edge
            l.AdjacentFaces[i] = r.Index;

            // update the adj. face on the other face - find the vertex that remains marked
            for (i = 0; i < lv.Length; i++) _vertexMarks[lv[i]] = false;
            for (i = 0; i < rv.Length; i++) {
                if (_vertexMarks[rv[i]]) break;
            }
            r.AdjacentFaces[i] = l.Index;
        }

        /*
                void InitSmall()
                {
                    for (int i = 0; i < Vertices.Length; i++)
                    {
                        ConvexHull.Add(i);
                    }
                }
        */

        /// <summary>
        ///     Init the hull if Vertices.Length == Dimension.
        /// </summary>
        private void InitSingle() {
            var vertices = new int[Dimension];
            for (var i = 0; i < _vertices.Length; i++) {
                vertices[i] = i;
                _convexHull.Add(i);
            }

            var newFace = FacePool[_objectManager.GetFace()];
            newFace.Vertices = vertices;
            Array.Sort(vertices);
            _mathHelper.CalculateFacePlane(newFace, _center);

            // Make sure the normal point downwards in case this is used for triangulation
            if (newFace.Normal[Dimension - 1] >= 0.0) {
                for (var i = 0; i < Dimension; i++) {
                    newFace.Normal[i] *= -1.0;
                }
                newFace.Offset = -newFace.Offset;
                newFace.IsNormalFlipped = !newFace.IsNormalFlipped;
            }

            _convexFaces.Add(newFace.Index);
        }

        /// <summary>
        ///     Find the (dimension+1) initial points and create the simplexes.
        /// </summary>
        private void InitConvexHull() {
            if (_vertices.Length < Dimension) {
                // In this case, there cannot be a single convex face, so we return an empty result.
                return;
            }
            if (_vertices.Length == Dimension) {
                // The vertices are all on the hull and form a single simplex.
                InitSingle();
                return;
            }

            var extremes = FindExtremes();
            var initialPoints = FindInitialPoints(extremes);

            // Add the initial points to the convex hull.
            foreach (var vertex in initialPoints) {
                _currentVertex = vertex;
                // update center must be called before adding the vertex.
                UpdateCenter();
                AddConvexVertex(vertex);

                // Mark the vertex so that it's not included in any beyond set.
                _vertexMarks[vertex] = true;
            }

            // Create the initial simplexes.
            var faces = CreateInitialHull();

            // Init the vertex beyond buffers.
            foreach (var face in faces.Select(faceIndex => FacePool[faceIndex])) {
                FindBeyondVertices(face);
                if (face.VerticesBeyond.Count == 0) _convexFaces.Add(face.Index); // The face is on the hull
                else _unprocessedFaces.Add(face);
            }

            // Unmark the vertices
            foreach (var vertex in initialPoints) _vertexMarks[vertex] = false;
        }

        /// <summary>
        ///     Used in the "initialization" code.
        /// </summary>
        private void FindBeyondVertices(ConvexFaceInternal face) {
            var beyondVertices = face.VerticesBeyond;

            _maxDistance = double.NegativeInfinity;
            _furthestVertex = 0;

            var count = _vertices.Length;
            for (var i = 0; i < count; i++) {
                if (_vertexMarks[i]) continue;
                IsBeyond(face, beyondVertices, i);
            }

            face.FurthestVertex = _furthestVertex;
        }

        /// <summary>
        ///     Finds (dimension + 1) initial points.
        /// </summary>
        /// <param name="extremes"></param>
        /// <returns></returns>
        private List<int> FindInitialPoints(IList<int> extremes) {
            var initialPoints = new List<int>();

            int first = -1, second = -1;
            double maxDist = 0;
            var temp = new double[Dimension];
            for (var i = 0; i < extremes.Count - 1; i++) {
                var a = extremes[i];
                for (var j = i + 1; j < extremes.Count; j++) {
                    var b = extremes[j];
                    _mathHelper.SubtractFast(a, b, temp);
                    var dist = MathHelper.LengthSquared(temp);
                    if (!(dist > maxDist)) continue;
                    first = a;
                    second = b;
                    maxDist = dist;
                }
            }

            initialPoints.Add(first);
            initialPoints.Add(second);

            for (var i = 2; i <= Dimension; i++) {
                var maximum = double.NegativeInfinity;
                var maxPoint = -1;
                foreach (var extreme in extremes) {
                    if (initialPoints.Contains(extreme)) continue;

                    var val = GetSquaredDistanceSum(extreme, initialPoints);

                    if (!(val > maximum)) continue;
                    maximum = val;
                    maxPoint = extreme;
                }

                if (maxPoint >= 0) initialPoints.Add(maxPoint);
                else {
                    var vCount = _vertices.Length;
                    for (var j = 0; j < vCount; j++) {
                        if (initialPoints.Contains(j)) continue;

                        var val = GetSquaredDistanceSum(j, initialPoints);

                        if (!(val > maximum)) continue;
                        maximum = val;
                        maxPoint = j;
                    }

                    if (maxPoint >= 0) initialPoints.Add(maxPoint);
                    else ThrowSingular();
                }
            }
            return initialPoints;
        }

        /// <summary>
        ///     Computes the sum of square distances to the initial points.
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="initialPoints"></param>
        /// <returns></returns>
        private double GetSquaredDistanceSum(int pivot, IList<int> initialPoints) {
            var initPtsNum = initialPoints.Count;
            var sum = 0.0;

            for (var i = 0; i < initPtsNum; i++) {
                var initPt = initialPoints[i];
                for (var j = 0; j < Dimension; j++) {
                    var t = GetCoordinate(initPt, j) - GetCoordinate(pivot, j);
                    sum += t * t;
                }
            }

            return sum;
        }

        private int LexCompare(int u, int v) {
            int uOffset = u * Dimension, vOffset = v * Dimension;
            for (var i = 0; i < Dimension; i++) {
                double x = _positions[uOffset + i], y = _positions[vOffset + i];
                var comp = x.CompareTo(y);
                if (comp != 0) return comp;
            }
            return 0;
        }

        /// <summary>
        ///     Finds the extremes in all dimensions.
        /// </summary>
        /// <returns></returns>
        private List<int> FindExtremes() {
            var extremes = new List<int>(2 * Dimension);

            var vCount = _vertices.Length;
            for (var i = 0; i < Dimension; i++) {
                double min = double.MaxValue, max = double.MinValue;
                int minInd = 0, maxInd = 0;
                for (var j = 0; j < vCount; j++) {
                    var v = GetCoordinate(j, i);
                    var diff = min - v;
                    if (diff >= 0.0) {
                        // if the extreme is a possibly the planar position, we take the lex. bigger one.
                        if (diff < _planeDistanceTolerance) {
                            if (LexCompare(j, minInd) > 0) {
                                min = v;
                                minInd = j;
                            }
                        } else {
                            min = v;
                            minInd = j;
                        }
                    }

                    diff = v - max;
                    if (!(diff >= 0.0)) continue;
                    if (diff < _planeDistanceTolerance) {
                        if (LexCompare(j, maxInd) <= 0) continue;
                        max = v;
                        maxInd = j;
                    } else {
                        max = v;
                        maxInd = j;
                    }
                }

                if (minInd != maxInd) {
                    extremes.Add(minInd);
                    extremes.Add(maxInd);
                } else extremes.Add(minInd);
            }

            // Do we have enough unique extreme points?
            var set = new HashSet<int>(extremes);
            if (set.Count <= Dimension) {
                // If not, just add the "first" non-included ones.
                var i = 0;
                while (i < vCount && set.Count <= Dimension) {
                    set.Add(i);
                    i++;
                }
            }

            return set.ToList();
        }

        /// <summary>
        ///     The exception thrown if singular input data detected.
        /// </summary>
        private static void ThrowSingular() {
            throw new InvalidOperationException(
                "Singular input data (i.e. trying to triangulate a data that contain a regular lattice of points) detected. "
                + "Introducing some noise to the data might resolve the issue.");
        }

        /// <summary>
        ///     Tags all faces seen from the current vertex with 1.
        /// </summary>
        /// <param name="currentFace"></param>
        private void TagAffectedFaces(ConvexFaceInternal currentFace) {
            _affectedFaceBuffer.Clear();
            _affectedFaceBuffer.Add(currentFace.Index);
            TraverseAffectedFaces(currentFace.Index);
        }

        /// <summary>
        ///     Recursively traverse all the relevant faces.
        /// </summary>
        private void TraverseAffectedFaces(int currentFace) {
            _traverseStack.Clear();
            _traverseStack.Push(currentFace);
            AffectedFaceFlags[currentFace] = true;

            while (_traverseStack.Count > 0) {
                var top = FacePool[_traverseStack.Pop()];
                for (var i = 0; i < Dimension; i++) {
                    var adjFace = top.AdjacentFaces[i];

                    if (AffectedFaceFlags[adjFace] ||
                        !(_mathHelper.GetVertexDistance(_currentVertex, FacePool[adjFace]) >= _planeDistanceTolerance))
                        continue;
                    _affectedFaceBuffer.Add(adjFace);
                    AffectedFaceFlags[adjFace] = true;
                    _traverseStack.Push(adjFace);
                }
            }
        }

        /// <summary>
        ///     Creates a new deferred face.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="faceIndex"></param>
        /// <param name="pivot"></param>
        /// <param name="pivotIndex"></param>
        /// <param name="oldFace"></param>
        /// <returns></returns>
        private DeferredFace MakeDeferredFace(ConvexFaceInternal face, int faceIndex, ConvexFaceInternal pivot, int pivotIndex,
            ConvexFaceInternal oldFace) {
            var ret = _objectManager.GetDeferredFace();

            ret.Face = face;
            ret.FaceIndex = faceIndex;
            ret.Pivot = pivot;
            ret.PivotIndex = pivotIndex;
            ret.OldFace = oldFace;

            return ret;
        }

        /// <summary>
        ///     Connect faces using a connector.
        /// </summary>
        /// <param name="connector"></param>
        private void ConnectFace(FaceConnector connector) {
            var index = connector.HashCode % CONNECTOR_TABLE_SIZE;
            var list = _connectorTable[index];

            for (var current = list.First; current != null; current = current.Next) {
                if (!FaceConnector.AreConnectable(connector, current, Dimension)) continue;
                list.Remove(current);
                FaceConnector.Connect(current, connector);
                current.Face = null;
                connector.Face = null;
                _objectManager.DepositConnector(current);
                _objectManager.DepositConnector(connector);
                return;
            }

            list.Add(connector);
        }

        /// <summary>
        ///     Removes the faces "covered" by the current vertex and adds the newly created ones.
        /// </summary>
        private bool CreateCone() {
            var currentVertexIndex = _currentVertex;
            _coneFaceBuffer.Clear();

            for (var fIndex = 0; fIndex < _affectedFaceBuffer.Count; fIndex++) {
                var oldFaceIndex = _affectedFaceBuffer[fIndex];
                var oldFace = FacePool[oldFaceIndex];

                // Find the faces that need to be updated
                var updateCount = 0;
                for (var i = 0; i < Dimension; i++) {
                    var af = oldFace.AdjacentFaces[i];
                    if (AffectedFaceFlags[af]) continue;
                    _updateBuffer[updateCount] = af;
                    _updateIndices[updateCount] = i;
                    ++updateCount;
                }

                for (var i = 0; i < updateCount; i++) {
                    var adjacentFace = FacePool[_updateBuffer[i]];

                    var oldFaceAdjacentIndex = 0;
                    var adjFaceAdjacency = adjacentFace.AdjacentFaces;
                    for (var j = 0; j < adjFaceAdjacency.Length; j++) {
                        if (oldFaceIndex != adjFaceAdjacency[j]) continue;
                        oldFaceAdjacentIndex = j;
                        break;
                    }

                    var forbidden = _updateIndices[i]; // Index of the face that corresponds to this adjacent face

                    int oldVertexIndex;

                    var newFaceIndex = _objectManager.GetFace();
                    var newFace = FacePool[newFaceIndex];
                    var vertices = newFace.Vertices;
                    for (var j = 0; j < Dimension; j++) vertices[j] = oldFace.Vertices[j];
                    oldVertexIndex = vertices[forbidden];

                    int orderedPivotIndex;

                    // correct the ordering
                    if (currentVertexIndex < oldVertexIndex) {
                        orderedPivotIndex = 0;
                        for (var j = forbidden - 1; j >= 0; j--) {
                            if (vertices[j] > currentVertexIndex) vertices[j + 1] = vertices[j];
                            else {
                                orderedPivotIndex = j + 1;
                                break;
                            }
                        }
                    } else {
                        orderedPivotIndex = Dimension - 1;
                        for (var j = forbidden + 1; j < Dimension; j++) {
                            if (vertices[j] < currentVertexIndex) vertices[j - 1] = vertices[j];
                            else {
                                orderedPivotIndex = j - 1;
                                break;
                            }
                        }
                    }

                    vertices[orderedPivotIndex] = _currentVertex;

                    if (!_mathHelper.CalculateFacePlane(newFace, _center)) {
                        return false;
                    }

                    _coneFaceBuffer.Add(MakeDeferredFace(newFace, orderedPivotIndex, adjacentFace, oldFaceAdjacentIndex, oldFace));
                }
            }

            return true;
        }

        /// <summary>
        ///     Commits a cone and adds a vertex to the convex hull.
        /// </summary>
        private void CommitCone() {
            // Add the current vertex.
            AddConvexVertex(_currentVertex);

            // Fill the adjacency.
            for (var i = 0; i < _coneFaceBuffer.Count; i++) {
                var face = _coneFaceBuffer[i];

                var newFace = face.Face;
                var adjacentFace = face.Pivot;
                var oldFace = face.OldFace;
                var orderedPivotIndex = face.FaceIndex;

                newFace.AdjacentFaces[orderedPivotIndex] = adjacentFace.Index;
                adjacentFace.AdjacentFaces[face.PivotIndex] = newFace.Index;

                // let there be a connection.
                for (var j = 0; j < Dimension; j++) {
                    if (j == orderedPivotIndex) continue;
                    var connector = _objectManager.GetConnector();
                    connector.Update(newFace, j, Dimension);
                    ConnectFace(connector);
                }

                // the id adjacent face on the hull? If so, we can use simple method to find beyond vertices.
                if (adjacentFace.VerticesBeyond.Count == 0) {
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond);
                }
                // it is slightly more effective if the face with the lower number of beyond vertices comes first.
                else if (adjacentFace.VerticesBeyond.Count < oldFace.VerticesBeyond.Count) {
                    FindBeyondVertices(newFace, adjacentFace.VerticesBeyond, oldFace.VerticesBeyond);
                } else {
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond, adjacentFace.VerticesBeyond);
                }

                // This face will definitely lie on the hull
                if (newFace.VerticesBeyond.Count == 0) {
                    _convexFaces.Add(newFace.Index);
                    _unprocessedFaces.Remove(newFace);
                    _objectManager.DepositVertexBuffer(newFace.VerticesBeyond);
                    newFace.VerticesBeyond = _emptyBuffer;
                } else // Add the face to the list
                  {
                    _unprocessedFaces.Add(newFace);
                }

                // recycle the object.
                _objectManager.DepositDeferredFace(face);
            }

            // Recycle the affected faces.
            for (var fIndex = 0; fIndex < _affectedFaceBuffer.Count; fIndex++) {
                var face = _affectedFaceBuffer[fIndex];
                _unprocessedFaces.Remove(FacePool[face]);
                _objectManager.DepositFace(face);
            }
        }

        /// <summary>
        ///     Check whether the vertex v is beyond the given face. If so, add it to beyondVertices.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="beyondVertices"></param>
        /// <param name="v"></param>
        private void IsBeyond(ConvexFaceInternal face, IndexBuffer beyondVertices, int v) {
            var distance = _mathHelper.GetVertexDistance(v, face);
            if (distance >= _planeDistanceTolerance) {
                if (distance > _maxDistance) {
                    // If it's within the tolerance distance, use the lex. larger point
                    if (distance - _maxDistance < _planeDistanceTolerance) {
                        if (LexCompare(v, _furthestVertex) > 0) {
                            _maxDistance = distance;
                            _furthestVertex = v;
                        }
                    } else {
                        _maxDistance = distance;
                        _furthestVertex = v;
                    }
                }
                beyondVertices.Add(v);
            }
        }

        /// <summary>
        ///     Used by update faces.
        /// </summary>
        private void FindBeyondVertices(ConvexFaceInternal face, IndexBuffer beyond, IndexBuffer beyond1) {
            var beyondVertices = _beyondBuffer;

            _maxDistance = double.NegativeInfinity;
            _furthestVertex = 0;
            int v;

            for (var i = 0; i < beyond1.Count; i++) _vertexMarks[beyond1[i]] = true;
            _vertexMarks[_currentVertex] = false;
            for (var i = 0; i < beyond.Count; i++) {
                v = beyond[i];
                if (v == _currentVertex) continue;
                _vertexMarks[v] = false;
                IsBeyond(face, beyondVertices, v);
            }

            for (var i = 0; i < beyond1.Count; i++) {
                v = beyond1[i];
                if (_vertexMarks[v]) IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = _furthestVertex;

            // Pull the old switch a roo (switch the face beyond buffers)
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            _beyondBuffer = temp;
        }

        private void FindBeyondVertices(ConvexFaceInternal face, IndexBuffer beyond) {
            var beyondVertices = _beyondBuffer;

            _maxDistance = double.NegativeInfinity;
            _furthestVertex = 0;

            for (var i = 0; i < beyond.Count; i++) {
                var v = beyond[i];
                if (v == _currentVertex) continue;
                IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = _furthestVertex;

            // Pull the old switch a roo (switch the face beyond buffers)
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            _beyondBuffer = temp;
        }

        /// <summary>
        ///     Recalculates the centroid of the current hull.
        /// </summary>
        private void UpdateCenter() {
            var count = _convexHull.Count + 1;
            for (var i = 0; i < Dimension; i++) _center[i] *= (count - 1);
            var f = 1.0 / count;
            var co = _currentVertex * Dimension;
            for (var i = 0; i < Dimension; i++) _center[i] = f * (_center[i] + _positions[co + i]);
        }

        /// <summary>
        ///     Removes the last vertex from the center.
        /// </summary>
        private void RollbackCenter() {
            var count = _convexHull.Count + 1;
            for (var i = 0; i < Dimension; i++) _center[i] *= count;
            var f = 1.0 / (count - 1);
            var co = _currentVertex * Dimension;
            for (var i = 0; i < Dimension; i++) _center[i] = f * (_center[i] - _positions[co + i]);
        }

        /// <summary>
        ///     Handles singular vertex.
        /// </summary>
        private void HandleSingular() {
            RollbackCenter();
            _singularVertices.Add(_currentVertex);

            // This means that all the affected faces must be on the hull and that all their "vertices beyond" are singular.
            for (var fIndex = 0; fIndex < _affectedFaceBuffer.Count; fIndex++) {
                var face = FacePool[_affectedFaceBuffer[fIndex]];
                var vb = face.VerticesBeyond;
                for (var i = 0; i < vb.Count; i++) {
                    _singularVertices.Add(vb[i]);
                }

                _convexFaces.Add(face.Index);
                _unprocessedFaces.Remove(face);
                _objectManager.DepositVertexBuffer(face.VerticesBeyond);
                face.VerticesBeyond = _emptyBuffer;
            }
        }

        /// <summary>
        ///     Fins the convex hull.
        /// </summary>
        private void FindConvexHull() {
            // Find the (dimension+1) initial points and create the simplexes.
            InitConvexHull();

            // Expand the convex hull and faces.
            while (_unprocessedFaces.First != null) {
                var currentFace = _unprocessedFaces.First;
                _currentVertex = currentFace.FurthestVertex;

                UpdateCenter();

                // The affected faces get tagged
                TagAffectedFaces(currentFace);

                // Create the cone from the currentVertex and the affected faces horizon.
                if (!_singularVertices.Contains(_currentVertex) && CreateCone()) CommitCone();
                else HandleSingular();

                // Need to reset the tags
                var count = _affectedFaceBuffer.Count;
                for (var i = 0; i < count; i++) AffectedFaceFlags[_affectedFaceBuffer[i]] = false;
            }
        }

        /// <summary>
        ///     Initialize buffers and lists.
        /// </summary>
        /// <param name="config"></param>
        private void InitializeData(ConvexHullComputationConfig config) {
            _convexHull = new IndexBuffer();
            _unprocessedFaces = new FaceList();
            _convexFaces = new IndexBuffer();

            FacePool = new ConvexFaceInternal[(Dimension + 1) * 10]; // must be initialized before object manager
            AffectedFaceFlags = new bool[(Dimension + 1) * 10];
            _objectManager = new ObjectManager(this);

            _center = new double[Dimension];
            _traverseStack = new IndexBuffer();
            _updateBuffer = new int[Dimension];
            _updateIndices = new int[Dimension];
            _emptyBuffer = new IndexBuffer();
            _affectedFaceBuffer = new IndexBuffer();
            _coneFaceBuffer = new SimpleList<DeferredFace>();
            _singularVertices = new HashSet<int>();
            _beyondBuffer = new IndexBuffer();

            _connectorTable = new ConnectorList[CONNECTOR_TABLE_SIZE];
            for (var i = 0; i < CONNECTOR_TABLE_SIZE; i++) _connectorTable[i] = new ConnectorList();

            _vertexMarks = new bool[_vertices.Length];
            _vertexAdded = new bool[_vertices.Length];
            InitializePositions(config);

            _mathHelper = new MathHelper(Dimension, _positions);
        }

        /// <summary>
        ///     Initialize the vertex positions based on the translation type from config.
        /// </summary>
        /// <param name="config"></param>
        private void InitializePositions(ConvexHullComputationConfig config) {
            _positions = new double[_vertices.Length * Dimension];
            var index = 0;
            if (_isLifted) {
                var origDim = Dimension - 1;
                var tf = config.PointTranslationGenerator;
                switch (config.PointTranslationType) {
                    case PointTranslationType.None:
                        foreach (var v in _vertices) {
                            var lifted = 0.0;
                            for (var i = 0; i < origDim; i++) {
                                var t = v.Position[i];
                                _positions[index++] = t;
                                lifted += t * t;
                            }
                            _positions[index++] = lifted;
                        }
                        break;

                    case PointTranslationType.TranslateInternal:
                        foreach (var v in _vertices) {
                            var lifted = 0.0;
                            for (var i = 0; i < origDim; i++) {
                                var t = v.Position[i] + tf();
                                _positions[index++] = t;
                                lifted += t * t;
                            }
                            _positions[index++] = lifted;
                        }
                        break;
                }
            } else {
                var tf = config.PointTranslationGenerator;
                switch (config.PointTranslationType) {
                    case PointTranslationType.None:
                        foreach (var v in _vertices) {
                            for (var i = 0; i < Dimension; i++) _positions[index++] = v.Position[i];
                        }
                        break;

                    case PointTranslationType.TranslateInternal:
                        foreach (var v in _vertices) {
                            for (var i = 0; i < Dimension; i++) _positions[index++] = v.Position[i] + tf();
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///     Get a vertex coordinate. Only used in the initialize functions,
        ///     in other places it part v * Dimension + i is inlined.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private double GetCoordinate(int v, int i) {
            return _positions[v * Dimension + i];
        }

        /// <summary>
        ///     Check if the vertex was already added and if not, add it.
        /// </summary>
        /// <param name="i"></param>
        private void AddConvexVertex(int i) {
            if (_vertexAdded[i]) return;
            _convexHull.Add(i);
            _vertexAdded[i] = true;
        }
    }
}