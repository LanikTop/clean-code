using Markdown.Models;

namespace Markdown
{
    internal static class InlineParser
    {
        public static RenderedBlock Parse(RawBlock block)
        {
            var tokens = InlineTokenizer.Tokenize(block.Content);
            var pieces = new InlineResolver().Resolve(tokens);
            return new RenderedBlock(block.Type, pieces);
        }
    }
}
