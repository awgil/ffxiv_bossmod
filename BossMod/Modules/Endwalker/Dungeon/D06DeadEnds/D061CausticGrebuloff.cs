namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D061CausticGrebuloff;

public enum OID : uint
{
    Boss = 0x34C4, // R=6.65
    Helper = 0x233C,
    WeepingMiasma = 0x34C5 // R=1.0
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    BefoulmentVisual = 25923, // Boss->self, 5.0s cast, single-target
    Befoulment = 25924, // Helper->player, 5.2s cast, range 6 circle, spread
    BlightedWaterVisual = 25921, // Boss->self, 5.0s cast, single-target
    BlightedWater = 25922, // Helper->players, 5.2s cast, range 6 circle, stack
    CertainSolitude = 28349, // Boss->self, no cast, range 40 circle, dorito stack
    CoughUp1 = 25917, // Boss->self, 4.0s cast, single-target
    CoughUpAOE = 25918, // Helper->location, 4.0s cast, range 6 circle
    Miasmata = 25916, // Boss->self, 5.0s cast, range 40 circle
    NecroticFluid = 25919, // WeepingMiasma->self, 6.5s cast, range 6 circle
    NecroticMist = 28348, // Helper->location, 1.3s cast, range 6 circle
    PoxFlail = 25920, // Boss->player, 5.0s cast, single-target, tankbuster
    WaveOfNausea = 28347 // Boss->self, 5.5s cast, range 6-40 donut
}

public enum SID : uint
{
    Necrosis = 2965, // Helper->player, extra=0x0, doom
    CravenCompanionship = 2966, // Boss->player, extra=0x0, turns into Hysteria if ignored
    Hysteria = 296 // Boss->player, extra=0x0
}

class CertainSolitude(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CravenCompanionship && Stacks.Count == 0)
            Stacks.Add(new(actor, 1.5f, 4, activation: WorldState.FutureTime(5)));
    }

    public override void Update()
    {
        if (Stacks.Count != 0 && Module.Raid.WithoutSlot().All(x => x.FindStatus(SID.CravenCompanionship) == null))
            Stacks.Clear();
    }
}

class Necrosis(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _doomed = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Necrosis)
            _doomed.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Necrosis)
            _doomed.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_doomed.Contains(actor) && !(actor.Role == Role.Healer || actor.Class == Class.BRD))
            hints.Add("You were doomed! Get cleansed fast.");
        if (_doomed.Contains(actor) && (actor.Role == Role.Healer || actor.Class == Class.BRD))
            hints.Add("Cleanse yourself! (Doom).");
        foreach (var c in _doomed)
            if (!_doomed.Contains(actor) && (actor.Role == Role.Healer || actor.Class == Class.BRD))
                hints.Add($"Cleanse {c.Name}! (Doom)");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in _doomed)
        {
            if (_doomed.Count > 0 && actor.Role == Role.Healer)
                hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Esuna), c, 1, false));
            if (_doomed.Count > 0 && actor.Class == Class.BRD)
                hints.PlannedActions.Add((ActionID.MakeSpell(BRD.AID.WardensPaean), c, 1, false));
        }
    }
}

