using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using StardewValley;
using StardewValley.TerrainFeatures;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using xTile;
using xTile.Format;
using xTile.Tiles;
using xTile.Dimensions;
using xTile.Display;

/* Advize's Farm Expansion Mod v1.3 */

namespace busStopStorage
{

    public class busStopStorage : Mod
    {

        private static Map loadedMap;
        private static string modPath = "";
        private static ContentManager content;
        //private static IDisplayDevice mapDisplayDevice;

        //Executes when SMAPI loads the mod. I subscribe to the events I need and attempt to load a config file.
        public override void Entry(params object[] objects)
        {
            modPath = this.PathOnDisk;
            TimeEvents.DayOfMonthChanged += Event_DayOfMonthChanged;
            GameEvents.LoadContent += Event_LoadContent;
        }

        static void Event_LoadContent(object sender, EventArgs e)
        {
            content = new ContentManager(new GameServiceContainer(),modPath);
            //mapDisplayDevice = new XnaDisplayDevice(content, Game1.game1.GraphicsDevice);
            loadedMap = content.Load<Map>("BusStopStorage");
            var sheet = loadedMap.GetTileSheet("zcustom");
            sheet.ImageSource = Path.Combine("../Mods/BusStopStorage", sheet.ImageSource);
        }

        //I use this event as a hack since there is no proper event that fires after a game save has been loaded, unsubscribe once done with it for performance boost
        static void Event_DayOfMonthChanged(object sender, EventArgs e)
        {
            // Error prevention. Returns early if all 47 default locations have not yet loaded.
            if (Game1.locations.Count < 47) return;
            string mapName = "BusStopStorage";
            // More error prevention. Returns and unsubscribes from event if custom location has already been added to the game.
            for (int i = 0; i <= Game1.locations.Count - 1; i++)
            {
                GameLocation currentLocation = Game1.locations[i];
                if (currentLocation.Name == mapName)
                {
                    TimeEvents.DayOfMonthChanged -= Event_DayOfMonthChanged;
                    return;
                }
                else if (currentLocation.Name == "BusStop")
                {
                    currentLocation.setTileProperty(3, 21, "Buildings", "Action","Warp 44 55 "+ mapName);/**/
                    PatchMap(currentLocation.Map);
                }
            }
            //Load my custom map, add location properties as necessary, add the location to the game, and add warp points.
            GameLocation busStopStorage = new GameLocation(loadedMap, mapName);


            busStopStorage.isFarm = false;
            busStopStorage.isOutdoors = false;
            Game1.locations.Add(busStopStorage);

            TimeEvents.DayOfMonthChanged -= Event_DayOfMonthChanged;
        }
        //Uses Xna Framework for loading maps outside of the game\content directory
        static Map LoadMap(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            Map map = null;

            var path = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            map = content.Load<Map>(fileName);
            if (map == null) throw new FileLoadException();

            return map;
        }
        //Tile edits
        private static void PatchMap(Map map)
        {
            //Create array of custom tile objects with layer, x & y location, and new texture information (-1 as a texture means set the tile to null)
            List<Tile> tileArray = new List<Tile>();
            tileArray.Add(new Tile(1, 3, 21, 435));
            tileArray.Add(new Tile(2, 3, 20, 410));
            tileArray.Add(new Tile(1, 3, 19, 411));
            tileArray.Add(new Tile(1, 3, 20, 436));

            //Attempt to make the tile changes based on new tiles found in the array
            foreach (Tile tile in tileArray)
            {
                //Log.Success("Now trying coordinates " + tile.l + " " + tile.x + " " + tile.y);
                if (tile.tileIndex < 0)
                {
                    map.Layers[tile.l].Tiles[tile.x, tile.y] = null;
                    //Log.Success("Setting tile: " + tile.l + " " + tile.x + " " + tile.y + " to null");
                    continue;
                }

                if (map.Layers[tile.l].Tiles[tile.x, tile.y] != null)
                {
                    map.Layers[tile.l].Tiles[tile.x, tile.y].TileIndex = tile.tileIndex;
                    //Log.Success("Setting tile: " + tile.l + " " + tile.x + " " + tile.y + " " + tile.tileIndex);
                }
                else
                {
                    int width = 0;
                    bool done = false;

                    while (width <= map.Layers[tile.l].LayerWidth && !done)
                    {
                        int height = 0;
                        while (height <= map.Layers[tile.l].LayerHeight)
                        {
                            if (map.Layers[tile.l].Tiles[width, height] != null)
                            {
                                if (map.Layers[tile.l].Tiles[width, height].TileIndex != tile.tileIndex)
                                {
                                    height++;
                                    continue;
                                }
                                map.Layers[tile.l].Tiles[tile.x, tile.y] = map.Layers[tile.l].Tiles[width, height];
                                //Log.Success(tile.l + " " + tile.x + " " + tile.y + " was null. Copying from " + tile.l + " " + width + " " + height + " " + map.Layers[tile.l].Tiles[width, height].TileIndex);
                                done = true;
                                break;
                            }
                            height++;
                        }
                        width++;
                    }
                    //if (!done) Log.Success("Failed to find a clone");
                }
            }
        }
    }

    public class Tile
    {
        public int l;
        public int x;
        public int y;
        public int tileIndex;

        public Tile(int l, int x, int y, int tileIndex)
        {
            this.l = l; this.x = x; this.y = y; this.tileIndex = tileIndex;
        }
    }
}