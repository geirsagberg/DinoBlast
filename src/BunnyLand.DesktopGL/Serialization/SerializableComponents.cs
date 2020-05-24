using System.Collections.Generic;
using BunnyLand.DesktopGL.Components;
using MessagePack;

namespace BunnyLand.DesktopGL.Serialization
{
    [MessagePackObject]
    public class SerializableComponents
    {
        [Key(0)] public HashSet<int> EntityIds { get; }
        [Key(1)] public Dictionary<int, SerializableTransform> Transforms { get; }
        [Key(2)] public Dictionary<int, Movable> Movables { get; }
        [Key(3)] public Dictionary<int, SpriteInfo> SpriteInfos { get; }
        [Key(4)] public Dictionary<int, CollisionBody> CollisionBodies { get; }
        [Key(5)] public Dictionary<int, Damaging> Damagings { get; }
        [Key(6)] public Dictionary<int, GravityField> GravityFields { get; }
        [Key(7)] public Dictionary<int, GravityPoint> GravityPoints { get; }
        [Key(8)] public Dictionary<int, Health> Healths { get; }
        [Key(9)] public Dictionary<int, PlayerInput> PlayerInputs { get; }
        [Key(10)] public Dictionary<int, Level> Levels { get; }
        [Key(11)] public Dictionary<int, PlayerState> PlayerStates { get; }
        [Key(12)] public Dictionary<int, Emitter> Emitters { get; }
        [Key(13)] public Dictionary<int, Lifetime> Lifetimes { get; }

        public SerializableComponents(HashSet<int> entityIds, Dictionary<int, SerializableTransform> transforms,
            Dictionary<int, Movable> movables, Dictionary<int, SpriteInfo> spriteInfos, Dictionary<int, CollisionBody> collisionBodies,
            Dictionary<int, Damaging> damagings, Dictionary<int, GravityField> gravityFields, Dictionary<int, GravityPoint> gravityPoints,
            Dictionary<int, Health> healths, Dictionary<int, PlayerInput> playerInputs, Dictionary<int, Level> levels,
            Dictionary<int, PlayerState> playerStates, Dictionary<int, Emitter> emitters, Dictionary<int, Lifetime> lifetimes)
        {
            EntityIds = entityIds;
            Transforms = transforms;
            Movables = movables;
            SpriteInfos = spriteInfos;
            CollisionBodies = collisionBodies;
            Damagings = damagings;
            GravityFields = gravityFields;
            GravityPoints = gravityPoints;
            Healths = healths;
            PlayerInputs = playerInputs;
            Levels = levels;
            PlayerStates = playerStates;
            Emitters = emitters;
            Lifetimes = lifetimes;
        }
    }
}
