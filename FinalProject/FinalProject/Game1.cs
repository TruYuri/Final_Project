﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;

namespace FinalProject
{
    public enum MessageType { UpdatePosition, WeaponFired, EndGame, StartGame, RejoinLobby, RestartGame, UpdateRemotePlayer, Kill, Respawn, CreateOrb }
    public enum GameState
    {
        SignIn, FindSession,
        CreateSession, Start, InGame, GameOver
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static ContentManager ContentManager;
        public static GraphicsDevice GraphicsDeviceRef;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Song song;
        Camera camera;
        Player localPlayer;
        List<Player> players;
        static int nPlayers = 3;
        Map map;
        Interface iface;
        List<GameObject> powerups;

        number<float> RestartTime;

        NetworkSession networkSession;
        PacketWriter packetWriter;
        PacketReader packetReader;
        GameState currentGameState;

        RenderTarget2D renderTarget;
        public static int xRes = 1024; // 1200; //1440; // 960;
        public static int yRes = 640; // 750; // 900; //600;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferHeight = yRes;
            graphics.PreferredBackBufferWidth = xRes;

            currentGameState = GameState.SignIn;
            packetWriter = new PacketWriter();
            packetReader = new PacketReader();
            RestartTime = new number<float>();
            players = new List<Player>();
        }

        protected override void Initialize()
        {
            Components.Add(new GamerServicesComponent(this));
            base.Initialize();
        }

        protected override void LoadContent() 
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            iface = new Interface();
            ContentManager = Content;
            GraphicsDeviceRef = GraphicsDevice;
            font = Content.Load<SpriteFont>("font");
            renderTarget = new RenderTarget2D(GraphicsDevice, xRes, yRes, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            if (this.IsActive)
            {
                // Run different methods based on game state
                switch (currentGameState)
                {
                    case GameState.SignIn:
                        Update_SignIn();
                        break;
                    case GameState.FindSession:
                        Update_FindSession();
                        break;
                    case GameState.CreateSession:
                        Update_CreateSession();
                        break;
                    case GameState.Start:
                        Update_Start(gameTime);
                        break;
                    case GameState.InGame:
                        Update_InGame(gameTime);
                        break;
                    case GameState.GameOver:
                        Update_GameOver(gameTime);
                        break;
                }
            }
            // Update the network session 
	        if (networkSession != null)
                networkSession.Update();

            base.Update(gameTime);
        }

        protected void StartGame()
        {
            // Set game state to InGame
            currentGameState = GameState.InGame;
            Interface.LoadGameplayInterface(players, camera);
            //GameObjectManager.Instance.Reset();
            
            // Any other things that need to be set up
            //for beginning a game
            //Starting audio, resetting values, etc.
        }

        protected void EndGame(string winner)
        {
            //Perform whatever actions are to occur
            //when a game ends. Stop music, play
            //A certain sound effect, etc.
            RestartTime.n = 20.0f;
            currentGameState = GameState.GameOver;
            Interface.LoadGameoverInterface(winner, RestartTime);
        }

        protected void ProcessIncomingData(GameTime gameTime)
        {
            // Process incoming data
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];

