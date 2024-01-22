using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

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
    class LastSongHint : CastHint
    {
        public LastSongHint() : base(ActionID.MakeSpell(AID.TheLastSong), "Take cover behind a barricade!") { }
    }
    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("These slimes start casting Final Song after death.\nWhile FInal Song is not deadly, it does heavy damage and applies silence\nto you. Take cover! For act 2 the spell Loom is strongly recommended.\nThe slimes are strong against blunt melee damage such as J Kick.");
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
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && !module.Enemies(OID.Boss).Any(e => e.CastInfo!.IsSpell(AID.TheLastSong));
        }
    }

    public class Stage14Act1 : BossModule
    {
        public static IEnumerable<WPos> Wall1()
        {
            yield return new WPos(85,95);
            yield return new WPos(95,95);
            yield return new WPos(95,89);
            yield return new WPos(94.5f,89);
            yield return new WPos(94.5f,94.5f);
            yield return new WPos(85,94.5f);
        }
        public static IEnumerable<WPos> Wall2()
        {
            yield return new WPos(105,95);
            yield return new WPos(115,95);
            yield return new WPos(115,94.5f);
            yield return new WPos(105.5f,94.5f);
            yield return new WPos(105.5f,89);
            yield return new WPos(105,89);
        }
        public Stage14Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override void DrawArenaForeground(int pcSlot, Actor pc)
        {
            Arena.AddPolygon(Wall1(),ArenaColor.Border);
            Arena.AddPolygon(Wall2(),ArenaColor.Border);
        }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
