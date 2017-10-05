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
        /// <param name="_render">Render Component (required).</param>
        /// <param name="dt">delta time. (seconds)</param>
		static void RenderPos (
            ref Entity._Renderable? _render,
            float dt
        ){
            if (!_render.HasValue) { return; }
            var render = _render.Value;
            if (render.pos == render.target) { return; }

            // I eventually want to do some interloptation thing.
            // But for now, let's get boring and linear
            Vector3 deltapos = render.target - render.pos;
            Vector3 newpos = render.velocity * dt * Vector3.Normalize(deltapos);

            if (deltapos.LengthSquared() < newpos.LengthSquared()) {
                render.pos = render.target;
            } else {
                render.pos += newpos;
            }

            _render = render;
        }

		/// <summary>
		/// Moves the entity if its visible and it has reached its target
        /// Will return parasite host chain if needed
		/// </summary>
		/// <param name="_mobile">Mobile Component (required).</param>
		/// <param name="_pos">Position Component (required).</param>
		/// <param name="_render">Render Component.</param>
		/// <param name="_parasite">Parasite Component.</param>
        static KeyValuePair<Guid, Entity>? MoveEntity(
			ref Entity._Mobile? _mobile,
			ref Entity._Position? _pos,
			ref Entity._Renderable? _render,
            ref Entity._Parasite? _parasite,
            Dictionary<Guid, Entity> entities
		) {

            // Requires Position and Mobile
            if(!_mobile.HasValue || !_pos.HasValue){
                return null;
            }
            var mobile = _mobile.Value;
            var pos = _pos.Value;

			if (!mobile.moving) { return null; } // If we're not moving, don't move
			if (mobile.stepArray.Count == 0) {
				// If we can't move, don't
				mobile.moving = false;
                _mobile = mobile;
				return null;
			}

            Console.WriteLine("moving...");

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

            // Update visible position if applicable
            if (_render.HasValue) {
                var render = _render.Value; 
                // Update render target position
                render.target = mobileTgtPos;

                // Pop step and (if needed) move host if done with command
                if (render.pos == render.target) {
                    Console.WriteLine("Done with step!");
                    mobile.stepArray.Dequeue();
                }

                _render = render;
            } else {
                // Immediately deque
                mobile.stepArray.Dequeue();
            }

            // Set entity position and mobile struct, because we are done.
			_pos = pos;
			_mobile = mobile;

            // Update host position if applicable
            if(
                _parasite.HasValue &&
                _parasite.Value.moveType == Entity._Parasite._MoveType.MOVEHOST
            ){
                Entity host = entities[_parasite.Value.hostID];
                if (!host.Position.HasValue) { return null;  }
                var hostpos = host.Position.Value;

				hostpos.X = mobileTgt.X;
				hostpos.Y = mobileTgt.Y;
				hostpos.Z = mobileTgt.Z;

                host.Position = hostpos;

                if (host.Renderable.HasValue) {
                    var render = host.Renderable.Value;
                    render.target = mobileTgtPos;
                    host.Renderable = render;
                }

                return new KeyValuePair<Guid, Entity>(_parasite.Value.hostID, host);
            }

            // Update things attached TO this entity if applicable

            return null;
		}

        /// <summary>
        /// Sends keyboard inputs to an entity's step queue.
        /// </summary>
        /// <param name="_pos">Position.</param>
        /// <param name="_mobile">Mobile.</param>
        /// <param name="_int">Intelligent.</param>
        /// <param name="list">List.</param>
        static void InputToStepQueue(
            ref Entity._Position? _pos,
            ref Entity._Mobile? _mobile,
            ref Entity._Intelligent? _int,
            Dictionary<Guid, Entity> list
        ){
            if (
                !_mobile.HasValue ||
                !_int.HasValue ||
                !_pos.HasValue
            ) { return; }

            var pos = _pos.Value;
            var mobile = _mobile.Value;

			foreach (var key in Keyboard.GetState().GetPressedKeys())
			{
				Entity._Position newpos = pos;
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

				mobile.moving = true;
				mobile.stepArray.Enqueue(newpos);
			}

            _mobile = mobile;
            _pos = pos;
        }

        /// <summary>
        /// Updates the entity's components
        /// </summary>
        /// <param name="_entity">Entity.</param>
        /// <param name="gametime">Gametime.</param>
        /// <param name="list">Entity list</param>
        static Dictionary<Guid, Entity> UpdateEntity (
            KeyValuePair<Guid, Entity> _entity,
            Dictionary<Guid, Entity> list, // REMOVE ME PLEASE
            GameTime gametime
        ){
            Dictionary<Guid, Entity> retval = new Dictionary<Guid, Entity>();
            float dt = (float)gametime.ElapsedGameTime.TotalSeconds;
            Entity entity = _entity.Value;

            // Get input into the step queue
            InputToStepQueue(
                ref entity.Position,
                ref entity.Mobile,
                ref entity.Intelligent,
                list
            );

            // Move object if it was told to move
            var tmp = MoveEntity(
                ref entity.Mobile,
                ref entity.Position,
				ref entity.Renderable,
                ref entity.Parasite,
                list
            );
            if(tmp.HasValue){
                retval.Add(tmp.Value.Key, tmp.Value.Value);
            }

			// Update visible position if not at target
			RenderPos(ref entity.Renderable, dt);

            // Add our entity if its changed and return
            if(!_entity.Value.Equals(entity)){
				retval.Add(_entity.Key, entity);
            }
            return retval;
        }

        static public void UpdateAll (ref Dictionary<Guid, Entity> list, GameTime gametime){

            // Updated list
            Dictionary<Guid, Entity> updated = new Dictionary<Guid, Entity>();

            // Entities
            foreach(var obj in list){
                Dictionary<Guid, Entity> newentities = 
                    UpdateEntity(obj, list, gametime);
                foreach(var x in newentities){
                    updated.Add(x.Key, x.Value);
                }
            }

            // Replace old entities with new ones
            foreach(var ent in updated){
                if(list.ContainsKey(ent.Key)){
                    list[ent.Key] = ent.Value;
                } else {
                    list.Add(ent.Key, ent.Value);
                }
            }
        }
    }
}
