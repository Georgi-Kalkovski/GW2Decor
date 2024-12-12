using Blish_HUD;
using Blish_HUD.Controls;
using DecorBlishhudModule.CustomControls.CustomTab;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;

public class InfoSection
{
    private readonly CustomTabbedWindow2 _decorWindow;
    private readonly Texture2D _info;

    public InfoSection(CustomTabbedWindow2 decorWindow, Texture2D info)
    {
        _decorWindow = decorWindow;
        _info = info;

        InitializeInfoPanel();
    }

    private void InitializeInfoPanel()
    {

        var infoIcon = new Image
        {
            Parent = _decorWindow,
            Location = new Point(_decorWindow.Width - 120, 5),
            Width = 35,
            Height = 35,
            Texture = _info,
        };

        var infoTextPanel = new Panel
        {
            Parent = _decorWindow,
            Size = new Point(210, 60),
            Location = new Point(_decorWindow.Width - 350, -5),
            ShowBorder = true,
            Visible = false,
            Opacity = 0,
        };

        var infoText = new Label
        {
            Parent = infoTextPanel,
            Text = "Click on the image or the name to copy its name.",
            Location = new Point(10, 0),
            Width = 200,
            Height = 45,
            ShowShadow = true,
            WrapText = true,
            StrokeText = true,
            TextColor = Color.White,
            ShadowColor = new Color(0, 0, 0),
            Font = GameService.Content.DefaultFont16,
        };

        infoIcon.MouseEntered += (sender, args) =>
        {
            AnimatePanel(infoTextPanel, true);
        };

        infoIcon.MouseLeft += (sender, args) =>
        {
            AnimatePanel(infoTextPanel, false);
        };
    }

    private void AnimatePanel(Panel panel, bool fadeIn)
    {
        var targetOpacity = fadeIn ? 1.0f : 0.0f;

        if (fadeIn)
        {
            panel.Visible = true;
        }

        var timer = new System.Windows.Forms.Timer { Interval = 1 };
        timer.Tick += (s, e) =>
        {
            panel.Opacity += fadeIn ? 0.1f : -0.1f;
            panel.Opacity = MathHelper.Clamp(panel.Opacity, 0.0f, 1.0f);

            if (fadeIn && panel.Opacity >= 1.0f)
            {
                panel.Opacity = 1.0f;
                timer.Stop();
            }
            else if (!fadeIn && panel.Opacity <= 0.0f)
            {
                panel.Opacity = 0.0f;
                panel.Visible = false;
                timer.Stop();
            }
        };
        timer.Start();
    }
}