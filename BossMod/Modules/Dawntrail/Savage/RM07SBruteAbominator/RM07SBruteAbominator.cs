namespace BossMod.Dawntrail.Savage.RM07SBruteAbominator;

class TendrilsOfTerror(BossModule module) : Components.GroupedAOEs(module, [AID.TendrilsOfTerror1, AID.TendrilsOfTerror2, AID.TendrilsOfTerror3], new AOEShapeCross(60, 2))
{
    public void ResetCount()
    {
        NumCasts = 0;
    }
}

class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, 22, 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var c in Casters)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo));
    }
}

class NeoBombarianSpecial(BossModule module) : Components.RaidwideCast(module, AID.NeoBombarianSpecial);

class RevengeOfTheVines(BossModule module) : Components.RaidwideCast(module, AID.RevengeOfTheVines);

class AbominableBlink(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(23), (uint)IconID.Flare, AID.AbominableBlink, activationDelay: 6.5f, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (CurrentBaits.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), CurrentBaits[0].Activation);
    }
}

class StrangeSeeds(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SinisterSeed, AID.StrangeSeedsSpread, 6, 5)
{
    public bool Risky = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Risky)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Risky)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class StrangeSeedsCounter(BossModule module) : BossComponent(module)
{
    public int NumCasts;

    // no cast event if target dies, but CastFinished is always fired
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StrangeSeedsSpread)
            NumCasts++;
    }
}
class KillerSeeds(BossModule module) : Components.StackWithCastTargets(module, AID.KillerSeeds, 6, 2, 2);

class Powerslam(BossModule module) : Components.RaidwideCast(module, AID.Powerslam);
class Sporesplosion(BossModule module) : Components.StandardAOEs(module, AID.Sporesplosion, 8, maxCasts: 12);

class Lariat(BossModule module) : Components.GroupedAOEs(module, [AID.LashingLariat1, AID.LashingLariat2], new AOEShapeRect(70, 16));
class Slaminator(BossModule module) : Components.CastTowers(module, AID.Slaminator, 8, maxSoakers: 8);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1024, NameID = 13756, PlanLevel = 100)]
public class RM07SBruteAbombinator(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (Bounds.Contains(PrimaryActor.Position - Center))
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        else
        {
            var (hx, hz) = Bounds switch
            {
                ArenaBoundsRect rect => (rect.HalfWidth, rect.HalfHeight),
                ArenaBoundsSquare s => (s.HalfWidth, s.HalfWidth),
                _ => (0, 0)
            };

            // the boss looks off-center if we use regular raycast clamp to bounds
            Arena.ActorOutsideBounds(new(Math.Clamp(PrimaryActor.Position.X, Center.X - hx, Center.X + hx), Math.Clamp(PrimaryActor.Position.Z, Center.Z - hz, Center.Z + hz)), PrimaryActor.Rotation, ArenaColor.Enemy);
        }
    }
}
