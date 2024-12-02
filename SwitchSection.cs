using Blish_HUD;
using Blish_HUD.Controls;
using DecorBlishhudModule;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;

public class SwitchSection
{
    private bool _isHomestead;
    private Panel toggleSwitch;
    private Panel toggleIcon;
    private Label homesteadSwitchText;
    private Label guildhallSwitchText;
    private TextBox searchTextBox;
    private FlowPanel homesteadDecorationsFlowPanel;
    private FlowPanel guildHallDecorationsFlowPanel;

    public SwitchSection(TextBox searchTextBox, FlowPanel homesteadDecorationsFlowPanel, FlowPanel guildHallDecorationsFlowPanel)
    {
        this.searchTextBox = searchTextBox;
        this.homesteadDecorationsFlowPanel = homesteadDecorationsFlowPanel;
        this.guildHallDecorationsFlowPanel = guildHallDecorationsFlowPanel;

        InitializeControls();
        InitializeEvents();
    }

    private void InitializeControls()
    {
        var decorModule = DecorModule.DecorModuleInstance;

        homesteadSwitchText = new Label
        {
            Parent = decorModule.DecorWindow,
            Text = "Homestead",
            Location = new Point(searchTextBox.Right + 20, 3),
            Width = 80,
            Height = 20,
            ShowShadow = true,
            WrapText = true,
            StrokeText = true,
            TextColor = new Color(254, 219, 114),
            ShadowColor = new Color(165, 123, 0),
            Font = GameService.Content.DefaultFont16,
        };

        toggleSwitch = new Panel
        {
            Parent = decorModule.DecorWindow,
            Location = new Point(homesteadSwitchText.Right + 5, -2),
            Size = new Point(60, 30),
            BackgroundTexture = decorModule.BackgroundSwitch,
        };

        toggleIcon = new Panel
        {
            Parent = toggleSwitch,
            Size = new Point(25, 25),
            BackgroundTexture = decorModule.HomesteadSwitch,
            Location = new Point(6, 2),
        };

        guildhallSwitchText = new Label
        {
            Parent = decorModule.DecorWindow,
            Text = "Guild Hall",
            Location = new Point(toggleSwitch.Right + 5, 3),
            Width = 70,
            Height = 20,
            ShowShadow = true,
            WrapText = true,
            StrokeText = true,
            TextColor = Color.LightGray,
            ShadowColor = new Color(0, 0, 0),
            Font = GameService.Content.DefaultFont16,
        };
    }

    private void InitializeEvents()
    {
        // Add click event for the toggle switch
        toggleSwitch.Click += (s, e) =>
        {
            _isHomestead = !_isHomestead;
            UpdateSwitchState();
        };

        // Add click event for the homestead switch text
        homesteadSwitchText.Click += (s, e) =>
        {
            _isHomestead = true;
            UpdateSwitchState();
        };

        // Add click event for the guildhall switch text
        guildhallSwitchText.Click += (s, e) =>
        {
            if (!_isHomestead) return; // Prevent redundant state changes.
            _isHomestead = false;
            UpdateSwitchState();
        };
    }

    private void UpdateSwitchState()
    {
        var decorModule = DecorModule.DecorModuleInstance;

        if (_isHomestead)
        {
            decorModule.DecorWindow.Subtitle = "Homestead Decorations";
            toggleIcon.BackgroundTexture = decorModule.HomesteadSwitch;
            toggleIcon.Location = new Point(6, 2);
            toggleIcon.Size = new Point(25, 25);

            homesteadSwitchText.TextColor = new Color(254, 219, 114);
            homesteadSwitchText.ShadowColor = new Color(165, 123, 0);
            guildhallSwitchText.TextColor = Color.LightGray;
            guildhallSwitchText.ShadowColor = Color.Black;

            homesteadDecorationsFlowPanel.Visible = true;
            guildHallDecorationsFlowPanel.Visible = false;
        }
        else
        {
            decorModule.DecorWindow.Subtitle = "Guild Hall Decorations";
            toggleIcon.BackgroundTexture = decorModule.ScribeSwitch;
            toggleIcon.Location = new Point(toggleSwitch.Width - toggleIcon.Width - 4, 5);
            toggleIcon.Size = new Point(22, 22);

            guildhallSwitchText.TextColor = new Color(168, 178, 230);
            guildhallSwitchText.ShadowColor = new Color(40, 47, 85);
            homesteadSwitchText.TextColor = Color.LightGray;
            homesteadSwitchText.ShadowColor = Color.Black;

            homesteadDecorationsFlowPanel.Visible = false;
            guildHallDecorationsFlowPanel.Visible = true;
        }
    }
}
