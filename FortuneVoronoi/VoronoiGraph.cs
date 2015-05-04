// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable PossibleUnintendedReferenceComparison

namespace FortuneVoronoi {

    public class VoronoiGraph {
        public HashSet<Vector> Vertizes = new HashSet<Vector>();
        public HashSet<VoronoiEdge> Edges = new HashSet<VoronoiEdge>();
    }

    // VoronoiVertex or VoronoiDataPoint are represented as Vector
}