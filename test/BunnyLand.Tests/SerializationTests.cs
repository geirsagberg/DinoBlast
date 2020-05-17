using System.Linq;
using BunnyLand.DesktopGL;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Serialization;
using FluentAssertions;
using LiteNetLib.Utils;
using MessagePack;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using Xunit;

namespace BunnyLand.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void Can_serialize_and_deserialize_entities()
        {
            var componentManager = new ComponentManager();
            var entityManager = new EntityManager(componentManager);
            var entityFactory = new EntityFactory();

            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(200, 400), 1, PlayerIndex.One);
            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(300, 400), 2, PlayerIndex.Two);

            var state = FullGameState.CreateFullGameState(componentManager, entityManager.Entities, 1);

            var writer = new NetDataWriter();

            writer.Put(MessagePackSerializer.Serialize(state));
            var bytes = writer.CopyData();
            var reader = new NetDataReader(bytes);
            var target = MessagePackSerializer.Deserialize<FullGameState>(reader.GetRemainingBytes());

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
