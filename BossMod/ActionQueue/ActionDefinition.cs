namespace BossMod;

// this contains all information about player actions that we care about (for action tweaks, autorotation, etc)
// some of the data is available in sheets, however unfortunately quite a bit is hardcoded in game functions; it often uses current player data
// however, we need this information outside game (ie in uidev) and for different players of different classes/levels (ie for replay analysis)
// because of that, we need to reimplement a lot of the logic here - this has to be synchronized to the code whenever game changes
public sealed record class ActionDefinition(
    ActionID ID,
    float Range, // 0 for self-targeted abilities
    float CastTime = 0, // 0 for instant-cast; can be adjusted by a number of factors (TODO: add functor)
    int MainCooldownGroup = -1,
    int ExtraCooldownGroup = -1,
    float Cooldown = 0, // for single charge (if multi-charge action); can be adjusted by a number of factors (TODO: add functor)
    int MaxChargesAtCap = 1, // TODO: this actually depends on unlocked traits...
    float InstantAnimLock = 0.6f, // animation lock if ability is instant-cast
    float CastAnimLock = 0.1f) // animation lock if ability is non-instant
{
    public delegate bool ConditionDelegate(WorldState ws, Actor player, Actor? target, AIHints hints);
    public delegate Actor? TransformTargetDelegate(WorldState ws, Actor player, Actor? target, AIHints hints);
    public delegate Angle? TransformAngleDelegate(WorldState ws, Actor player, Actor? target, AIHints hints);

    public float EffectDuration; // used by planner UI; TODO: this can change depending on traits...
    public ConditionDelegate? Condition; // optional condition, if it returns false, action is not executed
    public TransformTargetDelegate? TransformTarget; // optional target transformation
    public TransformAngleDelegate? TransformAngle; // optional facing angle transformation

    public float CooldownAtFirstCharge => (MaxChargesAtCap - 1) * Cooldown;
    public bool IsGCD => MainCooldownGroup == ActionDefinitions.GCDGroup || ExtraCooldownGroup == ActionDefinitions.GCDGroup;

    // for multi-charge abilities, assume action is ready when elapsed >= single-charge cd
    // TODO: use adjusted cooldown
    public float MainReadyIn(ReadOnlySpan<Cooldown> cooldowns) => MainCooldownGroup >= 0 ? Math.Min(cooldowns[MainCooldownGroup].Total, Cooldown) - cooldowns[MainCooldownGroup].Elapsed : 0;
    public float ExtraReadyIn(ReadOnlySpan<Cooldown> cooldowns) => ExtraCooldownGroup >= 0 ? cooldowns[ExtraCooldownGroup].Remaining : 0;
    public float ReadyIn(ReadOnlySpan<Cooldown> cooldowns) => Math.Max(MainReadyIn(cooldowns), ExtraReadyIn(cooldowns));
}

// database of all supported player-initiated actions
// note that it is associated to a specific worldstate, so that it can be used for things like action conditions
public sealed class ActionDefinitions : IDisposable
{
    public static ActionDefinitions Instance = new();

    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Action>? _actionsSheet = Service.LuminaSheet<Lumina.Excel.GeneratedSheets.Action>();
    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Item>? _itemsSheet = Service.LuminaSheet<Lumina.Excel.GeneratedSheets.Item>();
    private readonly List<IDisposable> _classDefinitions;
    private readonly Dictionary<ActionID, ActionDefinition> _definitions = [];

    public IEnumerable<ActionDefinition> Definitions => _definitions.Values;
    public ActionDefinition? this[ActionID action] => _definitions.GetValueOrDefault(action);
    public ActionDefinition? Spell<AID>(AID aid) where AID : Enum => _definitions.GetValueOrDefault(ActionID.MakeSpell(aid));

    public const int GCDGroup = 57;
    public const int PotionCDGroup = 58;
    public const int DutyAction0CDGroup = 80;
    public const int DutyAction1CDGroup = 81;

