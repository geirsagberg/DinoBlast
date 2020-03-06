using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using BunnyLand.DesktopGL.Messages;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using PubSub;

namespace BunnyLand.DesktopGL.Systems
{
    public class CollisionSystem : EntityProcessingSystem
    {
        private const int LogCollisionDetectionEveryNthFrame = 60;

        private readonly System.Collections.Generic.HashSet<(int entityA, int entityB)> checkedPairs =
            new System.Collections.Generic.HashSet<(int entityA, int entityB)>();

        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly TimeSpan[] timeSpans = new TimeSpan[LogCollisionDetectionEveryNthFrame];
        private readonly Variables variables;
        private ComponentMapper<CollisionBody> bodyMapper;
        private ComponentMapper<Damaging> damagingMapper;
        private ComponentMapper<Health> healthMapper;
        private ComponentMapper<Level> levelMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Player> playerMapper;

        private int timeSpanCounter;

        public Option<Level> Level { get; set; }

        public Dictionary<int, CollisionBody> Bodies { get; } = new Dictionary<int, CollisionBody>();

        public CollisionSystem(Variables variables) : base(Aspect.All(typeof(CollisionBody)))
        {
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            bodyMapper = mapperService.GetMapper<CollisionBody>();
            movableMapper = mapperService.GetMapper<Movable>();
            levelMapper = mapperService.GetMapper<Level>();
            healthMapper = mapperService.GetMapper<Health>();
            damagingMapper = mapperService.GetMapper<Damaging>();
            playerMapper = mapperService.GetMapper<Player>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            levelMapper.TryGet(entityId).IfSome(level => { Level = level; });
            bodyMapper.TryGet(entityId).IfSome(body => Bodies.Add(entityId, body));
        }

        protected override void OnEntityRemoved(int entityId)
        {
            bodyMapper.TryGet(entityId).IfSome(body => Bodies.Remove(entityId));
        }

        public override void Begin()
        {
            stopwatch.Restart();
            checkedPairs.Clear();
        }

        public override void End()
        {
            stopwatch.Stop();
            timeSpans[timeSpanCounter] = stopwatch.Elapsed;
            timeSpanCounter = (timeSpanCounter + 1) % LogCollisionDetectionEveryNthFrame;
            if (timeSpanCounter == 0) {
                Console.WriteLine(
                    $"Avg time collision detection: {timeSpans.Average(ts => ts.TotalMilliseconds):N} ms");
            }
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var elapsedTicks = gameTime.GetElapsedTicks(variables);

            bodyMapper.TryGet(entityId).IfSome(body => {
                movableMapper.TryGet(entityId).IfSome(movable => {
                    var collisionBounds = body.Bounds switch {
                        CircleF circle => FindCollisionBounds(movable, circle.ToRectangleF(), elapsedTicks),
                        RectangleF rectangle => FindCollisionBounds(movable, rectangle, elapsedTicks),
                        _ => throw new Exception("Unknown shape")
                    };
                    body.CollisionBounds = collisionBounds;

                    // TODO: If performance becomes a problem, look into broadphase algorithms like SAP or Dynamic tree, or separate collision tables per collidertype
                    var potentialCollisions = Bodies.Where(kvp => {
                        var b = kvp.Value;
                        return b != body && b.CollidesWith.HasFlag(body.ColliderType)
                            // && !checkedPairs.Contains((kvp.Key, entityId))
                            && b.Bounds.Intersects(collisionBounds);
                    }).ToList();

                    body.Collisions = potentialCollisions
                        .Select(other => (other.Key, body.CalculatePenetrationVector(other.Value)))
                        .Where(t => t.Item2 != Vector2.Zero).ToList();
                    // potentialCollisions.ForEach(b => checkedPairs.Add((entityId, b.Key)));

                    foreach (var (other, penetrationVector) in body.Collisions) {
                        damagingMapper.TryGet(entityId).IfSome(damaging =>
                            healthMapper.TryGet(other).IfSome(health => {
                                health.CurrentHealth -= damaging.Damage;
                                if (health.CurrentHealth < 0) {
                                    DestroyEntity(other);
                                    playerMapper.TryGet(other).IfSome(player =>
                                        Hub.Default.Publish(new RespawnPlayerMessage(player.PlayerIndex)));
                                }
                            }));
                        var otherBody = bodyMapper.Get(other);
                        switch (body.ColliderType) {
                            case ColliderTypes.Player when otherBody.ColliderType == ColliderTypes.Static:
                                movable.Transform.Position += penetrationVector;
                                movable.Velocity += penetrationVector / elapsedTicks;
                                break;
                            case ColliderTypes.Projectile:
                                movable.Transform.Position += penetrationVector;
                                DestroyEntity(entityId);
                                break;
                        }
                    }
                });
            });
        }


        private static RectangleF FindCollisionBounds(Option<Movable> maybeMovable, RectangleF rectangle,
            float elapsedTicks)
        {
            // If it is moving, we have to include the path from here to there in the potential collision
            return maybeMovable.Some(movable => rectangle.Expand(movable.Velocity * elapsedTicks))
                .None(rectangle);
        }
    }
}
