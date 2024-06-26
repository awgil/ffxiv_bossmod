namespace BossMod.Endwalker.Variant.V02MR.V021Yozakura;

class ArtOfTheFireblossom(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArtOfTheFireblossom), new AOEShapeCircle(9));
class ArtOfTheWindblossom(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArtOfTheWindblossom), new AOEShapeDonut(5, 60));
class KugeRantsui(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.KugeRantsui));
class OkaRanman(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OkaRanman));
//Temporary below, ninja mechanics are basically all previs dependent, timers are too short to be super useful

class SealOfTheFireblossom1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SealOfTheFireblossom1), new AOEShapeCircle(9));
class SealOfTheWindblossom1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SealOfTheWindblossom1), new AOEShapeDonut(5, 60));
class SealOfTheRainblossom1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SealOfTheRainblossom1), new AOEShapeCone(70, 22.5f.Degrees()));
class SealOfTheLevinblossom1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SealOfTheLevinblossom1), new AOEShapeCone(70, 22.5f.Degrees()));

class SeasonOfFire(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SeasonOfFire), new AOEShapeRect(46, 2.5f));
class SeasonOfWater(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SeasonOfWater), new AOEShapeRect(46, 2.5f));
class SeasonOfLightning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SeasonOfLightning), new AOEShapeCone(70, 22.5f.Degrees()));
class SeasonOfEarth(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SeasonOfEarth), new AOEShapeCone(70, 22.5f.Degrees()));

//Left Windy
class WindblossomWhirl2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindblossomWhirl2), new AOEShapeDonut(5.5f, 60));
class WindblossomWhirl3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindblossomWhirl3), new AOEShapeDonut(5.5f, 60));
class LevinblossomStrike2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LevinblossomStrike2), 3);
class DriftingPetals(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.DriftingPetals), 15, stopAtWall: true);

//Left Rainy
class MudrainAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MudrainAOE), 5); //persistent AOE, needs to be handled differently
class IcebloomRest(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.IcebloomRest), 6);
class ShadowflightAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ShadowflightAOE), new AOEShapeRect(10, 3));
class MudPieAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MudPieAOE), new AOEShapeRect(60, 3));

//Middle Rope Pulled
class FireblossomFlare2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FireblossomFlare2), 6);
class ArtOfTheFluff1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ArtOfTheFluff1));
class ArtOfTheFluff2(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ArtOfTheFluff2));

//Middle Rope Unpulled
class TatamiGaeshiAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RockRootArrangementVisual), new AOEShapeCircle(4));
class LevinblossomLance(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(60, 3.5f, 60);

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var increment = (IconID)iconID switch
        {
            IconID.RotateCW => -22.5f.Degrees(),
            IconID.RotateCCW => 22.5f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LevinblossomLanceFirst)
        {
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
        }
        if (_rotation != default)
            InitIfReady(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LevinblossomLanceFirst or AID.LevinblossomLanceRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, source.Position, _rotation, _increment, _activation, 1, 5));
            _rotation = default;
            _increment = default;
        }
    }
}

//Right No Dogu
class RootArrangement : Components.StandardChasingAOEs
{
    public RootArrangement(BossModule module) : base(module, new AOEShapeCircle(4), ActionID.MakeSpell(AID.RockRootArrangementFirst), ActionID.MakeSpell(AID.RockRootArrangementRest), 4, 1, 4)
    {
        ExcludedTargets = Raid.WithSlot(true).Mask();
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.RootArrangement)
            ExcludedTargets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

//Right Dogu

class Witherwind(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IReadOnlyList<Actor> _spirits = module.Enemies(OID.AutumnalTempest);

    private static readonly AOEShapeCircle _shape = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _spirits.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, Category = BossModuleInfo.Category.Criterion, GroupID = 945, NameID = 12325)]
public class V021Yozakura(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.X < -700 ? new(-775, 16) : primary.Position.X > 700 ? new(737, 220) : new(47, 93), new ArenaBoundsSquare(20));
