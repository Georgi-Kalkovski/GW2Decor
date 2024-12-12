using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecorBlishhudModule.CustomControls.CustomTab;

public class CustomLoadingSpinner : LoadingSpinner
{
    private const int DRAWLENGTH = 32;

    public CustomLoadingSpinner()
    {
        Size = new Point(32, 32);
    }

    public void Paint(SpriteBatch spriteBatch, Rectangle bounds)
    {
        LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, bounds);
    }
}