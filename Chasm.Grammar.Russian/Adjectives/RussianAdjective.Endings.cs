﻿using System;
using JetBrains.Annotations;

namespace Chasm.Grammar.Russian
{
    using AdjInfo = RussianAdjectiveInfo;
    using SubjProps = RussianNounProperties;

    public sealed partial class RussianAdjective
    {
        [Pure] private static ReadOnlySpan<char> DetermineAdjectiveEnding(AdjInfo info, SubjProps props)
        {
            ReadOnlySpan<byte> lookup = AdjectiveEndingLookup;

            // Get indices of both unstressed and stressed forms of endings (usually they're the same)
            int lookupIndex = ComposeEndingIndex(info, props, props.Case);
            int unStrIndex = lookup[lookupIndex];

            // Accusative case usually uses either genitive's or nominative's ending, depending on animacy.
            // In such case, the lookup yields the index 0 (element = null). Don't confuse with "" ('null ending' in grammar).
            if (unStrIndex == 0)
            {
                lookupIndex = ComposeEndingIndex(info, props, props.IsAnimate ? RussianCase.Genitive : RussianCase.Nominative);
                unStrIndex = lookup[lookupIndex];
            }
            // Stressed ending index is right next to the unstressed one's
            int strIndex = lookup[lookupIndex + 1];

            // If the ending depends on the stress, determine the one needed here.
            // If the endings are the same, it doesn't matter which one is used.
            bool stressed = unStrIndex != strIndex && IsEndingStressed(info, props);

            int spanPos = stressed ? strIndex : unStrIndex;
            return AdjectiveEndingSpan.Slice(spanPos & 0x3F, spanPos >> 6);
        }

        [Pure] private static bool IsEndingStressed(AdjInfo info, SubjProps props)
        {
            RussianStress stress = props.Case == (RussianCase)6
                ? info.Declension.StressPattern.Alt
                : info.Declension.StressPattern.Main;

            return stress switch
            {
                // Full-form and short-form:
                RussianStress.A => false,
                RussianStress.B => true,
                // Short-form only:
                RussianStress.C => props.Gender is RussianGender.Feminine,
                RussianStress.Ap => false,
                RussianStress.Bp => props.Gender is RussianGender.Neuter or RussianGender.Feminine,
                // TODO: ??
                RussianStress.Cp => props.Gender is RussianGender.Feminine,
                RussianStress.Cpp => props.Gender is RussianGender.Feminine,

                _ => throw new InvalidOperationException($"{stress} is not a valid stress pattern for adjectives."),
            };
        }

        [Pure] private static int ComposeEndingIndex(AdjInfo info, SubjProps props, RussianCase @case)
        {
            // Context-dependent variables are more significant and come first, subject-driven variables come next,
            // And finally, unstressed and stressed forms are next to each other to make stress-checking simpler.

            // Composite index: [short & case:7] [plural & gender:4] [stem type:7] [stress:2]

            int index = (int)@case;
            index = index * 4 + (int)props.Gender;
            index = index * 7 + (info.Declension.StemType - 1);
            index *= 2; // stress takes up the least significant bit

            return index;
        }

