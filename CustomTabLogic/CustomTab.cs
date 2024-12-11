using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DecorBlishhudModule.CustomTabLogic
{
    public class CustomTab
    {
        public AsyncTexture2D Icon { get; set; }

        public CustomLoadingSpinner Spinner { get; set; }

        public int OrderPriority { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; } = true;

        public CustomTab(AsyncTexture2D icon, string name = null, int? priority = null)
        {
            Icon = icon;
            Name = name;
            OrderPriority = priority.GetValueOrDefault();
            Spinner = new CustomLoadingSpinner();
        }

        public void Draw(Control tabbedControl, SpriteBatch spriteBatch, Rectangle bounds, bool selected, bool hovered)
        {
            if (Icon.HasTexture)
            {
                spriteBatch.DrawOnCtrl(
                    tabbedControl,
                    Icon,
                    new Rectangle(
                        bounds.X + (bounds.Width - Icon.Texture.Width) / 2,
                        bounds.Y + (bounds.Height - Icon.Texture.Height) / 2,
                        Icon.Texture.Width,
                        Icon.Texture.Height
                    ),
                    selected || hovered ? Color.White : ContentService.Colors.DullColor
                );
            }

            if (!Enabled && Spinner != null)
            {
                var spinnerBounds = new Rectangle(
                    tabbedControl.Location.X + bounds.X + (bounds.Width - Spinner.Size.X) / 2,
                    tabbedControl.Location.Y + bounds.Y + (bounds.Height - Spinner.Size.Y) / 2,
                    Spinner.Size.X,
                    Spinner.Size.Y
                );

                Spinner.Paint(spriteBatch, spinnerBounds);
            }
        }

    }
}
