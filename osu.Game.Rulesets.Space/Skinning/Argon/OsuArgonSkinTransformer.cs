// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Space.Skinning.Argon
{
    public class SpaceArgonSkinTransformer : SkinTransformer
    {
        public SpaceArgonSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            bool isPro = Skin is ArgonProSkin;

            switch (lookup)
            {
                case SkinComponentLookup<HitResult> resultComponent:
                    HitResult result = resultComponent.Component;

                    // This should eventually be moved to a skin setting, when supported.
                    if (isPro && (result == HitResult.Great || result == HitResult.Perfect))
                        return Drawable.Empty();

                    switch (result)
                    {
                        case HitResult.LargeTickHit:
                        case HitResult.SliderTailHit:
                            return null;

                        case HitResult.IgnoreMiss:
                        default:
                            return null;
                    }

                case SpaceSkinComponentLookup osuComponent:
                    // TODO: Once everything is finalised, consider throwing UnsupportedSkinComponentException on missing entries.
                    switch (osuComponent.Component)
                    {
                        case SpaceSkinComponents.Cursor:
                            return new ArgonCursor();

                        case SpaceSkinComponents.CursorTrail:
                            return new ArgonCursorTrail();
                    }

                    break;
            }

            return base.GetDrawableComponent(lookup);
        }
    }
}
