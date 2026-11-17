namespace Markdown.Models
{
    internal class RawBlock
    {
        public BlockType Type { get; }
        public string Content { get; }

        public RawBlock(BlockType type, string content)
        {
            Type = type;
            Content = content;
        }
    }

    internal enum BlockType
    {
        Heading,
        Paragraph
    }
}
