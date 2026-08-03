namespace BossMod.Global.Quest.FF16Collab.InfernalShadow;

class VulcanBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.VulcanBurstReal, "Time your dodge correctly");
class Hellfire(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.HellfireVisual, (uint)AID.HellfireRaidwide, 0.6f);
class Incinerate(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.IncinerateReal, 5f);

class SpreadingFire(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f), new AOEShapeDonut(30f, 40f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SpreadingFire1st)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.SpreadingFire1st => 0,
                (uint)AID.SpreadingFire2nd => 1,
                (uint)AID.SpreadingFire3rd => 2,
                (uint)AID.SpreadingFire4th => 3,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

abstract class Cone75(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40f, 75f.Degrees()));
class SmolderingClaw(BossModule module) : Cone75(module, (uint)AID.SmolderingClawReal);
class TailStrike(BossModule module) : Cone75(module, (uint)AID.TailStrikeReal);

class FireRampageCleave(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 90f.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FieryRampageCleaveReal or (uint)AID.FieryRampageCleaveReal2)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count > 1)
                SortHelpers.SortAOEByActivation(_aoes);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.FieryRampageCleaveReal or (uint)AID.FieryRampageCleaveReal2)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class FieryRampageCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FieryRampageCircleReal, 16f);
class FieryRampageRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.FieryRampageRaidwideReal, "Time your dodge correctly");
class Pyrosault(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PyrosaultReal, 10f);
class Fireball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireballReal, 6f);
class CrimsonRush(BossModule module) : Components.ChargeAOEs(module, (uint)AID.CrimsonRushReal, 10f);

class CrimsonStreak(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CrimsonStreakReal)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 10f), caster.Position.Quantized(), Angle.FromDirection(dir), Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.CrimsonStreakReal)
            _aoes.RemoveAt(0);
    }
}

class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EruptionReal, 8f);

class Eruption2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var act0 = _aoes[0].Activation;
        var compareFL = (_aoes[count - 1].Activation - act0).TotalSeconds > 1d;
        var aoes = new AOEInstance[count];
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            aoes[i] = (aoe.Activation - act0).TotalSeconds < 1d ? aoe with { Color = compareFL ? color : 0 } : aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EruptionReal2 or (uint)AID.EruptionReal3 or (uint)AID.EruptionReal4)
        {
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
            if (_aoes.Count > 1)
                SortHelpers.SortAOEByActivation(_aoes);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.EruptionReal2 or (uint)AID.EruptionReal3 or (uint)AID.EruptionReal4)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class BurningStrike(BossModule module) : BossComponent(module)
{
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BurningStrikeVisual)
            casting = true;
    }

    public override void Update()
    {
        var clives = Module.Enemies((uint)OID.DefendClive);
        var clive = clives.Count != 0 ? clives[0] : null;
        if (clive == null)
            return;
        if (clive.IsTargetable && casting)
            casting = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var clives = Module.Enemies((uint)OID.DefendClive);
        var clive = clives.Count != 0 ? clives[0] : null;
        if (clive == null)
            return;
        if (casting && !clive.IsTargetable)
            hints.Add("Prepare to defend Clive!");
        else if (clive.IsTargetable)
            hints.Add($"Interact with {clive.Name} and solve a QTE!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var clives = Module.Enemies((uint)OID.DefendClive);
        var clive = clives.Count != 0 ? clives[0] : null;
        if (clive == null)
            return;
        if (clive.IsTargetable)
            Arena.ZoneCircleOutline(clive.Position, 1.4f, Colors.Safe);
    }
}

class SearingStomp(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InfernalShroud)
        {
            ++NumCasts;
            if (NumCasts == 2)
                casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InfernalHowlReal)
            casting = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (casting)
            hints.Add("Prepare to solve a QTE!");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70334, NameID = 12564)] // also: CFC 959
public class InfernalShadow(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Clive), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.InfernalSword));
        Arena.Actors(Enemies((uint)OID.DefendClive), Colors.Object);
    }
}
