using Blish_HUD;
using Blish_HUD.Controls;
using System;
using System.Threading.Tasks;

namespace DecorBlishhudModule
{
    internal static class SaveTasks
    {
        private static readonly Logger Logger = Logger.GetLogger<DecorModule>();

        public static async Task FadePanel(Panel panel, float startOpacity, float endOpacity, int duration)
        {
            int steps = 30;
            float stepDuration = duration / steps;
            float opacityStep = (endOpacity - startOpacity) / steps;

            for (int i = 0; i < steps; i++)
            {
                panel.Opacity = startOpacity + opacityStep * i;
                await Task.Delay((int)stepDuration);
            }

            panel.Opacity = endOpacity;
        }

        public static async void ShowSavedPanel(Panel savedPanel)
        {
            savedPanel.Visible = true;
            await FadePanel(savedPanel, 0f, 1f, 100);

            await Task.Delay(100);

            await FadePanel(savedPanel, 1f, 0f, 200);

            savedPanel.Visible = false;
        }

        public static void CopyTextToClipboard(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(text);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to copy text to clipboard. Error: {ex.ToString()}");
                }
            }
        }
    }
}