using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Screens;

namespace BunnyLand.DesktopGL.Screens
{
    public class BattleScreen : GameScreen
    {
        private readonly EntityFactory entityFactory;
        private readonly World world;

        public BattleScreen(Game game, EntityFactory entityFactory, World world) : base(game)
        {
            this.entityFactory = entityFactory;
            this.world = world;

        }

        public override void LoadContent()
        {
            entityFactory.CreatePlanet(new Vector2(400, 400), 5000, 0.5f);
            entityFactory.CreatePlanet(new Vector2(800, 300), 8000, 0.8f);
            entityFactory.CreatePlayer(new Vector2(100,100));
            entityFactory.CreateLevel(1280, 720);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(GameTime gameTime)
        {

        }
    }
}
