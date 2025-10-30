namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class Roar1(BossModule module) : Components.RaidwideCast(module, AID.Roar1);
class Roar2(BossModule module) : Components.RaidwideCast(module, AID.Roar2);
class Roar3(BossModule module) : Components.RaidwideCast(module, AID.Roar3);

class WyvernsRadiancePuddle(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiancePuddleSmall, 6);
class WyvernsRadianceBigPuddle(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiancePuddleLarge, 12);

class WyvernsOuroblade(BossModule module) : Components.GroupedAOEs(module, [AID.Ouroblade1, AID.Ouroblade2, AID.Ouroblade3, AID.Ouroblade4], new AOEShapeCone(40, 90.Degrees()));

class WildEnergy(BossModule module) : Components.SpreadFromCastTargets(module, AID.WildEnergy, 6);

class SteeltailThrust(BossModule module) : Components.GroupedAOEs(module, [AID.SteeltailThrust1, AID.SteeltailThrust2], new AOEShapeRect(60, 3));

class ChainbladeCharge(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Share, AID.ChainbladeChargeStack, 6, 8.3f, minStackSize: 5);

class ForgedFury(BossModule module) : Components.CastHint(module, null, "Raidwide")
{
    private readonly List<DateTime> _activations = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ForgedFuryHit1 or AID.ForgedFuryHit2 or AID.ForgedFuryHit3)
        {
            _activations.Add(Module.CastFinishAt(spell));
            _activations.Sort();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activations.Count > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activations.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _activations[0]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ForgedFuryHit1 or AID.ForgedFuryHit2 or AID.ForgedFuryHit3)
        {
            NumCasts++;
            if (_activations.Count > 0)
                _activations.RemoveAt(0);
        }
    }
}

class SmallCrystal(BossModule module) : Components.StandardAOEs(module, AID.SmallCrystalExplosion, 6);
class BigCrystal(BossModule module) : Components.StandardAOEs(module, AID.BigCrystalExplosion, 12);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1044, NameID = 14237, PlanLevel = 100)]
public class Ex6GuardianArkveld(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
