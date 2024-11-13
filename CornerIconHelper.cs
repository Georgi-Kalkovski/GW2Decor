// CornerIconHelper.cs
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gw2DecorBlishhudModule
{
    public static class CornerIconHelper
    {
        public static CornerIcon CreateCornerIconWithContextMenu(Texture2D mugTexture, StandardWindow gw2DecorWindow, out LoadingSpinner loadingSpinner)
        {
            // Create and configure the corner icon
            var cornerIcon = new CornerIcon()
            {
                Icon = mugTexture,
                BasicTooltipText = $"{gw2DecorWindow?.Title}",
                Priority = 1645843523,
                Parent = GameService.Graphics.SpriteScreen
            };

            // Calculate the spinner position to overlay it on the corner icon
            var cornerIconPosition = cornerIcon.AbsoluteBounds.Location;

            // Initialize the loading spinner on the main screen at the corner icon's position
            loadingSpinner = new LoadingSpinner()
            {
                Parent = GameService.Graphics.SpriteScreen,  // Attach to the main screen instead
                Size = new Point(32, 32),                    // Set to 32x32 for matching icon size
                Location = cornerIconPosition,               // Position to overlay the corner icon
                Visible = true                               // Initially visible
            };

            // Click action for the corner icon
            cornerIcon.Click += (s, e) => gw2DecorWindow.ToggleWindow();

            // Add context menu to the corner icon
            var contextMenuStrip = new ContextMenuStrip();
            cornerIcon.Menu = contextMenuStrip;

            return cornerIcon;
        }
    }
}