    public static readonly ActionID IDSprint = new(ActionType.Spell, 3);
    public static readonly ActionID IDAutoAttack = new(ActionType.Spell, 7);
    public static readonly ActionID IDAutoShot = new(ActionType.Spell, 8);
    public static readonly ActionID IDPotionStr = new(ActionType.Item, 1039727); // hq grade 8 tincture of strength
    public static readonly ActionID IDPotionDex = new(ActionType.Item, 1039728); // hq grade 8 tincture of dexterity
    public static readonly ActionID IDPotionVit = new(ActionType.Item, 1039729); // hq grade 8 tincture of vitality
    public static readonly ActionID IDPotionInt = new(ActionType.Item, 1039730); // hq grade 8 tincture of intelligence
    public static readonly ActionID IDPotionMnd = new(ActionType.Item, 1039731); // hq grade 8 tincture of mind

    // special general actions that we support
    public static readonly ActionID IDGeneralLimitBreak = new(ActionType.General, 3);
    public static readonly ActionID IDGeneralSprint = new(ActionType.General, 4);
    public static readonly ActionID IDGeneralDuty1 = new(ActionType.General, 26);
    public static readonly ActionID IDGeneralDuty2 = new(ActionType.General, 27);

    private ActionDefinitions()
    {
        _classDefinitions = [
            new ClassShared.Definitions(this),
            new PLD.Definitions(this),
            new WAR.Definitions(this),
            new DRK.Definitions(this),
            new GNB.Definitions(this),
            new WHM.Definitions(this),
            new SCH.Definitions(this),
            new AST.Definitions(this),
            new SGE.Definitions(this),
            new MNK.Definitions(this),
            new DRG.Definitions(this),
            new NIN.Definitions(this),
            new SAM.Definitions(this),
            new RPR.Definitions(this),
            new BRD.Definitions(this),
            new MCH.Definitions(this),
            new DNC.Definitions(this),
            new BLM.Definitions(this),
            new SMN.Definitions(this),
            new RDM.Definitions(this),
            new BLU.Definitions(this),
        ];

        // items (TODO: more generic approach is needed...)
        RegisterPotion(IDPotionStr);
        RegisterPotion(IDPotionDex);
        RegisterPotion(IDPotionVit);
        RegisterPotion(IDPotionInt);
        RegisterPotion(IDPotionMnd);

        // bozja actions
        for (var i = BozjaHolsterID.None + 1; i < BozjaHolsterID.Count; ++i)
            RegisterBozja(i);
    }

    public void Dispose()
    {
        foreach (var c in _classDefinitions)
            c.Dispose();
    }

    public int MainCDGroupSpell(uint spellId) => (ActionData(spellId)?.CooldownGroup ?? 0) - 1;
    public int ExtraCDGroupSpell(uint spellId) => (ActionData(spellId)?.AdditionalCooldownGroup ?? 0) - 1;
    public int MainCDGroupAction(ActionID aid) => MainCDGroupSpell(GetSpellIdForAction(aid));
    public int ExtraCDGroupAction(ActionID aid) => aid.Type == ActionType.Spell ? ExtraCDGroupSpell(aid.ID) : -1; // see ActionManager.GetAdditionalRecastGroup - always -1 for non-spells

    // see ActionManager.GetSpellIdForAction
    public uint GetSpellIdForAction(ActionID aid) => aid.Type switch
    {
        ActionType.Spell => aid.ID,
        ActionType.Item => ItemData(aid.ID)?.ItemAction.Value?.Type ?? 0,
        ActionType.KeyItem => Service.LuminaRow<Lumina.Excel.GeneratedSheets.EventItem>(aid.ID)?.Action.Row ?? 0,
        ActionType.Ability => 2, // 'interaction'
        ActionType.General => Service.LuminaRow<Lumina.Excel.GeneratedSheets.GeneralAction>(aid.ID)?.Action.Row ?? 0, // note: duty action 1/2 (26/27) use special code
        ActionType.Mount => 4, // 'mount'
        ActionType.Ornament => 20061, // 'accessorize'
        _ => 0
    };

    // see ActionManager.GetActionRange
    // note that actions with range == -1 use data from equipped weapon; currently all weapons for phys-ranged classes have range 25, others have range 3
    public int GetSpellRange(uint spellId, bool isPhysRanged = false)
    {
        if ((SCH.AID)spellId is SCH.AID.PetEmbrace or SCH.AID.PetFeyUnion or SCH.AID.PetSeraphicVeil)
            return 30; // these are hardcoded
        var range = ActionData(spellId)?.Range ?? 0;
        return range >= 0 ? range : isPhysRanged ? 25 : 3;
    }
    public int GetActionRange(ActionID aid, bool isPhysRanged = false) => GetSpellRange(GetSpellIdForAction(aid), isPhysRanged);

