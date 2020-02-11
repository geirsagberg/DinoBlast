using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Controls;
using BunnyLand.DesktopGL.Enums;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Screens;
using PubSub;
using GuiScreen = MonoGame.Extended.Gui.Screen;

namespace BunnyLand.DesktopGL.Screens
{
    public class BattleScreen : GameScreen
    {
        private readonly EntityFactory entityFactory;
        private readonly GameSettings gameSettings;
        private readonly GuiSystem guiSystem;
        private readonly Variables variables;
        private readonly World world;

        public BattleScreen(Game game, EntityFactory entityFactory, World world, GameSettings gameSettings,
            GuiSystem guiSystem, Variables variables) : base(game)
        {
            this.entityFactory = entityFactory;
            this.world = world;
            this.gameSettings = gameSettings;
            this.guiSystem = guiSystem;
            this.variables = variables;
        }

        public override void LoadContent()
        {
            entityFactory.CreatePlanet(new Vector2(400, 400), 8000, 0.5f);
            entityFactory.CreatePlanet(new Vector2(800, 300), 12000, 0.8f);
            entityFactory.CreatePlayer(new Vector2(100, 100));
            entityFactory.CreateLevel(gameSettings.Width, gameSettings.Height);


            var stackPanel = new StackPanel {
                AttachedProperties = {{DockPanel.DockProperty, Dock.Right}}
            };

            foreach (var globalVariable in Enum.GetValues(typeof(GlobalVariable)).Cast<GlobalVariable>()) {
                foreach (var control in GetControls(globalVariable)) {
                    stackPanel.Items.Add(control);
                }
            }

            guiSystem.ActiveScreen = new GuiScreen {
                Content = new DockPanel {
                    LastChildFill = false,
                    Margin = 10,
                    Items = {
                        stackPanel
                    }
                }
            };
        }

        private IEnumerable<Control> GetControls(GlobalVariable variable)
        {
            yield return new Label(variable.ToString());
            var defaultValue = variables.Global[variable];
            var slider = new Slider(defaultValue * 0.1f, defaultValue * 10f, defaultValue);
            slider.OnSliderChanged += (_, value) => Hub.Default.Publish((variable, value));
            yield return slider;
        }

        public override void Update(GameTime gameTime)
        {
            guiSystem.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            guiSystem.Draw(gameTime);
        }
    }
}
