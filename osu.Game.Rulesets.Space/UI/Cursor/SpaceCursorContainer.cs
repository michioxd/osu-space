// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Space.UI.Cursor
{
    public partial class SpaceCursorContainer : GameplayCursorContainer, IKeyBindingHandler<SpaceAction>
    {
        public new SpaceCursor ActiveCursor => (SpaceCursor)base.ActiveCursor;

        protected override Drawable CreateCursor() => new SpaceCursor();
        protected override Container<Drawable> Content => fadeContainer;

        private readonly Container<Drawable> fadeContainer;

        private readonly Bindable<bool> showTrail = new(true);

        private readonly SkinnableDrawable cursorTrail;

        public SpaceCursorContainer()
        {
            InternalChild = fadeContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    cursorTrail = new SkinnableDrawable(new SpaceSkinComponentLookup(SpaceSkinComponents.CursorTrail), _ => new DefaultCursorTrail(), confineMode: ConfineMode.NoScaling),
                    new SkinnableDrawable(new SpaceSkinComponentLookup(SpaceSkinComponents.CursorParticles), confineMode: ConfineMode.NoScaling),
                ]
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(SpaceRulesetConfigManager rulesetConfig)
        {
            rulesetConfig?.BindWith(SpaceRulesetSetting.ShowCursorTrail, showTrail);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (ActiveCursor != null)
                ActiveCursor.Position = DrawSize / 2;

            showTrail.BindValueChanged(v => cursorTrail.FadeTo(v.NewValue ? 1 : 0, 200), true);

            ActiveCursor.CursorScale.BindValueChanged(e =>
            {
                var newScale = new Vector2(e.NewValue);
                cursorTrail.Scale = newScale;
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            if (cursorTrail.Drawable is CursorTrail trail)
            {
                trail.NewPartScale = ActiveCursor.CurrentExpandedScale;
                trail.PartRotation = ActiveCursor.CurrentRotation;
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            base.OnMouseMove(e);
            if (ActiveCursor != null)
            {
                Vector2 availableSize = DrawSize * SpacePlayfield.PLAYFIELD_SIZE;
                float side = Math.Min(availableSize.X, availableSize.Y);
                Vector2 actualSize = new Vector2(side);

                Vector2 minBound = (DrawSize - actualSize) / 2;
                Vector2 maxBound = minBound + actualSize;

                ActiveCursor.Position = Vector2.Clamp(ActiveCursor.Position, minBound, maxBound);

                if (cursorTrail.Drawable is CursorTrail trail)
                {
                    trail.AddTrail(ActiveCursor.ScreenSpaceDrawQuad.Centre);
                }
            }
            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<SpaceAction> e)
        {

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<SpaceAction> e)
        {

        }

        public override bool HandlePositionalInput => true; // OverlayContainer will set this false when we go hidden, but we always want to receive input.

        protected override void PopIn()
        {
            fadeContainer.FadeTo(1, 300, Easing.OutQuint);
            ActiveCursor.ScaleTo(1f, 400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            fadeContainer.FadeTo(0.05f, 450, Easing.OutQuint);
            ActiveCursor.ScaleTo(0.8f, 450, Easing.OutQuint);
        }

        private partial class DefaultCursorTrail : CursorTrail
        {
            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(@"Cursor/cursortrail");
                Scale = new Vector2(1 / Texture.ScaleAdjust);
            }
        }
    }
}
