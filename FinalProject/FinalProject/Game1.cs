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
    public enum MessageType { UpdatePosition, WeaponFired, EndGame, StartGame, RejoinLobby, RestartGame, UpdateRemotePlayer, Kill }
    public enum GameState
    {
        SignIn, FindSession,
        CreateSession, Start, InGame, GameOver
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static ContentManager ContentManager;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Camera camera;
        Player localPlayer;
        List<Player> players;
        Map map;
        NetworkSession networkSession;
        PacketWriter packetWriter;
        PacketReader packetReader;
        GameState currentGameState;
        byte id = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            currentGameState = GameState.SignIn;
            packetWriter = new PacketWriter();
            packetReader = new PacketReader();

            players = new List<Player>();

            ContentManager = Content;
        }

        protected override void Initialize()
        {
            Components.Add(new GamerServicesComponent(this));
            base.Initialize();
        }

        protected override void LoadContent() 
        {
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
            GameObjectManager.Instance.Reset();
            // Any other things that need to be set up
            //for beginning a game
            //Starting audio, resetting values, etc.
        }

        protected void EndGame()
        {
            //Perform whatever actions are to occur
            //when a game ends. Stop music, play
            //A certain sound effect, etc.
            currentGameState = GameState.GameOver;
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
                            EndGame();
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
                        //Any other actions for specific messages

                    }
                }
            }
        }

        protected void Kill()
        {
            // Get the other (non-local) player
            VehicleState killType = (VehicleState)packetReader.ReadInt32();
            string name;
            string name2;

            switch (killType)
            {
                case VehicleState.CrashedGround:
                    name = packetReader.ReadString();
                    FindAndKill(name);
                    break;
                case VehicleState.CrashedVehicle:
                    name = packetReader.ReadString();
                    name2 = packetReader.ReadString();
                    FindAndKill(name);
                    FindAndKill(name2);
                    break;
                case VehicleState.Died:
                    name = packetReader.ReadString();
                    FindAndKill(name);
                    break;
            }
        }

        private void FindAndKill(string name)
        {
            foreach (var player in players)
            {
                if (player.name == name)
                {
                    player.Delete();
                    player.alive = false;
                    player.health = 0;
                    break;
                }
            }
        }

        protected void WeaponFired()
        {
            var name = packetReader.ReadString();
            var weapon = packetReader.ReadString();

            foreach (var player in players)
            {
                if (player.name == name)
                {
                    player.weaponType = weapon;
                    player.status = VehicleState.WeaponFired;
                    break;
                }
            }
        }

        protected void WireUpEvents()
        {
            // Wire up events for gamers joining and leaving, defines what to do when a gamer
            //Joins or leaves the session
            //networkSession.GamerJoined += GamerJoined;
            //networkSession.GamerLeft += GamerLeft;

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
                currentGameState = GameState.InGame;
        }

        private void InitializeLevel()
        {
            map = new Map();
            
            if(camera == null)
                camera = new Camera(this, new Vector3(0, 600, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), map);

            var gom = GameObjectManager.Instance;
            Components.Clear();
            if(gom == null)
                gom = new GameObjectManager(this, camera);
            Components.Add(gom);

            Terrain center = new Terrain(this, camera);
            center.Load("image", Content.Load<Texture2D>("Map_c"), 256, 256, 5.0f, 1.0f, new MapOffset(), Content, GraphicsDevice);
            
            float mapOffSet = (float)Math.Abs(center.startPosition.X * 2);

            Terrain left = new Terrain(this, camera);
            MapOffset leftOffSet = new MapOffset(1, 0, 0, 0, mapOffSet);
            left.Load("image", Content.Load<Texture2D>("Map_c"), 256, 256, 5.0f, 1.0f, leftOffSet, Content, GraphicsDevice);

            Terrain right = new Terrain(this, camera);
            MapOffset rightOffSet = new MapOffset(0, 1, 0, 0, mapOffSet);
            right.Load("image", Content.Load<Texture2D>("Map_c"), 256, 256, 5.0f, 1.0f, rightOffSet, Content, GraphicsDevice);

            Terrain up = new Terrain(this, camera);
            MapOffset upOffSet = new MapOffset(0, 0, 0, 1, mapOffSet);
            up.Load("image", Content.Load<Texture2D>("Map_c"), 256, 256, 5.0f, 1.0f, upOffSet, Content, GraphicsDevice);

            Terrain down = new Terrain(this, camera);
            MapOffset downOffSet = new MapOffset(0, 0, 1, 0, mapOffSet);
            down.Load("image", Content.Load<Texture2D>("Map_c"), 256, 256, 5.0f, 1.0f, downOffSet, Content, GraphicsDevice);

            map.terrainPieces.Add(center);
            map.terrainPieces.Add(left);
            map.terrainPieces.Add(right);
            map.terrainPieces.Add(up);
            map.terrainPieces.Add(down);
        }

        private object CreateLocalPlayer(string name)
        {
            InitializeLevel();

            localPlayer = new Player(this, camera, map, true,
                                     new BasicModel(Content.Load<Model>("spaceship"), new Vector3(0, 600, 0)), name);
            localPlayer.Initialize();

            return localPlayer;
        }

        private object CreateRemotePlayer(string name)
        {
            InitializeLevel();

            Player remote = new Player(this, camera, map, false, new BasicModel(Content.Load<Model>("spaceship"), new Vector3(0, 600, 0)), name);
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
            localPlayer.Update(gameTime);

            if(localPlayer.status == VehicleState.CrashedVehicle)
            {
                FindAndKill(localPlayer.collider);
            }

            // Send data to other player
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                if(!gamer.IsLocal)
                {
                    if (localPlayer.alive)
                    {
                        // Send message to other player with message tag and new position of local player
                        packetWriter.Write((int)MessageType.UpdateRemotePlayer);
                        packetWriter.Write(localPlayer.name);
                        packetWriter.Write(localPlayer.Position);
                        packetWriter.Write(localPlayer.Forward);

                        localGamer.SendData(packetWriter, SendDataOptions.InOrder, gamer);

                        switch(localPlayer.status)
                        {
                            case VehicleState.TookDamage:
                                break;
                            case VehicleState.WeaponFired:
                                packetWriter.Write((int)MessageType.WeaponFired);
                                packetWriter.Write(localPlayer.name);
                                packetWriter.Write(localPlayer.weaponType);
                                break;
                        }
                    }
                    else
                    {
                        packetWriter.Write((int)MessageType.Kill);
                        packetWriter.Write((int)localPlayer.status);
                        switch(localPlayer.status)
                        {
                            case VehicleState.CrashedGround:
                                packetWriter.Write(localPlayer.name);
                                localGamer.SendData(packetWriter, SendDataOptions.ReliableInOrder, gamer);
                                break;
                            case VehicleState.CrashedVehicle:
                                packetWriter.Write(localPlayer.collider);
                                packetWriter.Write(localPlayer.name);
                                localGamer.SendData(packetWriter, SendDataOptions.ReliableInOrder, gamer);
                                break;
                            case VehicleState.Died:
                                packetWriter.Write(localPlayer.name);
                                localGamer.SendData(packetWriter, SendDataOptions.ReliableInOrder, gamer);
                                break;
                        }
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
                    gamer.Forward = packetReader.ReadVector3();
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
	            /*{
	                packetWriter.Write((int)MessageType.EndGame);
                    networkSession.LocalGamers[0].SendData(packetWriter, 
		            SendDataOptions.Reliable);
		            EndGame();
                }*/
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
                    pl.Delete();
                    //break;
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
            
            // If player presses Enter or A button, restart game
            if (keyboardState.IsKeyDown(Keys.Enter) ||
                gamePadSate.Buttons.A == ButtonState.Pressed)
            {
                // Send restart game message
                packetWriter.Write((int)MessageType.RestartGame);
                networkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.Reliable);
                //RestartGame();
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

        #region Network

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
            if (networkSession.AllGamers.Count == 2)
            {
                // If space bar or Start button is pressed, begin the game
                //if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
                    //GamePad.GetState(PlayerIndex.One).Buttons.Start ==
                   // ButtonState.Pressed)
                {
                    // Send message to other player that we're starting
                    packetWriter.Write((int)MessageType.StartGame);
                    localGamer.SendData(packetWriter, SendDataOptions.Reliable);
                    
                    // Call StartGame
                    StartGame();
                }
            }
            // Process any incoming packets
            ProcessIncomingData(gameTime);
        }

#endregion

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Only draw when game is active
            if (this.IsActive)
            {
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

            base.Draw(gameTime);
        }

        void DrawInGameScreen(GameTime gameTime)
        {
            localPlayer.Draw();
            foreach(var p in players)
            {
                p.Draw();
            }

            foreach(var m in map.terrainPieces)
            {
                m.Draw(gameTime);
            }
        }
    }
}
