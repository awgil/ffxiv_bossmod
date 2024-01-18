using System.Linq;

namespace BossMod.MaskedCarnivale.Stage02.Act2.ArenaGelato
{
    public enum OID : uint
    {
        Boss = 0x25C1, //R1.8
        Flan = 0x25C5, //R1.8
        Licorice = 0x25C3, //R=1.8

    };

public enum AID : uint
{
    Water = 14271, // 25C5->player, 1,0s cast, single-target
    Stone = 14270, // 25C3->player, 1,0s cast, single-target
    Blizzard = 14267, // 25C1->player, 1,0s cast, single-target
    GoldenTongue = 14265, // 25C5/25C3/25C1->self, 5,0s cast, single-target
};

class GoldenTongue : Components.CastHint
    {
        public GoldenTongue() : base(ActionID.MakeSpell(AID.GoldenTongue), "Can be interrupted, increases its magic defenses.") { }
    }
class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("To beat this stage in a timely manner,\nyou should have at least one spell of each element.\n(Water, Fire, Ice, Lightning, Earth and Wind)");
        } 
    }
class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Gelato is weak to fire spells.\nFlan is weak to lightning spells.\nLicorice is weak to water spells.");
        } 
    }    

class Stage02GelatoStates : StateMachineBuilder
    {
        public Stage02GelatoStates(Stage02Gelato module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<GoldenTongue>()
            .ActivateOnEnter<Hints2>()               
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () => module.MainBoss().IsDead || module.MainBoss().IsDestroyed;
        }
    }
[ModuleInfo(PrimaryActorOID = (uint)OID.Boss)]
public class Stage02Gelato : BossModule
    {
        public Actor? Flan;
        public Actor? Licorice;
        public Actor? Boss;
        public Actor MainBoss() => Flan ?? Licorice ?? PrimaryActor;
        public Stage02Gelato(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override void UpdateModule()
        {
            Licorice ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Licorice).FirstOrDefault() : null;
            Flan ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Licorice).FirstOrDefault() : null;
        }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Flan))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Licorice))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}