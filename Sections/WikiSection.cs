using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace DecorBlishhudModule.Sections
{
    public class WikiLicenseSection
    {
        private readonly Container _parentWindow;
        private Label _licenseLabel;

        public WikiLicenseSection(Container parentWindow)
        {
            _parentWindow = parentWindow;
            AddLicenseLabel();
            PositionLicenseLabel();
        }

        private void AddLicenseLabel()
        {
            _licenseLabel = new Label
            {
                Parent = _parentWindow,
                Text = "Images © Guild Wars 2 Wiki",
                Font = GameService.Content.DefaultFont14,
                WrapText = true,
                StrokeText = true,
                ShowShadow = true,
                ShadowColor = new Color(0, 0, 0),
                Height = 195,
                Width = 600,
            };

            _parentWindow.Resized += (sender, args) => PositionLicenseLabel();
        }

        private void PositionLicenseLabel()
        {
            int offsetX = 15;
            int offsetY = 15;
            _licenseLabel.Location = new Point(
                _parentWindow.Width - _licenseLabel.Width - offsetX,
                _parentWindow.Height - _licenseLabel.Height - offsetY
            );

            _licenseLabel.Invalidate();
        }

        public void UpdateFlowPanelPosition(bool isBigView)
        {
            _licenseLabel.Width = isBigView ? 1120 : 600;
            _licenseLabel.Height = isBigView ? 155 : 205;
            PositionLicenseLabel();  
        }
    }
}
