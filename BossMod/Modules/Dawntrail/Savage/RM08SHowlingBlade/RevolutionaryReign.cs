namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class WolvesReign(BossModule module) : Components.GroupedAOEs(module, [AID.WolvesReignClone2, AID.WolvesReignClone1, AID.WolvesReignClone3, AID.EminentReignJump, AID.RevolutionaryReignJump], new AOEShapeCircle(6));

class ReignJumpCounter(BossModule module) : Components.CastCounterMulti(module, [AID.EminentReignJump, AID.RevolutionaryReignJump])
{
    private WPos? _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (WatchedActions.Contains(spell.Action))
            _predicted = caster.Position;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
        {
            NumCasts++;
            _predicted = null;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_predicted is { } p)
            Arena.ActorOutsideBounds(p, Angle.FromDirection(p - Arena.Center), ArenaColor.Enemy);
    }
}

class ReignHints(BossModule module) : BossComponent(module)
{
    private readonly RM08SHowlingBladeConfig _config = Service.Config.Get<RM08SHowlingBladeConfig>();
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();

    private WPos? _source;
    private bool _in;
    private bool _jumped;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EminentReignJump or AID.RevolutionaryReignJump)
        {
            var dir = (Arena.Center - caster.Position).Normalized();
            var dist = caster.Position.X < 92 ? 17.25f : 17.75f;
            _source = caster.Position + dir * dist;
            _in = spell.Action.ID == (uint)AID.EminentReignJump;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WolvesReignRect1 or AID.WolvesReignRect2)
            _jumped = true;

        if ((AID)spell.Action.ID is AID.WolvesReignCone or AID.WolvesReignCircle)
        {
            _source = null;
            _in = false;
            _jumped = false;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var spot in EnumerateSafeSpots(pcSlot, pc))
            Arena.AddCircle(spot, 0.5f, _jumped ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var spot in EnumerateSafeSpots(slot, actor))
            movementHints.Add(actor.Position, spot, _jumped ? ArenaColor.Safe : ArenaColor.Danger);
    }

    private IEnumerable<WPos> EnumerateSafeSpots(int slot, Actor actor)
    {
        if (_source == null || _config.ReignHints == RM08SHowlingBladeConfig.ReignStrategy.Disabled)
            yield break;

        var assignment = _prc[WorldState.Party.Members[slot].ContentId];
        int lp;
        if (_config.ReignHints == RM08SHowlingBladeConfig.ReignStrategy.Any)
            lp = 0;
        else
            lp = assignment switch
            {
                PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.H1 => 1,
                PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H2 => 2,
                _ => 0
            };

        if (_config.ReignHints == RM08SHowlingBladeConfig.ReignStrategy.Inverse)
            lp = lp == 1 ? 2 : 1;

        var isTank = actor.Class.GetRole() == Role.Tank || assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT;

        var bossFacing = (Arena.Center - _source.Value).Normalized();

        if (_in)
        {
            if (isTank)
            {
                if (lp != 2)
                    yield return _source.Value + bossFacing.Rotate(-145.Degrees()) * 4;
                if (lp != 1)
                    yield return _source.Value + bossFacing.Rotate(145.Degrees()) * 4;
            }
            else
            {
                if (lp != 2)
                    yield return _source.Value + bossFacing.Rotate(-75.Degrees()) * 7;
                if (lp != 1)
                    yield return _source.Value + bossFacing.Rotate(75.Degrees()) * 7;
            }
        }
        else
        {
            if (isTank)
            {
                if (lp != 2)
                    yield return _source.Value + bossFacing.Rotate(-53.Degrees()) * 14.4f;
                if (lp != 1)
                    yield return _source.Value + bossFacing.Rotate(53.Degrees()) * 14.4f;
            }
            else
            {
                if (lp != 2)
                    yield return _source.Value + bossFacing.Rotate(-12.Degrees()) * 18;
                if (lp != 1)
                    yield return _source.Value + bossFacing.Rotate(12.Degrees()) * 18;
            }
        }
    }
}

class WolvesReignRect(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? Rect;

    public static readonly AOEShapeRect Shape = new(28, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(Rect);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WolvesReignRect1 or AID.WolvesReignRect2)
            Rect = new(Shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell), ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WolvesReignRect1 or AID.WolvesReignRect2)
        {
            NumCasts++;
            Rect = null;
        }

        if ((AID)spell.Action.ID == AID.EminentReignJump)
            Rect = new(Shape, caster.Position, Angle.FromDirection(Arena.Center - caster.Position), Color: ArenaColor.Danger);
    }
}

class ReignInout(BossModule module) : Components.GenericAOEs(module)
{
    public bool Risky;

