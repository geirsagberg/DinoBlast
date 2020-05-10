using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Serialization;
using FluentAssertions;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using Xunit;

namespace BunnyLand.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void Can_serialize_and_deserialize_components()
        {
            var components = new SerializableGenericComponents(new Dictionary<int, List<object>> {
                {1, new List<object> {
                    new PlayerInput(),
                    new SpriteInfo(SpriteType.Anki, new Size())
                }},
                {2, new List<object> {
                    new CollisionBody()
                }}
            });
        }


        [Fact]
        public void Can_serialize_and_deserialize_entities()
        {
            var componentManager = new ComponentManager();
            var entityManager = new EntityManager(componentManager);
            var entityFactory = new EntityFactory();


            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(200, 400), 1, PlayerIndex.One);
            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(300, 400), 2, PlayerIndex.Two);

            var serializableMapper = componentManager.GetMapper<Serializable>();
            var transformMapper = componentManager.GetMapper<Transform2>();
            var movableMapper = componentManager.GetMapper<Movable>();
            var spriteInfoMapper = componentManager.GetMapper<SpriteInfo>();

            var state = FullGameState.CreateFullGameState(new Serializer(), entityManager.Entities, serializableMapper, transformMapper, movableMapper,
                spriteInfoMapper);

            var writer = new NetDataWriter();

            state.Serialize(writer);
            var bytes = writer.CopyData();
            var reader = new NetDataReader(bytes);
            var target = new FullGameState(new Serializer());
            target.Deserialize(reader);


            target.Components.Should().BeEquivalentTo(state.Components);
        }

        [Fact]
        public void Serialized_HashSet_is_smaller_than_dictionary()
        {
            var range = Enumerable.Range(0, 1000).ToList();
            var hashSet = range.ToHashSet();
            var dictionary = range.ToDictionary(r => r);
            var serializer = new Serializer();

            serializer.Serialize(hashSet).Length.Should().BeLessThan(serializer.Serialize(dictionary).Length);
        }
    }
}
