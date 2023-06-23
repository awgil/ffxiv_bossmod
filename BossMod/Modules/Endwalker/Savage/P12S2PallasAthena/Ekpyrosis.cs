namespace BossMod.Endwalker.Savage.P12S2PallasAthena
{
    class EkpyrosisProximityV : Components.LocationTargetedAOEs
    {
        public EkpyrosisProximityV() : base(ActionID.MakeSpell(AID.EkpyrosisProximityV), 19) { } // TODO: verify falloff
    }

    class EkpyrosisProximityH : Components.SelfTargetedAOEs
    {
        public EkpyrosisProximityH() : base(ActionID.MakeSpell(AID.EkpyrosisProximityH), new AOEShapeCircle(19)) { } // TODO: verify falloff
    }

    class EkpyrosisExaflare : Components.Exaflare
    {
        public EkpyrosisExaflare() : base(6) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.EkpyrosisExaflareFirst)
            {
                Lines.Add(new() { Next = caster.Position, Advance = 8 * spell.Rotation.ToDirection(), NextExplosion = spell.FinishAt, TimeToMove = 2.1f, ExplosionsLeft = 5, MaxShownExplosions = 2 });
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.EkpyrosisExaflareFirst or AID.EkpyrosisExaflareRest)
            {
                ++NumCasts;
                int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
                if (index == -1)
                {
                    module.ReportError(this, $"Failed to find entry for {caster.InstanceID:X}");
                    return;
                }

                AdvanceLine(module, Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }

    class EkpyrosisSpread : Components.UniformStackSpread
    {
        public EkpyrosisSpread() : base(0, 6) { }

        public override void Init(BossModule module)
        {
            foreach (var p in module.Raid.WithoutSlot(true))
                AddSpread(p, module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.EkpyrosisSpread)
                Spreads.Clear();
        }
    }
}
