// CornerIconHelper.cs
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace Gw2DecorBlishhudModule
{
    public static class CornerIconHelper
    {
        public static CornerIcon CreateCornerIconWithContextMenu(Texture2D mugTexture, StandardWindow gw2DecorWindow)
        {
            var cornerIcon = new CornerIcon()
            {
                Icon = mugTexture,
                BasicTooltipText = $"{gw2DecorWindow?.Title}",
                Priority = 1645843523,
                Parent = GameService.Graphics.SpriteScreen
            };

            cornerIcon.Click += (s, e) => gw2DecorWindow.ToggleWindow();

            var contextMenuStrip = new ContextMenuStrip();
            cornerIcon.Menu = contextMenuStrip;

            return cornerIcon;
        }
    }
}
