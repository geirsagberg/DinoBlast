using System;
using System.Collections.Generic;
using System.Linq;
using BunnyLand.DesktopGL.Controls;
using BunnyLand.DesktopGL.Enums;
using BunnyLand.DesktopGL.Messages;
using BunnyLand.DesktopGL.Models;
using BunnyLand.DesktopGL.Serialization;
using BunnyLand.DesktopGL.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Screens;
using GuiScreen = MonoGame.Extended.Gui.Screen;

namespace BunnyLand.DesktopGL.Screens;

public class BattleScreen : GameScreen
{
    private readonly GuiSystem guiSystem;
    private readonly Variables variables;
    private readonly MessageHub messageHub;
    private readonly SharedContext sharedContext;

    public BattleScreen(Game game, GuiSystem guiSystem, Variables variables, MessageHub messageHub, SharedContext sharedContext, KeyboardListener keyboardListener) : base(game)
    {
        this.guiSystem = guiSystem;
        this.variables = variables;
        this.messageHub = messageHub;
        this.sharedContext = sharedContext;
        keyboardListener.KeyPressed += KeyboardListenerOnKeyPressed;
    }

    private void KeyboardListenerOnKeyPressed(object? sender, KeyboardEventArgs e)
    {
        if (e.Key == Keys.Escape) {
            messageHub.Publish(new ExitGameMessage());
        }
    }

    public override void LoadContent()
    {
        SetupDebugGui();
    }

    private void SetupDebugGui()
    {
        var stackPanel = new StackPanel {
            AttachedProperties = {{DockPanel.DockProperty, Dock.Right}}
        };

        foreach (var globalVariable in Enum.GetValues(typeof(GlobalVariable)).Cast<GlobalVariable>()) {
            foreach (var control in GetControls(globalVariable)) {
                stackPanel.Items.Add(control);
            }
        }

        var resetButton = new Button {
            Content = "Reset"
        };
        resetButton.Clicked += delegate { messageHub.Publish(new ResetWorldMessage()); };

        stackPanel.Items.Add(resetButton);

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
        slider.OnSliderChanged += (_, value) => messageHub.Publish(new SetGlobalVariableMessage(variable, value));
        yield return slider;
    }

    public override void Update(GameTime gameTime)
    {
        guiSystem.ActiveScreen.IsVisible = sharedContext.ShowDebugInfo;

        guiSystem.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        guiSystem.Draw(gameTime);
    }
}