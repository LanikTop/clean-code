using FluentAssertions;
using Markdown.Tokens;
using NUnit.Framework;

namespace Markdown.Tests.Tests
{
    [TestFixture]
    public class InlineTokenizerTests
    {
        [Test]
        public void InlineTokenizer_Tokenize_ReturnsExpectedTokensSmallTests()
        {
            InlineTokenizer.Tokenize("Heading").Should().Equal([new TextToken("Heading", false)]);
            InlineTokenizer.Tokenize("Heading _With em text_").Should().Equal(new Token[] {
                new TextToken("Heading ", true),
                new UnderscoreToken(1, ' ', 'W'),
                new TextToken("With em text", true),
                new UnderscoreToken(1, 't', null)
            });
            InlineTokenizer.Tokenize("Heading __With strong text__").Should().Equal(new Token[] {
                new TextToken("Heading ", true),
                new UnderscoreToken(2, ' ', 'W'),
                new TextToken("With strong text", true),
                new UnderscoreToken(2, 't', null)
            });
            InlineTokenizer.Tokenize("abc _em_ __strong__").Should().Equal(new Token[] {
                new TextToken("abc ", true),
                new UnderscoreToken(1, ' ', 'e'),
                new TextToken("em", false),
                new UnderscoreToken(1, 'm', ' '),
                new TextToken(" ", true),
                new UnderscoreToken(2, ' ', 's'),
                new TextToken("strong", false),
                new UnderscoreToken(2, 'g', null)
            });

            InlineTokenizer.Tokenize(@"\__вот это будет выделено Em, а не Strong тегом_\_").Should().Equal(new Token[]
            {
                new EscapedToken('_'),
                new UnderscoreToken(1, '_', 'в', true, false),
                new TextToken(@"вот это будет выделено Em, а не Strong тегом", true),
                new UnderscoreToken(1, 'м', '\\', false, true),
                new EscapedToken('_')
            });

            InlineTokenizer.Tokenize("").Should().BeEmpty();
            InlineTokenizer.Tokenize("   ").Should().Equal(new TextToken("   ", true));
        }

        [Test]
        public void InlineTokenizer_Tokenize_ReturnsExpectedTokensTestsWIthLinks()
        {
            InlineTokenizer.Tokenize("[abc](qwerty)").Should().Equal(new Token[] {
                new BracketOpenToken(),
                new TextToken(@"abc", false),
                new LinkTailToken(@"qwerty")
            });
            InlineTokenizer.Tokenize("[abc](qwer").Should().Equal(new Token[] {
                new BracketOpenToken(),
                new TextToken(@"abc](qwer", false),
            });
            InlineTokenizer.Tokenize("[abc]").Should().Equal(new Token[] {
                new BracketOpenToken(),
                new TextToken(@"abc]", false)
            });
            InlineTokenizer.Tokenize("[abc](qw_ert_y)").Should().Equal(new Token[] {
                new BracketOpenToken(),
                new TextToken(@"abc", false),
                new LinkTailToken(@"qw_ert_y")
            });
            InlineTokenizer.Tokenize("[_abc_](qwerty)").Should().Equal(new Token[] {
                new BracketOpenToken(),
                new UnderscoreToken(1, '[', 'a'),
                new TextToken(@"abc", false),
                new UnderscoreToken(1, 'c', ']'),
                new LinkTailToken(@"qwerty")
            });
            InlineTokenizer.Tokenize(@"\[abc](qwerty)").Should().Equal(new Token[] {
                new EscapedToken('['),
                new TextToken(@"abc](qwerty)", false),
            });
            InlineTokenizer.Tokenize(@"\\[abc](qwerty)").Should().Equal( new Token[] {
                new EscapedToken('\\'),
                new BracketOpenToken(),
                new TextToken("abc", false),
                new LinkTailToken("qwerty") 
            });
        }

