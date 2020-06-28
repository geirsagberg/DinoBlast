﻿using System;
using System.Net;
using System.Threading.Tasks;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Services;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
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
        private Label loadingLabel = new Label();
        private Screen? loadingScreen;
        private StackPanel? serversDialog;
        private StackPanel? serversPanel;
        private Screen? startMenuScreen;

        public MenuScreen(Game game, GuiSystem guiSystem, MessageHub messageHub) : base(game)
        {
            this.guiSystem = guiSystem;
            this.messageHub = messageHub;

            messageHub.Subscribe<ServerDiscoveredMessage>(OnServerDiscovered);
        }

        private void OnServerDiscovered(ServerDiscoveredMessage msg)
        {
            if (serversPanel != null) {
                serversPanel.Items.Add(CreateButton(msg.EndPoint.ToString(), async () => await JoinServerClicked(msg.EndPoint)));
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
            var size = loadingLabel.CalculateActualSize(guiSystem);
            loadingLabel.Position = (Size) guiSystem.BoundingRectangle.Center - size / 2;
            guiSystem.ActiveScreen = loadingScreen;
        }

        private Screen SetupLoading()
        {
            loadingLabel.Content = null;

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
                messageHub.Publish(new StartGameMessage());
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
                messageHub.Publish(new StartGameMessage());
            } else {
                ActivateStartMenu();
            }
        }

        private void JoinLanGameClicked()
        {
            messageHub.Publish(new StartServerSearchMessage());

            ShowServerList();
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

            serversDialog.Items.Add(CreateButton("Back", ActivateStartMenu));

            var screen = new Screen {
                Content = new Canvas {
                    Items = {
                        serversDialog
                    }
                }
            };
            guiSystem.ActiveScreen = screen;
        }

        private async Task JoinServerClicked(IPEndPoint server)
        {
            ShowLoadingMessage($"Joining game {server}...");
            var success = await messageHub.Send(new JoinServerRequest(server));
            if (success) {
                ShowLoadingMessage($"Server {server} joined!");
            } else {
                ShowLoadingMessage($"Could not connect to {server}");
            }
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
