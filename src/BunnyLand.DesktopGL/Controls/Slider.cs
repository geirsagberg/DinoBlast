using System;
using BunnyLand.DesktopGL.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Input;

namespace BunnyLand.DesktopGL.Controls
{
    public class Slider : ProgressBar
    {
        private readonly float initial;
        private readonly float max;
        private readonly float min;

        private bool isPressed;

        private float Range => max - min;

        private float Value {
            get => Progress * (max - min) + min;
            set => Progress = (value - min) / Range;
        }

        public Slider(float min, float max, float? initial)
        {
            if (min >= max)
                throw new ArgumentOutOfRangeException(nameof(min), "Min must be lower than max");
            if (initial < min || initial > max)
                throw new ArgumentOutOfRangeException(nameof(initial), "Initial must be within range");

            this.min = min;
            this.max = max;
            this.initial = Value = initial ?? min;
        }

        public override void Draw(IGuiContext context, IGuiRenderer renderer, float deltaSeconds)
        {
            base.Draw(context, renderer, deltaSeconds);
            renderer.DrawText(Font ?? Skin.DefaultFont, Value.ToString("F"), ContentRectangle.Location.ToVector2(),
                Color.White);
        }

        public override bool OnPointerDown(IGuiContext context, PointerEventArgs args)
        {
            if (IsEnabled) {
                switch (args.Button) {
                    case MouseButton.Left:
                        isPressed = true;
                        SetProgress(args);
                        break;
                    case MouseButton.Right:
                        Reset();
                        break;
                }
            }

            return base.OnPointerDown(context, args);
        }

        public void Reset()
        {
            Value = initial;
            NotifyChanged();
        }

        public event EventHandler<float> OnSliderChanged;

        private void SetProgress(PointerEventArgs args)
        {
            Progress = ((args.Position.X - ContentRectangle.X) / (float) ContentRectangle.Width).Constrain(0, 1f);
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            OnSliderChanged?.Invoke(this, Value);
        }

        public override bool OnPointerUp(IGuiContext context, PointerEventArgs args)
        {
            if (args.Button == MouseButton.Left)
                isPressed = false;
            return base.OnPointerUp(context, args);
        }

        public override bool OnPointerLeave(IGuiContext context, PointerEventArgs args)
        {
            isPressed = false;
            return base.OnPointerLeave(context, args);
        }

        public override bool OnPointerEnter(IGuiContext context, PointerEventArgs args)
        {
            if (IsEnabled) {
                var mouseState = Mouse.GetState();
                if (mouseState.LeftButton == ButtonState.Pressed) {
                    isPressed = true;
                } else if (mouseState.RightButton == ButtonState.Pressed) {
                    Reset();
                }
            }

            return base.OnPointerEnter(context, args);
        }

        public override bool OnPointerMove(IGuiContext context, PointerEventArgs args)
        {
            if (IsEnabled && isPressed) {
                SetProgress(args);
            }

            return base.OnPointerMove(context, args);
        }

        public class SliderEventArgs
        {
            public float Value { get; set; }
        }
    }
}
