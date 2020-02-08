using System;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Resources;
using BunnyLand.DesktopGL.Screens;
using BunnyLand.DesktopGL.Systems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Screens;

namespace BunnyLand.DesktopGL
{
    public class BunnyGame : Game
    {
        private readonly GameSettings gameSettings;
        private IServiceScope? serviceScope;

        protected GraphicsDeviceManager Graphics { get; }

        private new IServiceProvider Services { get; set; } = null!;

        public BunnyGame(GameSettings gameSettings)
        {
            IsMouseVisible = true;
            Graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = gameSettings.Width,
                PreferredBackBufferHeight = gameSettings.Height,
                PreferMultiSampling = true
            };
            Content.RootDirectory = "Content";
            this.gameSettings = gameSettings;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Configure services here if they should live for the entire game. Handles Dependency Injection and instance creation.
            // Services that are added and removed dynamically must use GameServiceContainer, which only supports adding and retrieving instances, not creating.

            services.Scan(scan => {
                scan.FromAssemblyOf<BunnyGame>()
                    .AddClasses().AsSelf().WithScopedLifetime();
            });
            services.AddSingleton(gameSettings);
            services.AddSingleton(GraphicsDevice);
            services.AddSingleton(Content);
            services.AddSingleton<Game>(this);
            services.AddTransient<WorldBuilder>();
            services.AddTransient<SpriteBatch>();

            services.AddSingleton(provider => provider.CreateWorld()
                .AddSystemService<BattleFieldSystem>()
                .AddSystemService<RenderSystem>()
                .AddSystemService<PlayerSystem>()
                .Build());
            services.AddSingleton(provider => {
                var textures = new Textures(provider.GetRequiredService<ContentManager>());
                textures.Load();
                return textures;
            });
            services.AddSingleton<ScreenManager>();
        }

        private T GetService<T>() => Services.GetRequiredService<T>();

        protected override void Initialize()
        {
            InitializeServices();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Services.RegisterComponent<World>();
            var screenManager = Services.RegisterComponent<ScreenManager>();
            screenManager.LoadScreen(GetService<BattleScreen>());
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
