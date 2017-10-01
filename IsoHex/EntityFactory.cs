using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Xna.Framework;

namespace IsoHex
{
    public class EntityFactory
    {
        /// <summary>
        /// Generates a ground tile entity
        /// </summary>
        /// <returns>A ground tile entity.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="height">Height.</param>
        static public Entity GroundFactory(int x, int y, int height)
        {
            Entity ground = new Entity();

            ground.Active =
                Entity._Components.POSITION |
                Entity._Components.RENDERABLE |
                Entity._Components.SOLID;
            
            ground.Position.X = x;
            ground.Position.Y = y;
            ground.Position.Z = 0;
            ground.Position.height = height;

            ground.Renderable.pos = new Vector3(x, y, 0);
            ground.Renderable.scale = new Vector3(1, 1, height);
            ground.Renderable.target = ground.Renderable.pos;
            ground.Renderable.modelID = "ground";
            ground.Renderable.animation = "default";

            ground.Solid.walkable = true;

            return ground;
        }

        /// <summary>
        /// Generates many ground tiles comprising a whole rectangular playfield
        /// </summary>
        /// <returns>List of ground tile entities</returns>
        /// <param name="width">Width.</param>
        /// <param name="depth">Depth.</param>
        static public List<Entity> TerrainFactory(int width, int depth)
        {
            List<Entity> result = new List<Entity>();
			Random rng = new Random();

            foreach(var x in Enumerable.Range(0, width)){
                foreach (var y in Enumerable.Range(0, depth)){
                    int height = rng.Next(1, 5);
                    result.Add(GroundFactory(x, y, height));
                }
            }

            return result;
        }

        /// <summary>
        /// Generate a Swordsman entity
        /// </summary>
        /// <returns>A Swordsman entity</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        static public Entity SwordsmanFactory(int x, int y, int z){
            Entity chara = new Entity();

            chara.Active =
                     Entity._Components.POSITION |
                     Entity._Components.RENDERABLE |
                     Entity._Components.TEAM |
                     Entity._Components.MOBILE |
                     Entity._Components.INTELLIGENT;

            chara.Position.X = x;
            chara.Position.Y = y;
            chara.Position.Z = z;
            chara.Position.dir = Entity._Position._Direction.RIGHT;

            chara.Renderable.name = "Swordsman";
            chara.Renderable.pos = new Vector3(x, y, z);
            chara.Renderable.target = chara.Renderable.pos;
            chara.Renderable.scale = new Vector3(0.5f, 0.5f, 0.8f);
            chara.Renderable.modelID = "swordsman";

            chara.Team.team = Entity._Team._Teams.RED;

            chara.Mobile.PPCost = 3;
            chara.Mobile.PPCostPerTile = 1;

            chara.Intelligent.PP = 50;
            chara.Intelligent.maxPP = 50;

            return chara;
        }
    }
}
