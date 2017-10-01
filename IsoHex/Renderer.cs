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

        public void DrawHexagon(Vector3 center, Vector3 scale, Color topColor, Color sideColor){

			VertexPositionColor[] Verts = new VertexPositionColor[6 * 3 * 3];
            foreach (var i in Enumerable.Range(0, 6))
            {
                // Create vertices
                float angle = ((float)Math.PI / 3) * i + ((float)Math.PI / 6);
                float angleNext = angle + ((float)Math.PI / 3);

                // Surface
                Verts[i * 9 + 0].Position = new Vector3(
                    center.X + CoordUtils.hexsize * (float)Math.Cos(angle),
                    center.Y + CoordUtils.hexsize * (float)Math.Sin(angle),
                    center.Z + scale.Z
                );
                Verts[i * 9 + 1].Position = new Vector3(
                    center.X, center.Y, center.Z + scale.Z
                );
                Verts[i * 9 + 2].Position = new Vector3(
                    center.X + CoordUtils.hexsize * (float)Math.Cos(angleNext),
                    center.Y + CoordUtils.hexsize * (float)Math.Sin(angleNext),
                    center.Z + scale.Z
                );

                // Sides (1)
				Verts[i * 9 + 3].Position = new Vector3(
					center.X + CoordUtils.hexsize * (float)Math.Cos(angle),
					center.Y + CoordUtils.hexsize * (float)Math.Sin(angle),
					center.Z + scale.Z
				);
				Verts[i * 9 + 4].Position = new Vector3(
					center.X + CoordUtils.hexsize * (float)Math.Cos(angleNext),
					center.Y + CoordUtils.hexsize * (float)Math.Sin(angleNext),
					center.Z + scale.Z
				);
				Verts[i * 9 + 5].Position = new Vector3(
					center.X + CoordUtils.hexsize * (float)Math.Cos(angle),
					center.Y + CoordUtils.hexsize * (float)Math.Sin(angle),
					center.Z
				);

				// Sides (2)
				Verts[i * 9 + 8].Position = new Vector3(
					center.X + CoordUtils.hexsize * (float)Math.Cos(angle),
					center.Y + CoordUtils.hexsize * (float)Math.Sin(angle),
					center.Z
				);
				Verts[i * 9 + 7].Position = new Vector3(
					center.X + CoordUtils.hexsize * (float)Math.Cos(angleNext),
					center.Y + CoordUtils.hexsize * (float)Math.Sin(angleNext),
					center.Z
				);
                Verts[i * 9 + 6].Position = new Vector3(
                    center.X + CoordUtils.hexsize * (float)Math.Cos(angleNext),
                    center.Y + CoordUtils.hexsize * (float)Math.Sin(angleNext),
                    center.Z + scale.Z
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

        public void DrawEntities(Dictionary<Guid, Entity> list, GameTime gametime){
            
            // Get every renderable thing
            foreach (var obj in list.Where(x => x.Value.Active.HasFlag(Entity._Components.RENDERABLE))){
                
                // Convert coordinates
                Vector3 pos = CoordUtils.GetWorldPosition(obj.Value.Renderable.pos);
                Vector3 scale = CoordUtils.GetWorldScale(obj.Value.Renderable.scale);

                // Get sprite width/height & offset by 1/2 that

				// Render object depending on model ID
                switch(obj.Value.Renderable.modelID){
                    case "ground":
                        DrawHexagon(pos, scale, Color.SpringGreen, Color.SaddleBrown);
                        break;
                    default:
                        //spritebatch.Draw(DebugDot, new Rectangle(centerPos.X, centerPos.Y, 8, 8), Color.Red);
                        DrawHexagon(pos, scale, Color.Red, Color.DarkRed);
                        break;
                }
            }
        }

        public void Draw(Dictionary<Guid, Entity> list, GameTime gametime){

            graphics.BeginDraw();

            // Rotate camera
            cameraPos.X = (int)(Math.Cos(gametime.TotalGameTime.TotalMilliseconds / 1000.0) * 50);
            cameraPos.Y = (int)(Math.Sin(gametime.TotalGameTime.TotalMilliseconds / 1000.0) * 50);

            // Set camera data
			Setup3D();

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
            var fps = string.Format("FPS: {0}", frameCounter.CurrentFramesPerSecond);

            // TODO: figure out how to get font imported.
            // Content pipeline tools keeps segfaulting, so that's out.
            Console.WriteLine(fps);

            //spritebatch.End();
            graphics.EndDraw();
        }
    }
}
