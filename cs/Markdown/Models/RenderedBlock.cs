using Markdown.InlinePieces;

namespace Markdown.Models
{
    internal class RenderedBlock
    {
        public BlockType Type { get; }
        public InlinePiece[] InlinePieces { get; }

        public RenderedBlock(BlockType type, InlinePiece[] linePieces)
        {
            Type = type;
            InlinePieces = linePieces;
        }
    }
}
