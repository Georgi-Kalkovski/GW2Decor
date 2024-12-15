using Blish_HUD;
using Blish_HUD.Controls;
using DecorBlishhudModule.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;

namespace DecorBlishhudModule.Sections
{
    public class RightSideSection
    {
        private static readonly Logger Logger = Logger.GetLogger<DecorModule>();

        public static async Task UpdateDecorationImageAsync(Decoration decoration, Container _decorWindow, Image _decorationImage)
        {
            var decorationNameLabel = _decorWindow.Children.OfType<Label>().FirstOrDefault();
            decorationNameLabel.Text = "";

            var savedPanel = new Panel
            {
                Parent = _decorWindow,
                Location = new Point(770, 550),
                Title = "Copied !",
                Width = 80,
                Height = 45,
                ShowBorder = true,
                Opacity = 0f,
                Visible = false,
            };

            decorationNameLabel.Click += (s, e) =>
            {
                if (savedPanel.Visible == false)
                {
                    SaveTasks.CopyTextToClipboard(decoration.Name);
                    SaveTasks.ShowSavedPanel(savedPanel);
                }
            };

            _decorationImage.Click += (s, e) =>
            {
                if (savedPanel.Visible == false)
                {
                    SaveTasks.CopyTextToClipboard(decoration.Name);
                    SaveTasks.ShowSavedPanel(savedPanel);
                }
            };

            CenterTextInParent(decorationNameLabel, _decorWindow);

            _decorationImage.Texture = null;
            AdjustImageSize(null, null);

            if (!string.IsNullOrEmpty(decoration.ImageUrl))
            {
                try
                {
                    var imageResponse = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(decoration.ImageUrl);

                    var borderedTexture = CreateBorderedTexture(imageResponse);

                    if (borderedTexture != null)
                    {
                        _decorationImage.Texture = borderedTexture;

                        AdjustImageSize(borderedTexture, _decorationImage);
                        CenterImageInParent(_decorationImage, _decorWindow);

                        decorationNameLabel.Text = decoration.Name.Replace(" ", " 🚪 ") ?? "Unknown Decoration";
                        CenterTextInParent(decorationNameLabel, _decorWindow);

                        PositionTextAboveImage(decorationNameLabel, _decorationImage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to load decoration image for '{decoration.Name}'. Error: {ex.ToString()}");

                    decorationNameLabel.Text = "Error Loading Decoration";
                    CenterTextInParent(decorationNameLabel, _decorWindow);
                }
            }
            else
            {
                _decorationImage.Texture = null;
                AdjustImageSize(null, null);

                decorationNameLabel.Text = decoration.Name ?? "Unknown Decoration";
                CenterTextInParent(decorationNameLabel, _decorWindow);
            }
        }

        private static Texture2D CreateBorderedTexture(byte[] imageResponse)
        {
            try
            {
                using (var memoryStream = new MemoryStream(imageResponse))
                using (var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext())
                {
                    var originalTexture = Texture2D.FromStream(graphicsContext.GraphicsDevice, memoryStream);

                    int borderWidth = 15;
                    Color innerBorderColor = new Color(86, 76, 55);
                    Color outerBorderColor = Color.Black;

                    int borderedWidth = originalTexture.Width + 2 * borderWidth;
                    int borderedHeight = originalTexture.Height + 2 * borderWidth;

                    var borderedTexture = new Texture2D(graphicsContext.GraphicsDevice, borderedWidth, borderedHeight);
                    Color[] borderedColorData = new Color[borderedWidth * borderedHeight];

                    for (int y = 0; y < borderedHeight; y++)
                    {
                        for (int x = 0; x < borderedWidth; x++)
                        {
                            int distanceFromEdge = Math.Min(Math.Min(x, borderedWidth - x - 1), Math.Min(y, borderedHeight - y - 1));
                            if (distanceFromEdge < borderWidth)
                            {
                                float gradientFactor = (float)distanceFromEdge / borderWidth;
                                borderedColorData[y * borderedWidth + x] = Color.Lerp(outerBorderColor, innerBorderColor, gradientFactor);
                            }
                            else
                            {
                                borderedColorData[y * borderedWidth + x] = Color.Transparent;
                            }
                        }
                    }

                    Color[] originalColorData = new Color[originalTexture.Width * originalTexture.Height];
                    originalTexture.GetData(originalColorData);

                    for (int y = 0; y < originalTexture.Height; y++)
                    {
                        for (int x = 0; x < originalTexture.Width; x++)
                        {
                            int borderedIndex = (y + borderWidth) * borderedWidth + x + borderWidth;
                            borderedColorData[borderedIndex] = originalColorData[y * originalTexture.Width + x];
                        }
                    }

                    borderedTexture.SetData(borderedColorData);
                    return borderedTexture;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to create bordered texture. Error: {ex.ToString()}");
                return null;
            }
        }

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
