using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace IsoHex
{
    public struct TerrainUtils
    {
		/// <summary>
		/// Returns a list of JUST terrain entities
		/// </summary>
		/// <returns>The terrain.</returns>
		/// <param name="list">Entity list.</param>
		static public IEnumerable<KeyValuePair<Guid,Entity>> FilterTerrain(Dictionary<Guid, Entity> list){
			var query =
				from e in list
                where e.Value.Position.HasValue &&
					  e.Value.Solid.HasValue &&
                      e.Value.Solid.Value.walkable
				select e;

            return query;
        }

        /// <summary>
        /// Given a known x/y tile position, retreive the highest Z
        /// </summary>
        /// <returns>The height of the tile.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="list">Entity list.</param>
        static public int GetHeightFromTile(int x, int y, Dictionary<Guid, Entity> list){
            var query =
                from e in list
                where e.Value.Position.HasValue &&
					  e.Value.Solid.HasValue &&
					  e.Value.Solid.Value.walkable &&
                      e.Value.Position.Value.X == x &&
                      e.Value.Position.Value.Y == y
                orderby (e.Value.Position.Value.Z + e.Value.Position.Value.height) descending
            	select (e.Value.Position.Value.Z + e.Value.Position.Value.height);

            foreach (int z in query){return z;}
            return -1;
        }

		/// <summary>
		/// Given a known x/y world position, retreives the highest Z
		/// </summary>
		/// <returns>The height at the given world coordinates.</returns>
		/// <param name="pos">Position.</param>
		/// <param name="list">Entity list.</param>
		static public float GetHeightFromWorld(Vector2 pos, Dictionary<Guid, Entity> list){
            // Just find the nearest tile and get the height of that!
            Guid nearID = GetNearestTile(pos, list);
            Entity nearTile = list[nearID];
            return nearTile.Position.Value.Z + nearTile.Position.Value.height;
        }

		/// <summary>
		/// Given a 2D vector, finds the nearest ground tile
		/// </summary>
		/// <returns>The nearest tile.</returns>
		/// <param name="src">Source Position.</param>
		/// <param name="list">Entity list.</param>
		static public Guid GetNearestTile(Vector2 src, Dictionary<Guid, Entity> list)
        {
            src = CoordUtils.GetWorldPosition(src);
            float shortestDist = float.PositiveInfinity;
            Guid shortestID;

            // Iterate through all ground tiles
            foreach(var obj in FilterTerrain(list)){
                Vector2 tmpVec = CoordUtils.GetWorldPosition(new Vector2(
                    obj.Value.Position.Value.X, obj.Value.Position.Value.Y
                ));
                float tmpDist = Vector2.Distance(src, tmpVec);

                if(tmpDist < shortestDist){
                    shortestDist = tmpDist;
                    shortestID = obj.Key;
                }
            }
            return shortestID;
        }

		/// <summary>
		/// Given a 3D vector, finds the nearest ground tile
		/// </summary>
		/// <returns>The nearest tile.</returns>
		/// <param name="src">Source Position.</param>
		/// <param name="list">Entity list.</param>
		static public Guid GetNearestTile(Vector3 src, Dictionary<Guid, Entity> list)
		{
            src = CoordUtils.GetWorldPosition(src);
			float shortestDist = float.PositiveInfinity;
			Guid shortestID;

			// Iterate through all ground tiles
			foreach (var obj in FilterTerrain(list))
			{
                Vector3 tmpVec = CoordUtils.GetWorldPosition(new Vector3(
                    obj.Value.Position.Value.X, 
                    obj.Value.Position.Value.Y,
                    obj.Value.Position.Value.Z
                ));
				float tmpDist = Vector3.Distance(src, tmpVec);

				if (tmpDist < shortestDist)
				{
					shortestDist = tmpDist;
					shortestID = obj.Key;
				}
			}

			return shortestID;
		}
    }
}
