namespace BossMod.Endwalker.Savage.P9SKokytos;

class Charibdys(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.CharybdisAOE), m => m.Enemies(OID.Charybdis).Where(v => v.EventState != 7), 0.6f);

class Comet(BossModule module) : Components.Adds(module, (uint)OID.Comet)
{
    public static bool IsActive(Actor c) => c.ModelState.AnimState2 == 1;
    public static bool IsFinished(Actor c) => c.IsDead || c.CastInfo != null;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in Actors.Where(a => !IsFinished(a)))
        {
            Arena.Actor(c, IsActive(c) ? ArenaColor.Enemy : ArenaColor.Object, true);
        }
    }
}

class CometImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CometImpact), new AOEShapeCircle(10)); // TODO: verify falloff
class CometBurst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CometBurstLong), new AOEShapeCircle(10));

class BeastlyBile(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4)
{
    public int NumCasts { get; private set; }
    private readonly Comet? _comet = module.FindComponent<Comet>();
    private DateTime _activation = module.WorldState.FutureTime(15); // assuming component is activated after proximity
    private BitMask _forbiddenPlayers;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_comet != null && IsStackTarget(actor))
        {
            if (_comet.Actors.Any(c => !Comet.IsActive(c) && c.Position.InCircle(actor.Position, StackRadius)))
                hints.Add("GTFO from normal comets!");
            if (!_comet.Actors.Any(c => Comet.IsActive(c) && c.Position.InCircle(actor.Position, StackRadius)))
                hints.Add("Bait to glowing comet!");
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        var target = NumCasts < 2 ? Raid.WithoutSlot().Farthest(Module.PrimaryActor.Position) : null;
        if (target != null)
            AddStack(target, _activation, _forbiddenPlayers);
        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BeastlyBileAOE)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(6);
            foreach (var t in spell.Targets)
                _forbiddenPlayers.Set(Raid.FindSlot(t.ID));
        }
    }
}

class Thunderbolt(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.ThunderboltAOE))
{
    private readonly Comet? _comet = module.FindComponent<Comet>();

    private static readonly AOEShapeCone _shape = new(40, 22.5f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var p in Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position).Take(4))
            CurrentBaits.Add(new(Module.PrimaryActor, p, _shape));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        foreach (var b in ActiveBaitsOn(actor))
            if (_comet?.Actors.Any(c => IsClippedBy(c, b)) ?? false)
                hints.Add("Aim away from comets!");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Raid.FindSlot(t.ID));
        }
    }
}

class EclipticMeteor(BossModule module) : Components.GenericLineOfSightAOE(module, ActionID.MakeSpell(AID.EclipticMeteorAOE), 60, false)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.EclipticMeteor)
        {
            Modify(actor.Position, Module.Enemies(OID.Comet).Where(c => c != actor && !Comet.IsFinished(c)).Select(c => (c.Position, c.HitboxRadius)));
        }
    }
}
