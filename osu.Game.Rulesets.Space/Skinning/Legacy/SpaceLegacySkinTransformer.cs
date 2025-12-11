// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Rulesets.Osu.Skinning;

namespace osu.Game.Rulesets.Space.Skinning.Legacy
{
    public class SpaceLegacySkinTransformer : LegacySkinTransformer
    {
        public override bool IsProvidingLegacyResources => base.IsProvidingLegacyResources || hasHitCircle.Value;

        private readonly Lazy<bool> hasHitCircle;


        public SpaceLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
            hasHitCircle = new Lazy<bool>(() => GetTexture("hitcircle") != null);
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                case GlobalSkinnableContainerLookup containerLookup:
                    // Only handle per ruleset defaults here.
                    if (containerLookup.Ruleset == null)
                        return base.GetDrawableComponent(lookup);

                    // we don't have enough assets to display these components (this is especially the case on a "beatmap" skin).
                    if (!IsProvidingLegacyResources)
                        return null;

                    // Our own ruleset components default.
                    switch (containerLookup.Lookup)
                    {
                        case GlobalSkinnableContainers.MainHUDComponents:
                            return new DefaultSkinComponentsContainer(container =>
                            {
                                var keyCounter = container.OfType<LegacyKeyCounterDisplay>().FirstOrDefault();

                                if (keyCounter != null)
                                {
                                    // set the anchor to top right so that it won't squash to the return button to the top
                                    keyCounter.Anchor = Anchor.CentreRight;
                                    keyCounter.Origin = Anchor.TopRight;
                                    keyCounter.Position = new Vector2(0, -40) * 1.6f;
                                }

                                var combo = container.OfType<LegacyDefaultComboCounter>().FirstOrDefault();
                                var spectatorList = container.OfType<SpectatorList>().FirstOrDefault();
                                var leaderboard = container.OfType<DrawableGameplayLeaderboard>().FirstOrDefault();

                                Vector2 pos = new Vector2();

                                if (combo != null)
                                {
                                    combo.Anchor = Anchor.BottomLeft;
                                    combo.Origin = Anchor.BottomLeft;
                                    combo.Scale = new Vector2(1.28f);

                                    pos += new Vector2(10, -(combo.DrawHeight * 1.56f + 20) * combo.Scale.X);
                                }

                                if (spectatorList != null)
                                {
                                    spectatorList.Anchor = Anchor.BottomLeft;
                                    spectatorList.Origin = Anchor.BottomLeft;
                                    spectatorList.Position = pos;

                                    // maximum height of the spectator list is around ~172 units
                                    pos += new Vector2(0, -185);
                                }

                                if (leaderboard != null)
                                {
                                    leaderboard.Anchor = Anchor.BottomLeft;
                                    leaderboard.Origin = Anchor.BottomLeft;
                                    leaderboard.Position = pos;
                                }
                            })
                            {
                                Children = new Drawable[]
                                {
                                    new LegacyDefaultComboCounter(),
                                    new LegacyKeyCounterDisplay(),
                                    new SpectatorList(),
                                    new DrawableGameplayLeaderboard(),
                                }
                            };
                    }

                    return null;

                case SkinComponentLookup<HitResult> resultComponent:
                    switch (resultComponent.Component)
                    {
                        case HitResult.LargeTickHit:
                        case HitResult.SliderTailHit:
                        case HitResult.LargeTickMiss:
                        case HitResult.IgnoreMiss:
                            if (getSliderPointTexture(resultComponent.Component == HitResult.LargeTickMiss
                                    ? HitResult.LargeTickHit
                                    : HitResult.SliderTailHit) != null)
                                return base.GetDrawableComponent(lookup) ?? Drawable.Empty();

                            break;
                    }

                    return base.GetDrawableComponent(lookup);

                    Texture? getSliderPointTexture(HitResult result)
                    {
                        // https://github.com/peppy/osu-stable-reference/blob/0e91e49bc83fe8b21c3ba5f1eb2d5d06456eae84/osu!/GameModes/Play/Rulesets/Ruleset.cs#L799
                        if (GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value < 2m)
                            // Note that osu!stable used sliderpoint30 for heads and repeats, and sliderpoint10 for ticks, but the mapping is intentionally changed here so that each texture represents one type of HitResult.
                            return GetTexture(result == HitResult.LargeTickHit ? "sliderpoint30" : "sliderpoint10");

                        return null;
                    }

                case SpaceSkinComponentLookup osuComponent:
                    switch (osuComponent.Component)
                    {

                        case SpaceSkinComponents.Cursor:
                            if (GetTexture("cursor") != null)
                                return new LegacyCursor(this);

                            return null;

                        case SpaceSkinComponents.CursorTrail:
                            if (GetTexture("cursortrail") != null)
                                return new LegacyCursorTrail(this);

                            return null;

                        default:
                            return null;
                    }

                default:
                    return base.GetDrawableComponent(lookup);
            }
        }

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case SpaceSkinColour colour:
                    return base.GetConfig<SkinCustomColourLookup, TValue>(new SkinCustomColourLookup(colour));

                case SpaceSkinConfiguration osuLookup:
                    switch (osuLookup)
                    {

                        case SpaceSkinConfiguration.HitCircleOverlayAboveNumber:
                            // See https://osu.ppy.sh/help/wiki/Skinning/skin.ini#%5Bgeneral%5D
                            // HitCircleOverlayAboveNumer (with typo) should still be supported for now.
                            return base.GetConfig<SpaceSkinConfiguration, TValue>(SpaceSkinConfiguration.HitCircleOverlayAboveNumber) ??
                                   base.GetConfig<SpaceSkinConfiguration, TValue>(SpaceSkinConfiguration.HitCircleOverlayAboveNumer);
                    }

                    break;
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}
