using System;
using System.Net;
using System.Threading.Tasks;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Services;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Screens;
using static BunnyLand.DesktopGL.Utils.UiHelper;
using Screen = MonoGame.Extended.Gui.Screen;

namespace BunnyLand.DesktopGL.Screens
{
    public class MenuScreen : GameScreen
    {
        private readonly GuiSystem guiSystem;
        private readonly MessageHub messageHub;
        private Label loadingLabel;
        private Screen? loadingScreen;
        private Screen? serverListScreen;
        private StackPanel? serversPanel;
        private Screen? startMenuScreen;
        private StackPanel? serversDialog;

        public MenuScreen(Game game, GuiSystem guiSystem, MessageHub messageHub) : base(game)
        {
            this.guiSystem = guiSystem;
            this.messageHub = messageHub;

            messageHub.Subscribe<ServerDiscoveredMessage>(OnServerDiscovered);
        }

        private void OnServerDiscovered(ServerDiscoveredMessage msg)
        {
            if (serversPanel != null) {
                serversPanel.Items.Add(CreateButton(msg.EndPoint.ToString(), () => JoinServerClicked(msg.EndPoint)));
                serversPanel.InvalidateMeasure();
            }
        }

        private void JoinServer(IPEndPoint msgEndPoint)
        {
            Console.WriteLine("Joining server " + msgEndPoint);
        }

        public override void LoadContent()
        {
            ActivateStartMenu();
        }

        private void ActivateStartMenu()
        {
            startMenuScreen ??= SetupStartMenu();
            guiSystem.ActiveScreen = startMenuScreen;
        }

        private void ShowLoadingMessage(string message)
        {
            loadingScreen ??= SetupLoading();
            loadingLabel.Content = message;
            loadingLabel.Position = guiSystem.BoundingRectangle.Center
                - new Point(loadingLabel.Width / 2, loadingLabel.Height / 2);
            guiSystem.ActiveScreen = loadingScreen;
        }

        private Screen SetupLoading()
        {
            loadingLabel = new Label();

            return new Screen {
                Content = new Canvas {
                    Items = {loadingLabel}
                }
            };
        }

        private Screen SetupStartMenu()
        {
            const int menuWidth = 400;
            const int menuHeight = 200;
            var menuPanel = new StackPanel {
                Width = menuWidth,
                Height = menuHeight,
                Position = guiSystem.BoundingRectangle.Center - new Point(menuWidth / 2, menuHeight / 2)
            };

            var startLocalButton = new Button {
                Content = "Start local game"
            };
            startLocalButton.Clicked += delegate {
                messageHub.Publish(new StartGameMessage(new GameOptions {
                    OnlineType = OnlineType.Offline
                }));
            };
            menuPanel.Items.Add(startLocalButton);

            var startLanGame = new Button {
                Content = "Start LAN game"
            };
            startLanGame.Clicked += async (obj, args) => await StartLanGameClicked();
            menuPanel.Items.Add(startLanGame);

            var joinLanGame = new Button {
                Content = "Join LAN game"
            };
            joinLanGame.Clicked += (obj, args) => JoinLanGameClicked();
            menuPanel.Items.Add(joinLanGame);

            return new Screen {
                Content = new Canvas {
                    Items = {menuPanel}
                }
            };
        }

        private async Task StartLanGameClicked()
        {
            ShowLoadingMessage("Starting server...");

            var success = await messageHub.Send(new StartServerRequest(OnlineType.LAN));

            if (success) {
                messageHub.Publish(new StartGameMessage(new GameOptions {OnlineType = OnlineType.LAN}));
            } else {
                ActivateStartMenu();
            }
        }

        private void JoinLanGameClicked()
        {
            // ShowLoadingMessage("Loading servers...");
            // ShowLoadingMessage("Joining game...");

            messageHub.Publish(new StartServerSearchMessage());

            ShowServerList();

            // var success = await mediator.Send(new JoinServerRequest(OnlineType.LAN));
            //
            // if (success) {
            //     ShowLoadingMessage("Success!");
            // } else {
            //     ActivateStartMenu();
            // }
        }

        private void ShowServerList()
        {
            serversDialog = new StackPanel {
                Width = 400,
                Height = 400,
                Position = guiSystem.BoundingRectangle.Center - new Point(200, 200)
            };

            serversDialog.Items.Add(new Label("Searching..."));

            serversPanel = new StackPanel {
                Height = 200
            };
            serversDialog.Items.Add(serversPanel);

            serversDialog.Items.Add(CreateButton("Back", () => ActivateStartMenu()));

            var screen = new Screen {
                Content = new Canvas {
                    Items = {
                        serversDialog
                    }
                }
            };
            guiSystem.ActiveScreen = screen;
        }

        private void JoinServerClicked(IPEndPoint server)
        {
            ShowLoadingMessage($"Joining game {server}...");
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
