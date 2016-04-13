using System;
using System.Collections.Generic;
using System.Reflection;

using StardewValley;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using xTile;
using xTile.Tiles;
using xTile.Dimensions;

/* Entoarox's Free Bus Ride Mod */

namespace FreeBusRide
{

    public class FreeBusRide : Mod
    {
        private static bool questionActive = false;
        public override void Entry(params object[] objects)
        {
            TimeEvents.DayOfMonthChanged += Event_DayOfMonthChanged;
            ControlEvents.MouseChanged += Events_MouseChanged;
            ControlEvents.ControllerButtonPressed += Events_ControllerButtonPressed;
        }

        static void Event_DayOfMonthChanged(object sender, EventArgs e)
        {
            if (Game1.locations.Count < 47) return;
            Game1.locations[24].setTileProperty(12, 8, "Buildings", "Action", "FreeBusTicket");
            //Game1.locations[24].setTileProperty(7,11, "Buildings", "Action", "FreeBusTicket");
            TimeEvents.DayOfMonthChanged -= Event_DayOfMonthChanged;
            List<Tile> tileArray = new List<Tile>();
            tileArray.Add(new Tile(1, 7, 11, -1));
            tileArray.Add(new Tile(2, 7, 10, -1));
            PatchMap(Game1.locations[24], tileArray);
        }

        static void Events_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (!Game1.hasLoadedGame) return;
            if (e.ButtonPressed == Buttons.A)
            {
                CheckForAction();
            }
        }

