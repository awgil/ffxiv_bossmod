namespace BossMod.RealmReborn.Dungeon.D10StoneVigil.D101ChudoYudo
{
    public enum OID : uint
    {
        Boss = 0x5B5, // x1
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast
        Rake = 901, // Boss->player, no cast, extra attack on tank
        LionsBreath = 902, // Boss->self, 1.0s cast, range 10.25 ?-degree cone aoe
        Swinge = 903, // Boss->self, 4.0s cast, range 40 ?-degree cone aoe
    };

    class LionsBreath : Components.SelfTargetedLegacyRotationAOEs
    {
        public LionsBreath() : base(ActionID.MakeSpell(AID.LionsBreath), new AOEShapeCone(10.25f, 60.Degrees())) { } // TODO: verify angle
    }

    class Swinge : Components.SelfTargetedLegacyRotationAOEs
    {
        public Swinge() : base(ActionID.MakeSpell(AID.Swinge), new AOEShapeCone(40, 30.Degrees())) { } // TODO: verify angle
    }

    // due to relatively short casts and the fact that boss likes moving across arena to cast swinge, we always want non-tanks to be positioned slightly behind
    class Positioning : BossComponent
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (actor.Role != Role.Tank)
                hints.AddForbiddenZone(ShapeDistance.Cone(module.PrimaryActor.Position, 10, module.PrimaryActor.Rotation, 90.Degrees()));
        }
    }

    class D101ChudoYudoStates : StateMachineBuilder
    {
        public D101ChudoYudoStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<LionsBreath>()
                .ActivateOnEnter<Swinge>()
                .ActivateOnEnter<Positioning>();
        }
    }

    public class D101ChudoYudo : BossModule
    {
        public D101ChudoYudo(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, 115), 20)) { }
    }
}
