using Markdown.Tokens;

namespace Markdown
{
    internal class InlineTokenizer
    {
        public static Token[] Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<Token>();

            var tokens = new List<Token>();
            var startTextTokenIdx = -1;

            var currentIdx = 0;
            var textLength = text.Length;

            void FlushText(int endText)
            {
                if (startTextTokenIdx < 0) return;
                tokens.Add(new TextToken(text.Substring(startTextTokenIdx, endText - startTextTokenIdx)));
                startTextTokenIdx = -1;
            }

            while (currentIdx < textLength)
            {
                var currentChar = text[currentIdx];
                (Token? token, int nextIndex) handleResult;

                if (currentChar == '\\' && currentIdx + 1 < textLength && IsCharEscapable(text[currentIdx + 1]))
                {
                    handleResult = HandleEscapeToken(currentIdx, text);
                }
                else if (currentChar == '_')
                {
                    handleResult = HandleUnderscoreToken(currentIdx, text);
                }
                else if (currentChar == '[')
                {
                    handleResult = HandleBracketOpenToken(currentIdx);
                }
                else if (currentChar == ']' && currentIdx + 1 < textLength && text[currentIdx + 1] == '(')
                {
                    handleResult = HandleLinkTailToken(currentIdx, text);
                }
                else
                {
                    if (startTextTokenIdx == -1) 
                    { 
                        startTextTokenIdx = currentIdx;
                    }
                    currentIdx++;
                    continue;
                }

                if (handleResult.token != null)
                {
                    FlushText(currentIdx);
                    tokens.Add(handleResult.token);
                }
                else
                {
                    if (startTextTokenIdx == -1)
                    {
                        startTextTokenIdx = currentIdx;
                    }
                }

                currentIdx = handleResult.nextIndex;

            }

            FlushText(textLength);
            return tokens.ToArray();
        }

        private static (Token? token, int nextIndex) HandleEscapeToken(int currentIdx, string text)
        {
            var token = new EscapedToken(text[currentIdx + 1]);
            return (token, currentIdx + 2);
        }

        private static (Token? token, int nextIndex) HandleUnderscoreToken(int currentIdx, string text)
        {
            var endUnderscoreIdx = currentIdx;
            var textLength = text.Length;

            while (endUnderscoreIdx < textLength && text[endUnderscoreIdx] == '_')
            {
                endUnderscoreIdx++;
            }
            var runLength = endUnderscoreIdx - currentIdx;

            if (runLength > 2)
            {
                return (null, endUnderscoreIdx);
            }

            char? left = currentIdx > 0 ? text[currentIdx - 1] : null;
            char? right = endUnderscoreIdx < textLength ? text[endUnderscoreIdx] : null;

            var leftEscaped = currentIdx > 1 && text[currentIdx - 2] == '\\' && IsCharEscapable(text[currentIdx - 1]);
            var rightEscaped = endUnderscoreIdx < textLength - 1 && text[endUnderscoreIdx] == '\\' && IsCharEscapable(text[endUnderscoreIdx + 1]);

            var leftIsSpace = !left.HasValue || char.IsWhiteSpace(left.Value);
            var rightIsSpace = !right.HasValue || char.IsWhiteSpace(right.Value);
            var bothNonSpace = !leftIsSpace && !rightIsSpace;
            var hasDigit = (left.HasValue && char.IsDigit(left.Value))
                         || (right.HasValue && char.IsDigit(right.Value));

            if (bothNonSpace && hasDigit)
            {
                return (null, endUnderscoreIdx);
            }

            var token = new UnderscoreToken(runLength, left, right, leftEscaped, rightEscaped);
            return (token, endUnderscoreIdx);
        }

        private static (Token? token, int nextIndex) HandleBracketOpenToken(int currentIdx)
        {
            return (new BracketOpenToken(), currentIdx + 1);
        }

        private static (Token? token, int nextIndex) HandleLinkTailToken(int currentIdx, string text)
        {
            var endLinkIdx = currentIdx;
            var textLength = text.Length;

            while (endLinkIdx < textLength && text[endLinkIdx] != ')')
            {
                endLinkIdx++;
            }

            if (endLinkIdx < textLength)
            {
                var token = new LinkTailToken(text.Substring(currentIdx + 2, endLinkIdx - currentIdx - 2), true);
                return (token, endLinkIdx + 1);
            }
            else
            {
                var token = new LinkTailToken(text.Substring(currentIdx + 2), false);
                return (token, textLength);
            }
        }

        private static bool IsCharEscapable(char c)
            => c == '_' || c == '\\' || c == '[' || c == ']' || c == '(' || c == ')';
    }
}
