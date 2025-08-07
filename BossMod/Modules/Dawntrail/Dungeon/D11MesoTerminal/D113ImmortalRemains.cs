namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D113ImmortalRemains;

public enum OID : uint
{
    Boss   = 0x48BE,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_               = 43826, // 48BE->player, no cast, single-target
    _Ability_Recollection      = 43825, // 48BE->self, 5.0s cast, range 60 circle
    _Ability_Memento           = 43809, // 48BE->self, 4.0+1,0s cast, single-target
    _Ability_Electray          = 43810, // 48BF->self, 5.0s cast, range 45 width 8 rect
    _Ability_MemoryOfTheStorm  = 43821, // 48BE->self, 4.0+1,0s cast, single-target
    _Ability_MemoryOfTheStorm1 = 43822, // 233C->self, no cast, range 60 width 12 rect
    _Ability_Bombardment       = 43811, // 233C->location, 1.5s cast, range 3 circle
    _Ability_Bombardment1      = 43812, // 233C->location, 1.5s cast, range 14 circle
    _Ability_Turmoil           = 43815, // 48BE->self, no cast, single-target
    _Ability_Turmoil1          = 43816, // 233C->self, no cast, range 40 width 20 rect
    _Ability_Impression        = 43817, // 48BE->self, no cast, single-target
    _Ability_Impression1       = 43819, // 233C->location, 5.0s cast, range 30 circle
    _Ability_Impression2       = 43818, // 233C->location, 5.0s cast, range 10 circle
    _Ability_MemoryOfThePyre   = 43823, // 48BE->self, 4.0+1,0s cast, single-target
    _Ability_MemoryOfThePyre1  = 43824, // 233C->player, 5.0s cast, single-target
}

public enum IconID : uint
{
    _Gen_Icon_share_laser_5sec_0t = 525, // Boss->player
}

class _Ability_Electray(BossModule        module) : Components.StandardAOEs(module, AID._Ability_Electray, new AOEShapeRect(45f, 4f));
class _Ability_Bombardment(BossModule     module) : Components.StandardAOEs(module, AID._Ability_Bombardment, 3f);
class _Ability_Bombardment1(BossModule     module) : Components.StandardAOEs(module, AID._Ability_Bombardment1, 14f);

class _Ability_Impression1(BossModule module) : Components.StandardAOEs(module, AID._Ability_Impression2, 10f)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime nextActivation = default;
        foreach (var c in ActiveCasters)
        {
            var color          = Color;
            var thisActivation = Module.CastFinishAt(c.CastInfo);
            yield return new AOEInstance(Shape, c.CastInfo!.LocXZ, c.CastInfo!.Rotation, thisActivation, color, Risky);
            yield return new AOEInstance(new AOEShapeDonut(12, 40f), c.CastInfo!.LocXZ, c.CastInfo!.Rotation, thisActivation, color, Risky);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var c in ActiveCasters)
        {
            var adjPos = Components.Knockback.AwayFromSource(pc.Position, c.CastInfo!.LocXZ, 11);
            Components.Knockback.DrawKnockback(pc, adjPos, Arena);
        }
    }
}

class _Ability_MemoryOfThePyre(BossModule module) : Components.SingleTargetCast(module, AID._Ability_MemoryOfThePyre);

class MemoryOfTheStorm(BossModule module) : Components.GenericWildCharge(module, 6f, AID._Ability_MemoryOfTheStorm, 60f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);

        if ((IconID)iconID is IconID._Gen_Icon_share_laser_5sec_0t)
        {
            Source     = Module.PrimaryActor;
            Activation = WorldState.FutureTime(6f);
            foreach (var (i, p) in Raid.WithSlot(true))
            {
                PlayerRoles[i] = p.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_MemoryOfTheStorm)
        {
            ++NumCasts;
            Source = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}




class D113ImmortalRemainsStates : StateMachineBuilder
{
    public D113ImmortalRemainsStates(BossModule module) : base(module)
    {
        TrivialPhase().
            ActivateOnEnter<_Ability_Electray>().
            ActivateOnEnter<_Ability_Bombardment>().
            ActivateOnEnter<_Ability_Bombardment1>().
            ActivateOnEnter<_Ability_Impression1>().
            ActivateOnEnter<_Ability_MemoryOfThePyre>().
            ActivateOnEnter<MemoryOfTheStorm>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028, NameID = 13974)]
public class D113ImmortalRemains(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(20));

