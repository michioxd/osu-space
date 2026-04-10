using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Game.Rulesets.Space.Configuration;
using osuTK;

#nullable enable

namespace osu.Game.Rulesets.Space.UI
{
    public partial class SpaceTouchInputMapper : Drawable
    {
        private readonly SpaceInputManager spaceInputManager;

        private readonly Bindable<SpaceTouchInputType> inputType = new();
        private readonly Bindable<float> touchSensitivity = new();

        private TouchSource? activeTouchSource;
        private Vector2 lastTouchPosition;

        private bool mousePositionInitialised;

        public SpaceTouchInputMapper(SpaceInputManager inputManager)
        {
            spaceInputManager = inputManager;
        }

        [BackgroundDependencyLoader(true)]
        private void load(SpaceRulesetConfigManager? config)
        {
            config?.BindWith(SpaceRulesetSetting.TouchInputType, inputType);
            config?.BindWith(SpaceRulesetSetting.TouchSensitivity, touchSensitivity);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            if (activeTouchSource != null)
                return false;

            activeTouchSource = e.Touch.Source;
            lastTouchPosition = e.ScreenSpaceTouch.Position;

            if (inputType.Value == SpaceTouchInputType.Absolute)
            {
                applyCursorPosition(e.ScreenSpaceTouch.Position);
                mousePositionInitialised = true;
            }
            else
            {
                if (!mousePositionInitialised)
                {
                    Vector2 screenCentre = ToScreenSpace(DrawSize / 2);
                    applyCursorPosition(screenCentre);
                    mousePositionInitialised = true;
                }
                else
                    applyCursorPosition(spaceInputManager.CurrentState.Mouse.Position);
            }

            return true;
        }

        protected override void OnTouchMove(TouchMoveEvent e)
        {
            base.OnTouchMove(e);

            if (e.Touch.Source != activeTouchSource)
                return;

            Vector2 currentTouchPosition = e.ScreenSpaceTouch.Position;

            if (inputType.Value == SpaceTouchInputType.Absolute)
                applyCursorPosition(currentTouchPosition);
            else
            {
                Vector2 delta = (currentTouchPosition - lastTouchPosition) * touchSensitivity.Value;
                Vector2 currentCursorPosition = spaceInputManager.CurrentState.Mouse.Position;
                applyCursorPosition(currentCursorPosition + delta);
            }

            lastTouchPosition = currentTouchPosition;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            if (e.Touch.Source == activeTouchSource)
                activeTouchSource = null;

            base.OnTouchUp(e);
        }

        private void applyCursorPosition(Vector2 screenSpacePosition)
        {
            new MousePositionAbsoluteInput { Position = screenSpacePosition }.Apply(
                spaceInputManager.CurrentState,
                spaceInputManager
            );
        }
    }
}
