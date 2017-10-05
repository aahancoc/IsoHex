using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Xna.Framework;

namespace IsoHex
{
    public static class EntityFactory
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
            return new Entity()
            {
                Position = new Entity._Position()
                {
                    X = x,
                    Y = y,
                    Z = 0,
                    height = height
                },

                Renderable = new Entity._Renderable()
                {
                    pos = new Vector3(x, y, 0),
                    target = new Vector3(x, y, 0),
                    scale = new Vector3(1, 1, height),
                    modelID = "ground",
                    animation = "default"
                },

                Solid = new Entity._Solid()
                {
                    walkable = true
                }
            };
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
                    int height = rng.Next(0, 3);
                    result.Add(GroundFactory(x, y, height));
                }
            }

            return result;
        }

        /// <summary>
        /// Generates the cursor object for a team
        /// </summary>
        /// <returns>A cursor entity.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        static public Entity CursorFactory(int x, int y, int z)
        {
            return new Entity()
            {
                Position = new Entity._Position()
                {
                    X = x,
                    Y = y,
                    Z = z,
                    dir = Entity._Position._Direction.RIGHT
                },

                Renderable = new Entity._Renderable()
                {
                    pos = new Vector3(x, y, z),
                    target = new Vector3(x, y, z),
                    scale = new Vector3(0.75f, 0.75f, 1f),
                    velocity = 5.0f,
                    modelID = "cursor"
                },

                Team = new Entity._Team()
                {
                    team = Entity._Team._Teams.RED
                }
            };
        }

        /// <summary>
        /// Generate a Swordsman entity
        /// </summary>
        /// <returns>A Swordsman entity</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        static public Entity SwordsmanFactory(int x, int y, int z)
        {
            return new Entity()
            {
                Position = new Entity._Position()
                {
                    X = x,
                    Y = y,
                    Z = z,
                    dir = Entity._Position._Direction.RIGHT
                },

                Renderable = new Entity._Renderable()
                {
                    name = "Swordsman",
                    pos = new Vector3(x, y, z),
                    target = new Vector3(x, y, z),
                    velocity = 5.0f,
                    scale = new Vector3(0.5f, 0.5f, 0.8f),
                    modelID = "swordsman",
                    alwaysVisible = true
                },

                Team = new Entity._Team()
                {
                    team = Entity._Team._Teams.RED
                },

                Mobile = new Entity._Mobile()
                {
                    PPCost = 3,
                    PPCostPerTile = 1,
                    stepArray = new Queue<Entity._Position>()
                },

                Intelligent = new Entity._Intelligent()
                {
                    PP = 50,
                    maxPP = 50
                }
            };
        }
    }
}