class NecroticFluidMist(BossModule module) : Components.Exaflare(module, 6)
{
    public enum Pattern { None, Southward, Northward }
    public Pattern CurrentWind { get; private set; } = Pattern.None;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x2B)
        {
            switch (state)
            {
                case 0x00020080:
                    CurrentWind = Pattern.Southward;
                    break;
                case 0x00200040:
                    CurrentWind = Pattern.Northward;
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NecroticFluid)
        {
            var numExplosions = GetNumExplosions(caster);
            var advance = 6 * (CurrentWind == Pattern.Southward ? 0.Degrees().ToDirection() : 180.Degrees().ToDirection());
            Lines.Add(new() { Next = caster.Position, Advance = advance, NextExplosion = spell.NPCFinishAt, TimeToMove = 2, ExplosionsLeft = numExplosions, MaxShownExplosions = 5 });
        }
    }

    private int GetNumExplosions(Actor caster)
    {
        return CurrentWind switch
        {
            Pattern.Southward => GetSouthwardExplosions(caster.Position, Module.Center),
            Pattern.Northward => GetNorthwardExplosions(caster.Position, Module.Center),
            _ => 0
        };
    }

    private static int GetSouthwardExplosions(WPos position, WPos center)
    {
        return position.Z switch
        {
            -184.5f => 3,
            -187.5f => 4,
            -191.4f => 5,
            -194.5f => 6,
            -196.9f => 7,
            _ => position.Z > -178 ? 1 : ((position - center).Length() > 10 && position.Z == -178) ? 3 : 4,
        };
    }

    private static int GetNorthwardExplosions(WPos position, WPos center)
    {
        return position.Z switch
        {
            -171.5f => 3,
            -168.5f => 4,
            -164.6f => 5,
            -161.5f => 6,
            -159.1f => 7,
            _ => position.Z < -178 ? 1 : ((position - center).Length() > 10 && position.Z == -178) ? 3 : 4,
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NecroticFluid or AID.NecroticMist)
        {
            Advance((AID)spell.Action.ID == AID.NecroticFluid ? caster.Position : spell.LocXZ);
            ++NumCasts;
        }
    }

    private void Advance(WPos position)
    {
        var index = Lines.FindIndex(item => item.Next.AlmostEqual(position, 1));
        if (index != -1)
        {
            AdvanceLine(Lines[index], position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
            if (Lines.Count == 0)
                CurrentWind = Pattern.None;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<WaveOfNausea>()!.ActiveAOEs(slot, actor).Any())
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Befoulment(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Befoulment), 6);
class BlightedWater(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BlightedWater), 6, 4);
class CoughUpAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.CoughUpAOE), 6);

class WaveOfNausea(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeDonut donut = new(6, 40);
    private static readonly List<Shape> differenceShapes = [new Circle(new(271.5f, -178), 6), new Circle(new(261.5f, -178), 6)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WaveOfNausea)
        {
            _aoes.Add(new(donut, caster.Position, default, spell.NPCFinishAt));
            if (Module.FindComponent<NecroticFluidMist>()!.CurrentWind == NecroticFluidMist.Pattern.Southward)
                _aoes.Add(new(new AOEShapeCustom([new ConeHA(caster.Position, 6, 180.Degrees(), 90.Degrees())], differenceShapes, true), caster.Position, default, spell.NPCFinishAt, ArenaColor.SafeFromAOE));
            else if (Module.FindComponent<NecroticFluidMist>()!.CurrentWind == NecroticFluidMist.Pattern.Northward)
                _aoes.Add(new(new AOEShapeCustom([new ConeHA(caster.Position, 6, 0.Degrees(), 90.Degrees())], differenceShapes, true), caster.Position, default, spell.NPCFinishAt, ArenaColor.SafeFromAOE));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WaveOfNausea)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveAOEs(default, default!).Any() && Module.FindComponent<NecroticFluidMist>()!.ActiveAOEs(default, default!).Any())
            hints.Add("Wait in marked safespot for donut resolve!");
    }
}

class PoxFlail(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.PoxFlail));
class Miasmata(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Miasmata));

class D061CausticGrebuloffStates : StateMachineBuilder
{
    public D061CausticGrebuloffStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Necrosis>()
            .ActivateOnEnter<Befoulment>()
            .ActivateOnEnter<BlightedWater>()
            .ActivateOnEnter<CoughUpAOE>()
            .ActivateOnEnter<NecroticFluidMist>()
            .ActivateOnEnter<WaveOfNausea>()
            .ActivateOnEnter<PoxFlail>()
            .ActivateOnEnter<CertainSolitude>()
            .ActivateOnEnter<Miasmata>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10313)]
public class D061CausticGrebuloff(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly List<Shape> union = [new Circle(new(266.5f, -178), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(266.5f, -198.75f), 20, 2), new Rectangle(new(266.5f, -157), 20, 2)];
    public static readonly ArenaBoundsComplex DefaultBounds = new(union, difference);
}
