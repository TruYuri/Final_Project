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
    class number<t>
    {
        public t n;
    }

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
                    var winner = (string)other["winner"];
                    var timer = (number<float>)other["time"];
                    float t = timer.n;

                    pos = new Vector2(Game1.xRes / 2, Game1.yRes / 4) - font.MeasureString("Game over!") / 2;
                    spriteBatch.DrawString(font, "Game over!", pos, Color.White);

                    if(winner == "") // draw
                    {
                        pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Draw!") / 2;
                        spriteBatch.DrawString(font, "Draw!", pos, Color.White);
                    }
                    else
                    {
                        pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString(winner + " won!") / 2;
                        spriteBatch.DrawString(font, winner + " won!", pos, Color.White);
                    }
                    pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2 + Game1.yRes / 4) - font.MeasureString("Next game in " + t.ToString("0")) / 2;
                    spriteBatch.DrawString(font, "Next game in " + t.ToString("0"), pos, Color.White); 
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
            if (player.status == PlayerState.Alive || player.status == PlayerState.WeaponFired)
            {
                var t = textures["target"];
                foreach (var player2 in players)
                {
                    var coords = Game1.GraphicsDeviceRef.Viewport.Project(player2.Position, camera.projection, camera.view,
                        Matrix.Identity);

                    if (coords.Z > 1.0f || !player2.alive)
                        continue;

                    float s = 0.5f;
                    spriteBatch.Draw(t, new Vector2(coords.X - t.Width * s / 2, coords.Y - t.Height * s / 2), null, Color.White, 0.0f, Vector2.Zero, coords.Z * s, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, player2.name,
                                            new Vector2((coords.X + t.Width * s / 2) * coords.Z, (coords.Y - t.Height * s / 2) * coords.Z), Color.White);
                    spriteBatch.DrawString(font, ((player2.Position - player.Position).Length().ToString("0")) + "m",
                                            new Vector2((coords.X + t.Width * s / 2) * coords.Z, (coords.Y + t.Height * s / 2) * coords.Z), Color.White);

                    var healthString = ("HP: " + (player2.health / player2.healthMax * 100.0f).ToString("0.00") + "%");
                    var shieldString = ("SH: " + (player2.shield / player2.shieldMax * 100.0f).ToString("0.00") + "%");
                    var hV = font.MeasureString(healthString);
                    var sV = font.MeasureString(shieldString);
                    spriteBatch.DrawString(font, healthString,
                                            new Vector2((coords.X - t.Width * s / 2) * coords.Z - hV.X, (coords.Y - t.Height * s / 2) * coords.Z), Color.White);
                    spriteBatch.DrawString(font, shieldString,
                                            new Vector2((coords.X - t.Width * s / 2) * coords.Z - sV.X, (coords.Y - t.Height * s / 2) * coords.Z + hV.Y), Color.White);
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
                pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Shot down by " + player.collider) / 2;
                spriteBatch.DrawString(font, "Shot down by " + player.collider, new Vector2(pos.X, Game1.yRes / 3), Color.White);
                Respawn(spriteBatch, font, player);
            }
            else if(player.status == PlayerState.OutOfBounds)
            {
                Vector2 m = font.MeasureString("Out of bounds!  Turn back!") / 2;
                pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - m;
                var pos2 = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString(player.boundsTimer.ToString("0")) / 2;
                spriteBatch.DrawString(font, "Out of bounds! Turn back!", new Vector2(pos.X, Game1.yRes / 3), Color.White);
                spriteBatch.DrawString(font, player.boundsTimer.ToString("0"), new Vector2(pos2.X, Game1.yRes / 3 + m.Y * 2), Color.White);
            }
            else if(player.status == PlayerState.DiedOOB)
            {
                pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - font.MeasureString("Flew too long out of bounds!") / 2;
                spriteBatch.DrawString(font, "Flew too long out of bounds!", new Vector2(pos.X, Game1.yRes / 3), Color.White);
                Respawn(spriteBatch, font, player);
            }
        }

        private void Respawn(SpriteBatch spriteBatch, SpriteFont font, Player player)
        {
            var pos = new Vector2(Game1.xRes / 2, Game1.yRes / 2);
            if (player.lives > 0)
            {
                string respawn = "Respawn in " + player.respawnTimer.ToString("0") + "...";
                pos -= (font.MeasureString(respawn) / 2);
                spriteBatch.DrawString(font, respawn, new Vector2(pos.X, Game1.yRes / 3 + Game1.yRes / 3), Color.White);
            }
            else
            {
                string done = "Out of lives!";
                pos -= (font.MeasureString(done) / 2);
                spriteBatch.DrawString(font, done, new Vector2(pos.X, Game1.yRes / 3 + Game1.yRes / 3), Color.White);
            }

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

        public static void LoadGameoverInterface(string winner, number<float> time)
        {
            textures = new Dictionary<string, Texture2D>();
            other = new Dictionary<string, object>();

            other.Add("winner", winner);
            other.Add("time", time);
        }
    }
}
