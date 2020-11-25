using Apos.Gui;

namespace MultiPong.DesktopGL
{
    public static class GuiExtensions
    {
        public static void Clear(this Panel panel)
        {
            var child = panel.GetFinal();

            while (child != panel) {
                panel.Remove(child);
                child = panel.GetFinal();
            }
        }
    }
}
