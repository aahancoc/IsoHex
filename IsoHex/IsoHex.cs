using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace IsoHex
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class IsoHex : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
        Renderer renderer;
        readonly List<Entity> entities;

        public IsoHex()
		{
			graphics = new GraphicsDeviceManager (this);
            IsFixedTimeStep = false;
            IsMouseVisible = true;

            entities = new List<Entity>();
		}

		protected override void Initialize ()
		{
			base.Initialize ();

			renderer = new Renderer(graphics, spriteBatch);
		}

		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
			entities.AddRange(EntityFactory.TerrainFactory(10, 10));

		}

		protected override void Update (GameTime gameTime)
		{
			base.Update (gameTime);
		}

		protected override void Draw (GameTime gameTime)
		{
            renderer.Draw(entities, gameTime);
		}
	}
}
