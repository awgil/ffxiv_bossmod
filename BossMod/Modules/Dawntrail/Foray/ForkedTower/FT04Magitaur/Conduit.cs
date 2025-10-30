namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class Conduit(BossModule module) : Components.CastCounterMulti(module, [AID.AuraBurst, AID.HolyEnrage])
{
    private uint _oid;

    public IEnumerable<Actor> ActiveActors => _oid > 0 ? Module.Enemies(_oid) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AuraBurstCast:
                _oid = (uint)OID.AxeEmpowermentConduit;
                break;
            case AID.HolyCast:
                _oid = (uint)OID.LanceEmpowermentConduit;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            _oid = 0;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_oid > 0)
            hints.PrioritizeTargetsByOID(_oid, 5);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_oid > 0)
            Arena.Actors(Module.Enemies(_oid), ArenaColor.Enemy);
    }
}

class ArcaneReaction(BossModule module) : Components.GenericBaitAway(module, AID.ArcaneReaction, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private Actor? _source;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.UniversalEmpowermentConduit)
            _source = actor;
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source is { } src)
        {
            var closest = WorldState.Actors.Where(a => a.Type == ActorType.Player && !a.IsDead).Closest(src.Position);
            if (closest != null)
                CurrentBaits.Add(new(src, closest, new AOEShapeRect(55, 3)));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var playerIsZerker = actor.FindStatus(4359) != null;

        if (ActiveBaitsOn(actor).Any() && !playerIsZerker)
            hints.Add("Avoid baiting!");

        if (playerIsZerker && ActiveBaitsNotOn(actor).Any(b => b.Target.FindStatus(4359) == null))
            hints.Add("Bait purple canister!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_source != null && pc.FindStatus(4359) != null)
            Arena.AddCircle(_source.Position, 1.5f, ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _source = null;
        }
    }
}

class ArcaneRecoil(BossModule module) : Components.CastCounter(module, AID.ArcaneRecoil)
{
    public record struct Bait(Actor Source, Actor Target);

    public readonly List<Bait> Baits = [];

    public override void Update()
    {
        Baits.Clear();
        foreach (var actor in Module.Enemies(OID.LanceEmpowermentConduit))
            AddBaits(actor);
        foreach (var actor in Module.Enemies(OID.AxeEmpowermentConduit))
            AddBaits(actor);
    }

    private void AddBaits(Actor source)
    {
        if (source.HPRatio > 0.5f || source.IsDead || source.HPMP.CurHP == 1)
            return;

        var baits = WorldState.Actors.Where(p => p.Type == ActorType.Player && !p.IsDead).SortedByRange(source.Position).Take(2);
        Baits.AddRange(baits.Select(b => new Bait(source, b)));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Baits.Any(b => b.Target == player) ? PlayerPriority.Danger : PlayerPriority.Normal;
}
