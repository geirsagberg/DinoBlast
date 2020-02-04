using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Resources;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace BunnyLand.DesktopGL.Scenes
{
    public class BattleScene : Scene
    {
        private readonly GameSettings gameSettings;
        private readonly List<Sprite> playerSprites;
        private SpriteAnimation runAnimation;

        public BattleScene(GameSettings gameSettings)
        {
            this.gameSettings = gameSettings;
            Textures = new Textures(Content);
            Textures.Load();


            playerSprites = Sprite.SpritesFromAtlas(Textures.PlayerAnimation, 35, 50);
            runAnimation = new SpriteAnimation(playerSprites.ToArray()[1..9], 15);

            CreateBlackHole(Screen.Center);
            CreatePlayer();
        }

        private void CreatePlayer()
        {
            var player = CreateEntity("player1");

            // player.AddComponent(new SpriteRenderer(playerSprites[0]));
            player.Position = Screen.Center + new Vector2(0, Screen.Height / 4f);
            var spriteAnimator = new SpriteAnimator();
            spriteAnimator.AddAnimation("run", runAnimation);
            player.AddComponent(spriteAnimator);
            spriteAnimator.Play("run");
        }

        private void CreateBlackHole(Vector2 position)
        {
            var blackHole = CreateEntity("blackHole");
            blackHole.Position = position;
            blackHole.AddComponent(new SpriteRenderer(Textures.blackhole));
            blackHole.AddComponent(new BlackHoleRotator());
        }

        public Textures Textures { get; }
    }
}
