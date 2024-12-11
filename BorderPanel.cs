using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls;

public class BorderPanel : Panel
{
    private readonly AsyncTexture2D _textureWindowCorner = AsyncTexture2D.FromAssetId(156008);

    protected Rectangle ResizeHandleBoundsRight { get; private set; } = Rectangle.Empty;
    protected Rectangle ResizeHandleBoundsLeft { get; private set; } = Rectangle.Empty;

    public override void RecalculateLayout()
    {
        ResizeHandleBoundsRight = new Rectangle(
            base.Width - _textureWindowCorner.Width + 3,
            base.Height - _textureWindowCorner.Height + 3,
            _textureWindowCorner.Width,
            _textureWindowCorner.Height
        );

        ResizeHandleBoundsLeft = new Rectangle(
            -3,
            base.Height - _textureWindowCorner.Height + 3,
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