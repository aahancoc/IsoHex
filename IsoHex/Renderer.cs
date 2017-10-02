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
		Vector3 cameraLookAt = Vector3.Zero;
        float aspRatio;
        const float nearClip = 0.1f;
        const float farClip = 2000f;
        readonly BasicEffect effect;

        // Triangles to draw
        List<VertexPositionColor> tris;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IsoHex.Renderer"/> class.
        /// </summary>
        /// <param name="_graphics">Graphics.</param>
        /// <param name="_spritebatch">Spritebatch.</param>
        public Renderer(GraphicsDeviceManager _graphics, SpriteBatch _spritebatch)
        {
            graphics = _graphics;
            spritebatch = _spritebatch;
            screen = graphics.GraphicsDevice;
			frameCounter = new FrameCounter();
            effect = new BasicEffect(screen);
			effect.VertexColorEnabled = true;

            //screen.BlendState = BlendState.Opaque;
            //screen.DepthStencilState = DepthStencilState.Default;
                
            // Get SOMETHING we can draw...
            DebugDot = new Texture2D(screen, 1, 1, false, SurfaceFormat.Color);
			Int32[] pixel = { 0xFFFFFF }; // White. 0xFF is Red, 0xFF0000 is Blue
			DebugDot.SetData(pixel, 0, 1);
        }

        /// <summary>
        /// Set up the camera.
        /// </summary>
        /// <param name="ui">UI/Camera parameters.</param>
        /// <param name="entities">Entity list.</param>
        public void SetupCamera(UI ui, Dictionary<Guid, Entity> entities){

            // Set camera focus
            SetFocus(ui.focusedEntity, entities);

            // Set angle and position
            Vector3 cameraPos = new Vector3(
                (float)Math.Cos(ui.yaw) * (float)Math.Cos(ui.pitch),
                (float)Math.Sin(ui.yaw) * (float)Math.Cos(ui.pitch),
                (float)Math.Sin(ui.pitch)
            ) * ui.zoomLevel + cameraLookAt;

            effect.View = Matrix.CreateLookAt(
                cameraPos,
                cameraLookAt,
                Vector3.UnitZ          
            );

            // Zoom, too
			effect.Projection = Matrix.CreateOrthographic(
                ui.zoomLevel, ui.zoomLevel * aspRatio, nearClip, farClip
            );

            Console.WriteLine(
                "Yaw: " + ui.yaw + "\n" +
                "Pitch: " + ui.pitch + "\n" +
                "Obj Pos: " + cameraLookAt.ToString() + "\n" +
                "Cam Pos: " + cameraPos.ToString() + "\n" 
            );
        }

        /// <summary>
        /// Sets up the 3D renderer for the next frame
        /// </summary>
        public void Setup3D(){

            // Set aspect ratio
            aspRatio = (
                graphics.PreferredBackBufferWidth /
                graphics.PreferredBackBufferHeight
            );

            /*effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                aspRatio,
                nearClip,
                farClip
            );*/

            tris = new List<VertexPositionColor>();
		}

        /// <summary>
        /// Draws a hexagon.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="scale">Scale.</param>
        /// <param name="topColor">Top color.</param>
        /// <param name="sideColor">Side color.</param>
        public void DrawHexagon(Vector3 center, Vector3 scale, Color topColor, Color sideColor){

            VertexPositionColor[] Verts;

            // Flat hexagon
            if (Equals(scale.Z, 0)) {
                Verts = new VertexPositionColor[6 * 3];

                foreach (var i in Enumerable.Range(0, 2))
                {
                    // Create vertices
                    float angle = ((float)Math.PI / 3) * i + ((float)Math.PI / 6);
                    float angleNext = angle + ((float)Math.PI / 3);

                    // Surface
                    Verts[i * 9 + 0].Position = new Vector3(
                        center.X + scale.X * (float)Math.Cos(angle),
                        center.Y + scale.Y * (float)Math.Sin(angle),
                        center.Z + scale.Z
                    );
                    Verts[i * 9 + 1].Position = new Vector3(
                        center.X, center.Y, center.Z + scale.Z
                    );
                    Verts[i * 9 + 2].Position = new Vector3(
                        center.X + scale.X * (float)Math.Cos(angleNext),
                        center.Y + scale.Y * (float)Math.Sin(angleNext),
                        center.Z + scale.Z
                    );

					// Top color
					Verts[i * 9 + 0].Color = topColor;
					Verts[i * 9 + 1].Color = topColor;
					Verts[i * 9 + 2].Color = topColor;
                }
            }
            // 3D hexagon (no bottom)
            else {
                Verts = new VertexPositionColor[6 * 3 * 3];

				foreach (var i in Enumerable.Range(0, 6))
				{
					// Create vertices
					float angle = ((float)Math.PI / 3) * i + ((float)Math.PI / 6);
					float angleNext = angle + ((float)Math.PI / 3);

					// Surface
					Verts[i * 9 + 0].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angle),
						center.Y + scale.Y * (float)Math.Sin(angle),
						center.Z + scale.Z
					);
					Verts[i * 9 + 1].Position = new Vector3(
						center.X, center.Y, center.Z + scale.Z
					);
					Verts[i * 9 + 2].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angleNext),
						center.Y + scale.Y * (float)Math.Sin(angleNext),
						center.Z + scale.Z
					);

					// Sides (1)
					Verts[i * 9 + 3].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angle),
						center.Y + scale.Y * (float)Math.Sin(angle),
						center.Z + scale.Z
					);
					Verts[i * 9 + 4].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angleNext),
						center.Y + scale.Y * (float)Math.Sin(angleNext),
						center.Z + scale.Z
					);
					Verts[i * 9 + 5].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angle),
						center.Y + scale.Y * (float)Math.Sin(angle),
						center.Z
					);

					// Sides (2)
					Verts[i * 9 + 8].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angle),
						center.Y + scale.Y * (float)Math.Sin(angle),
						center.Z
					);
					Verts[i * 9 + 7].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angleNext),
						center.Y + scale.Y * (float)Math.Sin(angleNext),
						center.Z
					);
					Verts[i * 9 + 6].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angleNext),
						center.Y + scale.Y * (float)Math.Sin(angleNext),
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
            }



            tris.AddRange(Verts);
        }

        /// <summary>
        /// Sets the focus of the camera
        /// </summary>
        /// <returns><c>true</c>, if focus was set, <c>false</c> otherwise.</returns>
        /// <param name="ID">Entity identifier.</param>
        /// <param name="list">Entity list.</param>
        public bool SetFocus(Guid ID, Dictionary<Guid, Entity> list){
            if (!list.ContainsKey(ID)) { return false; }
            Entity entity = list[ID];
            if (!entity.Active.HasFlag(Entity._Components.RENDERABLE)) { return false; }

            cameraLookAt = CoordUtils.GetWorldPosition(entity.Renderable.pos);
			return true;
        }

        /// <summary>
        /// Draws the entities.
        /// </summary>
        /// <param name="list">Entity list.</param>
        /// <param name="gametime">Gametime.</param>
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

        /// <summary>
        /// Draw everything
        /// </summary>
        /// <param name="list">Entity list.</param>
        /// <param name="gametime">Gametime.</param>
        public void Draw(Dictionary<Guid, Entity> list, GameTime gametime, UI ui){

            // Begin draw
            graphics.BeginDraw();

            // Rotate camera
            //cameraLookAt.X = (float)(Math.Cos(gametime.TotalGameTime.TotalMilliseconds / 1000.0) * 5.0f);
            //cameraLookAt.Y = (float)(Math.Sin(gametime.TotalGameTime.TotalMilliseconds / 1000.0) * 5.0f);
            ui.yaw = (float)(ui.yaw - (gametime.ElapsedGameTime.TotalMilliseconds / 10000.0)) % MathHelper.TwoPi;

            // Set camera and 3D renderer
            SetupCamera(ui, list);
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

            // UI
            //spritebatch.Begin();

            //spritebatch.End();

            // End draw
            graphics.EndDraw();

            // FPS
            var deltaTime = (float)gametime.ElapsedGameTime.TotalSeconds;
            frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", frameCounter.CurrentFramesPerSecond);

            // TODO: figure out how to get font imported.
            // Content pipeline tools keeps segfaulting, so that's out.
            //Console.WriteLine(fps);
        }
    }
}
