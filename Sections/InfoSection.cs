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

    private static Image _infoIcon;
    private static Panel _infoTextPanel;
    private static Panel _infoTextPanelBackground;
    private static Label _infoText;

    public static void InitializeInfoPanel()
    {
        // Create Info Panel and Text
        _infoIcon = new Image
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
            ZIndex = 1
        };

        _infoTextPanelBackground = new Panel
        {
            Parent = _infoTextPanel,
            Size = new Point(300, 60),
            Location = new Point(0, 0),
            BackgroundColor = new Color(0, 0, 0, 205),
            ZIndex = 0
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

        _infoIcon.MouseEntered += async (sender, args) => await AnimatePanel(_infoTextPanel, true);
        _infoIcon.MouseLeft += async (sender, args) => await AnimatePanel(_infoTextPanel, false);
    }

    private static async Task AnimatePanel(Panel panel, bool fadeIn)
    {
        var targetOpacity = fadeIn ? 1.0f : 0.0f;
        var step = fadeIn ? 0.1f : -0.1f;

        if (fadeIn)
        {
            panel.Visible = true;
        }

        while ((fadeIn && panel.Opacity < targetOpacity) || (!fadeIn && panel.Opacity > targetOpacity))
        {
            panel.Opacity = MathHelper.Clamp(panel.Opacity + step, 0.0f, 1.0f);
            await Task.Delay(16); // Smooth animation at ~60fps
        }

        panel.Opacity = targetOpacity;

        if (!fadeIn)
        {
            panel.Visible = false;
        }
    }

    public static void UpdateInfoText(string newText)
    {
        _infoText.Text = newText;
    }

    public static void UpdateInfoVisible(bool visible)
    {
        if (visible == true)
        {
            _infoIcon.Visible = true;
        }
        if (visible == false)
        {
            _infoIcon.Visible = false;
        }
    }
}
