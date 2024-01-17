using BossMod.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

    class Enaero : CastHint
    {
        private bool EnaeroBuff;
        private bool casting;
        public Enaero() : base(ActionID.MakeSpell(AID.RagingWinds1), "") { }
        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var boss = module.Enemies(OID.Boss).FirstOrDefault();  
            if (actor == boss)
            {if ((SID)status.ID == SID.Enaero)
                {
                    EnaeroBuff = true;
                }
            }
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.RagingWinds1)
                casting = true;
        }
        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.RagingWinds1)
                casting = false;
        }
        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            var boss = module.Enemies(OID.Boss).FirstOrDefault();  
            if (actor == boss)
            {
                if ((SID)status.ID == SID.Enaero)
                EnaeroBuff = false;
            }
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (casting == true)
            hints.Add("Applies Enaero to Lyon. Use Dispell to remove it");    
            if (EnaeroBuff == true)
            hints.Add("Enaero on Lyon. Use Dispell to remove it! You only need to do this once per duel, so you can switch to a different action after removing his buff.");
        }
    }
class HeartOfNatureConcentric : ConcentricAOEs
    {
        private static AOEShape[] _shapes = {new AOEShapeCircle(10), new AOEShapeDonut(10,20), new AOEShapeDonut(20,30)};

        public HeartOfNatureConcentric() : base(_shapes) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.NaturesPulse1)
                AddSequence(caster.Position, spell.FinishAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.NaturesPulse1 => 0,
                AID.NaturesPulse2 => 1,
                AID.NaturesPulse3 => 2,
                _ => -1
            };
            AdvanceSequence(order, caster.Position);
        }
    }
class TasteOfBlood : SelfTargetedAOEs
    {
        public TasteOfBlood() : base(ActionID.MakeSpell(AID.TasteOfBlood), new AOEShapeCone(40,90.Degrees())) { } 
    }
class RavenousGale : GenericAOEs
    {
        private bool activeTwister; 
        private bool casting;
        private static readonly AOEShapeCircle circle = new(0.5f);
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            var player = module.Raid.Player();
            if (casting && player != null)
                yield return new(circle, player.Position, player.Rotation, new());
            if (activeTwister)
                foreach (var p in module.Enemies(OID.RavenousGaleVoidzone))
                    yield return new(circle, p.Position, p.Rotation, new());
        }
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.RavenousGaleVoidzone)
                activeTwister = true;
                casting = false;
        }
        public override void OnActorDestroyed(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.RavenousGaleVoidzone)
                activeTwister = false;
                casting = false;
        }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.RavenousGale)
                casting = true;
        }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (casting)
                  hints.Add("Move a little to avoid voidzone spawning under you");
        }
    }
class TwinAgonies : SingleTargetCast
    {
        public TwinAgonies() : base(ActionID.MakeSpell(AID.TwinAgonies), "Heavy Tankbuster, use Manawall or tank mitigations") { }
    }
class WindsPeak : SelfTargetedAOEs
    {
        public WindsPeak() : base(ActionID.MakeSpell(AID.WindsPeak1), new AOEShapeCircle(5)) { } 
    }
class WindsPeakKB : KnockbackFromCastTarget //actual knockback is delayed between 0.4s to 0.6s, but 4s warning should be enough time to either move to safety or use anti-knockback ability
    {
        public WindsPeakKB() : base(ActionID.MakeSpell(AID.WindsPeak2), 15) { }
    }
class TheKingsNotice : CastGaze
    {
        public TheKingsNotice() : base(ActionID.MakeSpell(AID.TheKingsNotice)) { }
    }
class SplittingRage : CastHint
    {
        public SplittingRage() : base(ActionID.MakeSpell(AID.SplittingRage), "Applies temporary misdirection") { }
    }
 class NaturesBlood : Exaflare
    {
        public NaturesBlood() : base(7) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.NaturesBlood1)
            {
                Lines.Add(new() { Next = caster.Position, Advance = 6 * spell.Rotation.ToDirection(), NextExplosion = spell.FinishAt, TimeToMove = 1.1f, ExplosionsLeft = 8, MaxShownExplosions = 4 });
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NaturesBlood1 or AID.NaturesBlood2)
            {
                var pos = (AID)spell.Action.ID == AID.NaturesBlood1 ? caster.Position : spell.TargetXZ;
                int index = Lines.FindIndex(item => item.Next.AlmostEqual(pos, 1));
                if (index == -1)
                {
                    module.ReportError(this, $"Failed to find entry for {caster.InstanceID:X}");
                    return;
                }

                AdvanceLine(module, Lines[index], pos);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
class SkyrendingStrike : CastHint
    {
        private bool casting;
        private DateTime enragestart;
        public SkyrendingStrike() : base(ActionID.MakeSpell(AID.SkyrendingStrike), "") { }
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.SkyrendingStrike)
                {   
                    casting = true;       
                    enragestart = module.WorldState.CurrentTime;
                }
        }
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (casting)
                hints.Add($"Enrage! {Math.Max(35 - (module.WorldState.CurrentTime - enragestart).TotalSeconds, 0.0f):f1}s left.");
        }
    }