namespace BossMod.Stormblood.Alliance.A33DarkCrusader;

public enum OID : uint
{
    Boss = 0x25E3,
    Helper = 0x233C,
}

class DarkRite(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.None)
{
    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case 0x1EAA7B:
                Service.Log($"{actor.TimelineState:X4}");
                Towers.Add(new(actor.Position, 3, 2, 8, activation: WorldState.FutureTime(8.6f)));
                break;
            case 0x1EAA7C:
                Service.Log($"{actor.TimelineState:X4}");
                Towers.Add(new(actor.Position, 3, 3, 8, activation: WorldState.FutureTime(8.6f)));
                break;
            case 0x1EAA7D:
                Service.Log($"{actor.TimelineState:X4}");
                Towers.Add(new(actor.Position, 3, 4, 8, activation: WorldState.FutureTime(8.6f)));
                break;
        }
    }

    public override void OnActorTimelineState(Actor actor, ushort state)
    {
        switch ((actor.OID, state))
        {
            case (0x1EAA7B, 0x0040):
            case (0x1EAA7D, 0x0400):
                Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
                break;
        }
    }
}

class A33DarkCrusaderStates : StateMachineBuilder
{
    public A33DarkCrusaderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkRite>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7950)]
public class A33DarkCrusader(ModuleInit init) : BossModule(init, new(-625.29f, -431.055f), new ArenaBoundsCircle(19.5f));
