using System;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Extensions;
using LanguageExt;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using tainicom.Aether.Physics2D.Dynamics;
using World = tainicom.Aether.Physics2D.Dynamics.World;

namespace BunnyLand.DesktopGL.Systems
{
    /// <summary>
    ///     Handles adding <see cref="CollisionBody" /> components to the <see cref="CollisionComponent" />, and resetting
    ///     their collision info. Collision checking itself is handled within <see cref="CollisionComponent" />.
    /// </summary>
    public class CollisionSystem : EntityProcessingSystem
    {
        private ComponentMapper<CollisionBody> collisionBodyMapper;
        private ComponentMapper<Level> levelMapper;
        private ComponentMapper<Movable> movableMapper;
        private ComponentMapper<Body> bodyMapper;

        public Option<Level> Level { get; set; }

        public Bag<CollisionBody> Bodies { get; } = new Bag<CollisionBody>();

        public World PhysicsWorld { get; }

        public CollisionSystem(World physicsWorld) : base(Aspect.All(typeof(CollisionBody)))
        {
            PhysicsWorld = physicsWorld;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            collisionBodyMapper = mapperService.GetMapper<CollisionBody>();
            movableMapper = mapperService.GetMapper<Movable>();
            levelMapper = mapperService.GetMapper<Level>();
            bodyMapper = mapperService.GetMapper<Body>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            levelMapper.TryGet(entityId).IfSome(level => {
                Level = level;


                // var component = new CollisionComponent(level.Bounds);
                // foreach (var entity in ActiveEntities) {
                //     component.Insert(bodyMapper.Get(entity));
                // }

                // CollisionComponent = component;
            });
            bodyMapper.TryGet(entityId).IfSome(body => {
                // PhysicsWorld.Add(body);


            });
            collisionBodyMapper.TryGet(entityId).IfSome(body => {
                // var physicsBody = body.Bounds switch {
                //     RectangleF rectangle => PhysicsWorld.CreateRectangle(rectangle.Width, rectangle.Height, 1,
                //         new Aether.Physics2D.Common.Maths.Vector2(rectangle.X, rectangle.Y),
                //         bodyType: BodyType.Dynamic),
                //     CircleF circle => PhysicsWorld.CreateCircle(circle.Radius, 1,
                //         new Aether.Physics2D.Common.Maths.Vector2(circle.Position.X, circle.Position.Y)),
                //     _ => throw new NotSupportedException()
                // };

                // Bodies.Add(body);
            });
            // CollisionComponent.IfSome(collisionComponent =>
            //     bodyMapper.TryGet(entityId).IfSome(collisionComponent.Insert));
        }

        protected override void OnEntityRemoved(int entityId)
        {

            bodyMapper.TryGet(entityId).IfSome(body => PhysicsWorld.Remove(body));

            // CollisionComponent.IfSome(collisionComponent =>
            //     bodyMapper.TryGet(entityId).IfSome(collisionComponent.Remove));
            collisionBodyMapper.TryGet(entityId).IfSome(body => Bodies.Remove(body));
        }

        public override void Update(GameTime gameTime)
        {

            PhysicsWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);

            base.Update(gameTime);
            // CollisionComponent.IfSome(component => component.Update(gameTime));
        }

        public override void Process(GameTime gameTime, int entityId)
        {

            bodyMapper.TryGet(entityId).IfSome(body => {
                Level.IfSome(level => body.Wrap(level.Bounds));
            });



            // collisionBodyMapper.TryGet(entityId).IfSome(body => {
            //     var maybeMovable = movableMapper.TryGet(entityId);
            //     var collisionBounds = body.Bounds switch {
            //         CircleF circle => FindCollisionBounds(maybeMovable, circle.ToRectangleF()),
            //         RectangleF rectangle => FindCollisionBounds(maybeMovable, rectangle),
            //         _ => throw new Exception("Unknown shape")
            //     };
            //     body.CollisionBounds = collisionBounds;
            //
            //     var potentialCollisions = Bodies.Where(b =>
            //         b.CollidesWith.HasFlag(body.ColliderType) && b != body && b.Bounds.Intersects(collisionBounds));
            //
            //     // TODO: Tunneling-safe check for point of impact
            //
            //     body.Collisions = potentialCollisions
            //         .Select(other => (other, body.Bounds.CalculatePenetrationVector(other.Bounds)))
            //         .Where(t => t.Item2 != Vector2.Zero).ToList();
            // });
        }


        private static RectangleF FindCollisionBounds(Option<Movable> maybeMovable, RectangleF rectangle)
        {
            // If it is moving, we have to include the path from here to there in the potential collision
            return maybeMovable.Some(movable => rectangle.Expand(movable.Velocity))
                .None(rectangle);
        }
    }
}
