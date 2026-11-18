using FluentAssertions;
using Markdown.InlinePieces;
using Markdown.Tokens;
using NUnit.Framework;

namespace Markdown.Tests.Tests
{
    [TestFixture]
    public class InlineResolverTests
    {
        private static void ComparisonPieces(InlinePiece[] actual, InlinePiece[] expected)
        {
            actual.Should().HaveCount(expected.Length);
            for (int i = 0; i < expected.Length; i++)
                ComparisonPiece(actual[i], expected[i]);
        }

        private static void ComparisonPiece(InlinePiece actual, InlinePiece expected)
        {
            switch (expected)
            {
                case TextInlinePiece t:
                    actual.Should().BeOfType<TextInlinePiece>()
                        .Which.Text.Should().Be(t.Text);
                    break;

                case EmInlinePiece e:
                    ComparisonPieces(actual.As<EmInlinePiece>().Children.ToArray(), e.Children.ToArray());
                    break;

                case StrongInlinePiece s:
                    ComparisonPieces(actual.As<StrongInlinePiece>().Children.ToArray(), s.Children.ToArray());
                    break;

                case LinkInlinePiece l:
                    actual.Should().BeOfType<LinkInlinePiece>()
                        .Which.Url.Should().Be(l.Url);
                    ComparisonPieces(actual.As<LinkInlinePiece>().Children.ToArray(), l.Children.ToArray());
                    break;
            }
        }


        [Test]
        public void InlineResolver_Resolve_ReturnsExpectedPiecesSmallTests()
        {
            ComparisonPieces(new InlineResolver().Resolve(new Token[] {
                new TextToken("Heading", false)
            }),
            new InlinePiece[]
            {
                new TextInlinePiece(@"Heading")
            });

            ComparisonPieces(new InlineResolver().Resolve(new Token[] {
                new TextToken("Heading ", true),
                new UnderscoreToken(1, ' ', 'W'),
                new TextToken("With em text", true),
                new UnderscoreToken(1, 't', null)
                }),
                new InlinePiece[]
                {
                    new TextInlinePiece(@"Heading "),
                    new EmInlinePiece( new InlinePiece[] {new TextInlinePiece(@"With em text") })
                });

            ComparisonPieces(new InlineResolver().Resolve(new Token[] {
                new TextToken("Heading ", true),
                new UnderscoreToken(2, ' ', 'W'),
                new TextToken("With strong text", true),
                new UnderscoreToken(2, 't', null)
                }),
                new InlinePiece[]
                {
                    new TextInlinePiece(@"Heading "),
                    new StrongInlinePiece( new InlinePiece[] {new TextInlinePiece(@"With strong text") })
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken("abc ", true),
                    new UnderscoreToken(1, ' ', 'e'),
                    new TextToken("em", false),
                    new UnderscoreToken(1, 'm', ' '),
                    new TextToken(" ", true),
                    new UnderscoreToken(2, ' ', 's'),
                    new TextToken("strong", false),
                    new UnderscoreToken(2, 'g', null)
                }),
                new InlinePiece[]
                {
                    new TextInlinePiece(@"abc "),
                    new EmInlinePiece( new InlinePiece[] {new TextInlinePiece(@"em") }),
                    new TextInlinePiece(@" "),
                    new StrongInlinePiece( new InlinePiece[] {new TextInlinePiece(@"strong") })
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new EscapedToken('_'),
                    new UnderscoreToken(1, '_', 'в', true, false),
                    new TextToken(@"вот это будет выделено Em, а не Strong тегом", true),
                    new UnderscoreToken(1, 'м', '\\', false, true),
                    new EscapedToken('_')
                }),
                new InlinePiece[]
                {
                    new TextInlinePiece(@"_"),
                    new EmInlinePiece( new InlinePiece[] {new TextInlinePiece(@"вот это будет выделено Em, а не Strong тегом") }),
                    new TextInlinePiece(@"_")
                });

