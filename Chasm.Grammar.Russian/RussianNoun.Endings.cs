﻿using System;

namespace Chasm.Grammar.Russian
{
    using NounDecl = RussianNounDeclension;
    using NounDeclInfo = RussianNounInfoForDeclension;

    public sealed partial class RussianNoun
    {
        private static ReadOnlySpan<char> DetermineNounEnding(NounDecl declension, NounDeclInfo info)
        {
            const RussianDeclensionFlags allCircledNumbers
                = RussianDeclensionFlags.CircledOne |
                  RussianDeclensionFlags.CircledTwo |
                  RussianDeclensionFlags.CircledThree;

            // Handle ①②③ marks
            if ((declension.Flags & allCircledNumbers) != 0)
            {
                string? res = GetCircledNumberOverrideEnding(declension, info);
                if (res is not null) return res;
            }

            ReadOnlySpan<byte> lookup = NounEndingLookup;

            // Get indices of both unaccented and accented forms of endings (usually they're the same)
            int lookupIndex = ComposeEndingIndex(declension, info, info.Case);
            int unAcIndex = lookup[lookupIndex];

            // Accusative case usually uses either genitive's or nominative's ending, depending on animacy.
            // In such case, the lookup yields the index 0 (element = null). Don't confuse with "" ('null ending' in grammar).
            if (unAcIndex == 0)
            {
                lookupIndex = ComposeEndingIndex(declension, info, info.IsAnimate ? RussianCase.Genitive : RussianCase.Nominative);
                unAcIndex = lookup[lookupIndex];
            }
            // Accented ending index is right next to the unaccented one's
            int acIndex = lookup[lookupIndex + 1];

            // If the ending depends on the accent, determine the one needed here.
            // If the endings are the same, it doesn't matter which one is used.
            bool accented = unAcIndex != acIndex && IsAccentOnEnding(declension, info);

            int spanPos = accented ? acIndex : unAcIndex;
            return NounEndingSpan.Slice(spanPos & 0x3F, spanPos >> 6);
        }

        private static string? GetCircledNumberOverrideEnding(NounDecl declension, NounDeclInfo info)
        {
            if (info.IsPlural)
            {
                if ((declension.Flags & RussianDeclensionFlags.CircledOne) != 0 && info.IsNominativeNormalized)
                {
                    int decl = declension.Digit;
                    switch (info.Gender)
                    {
                        case RussianGender.Neuter:
                            return decl is 1 or 5 or 8 ? "ы" : "и";
                        case RussianGender.Masculine:
                            return decl is 1 or 3 or 4 or 5 ? "а" : "я";
                        case RussianGender.Feminine:
                            throw new InvalidOperationException();
                    }
                }
                if ((declension.Flags & RussianDeclensionFlags.CircledTwo) != 0 && info.IsGenitiveNormalized)
                {
                    int decl = declension.Digit;
                    switch (info.Gender)
                    {
                        case RussianGender.Neuter:
                            return decl switch
                            {
                                1 or 3 or 8 => "ов",
                                4 or 5 when IsAccentOnEnding(declension, info) => "ов",
                                2 or 6 or 7 when IsAccentOnEnding(declension, info) => "ёв",
                                _ => "ев",
                            };
                        case RussianGender.Masculine:
                            return decl is 1 or 3 or 4 or 5 ? "" : "ь";
                        case RussianGender.Feminine:
                            return "ей";
                    }
                }
            }
            else // if (info.IsSingular)
            {
                if ((declension.Flags & RussianDeclensionFlags.CircledThree) != 0 && declension.Digit == 7)
                {
                    if (info.Case == RussianCase.Prepositional || info.Gender == RussianGender.Feminine && info.Case == RussianCase.Dative)
                        return "е";
                }
            }
            return null;
        }

