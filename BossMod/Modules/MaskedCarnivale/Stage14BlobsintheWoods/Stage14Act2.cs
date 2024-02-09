using System.Linq;
using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage14.Act2
{
    public enum OID : uint
    {
        Boss = 0x271E, //R=2.0
    };

    public enum AID : uint
    {
        Syrup = 14757, // 271E->player, no cast, range 4 circle, applies heavy to player
        TheLastSong = 14756, // 271E->self, 6,0s cast, range 60 circle, heavy dmg, applies silence to player
    };

    class LastSong : GenericLineOfSightAOE
    {
        public LastSong() : base(ActionID.MakeSpell(AID.TheLastSong), 60, true) { } //TODO: find a way to use the obstacles on the map and draw proper AOEs, this does nothing right now
    }

    class LastSongHint : BossComponent
    {
        public static bool casting;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.TheLastSong)
                casting = true;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.TheLastSong)
                casting = false;
        }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (casting)
                hints.Add("Use the cube to take cover!");
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Same as first act, but the slimes will apply heavy to you.\nUse Loom to get out of line of sight as soon as Final Song gets casted.");
        }
    }

    class Stage14Act2States : StateMachineBuilder
    {
        public Stage14Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<LastSong>()
                .ActivateOnEnter<LastSongHint>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && !LastSongHint.casting;
        }
    }

    [ModuleInfo(CFCID = 624, NameID = 8108)]
    public class Stage14Act2 : BossModule
    {
        public Stage14Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
            ActivateComponent<LayoutBigQuad>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
