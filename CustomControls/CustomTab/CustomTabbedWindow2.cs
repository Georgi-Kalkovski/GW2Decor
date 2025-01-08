using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DecorBlishhudModule.CustomControls.CustomTab
{
    public class CustomTabbedWindow2 : WindowBase2, ICustomTabOwner
    {
        private const int TAB_VERTICALOFFSET = 80;
        private const int TAB_HEIGHT = 50;
        private const int TAB_WIDTH = 80;
        private const int TAB_GAP = 40;
        private static readonly Texture2D _textureTabActive = Content.GetTexture("window-tab-active");

        private CustomTab _selectedTabGroup1;
        private CustomTab _selectedTabGroup2;
        private CustomTab _selectedTabGroup3;

        public CustomTabCollection TabsGroup1 { get; }
        public CustomTabCollection TabsGroup2 { get; }
        public CustomTabCollection TabsGroup3 { get; }

        public CustomTab SelectedTabGroup1
        {
            get => _selectedTabGroup1;
            set
            {
                CustomTab previousTab = _selectedTabGroup1;
                if ((value == null || TabsGroup1.Contains(value)) && SetProperty(ref _selectedTabGroup1, value, invalidateLayout: true, nameof(SelectedTabGroup1)))
                {
                    OnTabChanged(new ValueChangedEventArgs<CustomTab>(previousTab, value));
                }
            }
        }

        public CustomTab SelectedTabGroup2
        {
            get => _selectedTabGroup2;
            set
            {
                CustomTab previousTab = _selectedTabGroup2;
                if ((value == null || TabsGroup2.Contains(value)) && SetProperty(ref _selectedTabGroup2, value, invalidateLayout: true, nameof(SelectedTabGroup2)))
                {
                    OnTabChanged(new ValueChangedEventArgs<CustomTab>(previousTab, value));
                }
            }
        }

        public CustomTab SelectedTabGroup3
        {
            get => _selectedTabGroup3;
            set
            {
                CustomTab previousTab = _selectedTabGroup3;
                if ((value == null || TabsGroup3.Contains(value)) && SetProperty(ref _selectedTabGroup3, value, invalidateLayout: true, nameof(SelectedTabGroup3)))
                {
                    OnTabChanged(new ValueChangedEventArgs<CustomTab>(previousTab, value));
                }
            }
        }

        private CustomTab HoveredTab { get; set; }

        public event EventHandler<ValueChangedEventArgs<CustomTab>> TabChanged;

        protected virtual void OnTabChanged(ValueChangedEventArgs<CustomTab> e)
        {
            if (Visible && e.PreviousValue != null)
            {
                Content.PlaySoundEffectByName($"tab-swap-{RandomUtil.GetRandom(1, 5)}");
            }
            TabChanged?.Invoke(this, e);
        }

        public CustomTabbedWindow2(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize)
        {
            TabsGroup1 = new CustomTabCollection(this);
            TabsGroup2 = new CustomTabCollection(this);
            TabsGroup3 = new CustomTabCollection(this);
            ShowSideBar = true;
            ConstructWindow(background, windowRegion, contentRegion, windowSize);
        }
        public CustomTabbedWindow2(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize)
            : this((AsyncTexture2D)background, windowRegion, contentRegion, windowSize)
        {
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (HoveredTab != null && HoveredTab.Enabled)
            {
                if (TabsGroup1.Contains(HoveredTab))
                {
                    SelectedTabGroup1 = HoveredTab;
                    SelectedTabGroup2 = SelectedTabGroup2 == null ? TabsGroup2.FromIndex(0) : SelectedTabGroup2;
                    SelectedTabGroup3 = null;
                }
                else if (TabsGroup2.Contains(HoveredTab))
                {
                    SelectedTabGroup1 = SelectedTabGroup1 == null ? TabsGroup1.FromIndex(0) : SelectedTabGroup2;
                    SelectedTabGroup2 = HoveredTab;
                    SelectedTabGroup3 = null;
                }
                else if (TabsGroup3.Contains(HoveredTab))
                {
                    SelectedTabGroup1 = null;
                    SelectedTabGroup2 = null;
                    SelectedTabGroup3 = HoveredTab;
                }
            }
            base.OnClick(e);
        }

        private void UpdateTabStates()
        {
            SideBarHeight =
                TAB_VERTICALOFFSET +
                TAB_HEIGHT * TabsGroup1.Count +
                TAB_HEIGHT * TabsGroup2.Count +
                TAB_HEIGHT * TabsGroup3.Count +
                TAB_GAP * 2;

            HoveredTab = MouseOver && SidebarActiveBounds.Contains(RelativeMousePosition)
                ? TabsFromPosition(RelativeMousePosition.Y - SidebarActiveBounds.Top)
                : null;

            BasicTooltipText = HoveredTab?.Name;
        }

        private CustomTab TabsFromPosition(int yPosition)
        {
            int tabIndex = 0;

            foreach (var tab in TabsGroup1)
            {
                int tabTop = SidebarActiveBounds.Top + TAB_VERTICALOFFSET + tabIndex * TAB_HEIGHT;

                if (yPosition + 40 >= tabTop && yPosition + 40 <= tabTop + TAB_HEIGHT)
                {
                    return tab;
                }
                tabIndex++;
            }

            foreach (var tab in TabsGroup2)
            {
                int tabTop = SidebarActiveBounds.Top + TAB_VERTICALOFFSET + TAB_GAP + tabIndex * TAB_HEIGHT;

                if (yPosition + 40 >= tabTop && yPosition + 40 <= tabTop + TAB_HEIGHT)
                {
                    return tab;
                }
                tabIndex++;
            }

            foreach (var tab in TabsGroup3)
            {
                int tabTop = SidebarActiveBounds.Top + TAB_VERTICALOFFSET + 2 * TAB_GAP + tabIndex * TAB_HEIGHT;

                if (yPosition + 40 >= tabTop && yPosition + 40 <= tabTop + TAB_HEIGHT)
                {
                    return tab;
                }
                tabIndex++;
            }
            return null;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            UpdateTabStates();
            base.UpdateContainer(gameTime);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            int tabIndex = 0;

            foreach (var tab in TabsGroup1)
            {
                int y = SidebarActiveBounds.Top + TAB_VERTICALOFFSET + tabIndex * TAB_HEIGHT;
                bool isSelected = tab == SelectedTabGroup1;
                bool isHovered = tab == HoveredTab;

                if (isSelected)
                {
                    Rectangle destinationRectangle = new Rectangle(
                        SidebarActiveBounds.Left - (TAB_WIDTH - SidebarActiveBounds.Width) + 2,
                        y,
                        TAB_WIDTH,
                        TAB_HEIGHT
                    );
                    spriteBatch.DrawOnCtrl(this, WindowBackground, destinationRectangle, new Rectangle(WindowRegion.Left + destinationRectangle.X + 20, destinationRectangle.Y - (int)Padding.Top, destinationRectangle.Width, destinationRectangle.Height));
                    spriteBatch.DrawOnCtrl(this, _textureTabActive, destinationRectangle);
                }
                tab.Draw(this, spriteBatch, new Rectangle(SidebarActiveBounds.X, y, SidebarActiveBounds.Width, TAB_HEIGHT), isSelected, isHovered);
                tabIndex++;
            }

            foreach (var tab in TabsGroup2)
            {
                int y = SidebarActiveBounds.Top + TAB_VERTICALOFFSET + TAB_GAP + tabIndex * TAB_HEIGHT;
                bool isSelected = tab == SelectedTabGroup2;
                bool isHovered = tab == HoveredTab;

                if (isSelected)
                {
                    Rectangle destinationRectangle = new Rectangle(
                        SidebarActiveBounds.Left - (TAB_WIDTH - SidebarActiveBounds.Width) + 2,
                        y,
                        TAB_WIDTH,
                        TAB_HEIGHT
                    );
                    spriteBatch.DrawOnCtrl(this, WindowBackground, destinationRectangle, new Rectangle(WindowRegion.Left + destinationRectangle.X + 20, destinationRectangle.Y - (int)Padding.Top, destinationRectangle.Width, destinationRectangle.Height));
                    spriteBatch.DrawOnCtrl(this, _textureTabActive, destinationRectangle);
                }
                tab.Draw(this, spriteBatch, new Rectangle(SidebarActiveBounds.X, y, SidebarActiveBounds.Width, TAB_HEIGHT), isSelected, isHovered);
                tabIndex++;
            }

            foreach (var tab in TabsGroup3)
            {
                int y = SidebarActiveBounds.Top + TAB_VERTICALOFFSET + TAB_GAP * 2 + tabIndex * TAB_HEIGHT;
                bool isSelected = tab == SelectedTabGroup3;
                bool isHovered = tab == HoveredTab;

                if (isSelected)
                {
                    Rectangle destinationRectangle = new Rectangle(
                        SidebarActiveBounds.Left - (TAB_WIDTH - SidebarActiveBounds.Width) + 2,
                        y,
                        TAB_WIDTH,
                        TAB_HEIGHT
                    );
                    spriteBatch.DrawOnCtrl(this, WindowBackground, destinationRectangle, new Rectangle(WindowRegion.Left + destinationRectangle.X + 20, destinationRectangle.Y - (int)Padding.Top, destinationRectangle.Width, destinationRectangle.Height));
                    spriteBatch.DrawOnCtrl(this, _textureTabActive, destinationRectangle);
                }
                tab.Draw(this, spriteBatch, new Rectangle(SidebarActiveBounds.X, y, SidebarActiveBounds.Width, TAB_HEIGHT), isSelected, isHovered);
                tabIndex++;
            }
        }
    }
}