        static void Events_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if (!Game1.hasLoadedGame) return;
            if (e.NewState.RightButton == ButtonState.Pressed && e.PriorState.RightButton != ButtonState.Pressed)
            {
                CheckForAction();
            }
        }

        static void CheckForAction()
        {
            if (!Game1.player.UsingTool && !Game1.pickingTool && !Game1.menuUp && (!Game1.eventUp || Game1.currentLocation.currentEvent.playerControlSequence) && !Game1.nameSelectUp && Game1.numberOfSelectedItems == -1 && !Game1.fadeToBlack)
            {
                Vector2 grabTile = new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)) / (float)Game1.tileSize;
                if (!Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
                {
                    grabTile = Game1.player.GetGrabTile();
                }
                xTile.Tiles.Tile tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(new Location((int)grabTile.X * Game1.tileSize, (int)grabTile.Y * Game1.tileSize), Game1.viewport.Size);
                xTile.ObjectModel.PropertyValue propertyValue = null;
                if (tile != null)
                {
                    tile.Properties.TryGetValue("Action", out propertyValue);
                }
                if (propertyValue != null)
                {
                    if (propertyValue == "FreeBusTicket" && !questionActive)
                    {
                        questionActive = true;
                        if (Game1.player.mailReceived.Contains("ccVault"))
                        {
                            Game1.currentLocation.lastQuestionKey = "RideBusQuestion";
                            Game1.currentLocation.createQuestionDialogue("Ride the bus to Calico Desert?", new string[] { "Yes", "No" }, rideBusAnswer, null);
                        }
                        else
                            Game1.drawObjectDialogue("The bus to Calico Desert is out of service.");
                    }
                }
            }
        }
        private static void pamReachedBusDoor(Character c, GameLocation l)
        {
            Game1.changeMusicTrack("none");
            c.position.X = -10000f;
            Game1.playSound("stoneStep");
        }
        private static Character pamHook;
        private static void playerReachedBusDoor(Character c, GameLocation l)
        {
            pamHook.position.X = -10000f;
            typeof(StardewValley.Locations.BusStop).GetField("forceWarpTimer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Game1.currentLocation, 0);
            Game1.player.position.X = -10000f;
            Game1.playSound("stoneStep");
            (Game1.currentLocation as StardewValley.Locations.BusStop).busDriveOff();
            if (Game1.player.getMount() == null)
                return;
            Game1.player.getMount().farmerPassesThrough = false;
        }

        private static void rideBusAnswer(Farmer who, string whichAnswer)
        {
            questionActive = false;
            if (whichAnswer == "Yes")
            {
                NPC characterFromName = Game1.getCharacterFromName("Pam");
                if (Game1.currentLocation.characters.Contains(characterFromName) && characterFromName.getTileLocation().Equals(new Vector2(11f, 10f)))
                {
                    characterFromName.ignoreMultiplayerUpdates = true;
                    characterFromName.faceTowardFarmerTimer = 0;
                    characterFromName.faceTowardFarmer = false;
                    characterFromName.faceAwayFromFarmer = false;
                    characterFromName.movementPause = 1;
                    characterFromName.controller = new PathFindController((Character)characterFromName, (GameLocation)Game1.currentLocation, new Point(12, 9), 0, new PathFindController.endBehavior(pamReachedBusDoor));
                    characterFromName.forceUpdateTimer = 15000;
                    pamHook = characterFromName;
                    Game1.freezeControls = true;
                    Game1.viewportFreeze = true;
                    typeof(StardewValley.Locations.BusStop).GetField("forceWarpTimer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Game1.currentLocation, 8000);
                    Game1.player.controller = new PathFindController((Character)Game1.player, (GameLocation)Game1.currentLocation, new Point(12, 9), 0, new PathFindController.endBehavior(playerReachedBusDoor));
                    Game1.player.setRunning(true, false);
                    if (Game1.player.getMount() != null)
                        Game1.player.getMount().farmerPassesThrough = true;
                }
                else
                    Game1.drawObjectDialogue("Please return when a bus driver is present.");
            }
        }
        private static void PatchMap(GameLocation gl, List<Tile> tileArray)
        {
            foreach (Tile tile in tileArray)
            {
                if (tile.tileIndex < 0)
                {
                    gl.map.Layers[tile.l].Tiles[tile.x, tile.y] = null;
                    continue;
                }

                if (gl.map.Layers[tile.l].Tiles[tile.x, tile.y] != null)
                {
                    gl.map.Layers[tile.l].Tiles[tile.x, tile.y].TileIndex = tile.tileIndex;
                }
                else
                {
                    int width = 0;
                    bool done = false;

                    while (width <= gl.map.Layers[tile.l].LayerWidth && !done)
                    {
                        int height = 0;
                        while (height <= gl.map.Layers[tile.l].LayerHeight)
                        {
                            if (gl.map.Layers[tile.l].Tiles[width, height] != null)
                            {
                                if (gl.map.Layers[tile.l].Tiles[width, height].TileIndex != tile.tileIndex)
                                {
                                    height++;
                                    continue;
                                }
                                gl.map.Layers[tile.l].Tiles[tile.x, tile.y] = gl.map.Layers[tile.l].Tiles[width, height];
                                done = true;
                                break;
                            }
                            height++;
                        }
                        width++;
                    }
                }
            }
        }
    }
    //custom tile class for map edits
    public class Tile
    {
        public int l;
        public int x;
        public int y;
        public int tileIndex;
        public string layer;
        public int tileSheet = 1;

        public Tile(int l, int x, int y, int tileIndex)
        {
            this.l = l; this.x = x; this.y = y; this.tileIndex = tileIndex;
            setLayerName(l);
        }

        public Tile(int l, int x, int y, int tileIndex, int tileSheet)
        {
            this.l = l; this.x = x; this.y = y; this.tileIndex = tileIndex; this.tileSheet = tileSheet;
            setLayerName(l);
        }

        public Tile(string layer, int x, int y, int tileIndex, int tileSheet)
        {
            this.layer = layer; this.x = x; this.y = y; this.tileIndex = tileIndex; this.tileSheet = tileSheet;
            setLayerNumber(layer);
        }

        public Tile(string layer, int x, int y, int tileIndex)
        {
            this.layer = layer; this.x = x; this.y = y; this.tileIndex = tileIndex;
            setLayerNumber(layer);
        }
        public void setLayerNumber(string layer)
        {
            switch (layer)
            {
                case "Back":
                    l = 0;
                    break;
                case "Buildings":
                    l = 1;
                    break;
                case "Paths":
                    l = 2;
                    break;
                case "Front":
                    l = 3;
                    break;
                case "AlwaysFront":
                    l = 4;
                    break;
                default:
                    break;//goto case 0;
            }
        }
        public void setLayerName(int l) //Works for the majority of maps, requires adjustment for maps with less layers
        {
            switch (l)
            {
                case 0:
                    layer = "Back";
                    break;
                case 1:
                    layer = "Buildings";
                    break;
                case 2:
                    layer = "Paths";
                    break;
                case 3:
                    layer = "Front";
                    break;
                case 4:
                    layer = "AlwaysFront";
                    break;
                default:
                    break;//goto case 0;
            }
        }
    }
}