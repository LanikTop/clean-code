namespace Markdown.Tokens
{
    internal sealed record TextToken : Token
    {
        public string Text { get; }
        public TextToken(string text) { Text = text; }
    }
}
