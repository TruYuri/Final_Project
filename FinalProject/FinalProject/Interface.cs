using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FinalProject
{
    class Interface
    {
        private static Dictionary<string, Texture2D> textures;
        private static Dictionary<string, object> other;

        public Interface() { }

        public void Update(GameTime gameTime)
        {
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, GameState currentGameState, Player player)
        {
            Vector2 pos;
            switch (currentGameState)
            {
                case GameState.SignIn:
                    pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Signing in...") / 2;
                    spriteBatch.DrawString(font, "Signing in...", pos, Color.White);
                    break;
                case GameState.FindSession:
                    pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Searching for game...") / 2;
                    spriteBatch.DrawString(font, "Searching for game...", pos, Color.White);
                    break;
                case GameState.CreateSession:
                    pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Creating game...") / 2;
                    spriteBatch.DrawString(font, "Creating game...", pos, Color.White);
                    break;
                case GameState.Start:
                    pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Starting game...") / 2;
                    spriteBatch.DrawString(font, "Starting game...", pos, Color.White);
                    break;
                case GameState.InGame:
                    DrawGameplayScreen(spriteBatch, font, player);
                    break;
                case GameState.GameOver:
                    pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Game over!") / 2;
                    spriteBatch.DrawString(font, "Game over!", pos, Color.White);
                    break;
            }
        }

        private void DrawGameplayScreen(SpriteBatch spriteBatch, SpriteFont font, Player player)
        {
            if (player == null)
                return;

            var players = (List<Player>)other["players"];
            var camera = (Camera)other["camera"];
            
            Vector2 pos;
            if (player.status == PlayerState.Alive)
            {
                var t = textures["target"];
                foreach (var player2 in players)
                {
                    var coords = Game1.GraphicsDeviceRef.Viewport.Project(player2.Position, camera.projection, camera.view,
                        Matrix.Identity);

                    if (coords.Z < 1.0f)
                    {
                        float s = 0.5f;
                        spriteBatch.Draw(t, new Vector2(coords.X - t.Width * s / 2, coords.Y - t.Height * s / 2), null, Color.White, 0.0f, Vector2.Zero, coords.Z * s, SpriteEffects.None, 0);
                        spriteBatch.DrawString(font, player2.name,
                                               new Vector2((coords.X + t.Width * s / 2) * coords.Z, (coords.Y - t.Height * s / 2)), Color.White);
                        spriteBatch.DrawString(font, ((player2.Position - player.Position).Length().ToString("0")) + "m",
                                               new Vector2((coords.X + t.Width * s / 2) * coords.Z, (coords.Y + t.Height * s / 2)), Color.White);
                    }
                }

                var r = textures["reticle"];
                spriteBatch.Draw(r, new Vector2(Game1.xRes / 2 - r.Width / 2, Game1.yRes / 2 - r.Height / 2), Color.White);
            }
            else if(player.status == PlayerState.CrashedGround)
            {
                pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Crashed!") / 2;
                spriteBatch.DrawString(font, "Crashed!", new Vector2(pos.X, Game1.yRes / 3), Color.White);
                Respawn(spriteBatch, font, player);
            }
            else if (player.status == PlayerState.CrashedVehicle)
            {
                pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Crashed into " + player.collider) / 2;
                spriteBatch.DrawString(font, "Crashed into " + player.collider, new Vector2(pos.X, Game1.yRes / 3), Color.White);
                Respawn(spriteBatch, font, player);
            }
            else if(player.status == PlayerState.Killed)
            {
                pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Killed by " + player.collider) / 2;
                spriteBatch.DrawString(font, "Killed by " + player.collider, new Vector2(pos.X, Game1.yRes / 3), Color.White);
                Respawn(spriteBatch, font, player);
            }
        }

        private void Respawn(SpriteBatch spriteBatch, SpriteFont font, Player player)
        {
            string respawn = "Respawn in " + player.respawnTimer.ToString("0") + "...";
            var pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString(respawn) / 2;
            spriteBatch.DrawString(font, respawn, new Vector2(pos.X, Game1.yRes / 3 + Game1.yRes / 3), Color.White);
        }

        public static void LoadGameplayInterface(List<Player> players, Camera camera)
        {
            textures = new Dictionary<string,Texture2D>();
            other = new Dictionary<string,object>();

            other.Add("players", players);
            other.Add("camera", camera);

            textures.Add("reticle", Game1.ContentManager.Load<Texture2D>("primary_reticle"));
            textures.Add("target", Game1.ContentManager.Load<Texture2D>("object_reticle"));
        }
    }
}
