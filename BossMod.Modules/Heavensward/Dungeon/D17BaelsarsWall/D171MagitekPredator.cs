namespace BossMod.Heavensward.Dungeon.D17BaelsarsWall.D171MagitekPredator;

public enum OID : uint
{
    Boss = 0x1938, // R2.94
    SkyArmorReinforcement = 0x1939, // R2.0
    Helper = 0x19A
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    MagitekClaw = 7346, // Boss->player, 4.0s cast, single-target, tankbuster
    MagitekHookMarker = 7349, // SkyArmorReinforcement->player, no cast, single-target
    MagitekHook = 7350, // Helper->player, no cast, single-target
    MagitekRay = 7347, // Boss->self, 3.0s cast, range 40+R width 6 rect
    MagitekMissile = 7348 // Boss->player, no cast, range ~5 circle spread (Prey)
}

public enum SID : uint
{
    Prey = 562,
    DamageUp = 290
}

class MagitekClaw(BossModule module) : Components.SingleTargetCast(module, AID.MagitekClaw);
class MagitekRay(BossModule module) : Components.StandardAOEs(module, AID.MagitekRay, new AOEShapeRect(42.94f, 3));
class SkyArmorReinforcement(BossModule module) : Components.Adds(module, (uint)OID.SkyArmorReinforcement, 1);

// TODO: verify Magitek Missile / Prey spread radius from replay
class MagitekMissile(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
            Spreads.RemoveAll(s => s.Target == actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MagitekMissile)
            Spreads.Clear();
    }
}

class D171MagitekPredatorStates : StateMachineBuilder
{
    public D171MagitekPredatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekClaw>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<SkyArmorReinforcement>()
            .ActivateOnEnter<MagitekMissile>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 219, NameID = 5560)] // TODO: clear after Magitek Missile radius verify
public class D171MagitekPredator(ModuleInit init) : BossModule(init, new(-174, 73), new ArenaBoundsSquare(19.5f));
