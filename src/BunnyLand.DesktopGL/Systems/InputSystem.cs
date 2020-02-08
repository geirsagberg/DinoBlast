using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Input.InputListeners;

namespace BunnyLand.DesktopGL.Systems
{
    public class InputSystem : UpdateSystem
    {
        private readonly KeyboardListener keyboardListener;
        private readonly GamePadListener gamePadListener;

        public InputSystem(KeyboardListener keyboardListener, GamePadListener gamePadListener)
        {
            this.keyboardListener = keyboardListener;
            this.gamePadListener = gamePadListener;

            
        }

        public override void Dispose()
        {
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}
