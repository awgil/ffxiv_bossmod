namespace BossMod.Global.MaskedCarnivale.Stage24.Act3;

public enum OID : uint
{
    Boss = 0x2739, //R=3.0
    ArenaMagus = 0x273A, //R=1.0
    VacuumWave = 0x273B, //R=1.0
}

public enum AID : uint
{
    AutoAttack = 6497, // 2739->player, no cast, single-target
    PageTear = 15324, // 2739->self, 3.5s cast, range 6+R 90-degree cone
    MagicHammer = 15327, // 2739->location, 3.0s cast, range 8 circle
    GaleCut = 15323, // 2739->self, 3.0s cast, single-target
    HeadDown = 15325, // 2739->player, 5.0s cast, width 8 rect charge, knockback 10, source forward
    VacuumBlade = 15328, // 273B->self, 3.0s cast, range 3 circle
    BoneShaker = 15326, // 2739->self, 3.0s cast, range 50+R circle, raidwide + adds
    Fire = 14266, // 273A->player, 1.0s cast, single-target
    SelfDetonate = 15329, // 273A->player, 3.0s cast, single-target
}

class MagicHammer(BossModule module) : Components.StandardAOEs(module, AID.MagicHammer, 8);
class PageTear(BossModule module) : Components.StandardAOEs(module, AID.PageTear, new AOEShapeCone(8, 45.Degrees()));

class VacuumBlade(BossModule module) : Components.GenericAOEs(module)
{
    private bool activeVacuumWave;
    private DateTime _activation;
    private static readonly AOEShapeCircle circle = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (activeVacuumWave)
            foreach (var p in Module.Enemies(OID.VacuumWave))
                yield return new(circle, p.Position, default, _activation);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.VacuumWave)
        {
            activeVacuumWave = true;
            _activation = WorldState.FutureTime(7.7f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VacuumBlade)
            activeVacuumWave = false;
    }
}

class HeadDown(BossModule module) : Components.BaitAwayChargeCast(module, AID.HeadDown, 4);

class HeadDownKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.HeadDown, 10, kind: Kind.DirForward)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (Module.FindComponent<VacuumBlade>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false)
            return true;
        if (!Module.InBounds(pos))
            return true;
        else
            return false;
    }
}

class BoneShaker(BossModule module) : Components.RaidwideCast(module, AID.BoneShaker, "Adds + Raidwide");

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.Enemies(OID.ArenaMagus).All(e => e.IsDead))
            hints.Add($"Kill {Module.Enemies(OID.ArenaMagus).FirstOrDefault()!.Name} fast or wipe!\nUse ranged physical attacks.");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} spawns two adds when casting Boneshaker. These should be a\npriority or they will explode and wipe you. To kill them without touching\nthe electric field use a ranged physical attack such as Fire Angon.\nYou can start the Final Sting combination at about 50% health left.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage24Act3States : StateMachineBuilder
{
    public Stage24Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<PageTear>()
            .ActivateOnEnter<MagicHammer>()
            .ActivateOnEnter<VacuumBlade>()
            .ActivateOnEnter<HeadDown>()
            .ActivateOnEnter<HeadDownKB>()
            .ActivateOnEnter<BoneShaker>()
            .ActivateOnEnter<Hints2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 634, NameID = 8125, SortOrder = 3)]
public class Stage24Act3 : BossModule
{
    public Stage24Act3(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ArenaMagus))
            Arena.Actor(s, ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.ArenaMagus => 1, //TODO: ideally Magus should only be attacked with ranged physical abilities
                OID.Boss => 0,
                _ => 0
            };
        }
    }
}
