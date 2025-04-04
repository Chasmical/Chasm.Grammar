﻿using System;
using System.Linq;
using GrammarSharp.Russian;
using JetBrains.Annotations;
using Xunit;

namespace GrammarSharp.Tests
{
    public partial class RussianAdjectiveTests
    {
        [Pure] public static FixtureAdapter<DeclensionFixture> CreateDeclensionFixtures()
        {
            FixtureAdapter<DeclensionFixture> adapter = [];

            DeclensionFixture New(string stem, string info)
                => adapter.Add(new DeclensionFixture(stem, info));

            #region Simple stem types 0 through 7

            New("хаки", "п 0")
                .Returns("хаки, хаки, хаки, хаки, хаки, хаки, хаки")
                .Returns("хаки, хаки, хаки, хаки, хаки, хаки, хаки")
                .Returns("хаки, хаки, хаки, хаки, хаки, хаки, хаки")
                .Returns("хаки, хаки, хаки, хаки, хаки, хаки, хаки");

            New("садовый", "п 1a")
                .Returns("садовое, садового, садовому, садового, садовым, садовом, садово")
                .Returns("садовый, садового, садовому, садового, садовым, садовом, садов")
                .Returns("садовая, садовой, садовой, садовую, садовой, садовой, садова")
                .Returns("садовые, садовых, садовым, садовых, садовыми, садовых, садовы");

            New("синий", "п 2a/c")
                .Returns("синее, синего, синему, синего, синим, синем, сине")
                .Returns("синий, синего, синему, синего, синим, синем, синь")
                .Returns("синяя, синей, синей, синюю, синей, синей, синя")
                .Returns("синие, синих, синим, синих, синими, синих, сини");

            New("строгий", "п 3a/c'")
                .Returns("строгое, строгого, строгому, строгого, строгим, строгом, строго")
                .Returns("строгий, строгого, строгому, строгого, строгим, строгом, строг")
                .Returns("строгая, строгой, строгой, строгую, строгой, строгой, строга")
                .Returns("строгие, строгих, строгим, строгих, строгими, строгих, строги");

            New("летучий", "п 4a")
                .Returns("летучее, летучего, летучему, летучего, летучим, летучем, летуче")
                .Returns("летучий, летучего, летучему, летучего, летучим, летучем, летуч")
                .Returns("летучая, летучей, летучей, летучую, летучей, летучей, летуча")
                .Returns("летучие, летучих, летучим, летучих, летучими, летучих, летучи");

            New("двулицый", "п 5a")
                .Returns("двулицее, двулицего, двулицему, двулицего, двулицым, двулицем, двулице")
                .Returns("двулицый, двулицего, двулицему, двулицего, двулицым, двулицем, двулиц")
                .Returns("двулицая, двулицей, двулицей, двулицую, двулицей, двулицей, двулица")
                .Returns("двулицые, двулицых, двулицым, двулицых, двулицыми, двулицых, двулицы");

            New("голошеий", "п 6a")
                .Returns("голошеее, голошеего, голошеему, голошеего, голошеим, голошеем, голошее")
                .Returns("голошеий, голошеего, голошеему, голошеего, голошеим, голошеем, голошей")
                .Returns("голошеяя, голошеей, голошеей, голошеюю, голошеей, голошеей, голошея")
                .Returns("голошеие, голошеих, голошеим, голошеих, голошеими, голошеих, голошеи");

            // I couldn't find any 7th type adjectives, so I came up with one
            New("виий", "п 7a")
                .Returns("виее, виего, виему, виего, виим, вием, вие")
                .Returns("виий, виего, виему, виего, виим, вием, вий")
                .Returns("вияя, вией, вией, виюю, вией, вией, вия")
                .Returns("виие, виих, виим, виих, виими, виих, вии");

            #endregion

            #region Alternating vowels, *

            New("скудный", "п 1*a/c'")
                .Returns("скудное, скудного, скудному, скудного, скудным, скудном, скудно")
                .Returns("скудный, скудного, скудному, скудного, скудным, скудном, скуден")
                .Returns("скудная, скудной, скудной, скудную, скудной, скудной, скудна")
                .Returns("скудные, скудных, скудным, скудных, скудными, скудных, скудны");

            New("столетний", "п 2*a")
                .Returns("столетнее, столетнего, столетнему, столетнего, столетним, столетнем, столетне")
                .Returns("столетний, столетнего, столетнему, столетнего, столетним, столетнем, столетен")
                .Returns("столетняя, столетней, столетней, столетнюю, столетней, столетней, столетня")
                .Returns("столетние, столетних, столетним, столетних, столетними, столетних, столетни");

            New("резкий", "п 3*a/c'")
                .Returns("резкое, резкого, резкому, резкого, резким, резком, резко")
                .Returns("резкий, резкого, резкому, резкого, резким, резком, резок")
                .Returns("резкая, резкой, резкой, резкую, резкой, резкой, резка")
                .Returns("резкие, резких, резким, резких, резкими, резких, резки");

            // There aren't any adjectives of type 4-7 that have alternating vowels?

            #endregion

            #region Circled numbers, ①②

            New("мысленный", "п 1*a(1)")
                .Returns("мысленное, мысленного, мысленному, мысленного, мысленным, мысленном, мысленно")
                .Returns("мысленный, мысленного, мысленному, мысленного, мысленным, мысленном, мыслен")
                .Returns("мысленная, мысленной, мысленной, мысленную, мысленной, мысленной, мысленна")
                .Returns("мысленные, мысленных, мысленным, мысленных, мысленными, мысленных, мысленны");

            // There aren't any adjectives of type 2-7 that have ①?

            New("настроенный", "п 1*a/b(2)")
                .Returns("настроенное, настроенного, настроенному, настроенного, настроенным, настроенном, настроено")
                .Returns("настроенный, настроенного, настроенному, настроенного, настроенным, настроенном, настроен")
                .Returns("настроенная, настроенной, настроенной, настроенную, настроенной, настроенной, настроена")
                .Returns("настроенные, настроенных, настроенным, настроенных, настроенными, настроенных, настроены");

            // There aren't any adjectives of type 2-7 that have ②?

            #endregion

            #region Alternating ё

            New("чёрствый", "п 1a/c', ё")
                .Returns("чёрствое, чёрствого, чёрствому, чёрствого, чёрствым, чёрством, чёрство")
                .Returns("чёрствый, чёрствого, чёрствому, чёрствого, чёрствым, чёрством, чёрств")
                .Returns("чёрствая, чёрствой, чёрствой, чёрствую, чёрствой, чёрствой, черства")
                .Returns("чёрствые, чёрствых, чёрствым, чёрствых, чёрствыми, чёрствых, черствы");

            New("мёртвый", "п 1a/c\", ё")
                .Returns("мёртвое, мёртвого, мёртвому, мёртвого, мёртвым, мёртвом, мертво")
                .Returns("мёртвый, мёртвого, мёртвому, мёртвого, мёртвым, мёртвом, мёртв")
                .Returns("мёртвая, мёртвой, мёртвой, мёртвую, мёртвой, мёртвой, мертва")
                .Returns("мёртвые, мёртвых, мёртвым, мёртвых, мёртвыми, мёртвых, мертвы");

            New("жёсткий", "п 3*a/c, ё")
                .Returns("жёсткое, жёсткого, жёсткому, жёсткого, жёстким, жёстком, жёстко")
                .Returns("жёсткий, жёсткого, жёсткому, жёсткого, жёстким, жёстком, жёсток")
                .Returns("жёсткая, жёсткой, жёсткой, жёсткую, жёсткой, жёсткой, жестка")
                .Returns("жёсткие, жёстких, жёстким, жёстких, жёсткими, жёстких, жёстки");

            // There aren't any adjectives of type 2 or 4-7 that have a 'ё'?

            #endregion

            #region Adjectives with pronoun declension

            New("дедов", "п <мс 1a>")
                .Returns("дедово, дедова, дедову, дедова, дедовым, дедовом, ")
                .Returns("дедов, дедова, дедову, дедова, дедовым, дедовом, ")
                .Returns("дедова, дедовой, дедовой, дедову, дедовой, дедовой, ")
                .Returns("дедовы, дедовых, дедовым, дедовых, дедовыми, дедовых, ");

            New("господень", "п <мс 2*a>")
                .Returns("господне, господня, господню, господнего, господним, господнем, ")
                .Returns("господень, господня, господню, господнего, господним, господнем, ")
                .Returns("господня, господней, господней, господню, господней, господней, ")
                .Returns("господни, господних, господним, господних, господними, господних, ");

            New("волчий", "п <мс 6*a>")
                .Returns("волчье, волчьего, волчьему, волчьего, волчьим, волчьем, ")
                .Returns("волчий, волчьего, волчьему, волчьего, волчьим, волчьем, ")
                .Returns("волчья, волчьей, волчьей, волчью, волчьей, волчьей, ")
                .Returns("волчьи, волчьих, волчьим, волчьих, волчьими, волчьих, ");

            #endregion

            #region Pronoun adjectives

            New("ваш", "мс-п 4a")
                .Returns("ваше, вашего, вашему, вашего, вашим, вашем, ")
                .Returns("ваш, вашего, вашему, вашего, вашим, вашем, ")
                .Returns("ваша, вашей, вашей, вашу, вашей, вашей, ")
                .Returns("ваши, ваших, вашим, ваших, вашими, ваших, ");

            New("мой", "мс-п 6b")
                .Returns("моё, моего, моему, моего, моим, моём, ")
                .Returns("мой, моего, моему, моего, моим, моём, ")
                .Returns("моя, моей, моей, мою, моей, моей, ")
                .Returns("мои, моих, моим, моих, моими, моих, ");

            New("чей", "мс-п 6*b")
                .Returns("чьё, чьего, чьему, чьего, чьим, чьём, ")
                .Returns("чей, чьего, чьему, чьего, чьим, чьём, ")
                .Returns("чья, чьей, чьей, чью, чьей, чьей, ")
                .Returns("чьи, чьих, чьим, чьих, чьими, чьих, ");

            #endregion

            #region Numeral adjectives

            New("первый", "числ.-п <п 1a>")
                .Returns("первое, первого, первому, первого, первым, первом, ")
                .Returns("первый, первого, первому, первого, первым, первом, ")
                .Returns("первая, первой, первой, первую, первой, первой, ")
                .Returns("первые, первых, первым, первых, первыми, первых, ");

            New("второй", "числ.-п <п 1b>")
                .Returns("второе, второго, второму, второго, вторым, втором, ")
                .Returns("второй, второго, второму, второго, вторым, втором, ")
                .Returns("вторая, второй, второй, вторую, второй, второй, ")
                .Returns("вторые, вторых, вторым, вторых, вторыми, вторых, ");

            New("третий", "числ.-п <мс 6*a>")
                .Returns("третье, третьего, третьему, третьего, третьим, третьем, ")
                .Returns("третий, третьего, третьему, третьего, третьим, третьем, ")
                .Returns("третья, третьей, третьей, третью, третьей, третьей, ")
                .Returns("третьи, третьих, третьим, третьих, третьими, третьих, ");

            #endregion

            return adapter;
        }

        public sealed class DeclensionFixture(string stem, string info) : FuncFixture<string>
        {
            [Obsolete(TestUtil.DeserCtor, true)] public DeclensionFixture() : this(null!, null!) { }

            public string Stem { get; } = stem;
            public string Info { get; } = info;

            public string? Expected { get; private set; }

            public DeclensionFixture Returns(string expected)
            {
                Expected = Expected is null ? expected : Expected + " // " + expected;
                if (Expected?.Count(ch => ch == '/') == 6) MarkAsComplete();
                return this;
            }
            public override void AssertResult(string? result)
            {
                Assert.NotNull(result);
                Assert.Equal(Expected, result);
            }
            public override string ToString()
                => $"{base.ToString()} {Stem}, {(RussianAdjectiveInfo.TryParse(Info, out var info) ? info : Info)}";

        }

    }
}
