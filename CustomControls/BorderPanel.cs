using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecorBlishhudModule.CustomControls;

public class BorderPanel : Panel
{
    private readonly AsyncTexture2D _textureWindowCorner = AsyncTexture2D.FromAssetId(156008);

    protected Rectangle ResizeHandleBoundsRight { get; private set; } = Rectangle.Empty;
    protected Rectangle ResizeHandleBoundsLeft { get; private set; } = Rectangle.Empty;

    public override void RecalculateLayout()
    {
        ResizeHandleBoundsRight = new Rectangle(
            Width - _textureWindowCorner.Width + 3,
            Height - _textureWindowCorner.Height + 3,
            _textureWindowCorner.Width,
            _textureWindowCorner.Height
        );

        ResizeHandleBoundsLeft = new Rectangle(
            -3,
            Height - _textureWindowCorner.Height + 3,
            _textureWindowCorner.Width,
            _textureWindowCorner.Height
        );
    }

    public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
    {
        PaintCorner(spriteBatch);
    }

    private void PaintCorner(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawOnCtrl(this, _textureWindowCorner, ResizeHandleBoundsRight);

        spriteBatch.DrawOnCtrl(
            this,
            _textureWindowCorner,
            ResizeHandleBoundsLeft,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            SpriteEffects.FlipHorizontally
        );
    }
}