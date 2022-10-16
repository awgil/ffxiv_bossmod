namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    // component dealing with tetra/octaflare mechanics (conceptual or not)
    class TetraOctaFlareCommon : Components.StackSpread
    {
        public enum Concept { None, Tetra, Octa }

        public TetraOctaFlareCommon() : base(3, 6, 2, 2) { }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.EmergentOctaflare or AID.EmergentTetraflare)
                StackMask = SpreadMask = new();
        }

        protected void SetupMasks(BossModule module, Concept concept)
        {
            switch (concept)
            {
                case Concept.Tetra:
                    // note that targets are either all dps or all tanks/healers, it seems to be unknown until actual cast, so for simplicity assume it will target tanks/healers (not that it matters much in practice)
                    StackMask = module.Raid.WithSlot().WhereActor(a => a.Role is Role.Tank or Role.Healer).Mask();
                    break;
                case Concept.Octa:
                    SpreadMask = module.Raid.WithSlot().Mask();
                    break;
            }
        }
    }

    class TetraOctaFlareImmediate : TetraOctaFlareCommon
    {
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Octaflare:
                    SetupMasks(module, Concept.Octa);
                    break;
                case AID.Tetraflare:
                    SetupMasks(module, Concept.Tetra);
                    break;
            }
        }
    }

    class TetraOctaFlareConceptual : TetraOctaFlareCommon
    {
        private Concept _concept;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_concept != Concept.None)
                hints.Add(_concept == Concept.Tetra ? "Prepare to stack in pairs" : "Prepare to spread");
        }

        public void Show(BossModule module)
        {
            SetupMasks(module, _concept);
            _concept = Concept.None;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ConceptualOctaflare:
                    _concept = Concept.Octa;
                    break;
                case AID.ConceptualTetraflare:
                    _concept = Concept.Tetra;
                    break;
            }
        }
    }
}
