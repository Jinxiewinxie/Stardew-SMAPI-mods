using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Inheritance;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using xTile;

namespace ShopExpander
{
    public class SObject : StardewValley.Object
    {
        public SObject()
        {
            name = "Modded Item Name";
            Description = "Modded Item Description";
            CategoryName = "Modded Item Category";
            Category = 4163;
            CategoryColour = Color.White;
            IsPassable = false;
            IsPlaceable = false;
            boundingBox = new Rectangle(0, 0, 64, 64);
            MaxStackSize = 999;

            type = "interactive";
        }

        public override string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description { get; set; }
        public Texture2D Texture { get; set; }
        public string CategoryName { get; set; }
        public Color CategoryColour { get; set; }
        public bool IsPassable { get; set; }
        public bool IsPlaceable { get; set; }
        public bool HasBeenRegistered { get; set; }
        public int RegisteredId { get; set; }

        public int ReplacementStack;
        public int StackAmount;

        public int MaxStackSize { get; set; }

        public bool WallMounted { get; set; }
        public Vector2 DrawPosition { get; set; }

        public bool FlaggedForPickup { get; set; }

        public Vector2 CurrentMouse { get; protected set; }

        public Vector2 PlacedAt { get; protected set; }

        public override int Stack
        {
            get { return stack; }
            set { stack = value; }
        }

