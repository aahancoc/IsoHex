using System;
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
        EntityFactory entityFactory;
        Renderer renderer;

		public IsoHex()
		{
			graphics = new GraphicsDeviceManager (this);
            entityFactory = new EntityFactory();
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
			entityFactory.List.AddRange(entityFactory.TerrainFactory(10, 10));
		}

		protected override void Update (GameTime gameTime)
		{
			base.Update (gameTime);
		}

		protected override void Draw (GameTime gameTime)
		{
            GraphicsDevice.Clear(Color.CornflowerBlue);
            renderer.Draw(entityFactory.List, gameTime);
			//base.Draw (gameTime);
		}
	}
}
