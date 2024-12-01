using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecorBlishhudModule
{
    public static class CornerIconHelper
    {
        public static CornerIcon CreateLoadingIcon(Texture2D _homesteadIconUnactive, Texture2D homesteadIconHover, StandardWindow decorWindow, out LoadingSpinner loadingSpinner)
        {
            var icon = new CornerIcon()
            {
                Icon = _homesteadIconUnactive,
                BasicTooltipText = "Decor",
                Priority = 1645843523,
                Parent = GameService.Graphics.SpriteScreen,
                HoverIcon = homesteadIconHover,
            };

            var iconPosition = icon.AbsoluteBounds.Location;

            var spinnerOffset = new Point(35, 0);

            loadingSpinner = new LoadingSpinner()
            {
                Parent = GameService.Graphics.SpriteScreen,
                BasicTooltipText = "Decor is fetching data...",
                Size = new Point(32, 32),
                Location = iconPosition + spinnerOffset,
                Visible = true
            };

            return icon;
        }
    }
}
