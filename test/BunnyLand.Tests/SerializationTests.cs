using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Serialization;
using FluentAssertions;
using LiteNetLib.Utils;
using MessagePack;
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
            var components = new SerializableGenericComponents(new Dictionary<int, List<ISerializableComponent>> {
                {
                    1, new List<ISerializableComponent> {
                        new PlayerInput(),
                        new SpriteInfo(SpriteType.Anki, new Size())
                    }
                }, {
                    2, new List<ISerializableComponent> {
                        new CollisionBody()
                    }
                }
            });

            var bytes = MessagePackSerializer.Serialize(components);
            var deserialized = MessagePackSerializer.Deserialize<SerializableGenericComponents>(bytes);

            deserialized.Should().BeEquivalentTo(components);
        }


        [Fact]
        public void Can_serialize_and_deserialize_entities()
        {
            var componentManager = new ComponentManager();
            var entityManager = new EntityManager(componentManager);
            var entityFactory = new EntityFactory();

            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(200, 400), 1, PlayerIndex.One);
            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(300, 400), 2, PlayerIndex.Two);

            var state = FullGameState.CreateFullGameState(new Serializer(), componentManager, entityManager.Entities);

            var writer = new NetDataWriter();

            writer.Put(state);
            var bytes = writer.CopyData();
            var reader = new NetDataReader(bytes);
            var target = new FullGameState(new Serializer());
            target.Deserialize(reader);

            target.Components.Should()
                .BeEquivalentTo(state.Components, o => o.Excluding(c => c.SelectedMemberPath.EndsWith(nameof(CollisionBody.OldPosition))));
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
