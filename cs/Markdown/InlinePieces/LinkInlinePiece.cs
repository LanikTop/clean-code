namespace Markdown.InlinePieces
{
    internal sealed record LinkInlinePiece(string Url, IReadOnlyList<InlinePiece> Children) : InlinePiece;
}
