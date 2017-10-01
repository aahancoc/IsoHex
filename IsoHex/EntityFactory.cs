using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Xna.Framework;

namespace IsoHex
{
    public class EntityFactory
    {
        public List<Entity> List;

        public EntityFactory()
        {
            // Just make a list of entities
            List = new List<Entity>();
        }

        public Entity GroundFactory(int x, int y, int height)
        {
            Entity ground = new Entity();

            ground.Active =
                Entity._Active.POSITION |
                Entity._Active.RENDERABLE |
                Entity._Active.SOLID;
            
            ground.Position.X = x;
            ground.Position.Y = y;
            ground.Position.Z = 0;
            ground.Position.height = height;

            ground.Renderable.pos = new Vector3(x, y, 0);
            ground.Renderable.height = height;
            ground.Renderable.modelID = "ground";
            ground.Renderable.animation = "default";

            ground.Solid.walkable = true;

            return ground;
        }

        public List<Entity> TerrainFactory(int width, int height)
        {
            List<Entity> result = new List<Entity>();

            foreach(var x in Enumerable.Range(0, width)){
                foreach (var y in Enumerable.Range(0, height)){
                    result.Add(GroundFactory(x, y, 1));
                }
            }

            return result;
        }
    }
}
