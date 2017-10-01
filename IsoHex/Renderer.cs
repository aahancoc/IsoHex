using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IsoHex
{
    public class Renderer
    {
        readonly Texture2D DebugDot;
        readonly GraphicsDeviceManager graphics;
        readonly GraphicsDevice screen;
        readonly SpriteBatch spritebatch;
		FrameCounter frameCounter;

        Point cameraPos = new Point(0, 0);

        public Renderer(GraphicsDeviceManager _graphics, SpriteBatch _spritebatch)
        {
            graphics = _graphics;
            spritebatch = _spritebatch;
            screen = graphics.GraphicsDevice;
			frameCounter = new FrameCounter();
                
            // Get SOMETHING we can draw...
            DebugDot = new Texture2D(screen, 1, 1, false, SurfaceFormat.Color);
			Int32[] pixel = { 0xFFFFFF }; // White. 0xFF is Red, 0xFF0000 is Blue
			DebugDot.SetData(pixel, 0, 1);
        }

        // Adjust position from hex grid coordinates to on-screen coordinates
        // We are using "odd-r horizontal layout"
        Point GetPixelPosition(Vector3 pos){

            Point result = new Point();
            const float size = 64.0f;
            const float sqrt3 = 1.73205080757f; // sqrt(3)
            const float height = size * 0.75f;
            const float width = sqrt3 * height;
            const float tileHeight = (height / 2.0f); // For now?

            // X should be offset + width/2 if on an odd row
            float oddness = (float)Math.Abs((pos.Y % 2.0) - 1);
            result.X = (int)((pos.X * width) + ((width / 2) * oddness));

			// Y should be multiplied by a scaler
            result.Y = (int)(pos.Y * height);

            // Z should add some value to Y.
            result.Y += (int)(pos.Z * tileHeight);

            return result;
        }

		// Turn Position coodinates into a Renderable coordinate
		/*void UpdatePosition(Entity obj)
		{
            // Check entity componets
            if(
                !obj.Active.HasFlag(Entity._Active.POSITION) ||
                !obj.Active.HasFlag(Entity._Active.RENDERABLE) ||
                obj.Renderable.hidden == true
            ){
                return;
            }
		}*/

        public void DrawEntities(List<Entity> list, GameTime gametime){
            
            // Get every renderable thing
            foreach (var obj in list.Where(x => x.Active.HasFlag(Entity._Active.RENDERABLE))){
                
                // Convert coordinates
                Point centerPos = GetPixelPosition(obj.Renderable.pos);


                // Get sprite width/height & offset by 1/2 that

				// Render as a dot for now
                spritebatch.Draw(DebugDot, new Rectangle(centerPos.X, centerPos.Y, 8, 8), Color.Green);
            }
        }

        public void Draw(List<Entity> list, GameTime gametime){

            graphics.BeginDraw();
            spritebatch.Begin();

            // Move camera
            cameraPos.X = (int)(Math.Cos(gametime.TotalGameTime.TotalMilliseconds / 500.0) * 50);
            cameraPos.Y = (int)(Math.Sin(gametime.TotalGameTime.TotalMilliseconds / 500.0) * 50);

            Viewport viewport = screen.Viewport;
            viewport.X = cameraPos.X;
            viewport.Y = cameraPos.Y;
            screen.Viewport = viewport;

            // BG
			screen.Clear(Color.CornflowerBlue);

            // Entities
            DrawEntities(list, gametime);

			// UI

			// FPS
			var deltaTime = (float)gametime.ElapsedGameTime.TotalSeconds;
			frameCounter.Update(deltaTime);
			var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);

            // TODO: figure out how to get font imported.
            // Content pipeline tools keeps segfaulting, so that's out.
            //Console.WriteLine(fps);

            spritebatch.End();
            graphics.EndDraw();
        }
    }
}
