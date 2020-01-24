using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BunnyLand.Models
{
    public abstract class Entity
    {
        public Texture2D Spritesheet { get; set; }

        public Color Color = Color.White;

        protected bool _Enabled = true;
        public virtual bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                _Enabled = value;
            }
        }
        protected int currentframe = 0;

        /// <summary>
        /// Wait this long between updating animation frames.
        /// </summary>
        protected int animationPeriod = 3;
        protected int animationPeriodCounter = 0;
        public Rectangle[] AnimationFrames { get; protected set; }
        /// <summary>
        /// Gets or sets the position of the entity's origin.
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position { get; set; }
        /// <summary>
        /// Gets the bounding box of this sprite, as rendered on screen.
        /// </summary>
        /// <value>The bounding box.</value>
        public virtual Rectangle BoundingBox { get { return CollisionEngine.CalculateBoundingRectangle(new Rectangle(0, 0, SourceRect.Width, SourceRect.Height), WorldTransform); } }
        public Vector2 Scale { get; set; }
        public float Angle { get; set; }
        /// <summary>
        /// Gets or sets the rotation velocity.
        /// </summary>
        /// <value>The velocity at which the entity rotates, in radians per frame.</value>
        public float Rotation { get; set; }
        public Vector2 Origin { get; protected set; }

        public Vector2 BottomPoint
        {
            get
            {
                Vector2 bottomPoint = new Vector2(Size.X / 2, Size.Y);
                return Vector2.Transform(bottomPoint, WorldTransform);
            }
        }

        public Matrix WorldTransform
        {
            get
            {
                return Matrix.CreateTranslation(-Origin.X, -Origin.Y, 0f) *
                    Matrix.CreateRotationZ(Angle) *
                    Matrix.CreateScale(Scale.X, Scale.Y, 1f) *
                    Matrix.CreateTranslation(Position.X, Position.Y, 0f);
            }
        }
        public SpriteEffects SpriteEffect
        {
            get
            {
                if (IsFacingRight)
                    return SpriteEffects.None;
                else
                    return SpriteEffects.FlipHorizontally;
            }
        }
        /* Cutout rectangle of spritesheet showing current animation frame. 
         * If only 1 sprite, same size as spritesheet */
        public Rectangle SourceRect
        {
            get
            {
                return AnimationFrames[currentframe];
            }
        }
        public Color[] ColorData
        {
            get
            {
                Color[] data = new Color[SourceRect.Width * SourceRect.Height];
                Spritesheet.GetData<Color>(0, SourceRect, data, 0, data.Length);
                return data;
            }
        }
        /// <summary>
        /// Gets the color data for the upper half of the texture, including the middle line if odd number of lines.
        /// </summary>
        /// <value>The color data for the upper half.</value>
        public Color[] ColorDataUpperHalf
        {
            get
            {
                Color[] data = new Color[SourceRect.Width * (SourceRect.Height / 2 + 1)];
                Spritesheet.GetData<Color>(0, SourceRect, data, 0, data.Length);
                return data;
            }
        }
        /// <summary>
        /// Gets the color data for the lower half of the textzure, excluding the middle line if odd number of lines.
        /// </summary>
        /// <value>The color data for the lower half.</value>
        public Color[] ColorDataLowerHalf
        {
            get
            {
                Color[] data = new Color[SourceRect.Width * (SourceRect.Height / 2)];
                Spritesheet.GetData<Color>(0, SourceRect, data, SourceRect.Width * (SourceRect.Height / 2 + 1), data.Length);
                return data;
            }
        }
        /// <summary>
        /// Gets the size of the texture.
        /// </summary>
        /// <value>The texture size.</value>
        public Vector2 Size { get { return new Vector2(SourceRect.Width * Scale.X, SourceRect.Height * Scale.Y); } }
        public bool IsFacingRight { get; set; }

        public Entity(Texture2D spritesheet, Vector2 position)
            : this(spritesheet)
        {
            this.Position = position;
        }
        public Entity(Texture2D spritesheet)
        {
            IsFacingRight = true;
            Spritesheet = spritesheet;
            Position = Vector2.Zero;
            Scale = Vector2.One;
            Angle = 0;
            Rotation = 0;
            AnimationFrames = Sprites.GetSprites(1, 1, spritesheet);
            currentframe = 0;
            Origin = new Vector2(spritesheet.Width / 2, spritesheet.Height / 2);
        }

        public virtual void Animate()
        {
            currentframe = (currentframe + 1) % AnimationFrames.Count();
        }
    }
}
