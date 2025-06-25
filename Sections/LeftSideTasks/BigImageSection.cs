using Blish_HUD;
using Blish_HUD.Controls;
using DecorBlishhudModule.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;

namespace DecorBlishhudModule.Sections.LeftSideTasks
{
    public class BigImageSection
    {
        private static readonly Logger Logger = Logger.GetLogger<DecorModule>();

        private static Panel bigImagePanel;

        private static DateTime _lastImageShownTime = DateTime.MinValue;
        
        public static async Task UpdateDecorationImageAsync(Decoration decoration, Container _decorWindow, Image _decorationImage)
        {
            _decorationImage.ZIndex = 101;

            var textureX = DecorModule.DecorModuleInstance.X2;
            var textureXActive = DecorModule.DecorModuleInstance.X2Active;

            var textureXImage = new Image(textureX)
            {
                Parent = _decorWindow,
                Size = new Point(30, 30),
                ZIndex = 102,
                Visible = false,
            };

            if (bigImagePanel == null)
            {
                bigImagePanel = new Panel
                {
                    Parent = _decorWindow,
                    Size = new Point(1075, 652),
                    Location = new Point(11, 42),
                    BackgroundColor = Color.Black,
                    Opacity = 0.5f,
                    Visible = false,
                    ZIndex = 100,
                };
            }

            _decorationImage.Texture = null;
            AdjustImageSize(null, null);

            if (!string.IsNullOrEmpty(decoration.ImageUrl))
            {
                try
                {
                    byte[] imageResponse;
                    string localImagePath = LeftSideSection.GetImageAndIconFilePath(decoration.ImageUrl);
                    var semaphore = LeftSideSection.GetFileSemaphore(localImagePath);

                    await semaphore.WaitAsync();
                    try
                    {
                        if (File.Exists(localImagePath))
                        {
                            imageResponse = File.ReadAllBytes(localImagePath);
                        }
                        else
                        {
                            imageResponse = await DecorModule.DecorModuleInstance.Client.GetByteArrayAsync(decoration.ImageUrl);
                            File.WriteAllBytes(localImagePath, imageResponse);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }

                    var borderedTexture = CreateBorderedTexture(imageResponse);

                    if (borderedTexture != null)
                    {
                        _decorationImage.Texture = borderedTexture;
                        AdjustImageSize(borderedTexture, _decorationImage);
                        CenterImageInParent(_decorationImage, _decorWindow);
                        PositionXIconAtTopLeft(textureXImage, _decorationImage);

                        bigImagePanel.Visible = true;
                        _lastImageShownTime = DateTime.Now;
                        if (_decorationImage.Visible)
                        {
                            textureXImage.Visible = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to load decoration image for '{decoration.Name}'. Error: {ex.ToString()}");
                }

                textureXImage.MouseEntered += (s, e) =>
                {
                    textureXImage.Texture = textureXActive;
                };

                textureXImage.MouseLeft += (s, e) =>
                {
                    textureXImage.Texture = textureX;
                };

                _decorationImage.Click += async (s, e) =>
                {
                    await Task.Delay(100);

                    if ((DateTime.Now - _lastImageShownTime).TotalMilliseconds > 200)
                    {
                        _decorationImage.Visible = false;
                        bigImagePanel.Visible = false;
                        textureXImage.Visible = false;
                    }
                };

                _decorWindow.Click += async (s, e) =>
                {
                    await Task.Delay(100);

                    if ((DateTime.Now - _lastImageShownTime).TotalMilliseconds > 200)
                    {
                        _decorationImage.Visible = false;
                        bigImagePanel.Visible = false;
                        textureXImage.Visible = false;
                    }
                };
            }
            else
            {
                _decorationImage.Texture = null;
                AdjustImageSize(null, null);

                bigImagePanel.Visible = false;
                textureXImage.Visible = false;
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

        public static void CenterImageInParent(Image image, Control parent)
        {
            int centerX = (parent.Width - image.Size.X) / 2;
            int centerY = (parent.Height - image.Size.Y) / 2;

            image.Location = new Point(centerX - 48, centerY - 30);
        }

        public static void AdjustImageSize(Texture2D loadedTexture, Image decorationImage)
        {
            if (loadedTexture == null)
            {
                return;
            }

            int maxDimension = 600;
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

        private static void PositionXIconAtTopLeft(Image xIcon, Image decorationImage)
        {
            int offsetX = 35;
            int offsetY = 5;

            xIcon.Location = new Point(
                decorationImage.Location.X + decorationImage.Size.X - offsetX,
                decorationImage.Location.Y + offsetY
            );
        }
    }
}
