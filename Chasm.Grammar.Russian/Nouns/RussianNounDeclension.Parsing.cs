﻿using System;
using Chasm.Formatting;
using Chasm.Utilities;
using JetBrains.Annotations;

namespace Chasm.Grammar.Russian
{
    public readonly partial struct RussianNounDeclension
    {
        [Pure] private static ParseCode ParseInternal(ReadOnlySpan<char> text, out RussianNounDeclension declension)
        {
            declension = default;
            SpanParser parser = new SpanParser(text);

            if (!parser.OnAsciiDigit) return ParseCode.StemTypeNotFound;
            int stemType = parser.Read() - '0';

            if (stemType == 0) return parser.CanRead() ? ParseCode.Unknown : ParseCode.Success;
            if (stemType > 8) return ParseCode.InvalidStemType;

            RussianDeclensionFlags flags = 0;

            if (parser.Skip('*')) flags |= RussianDeclensionFlags.Star;
            if (parser.Skip('°')) flags |= RussianDeclensionFlags.Circle;

            if (!parser.OnAsciiLetter) return ParseCode.StressNotFound;
            RussianStressPattern stress = (RussianStressPattern)((parser.Read() | ' ')  - '`');

            if (parser.SkipAny('\'', '′'))
                stress += 0b0111;
            else if (parser.SkipAny('"', '″'))
            {
                if (stress == RussianStressPattern.C)
                    stress = RussianStressPattern.Cpp;
                else if (stress == RussianStressPattern.F)
                    stress = RussianStressPattern.Fpp;
                else
                    return ParseCode.InvalidStress;
            }

            while (parser.Skip('('))
            {
                char read = parser.Read();
                if (!parser.Skip(')') || (uint)(read - '1') > '3' - '1') return ParseCode.Unknown;

                flags |= (RussianDeclensionFlags)((int)RussianDeclensionFlags.Circle << (read - '0'));
            }

            if (parser.Skip('①')) flags |= RussianDeclensionFlags.CircledOne;
            if (parser.Skip('②')) flags |= RussianDeclensionFlags.CircledTwo;
            if (parser.Skip('③')) flags |= RussianDeclensionFlags.CircledThree;

            if (parser.Skip(',', ' ', 'ё') || parser.Skip(' ', 'ё'))
                flags |= RussianDeclensionFlags.AlternatingYo;

            declension = new RussianNounDeclension(stemType, stress, flags);
            return ParseCode.Success;
        }

        [Pure] public static RussianNounDeclension Parse(string? text)
        {
            Guard.ThrowIfNull(text);
            return Parse(text.AsSpan());
        }
        [Pure] public static RussianNounDeclension Parse(ReadOnlySpan<char> text)
        {
            ParseCode code = ParseInternal(text, out RussianNounDeclension declension);
            if (code is ParseCode.Success) return declension;
            throw new ArgumentException(code.ToString(), nameof(text));
        }

        [Pure] public static bool TryParse(string? text, out RussianNounDeclension declension)
        {
            if (text is null) return Util.Fail(out declension);
            return TryParse(text.AsSpan(), out declension);
        }
        [Pure] public static bool TryParse(ReadOnlySpan<char> text, out RussianNounDeclension declension)
            => ParseInternal(text, out declension) is ParseCode.Success;

        private enum ParseCode
        {
            Success,
            Unknown,
            StemTypeNotFound,
            StressNotFound,
            InvalidStemType,
            InvalidStress,
        }
    }
}
