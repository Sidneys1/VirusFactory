// ReSharper disable CompareOfFloatsByEqualityOperator

using ProtoBuf;

namespace VirusFactory.Model.Geography {

    [ProtoContract]
    public struct Point {

        public Point(double x, double y) {
            Y = y;
            X = x;
        }

        [ProtoMember(1)]
        public readonly double X;

        //[FieldOffset(sizeof(double))]
        [ProtoMember(2)]
        public readonly double Y;

        //[FieldOffset(0)]
        //public fixed double Position[2];

        public override string ToString() => $"({X},{Y})";

        public override bool Equals(object obj) {
            if (!(obj is Point)) return false;
            var p = (Point)obj;

            return (X == p.X) && (Y == p.Y);
        }

        public override int GetHashCode() {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}