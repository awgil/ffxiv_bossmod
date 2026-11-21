namespace BossMod.Stormblood.Alliance.A33DarkCrusader;

public enum OID : uint
{
    Boss = 0x25E3, // R3.500, x1
    Nightstalker = 0x25E4, // R2.500, x3
    RuinsDweller = 0x25E7, // R2.800, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x10, Helper type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    DarkRite = 14207, // Boss->self, 9.0s cast, single-target
    Triclip = 14212, // 25E7->player, no cast, single-target
    Noahionto = 14206, // Boss->self, 4.0s cast, range 40 circle
    InfernalWaveCast = 14208, // Boss->self, 1.5s cast, single-target
    InfernalWave = 14209, // Helper->self, 3.0s cast, range 40+R width 8 rect
}

class DarkRite(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.None)
{
    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case 0x1EAA7B:
                Towers.Add(new(actor.Position, 3, 2, 8, activation: WorldState.FutureTime(8.6f)));
                break;
            case 0x1EAA7C:
                Towers.Add(new(actor.Position, 3, 3, 8, activation: WorldState.FutureTime(8.6f)));
                break;
            case 0x1EAA7D:
                Towers.Add(new(actor.Position, 3, 4, 8, activation: WorldState.FutureTime(8.6f)));
                break;
        }
    }

    public override void OnTimelineStateChange(Actor actor, uint state)
    {
        switch ((actor.OID, actor.TimelineState))
        {
            case (0x1EAA7B, 0x0040 or 0x0004):
            case (0x1EAA7C, 0x0004):
            case (0x1EAA7D, 0x0400 or 0x0004):
                Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
                break;
        }
    }
}

class Noahionto(BossModule module) : Components.RaidwideCast(module, AID.Noahionto);
class InfernalWave(BossModule module) : Components.StandardAOEs(module, AID.InfernalWave, new AOEShapeRect(40, 4));

class A33DarkCrusaderStates : StateMachineBuilder
{
    public A33DarkCrusaderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkRite>()
            .ActivateOnEnter<Noahionto>()
            .ActivateOnEnter<InfernalWave>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7950)]
public class A33DarkCrusader(ModuleInit init) : BossModule(init, new(-625.29f, -431.055f), new ArenaBoundsCircle(19.5f));
