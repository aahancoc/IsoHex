using System;
using Microsoft.Xna.Framework;

namespace IsoHex
{
    /// <summary>
    /// Main update struct for entities
    /// </summary>
    public struct Update
    {
		/// <summary>
		/// Update visible position of rendered object
		/// </summary>
		/// <param name="render">Render Component.</param>
        /// <param name="dt">delta time. (seconds)</param>
		static void RenderPos (Entity._Renderable render, float dt){
            if (render.pos == render.target) { return; }

            // I eventually want to do some interloptation thing.
            // But for now, let's get boring and linear

            Vector3 deltapos = render.target - render.pos;
            Vector3 newpos = render.velocity * dt * Vector3.Normalize(deltapos);
            Vector3.Clamp(newpos, Vector3.Zero, deltapos);

            render.pos = newpos;
        }

        /// <summary>
        /// Moves the entity if its visible and it has reached its target
        /// </summary>
        /// <param name="render">Render Component.</param>
        /// <param name="mobile">Mobile Component.</param>
        /// <param name="pos">Position Component.</param>
        static void MoveVisibleEntity (
            Entity._Renderable render,
            Entity._Mobile mobile,
            Entity._Position pos
        ){
            if (!mobile.moving) { return; } // If we're not moving, don't move
            if (mobile.stepArray.Count == 0) {
                // If we can't move, don't
                mobile.moving = false;
                return; 
            }

            // Get our target position in a more usable form
            Entity._Position mobileTgt = mobile.stepArray.Peek();
            Vector3 mobileTgtPos = new Vector3(
                mobileTgt.X,
                mobileTgt.Y,
                mobileTgt.Z
            );

            // Update actual position
            pos.X = mobileTgt.X;
            pos.Y = mobileTgt.Y;
            pos.Z = mobileTgt.Z;

            // Update render target position
            render.target = mobileTgtPos;

            // Pop step if done with command
            if(render.pos == render.target){
                mobile.stepArray.Pop();
            }
        }

        /// <summary>
        /// Moves the entity one tile instantly if it can't be rendered
        /// </summary>
        /// <param name="mobile">Mobile Component.</param>
        /// <param name="pos">Position Component.</param>
		static void MoveEntity(
			Entity._Mobile mobile,
			Entity._Position pos
		)
		{
			if (!mobile.moving) { return; } // If we're not moving, don't move
			if (mobile.stepArray.Count == 0)
			{
				// If we can't move, don't
				mobile.moving = false;
				return;
			}

			// Get our target position in a more usable form
			Entity._Position mobileTgt = mobile.stepArray.Peek();

			// Update actual position
			pos.X = mobileTgt.X;
			pos.Y = mobileTgt.Y;
			pos.Z = mobileTgt.Z;

            // We're done with step, so pop it
			mobile.stepArray.Pop();
		}

        /// <summary>
        /// Updates the entity's components
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="gametime">Gametime.</param>
        static public void UpdateEntity (Entity entity, GameTime gametime){

            float dt = (float)gametime.ElapsedGameTime.TotalSeconds;

            // Update visible position if not at target
            if(entity.Active.HasFlag(Entity._Components.RENDERABLE)){
                RenderPos(entity.Renderable, dt);
            }

            // Move object if it was told to move
			if (
                entity.Active.HasFlag(Entity._Components.POSITION) &&
                entity.Active.HasFlag(Entity._Components.RENDERABLE) &&
                entity.Active.HasFlag(Entity._Components.MOBILE)
            ){
				// Update position if we've visibly reached our first step
				MoveVisibleEntity(entity.Renderable, entity.Mobile, entity.Position);
			} else if (
				entity.Active.HasFlag(Entity._Components.POSITION) &&
				entity.Active.HasFlag(Entity._Components.MOBILE)
            ){
                // Update position instantly
                MoveEntity(entity.Mobile, entity.Position);
            }

        }
    }
}
