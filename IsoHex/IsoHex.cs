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
	/// Main game loop for IsoHex
	/// </summary>
	public class IsoHex : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
        Renderer renderer;
        readonly Dictionary<Guid, Entity> entities;
        readonly UI ui;

        public IsoHex()
		{
			graphics = new GraphicsDeviceManager (this);
            IsFixedTimeStep = false;
            IsMouseVisible = true;

            entities = new Dictionary<Guid, Entity>();
			ui = new UI();
		}

		protected override void Initialize ()
		{
			base.Initialize ();

			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);
			renderer = new Renderer(graphics, spriteBatch);
		}

		protected override void LoadContent ()
		{
            // Generate terrain
            foreach (Entity x in EntityFactory.TerrainFactory(10, 10)){
                entities.Add(Guid.NewGuid(), x);
            }

            // Create a swordsman
            ui.focusedEntity = Guid.NewGuid();

            entities.Add(
                ui.focusedEntity,
                EntityFactory.SwordsmanFactory(
                    0, 0, TerrainUtils.GetHeightFromTile(0, 0, entities)
                )
            );

		}

		protected override void Update (GameTime gameTime)
		{
			base.Update (gameTime);
		}

		protected override void Draw (GameTime gameTime)
		{
            renderer.Draw(entities, gameTime, ui);
		}
	}
}
