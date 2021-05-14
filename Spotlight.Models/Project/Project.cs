using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace Spotlight.Models
{
    public class Project
    {
        public Project()
        {
            //Palettes = new List<Palette>();
            //WorldInfo = new List<WorldInfo>();
            //EmptyWorld = new WorldInfo();
            //TileStates = new List<TileState>()
            //{
            //    new TileState("Background", TileState.Background),
            //    new TileState("Foreground", TileState.Foreground),
            //    new TileState("Water", TileState.Water),
            //    new TileState("Water Foreground", TileState.WaterForeground),
            //    new TileState("Semi-Solid", TileState.SemiSolid),
            //    new TileState("Hidden Item Block", TileState.HiddenItemBlock),
            //    new TileState("Solid", TileState.Solid),
            //    new TileState("Item Block", TileState.ItemBlock)
            //};

            //TileInteractions = new List<TileInteraction>()
            //{
            //    new TileInteraction("None", 0x00),
            //    new TileInteraction("Harmful", 0x01),
            //    new TileInteraction("Deletes Air", 0x02),
            //    new TileInteraction("Pushes Left", 0x03),
            //    new TileInteraction("Pushes Right", 0x04),
            //    new TileInteraction("Pushes Up", 0x05),
            //    new TileInteraction("Pushes Down", 0x06),
            //    new TileInteraction("Ten Coins", 0x07),
            //    new TileInteraction("Locked Door", 0x08),
            //    new TileInteraction("Objects Interact", 0x09),
            //    new TileInteraction("Activates Turrets", 0x0A),
            //    new TileInteraction("Climbable", 0x0B),
            //    new TileInteraction("Coin", 0x0C),
            //    new TileInteraction("Door", 0x0D),
            //    new TileInteraction("Cherry", 0x0E),
            //    new TileInteraction("Unused", 0x0F),
            //    new TileInteraction("Lava", TileState.Water | 0x01),
            //    new TileInteraction("Lava", TileState.WaterForeground | 0x01),
            //    new TileInteraction("Slick (Icy)", TileState.Solid | 0x02),
            //    new TileInteraction("Thin Ice (Breaks From Underneath)", TileState.Solid | 0x07),
            //    new TileInteraction("Left Vertical Pipe Enterance", TileState.Solid | 0x08),
            //    new TileInteraction("Right Vertical Pipe Entrance", TileState.Solid | 0x09),
            //    new TileInteraction("Horizontal Pipe Entrance", TileState.Solid | 0x0A),
            //    new TileInteraction("Climbable Semi-Solid", TileState.Solid | 0x0B),
            //    new TileInteraction("Key Interaction", TileState.SemiSolid | 0x0B),
            //    new TileInteraction("Objects Interact", 0x0C),
            //    new TileInteraction("Hammers and Explosion Breaks", 0x0D),
            //    new TileInteraction("P-Switch", 0x0E),
            //    new TileInteraction("Event Switch", 0x0F),
            //    new TileInteraction("Coin ",  TileState.ItemBlock),
            //    new TileInteraction("Hidden Coin ",  TileState.HiddenItemBlock ),
            //    new TileInteraction("Fire Flower", TileState.ItemBlock |0x01),
            //    new TileInteraction("Hidden Fire Flower", TileState.HiddenItemBlock |0x01),
            //    new TileInteraction("Super Leaf",  TileState.ItemBlock |0x02),
            //    new TileInteraction("Hidden Super Leaf",  TileState.HiddenItemBlock |0x02),
            //    new TileInteraction("Ice Flower",  TileState.ItemBlock |0x03),
            //    new TileInteraction("Hidden Ice Flower",  TileState.HiddenItemBlock |0x03),
            //    new TileInteraction("Frog Suit",  TileState.ItemBlock |0x04),
            //    new TileInteraction("Hidden Frog Suit",  TileState.HiddenItemBlock |0x04),
            //    new TileInteraction("FireFoxLeaf", TileState.ItemBlock | 0x05),
            //    new TileInteraction("Hidden FireFoxLeaf", TileState.HiddenItemBlock | 0x05),
            //    new TileInteraction("Koopa Shell", TileState.ItemBlock | 0x06),
            //    new TileInteraction("Hidden Koopa Shell", TileState.HiddenItemBlock | 0x06),
            //    new TileInteraction("Unused Item", TileState.ItemBlock | 0x07),
            //    new TileInteraction("Hidden Unused Item", TileState.HiddenItemBlock | 0x07),
            //    new TileInteraction("Hammer Suit", TileState.ItemBlock | 0x08),
            //    new TileInteraction("Hidden Hammer Suit", TileState.HiddenItemBlock | 0x08),
            //    new TileInteraction("Ninja Mushroom", TileState.ItemBlock |0x09),
            //    new TileInteraction("Hidden Ninja Mushroom", TileState.HiddenItemBlock |0x09),
            //    new TileInteraction("Unused Item",  TileState.ItemBlock |0x0A),
            //    new TileInteraction("Hidden Unused Item",  TileState.HiddenItemBlock |0x0A),
            //    new TileInteraction("Vine", TileState.ItemBlock | 0x0B),
            //    new TileInteraction("Hidden Vine", TileState.HiddenItemBlock | 0x0B),
            //    new TileInteraction("Toggle Block Above (P-Switch)", TileState.ItemBlock | 0x0C),
            //    new TileInteraction("Hidden Toggle Block Above (P-Switch)", TileState.HiddenItemBlock | 0x0C),
            //    new TileInteraction("Brick", TileState.ItemBlock | 0x0D),
            //    new TileInteraction("Hidden Brick", TileState.HiddenItemBlock | 0x0D),
            //    new TileInteraction("Spinner", TileState.ItemBlock | 0x0E),
            //    new TileInteraction("Hidden Spinner", TileState.HiddenItemBlock | 0x0E),
            //    new TileInteraction("Key", TileState.ItemBlock | 0x0F),
            //    new TileInteraction("Hidden Key", TileState.HiddenItemBlock | 0x0F),
            //    new TileInteraction("Waterfall", TileState.Water | 0x06),
            //    new TileInteraction("Waterfall", TileState.WaterForeground | 0x06),
            //    new TileInteraction("Conveyor Left", TileState.Solid | 0x03),
            //    new TileInteraction("Conveyor Right", TileState.Solid | 0x04),
            //    new TileInteraction("Conveyor Up", TileState.Solid | 0x05),
            //    new TileInteraction("Conveyor Down", TileState.Solid | 0x06)
            //};

            //TextTable = new Dictionary<string, List<KeyValuePair<string, string>>>();
            //TextTable["music"] = new List<KeyValuePair<string, string>>();
            //TextTable["music"].Add(new KeyValuePair<string, string>("0", "None"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("1", "Grass Land"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("2", "Desert Land"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("3", "Water Land"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("4", "Giant Land"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("5", "Sky Land"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("6", "Ice Land"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("7", "Pipe Land"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("8", "Dark Land"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("9", "Clouds"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("A", "Invinicible"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("B", "Warp Whistle"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("C", "Music Box"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("D", "Falling In the Sky"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("E", "Bonus Game"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("F", "Princess Saved/End Credits"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("10", "Plains"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("20", "Underground"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("30", "Water"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("40", "NoneFortress"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("50", "Boss Battle"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("60", "Air Ship"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("70", "Hammer Bros."));
            //TextTable["music"].Add(new KeyValuePair<string, string>("80", "P-Switch"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("90", "Hilly"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("A0", "P-Switch"));
            //TextTable["music"].Add(new KeyValuePair<string, string>("B0", "Bowser Fight"));

            //GameObjects = new GameObject[256];
            //MapTileInteractions = new List<MapTileInteraction>();
            //MapTileInteractions.Add(new MapTileInteraction() { Value = 0, Name = "Boundary" });
            //MapTileInteractions.Add(new MapTileInteraction() { Value = 1, Name = "Traversable" });
            //MapTileInteractions.Add(new MapTileInteraction() { Value = 2, Name = "Enterable" });
            //MapTileInteractions.Add(new MapTileInteraction() { Value = 3, Name = "Enterable Completable" });
            //MapTileInteractions.Add(new MapTileInteraction() { Value = 4, Name = "Blocked Road" });
        }

        public string Name { get; set; }
        public string DirectoryPath { get; set; }
        public List<Palette> Palettes { get; set; }
        public List<WorldInfo> WorldInfo { get; set; }
        public WorldInfo EmptyWorld { get; set; }
        public List<TileTerrain> TileTerrain { get; set; }
        public List<MapTileInteraction> MapTileInteractions { get; set; }
        public List<TileSet> TileSets { get; set; }
        public GameObject[] GameObjects { get; set; }
        public Dictionary<string, List<KeyValuePair<string, string>>> TextTable { get; set; }
        public Color[] RgbPalette { get; set; }
    }
}