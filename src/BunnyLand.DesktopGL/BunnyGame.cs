using System;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Resources;
using BunnyLand.DesktopGL.Systems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;

namespace BunnyLand.DesktopGL
{
    public class BunnyGame : Game
    {
        private readonly GameSettings gameSettings;
        private IServiceScope? serviceScope;
        private World world = null!;

        protected GraphicsDeviceManager Graphics { get; }

        private new IServiceProvider Services { get; set; } = null!;

        public BunnyGame(GameSettings gameSettings)
        {
            IsMouseVisible = true;
            Graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = gameSettings.Width,
                PreferredBackBufferHeight = gameSettings.Height
            };
            Content.RootDirectory = "Content";
            this.gameSettings = gameSettings;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.Scan(scan => {
                scan.FromAssemblyOf<BunnyGame>()
                    .AddClasses().AsSelf().WithScopedLifetime();
            });
            services.AddSingleton(gameSettings);
            services.AddSingleton(GraphicsDevice);
            services.AddSingleton(Content);
            services.AddTransient<WorldBuilder>();
            services.AddTransient<SpriteBatch>();

            services.AddSingleton(provider => provider.BuildWorld()
                .AddSystemService<BattleFieldSystem>()
                .AddSystemService<RenderSystem>()
                .AddSystemService<PlayerSystem>()
                .Build());
            services.AddSingleton(provider => {
                var textures = new Textures(provider.GetRequiredService<ContentManager>());
                textures.Load();
                return textures;
            });
        }

        protected override void LoadContent()
        {
            world = Services.GetRequiredService<World>();

            Services.GetRequiredService<EntityFactory>().CreatePlayer();
        }

        protected override void Initialize()
        {
            InitializeServices();

            base.Initialize();
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var provider = services.BuildServiceProvider();
            serviceScope = provider.CreateScope();
            Services = serviceScope.ServiceProvider;
        }

        protected override void Update(GameTime gameTime)
        {
            world.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            world.Draw(gameTime);

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            serviceScope?.Dispose();

            base.Dispose(disposing);
        }

        //
        // private Scene CreateBattleScene()
        // {
        //     var scene = new BattleScene(gameSettings);
        //     return scene;
        //     // var scene = Scene.CreateWithDefaultRenderer(Color.CornflowerBlue);
        //     // var textures = new Textures(scene.Content);
        //     // var sw = Stopwatch.StartNew();
        //     // textures.Load();
        //     // Console.WriteLine("Loaded textures in: " + sw.Elapsed);
        //     //
        //     //
        //     //
        //     // AddBlackHole(scene, textures);
        //     //
        //     //
        //     //
        //     //
        //     // return scene;
        // }
        //
        // private static void AddBlackHole(Scene scene, Textures textures)
        // {
        //     var blackHole = scene.CreateEntity("blackHole");
        //     blackHole.AddComponent(new SpriteRenderer(textures.blackhole));
        //     blackHole.AddComponent(new BlackHoleRotator());
        //     blackHole.Transform.Position = Screen.Center;
        // }

        // private readonly GraphicsDeviceManager graphics;
        // private readonly GameSettings settings;
        // private Vector2 blackHolePosition;
        // private float blackHoleRotation;
        // private SpriteBatch spriteBatch = null!;
        //
        // public BunnyGame(GameSettings settings)
        // {
        //     this.settings = settings;
        //     graphics = new GraphicsDeviceManager(this);
        //     Content.RootDirectory = "Content";
        //     IsMouseVisible = true;
        //     Textures = new Textures(Content);
        //     SpriteFonts = new SpriteFonts(Content);
        //     SoundEffects = new SoundEffects(Content);
        //     Songs = new Songs(Content);
        // }
        //
        // private Textures Textures { get; }
        // private SpriteFonts SpriteFonts { get; }
        // private SoundEffects SoundEffects { get; }
        // private Songs Songs { get; }
        //
        // protected override void Initialize()
        // {
        //
        //     // TODO: Add your initialization logic here
        //     blackHolePosition = new Vector2(graphics.PreferredBackBufferWidth / 2f,
        //         graphics.PreferredBackBufferHeight / 2f);
        //
        //     base.Initialize();
        // }
        //
        // protected override void LoadContent()
        // {
        //     spriteBatch = new SpriteBatch(GraphicsDevice);
        //
        //     Textures.Load();
        //     SpriteFonts.Load();
        // }
        //
        // private string debugText = "";
        // private MouseState previousMouseState;
        // private Vector2 startDragPosition;
        //
        // protected override void Update(GameTime gameTime)
        // {
        //     if (!IsActive) return;
        //
        //     if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
        //         Keyboard.GetState().IsKeyDown(Keys.Escape))
        //         Exit();
        //
        //     // TODO: Add your update logic here
        //
        //     blackHoleRotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalMilliseconds * 0.001);
        //
        //     var mouseState = Mouse.GetState();
        //
        //     if (previousMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed) {
        //         startDragPosition = mouseState.Position.ToVector2();
        //     } else if (previousMouseState.LeftButton == ButtonState.Pressed &&
        //         mouseState.LeftButton == ButtonState.Released)
        //         startDragPosition = default;
        //
        //     previousMouseState = mouseState;
        //
        //     debugText = JsonSerializer.Serialize(mouseState);
        //
        //     base.Update(gameTime);
        // }
        //
        // protected override void Draw(GameTime gameTime)
        // {
        //     GraphicsDevice.Clear(Color.CornflowerBlue);
        //
        //     // TODO: Add your drawing code here
        //     spriteBatch.Begin();
        //     spriteBatch.Draw(Textures.blackhole, blackHolePosition, null, Color.White, blackHoleRotation,
        //         new Vector2(Textures.blackhole.Width / 2f, Textures.blackhole.Height / 2f), Vector2.One,
        //         SpriteEffects.FlipHorizontally,
        //         0f);
        //     spriteBatch.DrawString(SpriteFonts.Verdana, debugText, Vector2.Zero, Color.Black);
        //     if (previousMouseState.LeftButton == ButtonState.Pressed)
        //         spriteBatch.DrawLine(Color.Chartreuse, startDragPosition, previousMouseState.Position.ToVector2());
        //     spriteBatch.End();
        //
        //     base.Draw(gameTime);
        // }
    }
}
