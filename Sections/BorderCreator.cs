using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace DecorBlishhudModule
{
    public class BorderCreator
    {
        private static readonly Logger Logger = Logger.GetLogger<BorderCreator>();

        public static Texture2D CreateBorderedTexture(Texture2D originalTexture)
        {
            try
            {
                using (var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext())
                {

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
                Logger.Warn($"Failed to create bordered texture. Error: {ex}");
                return null;
            }
        }
    }
}
