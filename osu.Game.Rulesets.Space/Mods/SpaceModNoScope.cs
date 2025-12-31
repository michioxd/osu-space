// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Space.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Space.Mods
{
    public class SpaceModNoScope : ModNoScope, IUpdatableByPlayfield
    {
        public override LocalisableString Description => "Where's the cursor?";

        public override BindableInt HiddenComboCount { get; } = new BindableInt(10)
        {
            MinValue = 0,
            MaxValue = 50,
        };

        public void Update(Playfield playfield)
        {
            var spacePlayfield = (SpacePlayfield)playfield;
            Debug.Assert(spacePlayfield.Cursor != null);

            bool shouldAlwaysShowCursor = IsBreakTime.Value;
            float targetAlpha = shouldAlwaysShowCursor ? 1 : ComboBasedAlpha;
            float currentAlpha = (float)Interpolation.Lerp(spacePlayfield.Cursor.Alpha, targetAlpha, Math.Clamp(spacePlayfield.Time.Elapsed / TRANSITION_DURATION, 0, 1));

            spacePlayfield.Cursor.Alpha = currentAlpha;
        }
    }
}
