using Markdown.Models;

namespace Markdown
{
    internal static class RawBlockParser
    {
        public static RawBlock[] Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<RawBlock>();

            var paragraphBuffer = new List<string>();
            var blocks = new List<RawBlock>();

            void TryCreateParagraphBlock()
            {
                if (paragraphBuffer.Count == 0) return;
                blocks.Add(new RawBlock(BlockType.Paragraph, string.Join(" ", paragraphBuffer)));
                paragraphBuffer.Clear();
            }

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
    }
}
