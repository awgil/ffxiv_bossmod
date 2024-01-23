using System.Linq;
using BossMod.Components;

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
    class LastSongHint : CastHint
    {
        public LastSongHint() : base(ActionID.MakeSpell(AID.TheLastSong), "Use the cube to break line of sight!") { }
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
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && !module.Enemies(OID.Boss).Any(e => e.CastInfo!.IsSpell(AID.TheLastSong));
        }
    }

    public class Stage14Act2 : LayoutBigQuad
    {
        public Stage14Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
