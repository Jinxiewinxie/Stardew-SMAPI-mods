using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using xTile;

namespace Entoarox.AdvancedLocationLoader
{
    public class ModConfig : Config
    {
        public override T GenerateDefaultConfig<T>()
        {
            debugMode = false;
            return this as T;
        }
        public bool debugMode;
    }
    public class LocationConfig:Config
    {
        public override T GenerateDefaultConfig<T>()
        {
            wasLoaded = true;
            return this as T;
        }
        public bool wasLoaded;
        public Dictionary<string, string> about;
        public List<Dictionary<string, string>> locations;
        public List<Dictionary<string, string>> overrides;
        public List<List<string>> tiles;
        public List<List<string>> properties;
        public List<List<string>> warps;
    }
    public class AdvancedLocationLoader : Mod
    {
        public static string modPath;
        public static string configPath;
        public static LogUtils logger;
        public static ModConfig config;
        public static List<LocationConfig> overrides= new List<LocationConfig>();
        public static List<LocationConfig> locations = new List<LocationConfig>();
        public static List<LocationConfig> tiles = new List<LocationConfig>();
        public static List<LocationConfig> properties = new List<LocationConfig>();
        public static List<LocationConfig> warps = new List<LocationConfig>();
        public static Dictionary<string, Microsoft.Xna.Framework.Content.ContentManager> cManagers=new Dictionary<string, Microsoft.Xna.Framework.Content.ContentManager>();
        public static Dictionary<string, string> managerMatch=new Dictionary<string, string>();
        public static Microsoft.Xna.Framework.Content.ContentManager getContentManager(string path)
        {
            if (!cManagers.ContainsKey(path))
                cManagers.Add(path, new Microsoft.Xna.Framework.Content.ContentManager(Game1.content.ServiceProvider, path));
            return cManagers[path];
        }
        public override void Entry(params object[] objects)
        {
            modPath = this.PathOnDisk;
            configPath = this.BaseConfigPath;
            logger = new LogUtils("AdvancedLocationLoader");
            GameEvents.LoadContent += Event_LoadContent;
            TimeEvents.DayOfMonthChanged += Event_DayOfMonthChanged;
        }
        public static void Event_LoadContent(object caller, EventArgs e)
        {
            try
            {
                Game1.game1.Content = new ContentManager(Game1.game1.Content.ServiceProvider, Game1.game1.Content.RootDirectory);
                config = new ModConfig().InitializeConfig(configPath);
                logger.setDebugMode(config.debugMode);
                logger.info("Loading patch data into memory...");
                foreach(string dir in Directory.GetDirectories(Path.Combine(modPath,"locations")))
                {
                    if (!File.Exists(Path.Combine(dir, "manifest.json")))
                    {
                        logger.error("Directory does not contain a manifest: " + dir);
                    }
                    else
                    {
                        logger.debug("Loading manifest for " + dir);
                        LocationConfig locConfig = new LocationConfig().InitializeConfig(Path.Combine(dir, "manifest.json"));
                        logger.debug("Checking for overrides...");
                        if (locConfig.overrides != null&&locConfig.overrides.Count>0)
                        {
                            logger.debug("Found overrides, added to cache");
                            overrides.Add(locConfig);
                        }
                        logger.debug("Checking for locations...");
                        if (locConfig.locations != null && locConfig.locations.Count > 0)
                        {
                            logger.debug("Found locations, added to cache");
                            locations.Add(locConfig);
                        }
                        logger.debug("Checking for tile edits...");
                        if (locConfig.tiles != null && locConfig.tiles.Count > 0)
                        {
                            logger.debug("Found tile edits, added to cache");
                            tiles.Add(locConfig);
                        }
                        logger.debug("Checking for tile properties...");
                        if (locConfig.properties != null && locConfig.properties.Count > 0)
                        {
                            logger.debug("Found tile properties, added to cache");
                            properties.Add(locConfig);
                        }
                        logger.debug("Checking for warps...");
                        if (locConfig.warps != null && locConfig.warps.Count > 0)
                        {
                            logger.debug("Found warps, added to cache");
                            warps.Add(locConfig);
                        }
                        logger.debug("Manifest has been successfully processed");
                    }
                }
                logger.info("Patch data ready, waiting for savegame to be loaded.");
            }
            catch(Exception err)
            {
                if (!config.debugMode)
                    logger.error("Something major went wrong during loading, please switch to debug mode for details.");
                else
                {
                    logger.error("Something major went wrong during loading, The error message follows below.");
                    logger.error(err.Message);
                }
            }
        }
        static void Event_DayOfMonthChanged(object sender, EventArgs e)
        {
            try
            {
            logger.info("Patching stardew valley with the new and changed content...");
            if (overrides.Count > 0)
            {
                logger.debug("Overriding existing locations");
                foreach (LocationConfig cfg in overrides)
                    foreach (Dictionary<string, string> map in cfg.overrides)
                        overrideMap(cfg.ConfigDir, map);
            }
            else
                logger.debug("No existing locations to override");
            if (locations.Count > 0)
            {
                logger.debug("Loading new locations");
                foreach (LocationConfig cfg in locations)
                    foreach (Dictionary<string, string> map in cfg.locations)
                        addLocation(cfg.ConfigDir, map);
            }
            else
                logger.debug("No new locations to load");
            if (tiles.Count > 0)
            {
                logger.debug("Patching tiles");
                foreach (LocationConfig cfg in tiles)
                    foreach (List<string> tile in cfg.tiles)
                        editTile(tile);
            }
            else
                logger.debug("No tiles to patch");
            if (properties.Count > 0)
            {
                logger.debug("Setting tile properties");
                foreach (LocationConfig cfg in properties)
                    foreach (List<string> property in cfg.properties)
                        addProperty(property);
            }
            else
                logger.debug("No tile properties to set");
            if (warps.Count > 0)
            {
                logger.debug("Inserting warps");
                foreach (LocationConfig cfg in warps)
                    foreach (List<string> warp in cfg.warps)
                        addProperty(warp);
            }
            else
                logger.debug("No warps to insert");
            logger.info("Patching has finished");
            TimeEvents.DayOfMonthChanged -= Event_DayOfMonthChanged;
        }
            catch (Exception err)
            {
                if (!config.debugMode)
                    logger.error("Something major went wrong during patching, please switch to debug mode for details.");
                else
                {
                    logger.error("Something major went wrong during patching, The error message follows below.");
                    logger.error(err.Message);
                }
            }
        }
        static void overrideMap(string path,Dictionary<string,string> cfg)
        {
            if(!cfg.ContainsKey("name"))
            {
                logger.error("Cannot override a given location, the required `name` property is missing");
                return;
            }
            if (!cfg.ContainsKey("file"))
            {
                logger.error("Cannot override the "+ cfg["name"]+" map, the required `file` property is missing");
                return;
            }
            int index = LocationUtils.getLocationIndex(cfg["name"]);
            if (index == -1)
            {
                logger.error("Cannot override the "+ cfg["name"]+" map, it does not exist!");
                return;
            }
            string type = "Default";
            if (cfg.ContainsKey("type"))
            {
                type = cfg["type"];
            }
            Map map = getContentManager(path).Load<Map>(cfg["file"]);
            if (cfg.ContainsKey("tile"))
            {
                string uri = Path.Combine(Path.GetDirectoryName(path), cfg["tile"]);
                managerMatch.Add(uri, path);
                map.GetTileSheet("zcustom").ImageSource = uri;
            }
            Game1.locations[index].map=map;
        }
        static void addLocation(string path, Dictionary<string, string> cfg)
        {
            if (!cfg.ContainsKey("name"))
            {
                logger.error("Cannot add a given location, the required `name` property is missing");
                return;
            }
            if (!cfg.ContainsKey("file"))
            {
                logger.error("Cannot add the " + cfg["name"] + " map, the required `file` property is missing");
                return;
            }
            if(Game1.getLocationFromName(cfg["name"])!=null)
            {
                logger.error("Cannot add the " + cfg["name"] + " map, a location using this name already exists");
                return;
            }
            GameLocation location=new GameLocation(getContentManager(path).Load<Map>(cfg["file"]), cfg["name"]);
            if (cfg.ContainsKey("tile"))
            {
                string uri = Path.Combine(Path.GetDirectoryName(path), cfg["tile"]);
                managerMatch.Add(uri, path);
                location.map.GetTileSheet("zcustom").ImageSource = uri;
            }
            if (cfg.ContainsKey("outdoor"))
            {
                location.isOutdoors = Convert.ToBoolean(cfg["outdoor"]);
            }
            if (cfg.ContainsKey("farmable"))
            {
                location.isFarm = Convert.ToBoolean(cfg["farmable"]);
            }
            Game1.locations.Add(location);
        }
        static void editTile(List<string> tile)
        {
            if(tile.Count<5)
            {
                logger.error("Received a tile modification that did not conform to the format required");
                return;
            }
            string map = tile[0];
            string layer = tile[1];
            int posX = Convert.ToInt32(tile[2]);
            int posY = Convert.ToInt32(tile[3]);
            try
            {
                if (tile[4].Contains(','))
                    if (tile.Count==7)
                    {
                        int[] tileIndexes = tile[4].Split(',').Cast<int>().ToArray();
                        int tileSheet = Convert.ToInt32(tile[5]);
                        int interval = Convert.ToInt32(tile[6]);
                        LocationUtils.setDynamicTile(Game1.getLocationFromName(map), layer, posX, posY, tileIndexes, tileSheet,interval);
                    }
                    else if(tile.Count==6)
                    {
                        int[] tileIndexes = tile[4].Split(',').Cast<int>().ToArray();
                        int interval = Convert.ToInt32(tile[5]);
                        LocationUtils.setDynamicTile(Game1.getLocationFromName(map), layer, posX, posY, tileIndexes,interval);
                    }
                    else
                    {
                        logger.error("Received a dynamic tile modification that did not conform to the format required");
                        return;
                    }
                else
                    if (tile.Count==6)
                    {
                        int tileIndex = Convert.ToInt32(tile[4]);
                        int tileSheet = Convert.ToInt32(tile[5]);
                        LocationUtils.setStaticTile(Game1.getLocationFromName(map), layer, posX, posY, tileIndex,tileSheet);
                    }
                    else
                    {
                        int tileIndex = Convert.ToInt32(tile[4]);
                        LocationUtils.setStaticTile(Game1.getLocationFromName(map), layer, posX, posY, tileIndex);
                    }
            }
            catch
            {
                logger.error("The tile edit `"+tile.ToString()+"` could not be applied");
            }
        }
        static void addProperty(List<string> property)
        {
            if(property.Count<6)
            {
                logger.error("Received a tile property that did not conform to the format required");
                return;
            }
            try
            {
                GameLocation map = Game1.getLocationFromName(property[0]);
                LocationUtils.setTileProperty(map, property[1], Convert.ToInt32(property[2]), Convert.ToInt32(property[3]), property[4], property[5]);
            }
            catch
            {
                logger.error("The tile property `"+property.ToString()+"` could not be applied");
            }
        }
        static void addWarp(List<string> warp)
        {
            if (warp.Count < 6)
            {
                logger.error("Received a warp that did not conform to the format required");
                return;
            }
            try
            {
                GameLocation map = Game1.getLocationFromName(warp[0]);
                map.warps.Add(new Warp(Convert.ToInt32(warp[1]), Convert.ToInt32(warp[2]),warp[3], Convert.ToInt32(warp[4]), Convert.ToInt32(warp[5]),false));
            }
            catch
            {
                logger.error("The warp `" + warp.ToString() + "` could not be applied");
            }
        }
    }
    public class ContentManager : Microsoft.Xna.Framework.Content.ContentManager
    {
        public ContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
            : base(serviceProvider, rootDirectory)
        {
        }
        public override T Load<T>(string assetName)
        {
            if (typeof(T) == typeof(Texture2D) && AdvancedLocationLoader.managerMatch.ContainsKey(assetName))
            {
                return (T)(object)AdvancedLocationLoader.cManagers[AdvancedLocationLoader.managerMatch[assetName]].Load<Texture2D>(Path.GetFileName(assetName));
            }
            return base.Load<T>(assetName);
        }
    }
}
