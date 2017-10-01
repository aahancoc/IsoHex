using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace IsoHex
{
    public struct TerrainUtils
    {
        // Returns a list of JUST terrain entities
        static IEnumerable<KeyValuePair<Guid,Entity>> FilterTerrain(Dictionary<Guid, Entity> list){
			var query =
				from e in list
				where e.Value.Active.HasFlag(Entity._Components.POSITION) &&
					  e.Value.Active.HasFlag(Entity._Components.SOLID) &&
                      e.Value.Solid.walkable
				select e;

            return query;
        }

        // Given a known x/y tile position, retreive the highest Z
        static int GetHeightTile(int x, int y, Dictionary<Guid, Entity> list){
            var query =
                from e in list
                where e.Value.Active.HasFlag(Entity._Components.POSITION) &&
					  e.Value.Active.HasFlag(Entity._Components.SOLID) &&
					  e.Value.Solid.walkable &&
                      e.Value.Position.X == x &&
                      e.Value.Position.Y == y
                orderby e.Value.Position.Z descending
            	select e.Value.Position.Z;

            foreach (int z in query){return z;}
            return -1;
        }

        // Given a known x/y world position, retrives the highest Z

        // Given a 2D vector, finds the nearest ground tile
        static Guid GetNearestTile(Vector2 src, Dictionary<Guid, Entity> list)
        {
            src = CoordUtils.GetWorldPosition(src);
            float shortestDist = float.PositiveInfinity;
            Guid shortestID;

            // Iterate through all ground tiles
            foreach(var obj in FilterTerrain(list)){
                Vector2 tmpVec = CoordUtils.GetWorldPosition(new Vector2(
                    obj.Value.Position.X, obj.Value.Position.Y
                ));
                float tmpDist = Vector2.Distance(src, tmpVec);

                if(tmpDist < shortestDist){
                    shortestDist = tmpDist;
                    shortestID = obj.Key;
                }
            }
            return shortestID;
        }

        // Given a 3D vector, finds the nearest ground tile
		static Guid GetNearestTile(Vector3 src, Dictionary<Guid, Entity> list)
		{
            src = CoordUtils.GetWorldPosition(src);
			float shortestDist = float.PositiveInfinity;
			Guid shortestID;

			// Iterate through all ground tiles
			foreach (var obj in FilterTerrain(list))
			{
                Vector3 tmpVec = CoordUtils.GetWorldPosition(new Vector3(
                    obj.Value.Position.X, obj.Value.Position.Y, obj.Value.Position.Z
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
