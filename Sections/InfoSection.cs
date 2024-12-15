using Blish_HUD;
using DecorBlishhudModule.CustomControls.CustomTab;
using DecorBlishhudModule;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Controls;

public class InfoSection
{
    private static CustomTabbedWindow2 _decorWindow = DecorModule.DecorModuleInstance.DecorWindow;
    private static Texture2D _info = DecorModule.DecorModuleInstance.Info;

    private static Panel _infoTextPanel;
    private static Label _infoText;

    public static async Task InitializeInfoPanel()
    {
        // Create Info Panel and Text
        var infoIcon = new Image
        {
            Parent = _decorWindow,
            Location = new Point(_decorWindow.Width - 120, 5),
            Width = 35,
            Height = 35,
            Texture = _info,
        };

        _infoTextPanel = new Panel
        {
            Parent = _decorWindow,
            Size = new Point(300, 60),
            Location = new Point(_decorWindow.Width - 430, -5),
            ShowBorder = true,
            Visible = false,
            Opacity = 0,
        };

        _infoText = new Label
        {
            Parent = _infoTextPanel,
            Text = "    Click on the name or the image\n            to copy its name.",
            Location = new Point(10, 0),
            Width = 270,
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
            AnimatePanel(_infoTextPanel, true);
        };

        infoIcon.MouseLeft += (sender, args) =>
        {
            AnimatePanel(_infoTextPanel, false);
        };
    }

    private static async Task AnimatePanel(Panel panel, bool fadeIn)
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

    public static void UpdateInfoText(string newText)
    {
        _infoText.Text = newText;
    }
}
