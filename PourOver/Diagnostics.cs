namespace PourOver
{
    /// <summary>
    /// Represening the diagnostic information.
    /// </summary>
    public sealed class Diagnostics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Diagnostics"/> class.
        /// </summary>
        /// <param name="row">
        /// The line number of the file.
        /// </param>
        /// <param name="id">
        /// The ID that the most left column contains.
        /// </param>
        /// <param name="kind">
        /// The kind of diagnostics.
        /// </param>
        /// <param name="message">
        /// The diagnostic message.
        /// </param>
        public Diagnostics(int row, string id, string kind, string message)
        {
            Row = row;
            Id = id;
            Kind = kind;
            Message = message;
        }

        /// <summary>
        /// Gets the line number of the file.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Gets the ID that the most left column contains.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the kind of diagnostics.
        /// </summary>
        public string Kind { get; }

        /// <summary>
        /// Gets the diagnostic message.
        /// </summary>
        public string Message { get; }
    }
}
