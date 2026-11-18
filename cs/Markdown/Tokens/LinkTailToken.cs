namespace Markdown.Tokens
{
    internal sealed record LinkTailToken : Token
    {
        public string Url { get; }
        public LinkTailToken(string url)
        {
            Url = url;
        }
    }
}
