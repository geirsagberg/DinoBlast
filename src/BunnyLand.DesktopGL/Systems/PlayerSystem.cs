using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input;

namespace BunnyLand.DesktopGL.Systems
{
    public class Player
    {
        //public  Type { get; set; }
    }

    public class BattleFieldSystem : EntityProcessingSystem
    {
        private ComponentMapper<Transform2> transformMapper = null!;

        public BattleFieldSystem() : base(Aspect.All(typeof(Transform2)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
        }

        protected override void OnEntityAdded(int entityId)
        {
            var transform = transformMapper.Get(entityId);
            transform.Position = new Vector2(100, 100);
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = transformMapper.Get(entityId);
        }
    }

    public class PlayerSystem : EntityProcessingSystem
    {
        private ComponentMapper<AnimatedSprite> spriteMapper = null!;
        private ComponentMapper<Transform2> transformMapper = null!;

        public PlayerSystem() : base(Aspect.All(typeof(Transform2), typeof(AnimatedSprite)))
        {
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
            spriteMapper = mapperService.GetMapper<AnimatedSprite>();

            //input

            //// States

            // Idle


            // Triggers
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var transform = transformMapper.Get(entityId);

            var state = KeyboardExtended.GetState();

            var unit = gameTime.GetElapsedSeconds() * 100;

            if (state.IsKeyDown(Keys.A)) {
                transform.Position = transform.Position.Translate(-unit, 0);
            }

            if (state.IsKeyDown(Keys.D)) {
                transform.Position = transform.Position.Translate(unit, 0);
            }

            if (state.IsKeyDown(Keys.W)) {
                transform.Position = transform.Position.Translate(0, -unit);
            }

            if (state.IsKeyDown(Keys.S)) {
                transform.Position = transform.Position.Translate(0, unit);
            }
        }
    }

    public enum PlayerState
    {
        Idle,
        MovingRight,
        MovingLeft,
    }

    public enum PlayerInputType
    {
        None,
        Jump,
        Move,
        Aim
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
