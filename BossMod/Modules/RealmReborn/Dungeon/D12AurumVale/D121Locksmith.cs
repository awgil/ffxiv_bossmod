namespace BossMod.RealmReborn.Dungeon.D12AurumVale.D121Locksmith;

public enum OID : uint
{
    Boss = 0x5BF, // x1
}

public enum AID : uint
{
    AutoAttack = 1350, // Boss->player, no cast, single-target
    HundredLashings = 1031, // Boss->self, no cast, range 8+R ?-degree cone
    GoldRush = 1032, // Boss->self, no cast, raidwide
    GoldDust = 1033, // Boss->location, 3.5s cast, range 8 circle
}

class HundredLashings(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.HundredLashings), new AOEShapeCone(12, 45.Degrees())); // TODO: verify angle
class GoldDust(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GoldDust), 8);

// arena has multiple weirdly-shaped puddles, so just prefer standing in large safe zone
class AIPosition(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(30, 0), 5));
    }
}

class D121LocksmithStates : StateMachineBuilder
{
    public D121LocksmithStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HundredLashings>()
            .ActivateOnEnter<GoldDust>()
            .ActivateOnEnter<AIPosition>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 5, NameID = 1534)]
public class D121Locksmith(WorldState ws, Actor primary) : BossModule(ws, primary, new(35, 0), new ArenaBoundsRect(15, 25));
