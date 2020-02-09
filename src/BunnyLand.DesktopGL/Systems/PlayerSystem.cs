using System;
using BunnyLand.DesktopGL.Components;
using BunnyLand.DesktopGL.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input;

namespace BunnyLand.DesktopGL.Systems
{
    public class PlayerSystem : EntityProcessingSystem
    {
        private ComponentMapper<AnimatedSprite> spriteMapper = null!;
        private ComponentMapper<Transform2> transformMapper = null!;
        private ComponentMapper<Player> playerMapper;
        private ComponentMapper<CollisionBody> collisionMapper;

        public PlayerSystem() : base(Aspect.All(typeof(Transform2), typeof(AnimatedSprite), typeof(Player), typeof(CollisionBody)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
            spriteMapper = mapperService.GetMapper<AnimatedSprite>();
            playerMapper = mapperService.GetMapper<Player>();
            collisionMapper = mapperService.GetMapper<CollisionBody>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = transformMapper.Get(entityId);
            var player = playerMapper.Get(entityId);
            var body = collisionMapper.Get(entityId);

            var state = KeyboardExtended.GetState();

            var unit = gameTime.GetElapsedSeconds() * 100;


        }
    }

    //public class StateMachine<TState, TEvents>
    //{
    //    private TState state;

    //    public static StateMachine<TState, TEvents> Start(TState state)
    //    {
    //        var stateMachine = new StateMachine<TState, TEvents> {state = state};
    //        return stateMachine;
    //    }

    //    public StateDefinition From(TState state)
    //    {
    //        return new StateDefinition(this, state);
    //    }

    //    public class StateDefinition
    //    {
    //        private StateMachine<TState, TEvents> stateMachine;
    //        private TState state;

    //        public StateDefinition(StateMachine<TState, TEvents> stateMachine, TState state)
    //        {
    //            this.stateMachine = stateMachine;
    //            this.state = state;
    //        }

    //        public
    //    }
    //}
}
