using System.Linq;

namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    class ManifoldFlames : Components.UniformStackSpread
    {
        public ManifoldFlames() : base(0, 6) { }

        public override void Init(BossModule module)
        {
            AddSpreads(module.Raid.WithoutSlot(true));
        }
    }

    class NestOfFlamevipersCommon : Components.CastCounter
    {
        protected BitMask BaitingPlayers;

        private static AOEShapeRect _shape = new(60, 2.5f);

        public NestOfFlamevipersCommon() : base(ActionID.MakeSpell(AID.NestOfFlamevipersAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (module.Raid.WithSlot().IncludedInMask(BaitingPlayers).WhereActor(p => p != actor && _shape.Check(actor.Position, module.PrimaryActor.Position, Angle.FromDirection(p.Position - module.PrimaryActor.Position))).Any())
                hints.Add("GTFO from baited aoe!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (_, player) in module.Raid.WithSlot().IncludedInMask(BaitingPlayers))
                _shape.Outline(arena, module.PrimaryActor.Position, Angle.FromDirection(player.Position - module.PrimaryActor.Position));
        }
    }

    // variant that happens right after manifold flames and baits to 4 closest players
    class NestOfFlamevipersBaited : NestOfFlamevipersCommon
    {
        private BitMask _forbiddenPlayers;
        public bool Active => NumCasts == 0 && _forbiddenPlayers.Any();

        public override void Update(BossModule module)
        {
            BaitingPlayers = Active ? module.Raid.WithSlot().SortedByRange(module.PrimaryActor.Position).Take(4).Mask() : new();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (Active)
            {
                bool shouldBait = !_forbiddenPlayers[slot];
                hints.Add(shouldBait ? "Move closer to bait" : "GTFO to avoid baits", shouldBait != BaitingPlayers[slot]);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.HemitheosFlare)
                _forbiddenPlayers.Set(module.Raid.FindSlot(spell.MainTargetID));
        }
    }

    // variant that happens when cast is started and baits to everyone
    class NestOfFlamevipersEveryone : NestOfFlamevipersCommon
    {
        public bool Active => NumCasts == 0 && BaitingPlayers.Any();

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.NestOfFlamevipers)
                BaitingPlayers = module.Raid.WithSlot().Mask();
        }
    }
}