    public enum Inout { None, In, Out }
    public Inout Next { get; private set; }

    public WPos? Source { get; private set; }
    private WPos _prevSource;
    private DateTime Activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EminentReignVisual1:
            case AID.EminentReignVisual2:
                Next = Inout.In;
                break;
            case AID.RevolutionaryReignVisual1:
            case AID.RevolutionaryReignVisual2:
                Next = Inout.Out;
                break;

            case AID.WolvesReignRect1:
            case AID.WolvesReignRect2:
                _prevSource = caster.Position;
                var dir = (Arena.Center - caster.Position).Normalized();
                Activation = WorldState.FutureTime(4.6f);
                var dist = caster.Position.X < 92 ? 17.25f : 17.75f;
                Source = caster.Position + dir * dist;
                break;

            case AID.WolvesReignCone:
            case AID.WolvesReignCircle:
                if (Source?.AlmostEqual(caster.Position, 0.05f) == false)
                {
                    var actual = caster.Position - _prevSource;
                    ReportError($"predicted in/out source too far off - expected {Source}, got {caster.Position}, angle {Angle.FromDirection(actual)} dist {actual.Length()} from {_prevSource}");
                }
                Source = caster.Position;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WolvesReignCone or AID.WolvesReignCircle)
        {
            NumCasts++;
            Source = null;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        switch (Next)
        {
            case Inout.In:
                hints.Add("Next: in");
                break;
            case Inout.Out:
                hints.Add("Next: out");
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Source != null)
            yield return new AOEInstance(Next == Inout.In ? new AOEShapeCone(40, 60.Degrees()) : new AOEShapeCircle(14), Source.Value, Angle.FromDirection(Arena.Center - Source.Value), Activation, Risky ? ArenaColor.Danger : ArenaColor.AOE, Risky);
    }
}

class ReignsEnd(BossModule module) : Components.GenericBaitAway(module, AID.ReignsEnd, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void Update()
    {
        CurrentBaits.Clear();
        CurrentBaits.AddRange(RaidByEnmity(Module.PrimaryActor).Take(2).Select(r => new Bait(Module.PrimaryActor, r, new AOEShapeCone(40, 30.Degrees()))));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role == Role.Tank)
        {
            var shouldVoke = false;
            foreach (var b in CurrentBaits)
            {
                if (b.Target == actor)
                    // we are baiting, all good
                    return;
                else if (b.Target.Role != Role.Tank)
                    shouldVoke = true;
            }
            if (shouldVoke)
                hints.Add("Provoke!");
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class SovereignScar : Components.CastCounter
{
    private Actor? Source;
    private readonly DateTime Activation;
    private static readonly AOEShape Shape = new AOEShapeCone(40, 15.Degrees());

    public SovereignScar(BossModule module) : base(module, AID.SovereignScar)
    {
        Source = module.PrimaryActor;
        Activation = WorldState.FutureTime(3.1f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Source == null || actor.Role == Role.Tank)
            return;

        if (actor.Role == Role.Healer)
        {
            var stacked = Raid.WithoutSlot().Exclude(actor).InShape(Shape, Source.Position, Source.AngleTo(actor)).ToList();
            if (stacked.Any(p => p.Role == Role.Healer))
                hints.Add("GTFO from other healer!");

            hints.Add("Stack with party!", !stacked.Any(p => p.Role != Role.Healer));
        }
        else
        {
            var healers = Raid.WithoutSlot().Where(x => x.Role == Role.Healer);
            hints.Add("Stack with healer!", !healers.Any(h => Shape.Check(actor.Position, Source.Position, Source.AngleTo(h))));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source != null)
        {
            foreach (var h in Raid.WithoutSlot().Where(x => x.Role == Role.Healer))
                hints.AddPredictedDamage(Raid.WithSlot().InShape(Shape, Source.Position, Source.AngleTo(h)).Mask(), Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Source = null;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source == null)
            return;

        foreach (var h in Raid.WithoutSlot().Where(p => p.Role == Role.Healer))
        {
            if (h == pc)
                Shape.Outline(Arena, Source.Position, Source.AngleTo(h), ArenaColor.Safe);
            else if (pc.Class.IsSupport())
                Shape.Draw(Arena, Source.Position, Source.AngleTo(h), ArenaColor.AOE);
            else
                Shape.Outline(Arena, Source.Position, Source.AngleTo(h), ArenaColor.Safe);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Source == null ? PlayerPriority.Irrelevant : player.Class.IsSupport() == pc.Class.IsSupport() ? PlayerPriority.Interesting : PlayerPriority.Normal;
}
