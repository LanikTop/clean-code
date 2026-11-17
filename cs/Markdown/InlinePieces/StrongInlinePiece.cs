namespace Markdown.InlinePieces
{
    internal sealed record StrongInlinePiece(IReadOnlyList<InlinePiece> Children) : InlinePiece;
}