        private static bool IsAccentOnEnding(NounDecl declension, NounDeclInfo info)
        {
            bool plural = info.IsPlural;

            // Accusative case's endings and accents depend on the noun's animacy.
            // Some accents, though, depend on the original case, like D′ and F′.
            RussianCase normCase = info.Case;
            if (normCase == RussianCase.Accusative)
                normCase = info.IsAnimate ? RussianCase.Genitive : RussianCase.Nominative;

            return declension.Letter switch
            {
                RussianDeclensionAccent.A => false,
                RussianDeclensionAccent.B => true,
                RussianDeclensionAccent.C => plural,
                RussianDeclensionAccent.D => !plural,
                RussianDeclensionAccent.E => plural && normCase != RussianCase.Nominative,
                RussianDeclensionAccent.F => !plural || normCase != RussianCase.Nominative,
                RussianDeclensionAccent.Bp => plural || normCase != RussianCase.Instrumental,
                RussianDeclensionAccent.Dp => !plural && info.Case != RussianCase.Accusative,
                RussianDeclensionAccent.Fp => plural ? normCase != RussianCase.Nominative : info.Case != RussianCase.Accusative,
                RussianDeclensionAccent.Fpp => plural ? normCase != RussianCase.Nominative : normCase != RussianCase.Instrumental,
            };
        }

        private static int ComposeEndingIndex(NounDecl declension, NounDeclInfo info, RussianCase @case)
        {
            // Context-dependent variables are more significant and come first, noun-dependent variables come next,
            // And finally, unaccented and accented forms are next to each other to make accent-checking simpler.

            // Composite index: [case:6] [plural:2] [gender:3] [declension:8] [accent:2]

            int index = (int)@case;
            index = index * 2 + (info.IsPlural ? 1 : 0);
            index = index * 3 + (int)info.Gender;
            index = index * 8 + (declension.Digit - 1);
            index *= 2; // accent takes up the least significant bit

            return index;
        }

