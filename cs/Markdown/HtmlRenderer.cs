using Markdown.InlinePieces;
using Markdown.Models;

namespace Markdown
{
    internal static class HtmlRenderer
    {
        public static string Render(RenderedBlock[] blocks)
        {
            if (blocks.Length == 0)
                return "<p></p>";

            var htmlStrings = new string[blocks.Length];

            for (var i = 0; i < blocks.Length; i++)
            {
                htmlStrings[i] = RenderBlock(blocks[i]);
            }

            return string.Join(Environment.NewLine, htmlStrings);
        }

        private static string RenderBlock(RenderedBlock block)
        {
            var htmlBlock = RenderInlinePieces(block.InlinePieces);

            switch (block.Type)
            {
                case BlockType.Heading:
                    return $"<h1>{htmlBlock}</h1>";
                case BlockType.Paragraph:
                    return $"<p>{htmlBlock}</p>";
                default:
                    throw new ArgumentException();
            }
        }

        private static string RenderInlinePieces(InlinePiece[] pieces)      
        {
            var htmlPieces = pieces.Select(RenderInlinePiece);

            return string.Join("", htmlPieces);
        }

        private static string RenderInlinePiece(InlinePiece piece)
        {
            switch (piece)
            {
                case TextInlinePiece text:
                    return text.Text;
                case EmInlinePiece em:
                    return @$"<em>{RenderInlinePieces(em.Children.ToArray())}</em>";
                case StrongInlinePiece strong:
                    return @$"<strong>{RenderInlinePieces(strong.Children.ToArray())}</strong>";
                case LinkInlinePiece link:
                    return $"<a href=\"{link.Url}\">{RenderInlinePieces(link.Children.ToArray())}</a>";
                default:
                    throw new ArgumentException();

            }
        }
    }
}
