using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BoB_LevelData
{
    public struct Level
    {
        public bool parse_succeeded;
        public int minX;
        public int maxX;
        public int minY;
        public int maxY;
        public BoB_Position[] players;
        public int enemy_type;
        public BoB_Position[] enemies;
        public BoB_Tile[] tiles;

        public static Level[] ParseLevels(string input, bool deleteFirstDefault)
        {
            List<Level> levels = new List<Level>();
            string arraySearch = "new Array";
            string concatSearch = "f_ConcatArray";
            string concatenatedData = "";
            if (input.Contains(arraySearch))
            {
                string[] arraySubstrings = input.Split(arraySearch, StringSplitOptions.TrimEntries);
                for (int i = 1; i < arraySubstrings.Length; i++)
                {
                    int startIndex = arraySubstrings[i].IndexOf("(") + 1;
                    int endIndex = arraySubstrings[i].IndexOf(")");
                    if (startIndex == -1 || endIndex == -1)
                    {
                        MessageBox.Show($"Detected Array[{i}]  did not contain parenthesis. Continuing...");
                        continue;
                    }
                    string rawData = arraySubstrings[i].Substring(startIndex, endIndex - startIndex);
                    if (arraySubstrings[i].Contains(concatSearch))
                    {
                        // This level is concatenated, don't add to the level array and just add its data to the next one
                        concatenatedData += rawData + ",";
                        Debug.WriteLine($"Detected Array[{i}] is concatenated data, continuing...");
                    }
                    else
                    {
                        string finalString = concatenatedData + rawData;
                        concatenatedData = "";
                        levels.Add(ParseLevel(finalString));
                        Debug.WriteLine($"Added New Level from Detected Array[{i}]...");
                    }
                }
            }
            else
            {
                MessageBox.Show("Text did not contain any level arrays...");
                return new Level[0];
            }
            for (int i = 0; i < levels.Count; i++)
            {
                // Trim out all invalid levels
                if (!levels[i].parse_succeeded)
                {
                    Debug.WriteLine($"Removing Level [{i}], it's parsing wasn't successful");
                    levels.RemoveAt(i);
                    i--;
                }
                else if (i == 0 && deleteFirstDefault)
                {
                    string[] default_substrings = DefaultLevelString().Split(",", StringSplitOptions.TrimEntries);
                    string[] current_substrings = LevelSubstrings(levels[i]);
                    if (default_substrings.Length == current_substrings.Length || !levels[i].parse_succeeded)
                    {
                        Debug.WriteLine($"Removing Level [{i}], it's the default, debug level");
                        levels.RemoveAt(i);
                        i--;
                    }
                }
            }
            return levels.ToArray();
        }
        public static Level ParseLevel(string input)
        {
            Debug.WriteLine("Preparing to parse next level: " +  input);
            Level l = new Level();
            l.parse_succeeded = false;
            string[] substrings = input.Split(",", StringSplitOptions.TrimEntries);
            if (substrings.Length < 12)
            {
                Debug.WriteLine("Input did not contain enough Level Array Data, continuing...");
                return l;
            }

            // Level Dimensions Data
            try
            {
                l.minX = int.Parse(substrings[0]);
                Debug.WriteLine($"Min X: {l.minX}");
                l.minY = int.Parse(substrings[1]);
                Debug.WriteLine($"Min Y: {l.minY}");
                l.maxX = int.Parse(substrings[2]);
                Debug.WriteLine($"Max X: {l.maxX}");
                l.maxY = int.Parse(substrings[3]);
                Debug.WriteLine($"Max Y: {l.maxY}");
            }
            catch
            {
                MessageBox.Show("Error parsing level dimensions data...");
                return l;
            }

            // Player Data
            try
            {
                l.players = new BoB_Position[4];
                l.players[0] = new BoB_Position { x = int.Parse(substrings[4]), y = int.Parse(substrings[5]) };
                Debug.WriteLine($"Player 1 Position: ({l.players[0].x},{l.players[0].y})");
                l.players[1] = new BoB_Position { x = int.Parse(substrings[6]), y = int.Parse(substrings[7]) };
                Debug.WriteLine($"Player 2 Position: ({l.players[1].x},{l.players[1].y})");
                l.players[2] = new BoB_Position { x = int.Parse(substrings[8]), y = int.Parse(substrings[9]) };
                Debug.WriteLine($"Player 3 Position: ({l.players[2].x},{l.players[2].y})");
                l.players[3] = new BoB_Position { x = int.Parse(substrings[10]), y = int.Parse(substrings[11]) };
                Debug.WriteLine($"Player 4 Position: ({l.players[3].x},{l.players[3].y})");
            }
            catch
            {
                MessageBox.Show("Error parsing player position data...");
                return l;
            }

            // This is where things start getting length dependent,
            // so readIndex is used to keep track of where we are in the array
            int readIndex = 12;
            // Enemy Data
            try
            {
                l.enemy_type = int.Parse(substrings[12]);
                Debug.WriteLine($"Enemy Type: {l.enemy_type}");
                l.enemies = new BoB_Position[int.Parse(substrings[13])];
                Debug.WriteLine($"Enemy Count: ({l.enemies.Length})");
                readIndex += 2;
                for (int i = 0; i < l.enemies.Length; i++)
                {
                    l.enemies[i] = new BoB_Position { x = int.Parse(substrings[readIndex]), y = int.Parse(substrings[readIndex + 1]) };
                    Debug.WriteLine($"Enemy {i} Position: ({l.enemies[i].x}, {l.enemies[i].y})");
                    readIndex += 2;
                }
            }
            catch
            {
                MessageBox.Show($"Error parsing enemy data...{Environment.NewLine}Stopped at substring: [{readIndex}]");
                return l;
            }

            // Tile Data
            List<BoB_Tile> tileList = new List<BoB_Tile>();
            while (readIndex < substrings.Length)
            {
                try
                {
                    BoB_Tile t = new BoB_Tile();
                    uint tileType = uint.Parse(substrings[readIndex]);
                    if (tileType == 0)
                    {
                        t.id = 0;
                        readIndex++;
                        tileList.Add(t);
                        Debug.WriteLine($"Tile {tileList.Count} --- ID: 0");
                    }
                    else
                    {
                        t.id = tileType;
                        readIndex++;
                        t.h = decimal.Parse(substrings[readIndex], NumberStyles.Float);
                        t.h = Math.Round(t.h, 14);
                        readIndex++;
                        t.col = uint.Parse(substrings[readIndex]);
                        readIndex++;
                        tileList.Add(t);
                        Debug.WriteLine($"Tile {tileList.Count} --- ID: {t.id}, Height: {t.h}, Collision: {t.col}");
                    }
                }
                catch
                {
                    try
                    {
                        MessageBox.Show($"Error parsing tile data...{Environment.NewLine}Stopped at substring: [{readIndex}], broken substring: '{substrings[readIndex]}'");
                    }
                    catch
                    {
                        MessageBox.Show("Error parsing tile data...");
                    }
                    return l;
                }
            }

            int width = l.maxX - l.minX;
            int height = l.maxY - l.minY;
            if (tileList[tileList.Count - 1].id == 0 && tileList.Count > (width * height))
            {
                int trimCount = tileList.Count - (width * height);
                Debug.WriteLine($"Trimming off ({trimCount}) additional tiles...");
                tileList.RemoveRange(width * height, trimCount);
            }
            l.tiles = tileList.ToArray();
            l.parse_succeeded = true;
            return l;
        }
        public static string[] LevelSubstrings(Level l)
        {
            List<string> substrings = new List<string>();
            substrings.Add(l.minX.ToString());
            substrings.Add(l.minY.ToString());
            substrings.Add(l.maxX.ToString());
            substrings.Add(l.maxY.ToString());
            substrings.Add(l.players[0].x.ToString());
            substrings.Add(l.players[0].y.ToString());
            substrings.Add(l.players[1].x.ToString());
            substrings.Add(l.players[1].y.ToString());
            substrings.Add(l.players[2].x.ToString());
            substrings.Add(l.players[2].y.ToString());
            substrings.Add(l.players[3].x.ToString());
            substrings.Add(l.players[3].y.ToString());
            substrings.Add(l.enemy_type.ToString());
            substrings.Add(l.enemies.Length.ToString());
            for (int i = 0; i < l.enemies.Length; i++)
            {
                substrings.Add(l.enemies[i].x.ToString());
                substrings.Add(l.enemies[i].y.ToString());
            }
            for (int i = 0; i < l.tiles.Length; i++)
            {
                substrings.Add(l.tiles[i].id.ToString());
                substrings.Add(l.tiles[i].h.ToString());
                substrings.Add(l.tiles[i].col.ToString());
            }
            substrings.Add("0");
            return substrings.ToArray();
        }
        public static string ExportedLevelString(Level level, string arrayVar, int index, int concatThresh)
        {
            string[] substrings = LevelSubstrings(level);
            string finalCode = $"{arrayVar}[{index}] = new Array(";
            int concatGoal = substrings.Length;
            Debug.WriteLine($"Preparing to write ({substrings.Length}) substrings...");
            if (concatThresh > 0)
            {
                if (substrings.Length > concatThresh)
                {
                    concatGoal = substrings.Length / 2;
                }
                if (substrings.Length > (2 * concatThresh))
                {
                    concatGoal = concatThresh;
                }
            }
            int substringsRead = 0;
            bool concated = false;
            for (int i = 0; i < substrings.Length; i++)
            {
                finalCode += substrings[i];
                substringsRead++;
                if (substringsRead == concatGoal && i < (substrings.Length - 5))
                {
                    substringsRead = 0;
                    if (concated)
                    {
                        finalCode += ")";
                    }
                    finalCode += $");{Environment.NewLine}   {arrayVar}[{index}] = f_ConcatArray({arrayVar}[{index}],new Array(";
                    concated = true;
                }
                else if (i < substrings.Length - 1)
                {
                    finalCode += ",";
                }
            }
            if (concated)
            {
                finalCode += ")";
            }
            finalCode += ");";
            return finalCode;
        }
        public static string DefaultLevelString()
        {
            return "0,0,5,5,0,0,4,4,0,4,4,0,7,1,2,2,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,2,0,0,0";
        }
    }
    public struct BoB_Position
    {
        public int x;
        public int y;
    }
    public struct BoB_Tile
    {
        public uint id;
        public decimal h;
        public uint col;
    }
}