        [Test]
        public void InlineTokenizer_Tokenize_ReturnsExpectedTokensRealTests()
        {
            InlineTokenizer.Tokenize(@"Спецификация языка разметки")
                .Should().Equal([new TextToken(@"Спецификация языка разметки", true)]);
            InlineTokenizer.Tokenize(@"Посмотрите этот файл в сыром виде. Сравните с тем, что показывает github. Все совпадения случайны ;)")
                .Should().Equal([new TextToken(@"Посмотрите этот файл в сыром виде. Сравните с тем, что показывает github. Все совпадения случайны ;)", true)]);
            InlineTokenizer.Tokenize(@"Курсив")
                .Should().Equal([new TextToken(@"Курсив", false)]);
            InlineTokenizer.Tokenize(@"Текст, _окруженный с двух сторон_ одинарными символами подчерка, должен помещаться в HTML-тег \<em> вот так:")
                .Should().Equal(new Token[] {
                    new TextToken(@"Текст, ", true),
                    new UnderscoreToken(1, ' ', 'о'),
                    new TextToken(@"окруженный с двух сторон", true),
                    new UnderscoreToken(1, 'н', ' '),
                    new TextToken(@" одинарными символами подчерка, должен помещаться в HTML-тег \<em> вот так:", true)
                    });
            InlineTokenizer.Tokenize(@"Текст, \<em>окруженный с двух сторон\</em> одинарными символами подчерка, должен помещаться в HTML-тег \<em>.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Текст, \<em>окруженный с двух сторон\</em> одинарными символами подчерка, должен помещаться в HTML-тег \<em>.", true)
                });
            InlineTokenizer.Tokenize(@"Полужирный")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Полужирный", false)
                });
            InlineTokenizer.Tokenize(@"__Выделенный двумя символами текст__ должен становиться полужирным с помощью тега \<strong>.")
                .Should().Equal(new Token[]
                {
                    new UnderscoreToken(2, null, 'В'),
                    new TextToken(@"Выделенный двумя символами текст", true),
                    new UnderscoreToken(2, 'т', ' '),
                    new TextToken(@" должен становиться полужирным с помощью тега \<strong>.", true)
                });
            InlineTokenizer.Tokenize(@"Экранирование")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Экранирование", false)
                });
            InlineTokenizer.Tokenize(@"Любой символ можно экранировать, чтобы он не считался частью разметки. \_Вот это\_, не должно выделиться тегом \<em>.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Любой символ можно экранировать, чтобы он не считался частью разметки. ", true),
                    new EscapedToken('_'),
                    new TextToken(@"Вот это", true),
                    new EscapedToken('_'),
                    new TextToken(@", не должно выделиться тегом \<em>.", true)
                });
            InlineTokenizer.Tokenize(@"Символ экранирования исчезает из результата, только если экранирует что-то. Здесь сим\волы экранирования\ \должны остаться.\")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Символ экранирования исчезает из результата, только если экранирует что-то. Здесь сим\волы экранирования\ \должны остаться.\", true),
                });
            InlineTokenizer.Tokenize(@"Символ экранирования тоже можно экранировать: \\_вот это будет выделено тегом_ \<em>")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Символ экранирования тоже можно экранировать: ", true),
                    new EscapedToken('\\'),
                    new UnderscoreToken(1, '\\', 'в', true, false),
                    new TextToken(@"вот это будет выделено тегом", true),
                    new UnderscoreToken(1, 'м', ' '),
                    new TextToken(@" \<em>", true)
                });
            InlineTokenizer.Tokenize(@"Взаимодействие тегов")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Взаимодействие тегов", true)
                });
            InlineTokenizer.Tokenize(@"Внутри __двойного выделения _одинарное_ тоже__ работает.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Внутри ", true),
                    new UnderscoreToken(2, ' ', 'д'),
                    new TextToken(@"двойного выделения ", true),
                    new UnderscoreToken(1, ' ', 'о'),
                    new TextToken(@"одинарное", false),
                    new UnderscoreToken(1, 'е', ' '),
                    new TextToken(@" тоже", true),
                    new UnderscoreToken(2, 'е', ' '),
                    new TextToken(@" работает.", true)
                });
            InlineTokenizer.Tokenize(@"Но не наоборот — внутри _одинарного __двойное__ не_ работает.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Но не наоборот — внутри ", true),
                    new UnderscoreToken(1, ' ', 'о'),
                    new TextToken(@"одинарного ", true),
                    new UnderscoreToken(2, ' ', 'д'),
                    new TextToken(@"двойное", false),
                    new UnderscoreToken(2, 'е', ' '),
                    new TextToken(@" не", true),
                    new UnderscoreToken(1, 'е', ' '),
                    new TextToken(@" работает.", true),
                });
            InlineTokenizer.Tokenize(@"Подчерки внутри текста c цифрами_12_3 не считаются выделением и должны оставаться символами подчерка.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Подчерки внутри текста c цифрами_12_3 не считаются выделением и должны оставаться символами подчерка.", true),
                });
            InlineTokenizer.Tokenize(@"Однако выделять часть слова они могут: и в _нач_але, и в сер_еди_не, и в кон_це._")
                .Should().Equal(new Token[]
                {
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
                });
            InlineTokenizer.Tokenize(@"В то же время выделение в ра_зных сл_овах не работает.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"В то же время выделение в ра", true),
                    new UnderscoreToken(1, 'а', 'з'),
                    new TextToken(@"зных сл", true),
                    new UnderscoreToken(1, 'л', 'о'),
                    new TextToken(@"овах не работает.", true),
                });
            InlineTokenizer.Tokenize(@"__Непарные_ символы в рамках одного абзаца не считаются выделением.")
                .Should().Equal(new Token[]
                {
                    new UnderscoreToken(2, null, 'Н'),
                    new TextToken(@"Непарные", false),
                    new UnderscoreToken(1, 'е', ' '),
                    new TextToken(@" символы в рамках одного абзаца не считаются выделением.", true),
                });
            InlineTokenizer.Tokenize(@"За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ подчерки_ не считаются выделением и остаются просто символами подчерка.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти", true),
                    new UnderscoreToken(1, 'и', ' '),
                    new TextToken(@" подчерки", true),
                    new UnderscoreToken(1, 'и', ' '),
                    new TextToken(@" не считаются выделением и остаются просто символами подчерка.", true),
                });
            InlineTokenizer.Tokenize(@"Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти _подчерки _не считаются_ окончанием выделения и остаются просто символами подчерка.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти ", true),
                    new UnderscoreToken(1, ' ', 'п'),
                    new TextToken(@"подчерки ", true),
                    new UnderscoreToken(1, ' ', 'н'),
                    new TextToken(@"не считаются", true),
                    new UnderscoreToken(1, 'я', ' '),
                    new TextToken(@" окончанием выделения и остаются просто символами подчерка.", true)
                });
            InlineTokenizer.Tokenize(@"В случае __пересечения _двойных__ и одинарных_ подчерков ни один из них не считается выделением.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"В случае ", true),
                    new UnderscoreToken(2, ' ', 'п'),
                    new TextToken(@"пересечения ", true),
                    new UnderscoreToken(1, ' ', 'д'),
                    new TextToken(@"двойных", false),
                    new UnderscoreToken(2, 'х', ' '),
                    new TextToken(@" и одинарных", true),
                    new UnderscoreToken(1, 'х', ' '),
                    new TextToken(@" подчерков ни один из них не считается выделением.", true)
                });
            InlineTokenizer.Tokenize(@"Если внутри подчерков пустая строка ____, то они остаются символами подчерка.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Если внутри подчерков пустая строка ____, то они остаются символами подчерка.", true),
                });
            InlineTokenizer.Tokenize(@"Заголовки")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Заголовки", false),
                });
            InlineTokenizer.Tokenize(@"Абзац, начинающийся с ""# "", выделяется тегом \<h1> в заголовок. В тексте заголовка могут присутствовать все прочие символы разметки с указанными правилами.")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Абзац, начинающийся с ""# "", выделяется тегом \<h1> в заголовок. В тексте заголовка могут присутствовать все прочие символы разметки с указанными правилами.", true),
                });
            InlineTokenizer.Tokenize(@"Таким образом")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Таким образом", true),
                });
            InlineTokenizer.Tokenize(@"Заголовок __с _разными_ символами__")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"Заголовок ", true),
                    new UnderscoreToken(2, ' ', 'с'),
                    new TextToken(@"с ", true),
                    new UnderscoreToken(1, ' ', 'р'),
                    new TextToken(@"разными", false),
                    new UnderscoreToken(1, 'и', ' '),
                    new TextToken(@" символами", true),
                    new UnderscoreToken(2, 'и', null)
                });
            InlineTokenizer.Tokenize(@"превратится в:")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"превратится в:", true),
                });
            InlineTokenizer.Tokenize(@"\<h1>Заголовок \<strong>с \<em>разными\</em> символами\</strong>\</h1>")
                .Should().Equal(new Token[]
                {
                    new TextToken(@"\<h1>Заголовок \<strong>с \<em>разными\</em> символами\</strong>\</h1>", true),
                });
        }
    }
}
