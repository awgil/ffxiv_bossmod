namespace BossMod.Global.MaskedCarnivale.Stage20.Act3;

public enum OID : uint
{
    Boss = 0x272C, //R=4.5
    Ultros = 0x272D, //R=5.1
    Tentacle = 0x272E, //R=7.2
}

public enum AID : uint
{
    Fungah = 14705, // 272C->self, no cast, range 8+R ?-degree cone, knockback 20 away from source
    Fireball = 14706, // 272C->location, 3.5s cast, range 8 circle
    Snort = 14704, // 272C->self, 7.0s cast, range 50+R circle
    Fireball2 = 14707, // 272C->player, no cast, range 8 circle
    Tentacle = 14747, // 272E->self, 3.0s cast, range 8 circle
    Wallop = 14748, // 272E->self, 3.5s cast, range 50+R width 10 rect, knockback 20 away from source
    Clearout = 14749, // 272E->self, no cast, range 13+R ?-degree cone, knockback 20 away from source
    AquaBreath = 14745, // 272D->self, 2.5s cast, range 8+R 90-degree cone
    Megavolt = 14746, // 272D->self, 3.0s cast, range 6+R circle
    ImpSong = 14744, // 272D->self, 6.0s cast, range 50+R circle
}

class AquaBreath(BossModule module) : Components.StandardAOEs(module, AID.AquaBreath, new AOEShapeCone(13.1f, 45.Degrees()));
class Megavolt(BossModule module) : Components.StandardAOEs(module, AID.Megavolt, new AOEShapeCircle(11.1f));
class Tentacle(BossModule module) : Components.StandardAOEs(module, AID.Tentacle, new AOEShapeCircle(8));
class Wallop(BossModule module) : Components.StandardAOEs(module, AID.Wallop, new AOEShapeRect(57.2f, 5));
class WallopKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Wallop, 20, kind: Kind.AwayFromOrigin); //knockback actually delayed by 0.8s
class Fireball(BossModule module) : Components.StandardAOEs(module, AID.Fireball, 8);
class ImpSong(BossModule module) : Components.CastHint(module, AID.ImpSong, "Interrupt Ultros!");
class Snort(BossModule module) : Components.CastHint(module, AID.Snort, "Use Diamondback!");
class SnortKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Snort, 30, kind: Kind.AwayFromOrigin);  //knockback actually delayed by 0.7s

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("This act is act 1+2 combined with tentacles on top.\nThe Final Sting combo (Off-guard->Bristle->Moonflute->Final Sting) makes\nthis act including the achievement much easier. Ultros is weak to fire.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirement for achievement: Don't kill any tentacles in this act", false);
    }
}

class Stage20Act3States : StateMachineBuilder
{
    public Stage20Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Tentacle>()
            .ActivateOnEnter<Megavolt>()
            .ActivateOnEnter<AquaBreath>()
            .ActivateOnEnter<ImpSong>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<WallopKB>()
            .ActivateOnEnter<Snort>()
            .ActivateOnEnter<SnortKB>()
            .ActivateOnEnter<Fireball>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 630, NameID = 3046, SortOrder = 3)]
public class Stage20Act3 : BossModule
{
    public Stage20Act3(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Ultros))
            Arena.Actor(s, ArenaColor.Object, false);
        foreach (var s in Enemies(OID.Tentacle))
            Arena.Actor(s, ArenaColor.Object, false);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Tentacle => 2,//this ruins the achievement if boss is still alive when tentacles spawn
                OID.Ultros => 1,
                OID.Boss => 0,
                _ => 0
            };
        }
    }
}
