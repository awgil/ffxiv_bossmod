namespace BossMod.Endwalker.Quest.WhereEverythingBegins.P1;

public enum OID : uint
{
    Boss = 0x39B3,
    Helper = 0x233C,
}

public enum AID : uint
{
    Nox = 30021, // Helper->self, 8.0s cast, range 10 circle
    VoidVortex = 30025, // Helper->39BE, 5.0s cast, range 6 circle
    VoidGravity = 30023, // Helper->player/39BC/39BF/39BE, 5.0s cast, range 6 circle
}

class Nox(BossModule module) : Components.StandardAOEs(module, AID.Nox, new AOEShapeCircle(10), maxCasts: 5);
class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, AID.VoidGravity, 6);
class VoidVortex(BossModule module) : Components.StackWithCastTargets(module, AID.VoidVortex, 6);

class ScarmiglioneStates : StateMachineBuilder
{
    public ScarmiglioneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<VoidGravity>()
            .ActivateOnEnter<VoidVortex>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70130, NameID = 11407)]
public class Scarmiglione(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -148), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
