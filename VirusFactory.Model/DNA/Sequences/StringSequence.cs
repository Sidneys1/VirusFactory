using System.Linq;
using System.Text;

namespace VirusFactory.Model.DNA.Sequences
{
	public class StringSequence : SequenceBase<string>
	{
		public StringSequence(string s = null)
		{
			if (s!= null)
				// ReSharper disable once DoNotCallOverridableMethodsInConstructor
				SetValue(s);
		}


		public override string GetValue()
		{
			if (CachedValue == default(string))
				CachedValue =  InternalBasePairs.Aggregate(new StringBuilder(), (x, y) => x.Append((char)y)).ToString();
			return CachedValue;
		}

		public override void SetValue(string value)
		{
			InternalBasePairs.Clear();

			foreach (var c in value)
			{
				InternalBasePairs.Add(c);
			}

			CachedValue = value;
		}
	}
}
