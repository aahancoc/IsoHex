using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace IsoHex
{
    /// <summary>
    /// Main update struct for entities
    /// </summary>
    public static class Updater
    {
		/// <summary>
		/// Update visible position of rendered object
		/// </summary>
		/// <param name="render">Render Component.</param>
        /// <param name="dt">delta time. (seconds)</param>
		static void RenderPos (
            ref Entity._Renderable render,
            float dt
        ){
            if (render.pos == render.target) { return; }

            // I eventually want to do some interloptation thing.
            // But for now, let's get boring and linear

            Vector3 deltapos = render.target - render.pos;
            Vector3 newpos = render.velocity * dt * Vector3.Normalize(deltapos);
            //Vector3.Clamp(newpos, Vector3.Zero, deltapos);

            if (deltapos.LengthSquared() < newpos.LengthSquared()) {
                render.pos = render.target;
            } else {
                render.pos += newpos;
            }
        }

		/// <summary>
		/// Moves the entity if its visible and it has reached its target
		/// </summary>
		/// <param name="render">Render Component.</param>
		/// <param name="mobile">Mobile Component.</param>
		/// <param name="pos">Position Component.</param>
        /// <param name="parasite">Parasite Component.</param>
		static void MoveEntity(
			ref Entity._Renderable render,
			ref Entity._Mobile mobile,
			ref Entity._Position pos,
            ref Entity._Parasite parasite
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

            // Pop step and (if needed) move host if done with command
			if (render.pos == render.target)
			{
				Console.WriteLine("Done with step!");
				mobile.stepArray.Dequeue();
			}
		}

        /// <summary>
        /// Moves the entity if its visible and it has reached its target
        /// </summary>
        /// <param name="render">Render Component.</param>
        /// <param name="mobile">Mobile Component.</param>
        /// <param name="pos">Position Component.</param>
        static void MoveEntity (
            ref Entity._Renderable render,
            ref Entity._Mobile mobile,
            ref Entity._Position pos
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
                Console.WriteLine("Done with step!");
                mobile.stepArray.Dequeue();
            }
        }

        /// <summary>
        /// Moves the entity one tile instantly if it can't be rendered
        /// </summary>
        /// <param name="mobile">Mobile Component.</param>
        /// <param name="pos">Position Component.</param>
		static void MoveEntity(
            ref Entity._Mobile mobile,
            ref Entity._Position pos
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
            mobile.stepArray.Dequeue();
		}

        /// <summary>
        /// Updates the entity's components
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="gametime">Gametime.</param>
        static Entity UpdateEntity (
            Entity entity,
            Dictionary<Guid, Entity> list, // REMOVE ME PLEASE
            GameTime gametime
        ){

            float dt = (float)gametime.ElapsedGameTime.TotalSeconds;

            // Check input (quick and dirty)
            if (
                entity.Active.HasFlag(Entity._Components.MOBILE) &&
                entity.Active.HasFlag(Entity._Components.INTELLIGENT) &&
                entity.Active.HasFlag(Entity._Components.POSITION)
            ) {
                foreach (var key in Keyboard.GetState().GetPressedKeys()) {
                    entity.Mobile.moving = true;
					Entity._Position newpos = entity.Position;
                    Console.WriteLine(key + " pressed");
                    switch (key) {
                        case Keys.Left:
                            newpos.X -= 1;
                            break;
						case Keys.Right:
							newpos.X += 1;
                            break;
						case Keys.Up:
							newpos.Y += 1;
                            break;
						case Keys.Down:
							newpos.Y -= 1;
                            break;
                    }
                    newpos.Z = TerrainUtils.GetHeightFromTile(newpos.X, newpos.Y, list);

                    entity.Mobile.stepArray.Enqueue(newpos);
                }
            }

            // Move object if it was told to move
			if (
                entity.Active.HasFlag(Entity._Components.POSITION) &&
                entity.Active.HasFlag(Entity._Components.RENDERABLE) &&
                entity.Active.HasFlag(Entity._Components.MOBILE)
            ){
                // Update position if we've visibly reached our first step
                MoveVisibleEntity(
                    ref entity.Renderable,
                    ref entity.Mobile,
                    ref entity.Position
                );
			} else if (
				entity.Active.HasFlag(Entity._Components.POSITION) &&
				entity.Active.HasFlag(Entity._Components.MOBILE)
            ){
                // Update position instantly
                MoveEntity(
                    ref entity.Mobile,
                    ref entity.Position
                );
            }

			// Update visible position if not at target
			if (entity.Active.HasFlag(Entity._Components.RENDERABLE))
			{
				RenderPos(
					ref entity.Renderable,
					dt
				);
			}

            return entity;
        }

        static public void UpdateAll (ref Dictionary<Guid, Entity> list, GameTime gametime){

            // Updated list
            Dictionary<Guid, Entity> updated = new Dictionary<Guid, Entity>();

            // Entities
            foreach(var obj in list){
                Entity newentity = UpdateEntity(obj.Value, list, gametime);
                if(obj.Value != newentity){
                    updated.Add(obj.Key, newentity);
                }
            }

            // Replace old entities with new ones
            foreach(var ent in updated){
                list[ent.Key] = ent.Value;
            }
        }
    }
}
