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

class ExtraplanarPursuit(BossModule module) : Components.RaidwideCastDelay(module, AID.ExtraplanarPursuitVisual, AID.ExtraplanarPursuit, 2.4f);

#if DEBUG
[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1026, NameID = 13843, PlanLevel = 100)]
public class RM08SHowlingBlade(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(12));
#endif
