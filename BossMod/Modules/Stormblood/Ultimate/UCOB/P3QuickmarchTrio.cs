namespace BossMod.Stormblood.Ultimate.UCOB;

class P3QuickmarchTrio(BossModule module) : BossComponent(module)
{
    private Actor? _relNorth;
    private readonly WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];
    private readonly UCOBConfig _config = Service.Config.Get<UCOBConfig>();

    public bool Active => _relNorth != null;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_relNorth, Colors.Object, true);
        var safespot = _safeSpots[pcSlot];
        if (safespot != default)
            Arena.ZoneCircleOutline(safespot, 1, Colors.Safe);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.BahamutPrime && id == 0x1E43)
        {
            _relNorth = actor;
            var dirToNorth = Angle.FromDirection(actor.Position - Arena.Center);
            foreach (var p in _config.P3QuickmarchTrioAssignments.Resolve(Raid))
            {
                var left = p.group < 4;
                var order = p.group & 3;
                var offset = (60 + order * 20).Degrees();
                var dir = dirToNorth + (left ? offset : -offset);
                _safeSpots[p.slot] = Arena.Center + 20 * dir.ToDirection();
            }
        }
    }
}

class P3TwistingDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwistingDive, new AOEShapeRect(63.96f, 4f));
class P3LunarDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LunarDive, new AOEShapeRect(62.55f, 4f));
class P3MegaflareDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegaflareDive, new AOEShapeRect(64.2f, 6f));
class P3Twister(BossModule module) : Components.ImmediateTwister(module, 2, (uint)OID.VoidzoneTwister, 1.4f); // TODO: verify radius

class P3MegaflareSpreadStack : Components.UniformStackSpread
{
    private BitMask _stackTargets;

    public P3MegaflareSpreadStack(BossModule module) : base(module, 5f, 5f, 3, 3)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(2.6d));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
            _stackTargets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MegaflareSpread:
                Spreads.Clear();
                var stackTarget = Raid.WithSlot(false, true, true).IncludedInMask(_stackTargets).FirstOrDefault().Item2; // random target
                if (stackTarget != null)
                    AddStack(stackTarget, WorldState.FutureTime(4), ~_stackTargets);
                break;
            case (uint)AID.MegaflareStack:
                Stacks.Clear();
                break;
        }
    }
}

class P3MegaflarePuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegaflarePuddle, 6);
class P3TempestWing(BossModule module) : Components.TankbusterTether(module, (uint)AID.TempestWing, (uint)TetherID.TempestWing, 5);
