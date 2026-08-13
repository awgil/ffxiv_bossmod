using System.ComponentModel;

namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// ---------
// Constants
// ---------
static class InsaneAirData
{

    public static readonly AOEShapeCone SnapCone = new(60f, 20f.Degrees()); // total 40 degrees
}

// -----------------------
// Baited conal telegraphs
// -----------------------

// Dynamic tracking cones during Insane Air: origin = closest surfboard marker, aim = marked player (updates every frame).

sealed class InsaneAirSnaps(BossModule module) : Components.GenericAOEs(module)
{
    private struct Pair(ulong o, ulong t, uint icon)
    {
        public ulong OriginID = o;
        public ulong TargetID = t;
        public uint Icon = icon; // 651 (blue) or 665 (red)
    }

    private readonly List<Pair> _pairs = [];
    private readonly List<AOEInstance> _active = [];

    private const uint IconBlue = 651;
    private const uint IconRed = 665;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _active.Clear();

        foreach (var p in _pairs)
        {
            var origin = WorldState.Actors.Find(p.OriginID);
            var target = WorldState.Actors.Find(p.TargetID);
            if (origin == null || target == null)
                continue;

            var rot = Angle.FromDirection(target.Position - origin.Position);
            var color = (p.Icon == IconRed) ? Colors.AOE : Colors.AOE;

            _active.Add(new(InsaneAirData.SnapCone, origin.Position, rot, WorldState.CurrentTime, color, true, origin.InstanceID));
        }

        return CollectionsMarshal.AsSpan(_active);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is not IconBlue and not IconRed)
            return;

        if (actor.OID != (uint)OID._Gen_)
            return;

        var target = WorldState.Actors.Find(targetID);
        if (target == null)
            return;

        // Optional (but good): enforce the expected snake status mapping
        if (iconID == IconBlue && target.FindStatus((uint)SID.Watersnaking) == null)
            return;
        if (iconID == IconRed && target.FindStatus((uint)SID.Firesnaking) == null)
            return;

        _pairs.Add(new(actor.InstanceID, targetID, iconID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // When the snaps resolve, clear that color’s current pairs
        if (spell.Action.ID == (uint)AID.PlungingSnap1)      // water cone resolves
            _pairs.RemoveAll(p => p.Icon == IconBlue);
        else if (spell.Action.ID == (uint)AID.BlastingSnap1) // fire cone resolves
            _pairs.RemoveAll(p => p.Icon == IconRed);
        else if (spell.Action.ID is (uint)AID.DiversDare or (uint)AID.DiversDare1)
            _pairs.Clear();
    }
}

// -----------------------------
// Persistent fire cone "puddles"
// -----------------------------
sealed class BlastingSnapPersistent(BossModule module) : Components.GenericAOEs(module, (uint)AID.BlastingSnap1)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            // Fire snap resolves -> leave a persistent cone at the helper's pos/rot
            case (uint)AID.BlastingSnap1:
                _aoes.Add(new(InsaneAirData.SnapCone, caster.Position, caster.Rotation, WorldState.CurrentTime, Colors.AOE, true, caster.InstanceID));
                break;

            // Cleanup at end of mechanic
            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _aoes.Clear();
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var id = actor.InstanceID;
        _aoes.RemoveAll(a => a.ActorID == id);
    }
}
