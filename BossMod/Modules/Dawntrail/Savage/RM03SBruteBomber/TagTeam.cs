namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class TagTeamLariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private readonly Actor?[] _tetherSource = new Actor?[PartyState.MaxPartySize];

    private static readonly AOEShapeRect _shapeReal = new(70, 17);
    private static readonly AOEShapeRect _shapeInverted = new(70, 7); // offset is 12 => this should be equal to 12*2-17

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in AOEs)
        {
            yield return _tetherSource[slot]?.Position.AlmostEqual(aoe.Origin, 25) ?? false
                ? new(_shapeInverted, Module.Center - (aoe.Origin - Module.Center), aoe.Rotation + 180.Degrees(), aoe.Activation)
                : aoe;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainDeathmatch && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            _tetherSource[slot] = source;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TagTeamLariatComboFirstRAOE or AID.TagTeamLariatComboFirstLAOE or AID.FusesOfFuryLariatComboFirstRAOE or AID.FusesOfFuryLariatComboFirstLAOE)
            AOEs.Add(new(_shapeReal, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TagTeamLariatComboFirstRAOE or AID.TagTeamLariatComboFirstLAOE or AID.TagTeamLariatComboSecondRAOE or AID.TagTeamLariatComboSecondLAOE
            or AID.FusesOfFuryLariatComboFirstRAOE or AID.FusesOfFuryLariatComboFirstLAOE or AID.FusesOfFuryLariatComboSecondRAOE or AID.FusesOfFuryLariatComboSecondLAOE)
        {
            ++NumCasts;
            Array.Fill(_tetherSource, null);
            var index = AOEs.FindIndex(aoe => aoe.Origin.AlmostEqual(spell.LocXZ, 1));
            if (index < 0)
            {
                ReportError($"Failed to find AOE for {spell.LocXZ}");
            }
            else if ((AID)spell.Action.ID is AID.TagTeamLariatComboFirstRAOE or AID.TagTeamLariatComboFirstLAOE or AID.FusesOfFuryLariatComboFirstRAOE or AID.FusesOfFuryLariatComboFirstLAOE)
            {
                ref var aoe = ref AOEs.Ref(index);
                aoe.Origin = Module.Center - (aoe.Origin - Module.Center);
                aoe.Rotation += 180.Degrees();
                aoe.Activation = WorldState.FutureTime(4.3f);
            }
            else
            {
                AOEs.RemoveAt(index);
            }
        }
    }
}

// players always need to get hit by this mechanic
class FusesOfFuryMurderousMist : Components.SelfTargetedAOEs
{
    public FusesOfFuryMurderousMist(BossModule module) : base(module, ActionID.MakeSpell(AID.FusesOfFuryMurderousMist), new AOEShapeCone(40, 45.Degrees(), 180.Degrees()))
    {
        WarningText = "Get hit by mist!";
    }
}
