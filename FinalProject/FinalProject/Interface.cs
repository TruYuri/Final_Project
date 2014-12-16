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
                var t = textures["Target"];
                foreach (var player2 in players)
                {
                    var coords = Game1.GraphicsDeviceRef.Viewport.Project(player2.Position, camera.projection, camera.view,
                        Matrix.Identity);

                    if (coords.Z > 1.0f || !player2.alive)
                        continue;

                    var topLeft = new Vector2(coords.X - t.Width / 2, coords.Y - t.Height / 2);
                    spriteBatch.Draw(t, topLeft, null, Color.White, 0.0f, Vector2.Zero, coords.Z, SpriteEffects.None, 0.0f);

                    var h = (player2.health / player.healthMax * 100.0f).ToString("0") + "%";
                    var l = "-";
                    var sh = (player2.shield / player.shieldMax * 100.0f).ToString("0") + "%";
                    var drawPos = topLeft + new Vector2(170, 242);
                    spriteBatch.DrawString(font, h, drawPos, Color.Red, 0.0f, Vector2.Zero, coords.Z, SpriteEffects.None, 0.0f);
                    drawPos.X += font.MeasureString(h).X;
                    spriteBatch.DrawString(font, l, drawPos, Color.White, 0.0f, Vector2.Zero, coords.Z, SpriteEffects.None, 0.0f);
                    drawPos.X += font.MeasureString(l).X;
                    spriteBatch.DrawString(font, sh, drawPos, Color.Blue, 0.0f, Vector2.Zero, coords.Z, SpriteEffects.None, 0.0f);

                    spriteBatch.DrawString(font, player2.name, topLeft + new Vector2(18, 44), Color.White, 0.0f, Vector2.Zero, coords.Z, SpriteEffects.None, 0.0f);
                    var dist = (player2.Position - player.Position).Length().ToString("0") + "m";
                    spriteBatch.DrawString(font, dist, topLeft + new Vector2(365, 35), Color.White, 0.0f, Vector2.Zero, coords.Z, SpriteEffects.None, 0.0f);
                }

                var r = textures["HUD"];
                spriteBatch.Draw(r, new Vector2(5, 6), Color.White);
                var h2 = (player.health / player.healthMax * 100.0f).ToString("0") + "%";
                var l2 = "/";
                var sh2 = (player.shield / player.shieldMax * 100.0f).ToString("0") + "%";
                spriteBatch.DrawString(font, h2, new Vector2(454, 22), Color.Red);
                spriteBatch.DrawString(font, l2, new Vector2(454 + font.MeasureString(h2).X, 22), Color.White);
                spriteBatch.DrawString(font, sh2, new Vector2(454 + font.MeasureString(h2).X + font.MeasureString(l2).X, 22), Color.Blue);
                var lv = "LIVES: " + player.lives.ToString();
                spriteBatch.DrawString(font, lv, new Vector2(5, 597), Color.White);
                var th = "THROTTLE: " + (camera.throttle * 100.0f).ToString("0") + "%";
                spriteBatch.DrawString(font, th, new Vector2(443, 596), Color.White);
                var ms = "MISSLES";
                spriteBatch.DrawString(font, ms, new Vector2(953, 568), Color.White);
                var bl = "BULLETS";
                spriteBatch.DrawString(font, bl, new Vector2(953, 602), Color.White);

                int y = (player.weaponType == "rocket" ? 568 : 602);
                spriteBatch.Draw(textures["Selector"], new Vector2(949, y), Color.White);
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

            textures.Add("HUD", Game1.ContentManager.Load<Texture2D>("HUD"));
            textures.Add("Target", Game1.ContentManager.Load<Texture2D>("Target"));
            textures.Add("Selector", Game1.ContentManager.Load<Texture2D>("Selector"));
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
