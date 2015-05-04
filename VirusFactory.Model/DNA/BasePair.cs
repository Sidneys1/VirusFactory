using VirusFactory.Model.DNA.Enums;

namespace VirusFactory.Model.DNA {
    public class BasePair {
        #region Properties and Variables

        private byte _cachedValue;
        private DnaValue _topValue = DnaValue.AA;
        private DnaValue _bottomValue = DnaValue.AA;

        public DnaValue TopValue {
            get { return _topValue; }
            set {
                _topValue = value;
                RecalcCachedValue();
            }
        }

        public DnaValue BottomValue {
            get { return _bottomValue; }
            set {
                _bottomValue = value;
                RecalcCachedValue();
            }
        }

        #endregion Properties and Variables

        public BasePair() {
        }

        public BasePair(byte b) {
            SetValue(b);
        }

        private void RecalcCachedValue() {
            _cachedValue = (byte)(((byte)_topValue << 4) | (byte)_bottomValue);
        }

        public void SetValue(byte b) {
            _topValue = (DnaValue)(b >> 4);
            _bottomValue = (DnaValue)(b & 0x0F);
            RecalcCachedValue();
        }

        #region Conversions

        public static implicit operator byte (BasePair b) {
            return b._cachedValue;
        }

        public static implicit operator char (BasePair b) {
            return (char)b._cachedValue;
        }

        public static implicit operator BasePair(byte b) {
            return new BasePair(b);
        }

        public static implicit operator BasePair(char c) {
            return new BasePair((byte)c);
        }

        #endregion Conversions

        #region Overrides

        public override string ToString() {
            var s = $"{_topValue}{_bottomValue}";
            var x = new[]
            {
                s[0],
                s[2],
                s[1],
                s[3]
            };
            return new string(x);
        }

        #endregion Overrides
    }
}