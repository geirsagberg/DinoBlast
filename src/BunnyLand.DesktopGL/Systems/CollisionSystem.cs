using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class CollisionSystem : EntityProcessingSystem
    {
        private readonly Variables variables;
        private const int LogCollisionDetectionEveryNthFrame = 60;
        private ComponentMapper<CollisionBody> bodyMapper;
        private ComponentMapper<Level> levelMapper;
        private ComponentMapper<Movable> movableMapper;

        public Option<Level> Level { get; set; }

        public Bag<CollisionBody> Bodies { get; } = new Bag<CollisionBody>();

        public CollisionSystem(Variables variables) : base(Aspect.All(typeof(CollisionBody)))
        {
            this.variables = variables;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            bodyMapper = mapperService.GetMapper<CollisionBody>();
            movableMapper = mapperService.GetMapper<Movable>();
            levelMapper = mapperService.GetMapper<Level>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            levelMapper.TryGet(entityId).IfSome(level => {
                Level = level;
            });
            bodyMapper.TryGet(entityId).IfSome(body => Bodies.Add(body));
        }

        protected override void OnEntityRemoved(int entityId)
        {
            bodyMapper.TryGet(entityId).IfSome(body => Bodies.Remove(body));
        }

        private readonly Stopwatch stopwatch = new Stopwatch();

        public override void Begin()
        {
            stopwatch.Restart();
            checkedPairs.Clear();
        }

        private System.Collections.Generic.HashSet<(CollisionBody, CollisionBody)> checkedPairs = new System.Collections.Generic.HashSet<(CollisionBody, CollisionBody)>();

        public override void End()
        {
            stopwatch.Stop();
            timeSpans[timeSpanCounter] = stopwatch.Elapsed;
            timeSpanCounter = (timeSpanCounter + 1) % LogCollisionDetectionEveryNthFrame;
            if (timeSpanCounter == 0) {
                Console.WriteLine($"Avg time collision detection: {timeSpans.Average(ts => ts.TotalMilliseconds):N} ms");
            }
        }

        private int timeSpanCounter = 0;
        private readonly TimeSpan[] timeSpans = new TimeSpan[LogCollisionDetectionEveryNthFrame];

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
                    var potentialCollisions = Bodies.Where(b =>
                        b != body && b.CollidesWith.HasFlag(body.ColliderType) && !checkedPairs.Contains((b, body)) && b.Bounds.Intersects(collisionBounds)).ToList();

                    body.Collisions = potentialCollisions
                        .Select(other => (other, body.CalculatePenetrationVector(other, elapsedTicks)))
                        .Where(t => t.Item2 != Vector2.Zero).ToList();
                    potentialCollisions.ForEach(b => checkedPairs.Add((body, b)));
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
