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
        List<VertexPositionColor> tris = new List<VertexPositionColor>();
        List<VertexPositionColor> wiretris = new List<VertexPositionColor>();

		// Rasterizer states
		readonly RasterizerState defaultRasterizer;
		readonly RasterizerState wireframeRasterizer;

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

			// Rasterizer states
			defaultRasterizer = screen.RasterizerState;
			wireframeRasterizer = new RasterizerState()
			{
				FillMode = FillMode.WireFrame,
				DepthClipEnable = false
			};
        }

        /// <summary>
        /// Set up the camera.
        /// </summary>
        /// <param name="ui">UI/Camera parameters.</param>
        /// <param name="entities">Entity list.</param>
        public void SetupCamera(UI ui, Dictionary<Guid, Entity> entities){

            // Set camera focus
            SetFocus(ui.cursorEntity, entities);

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

            /*Console.WriteLine(
                "Yaw: " + ui.yaw + "\n" +
                "Pitch: " + ui.pitch + "\n" +
                "Obj Pos: " + cameraLookAt.ToString() + "\n" +
                "Cam Pos: " + cameraPos.ToString() + "\n" 
            );*/
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

            tris.Clear();
            wiretris.Clear();
		}

        /// <summary>
        /// Draws the cursor.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="scale">Scale.</param>
        /// <param name="dir">Direction.</param>
        /// <param name="team">Team.</param>
        /// <param name="time">Total game time in seconds.</param>
        public void DrawCursor(
            Vector3 center,
            Vector3 scale,
            Entity._Position._Direction dir,
            Entity._Team._Teams team,
            double time
        ){
            // This will be a nightmare.
            // For now, were drawing one tri pointing towards the given direction.

            VertexPositionColor[] Verts = new VertexPositionColor[3];

            float angleFwd = ((float)Math.PI / 3) * ((int)dir - 1) + ((float)Math.PI / 6);
            float angleBackL = angleFwd - ((float)Math.PI / 1.45f);
            float angleBackR = angleFwd + ((float)Math.PI / 1.45f);

			Verts = new VertexPositionColor[6];

			Verts[0].Position = new Vector3(
				center.X + scale.X * (float)Math.Cos(angleFwd),
				center.Y + scale.Y * (float)Math.Sin(angleFwd),
				center.Z + 0.1f
			);
			Verts[1].Position = new Vector3(
                center.X + scale.X * (float)Math.Cos(angleBackL),
                center.Y + scale.Y * (float)Math.Sin(angleBackL),
				center.Z + 0.1f
			);
			Verts[2].Position = new Vector3(
				center.X + scale.X * (float)Math.Cos(angleBackR),
				center.Y + scale.Y * (float)Math.Sin(angleBackR),
				center.Z + 0.1f
			);

			Verts[3].Position = new Vector3(
				center.X + scale.X * 1.75f * (float)Math.Cos(angleFwd),
				center.Y + scale.Y * 1.75f * (float)Math.Sin(angleFwd),
                center.Z + 3f + (float)Math.Sin(time)
			);
			Verts[4].Position = new Vector3(
                center.X + (scale.X * 0.8f) * 1.75f * (float)Math.Cos(angleBackL),
				center.Y + (scale.Y * 0.8f) * 1.75f * (float)Math.Sin(angleBackL),
				center.Z + 3f + (float)Math.Sin(time)
			);
			Verts[5].Position = new Vector3(
                center.X + (scale.X * 0.8f) * 1.75f * (float)Math.Cos(angleBackR),
				center.Y + (scale.Y * 0.8f) * 1.75f * (float)Math.Sin(angleBackR),
				center.Z + 3f + (float)Math.Sin(time)
			);

            Color teamColor;
            switch(team){
                case Entity._Team._Teams.BLUE:
                    teamColor = Color.Blue;
                    break;
                case Entity._Team._Teams.RED:
                    teamColor = Color.Red;
                    break;
                default:
                    teamColor = Color.Magenta;
                    break;
            }

			Verts[0].Color = teamColor;
			Verts[1].Color = teamColor;
			Verts[2].Color = teamColor;
			Verts[3].Color = teamColor;
			Verts[4].Color = teamColor;
			Verts[5].Color = teamColor;

            wiretris.AddRange(Verts);
        }

        /// <summary>
        /// Draws a hexagon.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="scale">Scale.</param>
        /// <param name="topColor">Top color.</param>
        /// <param name="sideColor">Side color.</param>
        public void DrawHexagon(
            Vector3 center, Vector3 scale, Color topColor, Color sideColor, bool wire
        ){

            VertexPositionColor[] Verts;

            // Flat hexagon
            if (Equals(scale.Z, 0)) {
                Verts = new VertexPositionColor[6 * 3];

                foreach (var i in Enumerable.Range(0, 6))
                {
                    // Create vertices
                    float angle = ((float)Math.PI / 3) * i + ((float)Math.PI / 6);
                    float angleNext = angle + ((float)Math.PI / 3);

                    // Surface
                    Verts[i * 3 + 0].Position = new Vector3(
                        center.X + scale.X * (float)Math.Cos(angle),
                        center.Y + scale.Y * (float)Math.Sin(angle),
                        center.Z + scale.Z
                    );
                    Verts[i * 3 + 1].Position = new Vector3(
                        center.X, center.Y, center.Z + scale.Z
                    );
                    Verts[i * 3 + 2].Position = new Vector3(
                        center.X + scale.X * (float)Math.Cos(angleNext),
                        center.Y + scale.Y * (float)Math.Sin(angleNext),
                        center.Z + scale.Z
                    );

					// Top color
					Verts[i * 3 + 0].Color = topColor;
					Verts[i * 3 + 1].Color = topColor;
					Verts[i * 3 + 2].Color = topColor;
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

            if (wire) {
                wiretris.AddRange(Verts);
            } else {
                tris.AddRange(Verts);
            }
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
                        DrawHexagon(pos, scale, Color.SpringGreen, Color.SaddleBrown, false);
                        break;
                    case "cursor":
                        DrawCursor(pos, scale, obj.Value.Position.dir, obj.Value.Team.team, gametime.TotalGameTime.TotalSeconds);
                        break;
                    default:
                        //spritebatch.Draw(DebugDot, new Rectangle(centerPos.X, centerPos.Y, 8, 8), Color.Red);
                        DrawHexagon(pos, scale, Color.Magenta, Color.DarkMagenta, false);
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

                if (tris.Any()) {
                    screen.RasterizerState = defaultRasterizer;
                    screen.DrawUserPrimitives(
                        PrimitiveType.TriangleList, tris.ToArray(), 0, tris.Count() / 3
                    );
                }

                if (wiretris.Any()) {
                    screen.RasterizerState = wireframeRasterizer;
                    screen.DrawUserPrimitives(
                        PrimitiveType.TriangleList, wiretris.ToArray(), 0, wiretris.Count() / 3
                    );
                }
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
