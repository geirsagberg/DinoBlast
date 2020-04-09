using System;
using BunnyLand.DesktopGL.Messages;
using LiteNetLib;
using LiteNetLib.Utils;
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
        private readonly NetManager netClient;
        private readonly NetManager netServer;
        private NetPeer? joinedPeer;
        private readonly int port;

        public MenuScreen(Game game, GuiSystem guiSystem, GameSettings gameSettings) : base(game)
        {
            this.guiSystem = guiSystem;
            var clientListener = new EventBasedNetListener();
            netClient = new NetManager(clientListener);
            clientListener.NetworkReceiveEvent += (peer, reader, method) => {
                Console.WriteLine("Received: {0}", reader.GetString(100));
                reader.Recycle();
            };
            clientListener.NetworkErrorEvent += (endPoint, error) => { Console.WriteLine("Network error: {0}", error); };

            var serverListener = new EventBasedNetListener();
            netServer = new NetManager(serverListener);
            serverListener.ConnectionRequestEvent += request => { request.Accept(); };
            serverListener.PeerConnectedEvent += peer => {
                Console.WriteLine("Peer connected: {0}", peer.EndPoint);
                var writer = new NetDataWriter();
                writer.Put("Welcome!");
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            };
            port = gameSettings.Port;
        }

        public override void LoadContent()
        {
            ActivateStartMenu();
        }

        private void ActivateStartMenu()
        {
            var startMenu = SetupStartMenu();
            guiSystem.ActiveScreen = startMenu;
        }

        private Screen SetupStartMenu()
        {
            const int menuWidth = 400;
            const int menuHeight = 200;
            var menuPanel = new StackPanel
            {
                Width = menuWidth,
                Height = menuHeight,
                Position = guiSystem.BoundingRectangle.Center - new Point(menuWidth / 2, menuHeight / 2)
            };

            var startLocalButton = new Button
            {
                Content = "Start local game"
            };
            startLocalButton.Clicked += delegate
            {
                Hub.Default.Publish(new StartGameMessage(new GameOptions
                {
                    OnlineType = OnlineType.Local
                }));
            };
            menuPanel.Items.Add(startLocalButton);

            var startLanGame = new Button
            {
                Content = "Start LAN game"
            };
            startLanGame.Clicked += StartLanGameClicked;
            menuPanel.Items.Add(startLanGame);

            var joinLanGame = new Button
            {
                Content = "Join LAN game"
            };
            joinLanGame.Clicked += JoinLanGameClicked;
            menuPanel.Items.Add(joinLanGame);

            var startMenu = new Screen
            {
                Content = new Canvas
                {
                    Items = {menuPanel}
                }
            };
            return startMenu;
        }

        private void StartLanGameClicked(object? sender, EventArgs e)
        {
            guiSystem.ActiveScreen.Hide();
            if (netServer.Start(port)) {
                Hub.Default.Publish(new StartGameMessage(new GameOptions {OnlineType = OnlineType.LAN}));
            } else {
                guiSystem.ActiveScreen.Show();
            }
        }

        private void JoinLanGameClicked(object? sender, EventArgs args)
        {
            netClient.Start();
            joinedPeer = netClient.Connect("localhost", port, "BunnyLand");
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
