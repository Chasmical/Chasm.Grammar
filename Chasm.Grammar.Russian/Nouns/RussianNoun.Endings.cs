﻿using System;
using JetBrains.Annotations;

namespace Chasm.Grammar.Russian
{
    using NounDecl = RussianDeclension;
    using NounProps = RussianNounProperties;

    public sealed partial class RussianNoun
    {
        [Pure] private static ReadOnlySpan<char> DetermineNounEnding(NounDecl declension, NounProps props)
        {
            const RussianDeclensionFlags allCircledNumbers
                = RussianDeclensionFlags.CircledOne |
                  RussianDeclensionFlags.CircledTwo |
                  RussianDeclensionFlags.CircledThree;

            // Handle ①②③ marks
            if ((declension.Flags & allCircledNumbers) != 0)
            {
                string? res = GetCircledNumberOverrideEnding(declension, props);
                if (res is not null) return res;
            }

            ReadOnlySpan<byte> lookup = NounEndingLookup;

            // Get indices of both unstressed and stressed forms of endings (usually they're the same)
            int lookupIndex = ComposeEndingIndex(declension, props, props.Case);
            int unStrIndex = lookup[lookupIndex];

            // Accusative case usually uses either genitive's or nominative's ending, depending on animacy.
            // In such case, the lookup yields the index 0 (element = null). Don't confuse with "" ('null ending' in grammar).
            if (unStrIndex == 0)
            {
                lookupIndex = ComposeEndingIndex(declension, props, props.IsAnimate ? RussianCase.Genitive : RussianCase.Nominative);
                unStrIndex = lookup[lookupIndex];
            }
            // Stressed ending index is right next to the unstressed one's
            int strIndex = lookup[lookupIndex + 1];

            // If the ending depends on the stress, determine the one needed here.
            // If the endings are the same, it doesn't matter which one is used.
            bool stressed = unStrIndex != strIndex && IsEndingStressed(declension, props);

            int spanPos = stressed ? strIndex : unStrIndex;
            return NounEndingSpan.Slice(spanPos & 0x3F, spanPos >> 6);
        }

        [Pure] private static string? GetCircledNumberOverrideEnding(NounDecl declension, NounProps props)
        {
            if (props.IsPlural)
            {
                if ((declension.Flags & RussianDeclensionFlags.CircledOne) != 0 && props.IsNominativeNormalized)
                {
                    int decl = declension.StemType;
                    switch (props.Gender)
                    {
                        case RussianGender.Neuter:
                            return decl is 1 or 5 or 8 ? "ы" : "и";
                        case RussianGender.Masculine:
                            return decl is 1 or 3 or 4 or 5 ? "а" : "я";
                        case RussianGender.Feminine:
                            throw new InvalidOperationException();
                    }
                }
                if ((declension.Flags & RussianDeclensionFlags.CircledTwo) != 0 && props.IsGenitiveNormalized)
                {
                    int decl = declension.StemType;
                    switch (props.Gender)
                    {
                        case RussianGender.Neuter:
                            return decl switch
                            {
                                1 or 3 or 8 => "ов",
                                4 or 5 when IsEndingStressed(declension, props) => "ов",
                                2 or 6 or 7 when IsEndingStressed(declension, props) => "ёв",
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
                if ((declension.Flags & RussianDeclensionFlags.CircledThree) != 0 && declension.StemType == 7)
                {
                    if (props.Case == RussianCase.Prepositional || props.Gender == RussianGender.Feminine && props.Case == RussianCase.Dative)
                        return "е";
                }
            }
            return null;
        }

        [Pure] private static bool IsEndingStressed(NounDecl declension, NounProps props)
        {
            bool plural = props.IsPlural;

            // Accusative case's endings and stresses depend on the noun's animacy.
            // Some stresses, though, depend on the original case, like d′ and f′.
            RussianCase normCase = props.Case;
            if (normCase == RussianCase.Accusative)
                normCase = props.IsAnimate ? RussianCase.Genitive : RussianCase.Nominative;

            return declension.StressPattern.Main switch
            {
                RussianStress.A => false,
                RussianStress.B => true,
                RussianStress.C => plural,
                RussianStress.D => !plural,
                RussianStress.E => plural && normCase != RussianCase.Nominative,
                RussianStress.F => !plural || normCase != RussianCase.Nominative,
                RussianStress.Bp => plural || normCase != RussianCase.Instrumental,
                RussianStress.Dp => !plural && props.Case != RussianCase.Accusative,
                RussianStress.Fp => plural ? normCase != RussianCase.Nominative : props.Case != RussianCase.Accusative,
                RussianStress.Fpp => plural ? normCase != RussianCase.Nominative : normCase != RussianCase.Instrumental,
                _ => throw new InvalidOperationException($"{declension.StressPattern} is not a valid stress pattern for nouns."),
            };
        }

        [Pure] private static int ComposeEndingIndex(NounDecl declension, NounProps props, RussianCase @case)
        {
            // Context-dependent variables are more significant and come first, noun-dependent variables come next,
            // And finally, unstressed and stressed forms are next to each other to make stress-checking simpler.

            // Composite index: [case:6] [plural:2] [gender:3] [stem type:8] [stress:2]

            int index = (int)@case;
            index = index * 2 + (props.IsPlural ? 1 : 0);
            index = index * 3 + (int)props.Gender;
            index = index * 8 + (declension.StemType - 1);
            index *= 2; // stress takes up the least significant bit

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
