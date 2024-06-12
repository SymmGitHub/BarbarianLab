using BarbarianLab.Utilities;
using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static BoB_LevelData;

namespace BarbarianLab
{
    public partial class Form1 : Form
    {
        Level cur_level = new Level();
        List<Level> allLevels = new List<Level>();
        List<Theme> windowThemes = new List<Theme>();
        List<Level> levelHistory = new List<Level>();
        Bitmap? recentShot;
        Config config = new Config();
        Color[] playerColors = new Color[4]
        {
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(0, 255, 255),
            Color.FromArgb(255, 180, 0)
        };
        Color enemyColor = Color.FromArgb(200, 50, 115);
        Dictionary<int, string> characterNames = new Dictionary<int, string>();
        Dictionary<int, string> affectedTileStrings = new Dictionary<int, string>();
        Color[] tileColors = new Color[0];
        Image[] tileSprites = new Image[0];
        bool[] tileCollision = new bool[0];
        bool[] tileFluctuation = new bool[0];
        bool loading = false;
        public int tileX = -1;
        public int tileY = -1;
        public int selectingX = -1;
        public int selectingY = -1;
        public int floatingSelectionX = -1;
        public int floatingSelectionY = -1;
        public int floatingSelectionW = -1;
        public int floatingSelectionD = -1;
        BoB_Tile[]? floatingSelection;
        int? draggingSelectionOffsetX;
        int? draggingSelectionOffsetY;
        public decimal lowestHeight;
        public decimal highestHeight;
        string toolType = "";
        int viewType = 0;
        bool banCursor = false;
        bool toolActing = false;
        int maxEnemyCount = 8;
        int concatThreshold = 999;
        int levelThreshold = 6000;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            int previousTheme = 0;

