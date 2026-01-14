namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class CrownOfArcadia(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_CrownOfArcadia);

class RawSteelTrophyAxe(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RawSteelTrophyAxe)
        {
            // tankbuster is actually baited on 2nd enmity target, but let's not make this too complicated
            var tanksFirst = Raid.WithoutSlot().OrderByDescending(t => t.Role == Role.Tank);
            if (tanksFirst.First() is { } tank)
                Stacks.Add(new(tank, 6, maxSize: 2, activation: WorldState.FutureTime(8.3f)));
            foreach (var t in tanksFirst.Skip(2))
                Spreads.Add(new(t, 6, WorldState.FutureTime(9.4f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_RawSteel1:
                NumCasts++;
                Stacks.Clear();
                break;
            case AID._Weaponskill_Impact:
                NumCasts++;
                if (Spreads.Count > 0)
                    Spreads.RemoveAt(0);
                break;
        }
    }
}

class RawSteelTrophyScythe(BossModule module) : Components.UntelegraphedBait(module)
{
    public static readonly AOEShape Tankbuster = new AOEShapeCone(60, 45.Degrees());
    public static readonly AOEShape Stack = new AOEShapeCone(60, 22.5f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RawSteelTrophyScythe)
        {
            CurrentBaits.Add(new(Module.PrimaryActor.Position, Raid.WithSlot().WhereActor(t => t.Role == Role.Tank).Take(2).Mask(), Tankbuster, WorldState.FutureTime(9.4f), type: AIHints.PredictedDamageType.Tankbuster));
            CurrentBaits.Add(new(Module.PrimaryActor.Position, Raid.WithSlot().WhereActor(t => t.Role != Role.Tank).Mask(), Stack, WorldState.FutureTime(9.4f), count: 1, stackSize: 4));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_RawSteel4:
                CurrentBaits.RemoveAll(b => b.Type == AIHints.PredictedDamageType.Tankbuster);
                break;
            case AID._Weaponskill_HeavyHitter:
                CurrentBaits.RemoveAll(b => b.Type == AIHints.PredictedDamageType.Shared);
                break;
        }
    }
}
class RawSteelTrophyCounter(BossModule module) : Components.CastCounterMulti(module, [AID._Weaponskill_Impact, AID._Weaponskill_RawSteel4]);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1073, NameID = 14305)]
public class RM11STheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
