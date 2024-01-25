using System.Linq;
using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage14.Act1
{
    public enum OID : uint
    {
        Boss = 0x271D, //R=2.0
    };

    public enum AID : uint
    {
        TheLastSong = 14756, // 271D->self, 6,0s cast, range 60 circle
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
                hints.Add("Take cover behind a barricade!");
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("These slimes start casting Final Song after death.\nWhile Final Song is not deadly, it does heavy damage and applies silence\nto you. Take cover! For act 2 the spell Loom is strongly recommended.\nThe slimes are strong against blunt melee damage such as J Kick.");
        }
    }

    class Stage14Act1States : StateMachineBuilder
    {
        public Stage14Act1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .DeactivateOnEnter<Hints>()
                .ActivateOnEnter<LastSong>()
                .ActivateOnEnter<LastSongHint>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && !LastSongHint.casting;
        }
    }

    [ModuleInfo(CFCID = 624, NameID = 8108)]
    public class Stage14Act1 : BossModule
    {
        public Stage14Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
            ActivateComponent<Layout2Corners>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
