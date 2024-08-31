namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class TagTeamLariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private readonly Actor?[] _tetherSource = new Actor?[PartyState.MaxPartySize];

    private static readonly AOEShapeRect _shape = new(70, 17);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in AOEs)
        {
            var safe = _tetherSource[slot]?.Position.AlmostEqual(aoe.Origin, 25) ?? false;
            if (safe)
                yield return aoe with
                {
                    Origin = Module.Center - (aoe.Origin - Module.Center),
                    Rotation = aoe.Rotation + 180.Degrees(),
                    Shape = new AOEShapeRect(70, 7f)
                };
            else
                yield return aoe;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        int hitByWrong = 0, notHitByNeeded = 0;
        foreach (var aoe in ActiveAOEs(slot, actor).Where(aoe => aoe.Check(actor.Position) == aoe.Risky))
        {
            if (aoe.Risky)
                ++hitByWrong;
            else
                ++notHitByNeeded;
        }

        if (hitByWrong > 0)
            hints.Add(WarningText);
        if (notHitByNeeded > 0)
            hints.Add("Go into cleave!");
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
            AOEs.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
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

class FusesOfFuryMurderousMist(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FusesOfFuryMurderousMist), new AOEShapeCone(40, 45.Degrees(), 180.Degrees()))
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Any(c => Shape.Check(actor.Position, c.Position, c.CastInfo!.Rotation)))
            hints.Add("Get hit by mist!");
    }
}
