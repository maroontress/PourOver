namespace PourOver
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Representing a token and its counter.
    /// </summary>
    public sealed class CountedToken : IComparable<CountedToken>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CountedToken"/> class.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <remarks>
        /// The counter is set to one.
        /// </remarks>
        public CountedToken(string token)
        {
            Token = token;
            Count = 1;
        }

        /// <summary>
        /// Gets the counter.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the token.
        /// </summary>
        public string Token { get; }

        /// <inheritdoc/>
        public int CompareTo([AllowNull] CountedToken other)
        {
            return Token.CompareTo(other.Token);
        }

        /// <summary>
        /// Increments the counter.
        /// </summary>
        public void Increment()
        {
            ++Count;
        }
    }
}
