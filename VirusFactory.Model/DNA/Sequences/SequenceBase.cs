using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirusFactory.Model.DNA.Sequences {

    public abstract class SequenceBase<T> {
        protected List<BasePair> InternalBasePairs = new List<BasePair>();

        public IReadOnlyList<BasePair> BasePairs => InternalBasePairs;

        protected T CachedValue = default(T);

        public abstract T GetValue();

        public abstract void SetValue(T value);

        public override string ToString() {
            return InternalBasePairs.Aggregate(new StringBuilder(), (x, y) => x.Append($"{y}-")).ToString().Trim('-');
        }

        public string GetTopLine() {
            return InternalBasePairs.Aggregate(new StringBuilder(), (x, o) => x.Append(o.TopValue)).ToString();
        }

        public string GetBottomLine() {
            return InternalBasePairs.Aggregate(new StringBuilder(), (x, o) => x.Append(o.BottomValue)).ToString();
        }
    }
}