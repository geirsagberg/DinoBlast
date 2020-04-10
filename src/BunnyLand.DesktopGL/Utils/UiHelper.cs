using System;
using MonoGame.Extended.Gui.Controls;

namespace BunnyLand.DesktopGL.Utils
{
    public static class UiHelper
    {
        public static Button CreateButton(object content, Action onClick)
        {
            var button = new Button {
                Content = content
            };
            button.Clicked += delegate { onClick(); };
            return button;
        }
    }
}