        public override string getDescription()
        {
            return Description;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (Texture != null)
            {
                spriteBatch.Draw(Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize + Game1.tileSize / 2 + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), y * Game1.tileSize + Game1.tileSize / 2 + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0))), Game1.currentLocation.getSourceRectForObject(ParentSheetIndex), Color.White * alpha, 0f, new Vector2(8f, 8f), scale.Y > 1f ? getScale().Y : Game1.pixelZoom, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (isPassable() ? getBoundingBox(new Vector2(x, y)).Top : getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
            }
        }

        public new void drawAsProp(SpriteBatch b)
        {
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            Log.Debug("THIS DRAW FUNCTION IS NOT IMPLEMENTED I WANT TO KNOW WHERE IT IS CALLED");
        }

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

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (Texture != null)
            {
                var targSize = 64;
                var midX = (int)(objectPosition.X + 32);
                var midY = (int)(objectPosition.Y + 32);

                var targX = midX - targSize / 2;
                var targY = midY - targSize / 2;

                spriteBatch.Draw(Texture, new Rectangle(targX, targY, targSize, targSize), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, (f.getStandingY() + 2) / 10000f);
            }
        }

        public override Color getCategoryColor()
        {
            return CategoryColour;
        }

        public override string getCategoryName()
        {
            if (string.IsNullOrEmpty(CategoryName))
                return "Modded Item";
            return CategoryName;
        }

        public override bool isPassable()
        {
            return IsPassable;
        }

        public override bool isPlaceable()
        {
            return IsPlaceable;
        }

        public override int maximumStackSize()
        {
            return -1;
        }

        public SObject Clone()
        {
            var toRet = new SObject();

            toRet.Name = Name;
            toRet.CategoryName = CategoryName;
            toRet.Description = Description;
            toRet.Texture = Texture;
            toRet.IsPassable = IsPassable;
            toRet.IsPlaceable = IsPlaceable;
            toRet.quality = quality;
            toRet.scale = scale;
            toRet.isSpawnedObject = isSpawnedObject;
            toRet.isRecipe = isRecipe;
            toRet.questItem = questItem;
            toRet.stack = 1;
            toRet.HasBeenRegistered = HasBeenRegistered;
            toRet.RegisteredId = RegisteredId;

            return toRet;
        }

        public override Item getOne()
        {
            return Clone();
        }

        public override void actionWhenBeingHeld(Farmer who)
        {
            var x = Game1.oldMouseState.X + Game1.viewport.X;
            var y = Game1.oldMouseState.Y + Game1.viewport.Y;

            x = x / Game1.tileSize;
            y = y / Game1.tileSize;

            CurrentMouse = new Vector2(x, y);
            //Program.LogDebug(canBePlacedHere(Game1.currentLocation, CurrentMouse));
            base.actionWhenBeingHeld(who);
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            //Program.LogDebug(CurrentMouse.ToString().Replace("{", "").Replace("}", ""));
            if (!l.objects.ContainsKey(tile))
                return true;

            return false;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            if (Game1.didPlayerJustRightClick())
                return false;

            x = x / Game1.tileSize;
            y = y / Game1.tileSize;

            //Program.LogDebug(x + " - " + y);
            //Console.ReadKey();

            var key = new Vector2(x, y);

            if (!canBePlacedHere(location, key))
                return false;

            var s = Clone();

            s.PlacedAt = key;
            s.boundingBox = new Rectangle(x / Game1.tileSize * Game1.tileSize, y / Game1.tileSize * Game1.tileSize, boundingBox.Width, boundingBox.Height);

            location.objects.Add(key, s);
            Log.Async($"{GetHashCode()} - {s.GetHashCode()}");

            return true;
        }

        public override void actionOnPlayerEntry()
        {
            //base.actionOnPlayerEntry();
        }

        public override void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
        {
            if (canBePlacedHere(location, CurrentMouse))
            {
                var targSize = Game1.tileSize;

                var x = Game1.oldMouseState.X + Game1.viewport.X;
                var y = Game1.oldMouseState.Y + Game1.viewport.Y;
                spriteBatch.Draw(Game1.mouseCursors, new Vector2(x / Game1.tileSize * Game1.tileSize - Game1.viewport.X, y / Game1.tileSize * Game1.tileSize - Game1.viewport.Y), new Rectangle(Utility.playerCanPlaceItemHere(location, this, x, y, Game1.player) ? 194 : 210, 388, 16, 16), Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.01f);
            }
        }
    }
    public class ShopExpander: Mod
    {
        static string modPath;
        public override void Entry(params object[] objects)
        {
            modPath = this.PathOnDisk;
            MenuEvents.MenuChanged += Event_MenuChanged;
            MenuEvents.MenuClosed += Event_MenuClosed;
            GameEvents.LoadContent += Event_LoadContent;
        }
        static void generateObject(string name,string description, string texture, int replacement,int stackAmount=999)
        {
            SObject obj = new SObject();
            obj.Name = name;
            obj.CategoryName = "Resource";
            obj.CategoryColour = new Color(64, 102, 114);
            obj.Description = description;
            obj.Texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, new FileStream(Path.Combine(modPath, texture), FileMode.Open));
            obj.Stack = 1;
            obj.MaxStackSize = -1;
            AddedObjects.Add(name,obj);
            ReplacementStacks.Add(name, new StardewValley.Object(Vector2.Zero, replacement, stackAmount));
        }
        static private Dictionary<string, SObject> AddedObjects=new Dictionary<string, SObject>();
        static private Dictionary<string, Item> ReplacementStacks=new Dictionary<string, Item>();
        // Load our custom objects and their textures
        static void Event_LoadContent(object sender,EventArgs e)
        {
            generateObject("999 Wood", "A Sturdy, yet flexible plant material with a wide variety of uses.","wood.png", 388);
            generateObject("999 Stone", "A common material with many uses in crafting and building.","stone.png", 390);
        }
        static void Event_InventoryChanged(object send,EventArgs e)
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
            itemPriceAndStock.Add(item, new int[2] { ReplacementStacks[item.name].salePrice()* ReplacementStacks[item.name].Stack, int.MaxValue });
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
            if(Game1.activeClickableMenu is ShopMenu&&Game1.currentLocation.name=="ScienceHouse")
            {
                // Register to inventory changes so we can immediately replace bought stacks
                PlayerEvents.InventoryChanged += Event_InventoryChanged;
                // Reflect the required fields to be able to edit a shops stock
                itemPriceAndStock =ReflectionUtils.getReflectedInstanceField((Game1.activeClickableMenu as ShopMenu), "itemPriceAndStock");
                forSale=ReflectionUtils.getReflectedInstanceField((Game1.activeClickableMenu as ShopMenu), "forSale");
                // Add our custom items to the shop
                foreach (string key in AddedObjects.Keys)
                    addItem(AddedObjects[key]);
                // Use reflection to set the changed values
                ReflectionUtils.setReflectedInstanceField((Game1.activeClickableMenu as ShopMenu), "itemPriceAndStock", itemPriceAndStock);
                ReflectionUtils.setReflectedInstanceField((Game1.activeClickableMenu as ShopMenu), "forSale", forSale);
            }
        }
    }
    // Utility class to make using reflection a bit easier for me to do
    public class ReflectionUtils
    {
        public static dynamic getReflectedInstanceField(object target,string field)
        {
            return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }
        public static void setReflectedInstanceField(object target,string field,dynamic value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
