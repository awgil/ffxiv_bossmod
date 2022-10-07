using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows arbitrary shape for each existing object, assumed to be persistent voidzone center
    public class PersistentVoidzone : BossComponent
    {
        public uint SourceOID { get; private init; }
        public AOEShape Shape { get; private init; }
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;

        public PersistentVoidzone(uint sourceOID, AOEShape shape)
        {
            SourceOID = sourceOID;
            Shape = shape;
        }

        public override void Init(BossModule module)
        {
            _casters = module.Enemies(SourceOID);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Casters.Any(c => Shape.Check(actor.Position, c)))
                hints.Add("GTFO from voidzone!");
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, AIHints hints)
        {
            foreach (var c in Casters)
                hints.AddForbiddenZone(Shape, c.Position, c.Rotation);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in Casters)
                Shape.Draw(arena, c);
        }
    }
}
