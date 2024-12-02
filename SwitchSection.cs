using Blish_HUD;
using Blish_HUD.Controls;
using DecorBlishhudModule;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;

public class SwitchSection
{
    private bool _isHomestead;
    private Panel _toggleSwitch;
    private Panel _toggleIcon;
    private Label _homesteadSwitchText;
    private Label _guildhallSwitchText;
    private TextBox _searchTextBox;
    private FlowPanel homesteadDecorationsFlowPanel;
    private FlowPanel guildHallDecorationsFlowPanel;

    public SwitchSection(TextBox searchTextBox, FlowPanel homesteadDecorationsFlowPanel, FlowPanel guildHallDecorationsFlowPanel)
    {
        this._searchTextBox = searchTextBox;
        this.homesteadDecorationsFlowPanel = homesteadDecorationsFlowPanel;
        this.guildHallDecorationsFlowPanel = guildHallDecorationsFlowPanel;

        _isHomestead = true;
        InitializeControls();
        InitializeEvents();
        UpdateSwitchState();
    }

    private void InitializeControls()
    {
        var decorModule = DecorModule.DecorModuleInstance;

        _homesteadSwitchText = new Label
        {
            Parent = decorModule.DecorWindow,
            Text = "Homestead",
            Location = new Point(_searchTextBox.Right + 20, 3),
            Width = 80,
            Height = 20,
            ShowShadow = true,
            WrapText = true,
            StrokeText = true,
            TextColor = new Color(254, 219, 114),
            ShadowColor = new Color(165, 123, 0),
            Font = GameService.Content.DefaultFont16,
        };

        _toggleSwitch = new Panel
        {
            Parent = decorModule.DecorWindow,
            Location = new Point(_homesteadSwitchText.Right + 5, -2),
            Size = new Point(60, 30),
            BackgroundTexture = decorModule.BackgroundSwitch,
        };

        _toggleIcon = new Panel
        {
            Parent = _toggleSwitch,
            Size = new Point(25, 25),
            BackgroundTexture = decorModule.HomesteadSwitch,
            Location = new Point(6, 2),
        };

        _guildhallSwitchText = new Label
        {
            Parent = decorModule.DecorWindow,
            Text = "Guild Hall",
            Location = new Point(_toggleSwitch.Right + 5, 3),
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
        _homesteadSwitchText.MouseEntered += (s, e) =>
        {
            _homesteadSwitchText.TextColor = Color.White;
            _homesteadSwitchText.ShadowColor = Color.White;
        };
        _homesteadSwitchText.MouseLeft += (s, e) =>
        {
            _homesteadSwitchText.TextColor = _isHomestead
                ? new Color(254, 219, 114)
                : Color.LightGray;
            _homesteadSwitchText.ShadowColor = _isHomestead
                ? new Color(165, 123, 0)
                : Color.Black;
        };

        _guildhallSwitchText.MouseEntered += (s, e) =>
        {
            _guildhallSwitchText.TextColor = Color.White;
            _guildhallSwitchText.ShadowColor = Color.White;
        };
        _guildhallSwitchText.MouseLeft += (s, e) =>
        {
            _guildhallSwitchText.TextColor = !_isHomestead
                ? new Color(168, 178, 230)
                : Color.LightGray;
            _guildhallSwitchText.ShadowColor = !_isHomestead
                ? new Color(40, 47, 85)
                : Color.Black;
        };

        _toggleSwitch.Click += (s, e) =>
        {
            _isHomestead = !_isHomestead;
            UpdateSwitchState();
        };

        _homesteadSwitchText.Click += (s, e) =>
        {
            _isHomestead = true;
            UpdateSwitchState();
        };

        _guildhallSwitchText.Click += (s, e) =>
        {
            if (!_isHomestead) return;
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
            _toggleIcon.BackgroundTexture = decorModule.HomesteadSwitch;
            _toggleIcon.Location = new Point(6, 2);
            _toggleIcon.Size = new Point(25, 25);

            _homesteadSwitchText.TextColor = new Color(254, 219, 114);
            _homesteadSwitchText.ShadowColor = new Color(165, 123, 0);
            _guildhallSwitchText.TextColor = Color.LightGray;
            _guildhallSwitchText.ShadowColor = Color.Black;

            homesteadDecorationsFlowPanel.Visible = true;
            guildHallDecorationsFlowPanel.Visible = false;
        }
        else
        {
            decorModule.DecorWindow.Subtitle = "Guild Hall Decorations";
            _toggleIcon.BackgroundTexture = decorModule.ScribeSwitch;
            _toggleIcon.Location = new Point(_toggleSwitch.Width - _toggleIcon.Width - 4, 5);
            _toggleIcon.Size = new Point(22, 22);

            _guildhallSwitchText.TextColor = new Color(168, 178, 230);
            _guildhallSwitchText.ShadowColor = new Color(40, 47, 85);
            _homesteadSwitchText.TextColor = Color.LightGray;
            _homesteadSwitchText.ShadowColor = Color.Black;

            homesteadDecorationsFlowPanel.Visible = false;
            guildHallDecorationsFlowPanel.Visible = true;
        }
    }
}
