namespace BossMod.RealmReborn.Dungeon.D15WanderersPalace.D152GiantBavarois
{
    public enum OID : uint
    {
        Boss = 0x41D, // x1
        WhiteBavarois = 0x41E, // spawn during fight
        GreenBavarois = 0x41F, // spawn during fight
        PurpleBavarois = 0x421, // spawn during fight
        BlueBavarois = 0x422, // spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss/WhiteBavarois/GreenBavarois/PurpleBavarois/BlueBavarois->player, no cast, single-target
        AmorphicFlail = 943, // Boss/WhiteBavarois/GreenBavarois/PurpleBavarois/BlueBavarois->self, no cast, range 5+R ?-degree cone
        Fire = 1394, // Boss->player, 3.0s cast, single-target
        Blizzard = 1395, // WhiteBavarois->player, 1.0s cast, single-target
        Aero = 1397, // GreenBavarois->player, 1.0s cast, single-target
        Thunder = 1396, // PurpleBavarois->player, 1.0s cast, single-target
        Water = 971, // BlueBavarois->player, 1.0s cast, single-target
    };

    public enum IconID : uint
    {
        AmorphicFlail = 1, // player
    };

    class Fire : Components.SingleTargetCast
    {
        public Fire() : base(ActionID.MakeSpell(AID.Fire), "Single-target damage") { }
    }

    // TODO: verify implementation; find a condition for kite end
    class AmorphicFlail : BossComponent
    {
        private Actor? _kiter;

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (actor == _kiter)
                hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 8));
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.AmorphicFlail)
                _kiter = actor;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (caster == module.PrimaryActor && (AID)spell.Action.ID == AID.AmorphicFlail)
                _kiter = null;
        }
    }

    class D152GiantBavaroisStates : StateMachineBuilder
    {
        public D152GiantBavaroisStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Fire>()
                .ActivateOnEnter<AmorphicFlail>();
        }
    }

    public class D152GiantBavarois : BossModule
    {
        public D152GiantBavarois(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(43, -232), 20)) { }
    }
}
