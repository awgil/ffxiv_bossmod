namespace BossMod.Shadowbringers.Foray.Dalriada.D3Cuchulainn;

public enum OID : uint
{
    Boss = 0x31AB,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    PutrifiedSoul = 23695, // Boss->self, 5.0s cast, raidwide
    PutrifiedSoul1 = 23696, // Helper->self, 5.0s cast, raidwide
    BurgeoningDread = 23688, // Boss->self, 5.0s cast, forced march
    BurgeoningDread1 = 23689, // Helper->self, 5.0s cast, forced march
    FleshyNecromass = 23682, // Boss->self, 8.0s cast, single-target
    FleshyNecromass1 = 23683, // Boss->location, no cast, single-target
    FleshyNecromass2 = 23685, // Helper->self, no cast, range 12 circle
    FleshyNecromass3 = 23684, // Helper->self, no cast, range 12 circle
    FleshyNecromass4 = 24953, // Boss->location, no cast, single-target
    NecroticBillow = 23686, // Boss->self, 5.0s cast, single-target
    NecroticBillow1 = 23687, // Helper->self, 4.0s cast, range 8 circle
    AmbientPulsation = 23693, // Boss->self, 5.0s cast, single-target
    AmbientPulsation1 = 23694, // Helper->self, 8.0s cast, range 12 circle
    MightOfMalice = 23698, // Boss->player, 5.0s cast, single-target
    GhastlyAura = 24909, // Boss->self, 5.0s cast, misdirection
    GhastlyAura1 = 24910, // Helper->self, 5.0s cast, misdirection
    FellFlow = 23691, // Boss->self, 5.0s cast, range 50 120-degree cone
    FellFlow1 = 23692, // Helper->self, no cast, range 50 (12-15?)-degree cone
}

public enum SID : uint
{
    ForwardMarch = 2161, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    AboutFace = 2162, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x1/0x8/0x4/0x2
    Gelatinous = 2543, // none->player, extra=0x19B/0x1AD
    Bleeding = 642, // none->player, extra=0x0
    Infirmity = 172, // none->player, extra=0x0
    TemporaryMisdirection = 1422, // Helper/Boss->player, extra=0x2D0
}

public enum IconID : uint
{
    FellFlow = 40, // player->self
}

class FellFlowBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50, 7.5f.Degrees()), (uint)IconID.FellFlow, AID.FellFlow1, 5.2f);
class PutrifiedSoul(BossModule module) : Components.RaidwideCast(module, AID.PutrifiedSoul);
class BurgeoningDread(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, stopAtWall: true);
class NecroticBillow(BossModule module) : Components.StandardAOEs(module, AID.NecroticBillow1, new AOEShapeCircle(8));

class AmbientPulsation(BossModule module) : Components.StandardAOEs(module, AID.AmbientPulsation1, new AOEShapeCircle(12), maxCasts: 6)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count < 6 && NumCasts == 0)
            return;

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action == WatchedAction && NumCasts >= 9)
            NumCasts = 0;
    }
}
class FellFlow(BossModule module) : Components.StandardAOEs(module, AID.FellFlow, new AOEShapeCone(50, 60.Degrees()));

class Puddles(BossModule module) : Components.GenericAOEs(module)
{
    private bool Active;

    private static readonly float[] xs = [662.71f, 637.27f];
    private static readonly float[] ys = [-200.133f, -174.667f];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield break;

        foreach (var x in xs)
            foreach (var y in ys)
                yield return new AOEInstance(new AOEShapeCircle(5), new(x, y));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0C && state == 0x00020001)
            Active = true;
    }
}

class FourthMakeCuchulainnStates : StateMachineBuilder
{
    public FourthMakeCuchulainnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PutrifiedSoul>()
            .ActivateOnEnter<BurgeoningDread>()
            .ActivateOnEnter<NecroticBillow>()
            .ActivateOnEnter<AmbientPulsation>()
            .ActivateOnEnter<FellFlow>()
            .ActivateOnEnter<FellFlowBait>()
            .ActivateOnEnter<Puddles>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10004)]
public class FourthMakeCuchulainn(WorldState ws, Actor primary) : BossModule(ws, primary, new(650, -187.4f), new ArenaBoundsCircle(25.5f))
{
    public override bool DrawAllPlayers => true;
}