        // A compact (392 B) adjective ending index lookup, used by DetermineAdjectiveEnding()
        private static ReadOnlySpan<byte> AdjectiveEndingLookup =>
        [
            0x93, 0x93, 0xa3, 0xa3, 0x93, 0x93, 0xa3, 0x93, 0xa3, 0x93, 0xa3, 0xa3, 0xa3, 0xa3, 0x81, 0x91,
            0x9b, 0x9b, 0x9b, 0x91, 0x9b, 0x91, 0x81, 0x91, 0x9b, 0x9b, 0x9b, 0x9b, 0x9e, 0x9e, 0x9f, 0x9f,
            0x9e, 0x9e, 0x9e, 0x9e, 0x9e, 0x9e, 0x9f, 0x9f, 0x9f, 0x9f, 0x83, 0x83, 0xa2, 0xa2, 0xa2, 0xa2,
            0xa2, 0xa2, 0x83, 0x83, 0xa2, 0xa2, 0xa2, 0xa2, 0xc6, 0xc6, 0xc4, 0xc4, 0xc6, 0xc6, 0xc4, 0xc6,
            0xc4, 0xc6, 0xc4, 0xc4, 0xc4, 0xc4, 0xc6, 0xc6, 0xc4, 0xc4, 0xc6, 0xc6, 0xc4, 0xc6, 0xc4, 0xc6,
            0xc4, 0xc4, 0xc4, 0xc4, 0x91, 0x91, 0xa4, 0xa4, 0x91, 0x91, 0xa4, 0x91, 0xa4, 0x91, 0xa4, 0xa4,
            0xa4, 0xa4, 0x8b, 0x8b, 0x8f, 0x8f, 0x8f, 0x8f, 0x8f, 0x8f, 0x8b, 0x8b, 0x8f, 0x8f, 0x8f, 0x8f,
            0xc8, 0xc8, 0xd4, 0xd4, 0xc8, 0xc8, 0xd4, 0xc8, 0xd4, 0xc8, 0xd4, 0xd4, 0xd4, 0xd4, 0xc8, 0xc8,
            0xd4, 0xd4, 0xc8, 0xc8, 0xd4, 0xc8, 0xd4, 0xc8, 0xd4, 0xd4, 0xd4, 0xd4, 0x91, 0x91, 0xa4, 0xa4,
            0x91, 0x91, 0xa4, 0x91, 0xa4, 0x91, 0xa4, 0xa4, 0xa4, 0xa4, 0x8d, 0x8d, 0x99, 0x99, 0x99, 0x99,
            0x99, 0x99, 0x8d, 0x8d, 0x99, 0x99, 0x99, 0x99, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x96, 0x96, 0x97, 0x97, 0x96, 0x96, 0x96, 0x96, 0x96, 0x96, 0x97, 0x97,
            0x97, 0x97, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x8d, 0x8d, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x8d, 0x8d, 0x99, 0x99, 0x99, 0x99, 0x8d, 0x8d,
            0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x8d, 0x8d, 0x99, 0x99, 0x99, 0x99, 0x91, 0x91, 0xa4, 0xa4,
            0x91, 0x91, 0xa4, 0x91, 0xa4, 0x91, 0xa4, 0xa4, 0xa4, 0xa4, 0xcd, 0xcd, 0xd9, 0xd9, 0xd9, 0xd9,
            0xd9, 0xd9, 0xcd, 0xcd, 0xd9, 0xd9, 0xd9, 0xd9, 0x88, 0x88, 0x94, 0x94, 0x88, 0x88, 0x94, 0x88,
            0x94, 0x88, 0x94, 0x94, 0x94, 0x94, 0x88, 0x88, 0x94, 0x94, 0x88, 0x88, 0x94, 0x88, 0x94, 0x88,
            0x94, 0x94, 0x94, 0x94, 0x91, 0x91, 0xa4, 0xa4, 0x91, 0x91, 0xa4, 0x91, 0xa4, 0x91, 0xa4, 0xa4,
            0xa4, 0xa4, 0x8b, 0x8b, 0x8f, 0x8f, 0x8f, 0x8f, 0x8f, 0x8f, 0x8b, 0x8b, 0x8f, 0x8f, 0x8f, 0x8f,
            0x46, 0x46, 0x44, 0x61, 0x46, 0x46, 0x44, 0x46, 0x44, 0x46, 0x44, 0x61, 0x44, 0x61, 0x01, 0x01,
            0x5d, 0x5d, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x42, 0x42, 0x5d, 0x5d, 0x5e, 0x5e, 0x5f, 0x5f,
            0x5e, 0x5e, 0x5e, 0x5e, 0x5e, 0x5e, 0x5f, 0x5f, 0x5f, 0x5f, 0x41, 0x41, 0x4f, 0x4f, 0x4f, 0x4f,
            0x4f, 0x4f, 0x41, 0x41, 0x4f, 0x4f, 0x4f, 0x4f,
        ];

        // The 34 standard adjective endings condensed into a 38-char span
        // "ый ой ого ому ым ом ий его ему им ем ь й ая ую а яя ей юю я ое о ее е ё ые ых ыми ы ие их ими и"
        // "ыйыегогомуыхымихойоемуююимийьаяяёиеей"
        private static ReadOnlySpan<char> AdjectiveEndingSpan =>
        [
            '\0',
            'ы', 'й', 'ы', 'е', 'г', 'о', 'г', 'о', 'м', 'у', 'ы', 'х', 'ы', 'м', 'и', 'х',
            'о', 'й', 'о', 'е', 'м', 'у', 'ю', 'ю', 'и', 'м', 'и', 'й', 'ь', 'а', 'я', 'я',
            'ё', 'и', 'е', 'е', 'й',
        ];

    }
}
