namespace Markdown.Tokens
{
    internal sealed record LinkTailToken : Token
    {
        public string Url { get; }
        public bool Closed { get; }
        public LinkTailToken(string url, bool closed)
        {
            Url = url;
            Closed = closed;
        }
    }
}
