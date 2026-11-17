namespace Markdown.InlinePieces
{
    internal sealed record EmInlinePiece(IReadOnlyList<InlinePiece> Children) : InlinePiece;
}
