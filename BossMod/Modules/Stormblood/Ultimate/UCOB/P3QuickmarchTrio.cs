namespace BossMod.Stormblood.Ultimate.UCOB;

class P3QuickmarchTrio(BossModule module) : BossComponent(module)
{
    private Actor? _relNorth;
    private readonly WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];

    public bool Active => _relNorth != null;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_relNorth, ArenaColor.Object, true);
        var safespot = _safeSpots[pcSlot];
        if (safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
        {
            _relNorth = actor;
            var dirToNorth = Angle.FromDirection(actor.Position - Module.Center);
            foreach (var p in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
            {
                bool left = p.group < 4;
                int order = p.group & 3;
                var offset = (60 + order * 20).Degrees();
                var dir = dirToNorth + (left ? offset : -offset);
                _safeSpots[p.slot] = Module.Center + 20 * dir.ToDirection();
            }
        }
    }
}

class P3TwistingDive(BossModule module) : Components.StandardAOEs(module, AID.TwistingDive, new AOEShapeRect(60, 4));
class P3LunarDive(BossModule module) : Components.StandardAOEs(module, AID.LunarDive, new AOEShapeRect(60, 4));
class P3MegaflareDive(BossModule module) : Components.StandardAOEs(module, AID.MegaflareDive, new AOEShapeRect(60, 6));
class P3Twister(BossModule module) : Components.ImmediateTwister(module, 2, (uint)OID.VoidzoneTwister, 1.4f); // TODO: verify radius

class P3MegaflareSpreadStack : Components.UniformStackSpread
{
    private BitMask _stackTargets;

    public P3MegaflareSpreadStack(BossModule module) : base(module, 5, 5, 3, 3, alwaysShowSpreads: true)
    {
        AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(2.6f));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
            _stackTargets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MegaflareSpread:
                Spreads.Clear();
                var stackTarget = Raid.WithSlot().IncludedInMask(_stackTargets).FirstOrDefault().Item2; // random target
                if (stackTarget != null)
                    AddStack(stackTarget, WorldState.FutureTime(4), ~_stackTargets);
                break;
            case AID.MegaflareStack:
                Stacks.Clear();
                break;
        }
    }
}

class P3MegaflarePuddle(BossModule module) : Components.StandardAOEs(module, AID.MegaflarePuddle, 6);
class P3TempestWing(BossModule module) : Components.TankbusterTether(module, AID.TempestWing, (uint)TetherID.TempestWing, 5);
