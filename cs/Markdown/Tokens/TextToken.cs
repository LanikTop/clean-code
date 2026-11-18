namespace Markdown.Tokens
{
    internal sealed record TextToken : Token
    {
        public string Text { get; }
        public bool HasWhitespace { get; }
        public TextToken(string text, bool hasWhitespace)
        {
            Text = text;
            HasWhitespace = hasWhitespace;
        }
    }
}
