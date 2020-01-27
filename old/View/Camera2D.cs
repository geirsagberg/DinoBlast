using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BunnyLand.Views
{
    public class Camera2D
    {
        private Vector2 _Position;
        /// <summary>
        /// Gets or sets the center point of the camera
        /// </summary>
        /// <value>The center point.</value>
        public Vector2 Position
        {
            get { return _Position; }
            set
            {
                _Position = Vector2.Clamp(value, ScreenSize / (2 * Zoom), new Vector2(LevelSize.X - ScreenSize.X / (Zoom * 2), LevelSize.Y - ScreenSize.Y / (Zoom * 2)));
            }
        }
        public Vector2 Velocity { get; set; }
        private float _Zoom;
        public float Zoom
        {
            get { return _Zoom; }
            set
            {
                _Zoom = MathHelper.Clamp(value, MathHelper.Max(ScreenSize.X / LevelSize.X, ScreenSize.Y / LevelSize.Y), 1.0f);
            }
        }
        public float ZoomVelocity { get; set; }
        public float ZoomSpeed { get; set; }
        private float _Rotation;
        public float Rotation
        {
            get { return _Rotation; }
            set { _Rotation = MathHelper.WrapAngle(value); }
        }
        public float RotationVelocity { get; set; }
        public Vector2 ScreenSize { get; set; }
        public Vector2 LevelSize { get; set; }
        public Rectangle Viewport
        {
            get
            {
                return new Rectangle((int)Position.X - (int)(ScreenSize.X / (2 * Zoom)), (int)Position.Y - (int)(ScreenSize.Y / (2 * Zoom)), (int)(ScreenSize.X / Zoom), (int)(ScreenSize.Y / Zoom));
            }
        }
        public Matrix Transform
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(-Position, 0)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(new Vector3(Zoom, Zoom, 0)) *
                    Matrix.CreateTranslation(new Vector3(ScreenSize * 0.5f, 0));
            }
        }

        public Camera2D(int screenWidth, int screenHeight)
        {
            ScreenSize = new Vector2(screenWidth, screenHeight);
            LevelSize = new Vector2(screenWidth, screenHeight);
            Zoom = 1f;
            ZoomSpeed = 0.002f;
            Position = ScreenSize / 2;
            Rotation = 0f;
        }
    }
}
