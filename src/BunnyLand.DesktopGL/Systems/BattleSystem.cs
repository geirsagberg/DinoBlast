using System.Collections.Generic;
using BunnyLand.DesktopGL.Messages;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using PubSub;

namespace BunnyLand.DesktopGL.Systems
{
    public class BattleSystem : EntitySystem
    {
        private readonly HashSet<int> entities = new HashSet<int>();
        private readonly EntityFactory entityFactory;
        private readonly GameSettings gameSettings;

        public BattleSystem(EntityFactory entityFactory, GameSettings gameSettings) : base(Aspect.All())
        {
            this.entityFactory = entityFactory;
            this.gameSettings = gameSettings;
            Hub.Default.Subscribe((ResetWorldMessage _) => {
                foreach (var entity in entities) {
                    DestroyEntity(entity);
                }
                SetupEntities();
            });
        }

        private void SetupEntities()
        {
            entityFactory.CreateLevel(CreateEntity(), gameSettings.Width, gameSettings.Height);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(400, 400), 8000, 0.5f);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(800, 300), 12000, 0.8f);
            entityFactory.CreatePlanet(CreateEntity(), new Vector2(800, 600), 0, 0.05f);
            entityFactory.CreatePlayer(CreateEntity(), new Vector2(100, 100));
            entityFactory.CreateBlock(CreateEntity(), new RectangleF(600, 600, 10, 200));
        }

        protected override void OnEntityAdded(int entityId)
        {
            entities.Add(entityId);
        }

        protected override void OnEntityRemoved(int entityId)
        {
            entities.Remove(entityId);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
        }
    }
}