    // see ActionManager.GetCastTimeAdjusted
    public float GetBaseSpellCastTime(uint spellId) => (ActionData(spellId)?.Cast100ms ?? 0) * 0.1f;
    public float GetBaseActionCastTime(ActionID aid) => aid.Type switch
    {
        ActionType.Item => ItemData(aid.ID)?.CastTimes ?? 2,
        ActionType.KeyItem => Service.LuminaRow<Lumina.Excel.GeneratedSheets.EventItem>(aid.ID)?.CastTime ?? 0,
        ActionType.Spell or ActionType.Mount or ActionType.Ornament => GetBaseSpellCastTime(GetSpellIdForAction(aid)),
        _ => 0
    };

    // see ActionManager.GetAdjustedRecastTime
    public float GetBaseSpellCooldown(uint spellId) => (ActionData(spellId)?.Recast100ms ?? 0) * 0.1f;
    public float GetBaseActionCooldown(ActionID aid) => aid.Type switch
    {
        ActionType.Spell => GetBaseSpellCooldown(aid.ID),
        ActionType.Item => (ItemData(aid.ID)?.Cooldowns * (aid.ID > 1000000 ? 0.9f : 1.0f)) ?? 5,
        _ => 5,
    };

    private Lumina.Excel.GeneratedSheets.Action? ActionData(uint id) => _actionsSheet?.GetRow(id);
    private Lumina.Excel.GeneratedSheets.Item? ItemData(uint id) => _itemsSheet?.GetRow(id % 500000);

    // registration for different kinds of actions
    private void Register(ActionID aid, ActionDefinition definition) => _definitions.Add(aid, definition);

    public void RegisterSpell(ActionID aid, bool isPhysRanged = false, int maxCharges = 1, float instantAnimLock = 0.6f, float castAnimLock = 0.1f)
        => Register(aid, new(aid, GetSpellRange(aid.ID, isPhysRanged), GetBaseSpellCastTime(aid.ID), MainCDGroupSpell(aid.ID), ExtraCDGroupSpell(aid.ID), GetBaseSpellCooldown(aid.ID), maxCharges, instantAnimLock, castAnimLock));
    public void RegisterSpell<AID>(AID aid, bool isPhysRanged = false, int maxCharges = 1, float instantAnimLock = 0.6f, float castAnimLock = 0.1f) where AID : Enum
        => RegisterSpell(ActionID.MakeSpell(aid), isPhysRanged, maxCharges, instantAnimLock, castAnimLock);

    private void RegisterPotion(ActionID aid)
    {
        var baseId = aid.ID % 500000;
        var item = ItemData(baseId);
        var itemAction = item?.ItemAction.Value;
        var spellId = itemAction?.Type ?? 0;
        var cdgroup = MainCDGroupSpell(spellId);
        float cooldown = item?.Cooldowns ?? 0;
        var range = GetSpellRange(spellId);
        var castTime = item?.CastTimes ?? 2;
        var aidNQ = new ActionID(ActionType.Item, baseId);
        _definitions[aidNQ] = new(aidNQ, range, castTime, cdgroup, -1, cooldown, InstantAnimLock: 1.1f) { EffectDuration = itemAction?.Data[2] ?? 0 };
        var aidHQ = new ActionID(ActionType.Item, baseId + 1000000);
        _definitions[aidHQ] = new(aidHQ, range, castTime, cdgroup, -1, cooldown * 0.9f, InstantAnimLock: 1.1f) { EffectDuration = itemAction?.DataHQ[2] ?? 0 };
    }

    private void RegisterBozja(BozjaHolsterID id)
    {
        var normalAction = BozjaActionID.GetNormal(id);
        bool isItem = normalAction == BozjaActionID.GetHolster(id);
        RegisterSpell(normalAction, instantAnimLock: isItem ? 1.1f : 0.6f);
        if (!isItem)
        {
            var aid1 = ActionID.MakeBozjaHolster(id, 0);
            _definitions[aid1] = new(aid1, 0, InstantAnimLock: 2.1f);
            var aid2 = ActionID.MakeBozjaHolster(id, 1);
            _definitions[aid2] = new(aid2, 0, InstantAnimLock: 2.1f);
        }
    }
}
