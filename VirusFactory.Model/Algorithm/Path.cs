using System;
using System.Collections;
using System.Collections.Generic;
using VirusFactory.Model.Interface;

namespace VirusFactory.Model.Algorithm {

    public class Path<T> : IEnumerable<T>, IFormattable where T : ICoordinate {

        public T LastStep { get; }

        public T FirstStep => PreviousSteps != null ? PreviousSteps.FirstStep : LastStep;

        public Path<T> FirstPath => PreviousSteps != null ? PreviousSteps.FirstPath : this;

        public Path<T> PreviousSteps { get; set; }

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

        public override string ToString() {
            return PreviousSteps != null
                ? $"{PreviousSteps} ({TotalCost - PreviousSteps.TotalCost:#})→ {LastStep}"
                : LastStep.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider) {
            return PreviousSteps == null ? LastStep.ToString() : $"{PreviousSteps.ToString(format, formatProvider)} -> {LastStep}";
            //var diff = (TotalCost - PreviousSteps.TotalCost).ToString(format, formatProvider);
        }
    }
}