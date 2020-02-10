using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Screens;

namespace BunnyLand.DesktopGL.Screens
{
    public class BattleScreen : GameScreen
    {
        private readonly EntityFactory entityFactory;
        private readonly World world;
        private readonly GameSettings gameSettings;

        public BattleScreen(Game game, EntityFactory entityFactory, World world, GameSettings gameSettings) : base(game)
        {
            this.entityFactory = entityFactory;
            this.world = world;
            this.gameSettings = gameSettings;
        }

        public override void LoadContent()
        {
            entityFactory.CreatePlanet(new Vector2(400, 400), 8000, 0.5f);
            entityFactory.CreatePlanet(new Vector2(800, 300), 12000, 0.8f);
            entityFactory.CreatePlayer(new Vector2(100, 100));
            entityFactory.CreateLevel(gameSettings.Width, gameSettings.Height);
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
        }
    }
}
