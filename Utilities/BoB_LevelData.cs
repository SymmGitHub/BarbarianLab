using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BoB_LevelData
{
    public static string ArrayString(Level l)
    {
        string output = "";
        output += $"{l.minX},{l.minY},{l.maxX},{l.maxY},";
        output += $"{l.players[0].x},{l.players[0].y},";
        output += $"{l.players[1].x},{l.players[1].y},";
        output += $"{l.players[2].x},{l.players[2].y},";
        output += $"{l.players[3].x},{l.players[3].y},";
        output += $"{l.enemy_type},";
        output += $"{l.enemies.Length},";
        for (int i = 0; i < l.enemies.Length; i++)
        {
            output += $"{l.enemies[i].x},{l.enemies[i].y},";
            Debug.WriteLine("Writing enemy data: " + i);
        }
        int width = l.maxX - l.minX;
        int height = l.maxY - l.minY;
        for (int i = 0; i < (width * height); i++)
        {
            output += $"{l.tiles[i].id},{l.tiles[i].h},{l.tiles[i].col},";
            Debug.WriteLine("Writing tile data: " + i);
        }
        output += "0";
        return output;
    }
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
        public static Level ParseLevel(string input)
        {
            Level l = new Level();
            l.parse_succeeded = false;
            int startIndex = input.IndexOf("(") + 1;
            int endIndex = input.IndexOf(")");
            if (startIndex == -1 || endIndex == -1)
            {
                MessageBox.Show("Input did not contain parenthesis for marking a Level Array...");
                return l;
            }
            input = input.Substring(startIndex, endIndex - startIndex);
            string[] substrings = input.Split(",", StringSplitOptions.TrimEntries);
            if (substrings.Length < 12)
            {
                MessageBox.Show("Input did not contain enough Level Array Data...");
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
                    MessageBox.Show($"Error parsing tile data...{Environment.NewLine}Stopped at substring: [{readIndex}], broken substring: '{substrings[readIndex]}'");
                    return l;
                }
            }

            int width = l.maxX - l.minX;
            int height = l.maxY - l.minY;
            if (tileList[tileList.Count - 1].id == 0 && tileList.Count == 1 + (width * height))
            {
                Debug.WriteLine("Trimming off final empty tile");
                tileList.RemoveAt(tileList.Count - 1);
            }
            l.tiles = tileList.ToArray();
            l.parse_succeeded = true;
            return l;
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

