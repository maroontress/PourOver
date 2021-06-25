namespace PourOver
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using CsvHelper;

    /// <summary>
    /// Inputs a CSV file and outputs diagnostic information.
    /// </summary>
    public sealed class Drip
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Drip"/> class.
        /// </summary>
        /// <param name="reader">
        /// The reader to provide a CSV file.
        /// </param>
        /// <param name="action">
        /// The action to invoke when diagnostics found.
        /// </param>
        /// <param name="ignoreBlankField">
        /// The function that takes a string and returns whether the field is
        /// ignored.
        /// </param>
        public Drip(
            CsvReader reader,
            Action<Diagnostics> action,
            Func<string, bool> ignoreBlankField)
        {
            Reader = reader;
            Action = action;
            IgnoreBlankField = ignoreBlankField;

            Headers = reader.HeaderRecord
                .ToImmutableArray();
            Columns = Headers.Count;
            if (Columns < 3)
            {
                throw new IOException("csv file must have 3 or more columns.");
            }
            LeftLabel = Headers[1];
            IdMap = new Dictionary<string, int>();
        }

        private CsvReader Reader { get; }

        private Action<Diagnostics> Action { get; }

        private Func<string, bool> IgnoreBlankField { get; }

        private IList<string> Headers { get; }

        private int Columns { get; }

        private string LeftLabel { get; }

        private Dictionary<string, int> IdMap { get; }

        /// <summary>
        /// Starts to check all rows.
        /// </summary>
        public void Start()
        {
            while (Reader.Read())
            {
                CheckRow();
            }
        }

        private void CheckRow()
        {
            var row = Reader.Parser.RawRow;
            var id = Reader.GetField(0);
            if (IgnoreBlankField(id))
            {
                return;
            }
            if (IdMap.TryGetValue(id, out var lastRow))
            {
                var kind = nameof(Resource.DuplicateId);
                var m = string.Format(
                    Resource.DuplicateId,
                    lastRow);
                Action(new Diagnostics(row, id, kind, m));
                return;
            }
            IdMap[id] = row;
            var rawLeftField = Reader.GetField(1);
            if (IgnoreBlankField(rawLeftField))
            {
                return;
            }

            ISet<CountedToken> GetCountedTokenSet(int k)
            {
                try
                {
                    var s = Reader.GetField(k);
                    return TokenParser.Parse(s);
                }
                catch (FormatException e)
                {
                    var kind = nameof(Resource.InvalidToken);
                    var m = string.Format(
                        Resource.InvalidToken,
                        Headers[k],
                        e.Message);
                    Action(new Diagnostics(row, id, kind, m));
                    return ImmutableHashSet<CountedToken>.Empty;
                }
            }

            var tokenSet = Enumerable.Range(0, Columns)
                .Select(GetCountedTokenSet)
                .ToArray();
            var leftTokenSet = tokenSet[1];
            for (var k = 2; k < Columns; ++k)
            {
                if (IgnoreBlankField(Reader.GetField(k)))
                {
                    continue;
                }
                var rightTokenSet = tokenSet[k];
                var rightLabel = Headers[k];
                var list = CountedTokenSetValidator.Validate(
                    LeftLabel,
                    leftTokenSet,
                    rightLabel,
                    rightTokenSet);
                if (list.Count() == 0)
                {
                    continue;
                }
                foreach (var (kind, message) in list)
                {
                    Action(new Diagnostics(row, id, kind, message));
                }
            }
        }
    }
}
