namespace Markdown.Tokens
{
    internal sealed record EscapedToken : Token
    {
        public char EscapedChar { get; }
        public EscapedToken(char escapedChar) { EscapedChar = escapedChar; }
    }
}
