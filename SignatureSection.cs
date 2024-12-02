using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace DecorBlishhudModule
{
    public class SignatureSection
    {
        private readonly Container _parentWindow;
        private Label _signatureLabel;

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
                Text = "With love from Terter.4125",
                Font = GameService.Content.DefaultFont14,
                WrapText = true,
                StrokeText = true,
                ShowShadow = true,
                ShadowColor = new Color(0, 0, 0),
                Height = 185,     
                Width = 190,     
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
    }
}
