namespace BossMod.Stormblood.Ultimate.UCOB;

class P3QuickmarchTrio : BossComponent
{
    private Actor? _relNorth;
    private WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];

    public bool Active => _relNorth != null;

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.Actor(_relNorth, ArenaColor.Object, true);
        var safespot = _safeSpots[pcSlot];
        if (safespot != default)
            arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
        {
            _relNorth = actor;
            var dirToNorth = Angle.FromDirection(actor.Position - module.Bounds.Center);
            foreach (var p in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(module.Raid))
            {
                bool left = p.group < 4;
                int order = p.group & 3;
                var offset = (60 + order * 20).Degrees();
                var dir = dirToNorth + (left ? offset : -offset);
                _safeSpots[p.slot] = module.Bounds.Center + 20 * dir.ToDirection();
            }
        }
    }
}

class P3TwistingDive : Components.SelfTargetedAOEs
{
    public P3TwistingDive() : base(ActionID.MakeSpell(AID.TwistingDive), new AOEShapeRect(60, 4)) { }
}

class P3LunarDive : Components.SelfTargetedAOEs
{
    public P3LunarDive() : base(ActionID.MakeSpell(AID.LunarDive), new AOEShapeRect(60, 4)) { }
}

class P3MegaflareDive : Components.SelfTargetedAOEs
{
    public P3MegaflareDive() : base(ActionID.MakeSpell(AID.MegaflareDive), new AOEShapeRect(60, 6)) { }
}

class P3Twister : Components.ImmediateTwister
{
    public P3Twister() : base(2, (uint)OID.VoidzoneTwister, 1.4f) { } // TODO: verify radius
}

class P3MegaflareSpreadStack : Components.UniformStackSpread
{
    private BitMask _stackTargets;

    public P3MegaflareSpreadStack() : base(5, 5, 3, 3, alwaysShowSpreads: true) { }

    public override void Init(BossModule module)
    {
        AddSpreads(module.Raid.WithoutSlot(true), module.WorldState.CurrentTime.AddSeconds(2.6f));
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
            _stackTargets.Set(module.Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MegaflareSpread:
                Spreads.Clear();
                var stackTarget = module.Raid.WithSlot().IncludedInMask(_stackTargets).FirstOrDefault().Item2; // random target
                if (stackTarget != null)
                    AddStack(stackTarget, module.WorldState.CurrentTime.AddSeconds(4), ~_stackTargets);
                break;
            case AID.MegaflareStack:
                Stacks.Clear();
                break;
        }
    }
}

class P3MegaflarePuddle : Components.LocationTargetedAOEs
{
    public P3MegaflarePuddle() : base(ActionID.MakeSpell(AID.MegaflarePuddle), 6) { }
}

class P3TempestWing : Components.TankbusterTether
{
    public P3TempestWing() : base(ActionID.MakeSpell(AID.TempestWing), (uint)TetherID.TempestWing, 5) { }
}
