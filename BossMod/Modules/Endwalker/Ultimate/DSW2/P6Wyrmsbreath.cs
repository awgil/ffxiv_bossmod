namespace BossMod.Endwalker.Ultimate.DSW2;

// baited cones part of the mechanic
class P6Wyrmsbreath(BossModule module, bool allowIntersect) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.FlameBreath)) // note: cast is arbitrary
{
    public Actor?[] Dragons = [null, null]; // nidhogg & hraesvelgr
    public BitMask Glows;
    private readonly bool _allowIntersect = allowIntersect;
    private readonly Actor?[] _tetheredTo = new Actor?[PartyState.MaxPartySize];
    private BitMask _tooClose;

    private static readonly AOEShapeCone _shape = new(100, 10.Degrees()); // TODO: verify angle

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var b = ActiveBaitsOn(actor).FirstOrDefault();
        if (b.Source == null)
        {
            if (ActiveBaits.Any(b => IsClippedBy(actor, b)))
                hints.Add("GTFO from baits!");
        }
        else
        {
            if (_tooClose[slot])
                hints.Add("Stretch the tether!");

            Actor? partner = IgnoredPartner(slot, actor);
            if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(b).Any(p => p != partner)))
                hints.Add("Bait away from raid!");
            if (ActiveBaitsNotOn(actor).Any(b => b.Target != partner && IsClippedBy(actor, b)))
                hints.Add("GTFO from baited aoe!");
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Glows.Any())
            hints.Add(Glows.Raw == 3 ? "Tankbuster: shared" : "Tankbuster: solo");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Actor? partner = IgnoredPartner(pcSlot, pc);
        foreach (var bait in ActiveBaitsNotOn(pc).Where(b => b.Target != partner))
            bait.Shape.Draw(Arena, BaitOrigin(bait), bait.Rotation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaitsOn(pc))
            bait.Shape.Outline(Arena, BaitOrigin(bait), bait.Rotation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DreadWyrmsbreathNormal:
                Dragons[0] = caster;
                break;
            case AID.DreadWyrmsbreathGlow:
                Dragons[0] = caster;
                Glows.Set(0);
                break;
            case AID.GreatWyrmsbreathNormal:
                Dragons[1] = caster;
                break;
            case AID.GreatWyrmsbreathGlow:
                Dragons[1] = caster;
                Glows.Set(1);
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.FlameBreath or TetherID.IceBreath or TetherID.FlameIceBreathNear)
        {
            var slot = Raid.FindSlot(source.InstanceID);
            var boss = WorldState.Actors.Find(tether.Target);
            if (slot >= 0 && boss != null)
            {
                if (_tetheredTo[slot] == null)
                    CurrentBaits.Add(new(boss, source, _shape));
                _tooClose[slot] = (TetherID)tether.ID == TetherID.FlameIceBreathNear;
                _tetheredTo[slot] = boss;
            }
        }
    }

    private Actor? IgnoredPartner(int slot, Actor actor) => _allowIntersect && _tetheredTo[slot] != null ? Raid.WithSlot().WhereSlot(i => _tetheredTo[i] != null && _tetheredTo[i] != _tetheredTo[slot]).Closest(actor.Position).Item2 : null;
}
class P6Wyrmsbreath1(BossModule module) : P6Wyrmsbreath(module, true);
class P6Wyrmsbreath2(BossModule module) : P6Wyrmsbreath(module, false);

// note: it is actually symmetrical (both tanks get tankbusters), but that is hard to express, so we select one to show arbitrarily (nidhogg)
class P6WyrmsbreathTankbusterShared(BossModule module) : Components.GenericSharedTankbuster(module, ActionID.MakeSpell(AID.DarkOrb), 6)
{
    private readonly P6Wyrmsbreath? _main = module.FindComponent<P6Wyrmsbreath>();

    public override void Update()
    {
        Source = Target = null;
        if (_main?.Glows.Raw == 3)
        {
            Source = _main.Dragons[0];
            Target = WorldState.Actors.Find(Source?.TargetID ?? 0);
            Activation = Source?.CastInfo?.NPCFinishAt ?? WorldState.CurrentTime;
        }
    }
}

class P6WyrmsbreathTankbusterSolo(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly P6Wyrmsbreath? _main = module.FindComponent<P6Wyrmsbreath>();

    private static readonly AOEShapeCircle _shape = new(15);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_main?.Glows.Raw is 1 or 2)
        {
            var source = _main.Dragons[_main.Glows.Raw == 1 ? 1 : 0];
            var target = WorldState.Actors.Find(source?.TargetID ?? 0);
            if (source != null && target != null)
                CurrentBaits.Add(new(source, target, _shape));
        }
    }
}

class P6WyrmsbreathCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly P6Wyrmsbreath? _main = module.FindComponent<P6Wyrmsbreath>();

    private static readonly AOEShapeCone _shape = new(50, 15.Degrees()); // TODO: verify angle

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_main?.Glows.Raw is 1 or 2)
        {
            var source = _main.Dragons[_main.Glows.Raw == 1 ? 0 : 1];
            if (source != null)
                yield return new(_shape, source.Position, source.Rotation); // TODO: activation
        }
    }
}
