namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.LampasChrysine;

public enum OID : uint
{
    Boss = 0x3D40, //R=6
    BossHelper = 0x233C,
    BonusAdds_Lampas = 0x3D4D, //R=2.001, bonus loot adds
};

public enum AID : uint
{
    AutoAttack = 32287, // Boss->player, no cast, single-target
    AetherialLight = 32293, // Boss->self, 1,3s cast, single-target
    AetherialLight2 = 32294, // BossHelper->self, 3,0s cast, range 40 60-degree cone
    unknown = 32236, // Boss->self, no cast, single-target, seems to be connected to Aetherial Light
    Lightburst = 32289, // Boss->self, 3,3s cast, single-target
    Lightburst2 = 32290, // BossHelper->player, 5,0s cast, single-target
    Shine = 32291, // Boss->self, 1,3s cast, single-target
    Shine2 = 32292, // BossHelper->location, 3,0s cast, range 5 circle
    Summon = 32288, // Boss->self, 1,3s cast, single-target, spawns bonus loot adds
    Telega = 9630, // BonusAdds_Lampas->self, no cast, single-target, bonus loot add despawn
};

class Shine : Components.LocationTargetedAOEs
{
    public Shine() : base(ActionID.MakeSpell(AID.Shine2), 5) { }
}

class AetherialLight : Components.SelfTargetedAOEs
{
    public AetherialLight() : base(ActionID.MakeSpell(AID.AetherialLight2), new AOEShapeCone(40, 30.Degrees()), 4) { }
    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        return ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, (NumCasts > 2 && i < 2) ? ArenaColor.Danger : ArenaColor.AOE));
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(module, caster, spell);
        if ((AID)spell.Action.ID == AID.AetherialLight2)
            ++NumCasts;
    }
}

class Lightburst : Components.SingleTargetCast
{
    public Lightburst() : base(ActionID.MakeSpell(AID.Lightburst2)) { }
}

class Summon : Components.CastHint
{
    public Summon() : base(ActionID.MakeSpell(AID.Summon), "Calls bonus adds") { }
}

class LampasStates : StateMachineBuilder
{
    public LampasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Shine>()
            .ActivateOnEnter<AetherialLight>()
            .ActivateOnEnter<Lightburst>()
            .ActivateOnEnter<Summon>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAdds_Lampas).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12021)]
public class Lampas : BossModule
{
    public Lampas(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BonusAdds_Lampas))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAdds_Lampas => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
