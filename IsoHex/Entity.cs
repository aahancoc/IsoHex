using System;
namespace IsoHex.Entity
{
    public class Entity
    {
        Guid ID;
        public _Position Position;
        public _Renderable Renderable;
        public _Team Team;

        public delegate void SelfDelegate(Entity x);

        public struct _Position
        {
            public int X; // tiles
            public int Y; // tiles
            public int Z; // tiles
            public int height; // tiles
            public int dir; // 0-5, 0 = back-right 6 = front-right
        }

        public struct _Renderable
        {
            public string Name;
            public float X; // fraction of tile
            public float Y; // fraction of tile
            public float Z; // fraction of tile
            public bool alwaysVisible; // if true, render any blocking 
                                       // objects as wireframe
            public bool hidden; // object can be visible, but isn't right now

            public string modelID;
            public string animation;
            public int animationFrame;
        }

        public struct _Team
        {
            public int team;
        }

        // Will update position to match that of a host entity
        public struct _Parasite
        {
            public Guid parentID;
            // RELATIVE = when moved, stay some position relative to host
            // MOVEHOST = when moved, move host to new position
            public enum MoveType { RELATIVE, MOVEHOST };
            // ONDEATH = Drop command run when host dies
            // MANUAL = User can drop item on ground via menus
            [Flags] public enum DropType { ONDEATH = 1, MANUAL = 2 };

            // Action to call when dropped
            public SelfDelegate dropAction;
        }

        // Area from Z to Height cannot be entered if this is present.
        public struct _Solid
        {
            public bool walkable; // You can step on top of this
            public bool attack; // Attack anyone above if stepped on
            public bool spawn; // Spawns default attack if stepped on
            public int move; // # tiles to move if stepped on
            public bool die; // Destory self if stepped on
            public SelfDelegate custom; // Custom action
        }

        // Special effects to display on object destruction
        // Order: animation -> vfx -> custom -> destroy
        public struct _DeathFX
        {
            public enum Vfx { EXPLODE, SPARKLE, FADEOUT };
            public string animation; // animation to play, if any, on death
            public SelfDelegate custom; // custom action before destruction
            public bool persist; // if true, do not destroy object after vfx
        }

        // Just timers
        public struct _Timers
        {
            public struct _Timer
            {
                public enum Action { DIE, SPAWN, SPREAD, MOVE, CUSTOM };
                public enum Mode { FRAMES, TURNS };
                public SelfDelegate custom;
                public int framesLeft;
                public int turnsLeft;
            }

            public _Timer[] timers;
        }

        // Has ability to attack when stepped on (does not effect moveset!)
        // Requires solid->attack to be set to be used
        public struct _Attacker
        {
            public int defATK; // default attack strength
			// knocks target back X tiles when attacked
			// number of directions away from current direction you can face
			// to automatically attack someone who is attacking you in that
			// direction. Range 0-4.
			public int knockback; 
            public int retaliateWidth;
        }

        // Has ability to take damage
        public struct _Defender
        {
            public int HP;
            public int maxHP;
        }

        // Has ability to make decisions about what to do
        public struct _Intelligent
        {
            public int PP; // limits actions that can be done per turn
            public int maxPP;
            // input source?
        }

        // Can make clones of itself in various directions
        public struct _Spreadable
        {
            public int radiusCircle; // radius to fill a circle to
            public int radiusOddCross; // radius to extend in odd directions
            public int radiusEvenCross; // radius to extend in even directions
            public int steepness; // max allowed tile height variance
		}

        public struct _Spawner
        {
            // [move list]
            public int defaultMove;
            public int ATK; // value of defATK on any Attackers spawned
        }

        // Can gain EXP and alter base stats
        public struct _Upgradable
        {
            public int EXP; // max = LVL * 1000
            public int LVL; // max 99
            // [upgrade list]
        }

        // Has ability to move places
        public struct _Mobile
        {
            public int PPCost; // cost to start moving
            public int PPCostPerTile; // cost to keep moving
            public int steepness; // max allowed tile height variance
            public _Position[] stepArray; // array of places to move to
        }

        // ATK/DEF boosts. Should get consumed immediately after attachement.
        public struct _StatFX
        {
            public int ATKinc; // Spawner->Attack or Attacker->DefATK
            public int DEFinc; // Defender->DEF
        }

        // HP/PP boosts. Should get consumed immediately after attachement.
        public struct _HealFX
        {
            public int HPinc; // Defender->HP
            public int PPinc; // Intelligent->PP
        }

        // Mobility cost debuffs. Should persist. (Remove with timer)
        public struct _MobilityFX
        {
            public int startInc; // Starting cost increase
            public int stepInc; // Cost/tile increase
        }
    }
}