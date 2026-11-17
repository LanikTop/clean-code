using Markdown.InlinePieces;

namespace Markdown.Models
{
    internal sealed class OpenContainer
    {
        public ContainerKind Kind;
        public string OpenerText = "";
        public List<InlinePiece> Children = new();
        public bool HasWhitespace;
        public char? OpenerLeft;
        public bool OpenerLeftEscaped;
        public string Url;
    }

    internal enum ContainerKind { Root, Em, Strong, Link }
}
