
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;

namespace DecorBlishhudModule
{
    public class RightSideMethods
    {
        public static void CenterTextInParent(Label label, Control parent)
        {
            var font = GameService.Content.DefaultFont18;
            var textSize = font.MeasureString(label.Text);
            float textWidth = textSize.Width;

            int parentWidth = parent.Width;

            int startX = (460 + parentWidth - (int)textWidth) / 2;
            if (label.Text == "Loading...")
            {
                label.Location = new Point(startX, label.Location.Y + 300);
            }
            else
            {
                label.Location = new Point(startX, label.Location.Y);
            }
        }

        public static void PositionTextAboveImage(Label text, Image image)
        {
            int textY = image.Location.Y - text.Height + 30;
            text.Location = new Point(text.Location.X, textY);
        }

        public static void CenterImageInParent(Image image, Control parent)
        {
            int centerX = (parent.Width - image.Size.X) / 2;
            int centerY = (parent.Height - image.Size.Y) / 2;

            image.Location = new Point(centerX + 230, centerY - 40);
        }

        public static void AdjustImageSize(Texture2D loadedTexture, Image decorationImage)
        {
            if (loadedTexture == null)
            {
                return;
            }

            int maxDimension = 500;
            float aspectRatio = (float)loadedTexture.Width / loadedTexture.Height;

            int targetWidth, targetHeight;

            if (loadedTexture.Width > loadedTexture.Height)
            {
                targetWidth = maxDimension;
                targetHeight = (int)(maxDimension / aspectRatio);
            }
            else
            {
                targetHeight = maxDimension;
                targetWidth = (int)(maxDimension * aspectRatio);
            }

            decorationImage.Size = new Point(targetWidth, targetHeight);
        }
    }
}
