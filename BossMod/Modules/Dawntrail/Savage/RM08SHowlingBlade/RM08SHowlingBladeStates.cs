namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class WindStonefangCross(BossModule module) : Components.GroupedAOEs(module, [AID.WindfangCards, AID.WindfangIntercards, AID.StonefangCards, AID.StonefangIntercards], new AOEShapeCross(15, 3));
class WindfangDonut(BossModule module) : Components.StandardAOEs(module, AID.WindfangDonut, new AOEShapeDonut(8, 20));
class StonefangCircle(BossModule module) : Components.StandardAOEs(module, AID.StonefangCircle, new AOEShapeCircle(9));

class WindStonefang(BossModule module) : Components.CastCounter(module, default)
{
    private Actor? _source;
    private DateTime _activation;
    private bool _stack;

    private static readonly AOEShapeCone ActiveShape = new(40, 12.5f.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source != null)
        {
            var pcDir = Angle.FromDirection(actor.Position - _source.Position);
            var clipped = Raid.WithoutSlot().Exclude(actor).InShape(ActiveShape, _source.Position, pcDir);
            if (_stack)
            {
                var cond = clipped.CountByCondition(a => a.Class.IsSupport() == actor.Class.IsSupport());
                hints.Add("Stack in pairs!", cond.match != 0 || cond.mismatch != 1);
            }
            else
            {
                hints.Add("Spread!", clipped.Any());
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_source != null)
            hints.PredictedDamage.Add((new(0xff), _activation));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _source == null ? PlayerPriority.Irrelevant : player.Class.IsSupport() == pc.Class.IsSupport() ? PlayerPriority.Normal : PlayerPriority.Interesting;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_source != null)
        {
            var pcDir = Angle.FromDirection(pc.Position - _source.Position);
            ActiveShape.Outline(Arena, _source.Position, pcDir);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StonefangCards or AID.WindfangIntercards or AID.StonefangIntercards or AID.WindfangCards)
        {
            _source = caster;
            _stack = (AID)spell.Action.ID is AID.WindfangIntercards or AID.WindfangCards;
            _activation = Module.CastFinishAt(spell, 0.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.StonefangProtean or AID.WindfangProtean)
        {
            ++NumCasts;
            _source = null;
        }
    }
}

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

    enum Inout { None, In, Out }
    private Inout Next;

    public WPos? Source { get; private set; }
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
                var dir = (Arena.Center - caster.Position).Normalized();
                Activation = WorldState.FutureTime(4.6f);
                Source = caster.Position + dir * 17.75f;
                break;

            case AID.WolvesReignCone:
            case AID.WolvesReignCircle:
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

class ReignsEnd : Components.GenericBaitAway
{
    public ReignsEnd(BossModule module) : base(module, AID.ReignsEnd)
    {
        CurrentBaits.AddRange(Raid.WithoutSlot().Where(r => r.Role == Role.Tank).Select(r => new Bait(Module.PrimaryActor, r, new AOEShapeCone(40, 30.Degrees()), WorldState.FutureTime(3.1f))));
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
            var stacked = Raid.WithoutSlot().Exclude(actor).InShape(Shape, Source.Position, Source.AngleTo(actor));
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
                hints.PredictedDamage.Add((Raid.WithSlot().InShape(Shape, Source.Position, Source.AngleTo(h)).Mask(), Activation));
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

class MillennialDecay(BossModule module) : Components.RaidwideCast(module, AID.MillennialDecay);

class BreathOfDecay(BossModule module) : Components.StandardAOEs(module, AID.BreathOfDecay, new AOEShapeRect(40, 4))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in base.ActiveAOEs(slot, actor))
        {
            yield return aoe with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = i == 0 };
            if (++i > 2)
                break;
        }
    }
}

class Gust(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Gust, AID.Gust, 5, 5.1f);

class AeroIII(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AeroIII, 8);

class ProwlingGale(BossModule module) : Components.CastTowers(module, AID.ProwlingGale, 2, maxSoakers: 1)
{
    private BitMask Tethers;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID._Gen_WolfOfWind && (TetherID)tether.ID is TetherID.WindsOfDecayShort or TetherID.WindsOfDecayLong)
            UpdateMask(Raid.FindSlot(tether.Target));
    }

    private void UpdateMask(int slot = -1)
    {
        Tethers.Set(slot);

        for (var i = 0; i < Towers.Count; i++)
            Towers.Ref(i).ForbiddenSoakers = Tethers;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            UpdateMask();
    }
}

class WindsOfDecay : Components.GenericBaitAway
{
    private DateTime Activation;

