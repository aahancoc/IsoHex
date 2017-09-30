using System;
using Microsoft.Xna.Framework;

namespace IsoHex
{
    public class Entity
    {
        public Guid ID;
        public _Active Active;
        public _Position Position;
        public _Renderable Renderable;
        public _Team Team;
        public _Parasite Parasite;
        public _Solid Solid;
        public _DeathFX DeathFX;
        public _Timers Timers;
        public _Attacker Attacker;
        public _Defender Defender;
        public _Intelligent Intelligent;
        public _Spreadable Spreadable;
        public _Spawner Spawner;
        public _Upgradable Upgradable;
        public _Mobile Mobile;
        public _StatFX StatFX;
        public _HealFX HealFX;
        public _MobilityFX MobilityFX;

        public delegate void SelfDelegate(Entity x);

		[Flags]
		public enum _Active
		{
			NONE = 0,
			POSITION = 1 << 0,
			RENDERABLE = 1 << 1,
			TEAM = 1 << 2,
			PARASITE = 1 << 3,
			SOLID = 1 << 4,
			DEATHFX = 1 << 5,
			TIMERS = 1 << 6,
			ATTACKER = 1 << 7,
			DEFENDER = 1 << 8,
			INTELLIGENT = 1 << 9,
			SPREADABLE = 1 << 10,
			SPAWNER = 1 << 11,
			UPGRADABLE = 1 << 12,
			MOBILE = 1 << 13,
			STATFX = 1 << 14,
			HEALFX = 1 << 15,
			MOBILITYFX = 1 << 16
		}

        public Entity(){
            Active = _Active.NONE;
            ID = new Guid();
        }

        public struct _Position
        {
            public int X; // tiles
            public int Y; // tiles
            public int Z; // tiles
            public int height; // tiles
            public _Direction dir; // 0-5, 0 = back-right 6 = front-right

            [Flags] public enum _Direction {
                INVALID = 0,
                DOWNRIGHT = 1 << 0,
                RIGHT = 1 << 1,
                UPRIGHT = 1 << 2,
                UPLEFT = 1 << 3,
                LEFT = 1 << 4,
                DOWNLEFT = 1 << 5
            }
        }

        public struct _Renderable
        {
            public string Name;
            public Vector3 Pos; // tiles
            public float height; // tiles
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
            public _MoveType MoveType;
            public _DropType DropType;

            public enum _MoveType { 
                RELATIVE, // when moved, stay some position relative to host
				MOVEHOST  // when moved, move host to new position
			};

            [Flags] public enum _DropType { 
                NONE = 0,
                ONDEATH = 1 << 1, // Run DropAction when host dies
				MANUAL = 1 << 2   // User can drop item on ground via menus
			};

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
            public bool require; // require death to let user make next move
        }

        // Just timers
        public struct _Timers
        {
            public struct _Timer
            {
                public _Action Action;
                public _Mode Mode;
                public SelfDelegate custom;
                public float secondsLeft;
                public int turnsLeft;

				public enum _Action { DIE, SPAWN, SPREAD, MOVE, CUSTOM };
				public enum _Mode { SECONDS, TURNS };
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