using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

namespace BunnyLand.DesktopGL.Systems
{
    public class CameraSystem : EntityUpdateSystem
    {
        private readonly OrthographicCamera camera;
        private ComponentMapper<Transform2> transformMapper = null!;
        private ComponentMapper<Movable> movableMapper = null!;

        public CameraSystem(OrthographicCamera camera) : base(Aspect.All(typeof(Transform2)))
        {
            this.camera = camera;
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            transformMapper = mapperService.GetMapper<Transform2>();
            movableMapper = mapperService.GetMapper<Movable>();
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) {
                camera.ZoomIn(0.02f);
            } else if (Keyboard.GetState().IsKeyDown(Keys.F6)) {
                camera.ZoomOut(0.02f);
            }

            var points = new List<Point2>();

            foreach (var entity in ActiveEntities) {
                var transform = transformMapper.Get(entity);
                points.Add(transform.WorldPosition);

                if (movableMapper.Get(entity) is { ExpandsCamera: true }) {
                    while (camera.Zoom > camera.MinimumZoom
                        && camera.Contains(new Rectangle(transform.Position.ToPoint() - new Point(50, 50), new Point(100, 100))) != ContainmentType.Contains) {
                        camera.ZoomOut(0.01f);
                    }
                }
            }

            var totalRect = RectangleF.CreateFrom(points);
            totalRect.Inflate(120, 120);

            if (camera.Contains(totalRect.ToRectangle()) == ContainmentType.Contains)
                camera.ZoomIn(0.001f);
        }
    }
}
