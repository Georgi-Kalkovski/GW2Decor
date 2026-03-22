using Blish_HUD;
using Blish_HUD.Controls;
using DecorBlishhudModule;
using DecorBlishhudModule.CustomControls.CustomTab;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

public class InfoSection
{
    private static CustomTabbedWindow2 _decorWindow = DecorModule.DecorModuleInstance.DecorWindow;
    private static FlowPanel _infoContainer;
    private static FlowPanel _firstPanel;
    private static FlowPanel _secondPanel;

    public static void InitializeInfoPanel()
    {
        int containerWidth = 550;
        int containerHeight = 150;

        _infoContainer = new FlowPanel
        {
            Parent = _decorWindow,
            Location = new Point(_decorWindow.Width - containerWidth - 100, -2),
            Size = new Point(containerWidth, containerHeight),
            FlowDirection = ControlFlowDirection.SingleLeftToRight, // <-- row layout
            CanScroll = false,
            ControlPadding = new Vector2(4, 0), // horizontal padding between panels
        };

        _infoContainer.ZIndex = 100;

        // Create the two sub-panels
        _firstPanel = new FlowPanel
        {
            Width = (int)(_infoContainer.Width * 0.4), // half width
            Height = _infoContainer.Height,
            Parent = _infoContainer,
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            ControlPadding = new Vector2(0, 2),
            CanScroll = false,
        };

        _secondPanel = new FlowPanel
        {
            Width = (int)(_firstPanel.Width*4), // half width
            Height = _infoContainer.Height,
            Parent = _infoContainer,
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            ControlPadding = new Vector2(0, 2),
            CanScroll = false,
        };

        // Set default info items
        SetInfo(new[]
        {
              ("test/empty.png","                                                  "),
              ("test/click.png", "Double-click on an icon to go to its wiki page."),
              ("test/copy.png", "Click on the name or the image to copy its name."),
        });
    }

    public static void SetInfo((string iconPath, string text)[] items)
    {
        _firstPanel.ClearChildren();
        _secondPanel.ClearChildren();

        for (int i = 0; i < items.Length; i++)
        {
            int iconSize = 22;
            var textSize = GameService.Content.DefaultFont14.MeasureString(items[i].text);
            int panelHeight = Math.Max(iconSize, (int)textSize.Height);

            FlowPanel parentPanel = (i == 0) ? _firstPanel : _secondPanel;

            // Instead of making a full-width panel, make a small container just for icon+label
            int panelWidth = iconSize + 4 + (int)textSize.Width; // icon + spacing + text width
            var panel = new Panel
            {
                Size = new Point(panelWidth, panelHeight),
                Parent = parentPanel
            };

            var icon = new Image
            {
                Parent = panel,
                Size = new Point(iconSize, iconSize),
                Location = new Point(0, (panelHeight - iconSize) / 2),
                Texture = DecorModule.DecorModuleInstance.ContentsManager.GetTexture(items[i].iconPath)
            };

            var label = new Label
            {
                Parent = panel,
                Location = new Point(icon.Right + 4, 0),
                Size = new Point(panelWidth + (icon.Width + 4), panelHeight),
                Text = items[i].text,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.White,
                StrokeText = false,
                ShowShadow = false,
                WrapText = true
            };
        }
    }
    public static void UpdateInfoVisible(bool visible)
    {
        _infoContainer.Visible = visible;
    }
}