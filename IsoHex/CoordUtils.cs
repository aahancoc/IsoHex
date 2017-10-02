using System;
using Microsoft.Xna.Framework;
namespace IsoHex
{
    /// <summary>
    /// Utilities for converting coordinates
    /// </summary>
    public struct CoordUtils
    {

        // constants
		public const float hexsize = 4.0f;
		const float sqrt3 = 1.73205080757f; // sqrt(3)
		const float hexheight = hexsize * 2.0f;
		const float hexwidth = (sqrt3 / 2) * hexheight;

		/// <summary>
		/// Adjust position from tile coordinates to world coordinates
		/// We are using "odd-r horizontal layout"
		/// </summary>
		/// <returns>The world position.</returns>
		/// <param name="pos">Position.</param>
		static public Vector3 GetWorldPosition(Vector3 pos)
		{
			Vector3 result = new Vector3();

			// X should be offset + width/2 if on an odd row
			float oddness = (float)Math.Abs((pos.Y % 2.0) - 1);
			result.X = ((pos.X * hexwidth) + ((hexwidth / 2f) * oddness));

			// Y should be multiplied by a scaler
			result.Y = (pos.Y * (hexheight * 3.0f / 4.0f));

			// Z should be multiplied by a scaler.
			result.Z = (pos.Z * 10);

			return result;
		}

		/// <summary>
		/// Adjust position from tile coordinates to world coordinates
		/// We are using "odd-r horizontal layout"
		/// </summary>
		/// <returns>The world position.</returns>
		/// <param name="pos">Position.</param>
		static public Vector2 GetWorldPosition(Vector2 pos)
		{
			Vector3 src = new Vector3(pos, 0);
			Vector3 dest = GetWorldPosition(src);
			return new Vector2(dest.X, dest.Y);
		}

		/// <summary>
		/// Scale from tile coords to world coords
		/// </summary>
		/// <returns>The world scale.</returns>
		/// <param name="scale">Scale.</param>
		static public Vector3 GetWorldScale(Vector3 scale)
		{
			Vector3 result = new Vector3();
			result.X = scale.X * hexsize;
			result.Y = scale.Y * hexsize;
			result.Z = scale.Z * 10;
			return result;
		}
    }
}
