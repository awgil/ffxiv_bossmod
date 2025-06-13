namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class SnowballAdds(BossModule module) : Components.AddsMulti(module, [OID.FrozenPhobos, OID.FrozenTriton], 2);

class SnowBoulderHelper(BossModule module) : Components.ChargeAOEs(module, AID.SnowBoulderCast, 5);

class SnowBoulder(BossModule module) : Components.CastCounter(module, AID.SnowBoulder)
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();

    // 0 = north, 1 = south
    record struct Charge(WPos Source, WPos Target, DateTime Activation, int Snowball, int Order)
    {
        public readonly Func<WPos, bool> ShapeFn => ShapeContains.Rect(Source, Target, 5);
    }

    private readonly List<Charge> _charges = [];

    private readonly DateTime[] _vulns = new DateTime[PartyState.MaxPartySize];

    private bool IsAssigned(int slot, Charge c)
    {
        if (_vulns[slot] > c.Activation)
            return false;

        var (side, order) = (_config.PlayerAlliance.Group2() - 1, _config.PlayerAlliance.Group3() - 1);

        if (side < 0)
            return true;
        return side == c.Snowball && order == c.Order;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in _charges)
            Arena.ZoneRect(c.Source, c.Target, 5, IsAssigned(pcSlot, c) ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SnowBoulderCast)
        {
            var snowball = _charges.Count < 2
                ? (caster.Position.Z < Arena.Center.Z ? 0 : 1)
                : _charges.First(c => c.Target.AlmostEqual(caster.Position, 1)).Snowball;
            _charges.Add(new(caster.Position, spell.LocXZ, Module.CastFinishAt(spell), snowball, _charges.Count / 2));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        bool forbidden = false, inside = false, allowed = false;

        foreach (var c in _charges)
        {
            var assigned = IsAssigned(slot, c);
            if (assigned)
                allowed = true;

            if (c.ShapeFn(actor.Position))
            {
                inside = true;
                if (!assigned)
                    forbidden = true;
            }
        }

        if (forbidden)
            hints.Add("GTFO from forbidden charge!");
        else if (allowed)
            hints.Add("Stand in charge!", !inside);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_charges.Count(c => IsAssigned(slot, c)) > 1)
        {
            var anyCharge = ShapeContains.Union([.. _charges.Select(c => c.ShapeFn)]);
            hints.AddForbiddenZone(p => !anyCharge(p), _charges[0].Activation);
            return;
        }

        foreach (var c in _charges)
        {
            var shape = c.ShapeFn;
            hints.AddForbiddenZone(p => IsAssigned(slot, c) ? !shape(p) : shape(p), c.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_charges.Count > 0)
                _charges.RemoveAt(0);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.PhysicalVulnerabilityUp && Raid.TryFindSlot(actor, out var slot))
            _vulns[slot] = status.ExpireAt;
    }
}

class ChillingCollision(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ChillingCollisionIndicator, 21)
{
    public int NumRealCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.ChillingCollisionKB)
            NumRealCasts++;
    }
}

class Avalaunch : Components.StackWithCastTargets
{
    public Avalaunch(BossModule module) : base(module, AID.AvalaunchStack, 8)
    {
        EnableHints = false;
    }
}

class SelfDestruct(BossModule module) : Components.CastHint(module, AID.SelfDestructIce, "Kill snowballs!", true);
