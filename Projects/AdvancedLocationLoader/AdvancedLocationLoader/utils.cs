using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Microsoft.Xna.Framework.Content;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Locations;

using xTile;
using xTile.ObjectModel;
using xTile.Tiles;
using xTile.Layers;

namespace Entoarox.AdvancedLocationLoader
{

    public class LocationUtils
    {
        public static int getLocationIndex(string name)
        {
            for (var c = 0; c < Game1.locations.Count; c++)
                if (Game1.locations[c].name == name)
                    return c;
            return -1;
        }
        public static void setTileProperty(GameLocation location, string layer, int tileX, int tileY, string key, PropertyValue value)
        {
            try
            {
                if (!location.map.GetLayer(layer).Tiles[tileX, tileY].Properties.ContainsKey(key))
                    location.map.GetLayer(layer).Tiles[tileX, tileY].Properties.Add(key, new PropertyValue(value));
                else
                    location.map.GetLayer(layer).Tiles[tileX, tileY].Properties[key] = value;
            }
            catch
            {

            }
        }
        public static PropertyValue getTileProperty(GameLocation location, string layer, int tileX, int tileY, string key)
        {
            PropertyValue value;
            location.map.GetLayer(layer).Tiles[tileX, tileY].Properties.TryGetValue(key, out value);
            return value;
        }
        public static void setStaticTile(GameLocation location, string layer, int tileX, int tileY, int tileIndex)
        {
            try
            {
                Layer tileLayer = location.map.GetLayer(layer);
                if (tileLayer.Tiles[tileX, tileY] == null)
                {
                    tileLayer.Tiles[tileX, tileY] = (Tile)new StaticTile(tileLayer, location.map.TileSheets[0], BlendMode.Alpha, tileIndex);
                }
                else
                {
                    tileLayer.Tiles[tileX, tileY].TileIndex = tileIndex;
                }
            }
            catch
            {

            }
        }
        public static void setStaticTile(GameLocation location, string layer, int tileX, int tileY, int tileIndex, int tileSheet)
        {
            try
            {
                Layer tileLayer = location.map.GetLayer(layer);
                tileLayer.Tiles[tileX, tileY] = (Tile)new StaticTile(tileLayer, location.map.TileSheets[tileSheet], BlendMode.Alpha, tileIndex);
            }
            catch
            {

            }
        }
        public static void setDynamicTile(GameLocation location, string layer, int tileX, int tileY, int[] tileIndexes, int frameInterval)
        {
            setDynamicTile(location, layer, tileX, tileY, tileIndexes, 0, frameInterval);
        }
        public static void setDynamicTile(GameLocation location, string layer, int tileX, int tileY, int[] tileIndexes, int tileSheet, int frameInterval)
        {
            try
            {
                List<StaticTile> tiles = new List<StaticTile>();
                Layer tileLayer = location.map.GetLayer(layer);
                foreach (int i in tileIndexes)
                {
                    tiles.Add(new StaticTile(tileLayer, location.map.TileSheets[tileSheet], BlendMode.Alpha, i));
                }

                tileLayer.Tiles[tileX, tileY] = (Tile)new AnimatedTile(tileLayer, tiles.ToArray(), frameInterval);
            }
            catch
            {

            }
        }
        public static void setDynamicTile(GameLocation location, string layer, int tileX, int tileY, int[] tileIndexes, int[] tileSheets, int frameInterval)
        {
            try
            {
                List<StaticTile> tiles = new List<StaticTile>();
                Layer tileLayer = location.map.GetLayer(layer);
                foreach (int i in tileIndexes)
                {
                    tiles.Add(new StaticTile(tileLayer, location.map.TileSheets[tileSheets[tiles.Count()]], BlendMode.Alpha, i));
                }
                tileLayer.Tiles[tileX, tileY] = (Tile)new AnimatedTile(tileLayer, tiles.ToArray(), frameInterval);
            }
            catch
            {

            }
        }
        public static void removeTile(GameLocation location, string layer, int tileX, int tileY)
        {
            try
            {
                Layer tileLayer = location.map.GetLayer(layer);
                tileLayer.Tiles[tileX, tileY] = (Tile)null;
            }
            catch
            {

            }
        }
    }
    public class ReflectionUtils
    {
        public static dynamic getReflectedInstanceField(object target, string field)
        {
            return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }
        public static void setReflectedInstanceField(object target, string field, dynamic value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
    public class LogUtils
    {
        private string modName;
        private bool debugMode = false;
        public LogUtils(string modName)
        {
            this.modName = modName;
        }
        public LogUtils(string modName, bool debugMode)
        {
            this.modName = modName;
            this.debugMode = debugMode;
        }
        public void setDebugMode(bool debugMode)
        {
            this.debugMode = debugMode;
        }
        public void log(string message)
        {
            Log.Async('[' + modName + "/LOG] " + message);
        }
        public void debug(string message)
        {
            if (this.debugMode)
                Log.AsyncO('[' + modName + "/DEBUG] " + message);
        }
        public void info(string message)
        {
            Log.AsyncC('[' + modName + "/INFO] " + message);
        }
        public void error(string message)
        {
            Log.AsyncR('[' + modName + "/ERROR] " + message);
        }
    }
}
