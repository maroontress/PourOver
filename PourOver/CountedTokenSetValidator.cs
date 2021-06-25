namespace PourOver
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// The validator of two sets of <see cref="CountedToken"/>.
    /// </summary>
    public sealed class CountedTokenSetValidator
    {
        /// <summary>
        /// Validates the specified two sets of <see cref="CountedToken"/>.
        /// </summary>
        /// <param name="firstLabel">
        /// The label of the first set.
        /// </param>
        /// <param name="first">
        /// The first set of <see cref="CountedToken"/>.
        /// </param>
        /// <param name="secondLabel">
        /// The label of the second set.
        /// </param>
        /// <param name="second">
        /// The second set of <see cref="CountedToken"/>.
        /// </param>
        /// <returns>
        /// The list of a tuple containing the diagnostic kind and message.
        /// </returns>
        public static IList<(string Kind, string Message)> Validate(
            string firstLabel,
            ISet<CountedToken> first,
            string secondLabel,
            ISet<CountedToken> second)
        {
            if (first.Count != second.Count)
            {
                var kind = nameof(Resource.TypeNumberMismatch);
                var m = string.Format(
                    Resource.TypeNumberMismatch,
                    firstLabel,
                    first.Count,
                    secondLabel,
                    second.Count);
                return ImmutableList.Create((kind, m));
            }

            var list = new List<(string Kind, string Message)>();
            var firstMap = first.ToDictionary(i => i.Token, i => i.Count);
            var secondMap = second.ToDictionary(i => i.Token, i => i.Count);

            var firstKeys = firstMap.Keys.ToHashSet();
            var secondKeys = secondMap.Keys.ToHashSet();
            var leftDelta = firstKeys.Except(secondKeys);
            if (leftDelta.Any())
            {
                foreach (var d in leftDelta)
                {
                    var kind = nameof(Resource.StrayToken);
                    var m = string.Format(
                        Resource.StrayToken,
                        d,
                        firstLabel);
                    list.Add((kind, m));
                }
            }
            var rightDelta = secondKeys.Except(firstKeys);
            if (rightDelta.Any())
            {
                foreach (var d in rightDelta)
                {
                    var kind = nameof(Resource.StrayToken);
                    var m = string.Format(
                        Resource.StrayToken,
                        d,
                        secondLabel);
                    list.Add((kind, m));
                }
            }
            if (list.Any())
            {
                return list;
            }
            var keys = firstKeys;
            foreach (var k in keys)
            {
                var firstCount = firstMap[k];
                var secondCount = secondMap[k];
                if (firstCount != secondCount)
                {
                    var kind = nameof(Resource.FrequencyMismatch);
                    var m = string.Format(
                        Resource.FrequencyMismatch,
                        k,
                        firstCount,
                        firstLabel,
                        secondCount,
                        secondLabel);
                    list.Add((kind, m));
                }
            }
            return list;
        }
    }
}
