using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace DecorBlishhudModule.Sections
{
    public class SignatureSection
    {
        private readonly Container _parentWindow;
        private Label _signatureLabel;
        private Image _signatureImage;

        public SignatureSection(Container parentWindow)
        {
            _parentWindow = parentWindow;
            AddSignatureLabel();
            PositionSignatureLabel();
        }

        private void AddSignatureLabel()
        {
            _signatureLabel = new Label
            {
                Parent = _parentWindow,
                Text = "With      from Terter.4125",
                Font = GameService.Content.DefaultFont14,
                WrapText = true,
                StrokeText = true,
                ShowShadow = true,
                ShadowColor = new Color(0, 0, 0),
                Height = 185,
                Width = 235,
            };

            _signatureImage = new Image
            {
                Parent = _parentWindow,
                Texture = DecorModule.DecorModuleInstance.Heart,
                Location = new Point(_signatureLabel.Right + 693, _signatureLabel.Bottom + 489),
                Size = new Point(25, 25)
            };

            _parentWindow.Resized += (sender, args) => PositionSignatureLabel();
        }

        private void PositionSignatureLabel()
        {
            int offsetX = 20;
            int offsetY = 20;
            _signatureLabel.Location = new Point(
                _parentWindow.Width - _signatureLabel.Width - offsetX,
                _parentWindow.Height - _signatureLabel.Height - offsetY
            );

            _signatureLabel.Invalidate();
        }

        public void UpdateFlowPanelPosition(bool isBigView)
        {
            _signatureLabel.Width = isBigView ? 215 : 235;
            _signatureLabel.Height = isBigView ? 135 : 185;
            PositionSignatureLabel();

            _signatureImage.Location = new Point(
                _signatureLabel.Location.X + 33,
                _signatureLabel.Location.Y + (isBigView ? 54 : 79)
            );

            _signatureImage.Invalidate();
        }
    }
}
