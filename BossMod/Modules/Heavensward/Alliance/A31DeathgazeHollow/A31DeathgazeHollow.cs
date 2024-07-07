namespace BossMod.Heavensward.Alliance.A31DeathgazeHollow;

class DarkII(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkII), new AOEShapeCone(50, 30.Degrees()));
class BoltOfDarkness3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BoltOfDarkness3), new AOEShapeRect(31, 10));
class VoidDeath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VoidDeath), new AOEShapeCircle(10));

//yoinked from Sephirot Unreal and stripped down as both mechancics use the same logic and iconID
class VoidAeroII(BossModule module) : BossComponent(module)
{
    private BitMask _greenTargets;
    private BitMask _purpleTargets;

    private const float _greenRadius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if ((_greenTargets | _purpleTargets).None())
            return;

        bool clippedByGreen = Raid.WithSlot().Exclude(slot).IncludedInMask(_greenTargets).InRadius(actor.Position, _greenRadius).Any();
        hints.Add($"Spread! (debuff: {(_greenTargets[slot] ? "green" : _purpleTargets[slot] ? "purple" : "none")})", clippedByGreen);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _greenTargets[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (slot, actor) in Raid.WithSlot().IncludedInMask(_greenTargets))
            Arena.AddCircle(actor.Position, _greenRadius, 0xff00ff00, slot == pcSlot ? 2 : 1);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.VoidAeroII:
                _greenTargets.Clear(Raid.FindSlot(spell.MainTargetID));
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.WindSpread:
                _greenTargets.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}

class VoidBlizzardIIIAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VoidBlizzardIIIAOE), new AOEShapeCone(60, 10.Degrees()));

class VoidAeroIVKB1(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.VoidAeroIVKB1), 37, kind: Kind.DirLeft, stopAtWall: true);
class VoidAeroIVKB2(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.VoidAeroIVKB2), 37, kind: Kind.DirRight, stopAtWall: true);

class Unknown3(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Unknown3), 20, stopAtWall: true);
class VoidDeathKB2(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.VoidDeathKB2), 15, kind: Kind.TowardsOrigin, stopAtWall: true);
class VoidDeathKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.VoidDeathKB), 40, kind: Kind.TowardsOrigin, stopAtWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5507)]
public class A31DeathgazeHollow(WorldState ws, Actor primary) : BossModule(ws, primary, new(300, 410), new ArenaBoundsRect(30, 15))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.VoidSprite), ArenaColor.Enemy);
    }
}
