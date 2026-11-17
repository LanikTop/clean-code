using Markdown.InlinePieces;
using Markdown.Models;
using Markdown.Tokens;

namespace Markdown
{
    internal class InlineResolver
    {
        private Stack<OpenContainer> stack = new();

        public InlinePiece[] Resolve(Token[] tokens)
        {
            if (tokens.Length == 0)
                return Array.Empty<InlinePiece>();

            var root = new OpenContainer { Kind = ContainerKind.Root };
            stack.Push(root);

            foreach (var token in tokens)
            {
                switch (token)
                {
                    case TextToken t:
                        AddText(t.Text);
                        break;
                    case EscapedToken e:
                        AddText(e.EscapedChar.ToString());
                        break;
                    case UnderscoreToken u:
                        ProcessUnderscore(u);
                        break;
                    case BracketOpenToken:
                        stack.Push(new OpenContainer { Kind = ContainerKind.Link, OpenerText = "[" });
                        break;
                    case LinkTailToken lt:
                        ProcessLinkTail(lt);
                        break;
                }
            }

            while (stack.Count > 1)
                PopAsLiteral();

            return MergeText(root.Children.ToArray());
        }

        private void AddText(string text)
        {
            var top = stack.Peek();
            top.Children.Add(new TextInlinePiece(text));
            if (text.Any(char.IsWhiteSpace))
                top.HasWhitespace = true;
        }

        private void ProcessUnderscore(UnderscoreToken u)
        {
            var targetKind = u.Length == 1 ? ContainerKind.Em : ContainerKind.Strong;
            var text = u.Length == 1 ? "_" : "__";
            var top = stack.Peek();

            var canOpen = u.Right.HasValue && !char.IsWhiteSpace(u.Right.Value);
            var canClose = u.Left.HasValue && !char.IsWhiteSpace(u.Left.Value);

            // 1) Есть открытый тег и его можно попробовать закрыть
            if (canClose && top.Kind == targetKind)
            {
                if (CanClosePair(top, u))
                {
                    CloseContainer(targetKind);
                    return;
                }

                PopAsLiteral();
                AddText(text);
                return;
            }

            // 2) Непарное закрытие подчёркиваний
            if (canClose && (top.Kind == ContainerKind.Em || top.Kind == ContainerKind.Strong)
                         && top.Kind != targetKind)
            {
                var matchIdx = FindKindInStack(targetKind);
                if (matchIdx >= 0)
                {
                    while (stack.Count > matchIdx)
                        PopAsLiteral();
                    AddText(text);
                    return;
                }
            }

            // 3) Вдруг мы открывашка
            if (canOpen)
            {
                // Внутри одинарного двойное не работает
                if (targetKind == ContainerKind.Strong && ContainsKind(ContainerKind.Em))
                {
                    AddText(text);
                    return;
                }

                stack.Push(new OpenContainer
                {
                    Kind = targetKind,
                    OpenerText = text,
                    OpenerLeft = u.Left,
                    OpenerLeftEscaped = u.LeftEscaped,
                });
                return;
            }

            // 4) Мы никто
            AddText(text);
        }

        private static bool CanClosePair(OpenContainer top, UnderscoreToken closer)
        {
            if (!top.HasWhitespace) return true;
            var openerIsIntraword = IsIntrawordChar(top.OpenerLeft, top.OpenerLeftEscaped);
            var closerIsIntraword = IsIntrawordChar(closer.Right, closer.RightEscaped);
            return !(openerIsIntraword || closerIsIntraword);
        }

        private static bool IsIntrawordChar(char? c, bool isEscaped)
        {
            if (isEscaped) return false;
            return c.HasValue && !char.IsWhiteSpace(c.Value);
        }

        private void CloseContainer(ContainerKind kind)
        {
            var c = stack.Pop();
            var parent = stack.Peek();
            var children = MergeText(c.Children.ToArray());

            InlinePiece piece = kind switch
            {
                ContainerKind.Em => new EmInlinePiece(children),
                ContainerKind.Strong => new StrongInlinePiece(children),
                ContainerKind.Link => new LinkInlinePiece(c.Url, children),
                _ => throw new InvalidOperationException(),
            };
            parent.Children.Add(piece);
            if (c.HasWhitespace) parent.HasWhitespace = true;
        }

        private static InlinePiece[] MergeText(InlinePiece[] pieces)
        {
            var result = new List<InlinePiece>();
            foreach (var p in pieces)
            {
                if (p is TextInlinePiece t && result.Count > 0 && result[^1] is TextInlinePiece last)
                    result[^1] = new TextInlinePiece(last.Text + t.Text);
                else
                    result.Add(p);
            }
            return result.ToArray();
        }

        private void PopAsLiteral()
        {
            var c = stack.Pop();
            var parent = stack.Peek();
            parent.Children.Add(new TextInlinePiece(c.OpenerText));
            parent.Children.AddRange(c.Children);
            if (c.HasWhitespace || c.OpenerText.Any(char.IsWhiteSpace))
                parent.HasWhitespace = true;
        }

        private int FindKindInStack(ContainerKind kind)
        {
            var depth = 1;

            foreach (var c in stack)
            {
                if (c.Kind == kind) return depth;
                depth++;
            }

            return -1;
        }

        private bool ContainsKind(ContainerKind kind)
        {
            foreach (var c in stack)
                if (c.Kind == kind) return true;
            return false;
        }

        private void ProcessLinkTail(LinkTailToken lt)
        {
            int matchIdx = FindKindInStack(ContainerKind.Link);

            // Ссылка оказалась не ссылкой
            if (!lt.Closed || matchIdx < 0)
            {
                if (matchIdx >= 0)
                    while (stack.Count > matchIdx)
                        PopAsLiteral();
                AddText("](" + lt.Url + (lt.Closed ? ")" : ""));
                return;
            }

            while (stack.Count - 1 > matchIdx)
                PopAsLiteral();

            stack.Peek().Url = lt.Url;
            CloseContainer(ContainerKind.Link);
        }
    }
}