        // A compact (576 B) noun ending index lookup, used by DetermineNounEnding()
        private static ReadOnlySpan<byte> NounEndingLookup =>
        [
            0x54, 0x54, 0x47, 0x58, 0x54, 0x54, 0x47, 0x54, 0x47, 0x54, 0x47, 0x58, 0x47, 0x58, 0x54, 0x54,
            0x01, 0x01, 0x51, 0x51, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x48, 0x48, 0x48, 0x48, 0x51, 0x51,
            0x41, 0x41, 0x44, 0x44, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x44, 0x44, 0x44, 0x44, 0x51, 0x51,
            0x41, 0x41, 0x44, 0x44, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x44, 0x44, 0x44, 0x44, 0x41, 0x41,
            0x5c, 0x5c, 0x43, 0x43, 0x43, 0x43, 0x43, 0x43, 0x5c, 0x5c, 0x43, 0x43, 0x43, 0x43, 0x43, 0x43,
            0x5c, 0x5c, 0x43, 0x43, 0x43, 0x43, 0x43, 0x43, 0x5c, 0x5c, 0x43, 0x43, 0x43, 0x43, 0x43, 0x43,
            0x41, 0x41, 0x44, 0x44, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x44, 0x44, 0x44, 0x44, 0x41, 0x41,
            0x41, 0x41, 0x44, 0x44, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x44, 0x44, 0x44, 0x44, 0x43, 0x43,
            0x5c, 0x5c, 0x43, 0x43, 0x43, 0x43, 0x43, 0x43, 0x5c, 0x5c, 0x43, 0x43, 0x43, 0x43, 0x43, 0x43,
            0x01, 0x01, 0x51, 0x87, 0x01, 0x01, 0x01, 0x87, 0x01, 0x01, 0x48, 0x48, 0x48, 0x48, 0x01, 0x01,
            0x9a, 0x9a, 0x87, 0x87, 0x9a, 0x9a, 0x87, 0x87, 0x8d, 0x9a, 0x8d, 0x9f, 0x8d, 0x9f, 0x87, 0x87,
            0x01, 0x01, 0x51, 0x87, 0x01, 0x01, 0x01, 0x87, 0x01, 0x01, 0x48, 0x48, 0x48, 0x48, 0x87, 0x87,
            0x53, 0x53, 0x52, 0x52, 0x53, 0x53, 0x53, 0x53, 0x53, 0x53, 0x52, 0x52, 0x52, 0x52, 0x53, 0x53,
            0x53, 0x53, 0x52, 0x52, 0x53, 0x53, 0x53, 0x53, 0x53, 0x53, 0x52, 0x52, 0x52, 0x52, 0x43, 0x43,
            0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x43, 0x47, 0x43, 0x43,
            0x81, 0x81, 0x84, 0x84, 0x81, 0x81, 0x81, 0x81, 0x81, 0x81, 0x84, 0x84, 0x84, 0x84, 0x81, 0x81,
            0x81, 0x81, 0x84, 0x84, 0x81, 0x81, 0x81, 0x81, 0x81, 0x81, 0x84, 0x84, 0x84, 0x84, 0x84, 0x84,
            0x81, 0x81, 0x84, 0x84, 0x81, 0x81, 0x81, 0x81, 0x81, 0x81, 0x84, 0x84, 0x84, 0x84, 0x84, 0x84,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x53, 0x53, 0x52, 0x52, 0x53, 0x53, 0x53, 0x53, 0x53, 0x53, 0x52, 0x52, 0x52, 0x52, 0x51, 0x51,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x96, 0x96, 0x89, 0x9d, 0x96, 0x96, 0x89, 0x96, 0x89, 0x96, 0x89, 0x9d, 0x89, 0x9d, 0x96, 0x96,
            0x96, 0x96, 0x89, 0x9d, 0x96, 0x96, 0x89, 0x96, 0x89, 0x96, 0x89, 0x9d, 0x89, 0x9d, 0x89, 0x9d,
            0x94, 0x94, 0x87, 0x98, 0x94, 0x94, 0x87, 0x94, 0x87, 0x94, 0x87, 0x98, 0x87, 0x98, 0x91, 0x91,
            0xc1, 0xc1, 0xc4, 0xc4, 0xc1, 0xc1, 0xc1, 0xc1, 0xc1, 0xc1, 0xc4, 0xc4, 0xc4, 0xc4, 0xc1, 0xc1,
            0xc1, 0xc1, 0xc4, 0xc4, 0xc1, 0xc1, 0xc1, 0xc1, 0xc1, 0xc1, 0xc4, 0xc4, 0xc4, 0xc4, 0xc4, 0xc4,
            0xc1, 0xc1, 0xc4, 0xc4, 0xc1, 0xc1, 0xc1, 0xc1, 0xc1, 0xc1, 0xc4, 0xc4, 0xc4, 0xc4, 0xc4, 0xc4,
            0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x43, 0x47, 0x43, 0x43,
            0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x43, 0x47, 0x43, 0x43,
            0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x47, 0x43, 0x47, 0x43, 0x43,
            0x8b, 0x8b, 0x8f, 0x8f, 0x8b, 0x8b, 0x8b, 0x8b, 0x8b, 0x8b, 0x8f, 0x8f, 0x8f, 0x8f, 0x8b, 0x8b,
            0x8b, 0x8b, 0x8f, 0x8f, 0x8b, 0x8b, 0x8b, 0x8b, 0x8b, 0x8b, 0x8f, 0x8f, 0x8f, 0x8f, 0x8f, 0x8f,
            0x8b, 0x8b, 0x8f, 0x8f, 0x8b, 0x8b, 0x8b, 0x8b, 0x8b, 0x8b, 0x8f, 0x8f, 0x8f, 0x8f, 0x8f, 0x8f,
        ];
        // The 28 standard noun endings condensed into a 33-char span
        // "е и а у я ю й ы ь о ё ам ах ям ях ей ом ем ём ой ов ев ёй ёв ью ами ями"
        // "амиямиейемахевяхьюуойомёйовыёмёв"
        private static ReadOnlySpan<char> NounEndingSpan =>
        [
            '\0',
            'а', 'м', 'и', 'я', 'м', 'и', 'е', 'й', 'е', 'м', 'а', 'х', 'е', 'в', 'я', 'х',
            'ь', 'ю', 'у', 'о', 'й', 'о', 'м', 'ё', 'й', 'о', 'в', 'ы', 'ё', 'м', 'ё', 'в',
        ];

    }
}
