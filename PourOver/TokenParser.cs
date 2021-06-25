namespace PourOver
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// The parser of tokens in a string.
    /// </summary>
    public sealed class TokenParser
    {
        private TokenParser(string field)
        {
            Field = field;
            TokenMap = new Dictionary<string, CountedToken>();

            static void ThrowMissingClosingBrace()
                => throw new FormatException(Resource.MissingClosingBrace);

            void AddToken(int start, int end)
            {
                var token = Field[start..end];
                if (!TokenMap.TryGetValue(token, out var countedToken))
                {
                    countedToken = new CountedToken(token);
                    TokenMap[token] = countedToken;
                }
                else
                {
                    countedToken.Increment();
                }
            }

            ParserState NewInterTokenState(int start)
            {
                var charMap = new Dictionary<char, Action<int>>()
                {
                    ['{'] = o => throw new FormatException(
                        Resource.NestedOpeningBrace),
                    ['}'] = o =>
                    {
                        var end = o;
                        AddToken(start, end);
                        State = OuterTokenState;
                    },
                };
                return new ParserState()
                {
                    CharMap = charMap,
                    Finalize = ThrowMissingClosingBrace,
                };
            }

            var outerCharMap = new Dictionary<char, Action<int>>()
            {
                ['{'] = o => State = NewInterTokenState(o + 1),
                ['}'] = o => throw new FormatException(
                    Resource.MissingOpeningBrace),
            };
            OuterTokenState = new ParserState()
            {
                CharMap = outerCharMap,
                Finalize = () => {},
            };
            State = OuterTokenState;
        }

        private string Field { get; }

        private ParserState State { get; set; }

        private ParserState OuterTokenState { get; }

        private Dictionary<string, CountedToken> TokenMap { get; }

        /// <summary>
        /// Gets a set of <see cref="CountedToken"/> representing tokens in
        /// the specified string.
        /// </summary>
        /// <param name="s">
        /// The string containing tokens.
        /// </param>
        /// <returns>
        /// The set of <see cref="CountedToken"/>.
        /// </returns>
        public static ISet<CountedToken> Parse(string s)
        {
            var tokenParser = new TokenParser(s);
            return tokenParser.Parse();
        }

        private ISet<CountedToken> Parse()
        {
            var offset = 0;
            var length = Field.Length;
            while (offset < length)
            {
                var map = State.CharMap;
                if (map.TryGetValue(Field[offset], out var action))
                {
                    action(offset);
                }
                ++offset;
            }
            State.Finalize();
            return TokenMap.Values.ToImmutableHashSet();
        }

        private struct ParserState
        {
            public Dictionary<char, Action<int>> CharMap;
            public Action Finalize;
        }
    }
}