    public WindsOfDecay(BossModule module) : base(module, AID.WindsOfDecay)
    {
        EnableHints = false;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID._Gen_WolfOfWind && (TetherID)tether.ID is TetherID.WindsOfDecayShort or TetherID.WindsOfDecayLong && !CurrentBaits.Any(b => b.Target.InstanceID == tether.Target))
        {
            if (Activation == default)
                Activation = WorldState.FutureTime(7.2f);

            CurrentBaits.Add(new(source, WorldState.Actors.Find(tether.Target)!, new AOEShapeCone(40, 15.Degrees()), Activation));
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

class WindsOfDecayTether(BossModule module) : Components.CastCounter(module, AID.WindsOfDecay)
{
    private DateTime Activation;

    private readonly Dictionary<Actor, (ulong Target, bool Stretched)> Tethers = [];

    public bool EnableHints;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID._Gen_WolfOfWind && (TetherID)tether.ID is TetherID.WindsOfDecayShort or TetherID.WindsOfDecayLong)
        {
            if (Activation == default)
                Activation = WorldState.FutureTime(7.2f);

            Tethers[source] = (tether.Target, (TetherID)tether.ID == TetherID.WindsOfDecayLong);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Tethers.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            foreach (var (_, to) in Tethers)
                if (to.Target == actor.InstanceID)
                    hints.Add("Stretch tether!", !to.Stretched);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (from, to) in Tethers)
            if (to.Target == pc.InstanceID)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(from.Position, pc.Position, 0xFF000000, 2);
                Arena.AddLine(from.Position, pc.Position, to.Stretched ? ArenaColor.Safe : ArenaColor.Danger);
            }
    }
}

class TrackingTremorsStack(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.TrackingTremors)
            AddStack(actor, WorldState.FutureTime(5.9f));
    }
}

class TrackingTremors(BossModule module) : Components.CastCounter(module, AID.TrackingTremors);

class GreatDivide(BossModule module) : Components.CastSharedTankbuster(module, AID.GreatDivide, new AOEShapeRect(60, 3));

class TerrestrialTitans(BossModule module) : Components.StandardAOEs(module, AID.TerrestrialTitans, new AOEShapeCircle(5));
class TitanicPursuit(BossModule module) : Components.RaidwideCast(module, AID.TitanicPursuit);

class Towerfall(BossModule module) : Components.StandardAOEs(module, AID.Towerfall, new AOEShapeRect(30, 5));
class FangedCrossing(BossModule module) : Components.StandardAOEs(module, AID.FangedCrossing, new AOEShapeCross(21, 3.5f));

