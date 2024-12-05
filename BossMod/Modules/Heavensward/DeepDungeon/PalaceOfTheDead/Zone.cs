namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.Zone;

public abstract class PalaceFloorModule : ZoneModule
{
    private readonly EventSubscriptions _subscriptions;

    private bool _passageOpen = false;

    private static readonly uint[] RevealedTrapOIDs = [0x1EA091, 0x1EA08E];

    public PalaceFloorModule(WorldState ws) : base(ws)
    {
        _subscriptions = new(
            ws.Party.Modified.Subscribe(OnPartyChange),
            ws.Actors.EventObjectAnimation.Subscribe(OnEObjAnim)
        );
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    private void OnPartyChange(PartyState.OpModify modify)
    {
        if (modify.Slot == 0 && modify.Member.InCutscene)
        {
            Service.Log($"player entered cutscene, clearing passage flag");
            _passageOpen = false;
        }
    }

    private void OnEObjAnim(Actor actor, ushort p1, ushort p2)
    {
        if (actor.OID != 0x1EA094)
            return;

        if (p1 == 4 && p2 == 8)
            _passageOpen = true;
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (player.InCombat)
            return;

        Actor? coffer = null;
        Actor? hoardLight = null;
        Actor? hoard = null;
        Actor? passage = null;
        List<Func<WPos, float>> revealedTraps = [];

        foreach (var a in World.Actors)
        {
            if ((OID)a.OID is OID.GoldCoffer or OID.BronzeCoffer or OID.BandedCoffer && a.IsTargetable)
            {
                if (coffer == null || a.DistanceToHitbox(player) < coffer.DistanceToHitbox(player))
                    coffer = a;
            }

            if (a.OID == (uint)OID.BandedCofferIndicator)
                hoardLight = a;

            if (a.OID == (uint)OID.CairnOfPassage)
                passage = a;

            if (RevealedTrapOIDs.Contains(a.OID))
                revealedTraps.Add(ShapeDistance.Circle(a.Position, 2));
        }

        hints.InteractWithTarget = hoard ?? coffer;
        if (_passageOpen && passage is Actor c)
            hints.GoalZones.Add(hints.GoalSingleTarget(c.Position, 2));

        if (revealedTraps.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Union(revealedTraps));

        //if (hoardLight is Actor h)
        //    hints.GoalZones.Add(hints.GoalSingleTarget(h.Position, 2, 10));

        foreach (var p in hints.PotentialTargets)
            p.Priority = 0;
    }
}

enum OID : uint
{
    CairnOfPassage = 0x1EA094,
    GoldCoffer = 0x1EA13E,
    BronzeCoffer = 0x322,
    BandedCofferIndicator = 0x1EA1F6,
    BandedCoffer = 0x1EA1F7,
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 174)]
public class Palace10(WorldState ws) : PalaceFloorModule(ws);

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 175)]
public class Palace20(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 176)]
public class Palace30(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 204)]
public class Palace60(WorldState ws) : PalaceFloorModule(ws);
