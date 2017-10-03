using System;
using Microsoft.Xna.Framework;

namespace IsoHex
{
    /// <summary>
    /// Camera and UI properties
    /// </summary>
    public class UI
    {
        // Camera stuff
        public Guid cursorEntity;
        public float zoomLevel = 50.0f;
        public float pitch = (float)Math.Asin(1 / Math.Sqrt((2)));
        public float yaw = MathHelper.PiOver2;

        // UI stuff
        public enum _attribStatus {
            CLOSED, // attributes pane is closed
            MAIN,   // attributes pane is open to main menu
            ACTION, // attributes pane has action subwindow open
            MOVE,   // attributes pane is selecting space to move to
            STATS,  // attributes pane has stats subwindow open
        };
		public enum _attribOption
		{
            BACK,   // go to previous menu
			ACTION, // attributes pane has action subwindow open
			DEFEND, // defend subwindow open?
			MOVE,   // attributes pane is selecting space to move to
			STATS,  // attributes pane has stats subwindow open
			ENDTURN // end turn selection
		};

        public _attribStatus attribStatus; // State of attribute pane
        public _attribOption attribSelected; // Selected option in main menu
    }
}
