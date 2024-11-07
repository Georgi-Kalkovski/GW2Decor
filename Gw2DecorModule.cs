using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Gw2DecorBlishhudModule
{
    [Export(typeof(Module))]
    public class Gw2DecorModule : Module
    {
        // Logger instance for logging messages
        private static readonly Logger Logger = Logger.GetLogger<Gw2DecorModule>();

        // Images and other resources
        private Image _decorationIcon;
        private Image _decorationImage;
        private Texture2D _mugTexture;
        private CornerIcon _cornerIcon;
        private ContextMenuStrip _contextMenuStrip;
        private StandardWindow _gw2DecorWindow;

        // Settings entries
        private SettingEntry<bool> _boolGw2DecorSetting;
        private SettingEntry<int> _valueRangeGw2DecorSetting;
        private SettingEntry<int> _hiddenIntGw2DecorSetting;
        private SettingEntry<int> _hiddenIntGw2DecorSetting2;
        private SettingEntry<string> _stringGw2DecorSetting;
        private SettingEntry<ColorType> _enumGw2DecorSetting;
        private SettingCollection _internalGw2DecorSettingSubCollection;

        internal static Gw2DecorModule Gw2DecorModuleInstance;

        // Dependency managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public Gw2DecorModule([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
            Gw2DecorModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            // Define general settings
            settings.DefineSetting(
                "Gw2DecorSetting",
                "This is the default value of the setting",
                () => "Display name of setting",
                () => "Tooltip text of setting");

            _boolGw2DecorSetting = settings.DefineSetting(
                "bool gw2Decor",
                true,
                () => "This is a bool setting (checkbox)",
                () => "Settings can be many different types");

            _stringGw2DecorSetting = settings.DefineSetting(
                "string gw2Decor",
                "myText",
                () => "This is a string setting (textbox)",
                () => "Settings can be many different types");

            _valueRangeGw2DecorSetting = settings.DefineSetting(
                "int gw2Decor",
                20,
                () => "This is an int setting (slider)",
                () => "Settings can be many different types");
            _valueRangeGw2DecorSetting.SetRange(0, 255);

            _enumGw2DecorSetting = settings.DefineSetting(
                "enum gw2Decor",
                ColorType.Blue,
                () => "This is an enum setting (drop down menu)",
                () => "...");

            _boolGw2DecorSetting.Value = false;

            // Hidden settings
            _internalGw2DecorSettingSubCollection = settings.AddSubCollection("internal settings (not visible in UI)");
            _hiddenIntGw2DecorSetting = _internalGw2DecorSettingSubCollection.DefineSetting("gw2Decor window x position", 50);
            _hiddenIntGw2DecorSetting2 = _internalGw2DecorSettingSubCollection.DefineSetting("gw2Decor window y position", 50);
        }

        protected override async Task LoadAsync()
        {
            // Load all files in registered directories
            foreach (string directoryName in DirectoriesManager.RegisteredDirectories)
            {
                string fullDirectoryPath = DirectoriesManager.GetFullDirectoryPath(directoryName);
                var allFiles = Directory.EnumerateFiles(fullDirectoryPath, "*", SearchOption.AllDirectories).ToList();

                Logger.Info($"'{directoryName}' can be found at '{fullDirectoryPath}' and has {allFiles.Count} total files within it.");
            }

            // Load the mug texture
            _mugTexture = ContentsManager.GetTexture("test/603447.png");
            var windowBackgroundTexture = AsyncTexture2D.FromAssetId(155997);

            await CreateGw2StyleWindowThatDisplaysAllDecorations(windowBackgroundTexture);
            CreateCornerIconWithContextMenu();
        }

        protected override void Unload()
        {
            // Dispose of created resources
            _cornerIcon?.Dispose();
            _contextMenuStrip?.Dispose();
            _decorationIcon?.Dispose();
            _decorationImage?.Dispose();
            _mugTexture?.Dispose();
            Gw2DecorModuleInstance = null;
        }

        private void CreateCornerIconWithContextMenu()
        {
            // Create the corner icon with a context menu
            _cornerIcon = new CornerIcon
            {
                Icon = _mugTexture,
                BasicTooltipText = $"My Corner Icon Tooltip for {Name}",
                Priority = 1645843523,
                Parent = GameService.Graphics.SpriteScreen
            };

            _cornerIcon.Click += (s, e) => _gw2DecorWindow.ToggleWindow();

            _contextMenuStrip = new ContextMenuStrip();
            _cornerIcon.Menu = _contextMenuStrip;
        }

        List<string> categories = Categories.GetCategories();

        private async Task CreateGw2StyleWindowThatDisplaysAllDecorations(AsyncTexture2D windowBackgroundTexture)
        {
            List<Decoration> decorationsList = new List<Decoration>();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string decorationsUrl = "https://api.guildwars2.com/v2/homestead/decorations";

                    // Step 1: Fetch all decoration IDs
                    var decorationIdsResponse = await httpClient.GetStringAsync(decorationsUrl);
                    var ids = JsonConvert.DeserializeObject<List<int>>(decorationIdsResponse);

                    // Step 2: Process the IDs in batches of 200
                    int batchSize = 200;
                    for (int i = 0; i < ids.Count; i += batchSize)
                    {
                        var batch = ids.Skip(i).Take(batchSize).ToList();
                        var idsString = string.Join(",", batch);
                        var batchUrl = $"{decorationsUrl}?ids={idsString}";

                        try
                        {
                            // Step 3: Fetch decorations for the batch of IDs
                            var batchResponse = await httpClient.GetStringAsync(batchUrl);
                            var batchDecorations = JsonConvert.DeserializeObject<List<Decoration>>(batchResponse);

                            // Fetch all images in parallel for the current batch
                            var imageTasks = new List<Task>();

                            foreach (var decoration in batchDecorations)
                            {
                                // Manipulation of the decoration name to find the correct image from the wiki api
                                var decorationName = decoration.Name
                                    .Replace("WvW", "")
                                    .Replace(":", "-")
                                    .Replace("Guild Ballista", "Guild Ballistae")
                                    .Replace("Raven Spirit Statue", "Raven Statue (decoration)")
                                    .TrimEnd();
                                decorationName = decorationName.EndsWith("Siege")
                                    ? decorationName.Substring(0, decorationName.Length - 6)
                                    : decorationName;

                                var apiUrl = $"https://wiki.guildwars2.com/api.php?action=query&titles=File:{Uri
                                     .EscapeDataString(decorationName)}.jpg&format=json&prop=pageimages&pithumbsize=500";

                                var imageTask = FetchDecorationImage(decoration, apiUrl);
                                imageTasks.Add(imageTask);
                            }

                            // Wait for all image fetch tasks to complete
                            await Task.WhenAll(imageTasks);

                            // Add the decorated decorations to the list
                            decorationsList.AddRange(batchDecorations);
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"Failed to fetch batch of decorations: {e.Message}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Info("Failed to get decoration IDs from API: " + e.Message);
            }

            // After all the decorations are fetched, create and display the window
            _gw2DecorWindow = new StandardWindow(
                windowBackgroundTexture,
                new Rectangle(25, 26, 560, 640),
                new Rectangle(40, 50, 540, 590),
                new Point(1200, 800))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Decorations",
                Emblem = _mugTexture,
                Subtitle = "Homestead Decorations",
                Location = new Point(300, 300),
                SavesPosition = true,
                Id = $"{nameof(Gw2DecorModule)}_Decoration_Window"
            };

            // Create a TextBox for searching decorations
            var searchTextBox = new TextBox
            {
                Parent = _gw2DecorWindow,
                Location = new Point(10, 0),
                Width = 200,
                PlaceholderText = "Search Decorations...",
            };

            // Main container FlowPanel with scroll enabled
            var decorationsFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                Width = 500,
                Height = 640,
                CanScroll = true, // Only main panel should scroll
                Parent = _gw2DecorWindow,
                Location = new Point(10, searchTextBox.Bottom + 10) // Place below the search box
            };

            // Dictionary to store category-specific FlowPanels, mapped by category name
            Dictionary<string, FlowPanel> categoryPanels = new Dictionary<string, FlowPanel>();

            // Loop through categories to create and add each category panel
            foreach (string category in categories)
            {
                // Create a FlowPanel for each category
                var categoryFlowPanel = new FlowPanel
                {
                    Title = category,
                    FlowDirection = ControlFlowDirection.LeftToRight,
                    Width = decorationsFlowPanel.Width - 20, // Slightly less than main panel width to avoid overflow
                    Height = decorationsFlowPanel.Height - 480,
                    CanCollapse = true,
                    CanScroll = false, // Individual category panels should not scroll
                    Parent = decorationsFlowPanel // Set parent as the main FlowPanel
                };

                // Add category FlowPanel to the dictionary for later use
                categoryPanels[category] = categoryFlowPanel;
            }

            // Create a label to display the decoration name
            var decorationNameLabel = new Label
            {
                Text = "Select a decoration",
                Parent = _gw2DecorWindow,
                Location = new Point(decorationsFlowPanel.Right + 10, 20), // Position it next to the icons
                Width = 200,
                Height = 40,
            };

            // Create an image control to show the selected decoration's icon
            _decorationIcon = new Image
            {
                Size = new Point(40, 40),
                Parent = _gw2DecorWindow,
                Location = new Point(decorationNameLabel.Left, decorationNameLabel.Bottom + 5)
            };

            _decorationImage = new Image
            {
                Size = new Point(400, 400),
                Parent = _gw2DecorWindow,
                Location = new Point(decorationNameLabel.Left, _decorationIcon.Bottom + 5)
            };

            // Create a function to update the decorations based on the search input
            void UpdateDecorationList(string searchTerm)
            {
                // Clear all decorations from each category panel before re-populating
                foreach (var categoryPanel in categoryPanels.Values)
                {
                    foreach (var control in categoryPanel.Children.ToList())
                    {
                        categoryPanel.RemoveChild(control);
                    }
                }

                // Filter decorations based on the search term
                var filteredDecorations = decorationsList
                    .Where(decoration => decoration.Name != null && decoration.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                // Populate category panels with filtered decorations
                foreach (var decoration in filteredDecorations)
                {
                    // Find the category for the decoration
                    string categoryName = categories[decoration.Categories.FirstOrDefault() - 1]; // Adjust index as needed

                    // Get the category panel from the dictionary
                    if (categoryPanels.TryGetValue(categoryName, out var categoryPanel))
                    {
                        // Load the decoration icon texture (use a default if not available)
                        var iconTexture = AsyncTexture2D.FromAssetId(decoration.IconAssetId);

                        if (iconTexture != null)
                        {
                            var tooltipText = $"{decoration.Name ?? "Unknown Decoration"}\n{decoration.Description ?? "No description available"}";
                            var decorationIcon = new Image(iconTexture)
                            {
                                BasicTooltipText = tooltipText,
                                Size = new Point(40),
                                Parent = categoryPanel, // Add to the specific category panel
                            };

                            // Click event to update main display for the selected decoration
                            decorationIcon.Click += async (s, e) =>
                            {
                                decorationNameLabel.Text = decoration.Name ?? "Unknown Decoration";

                                if (!string.IsNullOrEmpty(decoration.Image))
                                {
                                    using (var httpClientInnerInner = new HttpClient())
                                    {
                                        try
                                        {
                                            httpClientInnerInner.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 5.0.1; GT-I9505 Build/LRX22C) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.98 Mobile Safari/537.36");

                                            var imageResponse = await httpClientInnerInner.GetByteArrayAsync(decoration.Image);
                                            using (var memoryStream = new MemoryStream(imageResponse))
                                            {
                                                using (var graphicsContext = GameService.Graphics.LendGraphicsDeviceContext())
                                                {
                                                    var loadedTexture = Texture2D.FromStream(graphicsContext.GraphicsDevice, memoryStream);
                                                    _decorationImage.Texture = loadedTexture;
                                                    // Resize the decoration image based on the loaded texture's size
                                                    _decorationImage.Size = new Point(loadedTexture.Width, loadedTexture.Height);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Warn($"Failed to load decoration image for '{decoration.Name}'. Error: {ex.Message}");
                                        }
                                    }
                                }
                                else
                                {
                                    Logger.Info("Decoration image URL is not available.");
                                }
                            };
                        }
                        else
                        {
                            Logger.Info("Failed to create decoration image. Either the icon texture or the decorations panel is null.");
                        }
                    }
                }
            }

            // Attach an event handler to the TextBox to filter decorations on text change
            searchTextBox.TextChanged += (s, e) => UpdateDecorationList(searchTextBox.Text);

            // Initial population of decorations
            UpdateDecorationList(string.Empty); // Show all decorations initially

            _gw2DecorWindow.Show();
        }

        // Helper method to fetch image for a single decoration
        private async Task FetchDecorationImage(Decoration decoration, string apiUrl)
        {
            try
            {
                using (var httpClientInner = new HttpClient())
                {
                    httpClientInner.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 5.0.1; GT-I9505 Build/LRX22C) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.98 Mobile Safari/537.36");

                    // First attempt to get the thumbnail from the API
                    var response = await httpClientInner.GetStringAsync(apiUrl);
                    var json = JsonConvert.DeserializeObject<JObject>(response);
                    var pages = json["query"]?["pages"];
                    string thumbnailPage = null;

                    if (pages != null)
                    {
                        foreach (var page in pages.Children())
                        {
                            var thumbnail = page.First["thumbnail"]?["source"]?.ToString();
                            if (!string.IsNullOrEmpty(thumbnail))
                            {
                                thumbnailPage = thumbnail; // Set the thumbnail if found
                            }
                        }
                    }

                    // If no thumbnail is found, attempt with a modified URL
                    if (string.IsNullOrEmpty(thumbnailPage))
                    {
                        // Prepend "Decoration-" to the URL and try again
                        var decoratedApiUrl = apiUrl.Replace("File:", "File:Decoration- ");
                        response = await httpClientInner.GetStringAsync(decoratedApiUrl);
                        json = JsonConvert.DeserializeObject<JObject>(response);
                        pages = json["query"]?["pages"];

                        if (pages != null)
                        {
                            foreach (var page in pages.Children())
                            {
                                var thumbnail = page.First["thumbnail"]?["source"]?.ToString();
                                if (!string.IsNullOrEmpty(thumbnail))
                                {
                                    thumbnailPage = thumbnail; // Set the thumbnail if found
                                    break;
                                }
                            }
                        }
                    }

                    // If still no thumbnail, replace `.jpg` with ` (decoration).jpg` in the URL
                    if (string.IsNullOrEmpty(thumbnailPage))
                    {
                        var modifiedApiUrl = apiUrl.Replace(".jpg", " (decoration).jpg");
                        response = await httpClientInner.GetStringAsync(modifiedApiUrl);
                        json = JsonConvert.DeserializeObject<JObject>(response);
                        pages = json["query"]?["pages"];

                        if (pages != null)
                        {
                            foreach (var page in pages.Children())
                            {
                                var thumbnail = page.First["thumbnail"]?["source"]?.ToString();
                                if (!string.IsNullOrEmpty(thumbnail))
                                {
                                    thumbnailPage = thumbnail; // Set the thumbnail if found
                                    break;
                                }
                            }
                        }
                    }

                    // Assign the image to the decoration
                    decoration.Image = thumbnailPage;
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to get image for decoration '{decoration.Name}': {e.Message}");
            }
        }
    }
}