class RM08SHowlingBladeStates : StateMachineBuilder
{
    public RM08SHowlingBladeStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ExtraplanarPursuit(id, 10.2f);
        WindStonefang(id + 0x10000, 8.9f);
        RevolutionaryReign(id + 0x20000, 5.1f);
        ExtraplanarPursuit(id + 0x30000, 2.2f);
        MillennialDecay(id + 0x40000, 8.5f);
        TrackingTremors(id + 0x50000, 0.8f);
        ExtraplanarPursuit(id + 0x60000, 1.8f);
        TerrestrialTitans(id + 0x70000, 3.8f);
        RevolutionaryReign(id + 0x80000, 0.3f);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void ExtraplanarPursuit(uint id, float delay)
    {
        CastStart(id, AID.ExtraplanarPursuitVisual, delay)
            .ActivateOnEnter<ExtraplanarPursuit>();
        ComponentCondition<ExtraplanarPursuit>(id + 1, 4, e => e.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<ExtraplanarPursuit>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WindStonefang(uint id, float delay)
    {
        CastMulti(id, [AID.WindfangIntercards, AID.WindfangCards, AID.StonefangCards, AID.StonefangIntercards], delay, 6, "In/out")
            .ActivateOnEnter<WindStonefangCross>()
            .ActivateOnEnter<WindfangDonut>()
            .ActivateOnEnter<StonefangCircle>()
            .ActivateOnEnter<WindStonefang>();

        ComponentCondition<WindStonefang>(id + 2, 0.1f, w => w.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<WindStonefangCross>()
            .DeactivateOnExit<WindfangDonut>()
            .DeactivateOnExit<StonefangCircle>()
            .DeactivateOnExit<WindStonefang>();
    }

    private void RevolutionaryReign(uint id, float delay)
    {
        CastMulti(id, [AID.EminentReignVisual1, AID.RevolutionaryReignVisual1, AID.EminentReignVisual2, AID.RevolutionaryReignVisual2], delay, 5.1f)
            .ActivateOnEnter<ReignJumpCounter>()
            .ActivateOnEnter<WolvesReign>()
            .ActivateOnEnter<ReignInout>()
            .ActivateOnEnter<WolvesReignRect>();

        ComponentCondition<ReignJumpCounter>(id + 2, 1.9f, e => e.NumCasts > 0, "Boss jump")
            .DeactivateOnExit<ReignJumpCounter>()
            .DeactivateOnExit<WolvesReign>();
        ComponentCondition<WolvesReignRect>(id + 3, 2.5f, w => w.NumCasts > 0, "Line AOE")
            .DeactivateOnExit<WolvesReignRect>();
        ComponentCondition<ReignInout>(id + 4, 3.1f, r => r.NumCasts > 0, "In/out")
            .ActivateOnEnter<ReignsEnd>()
            .ActivateOnEnter<SovereignScar>()
            .ExecOnEnter<ReignInout>(r => r.Risky = true)
            .DeactivateOnExit<ReignInout>()
            .DeactivateOnExit<SovereignScar>()
            .DeactivateOnExit<ReignsEnd>();
    }

    private void MillennialDecay(uint id, float delay)
    {
        Cast(id, AID.MillennialDecay, delay, 5)
            .ActivateOnEnter<MillennialDecay>()
            .ActivateOnEnter<BreathOfDecay>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<AeroIII>()
            .ActivateOnEnter<ProwlingGale>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<AeroIII>(id + 0x10, 10.7f, e => e.NumCasts > 0, "Knockback");

        ComponentCondition<BreathOfDecay>(id + 0x11, 1.5f, b => b.NumCasts > 0, "Line AOE 1");

        ComponentCondition<Gust>(id + 0x12, 0.4f, g => g.NumFinishedSpreads > 0, "Spreads 1");
        Timeout(id + 0x13, 5.1f, "Spreads 2")
            .DeactivateOnExit<Gust>();
        ComponentCondition<BreathOfDecay>(id + 0x14, 2.5f, b => b.NumCasts > 4, "Line AOE 5")
            .ActivateOnEnter<WindsOfDecay>()
            .ActivateOnEnter<WindsOfDecayTether>()
            .DeactivateOnExit<BreathOfDecay>();

        ComponentCondition<AeroIII>(id + 0x20, 6.2f, a => a.NumCasts > 1, "Knockback")
            .ExecOnExit<WindsOfDecay>(w => w.EnableHints = true)
            .ExecOnExit<WindsOfDecayTether>(w => w.EnableHints = true);

        ComponentCondition<ProwlingGale>(id + 0x22, 2.2f, p => p.NumCasts > 0, "Towers");
        ComponentCondition<WindsOfDecay>(id + 0x23, 0.2f, w => w.NumCasts > 0, "Baits")
            .ActivateOnEnter<TrackingTremors>()
            .ActivateOnEnter<TrackingTremorsStack>();
    }

    private void TrackingTremors(uint id, float delay)
    {
        Cast(id, AID.TrackingTremorsVisual, delay, 5);

        ComponentCondition<TrackingTremors>(id + 2, 0.9f, t => t.NumCasts > 0, "Stack 1");

        ComponentCondition<TrackingTremors>(id + 5, 7.5f, t => t.NumCasts == 8, "Stack 8")
            .DeactivateOnExit<TrackingTremors>()
            .DeactivateOnExit<TrackingTremorsStack>();
    }

    private void TerrestrialTitans(uint id, float delay)
    {
        Cast(id, AID.GreatDivide, delay, 5, "Tankbuster")
            .ActivateOnEnter<GreatDivide>()
            .DeactivateOnExit<GreatDivide>()
            .SetHint(StateMachine.StateHint.Tankbuster);

        Cast(id + 0x10, AID.TerrestrialTitansVisual, 11, 4, "Pillars appear")
            .ActivateOnEnter<TerrestrialTitans>()
            .DeactivateOnExit<TerrestrialTitans>();

        CastStart(id + 0x20, AID.TitanicPursuitVisual, 3.2f)
            .ActivateOnEnter<TitanicPursuit>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<FangedCrossing>();

        ComponentCondition<TitanicPursuit>(id + 0x21, 4, t => t.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<TitanicPursuit>();

        ComponentCondition<FangedCrossing>(id + 0x30, 7.9f, f => f.NumCasts > 0, "Safe spot")
            .DeactivateOnExit<FangedCrossing>()
            .DeactivateOnExit<Towerfall>();
    }
}
