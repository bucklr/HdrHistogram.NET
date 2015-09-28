﻿/*
 * Written by Matt Warren, and released to the public domain,
 * as explained at
 * http://creativecommons.org/publicdomain/zero/1.0/
 *
 * This is a .NET port of the original Java version, which was written by
 * Gil Tene as described in
 * https://github.com/HdrHistogram/HdrHistogram
 */

namespace HdrHistogram.NET.Iteration
{
    /**
     * Used for iterating through histogram values in logarithmically increasing levels. The iteration is
     * performed in steps that start at <i>valueUnitsInFirstBucket</i> and increase exponentially according to
     * <i>logBase</i>, terminating when all recorded histogram values are exhausted. Note that each iteration "bucket"
     * includes values up to and including the next bucket boundary value.
     */
    public class LogarithmicIterator : AbstractHistogramIterator
    {
        int valueUnitsInFirstBucket;
        double logBase;
        long nextValueReportingLevel;
        long nextValueReportingLevelLowestEquivalent;

        /**
         * Reset iterator for re-use in a fresh iteration over the same histogram data set.
         * @param valueUnitsInFirstBucket the size (in value units) of the first value bucket step
         * @param logBase the multiplier by which the bucket size is expanded in each iteration step.
         */
        public void reset(int valueUnitsInFirstBucket, double logBase) 
        {
            this.reset(this.SourceHistogram, valueUnitsInFirstBucket, logBase);
        }

        private void reset(AbstractHistogram histogram, int valueUnitsInFirstBucket, double logBase) 
        {
            base.ResetIterator(histogram);
            this.logBase = logBase;
            this.valueUnitsInFirstBucket = valueUnitsInFirstBucket;
            this.nextValueReportingLevel = valueUnitsInFirstBucket;
            this.nextValueReportingLevelLowestEquivalent = histogram.LowestEquivalentValue(this.nextValueReportingLevel);
        }

        /**
         * @param histogram The histogram this iterator will operate on
         * @param valueUnitsInFirstBucket the size (in value units) of the first value bucket step
         * @param logBase the multiplier by which the bucket size is expanded in each iteration step.
         */
        public LogarithmicIterator(AbstractHistogram histogram, int valueUnitsInFirstBucket, double logBase) 
        {
            this.reset(histogram, valueUnitsInFirstBucket, logBase);
        }

        public override bool HasNext() 
        {
            if (base.HasNext()) {
                return true;
            }
            // If next iterate does not move to the next sub bucket index (which is empty if
            // if we reached this point), then we are not done iterating... Otherwise we're done.
            return (this.nextValueReportingLevelLowestEquivalent < this.NextValueAtIndex);
        }

        protected override void IncrementIterationLevel() {
            this.nextValueReportingLevel *= (long)this.logBase;
            this.nextValueReportingLevelLowestEquivalent = this.SourceHistogram.LowestEquivalentValue(this.nextValueReportingLevel);
        }

        protected override long GetValueIteratedTo() {
            return this.nextValueReportingLevel;
        }

        protected override bool ReachedIterationLevel() {
            return (this.CurrentValueAtIndex >= this.nextValueReportingLevelLowestEquivalent);
        }
    }
}
