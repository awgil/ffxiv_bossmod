namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class FireballAdds(BossModule module) : Components.AddsMulti(module, [OID.GaseousNereid, OID.GaseousPhobos], 2);
class FireballTowerHint(BossModule module) : BossComponent(module)
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();

    private Actor? _fireballNE;
    private Actor? _fireballSW;

    public int FireballCount => (_fireballNE == null ? 0 : 1) + (_fireballSW == null ? 0 : 1);

    public Actor? PlayerFireball => _config.PlayerAlliance.Group2() switch
    {
        1 => _fireballNE,
        2 => _fireballSW,
        _ => null
    };

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EBDAD)
        {
            var arrowsStart = actor.Position + actor.Rotation.ToDirection() * -20; // estimate
            if (arrowsStart.InCone(Arena.Center, 112.5f.Degrees(), 90.Degrees()))
                AssignFireball(actor, ref _fireballNE);
            else
                AssignFireball(actor, ref _fireballSW);
        }
    }

    private void AssignFireball(Actor a, ref Actor? b)
    {
        if (b != null)
            ReportError($"fireball is already assigned to {b}");
        b = a;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_config.PlayerAlliance.Group2() != 2 && _fireballNE is { } ne)
            Arena.AddCircle(ne.Position, 1, ArenaColor.Safe);
        if (_config.PlayerAlliance.Group2() != 1 && _fireballSW is { } sw)
            Arena.AddCircle(sw.Position, 1, ArenaColor.Safe);
    }
}

class FireSpread(BossModule module) : Components.CastCounter(module, AID.FireSpread)
{
    private readonly FireballTowerHint _th = module.FindComponent<FireballTowerHint>()!;

    private readonly List<Actor> _towers = [];
    private readonly Actor?[][] _targets = [[null, null], [null, null]];

    public DateTime NextActivation { get; private set; } = module.WorldState.FutureTime(3);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.GaseousNereid or OID.GaseousPhobos)
            _towers.Add(actor);
    }

    record struct Target(Actor T, float Lsq);

    public override void Update()
    {
        if (_towers.Count != 2)
            return;

        Target? s1 = null, s2 = null, d1 = null, d2 = null;

        static void upd(ref Target? t, Actor src, Actor tar)
        {
            if (!src.IsTargetable)
                return;

            var prev = t?.Lsq ?? float.MaxValue;
            var dist = (tar.Position - src.Position).LengthSq();
            if (dist < prev)
                t = new(tar, dist);
        }

        foreach (var actor in WorldState.Actors.Where(a => a.Type == ActorType.Player && !a.IsDead))
        {
            if (actor.Class.IsDD())
            {
                upd(ref d1, _towers[0], actor);
                upd(ref d2, _towers[1], actor);
            }
            else if (actor.ClassCategory == ClassCategory.Healer)
            {
                upd(ref s1, _towers[0], actor);
                upd(ref s2, _towers[1], actor);
            }
        }

        _targets[0][0] = s1?.T;
        _targets[0][1] = d1?.T;
        _targets[1][0] = s2?.T;
        _targets[1][1] = d2?.T;
    }

    private IEnumerable<(Actor Source, Actor Target, bool Allowed)> Baits(Actor player)
    {
        for (var i = 0; i < _towers.Count; i++)
        {
            var src = _towers[i];
            if (_targets[i][0] is { } tar)
            {
                var allowed = tar == player
                    || tar.Class.IsSupport() == player.Class.IsSupport()
                        && (_th.PlayerFireball == null || _th.PlayerFireball.Position.AlmostEqual(src.Position, 1));
                yield return (src, tar, allowed);
            }
            if (_targets[i][1] is { } tar2)
            {
                var allowed = tar2 == player
                    || tar2.Class.IsSupport() == player.Class.IsSupport()
                        && (_th.PlayerFireball == null || _th.PlayerFireball.Position.AlmostEqual(src.Position, 1));
                yield return (src, tar2, allowed);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (src, tar, allowed) in Baits(pc))
        {
            if (tar == pc)
                Arena.AddCone(src.Position, 60, src.AngleTo(tar), 45.Degrees(), ArenaColor.Danger);
            else
                Arena.ZoneCone(src.Position, 0, 60, src.AngleTo(tar), 45.Degrees(), allowed ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = Baits(actor);
        bool stackHint = false, forbidden = false;

        foreach (var b in baits)
        {
            stackHint |= b.Target == actor;
            forbidden |= !b.Allowed && actor.Position.InCone(b.Source.Position, b.Source.AngleTo(b.Target), 45.Degrees());
        }

        if (stackHint)
            hints.Add("Stack with party!", false);

        if (forbidden)
            hints.Add("GTFO from forbidden bait!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in Baits(actor))
            if (!b.Allowed)
                hints.AddForbiddenZone(new AOEShapeCone(60, 45.Degrees()), b.Source.Position, b.Source.AngleTo(b.Target), NextActivation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts % 4 == 0)
                NextActivation = WorldState.FutureTime(4.2f);
        }
    }
}

class ElementalImpact(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ElementalImpact1 or AID.ElementalImpact2 or AID.ElementalImpact3)
            Towers.Add(new(spell.LocXZ, 5, forbiddenSoakers: Raid.WithSlot().Where(i => i.Item2.ClassCategory != ClassCategory.Tank).Mask(), activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ElementalImpact1 or AID.ElementalImpact2 or AID.ElementalImpact3)
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
    }

    public override void OnUntargetable(Actor actor)
    {
        if ((OID)actor.OID is OID.GaseousNereid or OID.GaseousPhobos)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
    }
}

class GeothermalRupture(BossModule module) : Components.StandardAOEs(module, AID.GeothermalRupture, 8);

class FlameThrower(BossModule module) : Components.MultiLineStack(module, 4, 40, AID.FlameThrowerTarget, AID.FlameThrower, 5)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Stacks.Clear();
    }
}

class FireDestruct(BossModule module) : Components.CastHint(module, AID.SelfDestructFire, "Kill fireballs!", true);
