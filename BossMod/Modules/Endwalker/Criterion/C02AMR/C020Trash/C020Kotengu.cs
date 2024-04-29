namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash2;

class BladeOfTheTengu(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(50, 45.Degrees()); // TODO: verify angle

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var secondAngle = (AID)spell.Action.ID switch
        {
            AID.NBackwardBlows or AID.SBackwardBlows => 180.Degrees(),
            AID.NLeftwardBlows or AID.SLeftwardBlows => 90.Degrees(),
            AID.NRightwardBlows or AID.SRightwardBlows => -90.Degrees(),
            _ => default
        };
        if (secondAngle != default)
        {
            NumCasts = 0;
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt.AddSeconds(0.1f)));
            _aoes.Add(new(_shape, caster.Position, spell.Rotation + secondAngle, spell.NPCFinishAt.AddSeconds(1.9f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBladeOfTheTengu or AID.SBladeOfTheTengu)
        {
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }
}

class WrathOfTheTengu(BossModule module, AID aid) : Components.RaidwideCast(module, ActionID.MakeSpell(aid), "Raidwide with bleed");
class NWrathOfTheTengu(BossModule module) : WrathOfTheTengu(module, AID.NWrathOfTheTengu);
class SWrathOfTheTengu(BossModule module) : WrathOfTheTengu(module, AID.SWrathOfTheTengu);

class GazeOfTheTengu(BossModule module, AID aid) : Components.CastGaze(module, ActionID.MakeSpell(aid));
class NGazeOfTheTengu(BossModule module) : GazeOfTheTengu(module, AID.NGazeOfTheTengu);
class SGazeOfTheTengu(BossModule module) : GazeOfTheTengu(module, AID.SGazeOfTheTengu);

class C020KotenguStates : StateMachineBuilder
{
    public C020KotenguStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BladeOfTheTengu>()
            .ActivateOnEnter<NWrathOfTheTengu>(!savage)
            .ActivateOnEnter<NGazeOfTheTengu>(!savage)
            .ActivateOnEnter<SWrathOfTheTengu>(savage)
            .ActivateOnEnter<SGazeOfTheTengu>(savage)
            .ActivateOnEnter<NMountainBreeze>(!savage) // for yamabiko
            .ActivateOnEnter<SMountainBreeze>(savage);
    }
}
class C020NKotenguStates(BossModule module) : C020KotenguStates(module, false);
class C020SKotenguStates(BossModule module) : C020KotenguStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NKotengu, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12410, SortOrder = 6)]
public class C020NKotengu(WorldState ws, Actor primary) : C020Trash2(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SKotengu, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12410, SortOrder = 6)]
public class C020SKotengu(WorldState ws, Actor primary) : C020Trash2(ws, primary);
