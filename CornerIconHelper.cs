// CornerIconHelper.cs
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gw2DecorBlishhudModule
{
    public static class CornerIconHelper
    {
        public static CornerIcon CreateLoadingIcon(Texture2D gw2DecorTexture, StandardWindow gw2DecorWindow, out LoadingSpinner loadingSpinner)
        {
            var loadingIcon = new CornerIcon()
            {
                Icon = gw2DecorTexture,
                BasicTooltipText = "Loading GW2 Decor...",
                Priority = 1645843523,
                Parent = GameService.Graphics.SpriteScreen
            };

            var iconPosition = loadingIcon.AbsoluteBounds.Location;

            loadingSpinner = new LoadingSpinner()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(32, 32),
                Location = new Point(iconPosition.X + 36, iconPosition.Y), // Adjust spinner position near the icon
                Visible = true
            };

            return loadingIcon;
        }

        // Method to create the final icon without spinner
        public static CornerIcon CreateFinalIcon(Texture2D gw2DecorTexture, StandardWindow gw2DecorWindow)
        {
            var finalIcon = new CornerIcon()
            {
                Icon = gw2DecorTexture,
                BasicTooltipText = $"{gw2DecorWindow?.Title}",
                Priority = 1645843524,
                Parent = GameService.Graphics.SpriteScreen
            };

            finalIcon.Click += (s, e) => gw2DecorWindow.ToggleWindow();

            return finalIcon;
        }
    }
}
