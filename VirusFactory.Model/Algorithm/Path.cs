using System;
using System.Collections;
using System.Collections.Generic;
using VirusFactory.Model.Interface;

namespace VirusFactory.Model.Algorithm
{
	public class Path<T> : IEnumerable<T>, IFormattable where T : ICoordinate {
		public T LastStep { get; }
		public Path<T> PreviousSteps { get; }
		public double TotalCost { get; }

		private Path(T lastStep, Path<T> previousSteps, double totalCost) {
			LastStep = lastStep;
			PreviousSteps = previousSteps;
			TotalCost = totalCost;
		}

		public Path(T start) : this(start, null, 0) {
		}

		public Path<T> AddStep(T step, double stepCost) {
			return new Path<T>(step, this, TotalCost + stepCost);
		}

		public IEnumerator<T> GetEnumerator() {
			for (var p = this; p != null; p = p.PreviousSteps)
				yield return p.LastStep;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString()
		{
			return PreviousSteps != null
				? $"{PreviousSteps} ({TotalCost - PreviousSteps.TotalCost:#})→ {LastStep}"
				: LastStep.ToString();
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (PreviousSteps == null) return LastStep.ToString();
			var diff = (TotalCost - PreviousSteps.TotalCost).ToString(format, formatProvider);
			return $"{PreviousSteps.ToString(format, formatProvider)} {diff}→ {LastStep}";
		}
	}
}