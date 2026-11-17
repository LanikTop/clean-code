using FluentAssertions;
using Markdown.InlinePieces;
using Markdown.Models;
using NUnit.Framework;

namespace Markdown.Tests.Tests
{
    [TestFixture]
    public class HtmlRendererTests
    {
        private static void RenderOneBlockHtml(RenderedBlock block, string expected)
        {
            HtmlRenderer.Render(new RenderedBlock[] { block }).Should().Be(expected);
        }


        [Test]
        public void HtmlRenderer_HeadingWithText_ReturnsH1Tag()
        {
            RenderOneBlockHtml(
                new RenderedBlock(BlockType.Heading, new InlinePiece[] {
            new TextInlinePiece("Heading")
                }),
                "<h1>Heading</h1>"
            );
        }

        [Test]
        public void HtmlRenderer_HeadingWithEmphasis_ReturnsH1WithEmTag()
        {
            RenderOneBlockHtml(
                new RenderedBlock(BlockType.Heading, new InlinePiece[] {
            new TextInlinePiece("Heading "),
            new EmInlinePiece(new InlinePiece[] { new TextInlinePiece("With em text") })
                }),
                "<h1>Heading <em>With em text</em></h1>"
            );
        }

        [Test]
        public void HtmlRenderer_HeadingWithStrong_ReturnsH1WithStrongTag()
        {
            RenderOneBlockHtml(
                new RenderedBlock(BlockType.Heading, new InlinePiece[] {
            new TextInlinePiece("Heading "),
            new StrongInlinePiece(new InlinePiece[] { new TextInlinePiece("With strong text") })
                }),
                "<h1>Heading <strong>With strong text</strong></h1>"
            );
        }

        [Test]
        public void HtmlRenderer_ParagraphWithMixedInlinePieces_ReturnsCorrectHtml()
        {
            RenderOneBlockHtml(
                new RenderedBlock(BlockType.Paragraph, new InlinePiece[] {
            new TextInlinePiece("abc "),
            new EmInlinePiece(new InlinePiece[] { new TextInlinePiece("em") }),
            new TextInlinePiece(" "),
            new StrongInlinePiece(new InlinePiece[] { new TextInlinePiece("strong") })
                }),
                "<p>abc <em>em</em> <strong>strong</strong></p>"
            );
        }

        [Test]
        public void HtmlRenderer_EmptyParagraph_ReturnsEmptyPTag()
        {
            RenderOneBlockHtml(
                new RenderedBlock(BlockType.Paragraph, Array.Empty<InlinePiece>()),
                "<p></p>"
            );
        }
    }
}
