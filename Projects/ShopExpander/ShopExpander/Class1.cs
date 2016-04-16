using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using StardewValley;
using StardewValley.Menus;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShopExpander
{
    public class modConfig: Config
    {
        public override T GenerateDefaultConfig<T>()
        {
            debugMode = false;
            shops = new List<string>() { "ScienceHouse","SeedShop" };
            objects = new List<List<string>>() {
                new List<string>() { "ScienceHouse","388","111","!earthquake" },
                new List<string>() { "ScienceHouse","390","111","!earthquake" },

                new List<string>() { "ScienceHouse","388","333","!secondYear,earthquake" },
                new List<string>() { "ScienceHouse","390","333","!secondYear,earthquake" },

                new List<string>() { "ScienceHouse","388","999","secondYear" },
                new List<string>() { "ScienceHouse","390","999","secondYear" },
                new List<string>() { "ScienceHouse","709","111","secondYear" },
                new List<string>() { "ScienceHouse","771","111","secondYear" },

                new List<string>() { "SeedShop", "472", "10","!secondYear,spring" },
                new List<string>() { "SeedShop", "473", "10","!secondYear,spring" },
                new List<string>() { "SeedShop", "474", "10","!secondYear,spring" },
                new List<string>() { "SeedShop", "475", "10","!secondYear,spring" },
                new List<string>() { "SeedShop", "427", "10","!secondYear,spring" },
                new List<string>() { "SeedShop", "429", "10","!secondYear,spring" },
                new List<string>() { "SeedShop", "477", "10","!secondYear,spring" },

                new List<string>() { "SeedShop", "480", "10","!secondYear,summer" },
                new List<string>() { "SeedShop", "482", "10","!secondYear,summer" },
                new List<string>() { "SeedShop", "483", "10","!secondYear,summer" },
                new List<string>() { "SeedShop", "484", "10","!secondYear,summer" },
                new List<string>() { "SeedShop", "479", "10","!secondYear,summer" },
                new List<string>() { "SeedShop", "302", "10","!secondYear,summer" },
                new List<string>() { "SeedShop", "453", "10","!secondYear,summer" },
                new List<string>() { "SeedShop", "455", "10","!secondYear,summer" },

                new List<string>() { "SeedShop", "487", "10","!secondYear,fall" },
                new List<string>() { "SeedShop", "488", "10","!secondYear,fall" },
                new List<string>() { "SeedShop", "490", "10","!secondYear,fall" },
                new List<string>() { "SeedShop", "299", "10","!secondYear,fall" },
                new List<string>() { "SeedShop", "301", "10","!secondYear,fall" },
                new List<string>() { "SeedShop", "492", "10","!secondYear,fall" },
                new List<string>() { "SeedShop", "491", "10","!secondYear,fall" },
                new List<string>() { "SeedShop", "493", "10","!secondYear,fall" },
                new List<string>() { "SeedShop", "425", "10","!secondYear,fall" },

                new List<string>() { "SeedShop", "472", "50","secondYear,spring" },
                new List<string>() { "SeedShop", "473", "50","secondYear,spring" },
                new List<string>() { "SeedShop", "474", "50","secondYear,spring" },
                new List<string>() { "SeedShop", "475", "50","secondYear,spring" },
                new List<string>() { "SeedShop", "427", "50","secondYear,spring" },
                new List<string>() { "SeedShop", "429", "50","secondYear,spring" },
                new List<string>() { "SeedShop", "477", "50","secondYear,spring" },
                new List<string>() { "SeedShop", "476", "50","secondYear,spring" },

                new List<string>() { "SeedShop", "480", "50","secondYear,summer" },
                new List<string>() { "SeedShop", "482", "50","secondYear,summer" },
                new List<string>() { "SeedShop", "483", "50","secondYear,summer" },
                new List<string>() { "SeedShop", "484", "50","secondYear,summer" },
                new List<string>() { "SeedShop", "479", "50","secondYear,summer" },
                new List<string>() { "SeedShop", "302", "50","secondYear,summer" },
                new List<string>() { "SeedShop", "453", "50","secondYear,summer" },
                new List<string>() { "SeedShop", "455", "50","secondYear,summer" },
                new List<string>() { "SeedShop", "485", "50","secondYear,summer" },

                new List<string>() { "SeedShop", "487", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "488", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "490", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "299", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "301", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "492", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "491", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "493", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "425", "50","secondYear,fall" },
                new List<string>() { "SeedShop", "489", "50","secondYear,fall" },
            };
            return this as T;
        }
        public bool debugMode;
        public List<string> shops;
        public List<List<string>> objects;
    }
    public class ShopExpander : Mod
    {
        static string modPath;
        public static modConfig config;
        public override void Entry(params object[] objects)
        {
            modPath = this.PathOnDisk;
            MenuEvents.MenuChanged += Event_MenuChanged;
            MenuEvents.MenuClosed += Event_MenuClosed;
            GameEvents.LoadContent += Event_LoadContent;
            config = new modConfig().InitializeConfig(this.BaseConfigPath);
        }
        static void generateObject(string location, int replacement, int stackAmount, string requirements)
        {
            SObject obj = new SObject();
            StardewValley.Object stack = new StardewValley.Object(Vector2.Zero, replacement, stackAmount);
            obj.Name = stackAmount.ToString()+' '+stack.Name;
            obj.CategoryName = stack.getCategoryName();
            obj.CategoryColour = stack.getCategoryColor();
            obj.Description = stack.getDescription();
            obj.Stack = 1;
            obj.MaxStackSize = (int)Math.Floor(999d/stackAmount);
            obj.stackAmount = stackAmount;
            obj.targetedShop = location;
            obj.parentSheetIndex = stack.parentSheetIndex;
            obj.requirements = requirements;
            logUtils.info("Object registered: " + obj.Name + ':' + replacement + '@' + location + '[' + obj.maximumStackSize() + ',' + obj.stackAmount + "]#"+requirements);
            AddedObjects.Add(obj.Name, obj);
            ReplacementStacks.Add(obj.Name, stack);
        }
        static private Dictionary<string, SObject> AddedObjects = new Dictionary<string, SObject>();
        static private Dictionary<string, Item> ReplacementStacks = new Dictionary<string, Item>();
        // Load our custom objects and their textures
        static void Event_LoadContent(object sender, EventArgs e)
        {
            affectedShops = config.shops;
            foreach(List<string> obj in config.objects)
            {
                try
                {
                    generateObject(obj[0], Convert.ToInt32(obj[1]), Convert.ToInt32(obj[2]), obj[3]);
                }
                catch(Exception err)
                {
                    logUtils.error("Object failed to generate: "+obj.ToString());
                    logUtils.debug("Error message: "+err.Message);
                }
            }

        }
        // If the inventory changes while this even is hooked, we need to check if any SObject instances are in it, so we can replace them
        static void Event_InventoryChanged(object send, EventArgs e)
        {
            for (int c = 0; c < Game1.player.Items.Count; c++)
            {
                if (Game1.player.Items[c] is SObject)
                {
                    logUtils.debug("Replacing Object:" + Game1.player.Items[c].Name + ':' + Game1.player.Items[c].maximumStackSize());
                    Item item = ReplacementStacks[Game1.player.Items[c].Name].getOne();
                    item.Stack = (Game1.player.Items[c] as SObject).getStackNumber();
                    Game1.player.Items[c] = item;
                }
            }
        }
        // When the menu closes, remove the hook for the inventory changed event
        static void Event_MenuClosed(object send, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is ShopMenu && affectedShops.Contains(Game1.currentLocation.name))
            {
                PlayerEvents.InventoryChanged -= Event_InventoryChanged;
            }
        }
        // Add a modified "stack" item to the shop
        static List<string> weekdays = new List<string> {"sunday","monday","tuesday","wednesday","thursday","friday","saturday" };
        static void addItem(SObject item, string location)
        {
            logUtils.debug("Checking if object should be added: " + item.Name + ':' + item.maximumStackSize());
            // Check that makes sure only the items that the current shop is supposed to sell are added
            if (location != item.targetedShop)
                return;
            logUtils.debug("Object passed location check");
            string[] requirements = item.requirements.Split(',');
            // Each item can only have 4 distinct requirements, if it has more we refuse it
            if(requirements.GetLength(0)>4)
            {
                logUtils.error("Object has to many requirements: " + item.requirements);
                return;
            }
            // Check if all the requirements this item has are matched
            foreach (string req in requirements)
            {
                switch (req)
                {
                    case "secondYear":
                        if (Game1.year < 2)
                            return;
                        break;
                    case "!secondYear":
                        if (Game1.year > 1)
                            return;
                        break;
                    case "earthquake":
                        if (Game1.stats.daysPlayed < 32)
                            return;
                        break;
                    case "!earthquake":
                        if (Game1.stats.daysPlayed > 31)
                            return;
                        break;
                    case "married":
                        if (Game1.player.spouse == null)
                            return;
                        break;
                    case "!married":
                        if (Game1.player.spouse != null)
                            return;
                        break;
                    case "spring":
                    case "summer":
                    case "fall":
                    case "winter":
                        if (!Game1.currentSeason.Equals(req))
                            return;
                        break;
                    case "!spring":
                    case "!summer":
                    case "!fall":
                    case "!winter":
                        if (Game1.currentSeason.Equals(req))
                            return;
                        break;
                    case "sunday":
                    case "monday":
                    case "tuesday":
                    case "wednesday":
                    case "thursday":
                    case "friday":
                    case "saturday":
                        if(weekdays[Game1.dayOfMonth%7]!=req)
                            return;
                        break;
                    case "!sunday":
                    case "!monday":
                    case "!tuesday":
                    case "!wednesday":
                    case "!thursday":
                    case "!friday":
                    case "!saturday":
                        if (weekdays[Game1.dayOfMonth % 7] != req.Substring(1))
                            return;
                        break;
                    case "marriedAlex":
                    case "marriedElliot":
                    case "marriedHarvey":
                    case "marriedSam":
                    case "marriedSebastian":
                    case "marriedAbigail":
                    case "marriedHaley":
                    case "marriedLeah":
                    case "marriedMaru":
                    case "marriedPenny":
                        if (Game1.player.spouse == null || !Game1.player.spouse.Equals(req.Substring(7)))
                            return;
                        break;
                    case "!marriedAlex":
                    case "!marriedElliot":
                    case "!marriedHarvey":
                    case "!marriedSam":
                    case "!marriedSebastian":
                    case "!marriedAbigail":
                    case "!marriedHaley":
                    case "!marriedLeah":
                    case "!marriedMaru":
                    case "!marriedPenny":
                        if (Game1.player.spouse != null && Game1.player.spouse.Equals(req.Substring(7)))
                            return;
                        break;
                }
            }
            logUtils.debug("Object passed requirement check and is being added to the current shop");
            if(item.stackAmount==1)
            {
                logUtils.debug("Detected single-item stack, inserting stardew item");
                forSale.Add(ReplacementStacks[item.Name].getOne());
                itemPriceAndStock.Add(ReplacementStacks[item.Name].getOne(), new int[2] { ReplacementStacks[item.name].salePrice(), int.MaxValue });
            }
            else
            {
                logUtils.debug("Detected multi-item stack, inserting stack item");
                forSale.Add(item);
                itemPriceAndStock.Add(item, new int[2] { ReplacementStacks[item.name].salePrice() * ReplacementStacks[item.name].Stack, int.MaxValue });
            }
        }
        static void addSimpleItem(Item item)
        {
            forSale.Add(item);
            itemPriceAndStock.Add(item, new int[2] { item.salePrice(), int.MaxValue });
        }
        static Dictionary<Item, int[]> itemPriceAndStock;
        static List<Item> forSale;
        static List<string> affectedShops;
        // If the shop menu opens while in the science house, we are buying stuff from robin and need to modify her shop
        static void Event_MenuChanged(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu && affectedShops.Contains(Game1.currentLocation.name))
            {
                logUtils.debug("Shop on the affected shops list opened");
                // Register to inventory changes so we can immediately replace bought stacks
                PlayerEvents.InventoryChanged += Event_InventoryChanged;
                // Reflect the required fields to be able to edit a shops stock
                itemPriceAndStock = ReflectionUtils.getReflectedInstanceField(Game1.activeClickableMenu, "itemPriceAndStock");
                forSale = ReflectionUtils.getReflectedInstanceField(Game1.activeClickableMenu, "forSale");
                // Add our custom items to the shop
                foreach (string key in AddedObjects.Keys)
                    addItem(AddedObjects[key], Game1.currentLocation.name);
                // Use reflection to set the changed values
                ReflectionUtils.setReflectedInstanceField(Game1.activeClickableMenu as ShopMenu, "itemPriceAndStock", itemPriceAndStock);
                ReflectionUtils.setReflectedInstanceField(Game1.activeClickableMenu as ShopMenu, "forSale", forSale);
            }
        }
    }
    // Utility class to make using reflection a bit easier for me to do
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
    public class logUtils
    {
        public static void debug(string message)
        {
            if(ShopExpander.config.debugMode)
                Log.AsyncO("[ShopExpander/DEBUG] "+message);
        }
        public static void info(string message)
        {
            Log.AsyncC("[ShopExpander/INFO] " + message);
        }
        public static void error(string message)
        {
            Log.AsyncR("[ShopExpander/ERROR] " + message);
        }
    }
    // My own extension to SMAPI's custom object, this lets it look as if shops are actually selling stacks even though they technically are not.
    public class SObject : StardewModdingAPI.Inheritance.SObject
    {
        public string targetedShop;
        public int stackAmount;
        public new int MaxStackSize;
        public string requirements;
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            if(this.isRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }
            SpriteBatch spriteBatch1 = spriteBatch;
            Texture2D texture = Game1.shadowTexture;
            Vector2 vector2_1 = location;
            double num1 = (double)(Game1.tileSize / 2);
            int num2 = Game1.tileSize * 3 / 4;
            int num3 = Game1.pixelZoom;
            double num4 = (double)num2;
            Vector2 vector2_2 = new Vector2((float)num1, (float)num4);
            Vector2 position = vector2_1 + vector2_2;
            Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds);
            Color color = Color.White * 0.5f;
            double num5 = 0.0;
            Vector2 origin = new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y);
            double num6 = 3.0;
            int num7 = 0;
            double num8 = (double)layerDepth - 9.99999974737875E-05;
            spriteBatch1.Draw(texture, position, sourceRectangle, color, (float)num5, origin, (float)num6, (SpriteEffects)num7, (float)num8);
            spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize), (float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize)), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.parentSheetIndex, 16, 16)), Color.White * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);
            var _scale = 0.5f + scaleSize;
            Game1.drawWithBorder(getStackNumber().ToString(), Color.Black, Color.White, location + new Vector2(Game1.tileSize - Game1.tinyFont.MeasureString(getStackNumber().ToString()).X * _scale, Game1.tileSize - (float)((double)Game1.tinyFont.MeasureString(getStackNumber().ToString()).Y * 3.0f / 4.0f) * _scale), 0.0f, _scale, 1f, true);
            if (!this.isRecipe)
                return;
            spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((float)(Game1.tileSize / 4), (float)(Game1.tileSize / 4)), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)((double)Game1.pixelZoom * 3.0 / 4.0), SpriteEffects.None, layerDepth + 0.0001f);
        }
        public int getStackNumber()
        {
            return (stack*stackAmount);
        }
        public override int maximumStackSize()
        {
            return MaxStackSize;
        }
        public new bool canStackWith(Item obj)
        {
            return obj.canStackWith(this) && obj is SObject&&(Stack+obj.Stack)<maximumStackSize();
        }
        public new SObject Clone()
        {
            var obj = new SObject();

            obj.Name = Name;
            obj.CategoryName = CategoryName;
            obj.Description = Description;
            obj.Texture = Texture;
            obj.IsPassable = IsPassable;
            obj.IsPlaceable = IsPlaceable;
            obj.quality = quality;
            obj.scale = scale;
            obj.isSpawnedObject = isSpawnedObject;
            obj.isRecipe = isRecipe;
            obj.questItem = questItem;
            obj.stack = 1;
            obj.parentSheetIndex = parentSheetIndex;
            obj.MaxStackSize = maximumStackSize();

            obj.stackAmount = stackAmount;
            obj.targetedShop = targetedShop;
            obj.requirements = requirements;

            return obj;
        }
        public override Item getOne()
        {
            return Clone();
        }
    }
}
