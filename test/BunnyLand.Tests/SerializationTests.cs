using System;
using BunnyLand.DesktopGL;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.NetMessages;
using BunnyLand.DesktopGL.Serialization;
using FluentAssertions;
using LiteNetLib.Utils;
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
            var serializer = new Serializer();

            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(200, 400), 1, PlayerIndex.One);
            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(300, 400), 2, PlayerIndex.Two);

            var state = FullGameState.CreateFullGameState(componentManager, entityManager.Entities, 1, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(1));

            var writer = new NetDataWriter();

            writer.Put(serializer.Serialize(state));
            var bytes = writer.CopyData();
            var reader = new NetDataReader(bytes);
            var target = serializer.Deserialize<FullGameState>(reader.GetRemainingBytes());

            target.Components.Should()
                .BeEquivalentTo(state.Components, o => o
                    .Excluding(c => c.SelectedMemberPath.EndsWith(nameof(CollisionBody.OldPosition))
                        || c.SelectedMemberPath.EndsWith(nameof(PlayerState.LocalPlayerIndex))
                        || c.SelectedMemberPath.EndsWith(nameof(PlayerState.IsLocal))
                    )
                );
        }

        [Fact]
        public void Can_serialize_and_deserialize_InputUpdateNetMessage()
        {
            var msg = new InputUpdateNetMessage(1, new PlayerInput());
            var serializer = new Serializer();

            var bytes = serializer.Serialize(msg);
            var deserialized = serializer.Deserialize<InputUpdateNetMessage>(bytes);

            deserialized.Should().BeEquivalentTo(msg);
        }
    }
}