            new InlineResolver().Resolve(
                new Token[] { })
                .Should().BeEmpty();
        }

        [Test]
        public void InlineResolver_Resolve_ReturnsExpectedPiecesTestsWithLinks()
        {
            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new BracketOpenToken(),
                    new TextToken(@"abc", false),
                    new LinkTailToken(@"qwerty")
                }),
                new InlinePiece[]
                {
                    new LinkInlinePiece(@"qwerty", new InlinePiece[] {new TextInlinePiece(@"abc") })
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new BracketOpenToken(),
                    new TextToken(@"abc", false),
                    new TextToken(@"](", false),
                    new TextToken(@"qwer", false)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"[abc](qwer")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new BracketOpenToken(),
                    new TextToken(@"abc]", false)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"[abc]")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new BracketOpenToken(),
                    new TextToken(@"abc", false),
                    new LinkTailToken(@"qw_ert_y")
                })
                ,new InlinePiece[]
                {
                    new LinkInlinePiece(@"qw_ert_y", new InlinePiece[] {new TextInlinePiece(@"abc") })
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new BracketOpenToken(),
                    new UnderscoreToken(1, '[', 'a'),
                    new TextToken(@"abc", false),
                    new UnderscoreToken(1, 'c', ']'),
                    new LinkTailToken(@"qwerty")
                })
                ,new InlinePiece[]
                {
                    new LinkInlinePiece(@"qwerty", new InlinePiece[] {new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"abc") }) })
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new EscapedToken('['),
                    new TextToken(@"abc", false),
                    new TextToken(@"](qwerty)", false)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"[abc](qwerty)")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new EscapedToken('\\'),
                    new BracketOpenToken(),
                    new TextToken("abc", false),
                    new LinkTailToken("qwerty")
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"\"),
                    new LinkInlinePiece(@"qwerty", new InlinePiece[] { new TextInlinePiece(@"abc") })
                });
        }

        [Test]
        public void InlineResolver_Resolve_ReturnsExpectedPiecesRealTests()
        {
            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Спецификация языка разметки", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Спецификация языка разметки"),
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Посмотрите этот файл в сыром виде. Сравните с тем, что показывает github. Все совпадения случайны ;)", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Посмотрите этот файл в сыром виде. Сравните с тем, что показывает github. Все совпадения случайны ;)")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Курсив", false)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Курсив")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Текст, ", true),
                    new UnderscoreToken(1, ' ', 'о'),
                    new TextToken(@"окруженный с двух сторон", true),
                    new UnderscoreToken(1, 'н', ' '),
                    new TextToken(@" одинарными символами подчерка, должен помещаться в HTML-тег \<em> вот так:", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Текст, "),
                    new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"окруженный с двух сторон") }),
                    new TextInlinePiece(@" одинарными символами подчерка, должен помещаться в HTML-тег \<em> вот так:")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Текст, \<em>окруженный с двух сторон\</em> одинарными символами подчерка, должен помещаться в HTML-тег \<em>.", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Текст, \<em>окруженный с двух сторон\</em> одинарными символами подчерка, должен помещаться в HTML-тег \<em>.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Полужирный", false)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Полужирный")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new UnderscoreToken(2, null, 'В'),
                    new TextToken(@"Выделенный двумя символами текст", true),
                    new UnderscoreToken(2, 'т', ' '),
                    new TextToken(@" должен становиться полужирным с помощью тега \<strong>.", true)
                })
                ,new InlinePiece[]
                {
                    new StrongInlinePiece(new InlinePiece[] { new TextInlinePiece(@"Выделенный двумя символами текст") }),
                    new TextInlinePiece(@" должен становиться полужирным с помощью тега \<strong>.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Экранирование", false)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Экранирование")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Любой символ можно экранировать, чтобы он не считался частью разметки. ", true),
                    new EscapedToken('_'),
                    new TextToken(@"Вот это", true),
                    new EscapedToken('_'),
                    new TextToken(@", не должно выделиться тегом \<em>.", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Любой символ можно экранировать, чтобы он не считался частью разметки. _Вот это_, не должно выделиться тегом \<em>.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Символ экранирования исчезает из результата, только если экранирует что-то. Здесь сим\волы экранирования\ \должны остаться.\", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Символ экранирования исчезает из результата, только если экранирует что-то. Здесь сим\волы экранирования\ \должны остаться.\")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Символ экранирования тоже можно экранировать: ", true),
                    new EscapedToken('\\'),
                    new UnderscoreToken(1, '\\', 'в', true, false),
                    new TextToken(@"вот это будет выделено тегом", true),
                    new UnderscoreToken(1, 'м', ' '),
                    new TextToken(@" \<em>", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Символ экранирования тоже можно экранировать: \"),
                    new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"вот это будет выделено тегом") }),
                    new TextInlinePiece(@" \<em>")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Взаимодействие тегов", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Взаимодействие тегов")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Внутри ", true),
                    new UnderscoreToken(2, ' ', 'д'),
                    new TextToken(@"двойного выделения ", true),
                    new UnderscoreToken(1, ' ', 'о'),
                    new TextToken(@"одинарное", false),
                    new UnderscoreToken(1, 'е', ' '),
                    new TextToken(@" тоже", true),
                    new UnderscoreToken(2, 'е', ' '),
                    new TextToken(@" работает.", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Внутри "),
                    new StrongInlinePiece(new InlinePiece[] { 
                        new TextInlinePiece(@"двойного выделения "), 
                        new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"одинарное") }),
                        new TextInlinePiece(@" тоже")
                        }),
                    new TextInlinePiece(@" работает.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Но не наоборот — внутри ", true),
                    new UnderscoreToken(1, ' ', 'о'),
                    new TextToken(@"одинарного ", false),
                    new UnderscoreToken(2, ' ', 'д'),
                    new TextToken(@"двойное", false),
                    new UnderscoreToken(2, 'е', ' '),
                    new TextToken(@" не", true),
                    new UnderscoreToken(1, 'е', ' '),
                    new TextToken(@" работает.", true),
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Но не наоборот — внутри "),
                    new EmInlinePiece(new InlinePiece[] {
                        new TextInlinePiece(@"одинарного __двойное__ не")
                        }),
                    new TextInlinePiece(@" работает.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Подчерки внутри текста c цифрами_12_3 не считаются выделением и должны оставаться символами подчерка.", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Подчерки внутри текста c цифрами_12_3 не считаются выделением и должны оставаться символами подчерка.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Однако выделять часть слова они могут: и в ", true),
                    new UnderscoreToken(1, ' ', 'н'),
                    new TextToken(@"нач", false),
                    new UnderscoreToken(1, 'ч', 'а'),
                    new TextToken(@"але, и в сер", true),
                    new UnderscoreToken(1, 'р', 'е'),
                    new TextToken(@"еди", false),
                    new UnderscoreToken(1, 'и', 'н'),
                    new TextToken(@"не, и в кон", true),
                    new UnderscoreToken(1, 'н', 'ц'),
                    new TextToken(@"це.", false),
                    new UnderscoreToken(1, '.', null)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Однако выделять часть слова они могут: и в "),
                    new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"нач") }),
                    new TextInlinePiece(@"але, и в сер"),
                    new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"еди") }),
                    new TextInlinePiece(@"не, и в кон"),
                    new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"це.") })
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"В то же время выделение в ра", true),
                    new UnderscoreToken(1, 'а', 'з'),
                    new TextToken(@"зных сл", true),
                    new UnderscoreToken(1, 'л', 'о'),
                    new TextToken(@"овах не работает.", true),
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"В то же время выделение в ра_зных сл_овах не работает."),
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new UnderscoreToken(2, null, 'Н'),
                    new TextToken(@"Непарные", false),
                    new UnderscoreToken(1, 'е', ' '),
                    new TextToken(@" символы в рамках одного абзаца не считаются выделением.", true),
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"__Непарные_ символы в рамках одного абзаца не считаются выделением."),
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти", true),
                    new UnderscoreToken(1, 'и', ' '),
                    new TextToken(@" подчерки", true),
                    new UnderscoreToken(1, 'и', ' '),
                    new TextToken(@" не считаются выделением и остаются просто символами подчерка.", true),
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ подчерки_ не считаются выделением и остаются просто символами подчерка."),
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти ", true),
                    new UnderscoreToken(1, ' ', 'п'),
                    new TextToken(@"подчерки ", true),
                    new UnderscoreToken(1, ' ', 'н'),
                    new TextToken(@"не считаются", true),
                    new UnderscoreToken(1, 'я', ' '),
                    new TextToken(@" окончанием выделения и остаются просто символами подчерка.", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти _подчерки "),
                    new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"не считаются") }),
                    new TextInlinePiece(@" окончанием выделения и остаются просто символами подчерка.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"В случае ", true),
                    new UnderscoreToken(2, ' ', 'п'),
                    new TextToken(@"пересечения ", true),
                    new UnderscoreToken(1, ' ', 'д'),
                    new TextToken(@"двойных", false),
                    new UnderscoreToken(2, 'х', ' '),
                    new TextToken(@" и одинарных", true),
                    new UnderscoreToken(1, 'х', ' '),
                    new TextToken(@" подчерков ни один из них не считается выделением.", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"В случае __пересечения _двойных__ и одинарных_ подчерков ни один из них не считается выделением.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Если внутри подчерков пустая строка ____, то они остаются символами подчерка.", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Если внутри подчерков пустая строка ____, то они остаются символами подчерка.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Заголовки", false)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Заголовки")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Абзац, начинающийся с ""# "", выделяется тегом \<h1> в заголовок. В тексте заголовка могут присутствовать все прочие символы разметки с указанными правилами.", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Абзац, начинающийся с ""# "", выделяется тегом \<h1> в заголовок. В тексте заголовка могут присутствовать все прочие символы разметки с указанными правилами.")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Таким образом", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Таким образом")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"Заголовок ", true),
                    new UnderscoreToken(2, ' ', 'с'),
                    new TextToken(@"с ", true),
                    new UnderscoreToken(1, ' ', 'р'),
                    new TextToken(@"разными", false),
                    new UnderscoreToken(1, 'и', ' '),
                    new TextToken(@" символами", true),
                    new UnderscoreToken(2, 'и', null)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"Заголовок "),
                    new StrongInlinePiece(new InlinePiece[]
                    { 
                        new TextInlinePiece(@"с "),
                        new EmInlinePiece(new InlinePiece[] { new TextInlinePiece(@"разными") }),
                        new TextInlinePiece(@" символами")
                    })
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"превратится в:", true)
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"превратится в:")
                });

            ComparisonPieces(new InlineResolver().Resolve(
                new Token[] {
                    new TextToken(@"\<h1>Заголовок \<strong>с \<em>разными\</em> символами\</strong>\</h1>", true),
                })
                ,new InlinePiece[]
                {
                    new TextInlinePiece(@"\<h1>Заголовок \<strong>с \<em>разными\</em> символами\</strong>\</h1>"),
                });
        }
    }
}
