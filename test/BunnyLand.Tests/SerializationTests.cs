using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Resources;
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
        public void Can_serialize_and_deserialize_entities()
        {
            var componentManager = new ComponentManager();
            var entityManager = new EntityManager(componentManager);
            var textures = new Textures();
            var entityFactory = new EntityFactory(textures);


            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(200, 400), PlayerIndex.One);
            entityFactory.CreatePlayer(entityManager.Create(), new Vector2(300, 400), PlayerIndex.Two);

            var serializableMapper = componentManager.GetMapper<Serializable>();
            var transformMapper = componentManager.GetMapper<Transform2>();
            var movableMapper = componentManager.GetMapper<Movable>();

            var serializables = new List<Serializable>();
            var transforms = new Dictionary<int, Transform2>();
            var movables = new Dictionary<int, Movable>();

            foreach (var entity in entityManager.Entities) {
                serializables.Add(serializableMapper.Get(entity));
                transformMapper.TryGet(entity).IfSome(transform => transforms[entity] = transform);
                movableMapper.TryGet(entity).IfSome(movable => movables[entity] = movable);
            }

            var serializableTransforms = transforms.ToDictionary(kvp => kvp.Key,
                kvp => new SerializableTransform { Position = kvp.Value.Position, Rotation = kvp.Value.Rotation, Scale = kvp.Value.Scale });

            var serializer = new Serializer();

            var state = new FullGameState(serializer) {
                Components = new SerializableComponents(serializables, serializableTransforms, movables)
            };
            var writer = new NetDataWriter();


            state.Serialize(writer);
            var bytes = writer.CopyData();
            var reader = new NetDataReader(bytes);
            var target = new FullGameState(serializer);
            target.Deserialize(reader);


            target.Components.Should().BeEquivalentTo(state.Components);
        }
    }
}
