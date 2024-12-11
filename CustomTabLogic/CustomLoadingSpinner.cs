using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls;

public class CustomLoadingSpinner : LoadingSpinner
{
    private const int DRAWLENGTH = 32;

    public CustomLoadingSpinner()
    {
        base.Size = new Point(32, 32);
    }

    public void Paint(SpriteBatch spriteBatch, Rectangle bounds)
    {
        LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, bounds);
    }
}