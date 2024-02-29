namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P3BlackfireTrio : BossComponent
    {
        private Actor? _nael;

        public bool Active => _nael != null;

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actor(_nael, ArenaColor.Object, true);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.NaelDeusDarnus && id == 0x1E43)
            {
                _nael = actor;
            }
        }
    }

    class P3ThermionicBeam : Components.UniformStackSpread
    {
        public P3ThermionicBeam() : base(4, 0, 8) { }

        public override void Init(BossModule module)
        {
            var target = module.Raid.Player(); // note: target is random
            if (target != null)
                AddStack(target, module.WorldState.CurrentTime.AddSeconds(5.3f)); // assume it is activated right when downtime starts
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.ThermionicBeam)
                Stacks.Clear();
        }
    }

    class P3MegaflareTower : Components.CastTowers
    {
        public P3MegaflareTower() : base(ActionID.MakeSpell(AID.MegaflareTower), 3) { }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.MegaflareStack)
            {
                var slot = module.Raid.FindSlot(actor.InstanceID);
                foreach (ref var t in Towers.AsSpan())
                    t.ForbiddenSoakers.Set(slot);
                // TODO: consider making per-tower assignments
            }
        }
    }

    class P3MegaflareStack : Components.UniformStackSpread
    {
        public P3MegaflareStack() : base(5, 0, 4, 4) { } // TODO: verify stack radius

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.MegaflareStack)
            {
                if (Stacks.Count == 0)
                    AddStack(actor, module.WorldState.CurrentTime.AddSeconds(5), new(0xff));
                Stacks.Ref(0).ForbiddenPlayers.Clear(module.Raid.FindSlot(actor.InstanceID));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.MegaflareStack)
                Stacks.Clear();
        }
    }
}