            config.ParseConfig(AppDomain.CurrentDomain.BaseDirectory + "\\Config.txt");
            if (config.sections.ContainsKey("Miscellaneous"))
            {
                int newMaxX;
                if (int.TryParse(config.sections["Miscellaneous"].settings["Max Level Width"].data[0], out newMaxX))
                {
                    if (newMaxX < 1)
                    {
                        newMaxX = 1;
                    }
                    levelWidth.Maximum = newMaxX;
                }
                int newMaxY;
                if (int.TryParse(config.sections["Miscellaneous"].settings["Max Level Depth"].data[0], out newMaxY))
                {
                    if (newMaxY < 1)
                    {
                        newMaxY = 1;
                    }
                    levelDepth.Maximum = newMaxY;
                }
                int enemyMax;
                if (int.TryParse(config.sections["Miscellaneous"].settings["Max Enemy Count"].data[0], out enemyMax))
                {
                    maxEnemyCount = enemyMax;
                }
                int concatThresh;
                if (int.TryParse(config.sections["Miscellaneous"].settings["Array Concat Substring Limit"].data[0], out concatThresh))
                {
                    concatThreshold = concatThresh;
                }
                int levelThresh;
                if (int.TryParse(config.sections["Miscellaneous"].settings["Level Init Substring Limit"].data[0], out levelThresh))
                {
                    levelThreshold = levelThresh;
                }
                int pIndex = 1;
                List<Color> playerColorList = new List<Color>();
                while (config.sections["Miscellaneous"].settings.ContainsKey($"Player {pIndex} Color"))
                {
                    try
                    {
                        int r = int.Parse(config.sections["Miscellaneous"].settings[$"Player {pIndex} Color"].data[0]);
                        int g = int.Parse(config.sections["Miscellaneous"].settings[$"Player {pIndex} Color"].data[1]);
                        int b = int.Parse(config.sections["Miscellaneous"].settings[$"Player {pIndex} Color"].data[2]);
                        playerColorList.Add(Color.FromArgb(r, g, b));
                    }
                    catch { }
                    pIndex++;
                }
                if (playerColorList.Count > 0) playerColors = playerColorList.ToArray();
                if (config.sections["Miscellaneous"].settings.ContainsKey("Enemy Color"))
                {
                    try
                    {
                        int r = int.Parse(config.sections["Miscellaneous"].settings[$"Player {pIndex} Color"].data[0]);
                        int g = int.Parse(config.sections["Miscellaneous"].settings[$"Player {pIndex} Color"].data[1]);
                        int b = int.Parse(config.sections["Miscellaneous"].settings[$"Player {pIndex} Color"].data[2]);
                        enemyColor = Color.FromArgb(r, g, b);
                    }
                    catch { }
                }
                try
                {
                    int.TryParse(config.sections["Miscellaneous"].settings["Previous Theme Index"].data[0], out previousTheme);
                }
                catch { }
            }
            if (config.sections.ContainsKey("Themes"))
            {
                foreach (string name in config.sections["Themes"].settings.Keys)
                {
                    string[] data = config.sections["Themes"].settings[name].data;
                    Theme newTheme = new Theme();
                    for (int i = 0; i < data.Length; i++)
                    {
                        string colorType = data[i].Substring(0, data[i].IndexOf("(") - 1).Trim().ToUpper().Replace(" ", "_");
                        string colorData = data[i].Substring(data[i].IndexOf("(") + 1);
                        colorData = colorData.Substring(0, colorData.Length - 1);
                        Color newCol = ColorFromStringOrRGB(colorData);
                        switch (colorType)
                        {
                            case "BG":
                                newTheme.bg = newCol;
                                break;
                            case "LEVEL_BG":
                                newTheme.level_bg = newCol;
                                break;
                            case "UI":
                                newTheme.ui = newCol;
                                break;
                            case "BUTTONS":
                                newTheme.btn = newCol;
                                break;
                            case "BG_TEXT":
                                newTheme.bg_text = newCol;
                                break;
                            case "UI_TEXT":
                                newTheme.ui_text = newCol;
                                break;
                            case "BUTTON_TEXT":
                                newTheme.btn_text = newCol;
                                break;
                            case "TOOL":
                                newTheme.tool = newCol;
                                break;
                            case "TOOL_PRESSED":
                                newTheme.tool_pressed = newCol;
                                break;
                            case "HIGHLIGHT":
                                newTheme.highlight = newCol;
                                break;
                        }
                    }
                    windowThemes.Add(newTheme);
                    ThemeSelectionBox.Items.Add($"Theme: {name}");
                }
            }
            if (config.sections.ContainsKey("Characters"))
            {
                foreach (string character in config.sections["Characters"].settings.Keys)
                {
                    string data = config.sections["Characters"].settings[character].data[0];
                    characterNames.Add(int.Parse(data), character);
                    e_Name.Items.Add(character);
                }
            }
            if (config.sections.ContainsKey("Tiles"))
            {
                tileColors = new Color[config.sections["Tiles"].settings.Count + 1];
                tileSprites = new Image[config.sections["Tiles"].settings.Count + 1];
                tileCollision = new bool[config.sections["Tiles"].settings.Count + 1];
                tileFluctuation = new bool[config.sections["Tiles"].settings.Count + 1];
                tileCollision[0] = true;
                tileFluctuation[0] = true;
                foreach (string tile in config.sections["Tiles"].settings.Keys)
                {
                    try
                    {
                        int index = int.Parse(config.sections["Tiles"].settings[tile].data[0]);
                        if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + $"\\Tiles\\{index}.png"))
                        {
                            tileSprites[index] = Bitmap.FromFile(AppDomain.CurrentDomain.BaseDirectory + $"\\Tiles\\{index}.png");
                        }
                        try
                        {
                            tileCollision[index] = false;
                            tileFluctuation[index] = false;
                            if (config.sections["Tiles"].settings[tile].data[1].ToLower() == "collision")
                            {
                                tileCollision[index] = true;
                            }
                            if (config.sections["Tiles"].settings[tile].data[2].ToLower() == "fluctuatingheight")
                            {
                                tileFluctuation[index] = true;
                            }
                            int r = int.Parse(config.sections["Tiles"].settings[tile].data[3]);
                            int g = int.Parse(config.sections["Tiles"].settings[tile].data[4]);
                            int b = int.Parse(config.sections["Tiles"].settings[tile].data[5]);
                            tileColors[index] = Color.FromArgb(r, g, b);
                        }
                        catch { tileColors[index] = Color.FromArgb(255, 0, 255); }
                        t_Name.Items.Add(tile);
                    }
                    catch { }
                }
            }

            // Post-Config-Parsing loading
            t_ID.Value = 2;
            if (ThemeSelectionBox.Items.Count > 0)
            {
                previousTheme = Math.Clamp(previousTheme, 0, ThemeSelectionBox.Items.Count - 1);
                ThemeSelectionBox.SelectedIndex = previousTheme;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            config.WriteConfig(AppDomain.CurrentDomain.BaseDirectory + "\\Config.txt");
        }
        private void UpdateTheme(Theme t)
        {
            BackColor = t.bg;
            splitContainer1.BackColor = t.bg;
            LevelMap.BackColor = t.level_bg;
            label1.ForeColor = t.bg_text;
            label2.ForeColor = t.bg_text;
            label3.ForeColor = t.bg_text;
            label4.ForeColor = t.bg_text;
            label5.ForeColor = t.bg_text;
            label6.ForeColor = t.bg_text;
            label7.ForeColor = t.bg_text;
            label8.ForeColor = t.bg_text;
            label9.ForeColor = t.bg_text;
            label10.ForeColor = t.bg_text;
            label11.ForeColor = t.bg_text;
            label12.ForeColor = t.bg_text;
            label13.ForeColor = t.bg_text;
            label14.ForeColor = t.bg_text;
            label15.ForeColor = t.bg_text;
            label16.ForeColor = t.bg_text;
            label17.ForeColor = t.bg_text;
            label18.ForeColor = t.bg_text;
            label19.ForeColor = t.bg_text;
            tipText.ForeColor = t.bg_text;
            t_Collision.ForeColor = t.bg_text;

            ToolPaint.BackColor = t.tool;
            ToolErase.BackColor = t.tool;
            ToolSelect.BackColor = t.tool;
            ToolCollision.BackColor = t.tool;
            ToolElevate.BackColor = t.tool;

            elev_increment.BackColor = t.ui;
            elev_increment.ForeColor = t.ui_text;
            t_Sprite.BackColor = t.ui;
            t_ID.BackColor = t.ui;
            t_ID.ForeColor = t.ui_text;
            t_Name.BackColor = t.ui;
            t_Name.ForeColor = t.ui_text;
            t_Height.BackColor = t.ui;
            t_Height.ForeColor = t.ui_text;

            levelSelectionBox.BackColor = t.ui;
            levelSelectionBox.ForeColor = t.ui_text;
            levelWidth.BackColor = t.ui;
            levelWidth.ForeColor = t.ui_text;
            levelDepth.BackColor = t.ui;
            levelDepth.ForeColor = t.ui_text;
            levelMinX.BackColor = t.ui;
            levelMinX.ForeColor = t.ui_text;
            levelMinY.BackColor = t.ui;
            levelMinY.ForeColor = t.ui_text;
            p1x.BackColor = t.ui;
            p1x.ForeColor = t.ui_text;
            p1y.BackColor = t.ui;
            p1y.ForeColor = t.ui_text;
            p2x.BackColor = t.ui;
            p2x.ForeColor = t.ui_text;
            p2y.BackColor = t.ui;
            p2y.ForeColor = t.ui_text;
            p3x.BackColor = t.ui;
            p3x.ForeColor = t.ui_text;
            p3y.BackColor = t.ui;
            p3y.ForeColor = t.ui_text;
            p4x.BackColor = t.ui;
            p4x.ForeColor = t.ui_text;
            p4y.BackColor = t.ui;
            p4y.ForeColor = t.ui_text;
            e_Type.BackColor = t.ui;
            e_Type.ForeColor = t.ui_text;
            e_Name.BackColor = t.ui;
            e_Name.ForeColor = t.ui_text;
            EnemyList.BackColor = t.ui;
            EnemyList.ForeColor = t.ui_text;
            ex.BackColor = t.ui;
            ex.ForeColor = t.ui_text;
            ey.BackColor = t.ui;
            ey.ForeColor = t.ui_text;

            removeLevel.BackColor = t.btn;
            removeLevel.ForeColor = t.btn_text;
            AddEnemy.BackColor = t.btn;
            AddEnemy.ForeColor = t.btn_text;
            DeleteEnemy.BackColor = t.btn;
            DeleteEnemy.ForeColor = t.btn_text;
        }

        // ==========================================
        // ============== TOOLSTRIP =================
        // ==========================================
        private void openLevelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Theme t = windowThemes[ThemeSelectionBox.SelectedIndex];
            string windowText =
                $"// Copy-Paste any amount of levels below to import them into the tool.{Environment.NewLine}" +
                $"// You can even put entire f_Levels_InitLevels functions here!{Environment.NewLine}{Environment.NewLine}" +
                $"// _locX_[Y] = new Array(Big list of numbers);{Environment.NewLine}" +
                $"// _locX_[Y] = f_ConcatArray(_locX_[Y],new Array(Additional data for larger levels);{Environment.NewLine}";

            TextWindow open = new TextWindow(t, "Open_Level", windowText);
            if (open.ShowDialog() == DialogResult.OK)
            {
                if (loading || open.levels == null) return;
                loading = true;
                int firstNew = allLevels.Count;
                allLevels.AddRange(open.levels);
                RefreshLevelList();
                loading = false;
                levelSelectionBox.SelectedIndex = Math.Clamp(firstNew, -1, levelSelectionBox.Items.Count - 1);
            }
        }
        private void newLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allLevels.Add(Level.ParseLevels($"new Array({Level.DefaultLevelString()});", false)[0]);
            affectedTileStrings = new Dictionary<int, string>();
            loading = true;
            RefreshLevelList();
            loading = false;
            levelSelectionBox.SelectedIndex = levelSelectionBox.Items.Count - 1;
        }
        private void AutosaveLevel()
        {
            if (levelSelectionBox.SelectedIndex < 0 || levelSelectionBox.SelectedIndex >= allLevels.Count) return;
            allLevels[levelSelectionBox.SelectedIndex] = cur_level;
            int prevIndex = levelSelectionBox.SelectedIndex;
            loading = true;
            RefreshLevelList();
            levelSelectionBox.SelectedIndex = prevIndex;
            loading = false;
        }
        private void RemoveLevel()
        {
            allLevels.RemoveAt(levelSelectionBox.SelectedIndex);
            int prevIndex = levelSelectionBox.SelectedIndex;
            loading = true;
            RefreshLevelList();
            loading = false;
            levelSelectionBox.SelectedIndex = prevIndex - 1;
            if (levelSelectionBox.SelectedIndex == -1)
            {
                if (levelSelectionBox.Items.Count > 1)
                {
                    levelSelectionBox.SelectedIndex = 0;
                }
                else
                {
                    levelSelectionBox.Text = "";
                    cur_level.parse_succeeded = false;
                    LevelMap.BackgroundImage = null;
                }

            }
        }
        private void RefreshLevelList()
        {
            if (allLevels.Count > 0)
            {
                levelSelectionBox.Items.Clear();
                for (int i = 0; i < allLevels.Count; i++)
                {
                    Level l = allLevels[i];
                    int width = l.maxX - l.minX;
                    int depth = l.maxY - l.minY;
                    string e_name = $"Unknown";
                    if (characterNames.ContainsKey(l.enemy_type))
                    {
                        e_name = characterNames[l.enemy_type];
                    }
                    string name = $"{e_name} ({width},{depth})";
                    string finalName = name;
                    int copyIndex = 2;
                    while (levelSelectionBox.Items.Contains(finalName))
                    {
                        finalName = name + $" ({copyIndex})";
                        copyIndex++;
                    }
                    levelSelectionBox.Items.Add(finalName);
                }
            }
        }
        private void exportLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string windowText = $"// Copy the level array below!{Environment.NewLine}" +
                $"// Make sure both the _loc_ number and index{Environment.NewLine}" +
                $"// match the level you're replacing...{Environment.NewLine}{Environment.NewLine}" +
                $"{Level.ExportedLevelString(cur_level, "_loc3_", 0, concatThreshold)}{Environment.NewLine}";
                Theme t = windowThemes[ThemeSelectionBox.SelectedIndex];
                TextWindow save = new TextWindow(t, "Export_Level", windowText);
                save.ShowDialog();
            }
            catch { }
        }
        private void exportPlaylistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (allLevels.Count <= 0) return;

            // First group all the levels, dividing them into groups to not obliterate JPexs's byte limits.
            int globalSubstringCount = 0;
            List<Level> curLevelGroup = new List<Level>();
            List<Level[]> levelGroups = new List<Level[]>();
            for (int i = 0; i < allLevels.Count; i++)
            {
                curLevelGroup.Add(allLevels[i]);
                globalSubstringCount += Level.LevelSubstrings(allLevels[i]).Length;
                if (globalSubstringCount > (levelThreshold * (levelGroups.Count + 1)))
                {
                    levelGroups.Add(curLevelGroup.ToArray());
                    curLevelGroup = new List<Level>();
                }
            }
            levelGroups.Add(curLevelGroup.ToArray());


            // Create function f_Levels_InitLevels
            int curLevelIndex = 0;
            string windowText = $"// Copy the level init functions below!{Environment.NewLine}" +
            $"// If you don't want levels to be shuffled,{Environment.NewLine}" +
            $"// erase the if condition containing f_ShuffleArray{Environment.NewLine}" +
            Environment.NewLine +
            $"function f_Levels_InitLevels(o_game){Environment.NewLine}" +
            "{" + Environment.NewLine +
            $"   if(_root.a_levels != undefined){Environment.NewLine}" +
            "   {" + Environment.NewLine +
            $"      o_game.a_levels = _root.a_levels;{Environment.NewLine}" +
            $"      trace(\"REUSE\");{Environment.NewLine}" +
            $"      return undefined;{Environment.NewLine}" +
            "   }" + Environment.NewLine +
            $"   _root.a_newLevel = new Array({Level.DefaultLevelString()}){Environment.NewLine}" +
            $"   var levels = new Array({allLevels.Count});{Environment.NewLine}" +
            $"   _root.a_levels = levels;{Environment.NewLine}" +
            $"   o_game.a_levels = levels;{Environment.NewLine}";

            // Write each of the first level group's levels
            for (int i = 0; i < levelGroups[0].Length; i++)
            {
                windowText += $"   {Level.ExportedLevelString(levelGroups[0][i], "levels", curLevelIndex, concatThreshold)}{Environment.NewLine}";
                curLevelIndex++;
            }

            // Write calls for each addendum level group function
            for (int i = 1; i < levelGroups.Count; i++)
            {
                windowText += $"   f_Levels_InitLevels{i + 1}(o_game);" + Environment.NewLine;
            }

            // Write shuffling
            windowText +=
            $"   if(n_debugLevel == undefined){Environment.NewLine}" +
            "   {" + Environment.NewLine +
            $"      f_ShuffleArray(levels,levels.length);{Environment.NewLine}" +
            "   }" + Environment.NewLine +
            "}" + Environment.NewLine;

            // Write each addendum function
            for (int i = 1; i < levelGroups.Count; i++)
            {
                windowText +=
                $"function f_Levels_InitLevels{i + 1}(o_game){Environment.NewLine}" +
                "{" + Environment.NewLine +
                $"   var levels = _root.a_levels;{Environment.NewLine}";
                for (int j = 0; j < levelGroups[i].Length; j++)
                {
                    windowText += $"   {Level.ExportedLevelString(levelGroups[i][j], "levels", curLevelIndex, concatThreshold)}{Environment.NewLine}";
                    curLevelIndex++;
                }
                windowText +=
                "}" + Environment.NewLine;
            }
            Theme t = windowThemes[ThemeSelectionBox.SelectedIndex];
            TextWindow save = new TextWindow(t, "Export_Playlist", windowText);
            save.ShowDialog();
        }
        private void saveScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (recentShot == null) return;
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Image File|*.png";
            save.DefaultExt = ".png";
            save.FileName = "level0.png";
            save.Title = "Save Screenshot";
            if (save.ShowDialog() == DialogResult.OK)
            {
                string filePath = save.FileName;
                banCursor = true;
                UpdateLevelImage(sender, e);
                recentShot.MakeTransparent(Color.Black);
                Bitmap bmp;
                Graphics g = Graphics.FromImage(bmp = new Bitmap(recentShot.Width * 10, recentShot.Height * 10));
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(recentShot, 0, 0, recentShot.Width * 10, recentShot.Height * 10);
                g.Dispose();
                bmp.Save(filePath, ImageFormat.Png);
                UpdateLevelImage(sender, e);
                banCursor = false;
            }
        }
        private void roundTileHeightsToThousandthsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < cur_level.tiles.Length; i++)
            {
                BoB_Tile t = cur_level.tiles[i];
                t.h = Math.Round(t.h, 3);
                cur_level.tiles[i] = t;
            }
        }
        private void setAllTileCollisionsToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < cur_level.tiles.Length; i++)
            {
                BoB_Tile t = cur_level.tiles[i];
                if (t.id > 0 && t.id < tileCollision.Length)
                {
                    t.col = 0;
                    if (tileCollision[t.id])
                    {
                        t.col = 1;
                    }
                    cur_level.tiles[i] = t;
                }
            }
            UpdateLevelImage(sender, e);
        }

        private void heightMultiplyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            decimal multiplier;
            if (!cur_level.parse_succeeded) return;
            if (decimal.TryParse(heightMultiplierBox.Text, out multiplier))
            {
                for (int i = 0; i < cur_level.tiles.Length; i++)
                {
                    BoB_Tile t = cur_level.tiles[i];
                    t.h *= multiplier;
                    cur_level.tiles[i] = t;
                }
                UpdateLevelHeights();
            }
            else
            {
                MessageBox.Show("Height multiplier was invalid...");
            }
            UpdateLevelImage(sender, e);
        }

        private void LoadLevelData(bool undoing)
        {
            loading = true;
            int newWidth = (cur_level.maxX - cur_level.minX);
            if (newWidth > levelWidth.Maximum)
            {
                levelWidth.Maximum = newWidth;
            }
            levelWidth.Value = newWidth;
            int newDepth = (cur_level.maxY - cur_level.minY);
            if (newDepth > levelDepth.Maximum)
            {
                levelDepth.Maximum = newDepth;
            }
            levelDepth.Value = newDepth;
            levelMinX.Value = cur_level.minX;
            levelMinY.Value = cur_level.minY;
            p1x.Value = cur_level.players[0].x;
            p1y.Value = cur_level.players[0].y;
            p2x.Value = cur_level.players[1].x;
            p2y.Value = cur_level.players[1].y;
            p3x.Value = cur_level.players[2].x;
            p3y.Value = cur_level.players[2].y;
            p4x.Value = cur_level.players[3].x;
            p4y.Value = cur_level.players[3].y;
            if (cur_level.enemy_type >= 2)
            {
                e_Name.SelectedIndex = cur_level.enemy_type - 2;
            }
            e_Type.Text = cur_level.enemy_type.ToString();
            if (characterNames.ContainsKey(cur_level.enemy_type))
            {
                e_Name.Text = characterNames[cur_level.enemy_type];
            }
            RefreshEnemyList();
            UpdateLevelHeights();
            loading = false;
            UpdateLevelImage(LevelMap, new EventArgs());

            if (EnemyList.Items.Count > 0)
            {
                EnemyList.SelectedIndex = 0;
            }
            if (!undoing) AddLevelHistory();
        }
        private void SelectTool(uint type)
        {
            if (floatingSelection != null)
            {
                PlaceSelection();
            }
            toolType = "";
            tipText.Text = "";
            Theme t = windowThemes[ThemeSelectionBox.SelectedIndex];
            ToolPaint.BackColor = t.tool;
            ToolPaint.Enabled = true;
            ToolErase.BackColor = t.tool;
            ToolErase.Enabled = true;
            ToolCollision.BackColor = t.tool;
            ToolCollision.Enabled = true;
            ToolElevate.BackColor = t.tool;
            ToolElevate.Enabled = true;
            ToolSelect.BackColor = t.tool;
            ToolSelect.Enabled = true;
            switch (type)
            {
                case 1:
                    toolType = "paint";
                    ToolPaint.BackColor = t.tool_pressed;
                    ToolPaint.Enabled = false;
                    tipText.Text = "Left Click: Paint  |  Shift + Left-Click: Paint (ID Only)" + Environment.NewLine +
                    "Ctrl + Left-Click: Replace All of an ID  |  Right-Click: Tile Eyedropper Tool";
                    break;
                case 2:
                    toolType = "erase";
                    ToolErase.BackColor = t.tool_pressed;
                    ToolErase.Enabled = false;
                    tipText.Text = "Left Click: Erase  |  Ctrl + Left-Click: Erase All of an ID" + Environment.NewLine +
                    "Right-Click: Tile Eyedropper Tool";
                    break;
                case 3:
                    toolType = "select";
                    ToolSelect.BackColor = t.tool_pressed;
                    ToolSelect.Enabled = false;
                    tipText.Text = "Left Click: Select/Move Selection  |  Ctrl + Delete: Delete Tile Selection" + Environment.NewLine +
                    "Ctrl + C / X / V: Copy/Cut/Paste Tile Selection";
                    break;
                case 4:
                    toolType = "collision";
                    ToolCollision.BackColor = t.tool_pressed;
                    ToolCollision.Enabled = false;
                    tipText.Text = "Left Click: Edit Collision  |  Right-Click: Tile Eyedropper Tool" + Environment.NewLine +
                    "Darker gray tiles have tile IDs where default collision takes priority.";
                    break;
                case 5:
                    toolType = "elevation";
                    ToolElevate.BackColor = t.tool_pressed;
                    ToolElevate.Enabled = false;
                    tipText.Text = "Left Click: Increment Tile Height  |  Shift + Left-Click: Reverse Increment Height" + Environment.NewLine +
                    "Ctrl + Left-Click: Set Tile Height  |  Right-Click: Tile Eyedropper Tool";
                    break;
            }
            UpdateLevelImage(LevelMap, new EventArgs());
        }
        private void unselectToolsToolStripMenuItem_Click(object sender, EventArgs e) { SelectTool(0); }
        private void paintToolToolStripMenuItem_Click(object sender, EventArgs e) { SelectTool(1); }
        private void eraseToolToolStripMenuItem_Click(object sender, EventArgs e) { SelectTool(2); }
        private void selectionToolToolStripMenuItem_Click(object sender, EventArgs e) { SelectTool(3); }
        private void collisionToolToolStripMenuItem_Click(object sender, EventArgs e) { SelectTool(4); }
        private void elevationToolToolStripMenuItem_Click(object sender, EventArgs e) { SelectTool(5); }
        private void ToolPaint_Click(object sender, EventArgs e) { paintToolToolStripMenuItem_Click(sender, e); }
        private void ToolErase_Click(object sender, EventArgs e) { eraseToolToolStripMenuItem_Click(sender, e); }
        private void ToolCollision_Click(object sender, EventArgs e) { collisionToolToolStripMenuItem_Click(sender, e); }
        private void ToolElevate_Click(object sender, EventArgs e) { elevationToolToolStripMenuItem_Click(sender, e); }
        private void ToolSelect_Click(object sender, EventArgs e) { selectionToolToolStripMenuItem_Click(sender, e); }
        private void SelectViewType(int type)
        {
            viewType = type;
            viewContextualToolStripMenuItem.Enabled = true;
            viewTileColorToolStripMenuItem.Enabled = true;
            viewTileCollisionMapToolStripMenuItem.Enabled = true;
            viewTileHeightMapToolStripMenuItem.Enabled = true;
            viewContextualToolStripMenuItem.Checked = false;
            viewTileColorToolStripMenuItem.Checked = false;
            viewTileCollisionMapToolStripMenuItem.Checked = false;
            viewTileHeightMapToolStripMenuItem.Checked = false;
            switch (type)
            {
                case 0:
                    viewContextualToolStripMenuItem.Enabled = false;
                    viewContextualToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    viewTileColorToolStripMenuItem.Enabled = false;
                    viewTileColorToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    viewTileCollisionMapToolStripMenuItem.Enabled = false;
                    viewTileCollisionMapToolStripMenuItem.Checked = true;
                    break;
                case 3:
                    viewTileHeightMapToolStripMenuItem.Enabled = false;
                    viewTileHeightMapToolStripMenuItem.Checked = true;
                    break;
            }
            UpdateLevelImage(LevelMap, new EventArgs());
        }
        private void viewContextualToolStripMenuItem_Click(object sender, EventArgs e) { SelectViewType(0); }
        private void viewTileColorToolStripMenuItem_Click(object sender, EventArgs e) { SelectViewType(1); }
        private void viewTileCollisionMapToolStripMenuItem_Click(object sender, EventArgs e) { SelectViewType(2); }
        private void viewTileHeightMapToolStripMenuItem_Click(object sender, EventArgs e) { SelectViewType(3); }

        private void UpdateLevelDimensions(object sender, EventArgs e)
        {
            if (loading) return;
            loading = true;
            try
            {
                AddLevelHistory();
                int width = cur_level.maxX - cur_level.minX;
                int depth = cur_level.maxY - cur_level.minY;
                int pWidth = width;
                int pDepth = depth;
                int widthDifference = (int)levelWidth.Value - width;
                int depthDifference = (int)levelDepth.Value - depth;
                affectedTileStrings = new Dictionary<int, string>();
                List<BoB_Tile> tileList = cur_level.tiles.ToList();
                BoB_Tile t = new BoB_Tile();
                t.id = 1;
                t.h = 0;
                t.col = 0;

                // Update Width
                if (widthDifference > 0)
                {
                    for (int w = 0; w < widthDifference; w++)
                    {
                        for (int d = 0; d < depth; d++)
                        {
                            tileList.Insert((d * width) + width + d, t);
                        }
                        cur_level.maxX++;
                        width = cur_level.maxX - cur_level.minX;
                    }
                }
                else if (widthDifference < 0)
                {
                    for (int w = 0; w < -widthDifference; w++)
                    {
                        for (int d = 0; d < depth; d++)
                        {
                            int wIndex = ((d * width) + width) - (1 + d);
                            BoB_Tile pt = tileList[wIndex];
                            affectedTileStrings.Add(wIndex, $"{wIndex},{pt.id},{pt.h},{pt.col}");
                            tileList.RemoveAt(wIndex);
                        }
                        cur_level.maxX--;
                        width = cur_level.maxX - cur_level.minX;
                    }
                }

                // Update Height 
                if (depthDifference > 0)
                {
                    for (int d = 0; d < depthDifference; d++)
                    {
                        for (int w = 0; w < width; w++)
                        {
                            tileList.Add(t);
                        }
                        cur_level.maxY++;
                        depth = cur_level.maxY - cur_level.minY;
                    }
                }
                else if (depthDifference < 0)
                {
                    for (int d = 0; d < -depthDifference; d++)
                    {
                        //tileList.RemoveRange(((depth - 1) * width), width);
                        for (int w = 0; w < width; w++)
                        {
                            int dIndex = tileList.Count - 1;
                            BoB_Tile pt = tileList[dIndex];
                            affectedTileStrings.Add(dIndex, $"{dIndex},{pt.id},{pt.h},{pt.col}");
                            tileList.RemoveAt(dIndex);
                        }
                        cur_level.maxY--;
                        depth = cur_level.maxY - cur_level.minY;
                    }
                }
                cur_level.tiles = tileList.ToArray();
                AutosaveLevel();
            }
            catch { }
            loading = false;
            UpdateLevelImage(sender, e);
        }
        private void UpdateLevelMinValues(object sender, EventArgs e)
        {
            if (loading || !cur_level.parse_succeeded) return;
            loading = true;
            try
            {
                int newMinX = (int)levelMinX.Value;
                int newMinY = (int)levelMinY.Value;
                int xOff = newMinX - cur_level.minX;
                int yOff = newMinY - cur_level.minY;
                cur_level.minX = newMinX;
                cur_level.minY = newMinY;
                cur_level.maxX += xOff;
                cur_level.maxY += yOff;
                for (int i = 0; i < cur_level.players.Length; i++)
                {
                    BoB_Position newPos = new BoB_Position();
                    newPos.x = cur_level.players[i].x + xOff;
                    newPos.y = cur_level.players[i].y + yOff;
                    cur_level.players[i] = newPos;
                }
                for (int i = 0; i < cur_level.enemies.Length; i++)
                {
                    BoB_Position newPos = new BoB_Position();
                    newPos.x = cur_level.enemies[i].x + xOff;
                    newPos.y = cur_level.enemies[i].y + yOff;
                    cur_level.enemies[i] = newPos;
                }
                p1x.Value = cur_level.players[0].x;
                p1y.Value = cur_level.players[0].y;
                p2x.Value = cur_level.players[1].x;
                p2y.Value = cur_level.players[1].y;
                p3x.Value = cur_level.players[2].x;
                p3y.Value = cur_level.players[2].y;
                p4x.Value = cur_level.players[3].x;
                p4y.Value = cur_level.players[3].y;

                if (EnemyList.SelectedIndex > -1)
                {
                    ex.Value = cur_level.enemies[EnemyList.SelectedIndex].x;
                    ey.Value = cur_level.enemies[EnemyList.SelectedIndex].y;
                }
                AutosaveLevel();
            }
            catch { }
            loading = false;
            UpdateLevelImage(sender, e);
        }
        private void UpdatePlayerPositions(object sender, EventArgs e)
        {
            if (loading || !cur_level.parse_succeeded) return;
            loading = true;
            try
            {
                AddLevelHistory();
                BoB_Position p1 = new BoB_Position()
                {
                    x = (int)p1x.Value,
                    y = (int)p1y.Value
                };
                BoB_Position p2 = new BoB_Position()
                {
                    x = (int)p2x.Value,
                    y = (int)p2y.Value
                };
                BoB_Position p3 = new BoB_Position()
                {
                    x = (int)p3x.Value,
                    y = (int)p3y.Value
                };
                BoB_Position p4 = new BoB_Position()
                {
                    x = (int)p4x.Value,
                    y = (int)p4y.Value
                };
                cur_level.players = new BoB_Position[4] { p1, p2, p3, p4 };
                AutosaveLevel();
            }
            catch { }
            loading = false;
            UpdateLevelImage(sender, e);
        }

        private void t_ID_ValueChanged(object sender, EventArgs e)
        {
            if (loading) return;
            int id = (int)t_ID.Value;
            loading = true;
            try
            {
                t_Sprite.BackgroundImage = tileSprites[id];
                t_Name.SelectedIndex = id - 1;
            }
            catch
            {
                t_Sprite.BackgroundImage = null;
                t_Name.SelectedIndex = -1;
            }
            loading = false;
        }
        private void t_Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading) return;
            int id = t_Name.SelectedIndex + 1;
            loading = true;
            try
            {
                t_Sprite.BackgroundImage = tileSprites[id];
                t_ID.Value = id;
            }
            catch
            {
                t_Sprite.BackgroundImage = null;
                t_ID.Value = t_ID.Minimum;
            }
            loading = false;
        }

        // ==========================================
        // ============== ENEMIES ===================
        // ==========================================

        private void e_Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading) return;
            int id = characterNames.FirstOrDefault(x => x.Value == e_Name.Text).Key;
            e_Type.Text = id.ToString();
            Level l = cur_level;
            l.enemy_type = id;
            cur_level = l;
            UpdateEnemyData(sender, e);
            RefreshEnemyList();
        }
        private void e_Type_ValueChanged(object sender, EventArgs e)
        {
            if (loading) return;
            loading = true;
            try
            {
                int id = (int)e_Type.Value;
                if (characterNames.ContainsKey(id))
                {
                    e_Name.Text = characterNames[id];
                }
                else e_Name.SelectedIndex = -1;
                Level l = cur_level;
                l.enemy_type = id;
                cur_level = l;
            }
            catch { e_Name.SelectedIndex = -1; }
            loading = false;
            UpdateEnemyData(sender, e);
            RefreshEnemyList();
        }
        private void UpdateEnemyData(object sender, EventArgs e)
        {
            if (loading) return;
            loading = true;
            if (EnemyList.SelectedIndex > -1)
            {
                AddLevelHistory();
                BoB_Position e_pos = new BoB_Position()
                {
                    x = (int)ex.Value,
                    y = (int)ey.Value
                };
                Level l = cur_level;
                l.enemies[EnemyList.SelectedIndex] = e_pos;
                cur_level = l;
            }
            loading = false;
            AutosaveLevel();
            UpdateLevelImage(sender, e);
        }
        private void EnemyList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EnemyList.SelectedIndex == -1 || loading || !cur_level.parse_succeeded) return;
            loading = true;
            try
            {
                ex.Value = cur_level.enemies[EnemyList.SelectedIndex].x;
                ey.Value = cur_level.enemies[EnemyList.SelectedIndex].y;
            }
            catch
            {
                ex.Value = 0;
                ey.Value = 0;
            }
            loading = false;
        }
        private void RefreshEnemyList()
        {
            if (!cur_level.parse_succeeded) return;
            string charaName = "Unknown";
            int prevIndex = EnemyList.SelectedIndex;
            try
            {
                int id = cur_level.enemy_type;
                if (e_Name.SelectedIndex >= 0)
                {
                    charaName = characterNames[id];
                }
            }
            catch { }
            EnemyList.Items.Clear();
            for (int i = 0; i < cur_level.enemies.Length; i++)
            {
                EnemyList.Items.Add($"{charaName} {i + 1}");
            }

            if (prevIndex <= 0)
            {
                prevIndex = 0;
            }
            else if (prevIndex >= EnemyList.Items.Count)
            {
                prevIndex = EnemyList.Items.Count - 1;
            }
            UpdateLevelImage(LevelMap, new EventArgs());
        }
        private void AddEnemy_Click(object sender, EventArgs e)
        {
            if (!cur_level.parse_succeeded) return;
            if (cur_level.enemies.Length < maxEnemyCount || maxEnemyCount <= 0)
            {
                AddLevelHistory();
                BoB_Position pos = new BoB_Position()
                { x = 0, y = 0 };
                List<BoB_Position> enemy_positions = cur_level.enemies.ToList();
                enemy_positions.Add(pos);
                cur_level.enemies = enemy_positions.ToArray();
                AutosaveLevel();
                RefreshEnemyList();
            }
        }
        private void DeleteEnemy_Click(object sender, EventArgs e)
        {
            if (!cur_level.parse_succeeded) return;
            if (cur_level.enemies.Length > 1)
            {
                AddLevelHistory();
                List<BoB_Position> enemy_positions = cur_level.enemies.ToList();
                int index = EnemyList.Items.Count - 1;
                if (EnemyList.SelectedIndex >= 0)
                {
                    index = EnemyList.SelectedIndex;
                }
                enemy_positions.RemoveAt(index);
                cur_level.enemies = enemy_positions.ToArray();
                AutosaveLevel();
            }
            RefreshEnemyList();
        }

        // ==========================================
        // ============= LEVEL MAP ==================
        // ==========================================
        private void levelSelectionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading || levelSelectionBox.SelectedIndex < 0 || allLevels.Count <= 0) return;
            loading = true;
            try
            {
                cur_level = allLevels[levelSelectionBox.SelectedIndex];
                levelHistory = new List<Level>();
                LoadLevelData(false);
            }
            catch { }
            loading = false;
        }
        private void removeLevel_Click(object sender, EventArgs e)
        {
            if (allLevels.Count <= 0) return;
            if (ModifierKeys == Keys.Control)
            {
                RemoveLevel();
            }
            else if (MessageBox.Show("Are you sure?", "Delete Level", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                RemoveLevel();
            }
        }
        private void UpdateLevelHeights()
        {
            lowestHeight = 0;
            highestHeight = 0;
            bool setFirstHeight = false;
            for (int i = 0; i < cur_level.tiles.Length; i++)
            {
                BoB_Tile t = cur_level.tiles[i];
                if (t.id > 1)
                {
                    if (!setFirstHeight)
                    {
                        setFirstHeight = true;
                        lowestHeight = t.h;
                        highestHeight = t.h;
                    }
                    decimal newH = 0;
                    try
                    {
                        newH = t.h;
                    }
                    catch { }
                    if (newH > highestHeight) highestHeight = newH;
                    else if (newH < lowestHeight) lowestHeight = newH;
                }
            }
        }
        private void UpdateLevelImage(object sender, EventArgs e)
        {
            int width = cur_level.maxX - cur_level.minX;
            int height = cur_level.maxY - cur_level.minY;
            if (width <= 0 || height <= 0 || !cur_level.parse_succeeded || loading) return;
            LevelMap.BackgroundImage = LevelImage();
        }
        private void LevelMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
            {
                ToolAction(false, false);
            }
            else
            {
                ToolAction(true, false);
            }
            UpdateLevelImage(sender, e);
        }
        private void LevelMap_MouseUp(object sender, MouseEventArgs e)
        {
            ToolDeactivate();
            UpdateLevelImage(sender, e);
        }
        private void LevelMap_MouseLeave(object sender, EventArgs e)
        {
            ToolDeactivate();
            tileX = -1;
            tileY = -1;
            UpdateLevelImage(sender, e);
        }
        private void ToolDeactivate()
        {
            toolActing = false;
            if (selectingX != -1 && selectingY != -1)
            {
                int selectMinX = Math.Min(tileX, selectingX);
                int selectMinY = Math.Min(tileY, selectingY);
                int selectMaxX = Math.Max(tileX, selectingX);
                int selectMaxY = Math.Max(tileY, selectingY);
                MakeSelection(selectMinX, selectMinY, selectMaxX, selectMaxY);
                selectingX = -1;
                selectingY = -1;
            }
            draggingSelectionOffsetX = null;
            draggingSelectionOffsetY = null;
        }
        private void AddLevelHistory()
        {
            Level l = new Level();
            l.parse_succeeded = cur_level.parse_succeeded;
            l.minX = cur_level.minX;
            l.maxX = cur_level.maxX;
            l.minY = cur_level.minY;
            l.maxY = cur_level.maxY;
            l.players = new BoB_Position[cur_level.players.Length];
            l.enemy_type = cur_level.enemy_type;
            l.enemies = new BoB_Position[cur_level.enemies.Length];
            l.tiles = new BoB_Tile[cur_level.tiles.Length];
            for (int i = 0; i < l.players.Length; i++)
            {
                l.players[i] = new BoB_Position() { x = cur_level.players[i].x, y = cur_level.players[i].y };
            }
            for (int i = 0; i < l.enemies.Length; i++)
            {
                l.enemies[i] = new BoB_Position() { x = cur_level.enemies[i].x, y = cur_level.enemies[i].y };
            }
            for (int i = 0; i < l.tiles.Length; i++)
            {
                l.tiles[i] = new BoB_Tile()
                {
                    id = cur_level.tiles[i].id,
                    h = cur_level.tiles[i].h,
                    col = cur_level.tiles[i].col
                };
            }
            levelHistory.Add(l);
            if (levelHistory.Count > 255)
            {
                levelHistory.RemoveAt(0);
            }
        }
        private void Undo(object sender, EventArgs e)
        {
            floatingSelectionX = -1;
            floatingSelectionY = -1;
            floatingSelectionW = -1;
            floatingSelectionD = -1;
            floatingSelection = null;
            if (levelHistory.Count < 1)
            {
                Debug.WriteLine("No further level history!");
                return;
            }
            cur_level = levelHistory[levelHistory.Count - 1];
            levelHistory.RemoveAt(levelHistory.Count - 1);
            LoadLevelData(true);
        }
        private void Redo(object sender, EventArgs e)
        {

        }
        private void LevelMap_MouseMove(object sender, MouseEventArgs e)
        {
            int prevX = tileX;
            int prevY = tileY;
            double tileScale;
            int width = cur_level.maxX - cur_level.minX;
            int height = cur_level.maxY - cur_level.minY;
            if (width == 0 || height == 0) return;
            double levelAspect = width / (double)height;
            double canvasAspect = LevelMap.Width / (double)LevelMap.Height;
            if (canvasAspect >= levelAspect)
            {
                tileScale = LevelMap.Height / (double)height;
                double x_offset = (LevelMap.Width - Math.Floor(tileScale * width)) / 2;
                tileX = (int)Math.Floor((e.X - x_offset) / tileScale);
                tileY = (int)Math.Floor(e.Y / tileScale);
            }
            else
            {
                tileScale = LevelMap.Width / (double)width;
                double y_offset = (LevelMap.Height - Math.Floor(tileScale * height)) / 2;
                tileX = (int)Math.Floor(e.X / tileScale);
                tileY = (int)Math.Floor((e.Y - y_offset) / tileScale);
            }
            tileX = Math.Clamp(tileX, 0, width - 1);
            tileY = Math.Clamp(tileY, 0, height - 1);
            if (tileX != prevX || tileY != prevY)
            {
                if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
                {
                    ToolAction(false, true);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    ToolAction(true, true);
                }
                UpdateLevelImage(sender, e);
            }
        }

        private Bitmap LevelImage()
        {
            int width = cur_level.maxX - cur_level.minX;
            int height = cur_level.maxY - cur_level.minY;
            int selectMinX = Math.Min(tileX, selectingX);
            int selectMinY = Math.Min(tileY, selectingY);
            int selectMaxX = Math.Max(tileX, selectingX);
            int selectMaxY = Math.Max(tileY, selectingY);

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    BoB_Tile t = cur_level.tiles[(y * width) + x];
                    Color pixCol = TileColor(t);
                    if (floatingSelection != null)
                    {
                        if (x >= floatingSelectionX && x <= floatingSelectionX + floatingSelectionW)
                        {
                            if (y >= floatingSelectionY && y <= floatingSelectionY + floatingSelectionD)
                            {
                                int index = ((y - floatingSelectionY) * floatingSelectionW) + (y - floatingSelectionY) + (x - floatingSelectionX);
                                if (index >= 0 && index < floatingSelection.Length)
                                {
                                    BoB_Tile t2 = floatingSelection[index];
                                    if (t2.id > 1)
                                    {
                                        Color col1 = TileColor(t2);
                                        Color col2 = HighlightColor();
                                        int r = Math.Min((int)Math.Round((col1.R * 0.8) + (col2.R * 0.2)), 255);
                                        int g = Math.Min((int)Math.Round((col1.G * 0.8) + (col2.G * 0.2)), 255);
                                        int b = Math.Min((int)Math.Round((col1.B * 0.8) + (col2.B * 0.2)), 255);
                                        pixCol = Color.FromArgb(r, g, b);
                                    }
                                }
                            }
                        }
                    }
                    else if (selectingX != -1 && selectingY != -1)
                    {
                        // Highlight selection range
                        if (x >= selectMinX && x <= selectMaxX)
                        {
                            if (y >= selectMinY && y <= selectMaxY)
                            {
                                Color col1 = TileColor(t);
                                Color col2 = HighlightColor();
                                int r = Math.Min((int)Math.Round((col1.R * 0.8) + (col2.R * 0.2)), 255);
                                int g = Math.Min((int)Math.Round((col1.G * 0.8) + (col2.G * 0.2)), 255);
                                int b = Math.Min((int)Math.Round((col1.B * 0.8) + (col2.B * 0.2)), 255);
                                pixCol = Color.FromArgb(r, g, b);
                            }
                        }
                    }
                    if (x == tileX && y == tileY && !banCursor)
                    {
                        pixCol = HighlightColor();
                    }
                    bmp.SetPixel(x, y, pixCol);
                }
            }
            if (showEntitiesToolStripMenuItem.Checked && ((viewType != 3 && viewType != 0) || (viewType == 0 && toolType != "elevation")))
            {
                for (int i = 0; i < 4; i++)
                {
                    Color playerCol = playerColors[i];
                    BoB_Position p_pos = cur_level.players[i];
                    int relativeX = p_pos.x - cur_level.minX;
                    int relativeY = p_pos.y - cur_level.minY;
                    if (relativeX >= 0 && relativeX < width && relativeY >= 0 && relativeY < height)
                    {
                        bmp.SetPixel(relativeX, relativeY, playerCol);
                    }
                }
                foreach (BoB_Position e_pos in cur_level.enemies)
                {
                    int relativeX = e_pos.x - cur_level.minX;
                    int relativeY = e_pos.y - cur_level.minY;
                    if (relativeX >= 0 && relativeX < width && relativeY >= 0 && relativeY < height)
                    {
                        bmp.SetPixel(relativeX, relativeY, enemyColor);
                    }
                }
            }
            recentShot = bmp;

            // Scale up
            Bitmap bmp2;
            Graphics gph = Graphics.FromImage(bmp2 = new Bitmap(LevelMap.Width, LevelMap.Height));
            gph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            gph.PixelOffsetMode = PixelOffsetMode.Half;

            double tileScale;
            double levelAspect = width / (double)height;
            double canvasAspect = LevelMap.Width / (double)LevelMap.Height;
            int x_offset = 0;
            int x_scale = LevelMap.Width;
            int y_offset = 0;
            int y_scale = LevelMap.Height;
            if (canvasAspect >= levelAspect)
            {
                tileScale = LevelMap.Height / (double)height;
                x_scale = (int)Math.Round(tileScale * width);
                x_offset = (LevelMap.Width - x_scale) / 2;
            }
            else
            {
                tileScale = LevelMap.Width / (double)width;
                y_scale = (int)Math.Round(tileScale * height);
                y_offset = (LevelMap.Height - y_scale) / 2;
            }
            gph.DrawImage(bmp, x_offset, y_offset, x_scale, y_scale);
            gph.Dispose();
            return bmp2;
        }

        private void ToolAction(bool rightClickVer, bool mouseDragVer)
        {
            if (tileX != -1 && tileY != -1)
            {
                int width = cur_level.maxX - cur_level.minX;
                int i = (tileY * width) + tileX;
                BoB_Tile t = cur_level.tiles[i];

                if (!mouseDragVer && !rightClickVer && toolType != "select")
                {
                    // Initial Left CLick
                    affectedTileStrings = new Dictionary<int, string>();
                    toolActing = true;
                    AddLevelHistory();
                }
                if (rightClickVer)
                {
                    // Right Click
                    t_Collision.Checked = true;
                    t_ID.Value = t.id;
                    t_Height.Text = t.h.ToString();
                    t_Collision.Checked = true;
                    if (t.col == 0)
                    {
                        t_Collision.Checked = false;
                    }
                }
                else if (!affectedTileStrings.ContainsKey(i) || toolType == "select")
                {
                    // Left Click
                    if (toolType != "select") affectedTileStrings.Add(i, $"{i},{t.id},{t.h},{t.col}");
                    switch (toolType)
                    {
                        case "paint":
                            // Tile ID
                            uint tileID = (uint)t_ID.Value;
                            t.id = tileID;
                            if (ModifierKeys == Keys.Control && !mouseDragVer)
                            {
                                // Replace all tiles with the target's ID with the new tile
                                uint targetID = cur_level.tiles[i].id;
                                for (int a = 0; a < cur_level.tiles.Length; a++)
                                {
                                    if (cur_level.tiles[a].id == targetID)
                                    {
                                        BoB_Tile recoloredTile = cur_level.tiles[a];
                                        recoloredTile.id = tileID;
                                        cur_level.tiles[a] = recoloredTile;
                                    }
                                }
                            }
                            else if (ModifierKeys != Keys.Shift)
                            {
                                // Edit more than Tile ID if Shift isn't held.
                                if (t.id > 1)
                                {
                                    decimal paintTestHeight = 0;
                                    if (decimal.TryParse(t_Height.Text, out paintTestHeight))
                                    {
                                        t.h = paintTestHeight;
                                        UpdateLevelHeights();
                                    }
                                }
                                t.col = 0;
                                if (t_Collision.Checked)
                                {
                                    t.col = 1;
                                }
                                cur_level.tiles[i] = t;
                            }
                            else
                            {
                                cur_level.tiles[i] = t;
                            }
                            break;
                        case "erase":
                            t.id = 1;
                            t.col = 0;
                            t.h = 0;
                            if (ModifierKeys == Keys.Control && !mouseDragVer)
                            {
                                // Replace all tiles with the target's ID with the new tile
                                uint targetID = cur_level.tiles[i].id;
                                for (int a = 0; a < cur_level.tiles.Length; a++)
                                {
                                    if (cur_level.tiles[a].id == targetID)
                                    {
                                        cur_level.tiles[a] = t;
                                    }
                                }
                            }
                            else
                            {
                                cur_level.tiles[i] = t;
                            }
                            break;
                        case "collision":
                            t.col = 0;
                            if (t_Collision.Checked)
                            {
                                t.col = 1;
                            }
                            cur_level.tiles[i] = t;
                            break;
                        case "elevation":
                            decimal elevateHeight = 0;
                            if (ModifierKeys == Keys.Control)
                            {
                                // Just Set Height
                                if (decimal.TryParse(t_Height.Text, out elevateHeight))
                                {
                                    t.h = elevateHeight;
                                    cur_level.tiles[i] = t;
                                }
                            }
                            else
                            {
                                // Elevate in an increment
                                decimal elevateIncrement = 0;
                                if (decimal.TryParse(elev_increment.Text, out elevateIncrement))
                                {
                                    if (ModifierKeys == Keys.Shift)
                                    {
                                        // Add negative increment
                                        t.h -= elevateIncrement;
                                    }
                                    else
                                    {
                                        t.h += elevateIncrement;
                                    }
                                    cur_level.tiles[i] = t;
                                }
                            }
                            UpdateLevelHeights();
                            break;
                        case "select":
                            if (!mouseDragVer)
                            {
                                if (floatingSelection == null)
                                {
                                    selectingX = tileX;
                                    selectingY = tileY;
                                }
                                else
                                {
                                    if (IsPointWithinTileSelection(tileX, tileY))
                                    {
                                        draggingSelectionOffsetX = floatingSelectionX - tileX;
                                        draggingSelectionOffsetY = floatingSelectionY - tileY;
                                    }
                                    else PlaceSelection();
                                }
                            }
                            else if (draggingSelectionOffsetX != null && draggingSelectionOffsetY != null)
                            {
                                floatingSelectionX = tileX + (int)draggingSelectionOffsetX;
                                floatingSelectionY = tileY + (int)draggingSelectionOffsetY;
                            }
                            break;
                    }
                }
            }
        }
        private Color HighlightColor()
        {
            if ((viewType == 0 && toolType == "elevation") || viewType == 3)
            {
                return Color.White;
            }
            Theme t = windowThemes[ThemeSelectionBox.SelectedIndex];
            return t.highlight;
        }
        private Color TileColor(BoB_Tile t)
        {
            if ((viewType == 0 && toolType == "collision") || viewType == 2)
            {
                if (t.id > 1)
                {
                    if (t.id < tileCollision.Length)
                    {
                        if (tileCollision[t.id])
                        {
                            return Color.DimGray;
                        }
                        else if (t.col != 0)
                        {
                            return Color.FromArgb(175, 175, 175);
                        }
                        return Color.White;
                    }
                    else
                    {
                        if (t.col != 0)
                        {
                            return Color.FromArgb(175, 175, 175);
                        }
                        return Color.White;
                    }
                }
                return Color.Black;
            }
            else if ((viewType == 0 && toolType == "elevation") || viewType == 3)
            {
                if (t.id > 1)
                {
                    Color heightMap = ColorFromHSV(120, 0.5, 0.75);
                    float heightRange = (float)(highestHeight - lowestHeight);
                    if (heightRange <= 0)
                    {
                        return heightMap;
                    }
                    float heightAboveLow = (float)(t.h - lowestHeight);
                    float relativeHeight = heightAboveLow / heightRange;
                    double hue = 120f + (240f * (relativeHeight - 0.5f));
                    double sat = 0.5f + Math.Abs(relativeHeight - 0.5f); // gets less saturated the closer to the center it becomes.
                    double val = 0.75f + (0.5 * Math.Abs(relativeHeight - 0.5f)); // gets slightly darker the closer to the center it becomes.
                    return ColorFromHSV(hue, sat, val);
                }
                return Color.Black;
            }
            else if (t.id >= 0 && t.id < tileColors.Length)
            {
                return tileColors[t.id];
            }
            else return Color.Magenta;
        }
        public static Color ColorFromStringOrRGB(string input)
        {
            Color color = Color.Magenta;
            try
            {
                string[] data = input.Split(",", StringSplitOptions.TrimEntries);
                int r = int.Parse(data[0]);
                int g = int.Parse(data[1]);
                int b = int.Parse(data[2]);
                color = Color.FromArgb(r, g, b);
            }
            catch
            {
                try
                {
                    color = Color.FromName(input);
                }
                catch { }
            }
            return color;
        }
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
            switch (hi)
            {
                case 0:
                    return Color.FromArgb(255, v, t, p);
                case 1:
                    return Color.FromArgb(255, q, v, p);
                case 2:
                    return Color.FromArgb(255, p, v, t);
                case 3:
                    return Color.FromArgb(255, p, q, v);
                case 4:
                    return Color.FromArgb(255, t, p, v);
                default:
                    return Color.FromArgb(255, v, p, q);
            }
        }

        private void ThemeSelectionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ThemeSelectionBox.SelectedIndex == -1) return;
            Theme t = windowThemes[ThemeSelectionBox.SelectedIndex];
            try
            {
                config.OverwriteData("Miscellaneous", "Previous Theme Index", ThemeSelectionBox.SelectedIndex.ToString());
            }
            catch { }
            UpdateTheme(t);
            SelectTool(0);
        }
        private void MakeSelection(int minX, int minY, int maxX, int maxY)
        {
            AddLevelHistory();
            floatingSelection = null;
            int nonAirTileCount = 0;
            List<BoB_Tile> floatingTiles = new List<BoB_Tile>();
            int width = cur_level.maxX - cur_level.minX;
            int depth = cur_level.maxY - cur_level.minY;
            for (int y = 0; y < depth; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                    {
                        BoB_Tile t = cur_level.tiles[(y * width) + x];
                        BoB_Tile t2 = new BoB_Tile()
                        {
                            id = t.id,
                            h = t.h,
                            col = t.col
                        };
                        if (t2.id > 1) nonAirTileCount++;
                        floatingTiles.Add(t2);
                        Debug.WriteLine($"Adding Tile To Selection, X: {x}, Y: {y}, ID: {t2.id}");
                        t.id = 1;
                        t.h = 0;
                        t.col = 0;
                        cur_level.tiles[(y * width) + x] = t;
                    }
                }
            }
            if (nonAirTileCount > 0)
            {
                floatingSelectionX = minX;
                floatingSelectionY = minY;
                floatingSelectionW = maxX - minX;
                floatingSelectionD = maxY - minY;
                floatingSelection = floatingTiles.ToArray();
            }
        }
        private void PlaceSelection()
        {
            AddLevelHistory();
            if (floatingSelection == null) return;
            for (int d = 0; d <= floatingSelectionD; d++)
            {
                for (int w = 0; w <= floatingSelectionW; w++)
                {
                    int width = cur_level.maxX - cur_level.minX;
                    int depth = cur_level.maxY - cur_level.minY;
                    if (w + floatingSelectionX >= 0 && w + floatingSelectionX < width)
                    {
                        if (d + floatingSelectionY >= 0 && d + floatingSelectionY < depth)
                        {
                            BoB_Tile t = floatingSelection[(d * floatingSelectionW) + d + w];
                            if (t.id > 1)
                            {
                                int targetIndex = ((floatingSelectionY + d) * width) + (floatingSelectionX + w);
                                if (targetIndex >= 0 && targetIndex < cur_level.tiles.Length)
                                {
                                    cur_level.tiles[targetIndex] = t;
                                }
                            }
                        }
                    }
                }
            }
            floatingSelectionX = -1;
            floatingSelectionY = -1;
            floatingSelectionW = -1;
            floatingSelectionD = -1;
            floatingSelection = null;
        }
        private void CopySelection(object sender, EventArgs e)
        {
            if (floatingSelection == null) return;
            string output = $"{floatingSelectionW},{floatingSelectionD},";
            for (int i = 0; i < floatingSelection.Length; i++)
            {
                BoB_Tile t = floatingSelection[i];
                output += $"{t.id},{t.h},{t.col}";
                if (i < floatingSelection.Length - 1) output += ",";
            }
            Clipboard.SetText(output);
        }
        private void PasteSelection(object sender, EventArgs e)
        {
            string input = Clipboard.GetText();
            floatingSelectionX = -1;
            floatingSelectionY = -1;
            floatingSelectionW = -1;
            floatingSelectionD = -1;
            floatingSelection = null;
            SelectTool(3);
            try
            {
                string[] data = input.Split(",", StringSplitOptions.TrimEntries);
                List<BoB_Tile> tileList = new List<BoB_Tile>();
                floatingSelectionX = 0;
                floatingSelectionY = 0;
                floatingSelectionW = int.Parse(data[0]);
                floatingSelectionD = int.Parse(data[1]);
                for (int i = 0; i < (data.Length - 2) / 3; i++)
                {
                    BoB_Tile t = new BoB_Tile()
                    {
                        id = uint.Parse(data[(i * 3) + 2]),
                        h = decimal.Parse(data[(i * 3) + 3]),
                        col = uint.Parse(data[(i * 3) + 4])
                    };
                    tileList.Add(t);
                }
                floatingSelection = tileList.ToArray();
            }
            catch
            {
                Debug.WriteLine("Clipboard contained invalid Tile Data...");
            }
        }
        private void DeleteSelection(object sender, EventArgs e)
        {
            floatingSelectionX = -1;
            floatingSelectionY = -1;
            floatingSelectionW = -1;
            floatingSelectionD = -1;
            floatingSelection = null;
        }
        bool IsPointWithinTileSelection(int x, int y)
        {
            if (x >= floatingSelectionX && x <= floatingSelectionX + floatingSelectionW)
            {
                if (y >= floatingSelectionY && y <= floatingSelectionY + floatingSelectionD)
                {
                    return true;
                }
            }
            return false;
        }

        private void CutSelection(object sender, EventArgs e)
        {
            CopySelection(sender, e);
            DeleteSelection(sender, e);
        }
        public struct Theme
        {
            public Color bg;
            public Color level_bg;
            public Color ui;
            public Color btn;
            public Color bg_text;
            public Color ui_text;
            public Color btn_text;
            public Color tool;
            public Color tool_pressed;
            public Color highlight;
        }
    }
}