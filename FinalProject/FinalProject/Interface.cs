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

        public void Draw(SpriteBatch spriteBatch, GameState currentGameState, Player player)
        {
            switch (currentGameState)
            {
                case GameState.SignIn:
                case GameState.FindSession:
                case GameState.CreateSession:
                case GameState.Start:
                    break;
                case GameState.InGame:
                    DrawGameplayScreen(spriteBatch, player);
                    break;
                case GameState.GameOver:
                    break;
            }
        }

        private void DrawGameplayScreen(SpriteBatch spriteBatch, Player player)
        {
            if (player == null)
                return;

            var players = (List<Player>)other["players"];
            var camera = (Camera)other["camera"];

            if (player.status == PlayerState.Alive)
            {
                var t = textures["target"];
                foreach (var player2 in players)
                {
                    var coords = Game1.GraphicsDeviceRef.Viewport.Project(player2.Position, camera.projection, camera.view,
                        Matrix.Identity);

                    if(coords.Z < 1.0f)
                        spriteBatch.Draw(t, new Vector2(coords.X - t.Width / 2, coords.Y - t.Height / 2), Color.White);
                }

                var r = textures["reticle"];
                spriteBatch.Draw(r, new Vector2(Game1.xRes / 2 - r.Width / 2, Game1.yRes / 2 - r.Height / 2), Color.White);
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
    }
}
