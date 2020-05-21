using System;
using System.Linq;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Resources;
using BunnyLand.DesktopGL.Screens;
using BunnyLand.DesktopGL.Services;
using BunnyLand.DesktopGL.Systems;
using BunnyLand.DesktopGL.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using Screen = MonoGame.Extended.Screens.Screen;

namespace BunnyLand.DesktopGL
{
    public class BunnyGame : Game
    {
        private readonly GameSettings gameSettings;

        internal GraphicsDeviceManager Graphics { get; }

        internal new IServiceProvider Services { get; set; } = null!;

        private MessageHub MessageHub => Services.GetRequiredService<MessageHub>();

        public BunnyGame(GameSettings gameSettings)
        {
            IsMouseVisible = true;
            Graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = gameSettings.Width,
                PreferredBackBufferHeight = gameSettings.Height,
                PreferMultiSampling = true,
                SynchronizeWithVerticalRetrace = gameSettings.VSyncEnabled,
                IsFullScreen = gameSettings.FullScreen
            };

            Content.RootDirectory = "Content";
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1f / 60f);

            this.gameSettings = gameSettings;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure services here if they should live for the entire game. Handles Dependency Injection and instance creation.
            // Services that are added and removed dynamically must use GameServiceContainer, which only supports adding and retrieving instances, not creating.

            services.Scan(scan => {
                scan.FromAssemblyOf<BunnyGame>()
                    .AddClasses().AsSelf().WithSingletonLifetime()
                    .AddClasses().AsImplementedInterfaces().WithSingletonLifetime()
                    ;
            });
            services.AddSingleton(gameSettings);
            services.AddSingleton(GraphicsDevice);
            services.AddSingleton(Content);
            services.AddSingleton<Game>(this);
            services.AddTransient<WorldBuilder>();
            services.AddTransient<BunnyWorld>();
            services.AddTransient<SpriteBatch>();

            services.AddSingleton<Random>();

            services.AddSingleton(BuildWorld);
            services.AddSingleton(provider => {
                var loader = provider.GetRequiredService<ResourceLoader>();
                var textures = new Textures();
                loader.Load<Texture2D>(textures);
                return textures;
            });
            services.AddSingleton(provider => {
                var spriteFonts = new SpriteFonts(provider.GetRequiredService<ContentManager>());
                spriteFonts.Load();
                return spriteFonts;
            });
            services.AddSingleton<ScreenManager>();

            services.AddSingleton(provider => new MouseListener(provider.GetService<ViewportAdapter>()));
            services.AddSingleton(new KeyboardListener(new KeyboardListenerSettings { RepeatPress = false }));
            services.AddSingleton(new GamePadListener(new GamePadListenerSettings(PlayerIndex.One)));
            services.AddSingleton(new GamePadListener(new GamePadListenerSettings(PlayerIndex.Two)));
            services.AddSingleton(new GamePadListener(new GamePadListenerSettings(PlayerIndex.Three)));
            services.AddSingleton(new GamePadListener(new GamePadListenerSettings(PlayerIndex.Four)));
            services.AddSingleton(provider =>
                new InputListener[] { provider.GetService<MouseListener>(), provider.GetService<KeyboardListener>() }
                    .Concat(provider.GetServices<GamePadListener>()).ToArray());
            services.AddSingleton<InputListenerComponent>();

            services.AddSingleton<ViewportAdapter, DefaultViewportAdapter>();
            services.AddSingleton<IGuiRenderer>(provider =>
                new GuiSpriteBatchRenderer(provider.GetRequiredService<GraphicsDevice>(), () => Matrix.Identity));
            services.AddSingleton<GuiSystem>();
        }

        private static World BuildWorld(IServiceProvider provider) => provider.CreateWorld()
            .AddSystemService<NetServerSystem>()
            .AddSystemService<NetClientSystem>()
            .AddSystemService<LifetimeSystem>()
            .AddSystemService<InputSystem>()
            .AddSystemService<PlayerSystem>()
            .AddSystemService<AcceleratorSystem>()
            .AddSystemService<EmitterSystem>()
            .AddSystemService<GravitySystem>()
            .AddSystemService<PhysicsSystem>()
            .AddSystemService<CollisionSystem>()
            .AddSystemService<BattleSystem>()
            .AddSystemService<RenderSystem>()
            .Build();

        private T GetService<T>() => Services.GetRequiredService<T>();

        protected override void Initialize()
        {
            InitializeServices();

            GetService<DebugLogger>().Connect();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Services.RegisterGameComponent<InputListenerComponent>();
            Services.RegisterGameComponent<World>();
            Services.RegisterGameComponent<ScreenManager>();

            var bitmapFont = Content.Load<BitmapFont>("Fonts/bryndan-medium");
            Skin.CreateDefault(bitmapFont);

            MessageHub.Subscribe<StartGameMessage>(msg => {
                LoadScreen<BattleScreen>();
                MessageHub.Publish(new ResetWorldMessage(msg.GameState));
            });
            MessageHub.Subscribe<ServerDisconnectedMessage>(msg => { LoadScreen<MenuScreen>(); });

            LoadScreen<MenuScreen>();
        }

        private void LoadScreen<T>() where T : Screen
        {
            GetService<ScreenManager>().LoadScreen(GetService<T>());
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var provider = services.BuildServiceProvider();
            Services = provider;
        }
    }
}