            // While there are packets to be read...
            while (localGamer.IsDataAvailable)
            {
                // Get the packet and info on sender
                NetworkGamer sender;
                localGamer.ReceiveData(packetReader, out sender);

                // Ignore the packet if you sent it
                if (!sender.IsLocal)
                {
                    // Read messagetype from start of packet and call appropriate method
                    MessageType messageType = (MessageType)packetReader.ReadInt32();
                    switch (messageType)
                    {
                        case MessageType.EndGame:
                            EndGame(packetReader.ReadString());
                            break;
                        case MessageType.StartGame:
                            StartGame();
                            break;
                        case MessageType.RejoinLobby:
                            currentGameState = GameState.Start;
                            break;
                        case MessageType.RestartGame:
                            StartGame();
                            break;
                        case MessageType.UpdateRemotePlayer:
                            UpdateRemotePlayer(gameTime);
                            break;
                        case MessageType.WeaponFired:
                            WeaponFired();
                            break;
                        case MessageType.Kill:
                            Kill();
                            break;
                        case MessageType.Respawn:
                            Respawn();
                            break;
                        case MessageType.CreateOrb:    
                            var type = packetReader.ReadInt32();
                            var pos = packetReader.ReadVector3();
                            string name = (type == 0 ? "health_orb" : "shield_orb");
                            var orb = new GameObject(new BasicModel(Content.Load<Model>(name), pos), true, name, "");
                            orb.world = Matrix.CreateWorld(pos, Vector3.Forward, Vector3.Up);
                            break;
                        //Any other actions for specific messages

                    }
                }
            }
        }

        protected void Respawn()
        {
            string name = packetReader.ReadString();
            foreach (var player in players)
            {
                if (player.name == name)
                {
                    player.Initialize();
                }
            }
        }

        protected void Kill()
        {
            // Get the other (non-local) player
            PlayerState killType = (PlayerState)packetReader.ReadInt32();
            string name;
            string name2;

            switch (killType)
            {
                case PlayerState.CrashedVehicle:
                    name = packetReader.ReadString();
                    name2 = packetReader.ReadString();
                    Player a = FindPlayer(name);
                    Player b = FindPlayer(name2);
                    a.collider = name2;
                    b.collider = name;
                    a.Kill(5.0f, PlayerState.CrashedVehicle);
                    b.Kill(5.0f, PlayerState.CrashedVehicle);
                    break;
                case PlayerState.CrashedGround:
                    name = packetReader.ReadString();
                    FindAndKill(name, PlayerState.CrashedGround);
                    break;
                case PlayerState.Killed:
                    name = packetReader.ReadString();
                    FindAndKill(name, PlayerState.Killed);
                    break;
            }
        }

        private Player FindPlayer(string name)
        {
            if (name == localPlayer.name)
                return localPlayer;
            foreach (var player in players)
            {
                if (player.name == name)
                {
                    return player;
                }
            }

            return null;
        }

        private void FindAndKill(string name, PlayerState reason)
        {
            Player player = FindPlayer(name);
            player.Kill(5.0f, reason);
        }

        protected void WeaponFired()
        {
            var name = packetReader.ReadString();
            var weapon = packetReader.ReadString();

            foreach (var player in players)
            {
                if (player.name == name)
                {
                    player.FireWeapon(weapon);
                    break;
                }
            }
        }

        protected void WireUpEvents()
        {
            // Wire up events for gamers joining and leaving, defines what to do when a gamer
            //Joins or leaves the session

            networkSession.GamerJoined += new EventHandler<GamerJoinedEventArgs>(GamerJoined);
            networkSession.GamerLeft += new EventHandler<GamerLeftEventArgs>(GamerLeft);
        }

        void GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            // Gamer joined. Set the tag for the gamer to a new UserControlledObject.
            // These Tags are going to be your local representation of remote players

            if(e.Gamer.IsLocal)
                e.Gamer.Tag = CreateLocalPlayer(e.Gamer.Gamertag);
            else
                e.Gamer.Tag = CreateRemotePlayer(e.Gamer.Gamertag);

            if (e.Gamer.IsHost)
                currentGameState = GameState.Start;
        }

        private void InitializeLevel()
        {
            map = new Map(new Vector2(5, 5));
            
            if(camera == null)
                camera = new Camera(this, map);

            var gom = GameObjectManager.Instance;
            var am = AudioManager.Instance;
            Components.Clear();
            if(gom == null)
                gom = new GameObjectManager(this, camera);
            if (am == null)
                am = new AudioManager(this);
            Components.Add(gom);
            Components.Add(am);

            if(song == null)
                song = Content.Load<Song>(@"Audio/Yerusholayim");
            else
                am.StopSong();
            am.PlaySong(song);
            #region Map

            Terrain x0y0 = new Terrain(@"Map\Final00", @"Map\Final00_Texture", 0, 0);
            Terrain x0y1 = new Terrain(@"Map\Final01", @"Map\Final01_Texture", 0, 1);
            Terrain x0y2 = new Terrain(@"Map\Final02", @"Map\Final02_Texture", 0, 2);
            Terrain x0y3 = new Terrain(@"Map\Final03", @"Map\Final03_Texture", 0, 3);
            Terrain x0y4 = new Terrain(@"Map\Final04", @"Map\Final04_Texture", 0, 4);

            Terrain x1y0 = new Terrain(@"Map\Final10", @"Map\Final10_Texture", 1, 0);
            Terrain x1y1 = new Terrain(@"Map\Final11", @"Map\Final11_Texture", 1, 1);
            Terrain x1y2 = new Terrain(@"Map\Final12", @"Map\Final12_Texture", 1, 2);
            Terrain x1y3 = new Terrain(@"Map\Final13", @"Map\Final13_Texture", 1, 3);
            Terrain x1y4 = new Terrain(@"Map\Final14", @"Map\Final14_Texture", 1, 4);

            Terrain x2y0 = new Terrain(@"Map\Final20", @"Map\Final20_Texture", 2, 0);
            Terrain x2y1 = new Terrain(@"Map\Final21", @"Map\Final21_Texture", 2, 1);
            Terrain x2y2 = new Terrain(@"Map\Final22", @"Map\Final22_Texture", 2, 2);
            Terrain x2y3 = new Terrain(@"Map\Final23", @"Map\Final23_Texture", 2, 3);
            Terrain x2y4 = new Terrain(@"Map\Final24", @"Map\Final24_Texture", 2, 4);

            Terrain x3y0 = new Terrain(@"Map\Final30", @"Map\Final30_Texture", 3, 0);
            Terrain x3y1 = new Terrain(@"Map\Final31", @"Map\Final31_Texture", 3, 1);
            Terrain x3y2 = new Terrain(@"Map\Final32", @"Map\Final32_Texture", 3, 2);
            Terrain x3y3 = new Terrain(@"Map\Final33", @"Map\Final33_Texture", 3, 3);
            Terrain x3y4 = new Terrain(@"Map\Final34", @"Map\Final34_Texture", 3, 4);

            Terrain x4y0 = new Terrain(@"Map\Final40", @"Map\Final40_Texture", 4, 0);
            Terrain x4y1 = new Terrain(@"Map\Final41", @"Map\Final41_Texture", 4, 1);
            Terrain x4y2 = new Terrain(@"Map\Final42", @"Map\Final42_Texture", 4, 2);
            Terrain x4y3 = new Terrain(@"Map\Final43", @"Map\Final43_Texture", 4, 3);
            Terrain x4y4 = new Terrain(@"Map\Final44", @"Map\Final44_Texture", 4, 4);

            map.terrainPieces.Add(x0y0);
            map.terrainPieces.Add(x0y1);
            map.terrainPieces.Add(x0y2);
            map.terrainPieces.Add(x0y3);
            map.terrainPieces.Add(x0y4);

            map.terrainPieces.Add(x1y0);
            map.terrainPieces.Add(x1y1);
            map.terrainPieces.Add(x1y2);
            map.terrainPieces.Add(x1y3);
            map.terrainPieces.Add(x1y4);

            map.terrainPieces.Add(x2y0);
            map.terrainPieces.Add(x2y1);
            map.terrainPieces.Add(x2y2);
            map.terrainPieces.Add(x2y3);
            map.terrainPieces.Add(x2y4);

            map.terrainPieces.Add(x3y0);
            map.terrainPieces.Add(x3y1);
            map.terrainPieces.Add(x3y2);
            map.terrainPieces.Add(x3y3);
            map.terrainPieces.Add(x3y4);

            map.terrainPieces.Add(x4y0);
            map.terrainPieces.Add(x4y1);
            map.terrainPieces.Add(x4y2);
            map.terrainPieces.Add(x4y3);
            map.terrainPieces.Add(x4y4);

            if (powerups == null)
                powerups = new List<GameObject>();
            foreach(var powerup in powerups)
            {
                GameObjectManager.Instance.Delete(powerup);
            }

            map.BottomLeft = x0y0;
            #endregion

            map.Load();

            var pos = map.CreateRandomSpawnAtHeight(600, new Random());
            var c = map.Center(600);
            camera.PlaceCamera(pos, c - pos, Vector3.Up);
        }

        private object CreateLocalPlayer(string name)
        {
            InitializeLevel();

            localPlayer = new Player(this, camera, map, true, name);
            localPlayer.Initialize();

            return localPlayer;
        }

        private object CreateRemotePlayer(string name)
        {
            InitializeLevel();

            Player remote = new Player(this, camera, map, false, name);
            remote.Initialize();

            players.Add(remote);
            return remote;
        }

        protected void UpdateLocalPlayer(GameTime gameTime)
        {
            // Get local player
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];

            // Get the local player's sprite
            //Player local = (Player)localGamer.Tag;

            // Call the local's Update method, which will process user input
            // for movement and update the animation frame
            //Boolean used to inform the Update function that the local player is calling update,          //therefore update based on local input
            var old = localPlayer.status;
            localPlayer.Update(gameTime);

            if(localPlayer.status == PlayerState.CrashedVehicle)
            {
                FindAndKill(localPlayer.collider, PlayerState.CrashedVehicle);
            }

            // Send data to other player
            //foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                //if(!gamer.IsLocal)
                {
                    if (localPlayer.alive)
                    {
                        // Send message to other player with message tag and new position of local player
                        packetWriter.Write((int)MessageType.UpdateRemotePlayer);
                        packetWriter.Write(localPlayer.name);
                        packetWriter.Write(localPlayer.Position);
                        packetWriter.Write(localPlayer.Velocity);
                        packetWriter.Write(localPlayer.health);
                        packetWriter.Write(localPlayer.shield);
                        localGamer.SendData(packetWriter, SendDataOptions.InOrder);

                        switch(localPlayer.status)
                        {
                            case PlayerState.WeaponFired:
                                packetWriter.Write((int)MessageType.WeaponFired);
                                packetWriter.Write(localPlayer.name);
                                packetWriter.Write(localPlayer.weaponType);
                                localGamer.SendData(packetWriter, SendDataOptions.InOrder);
                                break;
                            case PlayerState.Respawn:
                                packetWriter.Write((int)MessageType.Respawn);
                                packetWriter.Write(localPlayer.name);
                                localGamer.SendData(packetWriter, SendDataOptions.ReliableInOrder);
                                break;
                        }
                    }
                    else if(old != localPlayer.status) // fresh kill
                    {
                        packetWriter.Write((int)MessageType.Kill);
                        packetWriter.Write((int)localPlayer.status);
                        switch(localPlayer.status)
                        {
                            case PlayerState.CrashedGround:
                                packetWriter.Write(localPlayer.name);
                                break;
                            case PlayerState.CrashedVehicle:
                                packetWriter.Write(localPlayer.collider);
                                packetWriter.Write(localPlayer.name);
                                break;
                            case PlayerState.Killed:
                                packetWriter.Write(localPlayer.name);
                                break;
                        }

                        localGamer.SendData(packetWriter, SendDataOptions.ReliableInOrder);
                    }
                }
            }

            //Package up any other necessary data and send it to other player
        }

        protected void UpdateRemotePlayer(GameTime gameTime)
        {
            // Get the other (non-local) player
            string name = packetReader.ReadString();
            foreach (var gamer in players)
            {
                if (gamer.name == name)
                {
                    gamer.Position = packetReader.ReadVector3();
                    gamer.Velocity = packetReader.ReadVector3();
                    gamer.health = packetReader.ReadSingle();
                    gamer.shield = packetReader.ReadSingle();
                }
            }
        }

        private void Update_InGame(GameTime gameTime)
        {
            // Update the local player
            UpdateLocalPlayer(gameTime);

            foreach (var p in players)
                p.Update(gameTime);

	     // Read any incoming data
            ProcessIncomingData(gameTime);
            
            // Only host checks for endgame
            if (networkSession.IsHost)
            {
                    // Check for end game conditions, if they are met send a message to other player
                string name = null;
                int deadCount = 0;
                foreach (var player in players)
                {
                    if (player.lives <= 0)
                    {
                        deadCount++;
                        break;
                    }
                    else
                        name = player.name;
                }

                if (localPlayer.lives <= 0)
                    deadCount++;
                else
                    name = localPlayer.name;

                if (name != null && deadCount >= nPlayers - 1)
                {
                    //foreach (NetworkGamer gamer in networkSession.AllGamers)
                    {
                        packetWriter.Write((int)MessageType.EndGame);
                        packetWriter.Write(name);
                        networkSession.LocalGamers[0].SendData(packetWriter,
                        SendDataOptions.Reliable);
                    }

                    if (deadCount == nPlayers)
                        name = "";
                    EndGame(name);
                }
            }
        }

        void GamerLeft(object sender, GamerLeftEventArgs e)
        {
            // Dispose of the network session, set it to null.
            networkSession.Dispose();
            networkSession = null;

            Player player = null;
            foreach (var pl in players)
            {
                //Perform any necessary clean up,
                //stop sound track, etc.

                if (e.Gamer.Gamertag == pl.name)
                {
                    player = pl;
                    pl.Kill(0.0f, PlayerState.Left);
                    break;
                }
            }
            players.Remove(player);
        
		    //Go back to looking for another session
		    currentGameState = GameState.FindSession;
        }

        private void Update_GameOver(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadSate = GamePad.GetState(PlayerIndex.One);
            RestartTime.n -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // If player presses Enter or A button, restart game
            if (keyboardState.IsKeyDown(Keys.Enter) ||
                gamePadSate.Buttons.A == ButtonState.Pressed || RestartTime.n <= 0.0f)
            {
                // Send restart game message
                packetWriter.Write((int)MessageType.RestartGame);
                networkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.Reliable);
                RestartGame();
            }

            // If player presses Escape or B button, rejoin lobby
            if (keyboardState.IsKeyDown(Keys.Escape) ||
                gamePadSate.Buttons.B == ButtonState.Pressed)
            {
                // Send rejoin lobby message
                packetWriter.Write((int)MessageType.RejoinLobby);
                networkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.Reliable);
                //RejoinLobby();
            }

            // Read any incoming messages
            ProcessIncomingData(gameTime);
        }

        public void RestartGame()
        {
            players.Clear();
            InitializeLevel();
            GameObjectManager.Instance.Reset();
            foreach(NetworkGamer gamer in networkSession.AllGamers)
            {
                if (gamer.IsLocal)
                {
                    gamer.Tag = CreateLocalPlayer(gamer.Gamertag);
                    localPlayer.Initialize();
                }
                else
                {
                    gamer.Tag = CreateRemotePlayer(gamer.Gamertag);
                }
            }

            currentGameState = GameState.Start;
        }

        protected void Update_SignIn()
        {
            // If no local gamers are signed in, show sign-in screen
            if (Gamer.SignedInGamers.Count < 1)
            {
                //Guide is a part of GamerServices, this will bring up the 	//SignIn window, allowing 1 user to Sign in. The false allows 	//users to sign in locally
                Guide.ShowSignIn(1, false);
            }
            else
            {
                // Local gamer signed in, move to find sessions
                currentGameState = GameState.FindSession;
            }
        }

        private void Update_FindSession()
        {
            // Find sesssions of the current game over SystemLink, 1 local gamer,
            //no special properties
            AvailableNetworkSessionCollection sessions =
                NetworkSession.Find(NetworkSessionType.SystemLink, 1, null);

            if (sessions.Count == 0)
            {
                // If no sessions exist, move to the CreateSession game state
                currentGameState = GameState.CreateSession;
            }
            else
            {
                // If a session does exist, join it, wire up events,
                // and move to the Start game state
                networkSession = NetworkSession.Join(sessions[0]);
                networkSession.Update();
                WireUpEvents();
            }
        }

        private void Update_CreateSession()
        {
            // Create a new session using SystemLink with a max of 1
            // local playerand a max of 2 total players
            networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 3);
	       //If the host drops, other player becomes host
            networkSession.AllowHostMigration = true;
	       //Cannot join a game in progress
            networkSession.AllowJoinInProgress = false;
            networkSession.Update();
            // Wire up events and move to the Start game state
            WireUpEvents();
        }

        private void Update_Start(GameTime gameTime)
        {
            // Get local gamer, should be just one
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];
            
            // Check for game start key or button press
            // only if there are two players
            if (networkSession.AllGamers.Count == nPlayers)
            {
                // If space bar or Start button is pressed, begin the game
                //if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                    //GamePad.GetState(PlayerIndex.One).Buttons.Start ==
                   // ButtonState.Pressed)
                {
                    // Send message to other player that we're starting
                    //packetWriter.Write((int)MessageType.StartGame);
                    //localGamer.SendData(packetWriter, SendDataOptions.Reliable);
                    if (localGamer.IsHost)
                    {
                        powerups = map.MakePowerups(50);

                        foreach (var go in powerups)
                        {
                            int type = go.type == "health_orb" ? 0 : 1;
                            packetWriter.Flush();
                            packetWriter.Write((int)MessageType.CreateOrb);
                            packetWriter.Write(type);
                            packetWriter.Write(go.world.Translation);
                            localGamer.SendData(packetWriter, SendDataOptions.ReliableInOrder);
                        }
                    }
                    // Call StartGame
                    StartGame();
                }
            }
            // Process any incoming packets
            ProcessIncomingData(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Only draw when game is active

            if (this.IsActive)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.SetRenderTarget(renderTarget);
                GraphicsDevice.Clear(Color.CornflowerBlue);
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.BlendState = BlendState.Opaque;

                // Based on the current game state,
                // call the appropriate method
                switch (currentGameState)
                {
                    case GameState.SignIn:
                    case GameState.FindSession:
                    case GameState.CreateSession:
                        GraphicsDevice.Clear(Color.DarkBlue);
                        break;
                    case GameState.Start:
		            //Write function to draw the start screen
                        //DrawStartScreen();
                        break;
                    case GameState.InGame:
		            //Write function to handle draws during game time (terrain, models, etc)‏
                        DrawInGameScreen(gameTime);
                        break;
                    case GameState.GameOver:
		            //Write function to draw game over screen
                        //DrawGameOverScreen();
                        break;
                }
            }

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, xRes, yRes), Color.White);
            iface.Draw(spriteBatch, font, currentGameState, localPlayer);
            spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            base.Draw(gameTime);
        }

        void DrawInGameScreen(GameTime gameTime)
        {
            localPlayer.Draw();
            foreach(var p in players)
            {
                p.Draw();
            }

            map.Draw(camera);
        }
    }
}
