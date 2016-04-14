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
    public class ShopExpander : Mod
    {
        static string modPath;
        public override void Entry(params object[] objects)
        {
            modPath = this.PathOnDisk;
            MenuEvents.MenuChanged += Event_MenuChanged;
            MenuEvents.MenuClosed += Event_MenuClosed;
            GameEvents.LoadContent += Event_LoadContent;
        }
        static void generateObject(string name, string description, string texture, int replacement, int stackAmount = 999)
        {
            SObject obj = new SObject();
            obj.Name = name;
            obj.CategoryName = "Resource";
            obj.CategoryColour = new Color(64, 102, 114);
            obj.Description = description;
            obj.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(Path.Combine(modPath, texture), FileMode.Open));
            obj.Stack = 1;
            obj.MaxStackSize = -1;
            AddedObjects.Add(name, obj);
            ReplacementStacks.Add(name, new StardewValley.Object(Vector2.Zero, replacement, stackAmount));
        }
        static private Dictionary<string, SObject> AddedObjects = new Dictionary<string, SObject>();
        static private Dictionary<string, Item> ReplacementStacks = new Dictionary<string, Item>();
        // Load our custom objects and their textures
        static void Event_LoadContent(object sender, EventArgs e)
        {
            generateObject("999 Wood", "A Sturdy, yet flexible plant material with a wide variety of uses.", "wood.png", 388);
            generateObject("999 Stone", "A common material with many uses in crafting and building.", "stone.png", 390);
        }
        static void Event_InventoryChanged(object send, EventArgs e)
        {
            for (int c = 0; c < Game1.player.Items.Count; c++)
            {
                if (Game1.player.Items[c] is SObject)
                {
                    Item item = ReplacementStacks[Game1.player.Items[c].Name].getOne();
                    item.Stack = ReplacementStacks[Game1.player.Items[c].Name].Stack;
                    Game1.player.Items[c] = item;
                }
            }
        }
        // When the menu closes, replace any stacks in the inventory with our own
        static void Event_MenuClosed(object send, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is ShopMenu && Game1.currentLocation.name == "ScienceHouse")
            {
                PlayerEvents.InventoryChanged -= Event_InventoryChanged;
            }
        }
        // Add a modified "stack" item to the shop
        static void addItem(SObject item)
        {
            forSale.Add(item);
            itemPriceAndStock.Add(item, new int[2] { ReplacementStacks[item.name].salePrice() * ReplacementStacks[item.name].Stack, int.MaxValue });
        }
        // Add a generic game-item to the shop
        static void addItem(Item item)
        {
            forSale.Add(item);
            itemPriceAndStock.Add(item, new int[2] { item.salePrice(), int.MaxValue });
        }
        static Dictionary<Item, int[]> itemPriceAndStock;
        static List<Item> forSale;
        // If the shop menu opens while in the science house, we are buying stuff from robin and need to modify her shop
        static void Event_MenuChanged(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu && Game1.currentLocation.name == "ScienceHouse")
            {
                // Register to inventory changes so we can immediately replace bought stacks
                PlayerEvents.InventoryChanged += Event_InventoryChanged;
                // Reflect the required fields to be able to edit a shops stock
                itemPriceAndStock = ReflectionUtils.getReflectedInstanceField(Game1.activeClickableMenu, "itemPriceAndStock");
                forSale = ReflectionUtils.getReflectedInstanceField(Game1.activeClickableMenu, "forSale");
                // Add our custom items to the shop
                foreach (string key in AddedObjects.Keys)
                    addItem(AddedObjects[key]);
                // Use reflection to set the changed values
                ReflectionUtils.setReflectedInstanceField(Game1.activeClickableMenu, "itemPriceAndStock", itemPriceAndStock);
                ReflectionUtils.setReflectedInstanceField(Game1.activeClickableMenu, "forSale", forSale);
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
    // My own extension to SMAPI's custom object, this lets it look as if shops are actually selling stacks.
    public class SObject : StardewModdingAPI.Inheritance.SObject
    {
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            if (isRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }

            if (Texture != null)
            {
                var targSize = (int)(64 * scaleSize * 0.9f);
                var midX = (int)(location.X + 32);
                var midY = (int)(location.Y + 32);

                var targX = midX - targSize / 2;
                var targY = midY - targSize / 2;

                spriteBatch.Draw(Texture, new Rectangle(targX, targY, targSize, targSize), null, new Color(255, 255, 255, transparency), 0, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
            if (drawStackNumber)
            {
                var _scale = 0.5f + scaleSize;
                Game1.drawWithBorder(Name.Split(' ')[0], Color.Black, Color.White, location + new Vector2(Game1.tileSize - Game1.tinyFont.MeasureString(string.Concat(Name.Split(' ')[0])).X * _scale, Game1.tileSize - (float)((double)Game1.tinyFont.MeasureString(string.Concat(Name.Split(' ')[0])).Y * 3.0f / 4.0f) * _scale), 0.0f, _scale, 1f, true);
            }
        }
        public override int maximumStackSize()
        {
            return -1;
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
            obj.HasBeenRegistered = HasBeenRegistered;
            obj.RegisteredId = RegisteredId;

            return obj;
        }
        public override Item getOne()
        {
            return Clone();
        }
    }
}
