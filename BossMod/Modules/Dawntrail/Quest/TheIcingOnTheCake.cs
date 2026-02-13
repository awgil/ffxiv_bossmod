using BossMod.QuestBattle;

namespace BossMod.Dawntrail.Quest.TheIcingOnTheCake;

// Valentione's Day 2026
public enum OID : uint
{
    MammetPtissier = 0x48DF, // R0.150, x?
    KupkaKupp = 0x48E0, // R0.300, x?
    Boss = KupkaKupp,

    CakeStand = 0x1EBE61,

    WhippedCreamFrosting = 0x1EBE4D, // 0x56
    ChocolateFrosting = 0x1EBE4E,
    PistachioFrosting = 0x1EBE4F,
    BerryFrosting = 0x1EBE50,
    LemonFrosting = 0x1EBE51,

    WhiteRolanberries = 0x1EBE52,
    Almonds = 0x1EBE53,
    Branchberries = 0x1EBE54,
    Rolanberries = 0x1EBE55,
    SunLemons = 0x1EBE56,

    SugarPearls = 0x1EBE57,
    ChocolateCubes = 0x1EBE58,
    ChocolateFlowers = 0x1EBE59,
    ChocolateHearts = 0x1EBE5A,
    LemonMacarons = 0x1EBE5B,

    MoogleCakeTopper = 0x1EBE5C,
    ChocolateEggCakeTopper = 0x1EBE5D,
    CactuarCakeTopper = 0x1EBE5E,
    HeartCakeTopper = 0x1EBE5F,
    ChocoboCakeTopper = 0x1EBE60, // 0x6A

    Exit = 0x1EB2CB,
}

public enum AID : uint
{
    SugarBlizzard = 44036, // 48DF->self, 3.0s cast, range 15 90-degree cone
}

public enum SID : uint
{
    Transporting = 404 // none->player, extra=0x15
}

class SugarBlizzard(BossModule module) : Components.StandardAOEs(module, AID.SugarBlizzard, new AOEShapeCone(15, 45.Degrees()));

class DecorateCake(BossModule module) : BossComponent(module)
{
    private Actor? CakeStand => WorldState.Actors.FirstOrDefault(o => o.OID == (uint)OID.CakeStand && o.IsTargetable);
    private readonly List<Actor> Ingredients = [];

    public override void OnMapEffect(byte index, uint state)
    {
        switch (state)
        {
            case 0x00020001: // ingredients deactivated
                if (MapEffectIndexToOID(index) is { } remove)
                    Ingredients.RemoveAll(i => i.OID == (uint)remove);
                break;
            case 0x00080004: // ingredients activated
                if (MapEffectIndexToOID(index) is { } add && WorldState.Actors.FirstOrDefault(o => o.OID == (uint)add) is { } actor)
                    Ingredients.Add(actor);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            hints.SetPriority(e.Actor, AIHints.Enemy.PriorityForbidden);

        if (actor.FindStatus(SID.Transporting) == null)
            hints.InteractWithTarget = Ingredients.MinBy(actor.DistanceToHitbox);
        else if (CakeStand is { })
            hints.InteractWithTarget = CakeStand;
    }

    private OID? MapEffectIndexToOID(byte index) => index switch
    {
        var ingredient when ingredient is >= 0x56 and <= 0x6A => (OID)((uint)OID.WhippedCreamFrosting + (index - 0x57)),
        _ => null
    };
}

class LeaveOnExitAppear(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (WorldState.Actors.Any(a => a.OID == (uint)OID.Exit && a.IsTargetable))
            hints.ShouldLeaveDuty = true;
    }
}

// someone can finish/fix this if they want. I really do not care enough for a seasonal quest
class BakeOffAI(BossModule module) : RotationModule<AutoBakeOff>(module);
public class AutoBakeOff(WorldState ws) : UnmanagedRotation(ws, 5)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;
        if (Player.FindStatus(SID.Transporting) != null)
            return;
        if (!Player.InCombat)
            return;
        // iterate Hints.PriorityTargets and compare against Hints.NumPriorityTargetsInAOECone. Only execute when they're the same.
        // If they're not the same, set a goal zone for an area that will hit all of them, then call useaction with the correct facing angle
        UseAction(Roleplay.AID.BakeOff, primaryTarget);
    }
}

class TheIcingOnTheCakeStates : StateMachineBuilder
{
    public TheIcingOnTheCakeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DecorateCake>()
            .ActivateOnEnter<SugarBlizzard>()
            .ActivateOnEnter<LeaveOnExitAppear>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1045, NameID = 14234)]
public class TheIcingOnTheCake(ModuleArgs init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(45))
{
    protected override bool CheckPull() => !PrimaryActor.IsDead;
}
