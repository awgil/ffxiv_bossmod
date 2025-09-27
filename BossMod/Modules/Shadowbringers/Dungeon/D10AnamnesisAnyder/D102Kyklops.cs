namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D102Kyklops;

public enum OID : uint
{
    Boss = 0x2CFE,
    Helper = 0x233C,
}

public enum AID : uint
{
    TheFinalVerse = 19288, // Boss->self, 4.0s cast, range 40 circle
    W2000MinaSwing = 19285, // Boss->self, 4.0s cast, range 12 circle
    EyeOfTheCyclone = 19287, // Boss->self, 4.0s cast, range 8-25 donut
    TerribleHammer = 19293, // Helper->self, no cast, range 10 width 10 rect
    TerribleBlade = 19294, // Helper->self, no cast, range 10 width 10 rect
    RagingGlower = 19286, // Boss->self, 3.0s cast, range 45 width 6 rect
    W2000MinaSwipe = 19284, // Boss->self, 4.0s cast, range 12 120-degree cone
    OpenHearth = 19296, // Helper->player, 5.0s cast, range 6 circle
    WanderersPyre = 19295, // Helper->player, 5.0s cast, range 5 circle
}

class FinalVerse(BossModule module) : Components.RaidwideCast(module, AID.TheFinalVerse);
class C2000MinaSwing(BossModule module) : Components.StandardAOEs(module, AID.W2000MinaSwing, new AOEShapeCircle(12));
class WanderersPyre(BossModule module) : Components.SpreadFromCastTargets(module, AID.WanderersPyre, 5);
class OpenHearth(BossModule module) : Components.StackWithCastTargets(module, AID.OpenHearth, 6);
class RagingGlower(BossModule module) : Components.StandardAOEs(module, AID.RagingGlower, new AOEShapeRect(45, 3));
class C2000MinaSwipe(BossModule module) : Components.StandardAOEs(module, AID.W2000MinaSwipe, new AOEShapeCone(12, 60.Degrees()));
class EyeOfTheCyclone(BossModule module) : Components.StandardAOEs(module, AID.EyeOfTheCyclone, new AOEShapeDonut(8, 25));
class Terrible(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<List<AOEInstance>> aoes = [];

    private static readonly WDir[] X = [new(-10, 10), new(10, 10), new(0, 0), new(-10, -10), new(10, -10)];
    private static readonly WDir[] Plus = [new(0, 10), new(10, 0), new(0, -10), new(-10, 0)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes.FirstOrDefault([]);

    public override void OnMapEffect(byte index, uint state)
    {
        if (aoes.Count > 0)
            return;

        switch (state)
        {
            case 0x08000400:
            case 0x02000100:
                AddPattern(X, Plus);
                break;
            case 0x20001000:
            case 0x00800040:
                AddPattern(Plus, X);
                break;
        }
    }

    private void AddPattern(WDir[] first, WDir[] second)
    {
        aoes.Add([.. first.Select(d => new AOEInstance(new AOEShapeRect(5, 5, 5), Arena.Center + d, default, WorldState.FutureTime(16f)))]);
        aoes.Add([.. second.Select(d => new AOEInstance(new AOEShapeRect(5, 5, 5), Arena.Center + d, default, WorldState.FutureTime(18.2f)))]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TerribleHammer or AID.TerribleBlade)
        {
            if (aoes.Count > 0)
            {
                aoes[0].RemoveAt(0);
                if (aoes[0].Count == 0)
                    aoes.RemoveAt(0);
            }
        }
    }
}

class KyklopsStates : StateMachineBuilder
{
    public KyklopsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FinalVerse>()
            .ActivateOnEnter<C2000MinaSwing>()
            .ActivateOnEnter<C2000MinaSwipe>()
            .ActivateOnEnter<WanderersPyre>()
            .ActivateOnEnter<OpenHearth>()
            .ActivateOnEnter<RagingGlower>()
            .ActivateOnEnter<EyeOfTheCyclone>()
            .ActivateOnEnter<Terrible>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714, NameID = 9263)]
public class Kyklops(WorldState ws, Actor primary) : BossModule(ws, primary, new(20, -80), new ArenaBoundsSquare(14.5f));
