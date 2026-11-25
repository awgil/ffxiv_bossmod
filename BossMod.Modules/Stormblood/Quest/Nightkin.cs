namespace BossMod.Stormblood.Quest.Nightkin;

public enum OID : uint
{
    Boss = 0x1B99, // R2.850
    Voidsphere = 0x1BA3, // R1.000
    LastingManuscript = 0x1B98, // R0.950
    DeepRedXrhunTia = 0x1B97, // R0.500
    Helper = 0x1D68, // R0.500
}

public enum AID : uint
{
    Thrub = 8217, // Boss->player, no cast, single-target
    VoidSparkBait = 8216, // Boss->player, 3.7s cast, single-target, drops a Voidsphere on the player
    VoidSpark = 8261, // Voidsphere->self, no cast, range 7+R circle, pulses every ~5.1s, gives Vulnerability Up
    MagicBurst = 8219, // Boss->self, 3.7s cast, range 15 circle
    DeepDarkness = 8218, // Boss->self, 2.7s cast, range 22 donut
    Reprint = 9196, // Boss->self, 14.7s cast, spawns 4 Lasting Manuscripts
}

class Voidsphere(BossModule module) : Components.GenericAOEs(module, AID.VoidSpark)
{
    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(OID.Voidsphere).Where(s => !s.IsDead).Select(s => new AOEInstance(_shape, s.Position));
}

class MagicBurst(BossModule module) : Components.StandardAOEs(module, AID.MagicBurst, 15);
class DeepDarkness(BossModule module) : Components.StandardAOEs(module, AID.DeepDarkness, new AOEShapeDonut(11.5f, 25));

class EverlivingBibliotaphStates : StateMachineBuilder
{
    public EverlivingBibliotaphStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Voidsphere>()
            .ActivateOnEnter<MagicBurst>()
            .ActivateOnEnter<DeepDarkness>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68121, NameID = 3930)]
public class EverlivingBibliotaph(ModuleInit init) : BossModule(init, new(377.4f, -59.7f), new ArenaBoundsCircle(25))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable), ArenaColor.Enemy);
}
