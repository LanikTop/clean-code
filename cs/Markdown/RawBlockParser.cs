using Markdown.Models;

namespace Markdown
{
    internal class RawBlockParser
    {
        private List<string> paragraphBuffer = new();
        private List<RawBlock> blocks = new();

        public RawBlock[] Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<RawBlock>();

            foreach (var line in text.Split(Environment.NewLine))
            {

                if (string.IsNullOrWhiteSpace(line))
                {
                    TryCreateParagraphBlock();
                    continue;
                }

                if (line.Length >= 2 && line[0] == '#' && line[1] == ' ')
                {
                    TryCreateParagraphBlock();
                    blocks.Add(new RawBlock(BlockType.Heading, line.Substring(2)));
                }
                else
                {
                    paragraphBuffer.Add(line);
                }
            }

            TryCreateParagraphBlock();
            return blocks.ToArray();
        }

        private void TryCreateParagraphBlock()
        {
            if (paragraphBuffer.Count == 0) return;
            blocks.Add(new RawBlock(BlockType.Paragraph, string.Join(" ", paragraphBuffer)));
            paragraphBuffer.Clear();
        }
    }
}
