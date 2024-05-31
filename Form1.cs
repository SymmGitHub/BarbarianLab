using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Xml;
using BarbarianLab.Utilities;
using static BoB_LevelData;

namespace BarbarianLab
{
    public partial class Form1 : Form
    {
        Level level = new Level();
        Bitmap recentShot;
        Config config = new Config();
        Dictionary<int, string> characterNames = new Dictionary<int, string>();
        Dictionary<int, BoB_Tile> affectedTiles = new Dictionary<int, BoB_Tile>();
        List<Level> levelHistory = new List<Level>();
        Color[] tileColors;
        Image[] tileSprites;
        bool[] tileCollision;
        bool[] tileFluctuation;
        bool loading = false;
        public int tileX = -1;
        public int tileY = -1;
        public decimal lowestHeight;
        public decimal highestHeight;
        string toolType = "";
        int viewType = 0;
        bool banCursor = false;
        bool toolActing = false;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            config.ParseConfig(AppDomain.CurrentDomain.BaseDirectory + "\\Config.txt");
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
            t_ID.Value = 2;
        }

        // ==========================================
        // ============== TOOLSTRIP =================
        // ==========================================
        private void openLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLevel open = new OpenLevel();
            if (open.ShowDialog() == DialogResult.OK)
            {
                if (loading) return;
                level = open.level;
                affectedTiles = new Dictionary<int, BoB_Tile>();
                levelHistory = new List<Level>();
                levelHistory.Add(level);
                LoadLevelData();
            }
        }
        private void copyLevelArrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText($"new Array({BoB_LevelData.ArrayString(level)});");
        }
        private void LoadLevelData()
        {
            loading = true;
            levelWidth.Value = (level.maxX - level.minX);
            levelHeight.Value = (level.maxY - level.minY);
            levelMinX.Value = level.minX;
            levelMinY.Value = level.minY;
            p1x.Value = level.players[0].x;
            p1y.Value = level.players[0].y;
            p2x.Value = level.players[1].x;
            p2y.Value = level.players[1].y;
            p3x.Value = level.players[2].x;
            p3y.Value = level.players[2].y;
            p4x.Value = level.players[3].x;
            p4y.Value = level.players[3].y;
            if (level.enemy_type >= 2)
            {
                e_Name.SelectedIndex = level.enemy_type - 2;
            }
            e_Type.Text = level.enemy_type.ToString();
            if (characterNames.ContainsKey(level.enemy_type))
            {
                e_Name.Text = characterNames[level.enemy_type];
            }
            UpdateEnemyList();
            UpdateLevelHeights();
            UpdateLevelImage(LevelMap, new EventArgs());
            loading = false;

            if (EnemyList.Items.Count > 0)
            {
                EnemyList.SelectedIndex = 0;
            }
        }
        private void SelectTool(uint type)
        {
            toolType = "";
            tipText.Text = "";
            ToolPaint.BackColor = Color.FromArgb(200, 180, 200);
            ToolPaint.Enabled = true;
            ToolErase.BackColor = Color.FromArgb(200, 180, 200);
            ToolErase.Enabled = true;
            ToolCollision.BackColor = Color.FromArgb(200, 180, 200);
            ToolCollision.Enabled = true;
            ToolElevate.BackColor = Color.FromArgb(200, 180, 200);
            ToolElevate.Enabled = true;
            switch (type)
            {
                case 1:
                    toolType = "paint";
                    ToolPaint.BackColor = Color.FromArgb(255, 255, 192);
                    ToolPaint.Enabled = false;
                    tipText.Text = "Left Click: Paint  |  Shift + Left-Click: Paint (ID Only)" + Environment.NewLine +
                    "Ctrl + Left-Click: Replace All of an ID  |  Right-Click: Tile Eyedropper Tool";
                    break;
                case 2:
                    toolType = "erase";
                    ToolErase.BackColor = Color.FromArgb(255, 255, 192);
                    ToolErase.Enabled = false;
                    tipText.Text = "Left Click: Erase  |  Ctrl + Left-Click: Erase All of an ID" + Environment.NewLine +
                    "Right-Click: Tile Eyedropper Tool";
                    break;
                case 3:
                    toolType = "collision";
                    ToolCollision.BackColor = Color.FromArgb(255, 255, 192);
                    ToolCollision.Enabled = false;
                    tipText.Text = "Left Click: Edit Collision  |  Right-Click: Tile Eyedropper Tool" + Environment.NewLine +
                    "Darker gray tiles have tile IDs where default collision takes priority.";
                    break;
                case 4:
                    toolType = "elevation";
                    ToolElevate.BackColor = Color.FromArgb(255, 255, 192);
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
        private void collisionToolToolStripMenuItem_Click(object sender, EventArgs e) { SelectTool(3); }
        private void elevationToolToolStripMenuItem_Click(object sender, EventArgs e) { SelectTool(4); }
        private void ToolPaint_Click(object sender, EventArgs e) { paintToolToolStripMenuItem_Click(sender, e); }
        private void ToolErase_Click(object sender, EventArgs e) { eraseToolToolStripMenuItem_Click(sender, e); }
        private void ToolCollision_Click(object sender, EventArgs e) { collisionToolToolStripMenuItem_Click(sender, e); }
        private void ToolElevate_Click(object sender, EventArgs e) { elevationToolToolStripMenuItem_Click(sender, e); }
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
                int width = level.maxX - level.minX;
                int height = level.maxY - level.minY;
                int widthDifference = (int)levelWidth.Value - width;
                int heightDifference = (int)levelHeight.Value - height;
                List<BoB_Tile> tileList = level.tiles.ToList();
                BoB_Tile t = new BoB_Tile();
                t.id = 1;
                t.h = 0;
                t.col = 0;

                // Update Width
                if (widthDifference > 0)
                {
                    for (int w = 0; w < widthDifference; w++)
                    {
                        for (int h = 0; h < height; h++)
                        {
                            tileList.Insert((h * width) + width + h, t);
                        }
                        level.maxX++;
                        width = level.maxX - level.minX;
                    }
                }
                else if (widthDifference < 0)
                {
                    for (int w = 0; w < -widthDifference; w++)
                    {
                        for (int h = 0; h < height; h++)
                        {
                            tileList.RemoveAt(((h * width) + width) - (1 + h));
                        }
                        level.maxX--;
                        width = level.maxX - level.minX;
                    }
                }

                // Update Height 
                if (heightDifference > 0)
                {
                    for (int h = 0; h < heightDifference; h++)
                    {
                        for (int w = 0; w < width; w++)
                        {
                            tileList.Add(t);
                        }
                        level.maxY++;
                        height = level.maxY - level.minY;
                    }
                }
                else if (heightDifference < 0)
                {
                    for (int h = 0; h < -heightDifference; h++)
                    {
                        tileList.RemoveRange(((height - 1) * width), width);
                        level.maxY--;
                        height = level.maxY - level.minY;
                    }
                }
                level.tiles = tileList.ToArray();
            }
            catch { }
            loading = false;
            UpdateLevelImage(sender, e);
        }
        private void UpdateLevelMinValues(object sender, EventArgs e)
        {
            if (loading || !level.parse_succeeded) return;
            loading = true;
            try
            {
                int newMinX = (int)levelMinX.Value;
                int newMinY = (int)levelMinY.Value;
                int xOff = newMinX - level.minX;
                int yOff = newMinY - level.minY;
                level.minX = newMinX;
                level.minY = newMinY;
                level.maxX += xOff;
                level.maxY += yOff;
                for (int i = 0; i < level.players.Length; i++)
                {
                    BoB_Position newPos = new BoB_Position();
                    newPos.x = level.players[i].x + xOff;
                    newPos.y = level.players[i].y + yOff;
                    level.players[i] = newPos;
                }
                for (int i = 0; i < level.enemies.Length; i++)
                {
                    BoB_Position newPos = new BoB_Position();
                    newPos.x = level.enemies[i].x + xOff;
                    newPos.y = level.enemies[i].y + yOff;
                    level.enemies[i] = newPos;
                }
                p1x.Value = level.players[0].x;
                p1y.Value = level.players[0].y;
                p2x.Value = level.players[1].x;
                p2y.Value = level.players[1].y;
                p3x.Value = level.players[2].x;
                p3y.Value = level.players[2].y;
                p4x.Value = level.players[3].x;
                p4y.Value = level.players[3].y;

                if (EnemyList.SelectedIndex > -1)
                {
                    ex.Value = level.enemies[EnemyList.SelectedIndex].x;
                    ey.Value = level.enemies[EnemyList.SelectedIndex].y;
                }
            }
            catch { }
            loading = false;
            UpdateLevelImage(sender, e);
        }
        private void UpdatePlayerPositions(object sender, EventArgs e)
        {
            if (loading || !level.parse_succeeded) return;
            loading = true;
            try
            {
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
                level.players = new BoB_Position[4] { p1, p2, p3, p4 };
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
            loading = true;
            try
            {
                int id = characterNames.FirstOrDefault(x => x.Value == e_Name.Text).Key;
                e_Type.Text = id.ToString();
                Level l = level;
                l.enemy_type = id;
                level = l;
            }
            catch { e_Type.Text = ""; }
            loading = false;
            UpdateEnemyData(sender, e);
            UpdateEnemyList();
        }
        private void e_Type_TextChanged(object sender, EventArgs e)
        {
            if (loading) return;
            loading = true;
            try
            {
                int id = int.Parse(e_Type.Text);
                if (characterNames.ContainsKey(id))
                {
                    e_Name.Text = characterNames[id];
                }
                else e_Name.SelectedIndex = -1;
                Level l = level;
                l.enemy_type = id;
                level = l;
            }
            catch { e_Name.SelectedIndex = -1; }
            loading = false;
            UpdateEnemyData(sender, e);
            UpdateEnemyList();
        }
        private void UpdateEnemyData(object sender, EventArgs e)
        {
            if (loading || EnemyList.SelectedIndex < 0) return;
            loading = true;
            try
            {
                BoB_Position e_pos = new BoB_Position()
                {
                    x = (int)ex.Value,
                    y = (int)ey.Value
                };
                Level l = level;
                l.enemies[EnemyList.SelectedIndex] = e_pos;
                level = l;
            }
            catch { }
            loading = false;
            UpdateLevelImage(sender, e);
        }
        private void EnemyList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EnemyList.SelectedIndex == -1 || loading || !level.parse_succeeded) return;
            loading = true;
            try
            {
                ex.Value = level.enemies[EnemyList.SelectedIndex].x;
                ey.Value = level.enemies[EnemyList.SelectedIndex].y;
            }
            catch
            {
                ex.Value = 0;
                ey.Value = 0;
            }
            loading = false;
        }
        private void UpdateEnemyList()
        {
            string charaName = "Unknown";
            int prevIndex = EnemyList.SelectedIndex;
            try
            {
                int id = level.enemy_type;
                if (e_Name.SelectedIndex >= 0)
                {
                    charaName = characterNames[id];
                }
            }
            catch { }
            EnemyList.Items.Clear();
            for (int i = 0; i < level.enemies.Length; i++)
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
            BoB_Position pos = new BoB_Position()
            { x = 0, y = 0 };
            List<BoB_Position> enemy_positions = level.enemies.ToList();
            enemy_positions.Add(pos);
            level.enemies = enemy_positions.ToArray();
            UpdateEnemyList();
        }
        private void DeleteEnemy_Click(object sender, EventArgs e)
        {
            if (level.enemies.Length > 0)
            {
                List<BoB_Position> enemy_positions = level.enemies.ToList();
                int index = EnemyList.Items.Count - 1;
                if (EnemyList.SelectedIndex >= 0)
                {
                    index = EnemyList.SelectedIndex;
                }
                enemy_positions.RemoveAt(index);
                level.enemies = enemy_positions.ToArray();
            }
            UpdateEnemyList();
        }

        // ==========================================
        // ============= LEVEL MAP ==================
        // ==========================================
        private void UpdateLevelHeights()
        {
            lowestHeight = 0;
            highestHeight = 0;
            for (int i = 0; i < level.tiles.Length; i++)
            {
                BoB_Tile t = level.tiles[i];
                if (t.id > 1)
                {
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
            int width = level.maxX - level.minX;
            int height = level.maxY - level.minY;
            if (width <= 0 || height <= 0 || !level.parse_succeeded) return;
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
            if (toolActing)
            {
                toolActing = false;
                UpdateLevelImage(sender, e);
            }
        }
        private void LevelMap_MouseLeave(object sender, EventArgs e)
        {
            if (toolActing)
            {
                toolActing = false;
            }
            tileX = -1;
            tileY = -1;
            UpdateLevelImage(sender, e);
        }
        private void LevelMap_MouseMove(object sender, MouseEventArgs e)
        {
            int prevX = tileX;
            int prevY = tileY;
            double tileScale;
            int width = level.maxX - level.minX;
            int height = level.maxY - level.minY;
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
            int width = level.maxX - level.minX;
            int height = level.maxY - level.minY;
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    BoB_Tile t = level.tiles[(y * width) + x];
                    Color pixCol = TileColor(t);
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
                    Color playerCol = Color.FromArgb(0, 255, 0); // Green
                    switch (i)
                    {
                        case 1:
                            playerCol = Color.FromArgb(255, 0, 0); // Red
                            break;
                        case 2:
                            playerCol = Color.FromArgb(0, 255, 255); // Blue
                            break;
                        case 3:
                            playerCol = Color.FromArgb(255, 180, 0); // Orange
                            break;
                    }
                    BoB_Position p_pos = level.players[i];
                    int relativeX = p_pos.x - level.minX;
                    int relativeY = p_pos.y - level.minY;
                    if (relativeX >= 0 && relativeX < width && relativeY >= 0 && relativeY < height)
                    {
                        bmp.SetPixel(relativeX, relativeY, playerCol);
                    }
                }
                foreach (BoB_Position e_pos in level.enemies)
                {
                    int relativeX = e_pos.x - level.minX;
                    int relativeY = e_pos.y - level.minY;
                    if (relativeX >= 0 && relativeX < width && relativeY >= 0 && relativeY < height)
                    {
                        bmp.SetPixel(relativeX, relativeY, Color.FromArgb(200, 50, 115)); // Dark Pink
                    }
                }
            }
            recentShot = bmp;

            // Scale up
            Bitmap bmp2;
            Graphics g = Graphics.FromImage(bmp2 = new Bitmap(LevelMap.Width, LevelMap.Height));
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

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
            g.DrawImage(bmp, x_offset, y_offset, x_scale, y_scale);
            g.Dispose();
            return bmp2;
        }

        private void ToolAction(bool rightClickVer, bool mouseDragVer)
        {
            if (tileX != -1 && tileY != -1)
            {
                int width = level.maxX - level.minX;
                int i = (tileY * width) + tileX;
                BoB_Tile t = level.tiles[i];

                if (rightClickVer)
                {
                    t_Collision.Checked = true;
                    t_ID.Value = t.id;
                    t_Height.Text = t.h.ToString();
                    t_Collision.Checked = true;
                    if (t.col == 0)
                    {
                        t_Collision.Checked = false;
                    }
                }
                else if (!mouseDragVer)
                {
                    affectedTiles = new Dictionary<int, BoB_Tile>();
                    toolActing = true;
                }
                if (!affectedTiles.ContainsKey(i))
                {
                    affectedTiles.Add(i, t);
                    switch (toolType)
                    {
                        case "paint":
                            // Tile ID
                            uint tileID = (uint)t_ID.Value;
                            t.id = tileID;
                            if (ModifierKeys == Keys.Control && !mouseDragVer)
                            {
                                // Replace all tiles with the target's ID with the new tile
                                uint targetID = level.tiles[i].id;
                                for (int a = 0; a < level.tiles.Length; a++)
                                {
                                    if (level.tiles[a].id == targetID)
                                    {
                                        BoB_Tile recoloredTile = level.tiles[a];
                                        recoloredTile.id = tileID;
                                        level.tiles[a] = recoloredTile;
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
                                level.tiles[i] = t;
                            }
                            else
                            {
                                level.tiles[i] = t;
                            }
                            break;
                        case "erase":
                            t.id = 1;
                            t.col = 0;
                            t.h = 0;
                            if (ModifierKeys == Keys.Control && !mouseDragVer)
                            {
                                // Replace all tiles with the target's ID with the new tile
                                uint targetID = level.tiles[i].id;
                                for (int a = 0; a < level.tiles.Length; a++)
                                {
                                    if (level.tiles[a].id == targetID)
                                    {
                                        level.tiles[a] = t;
                                    }
                                }
                            }
                            else
                            {
                                level.tiles[i] = t;
                            }
                            break;
                        case "collision":
                            t.col = 0;
                            if (t_Collision.Checked)
                            {
                                t.col = 1;
                            }
                            level.tiles[i] = t;
                            break;
                        case "elevation":
                            decimal elevateHeight = 0;
                            if (ModifierKeys == Keys.Control)
                            {
                                // Just Set Height
                                if (decimal.TryParse(t_Height.Text, out elevateHeight))
                                {
                                    t.h = elevateHeight;
                                    level.tiles[i] = t;
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
                                    level.tiles[i] = t;
                                }
                            }
                            UpdateLevelHeights();
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
            return Color.Yellow;
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
                    Color heightMap = Color.FromArgb(0, 255, 0);
                    decimal heightRange = highestHeight - lowestHeight;
                    if (heightRange <= 0)
                    {
                        return heightMap;
                    }
                    double hue = heightMap.GetHue();
                    decimal heightAboveLow = t.h - lowestHeight;
                    decimal relativeHeight = heightAboveLow / heightRange;
                    hue = 240 * (double)relativeHeight;
                    return ColorFromHSV(hue, 1.0, 1.0);
                }
                return Color.Black;
            }
            else if (t.id >= 0 && t.id < tileColors.Length)
            {
                return tileColors[t.id];
            }
            else return Color.Magenta;
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
            for (int i = 0; i < level.tiles.Length; i++)
            {
                BoB_Tile t = level.tiles[i];
                t.h = Math.Round(t.h, 3);
                level.tiles[i] = t;
            }
        }
        private void setAllTileCollisionsToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < level.tiles.Length; i++)
            {
                BoB_Tile t = level.tiles[i];
                if (t.id > 0 && t.id < tileCollision.Length)
                {
                    t.col = 0;
                    if (tileCollision[t.id])
                    {
                        t.col = 1;
                    }
                    level.tiles[i] = t;
                }
            }
            UpdateLevelImage(sender, e);
        }

        private void heightMultiplyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            decimal multiplier;
            if (!level.parse_succeeded) return;
            if (decimal.TryParse(heightMultiplierBox.Text, out multiplier))
            {
                for (int i = 0; i < level.tiles.Length; i++)
                {
                    BoB_Tile t = level.tiles[i];
                    t.h *= multiplier;
                    level.tiles[i] = t;
                }
                UpdateLevelHeights();
            }
            else
            {
                MessageBox.Show("Height multiplier was invalid...");
            }
            UpdateLevelImage(sender, e);
        }

        private void newLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            level = new Level();
            level.parse_succeeded = true;
            level.minX = 0;
            level.maxX = 5;
            level.minY = 0;
            level.maxY = 5;
            level.players = new BoB_Position[4];
            level.players[0] = new BoB_Position() { x = 0, y = 0 };
            level.players[1] = new BoB_Position() { x = 4, y = 4 };
            level.players[2] = new BoB_Position() { x = 0, y = 4 };
            level.players[3] = new BoB_Position() { x = 4, y = 0 };
            level.enemy_type = 7;
            level.enemies = new BoB_Position[1];
            level.enemies[0] = new BoB_Position() { x = 2, y = 2 };
            level.tiles = new BoB_Tile[25];
            for (int i = 0; i < level.tiles.Length; i++)
            {
                BoB_Tile t = new BoB_Tile()
                {
                    id = 2,
                    h = 0,
                    col = 0
                };
                level.tiles[i] = t;
            }
            LoadLevelData();
        }
    }
}