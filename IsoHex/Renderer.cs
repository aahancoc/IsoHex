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
        readonly FrameCounter frameCounter;

        // Camera
        Vector3 cameraPos = new Vector3(0, 0, 40);
		Vector3 cameraLookAt = Vector3.Zero;
		Vector3 cameraUp = Vector3.UnitZ;
        float aspRatio;
        const float nearClip = 1;
        const float farClip = 2000;
        readonly BasicEffect effect;

        // Triangles to draw
        List<VertexPositionColor> tris;

        // constants
        const float hexsize = 4.0f;
		const float sqrt3 = 1.73205080757f; // sqrt(3)
		const float hexheight = hexsize * 2.0f;
        const float hexwidth = (sqrt3 / 2) * hexheight;

        public Renderer(GraphicsDeviceManager _graphics, SpriteBatch _spritebatch)
        {
            graphics = _graphics;
            spritebatch = _spritebatch;
            screen = graphics.GraphicsDevice;
			frameCounter = new FrameCounter();
            effect = new BasicEffect(screen);

            //screen.BlendState = BlendState.Opaque;
            //screen.DepthStencilState = DepthStencilState.Default;
                
            // Get SOMETHING we can draw...
            DebugDot = new Texture2D(screen, 1, 1, false, SurfaceFormat.Color);
			Int32[] pixel = { 0xFFFFFF }; // White. 0xFF is Red, 0xFF0000 is Blue
			DebugDot.SetData(pixel, 0, 1);
        }

        // Adjust position from hex grid coordinates to on-screen coordinates
        // We are using "odd-r horizontal layout"
       Vector3 GetWorldPosition(Vector3 pos){

            Vector3 result = new Vector3();
            //const float tileHeight = (hexheight / 2.0f); // For now?

            // X should be offset + width/2 if on an odd row
            float oddness = (float)Math.Abs((pos.Y % 2.0) - 1);
            result.X = ((pos.X * hexwidth) + ((hexwidth / 2f) * oddness));

			// Y should be multiplied by a scaler
            result.Y = (pos.Y * (hexheight * 3.0f / 4.0f));

            // Z should be multiplied by a scaler.
            result.Z = (pos.Z * 10);

            return result;
        }

        Vector3 GetWorldScale(Vector3 scale){
            Vector3 result = new Vector3();
            result.X = scale.X * hexwidth;
            result.Y = scale.Y * hexheight;
            result.Z = scale.Z * 10;
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

        public void Setup3D(){
            aspRatio = (
                graphics.PreferredBackBufferWidth /
                graphics.PreferredBackBufferHeight
            );

			effect.Projection = Matrix.CreateOrthographic(
                50, 50, nearClip, farClip
            );

			effect.View = Matrix.CreateLookAt(
		        cameraPos, cameraLookAt, cameraUp
            );

            effect.VertexColorEnabled = true;

			/*effect.Projection = Matrix.CreatePerspectiveFieldOfView(
		        fov, aspRatio, nearClip, farClip
            );*/

           tris = new List<VertexPositionColor>();
		}

        public void DrawHexagon(Vector3 center, float height, Color topColor, Color sideColor){

			VertexPositionColor[] Verts = new VertexPositionColor[6 * 3 * 3];
            foreach (var i in Enumerable.Range(0, 6))
            {
                // Create vertices
                float angle = ((float)Math.PI / 3) * i + ((float)Math.PI / 6);
                float angleNext = angle + ((float)Math.PI / 3);

                // Surface
                Verts[i * 9 + 0].Position = new Vector3(
                    center.X + hexsize * (float)Math.Cos(angle),
                    center.Y + hexsize * (float)Math.Sin(angle),
                    center.Z + height
                );
                Verts[i * 9 + 1].Position = new Vector3(
                    center.X, center.Y, center.Z + height
                );
                Verts[i * 9 + 2].Position = new Vector3(
                    center.X + hexsize * (float)Math.Cos(angleNext),
                    center.Y + hexsize * (float)Math.Sin(angleNext),
                    center.Z + height
                );

                // Sides (1)
				Verts[i * 9 + 3].Position = new Vector3(
					center.X + hexsize * (float)Math.Cos(angle),
					center.Y + hexsize * (float)Math.Sin(angle),
					center.Z + height
				);
				Verts[i * 9 + 4].Position = new Vector3(
					center.X + hexsize * (float)Math.Cos(angleNext),
					center.Y + hexsize * (float)Math.Sin(angleNext),
					center.Z + height
				);
				Verts[i * 9 + 5].Position = new Vector3(
					center.X + hexsize * (float)Math.Cos(angle),
					center.Y + hexsize * (float)Math.Sin(angle),
					center.Z
				);

				// Sides (2)
				Verts[i * 9 + 8].Position = new Vector3(
					center.X + hexsize * (float)Math.Cos(angle),
					center.Y + hexsize * (float)Math.Sin(angle),
					center.Z
				);
				Verts[i * 9 + 7].Position = new Vector3(
					center.X + hexsize * (float)Math.Cos(angleNext),
					center.Y + hexsize * (float)Math.Sin(angleNext),
					center.Z
				);
                Verts[i * 9 + 6].Position = new Vector3(
                    center.X + hexsize * (float)Math.Cos(angleNext),
                    center.Y + hexsize * (float)Math.Sin(angleNext),
                    center.Z + height
                );

                // Top color
                Verts[i * 9 + 0].Color = topColor;
                Verts[i * 9 + 1].Color = topColor;
                Verts[i * 9 + 2].Color = topColor;

				// Side colors
				Verts[i * 9 + 3].Color = sideColor;
				Verts[i * 9 + 4].Color = sideColor;
				Verts[i * 9 + 5].Color = sideColor;
                Verts[i * 9 + 6].Color = sideColor;
                Verts[i * 9 + 7].Color = sideColor;
                Verts[i * 9 + 8].Color = sideColor;
            }

            tris.AddRange(Verts);
        }

        public void DrawEntities(List<Entity> list, GameTime gametime){
            
            // Get every renderable thing
            foreach (var obj in list.Where(x => x.Active.HasFlag(Entity._Active.RENDERABLE))){
                
                // Convert coordinates
                Vector3 centerPos = GetWorldPosition(obj.Renderable.pos);

                // Get sprite width/height & offset by 1/2 that

				// Render object depending on model ID
                switch(obj.Renderable.modelID){
                    case "ground":
                        DrawHexagon(centerPos, obj.Renderable.height * 10, Color.LawnGreen, Color.LightGray);
                        break;
                    default:
                        //spritebatch.Draw(DebugDot, new Rectangle(centerPos.X, centerPos.Y, 8, 8), Color.Red);
                        break;
                }
            }
        }

        public void Draw(List<Entity> list, GameTime gametime){

            graphics.BeginDraw();

            // Rotate camera
            cameraPos.X = (int)(Math.Cos(gametime.TotalGameTime.TotalMilliseconds / 1000.0) * 50);
            cameraPos.Y = (int)(Math.Sin(gametime.TotalGameTime.TotalMilliseconds / 1000.0) * 50);

            // Set camera data
			Setup3D();

            /*Viewport viewport = screen.Viewport;
            viewport.X = cameraPos.X;
            viewport.Y = cameraPos.Y;
            screen.Viewport = viewport;*/

            // BG
			screen.Clear(Color.CornflowerBlue);

			// Add entities to render list
			DrawEntities(list, gametime);

			// Draw all triangles
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
                graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList, tris.ToArray(), 0, tris.Count() / 3
                );
			}

			//spritebatch.Begin();

			// UI

			// FPS
			var deltaTime = (float)gametime.ElapsedGameTime.TotalSeconds;
			frameCounter.Update(deltaTime);
			var fps = string.Format("FPS: {0}", frameCounter.AverageFramesPerSecond);

            // TODO: figure out how to get font imported.
            // Content pipeline tools keeps segfaulting, so that's out.
            Console.WriteLine(fps);

            //spritebatch.End();
            graphics.EndDraw();
        }
    }
}
