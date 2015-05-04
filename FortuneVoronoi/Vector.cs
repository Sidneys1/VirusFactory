using System;
using System.Collections;
using System.Linq;

namespace FortuneVoronoi {
    /// <summary>
    /// A vector class, implementing all interesting features of vectors
    /// </summary>
    public class Vector : IEnumerable, IComparable {
        /// <summary>
        /// Global precision for any calculation
        /// </summary>
        public static readonly int Precision = 10;

        readonly double[] _data;
        public object Tag = null;
        /// <summary>
        /// Build a new vector
        /// </summary>
        /// <param name="dim">The dimension</param>
        public Vector(int dim) {
            _data = new double[dim];
        }
        /// <summary>
        /// Build a new vector
        /// </summary>
        /// <param name="x">The elements of the vector</param>
        public Vector(params double[] x) {
            _data = new double[x.Length];
            x.CopyTo(_data, 0);
        }
        /// <summary>
        /// Build a new vector as a copy of an existing one
        /// </summary>
        /// <param name="o">The existing vector</param>
        public Vector(Vector o)
            : this(o._data) { }
        /// <summary>
        /// Build a new vector from a string
        /// </summary>
        /// <param name="s">A string, as produced by ToString</param>
        public Vector(string s) {
            if (s[0] != '(' || s[s.Length - 1] != ')')
                throw new Exception("Formatfehler!");
            var p = MathTools.HighLevelSplit(s.Substring(1, s.Length - 2), ';');
            _data = new double[p.Length];
            int i;
            for (i = 0; i < _data.Length; i++) {
                _data[i] = Convert.ToDouble(p[i]);
            }
        }
        /// <summary>
        /// Gets or sets the value of the vector at the given index
        /// </summary>
        public double this[int i] {
            get {
                return _data[i];
            }
            set {
                _data[i] = Math.Round(value, Precision);
            }
        }

        /// <summary>
        /// The dimension of the vector
        /// </summary>
        public int Dim => _data.Length;

        /// <summary>
        /// The squared length of the vector
        /// </summary>
        public double SquaredLength => this * this;

        /// <summary>
        /// The sum of all elements in the vector
        /// </summary>
        public double ElementSum {
            get {
                int i;
                double e = 0;
                for (i = 0; i < Dim; i++)
                    e += _data[i];
                return e;
            }
        }
        /// <summary>
        /// Reset all elements with ransom values from the given range
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        public void Randomize(double min, double max) {
            int i;
            for (i = 0; i < _data.Length; i++) {
                this[i] = min + (max - min) * MathTools.R.NextDouble();
            }
        }
        /// <summary>
        /// Reset all elements with ransom values from the given range
        /// </summary>
        /// <param name="minMax">MinMax[0] - Min
        /// MinMax[1] - Max</param>
        public void Randomize(Vector[] minMax) {
            int i;
            for (i = 0; i < _data.Length; i++) {
                this[i] = minMax[0][i] + (minMax[1][i] - minMax[0][i]) * MathTools.R.NextDouble();
            }
        }
        /// <summary>
        /// Scale all elements by r
        /// </summary>
        /// <param name="r">The scalar</param>
        public void Multiply(double r) {
            int i;
            for (i = 0; i < _data.Length; i++) {
                this[i] *= r;
            }
        }
        /// <summary>
        /// Add another vector
        /// </summary>
        /// <param name="v">V</param>
        public void Add(Vector v) {
            int i;
            for (i = 0; i < _data.Length; i++) {
                this[i] += v[i];
            }
        }
        /// <summary>
        /// Add a constant to all elements
        /// </summary>
        /// <param name="d">The constant</param>
        public void Add(double d) {
            int i;
            for (i = 0; i < _data.Length; i++) {
                this[i] += d;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return _data.GetEnumerator();
        }

        /// <summary>
        /// Convert the vector into a reconstructable string representation
        /// </summary>
        /// <returns>A string from which the vector can be rebuilt</returns>
        public override string ToString() {
            var s = "(";
            int i;
            for (i = 0; i < _data.Length; i++) {
                s += _data[i].ToString("G4");
                if (i < _data.Length - 1)
                    s += ";";
            }
            s += ")";
            return s;
        }

        /// <summary>
        /// Compares this vector with another one
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            var b = obj as Vector;
            if (b == null || _data.Length != b._data.Length)
                return false;
            int i;
            for (i = 0; i < _data.Length; i++) {
                if (/*!data[i].Equals(B.data[i]) && */Math.Abs(_data[i] - b._data[i]) > 1e-10)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves a hashcode that is dependent on the elements
        /// </summary>
        /// <returns>The hashcode</returns>
        public override int GetHashCode() {
            return _data.Aggregate(0, (current, d) => current ^ Math.Round(d, Precision).GetHashCode());
        }

        /// <summary>
        /// Subtract two vectors
        /// </summary>
        public static Vector operator -(Vector a, Vector b) {
            if (a.Dim != b.Dim)
                throw new Exception("Vectors of different dimension!");
            var erg = new Vector(a.Dim);
            int i;
            for (i = 0; i < a.Dim; i++)
                erg[i] = a[i] - b[i];
            return erg;
        }

        /// <summary>
        /// Add two vectors
        /// </summary>
        public static Vector operator +(Vector a, Vector b) {
            if (a.Dim != b.Dim)
                throw new Exception("Vectors of different dimension!");
            var erg = new Vector(a.Dim);
            int i;
            for (i = 0; i < a.Dim; i++)
                erg[i] = a[i] + b[i];
            return erg;
        }

        /// <summary>
        /// Get the scalar product of two vectors
        /// </summary>
        public static double operator *(Vector a, Vector b) {
            if (a.Dim != b.Dim)
                throw new Exception("Vectors of different dimension!");
            double erg = 0;
            int i;
            for (i = 0; i < a.Dim; i++)
                erg += a[i] * b[i];
            return erg;
        }

        /// <summary>
        /// Scale one vector
        /// </summary>
        public static Vector operator *(Vector a, double b) {
            var erg = new Vector(a.Dim);
            int i;
            for (i = 0; i < a.Dim; i++)
                erg[i] = a[i] * b;
            return erg;
        }

        /// <summary>
        /// Scale one vector
        /// </summary>
        public static Vector operator *(double a, Vector b) {
            return b * a;
        }
        /// <summary>
        /// Interprete the vector as a double-array
        /// </summary>
        public static explicit operator double[] (Vector a) {
            return a._data;
        }
        /// <summary>
        /// Get the distance of two vectors
        /// </summary>
        public static double Dist(Vector v1, Vector v2) {
            if (v1.Dim != v2.Dim)
                return -1;
            int i;
            double e = 0;
            for (i = 0; i < v1.Dim; i++) {
                var d = (v1[i] - v2[i]);
                e += d * d;
            }
            return e;
        }

        /// <summary>
        /// Compare two vectors
        /// </summary>
        public int CompareTo(object obj) {
            var a = this;
            var b = obj as Vector;
            if (b == null)
                return 0;
            var al = a.SquaredLength;
            var bl = b.SquaredLength;
            if (al > bl)
                return 1;
            if (al < bl)
                return -1;
            int i;
            for (i = 0; i < a.Dim; i++) {
                if (a[i] > b[i])
                    return 1;
                if (a[i] < b[i])
                    return -1;
            }
            return 0;
        }
        /// <summary>
        /// Get a copy of one vector
        /// </summary>
        /// <returns></returns>
        public virtual Vector Clone() {
            return new Vector(_data);
        }
    }
}