namespace VirusFactory.Model.DNA.Sequences
{
	public class FixedStringSequence : StringSequence
	{
		private readonly int _length;

		public FixedStringSequence(int length, string s = null)
		{
			_length = length;

			if (s != null)
				// ReSharper disable once DoNotCallOverridableMethodsInConstructor
				SetValue(s);
		}

		public override string GetValue()
		{
			return base.GetValue().Trim('\0');
		}
		
		public override void SetValue(string value)
		{
			if (value.Length > _length)
				value = value.Substring(0, _length);
			else if (value.Length < _length)
				value = value + '\0';

			base.SetValue(value);
		}
	}
}
