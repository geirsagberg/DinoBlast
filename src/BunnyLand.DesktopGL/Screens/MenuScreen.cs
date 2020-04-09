using BunnyLand.DesktopGL.Messages;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Screens;
using PubSub;
using Screen = MonoGame.Extended.Gui.Screen;

namespace BunnyLand.DesktopGL.Screens
{
    public class MenuScreen : GameScreen
    {
        private readonly GuiSystem guiSystem;

        public MenuScreen(Game game, GuiSystem guiSystem) : base(game)
        {
            this.guiSystem = guiSystem;
        }

        public override void LoadContent()
        {
            const int menuWidth = 400;
            const int menuHeight = 200;
            var menuPanel = new StackPanel {
                Width = menuWidth,
                Height = menuHeight,
                Position = guiSystem.BoundingRectangle.Center - new Point(menuWidth / 2, menuHeight / 2)
                // Padding = new Thickness{Top = 200, Left = 200, Bottom = 200, Right = 200}
                // AttachedProperties = {{DockPanel.DockProperty, Dock.Top}}
            };

            var startLocalButton = new Button {
                Content = "Start local game"
            };
            startLocalButton.Clicked += delegate {
                Hub.Default.Publish(new StartGameMessage(new GameOptions {
                    OnlineType = OnlineType.Local
                }));
            };

            menuPanel.Items.Add(startLocalButton);

            guiSystem.ActiveScreen = new Screen {
                Content = new Canvas {
                    Items = {menuPanel}
                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            guiSystem.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            guiSystem.Draw(gameTime);
        }
    }
}
