namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

static class Portals
{
    private const float _portalLength = 10f;

    // returns null if this is not arrow appear eanim
    public static WPos? DestinationForEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.Portal)
            return null;

        var rotation = state switch
        {
            0x00400080u => -90f, // CW arrows appear
            0x01000200u => 90f, // CCW arrows appear
            _ => default, // other known: 0x04000800 = CW arrows end, 0x10002000 = CCW arrows end, 0x00100020 = arrows disappear, 0x00040008 = disappear
        };
        if (rotation == default)
            return null;

        return actor.Position + _portalLength * (actor.Rotation + rotation.Degrees()).ToDirection();
    }
}

class PortalsAOE(BossModule module, uint aid, uint movedOID, double activationDelay, AOEShape shape) : Components.GenericAOEs(module, aid)
{
    private readonly List<Actor> _movedActors = module.Enemies(movedOID);
    private readonly double _activationDelay = activationDelay;
    private readonly AOEShape _shape = shape;
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var dest = Portals.DestinationForEAnim(actor, state);
        if (dest == null)
            return;

        var movedActor = _movedActors.FirstOrDefault(a => a.Position.AlmostEqual(actor.Position, 1f));
        if (movedActor != null)
            _aoes.Add(new(_shape, dest.Value, movedActor.Rotation, WorldState.FutureTime(_activationDelay)));
    }
}

abstract class PortalsBurn(BossModule module, uint aid, uint oid) : PortalsAOE(module, aid, oid, 11.6d, new AOEShapeCircle(12f));
sealed class NPortalsBurn(BossModule module) : PortalsBurn(module, (uint)AID.NBurn, (uint)OID.NBallOfFire);
sealed class SPortalsBurn(BossModule module) : PortalsBurn(module, (uint)AID.SBurn, (uint)OID.SBallOfFire);

abstract class PortalsMirror(BossModule module, uint aid, uint oid) : PortalsAOE(module, aid, oid, 11.7d, new AOEShapeRect(60f, 5f, 60f));
sealed class NPortalsMirror(BossModule module) : PortalsMirror(module, (uint)AID.NBlazingBenifice, (uint)OID.NArcaneFont);
sealed class SPortalsMirror(BossModule module) : PortalsMirror(module, (uint)AID.SBlazingBenifice, (uint)OID.SArcaneFont);

sealed class PortalsWave(BossModule module) : BossComponent(module)
{
    public bool Done;
    private readonly List<(WPos n, WPos s)> _portals = [];
    private readonly int[] _playerPortals = new int[PartyState.MaxPartySize]; // 0 = unassigned, otherwise 'z direction sign' (-1 if own portal points N, +1 for S)

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var dir = _playerPortals[pcSlot];
        if (dir != 0)
        {
            for (var i = 0; i < _portals.Count; ++i)
            {
                var p = _portals[i];
                Arena.ZoneCircleOutline(dir > 0 ? p.s : p.n, 1, Colors.Safe, 2f);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.PlayerPortal)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                _playerPortals[slot] = status.Extra switch
                {
                    0x1CD or 0x1CE => -1,
                    0x1D2 or 0x1D3 => +1,
                    _ => 0
                };
            }
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.Portal && state == 0x00100020u)
        {
            Done = true;
            return;
        }

        var dest = Portals.DestinationForEAnim(actor, state);
        if (dest == null || !Arena.InBounds(dest.Value))
            return;

        var n = actor.Position;
        var s = dest.Value;
        if (n.Z > s.Z)
            Utils.Swap(ref n, ref s);
        _portals.Add((n, s));
    }
}
