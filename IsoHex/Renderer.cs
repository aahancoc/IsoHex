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
        Vector3 cameraPos = Vector3.Zero;
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

		// Taken from https://blogs.msdn.microsoft.com/rezanour/2011/08/07/barycentric-coordinates-and-point-in-triangle-tests/
		///<summary>
		/// Determine whether a point P is inside the triangle ABC. Note, this function
		/// assumes that P is coplanar with the triangle.
		///</summary>
		///<returns>True if the point is inside, false if it is not.</returns>
		public static bool PointInTriangle(ref Vector3 A, ref Vector3 B, ref Vector3 C, ref Vector3 P)
		{

			// Prepare our barycentric variables
			Vector3 u = B - A;
			Vector3 v = C - A;
			Vector3 w = P - A;

			Vector3 vCrossW = Vector3.Cross(v, w);
			Vector3 vCrossU = Vector3.Cross(v, u);

			// Test sign of r
			if (Vector3.Dot(vCrossW, vCrossU) < 0)
				return false;
            
			Vector3 uCrossW = Vector3.Cross(u, w);
			Vector3 uCrossV = Vector3.Cross(u, v);

            // Test sign of t
            if (Vector3.Dot(uCrossW, uCrossV) < 0) {
                return false;
            }
            
			// At this point, we know that r and t and both > 0.
			// Therefore, as long as their sum is <= 1, each must be less <= 1
			float denom = uCrossV.Length();
			float r = vCrossW.Length() / denom;
			float t = uCrossW.Length() / denom;

			return (r + t <= 1);
		}

        /// <summary>
        /// Set up the camera.
        /// </summary>
        /// <param name="ui">UI/Camera parameters.</param>
        /// <param name="entities">Entity list.</param>
        public void SetupCamera(UI ui, Dictionary<Guid, Entity> entities){

            // Set camera focus
            SetFocus(ui.cursorID, entities);

            // Set angle and position
            cameraPos = new Vector3(
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
        public VertexPositionColor[] DrawCursor(
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

            return Verts;
        }

        /// <summary>
        /// Draws a hexagon.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="scale">Scale.</param>
        /// <param name="topColor">Top color.</param>
        /// <param name="sideColor">Side color.</param>
        public VertexPositionColor[] DrawHexagon(
            Vector3 center, Vector3 scale, Color topColor, Color sideColor
        ){

            VertexPositionColor[] Verts;
            int tricount;
            int vtxPerSide;

            // Flat hexagon
            if (Equals(scale.Z, 0)) {
                tricount = 6;
                vtxPerSide = 3;
            } else {
                tricount = 18;
                vtxPerSide = 9;
            }

            Verts = new VertexPositionColor[3 * tricount];

            // Surface vertices
            foreach (var i in Enumerable.Range(0, 6)) {
				float angle = ((float)Math.PI / 3) * i + ((float)Math.PI / 6);
				float angleNext = angle + ((float)Math.PI / 3);

                Verts[i * vtxPerSide + 0].Position = new Vector3(
                    center.X + scale.X * (float)Math.Cos(angle),
                    center.Y + scale.Y * (float)Math.Sin(angle),
                    center.Z + scale.Z
                );
                Verts[i * vtxPerSide + 1].Position = new Vector3(
                    center.X, center.Y, center.Z + scale.Z
                );
                Verts[i * vtxPerSide + 2].Position = new Vector3(
                    center.X + scale.X * (float)Math.Cos(angleNext),
                    center.Y + scale.Y * (float)Math.Sin(angleNext),
                    center.Z + scale.Z
                );

				// Top color
				Verts[i * vtxPerSide + 0].Color = topColor;
				Verts[i * vtxPerSide + 1].Color = topColor;
				Verts[i * vtxPerSide + 2].Color = topColor;
            }

            // 3D hexagon (no bottom)
            if (tricount > 6) {
				foreach (var i in Enumerable.Range(0, 6)) {
					float angle = ((float)Math.PI / 3) * i + ((float)Math.PI / 6);
					float angleNext = angle + ((float)Math.PI / 3);

					// Sides (1)
					Verts[i * vtxPerSide + 3].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angle),
						center.Y + scale.Y * (float)Math.Sin(angle),
						center.Z + scale.Z
					);
					Verts[i * vtxPerSide + 4].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angleNext),
						center.Y + scale.Y * (float)Math.Sin(angleNext),
						center.Z + scale.Z
					);
					Verts[i * vtxPerSide + 5].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angle),
						center.Y + scale.Y * (float)Math.Sin(angle),
						center.Z
					);

					// Sides (2)
					Verts[i * vtxPerSide + 8].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angle),
						center.Y + scale.Y * (float)Math.Sin(angle),
						center.Z
					);
					Verts[i * vtxPerSide + 7].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angleNext),
						center.Y + scale.Y * (float)Math.Sin(angleNext),
						center.Z
					);
					Verts[i * vtxPerSide + 6].Position = new Vector3(
						center.X + scale.X * (float)Math.Cos(angleNext),
						center.Y + scale.Y * (float)Math.Sin(angleNext),
						center.Z + scale.Z
					);

					// Side colors
					Verts[i * vtxPerSide + 3].Color = sideColor;
					Verts[i * vtxPerSide + 4].Color = sideColor;
					Verts[i * vtxPerSide + 5].Color = sideColor;

					Verts[i * vtxPerSide + 6].Color = sideColor;
					Verts[i * vtxPerSide + 7].Color = sideColor;
					Verts[i * vtxPerSide + 8].Color = sideColor;
				}
            }

            // Add to render list
            return Verts;
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
            if (!entity.Renderable.HasValue) { return false; }

            cameraLookAt = CoordUtils.GetWorldPosition(entity.Renderable.Value.pos);
			return true;
        }

        /// <summary>
        /// Draws the entities.
        /// </summary>
        /// <param name="list">Entity list.</param>
        /// <param name="gametime">Gametime.</param>
        public void DrawEntities(Dictionary<Guid, Entity> list, GameTime gametime){
            
            // Get every renderable thing
            foreach (var obj in list.Where(
                x => x.Value.Renderable.HasValue
            )){
                
                // Convert coordinates
                Vector3 pos = 
                    CoordUtils.GetWorldPosition(obj.Value.Renderable.Value.pos);
                Vector3 scale = 
                    CoordUtils.GetWorldScale(obj.Value.Renderable.Value.scale);

                // Render object depending on model ID
                VertexPositionColor[] Verts;
				bool isWireframe = false;

                switch(obj.Value.Renderable.Value.modelID){
                    case "ground":
                        Verts = DrawHexagon(
                            pos,
                            scale,
                            Color.SpringGreen,
                            Color.SaddleBrown
                        );
                        break;
                    case "cursor":
                        isWireframe = true; // always wireframe
                        Verts = DrawCursor(
                            pos, 
                            scale, 
                            obj.Value.Position.Value.dir,
                            obj.Value.Team.Value.team, 
                            gametime.TotalGameTime.TotalSeconds
                        );
                        break;
                    default:
                        Verts = DrawHexagon(
                            pos,
                            scale,
                            Color.Magenta, 
                            Color.DarkMagenta
                        );
                        break;
                }

                // If we know its wireframe, just draw it
                if(isWireframe){
                    wiretris.AddRange(Verts);
                    continue;
                }

                // Otherwise check for clipping collisions with must-be-drawn objects
                foreach(var mbd in list.Where(
                    x => x.Key != obj.Key &&
                    x.Value.Renderable.HasValue &&
                    x.Value.Renderable.Value.alwaysVisible
                )){
					// Get center of object and direction from object to camera
					Vector3 mbdCenter = 
                        CoordUtils.GetWorldPosition(mbd.Value.Renderable.Value.pos);
					Vector3 mbdScale = 
                        CoordUtils.GetWorldScale(mbd.Value.Renderable.Value.scale);
                    mbdCenter.Z += 0.5f * mbdScale.Z;

					Vector3 dir = Vector3.Normalize(cameraPos - mbdCenter);

                    // Make ray from left side to camera
                    Vector3 mbdLeft = mbdCenter;
                    mbdLeft.X -= 1.5f * mbdScale.X * (float)Math.Cos(dir.X);
                    mbdLeft.Y -= 1.5f * mbdScale.Y * (float)Math.Sin(dir.Y);

                    // And the left side
					Vector3 mbdRight = mbdCenter;
					mbdRight.X += 1.5f * mbdScale.X * (float)Math.Cos(dir.X);
					mbdRight.Y += 1.5f * mbdScale.Y * (float)Math.Sin(dir.Y);

                    // Make the directions the opposite for maximum range
					Ray rayL = new Ray(mbdLeft, cameraPos - mbdRight);
                    Ray rayR = new Ray(mbdRight, cameraPos - mbdLeft);

                    //Console.WriteLine("RayL: " + rayL);// + "\nRayR: " + rayR);

                    // Run through every triangle we're about to render
                    foreach(var i in Enumerable.Range(0, Verts.Count() / 3)){
                        // Generate plane from triangle points
                        Plane currTri = new Plane(
                            Verts[i * 3 + 0].Position,
                            Verts[i * 3 + 1].Position,
                            Verts[i * 3 + 2].Position
                        );

                        // Determine if and where ray intersects plane
                        float? distL = rayL.Intersects(currTri);
                        float? distR = rayR.Intersects(currTri);

                        //Console.WriteLine("DistL: " + distL + "\nDistR: " + distR + "\n");

                        // Determine if left intersection point is within triangle
                        if (distL.HasValue) {
                            Vector3 isectL = rayL.Position + distL.Value * rayL.Direction;
                            if (PointInTriangle(
                                ref Verts[i * 3 + 0].Position,
                                ref Verts[i * 3 + 1].Position,
                                ref Verts[i * 3 + 2].Position,
                                ref isectL
                            ))
                            {
                                isWireframe = true;
                                break;
                            }
                        }

						// Determine if right intersection point is within triangle
						if (distR.HasValue) {
							Vector3 isectR = rayR.Position + distR.Value * rayR.Direction;
                            if (PointInTriangle(
                                ref Verts[i * 3 + 0].Position,
                                ref Verts[i * 3 + 1].Position,
                                ref Verts[i * 3 + 2].Position,
                                ref isectR
                            ))
                            {
                                isWireframe = true;
                                break;
                            }
						}

                        // Nope, it's clear. Next triangle.
                    }
                }

				// Add vertices to correct list
				if (isWireframe) {
					wiretris.AddRange(Verts);
                } else {
                    tris.AddRange(Verts);
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
