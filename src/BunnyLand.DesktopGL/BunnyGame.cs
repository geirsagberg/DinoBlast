using System;
using System.Linq;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Resources;
using BunnyLand.DesktopGL.Screens;
using BunnyLand.DesktopGL.Systems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collisions;
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
        private IServiceScope? serviceScope;

        protected GraphicsDeviceManager Graphics { get; }

        private new IServiceProvider Services { get; set; }

        public BunnyGame(GameSettings gameSettings)
        {
            IsMouseVisible = true;
            Graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = gameSettings.Width,
                PreferredBackBufferHeight = gameSettings.Height,
                PreferMultiSampling = true,
                SynchronizeWithVerticalRetrace = gameSettings.VSyncEnabled,
            };
            Content.RootDirectory = "Content";
            IsFixedTimeStep = gameSettings.FixedTimeStep;

            this.gameSettings = gameSettings;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure services here if they should live for the entire game. Handles Dependency Injection and instance creation.
            // Services that are added and removed dynamically must use GameServiceContainer, which only supports adding and retrieving instances, not creating.

            services.Scan(scan => {
                scan.FromAssemblyOf<BunnyGame>()
                    .AddClasses().AsSelf().WithScopedLifetime()
                    .AddClasses().AsImplementedInterfaces().WithScopedLifetime()
                    ;
            });
            services.AddSingleton(gameSettings);
            services.AddSingleton(GraphicsDevice);
            services.AddSingleton(Content);
            services.AddSingleton<Game>(this);
            services.AddTransient<WorldBuilder>();
            services.AddTransient<SpriteBatch>();

            services.AddSingleton(BuildWorld);
            services.AddSingleton(provider => {
                var textures = new Textures(provider.GetRequiredService<ContentManager>());
                textures.Load();
                return textures;
            });
            services.AddSingleton(provider => {
                var spriteFonts = new SpriteFonts(provider.GetRequiredService<ContentManager>());
                spriteFonts.Load();
                return spriteFonts;
            });
            services.AddSingleton<ScreenManager>();
            services.AddSingleton(new KeyboardListener(new KeyboardListenerSettings {RepeatPress = false}));
            services.AddSingleton(new GamePadListener(new GamePadListenerSettings(PlayerIndex.One)));
            services.AddSingleton(new GamePadListener(new GamePadListenerSettings(PlayerIndex.Two)));
            services.AddSingleton(new GamePadListener(new GamePadListenerSettings(PlayerIndex.Three)));
            services.AddSingleton(new GamePadListener(new GamePadListenerSettings(PlayerIndex.Four)));
            services.AddSingleton(provider =>
                provider.GetServices<GamePadListener>().Cast<InputListener>()
                    .Concat(provider.GetServices<KeyboardListener>()).ToArray());
            services.AddSingleton<InputListenerComponent>();

            services.AddSingleton<ViewportAdapter, DefaultViewportAdapter>();
            services.AddSingleton<IGuiRenderer>(provider =>
                new GuiSpriteBatchRenderer(provider.GetRequiredService<GraphicsDevice>(), () => Matrix.Identity));
            services.AddSingleton<GuiSystem>();
            services.AddSingleton<Variables>();
            services.AddSingleton<tainicom.Aether.Physics2D.Dynamics.World>();
        }

        private static World BuildWorld(IServiceProvider provider)
        {
            return provider.CreateWorld()
                .AddSystemService<InputSystem>()
                .AddSystemService<PlayerSystem>()
                .AddSystemService<GravitySystem>()
                .AddSystemService<PhysicsSystem>()
                .AddSystemService<CollisionSystem>()
                .AddSystemService<RenderSystem>()
                .Build();
        }

        private T GetService<T>() => Services.GetRequiredService<T>();

        protected override void Initialize()
        {
            InitializeServices();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Services.RegisterGameComponent<InputListenerComponent>();
            Services.RegisterGameComponent<World>();
            Services.RegisterGameComponent<ScreenManager>();

            var bitmapFont = Content.Load<BitmapFont>("Fonts/bryndan-medium");
            Skin.CreateDefault(bitmapFont);

            LoadScreen<BattleScreen>();
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
            serviceScope = provider.CreateScope();
            Services = serviceScope.ServiceProvider;
        }

        protected override void Dispose(bool disposing)
        {
            serviceScope?.Dispose();

            base.Dispose(disposing);
        }
    }
}
