namespace BossMod;

// allowed categories of targets for an action
[Flags]
public enum ActionTargets
{
    None = 0,
    Self = 1 << 0,
    Party = 1 << 1,
    Alliance = 1 << 2,
    Hostile = 1 << 3,
    Friendly = 1 << 4,
    OwnPet = 1 << 5,
    PartyPet = 1 << 6,
    Area = 1 << 7,
    Dead = 1 << 8,

    All = (1 << 9) - 1,
}

// this contains all information about player actions that we care about (for action tweaks, autorotation, etc)
// some of the data is available in sheets, however unfortunately quite a bit is hardcoded in game functions; it often uses current player data
// however, we need this information outside game (ie in uidev) and for different players of different classes/levels (ie for replay analysis)
// because of that, we need to reimplement a lot of the logic here - this has to be synchronized to the code whenever game changes
public sealed record class ActionDefinition(ActionID ID)
{
    public delegate bool ConditionDelegate(WorldState ws, Actor player, Actor? target, AIHints hints);
    public delegate Actor? SmartTargetDelegate(WorldState ws, Actor player, Actor? target, AIHints hints);
    public delegate Angle? TransformAngleDelegate(WorldState ws, Actor player, Actor? target, AIHints hints);

    public BitMask AllowedClasses = new(~0ul);
    public int MinLevel;
    public uint UnlockLink;
    public ActionTargets AllowedTargets;
    public float Range; // 0 for self-targeted abilities
    public float CastTime; // 0 for instant-cast; can be adjusted by a number of factors (TODO: add functor)
    public int MainCooldownGroup = -1;
    public int ExtraCooldownGroup = -1;
    public float Cooldown; // for single charge (if multi-charge action); can be adjusted by a number of factors (TODO: add functor)
    public int MaxChargesAtCap = 1; // TODO: this actually depends on unlocked traits...
    public float InstantAnimLock = 0.6f; // animation lock if ability is instant-cast
    public float CastAnimLock = 0.1f; // animation lock if ability is non-instant
    public ConditionDelegate? Condition; // optional condition, if it returns false, action is not executed
    public SmartTargetDelegate? SmartTarget; // optional target transformation for 'smart targeting' feature
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
    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Action>? _actionsSheet = Service.LuminaSheet<Lumina.Excel.GeneratedSheets.Action>();
    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Item>? _itemsSheet = Service.LuminaSheet<Lumina.Excel.GeneratedSheets.Item>();
    private readonly Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.ClassJobCategory>? _cjcSheet = Service.LuminaSheet<Lumina.Excel.GeneratedSheets.ClassJobCategory>();
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
    public static readonly ActionID IDPotionStr = new(ActionType.Item, 1044157); // hq grade 1 gemdraught of strength
    public static readonly ActionID IDPotionDex = new(ActionType.Item, 1044158); // hq grade 1 gemdraught of dexterity
    public static readonly ActionID IDPotionVit = new(ActionType.Item, 1044159); // hq grade 1 gemdraught of vitality
    public static readonly ActionID IDPotionInt = new(ActionType.Item, 1044160); // hq grade 1 gemdraught of intelligence
    public static readonly ActionID IDPotionMnd = new(ActionType.Item, 1044161); // hq grade 1 gemdraught of mind

    // special general actions that we support
    public static readonly ActionID IDGeneralLimitBreak = new(ActionType.General, 3);
    public static readonly ActionID IDGeneralSprint = new(ActionType.General, 4);
    public static readonly ActionID IDGeneralDuty1 = new(ActionType.General, 26);
    public static readonly ActionID IDGeneralDuty2 = new(ActionType.General, 27);

    public static readonly ActionDefinitions Instance = new();

    public Func<uint, bool>? UnlockCheck;

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

    // smart targeting utility: return target (if friendly) or null (otherwise)
    public static Actor? SmartTargetFriendly(Actor? primaryTarget) => (primaryTarget?.IsAlly ?? false) ? primaryTarget : null;

    // smart targeting utility: return target (if friendly) or other tank (if available) or null (otherwise)
    public static Actor? FindCoTank(WorldState ws, Actor player) => ws.Party.WithoutSlot().Exclude(player).FirstOrDefault(a => a.Role == Role.Tank);
    public static Actor? SmartTargetCoTank(WorldState ws, Actor player, Actor? primaryTarget, AIHints hints) => SmartTargetFriendly(primaryTarget) ?? FindCoTank(ws, player);

    // smart targeting utility: return target (if friendly) or any esunable player (if any) or self (otherwise)
    public static Actor? FindEsunaTarget(WorldState ws) => ws.Party.WithoutSlot().FirstOrDefault(p => p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)));
    public static Actor? SmartTargetEsunable(WorldState ws, Actor player, Actor? primaryTarget, AIHints hints) => SmartTargetFriendly(primaryTarget) ?? FindEsunaTarget(ws) ?? player;

    // see ActionManager.GetSpellIdForAction
    public uint ActionSpellId(ActionID aid) => aid.Type switch
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

    public BitMask SpellAllowedClasses(Lumina.Excel.GeneratedSheets.Action? data)
    {
        BitMask res = default;
        var cjc = _cjcSheet?.GetRowParser(data?.ClassJobCategory.Row ?? 0);
        if (cjc != null)
            for (int i = 1; i < _cjcSheet!.ColumnCount; ++i)
                res[i - 1] = cjc.ReadColumn<bool>(i);
        return res;
    }
    public BitMask SpellAllowedClasses(uint spellId) => SpellAllowedClasses(ActionData(spellId));
    public BitMask ActionAllowedClasses(ActionID aid) => aid.Type == ActionType.Spell ? SpellAllowedClasses(aid.ID) : new(~0ul);

    public int SpellMinLevel(Lumina.Excel.GeneratedSheets.Action? data) => data?.ClassJobLevel ?? 0;
    public int SpellMinLevel(uint spellId) => SpellMinLevel(ActionData(spellId));
    public int ActionMinLevel(ActionID aid) => aid.Type == ActionType.Spell ? SpellMinLevel(aid.ID) : 0;

    public uint SpellUnlockLink(Lumina.Excel.GeneratedSheets.Action? data) => data?.UnlockLink ?? 0;
    public uint SpellUnlockLink(uint spellId) => SpellUnlockLink(ActionData(spellId));
    public uint ActionUnlockLink(ActionID aid) => aid.Type == ActionType.Spell ? SpellUnlockLink(aid.ID) : 0;

    // see ActionManager.CanUseActionOnTarget
    public ActionTargets SpellAllowedTargets(Lumina.Excel.GeneratedSheets.Action? data)
    {
        ActionTargets res = ActionTargets.None;
        if (data != null)
        {
            if (data.CanTargetSelf)
                res |= ActionTargets.Self;
            if (data.CanTargetParty)
                res |= ActionTargets.Party;
            if (data.CanTargetFriendly)
                res |= ActionTargets.Alliance;
            if (data.CanTargetHostile)
                res |= ActionTargets.Hostile;
            if (data.Unknown19)
                res |= ActionTargets.Friendly;
            if (data.Unknown22)
                res |= ActionTargets.OwnPet;
            if (data.Unknown23)
                res |= ActionTargets.PartyPet;
            if (data.TargetArea)
                res |= ActionTargets.Area;
            if (data.Unknown24 == 1)
                res |= ActionTargets.Dead;
        }
        return res;
    }
    public ActionTargets SpellAllowedTargets(uint spellId) => SpellAllowedTargets(ActionData(spellId));
    public ActionTargets ActionAllowedTargets(ActionID aid) => SpellAllowedTargets(ActionSpellId(aid));

    // see ActionManager.GetActionRange
    // note that actions with range == -1 use data from equipped weapon; currently all weapons for phys-ranged classes have range 25, others have range 3
    public int SpellRange(Lumina.Excel.GeneratedSheets.Action? data, bool isPhysRanged = false)
    {
        if ((SCH.AID)(data?.RowId ?? 0) is SCH.AID.PetEmbrace or SCH.AID.PetFeyUnion or SCH.AID.PetSeraphicVeil)
            return 30; // these are hardcoded
        var range = data?.Range ?? 0;
        return range >= 0 ? range : isPhysRanged ? 25 : 3;
    }
    public int SpellRange(uint spellId, bool isPhysRanged = false) => SpellRange(ActionData(spellId), isPhysRanged);
    public int ActionRange(ActionID aid, bool isPhysRanged = false) => SpellRange(ActionSpellId(aid), isPhysRanged);

    // see ActionManager.GetCastTimeAdjusted
    public float SpellBaseCastTime(Lumina.Excel.GeneratedSheets.Action? data) => (data?.Cast100ms ?? 0) * 0.1f;
    public float SpellBaseCastTime(uint spellId) => SpellBaseCastTime(ActionData(spellId));
    public float ActionBaseCastTime(ActionID aid) => aid.Type switch
    {
        ActionType.Item => ItemData(aid.ID)?.CastTimes ?? 2,
        ActionType.KeyItem => Service.LuminaRow<Lumina.Excel.GeneratedSheets.EventItem>(aid.ID)?.CastTime ?? 0,
        ActionType.Spell or ActionType.Mount or ActionType.Ornament => SpellBaseCastTime(ActionSpellId(aid)),
        _ => 0
    };

    public int SpellMainCDGroup(Lumina.Excel.GeneratedSheets.Action? data) => (data?.CooldownGroup ?? 0) - 1;
    public int SpellMainCDGroup(uint spellId) => SpellMainCDGroup(ActionData(spellId));
    public int ActionMainCDGroup(ActionID aid) => SpellMainCDGroup(ActionSpellId(aid));

    public int SpellExtraCDGroup(Lumina.Excel.GeneratedSheets.Action? data) => (data?.AdditionalCooldownGroup ?? 0) - 1;
    public int SpellExtraCDGroup(uint spellId) => SpellExtraCDGroup(ActionData(spellId));
    public int ActionExtraCDGroup(ActionID aid) => aid.Type == ActionType.Spell ? SpellExtraCDGroup(aid.ID) : -1; // see ActionManager.GetAdditionalRecastGroup - always -1 for non-spells

    // see ActionManager.GetAdjustedRecastTime
    public float SpellBaseCooldown(Lumina.Excel.GeneratedSheets.Action? data) => (data?.Recast100ms ?? 0) * 0.1f;
    public float SpellBaseCooldown(uint spellId) => SpellBaseCooldown(ActionData(spellId));
    public float ActionBaseCooldown(ActionID aid) => aid.Type switch
    {
        ActionType.Spell => SpellBaseCooldown(aid.ID),
        ActionType.Item => (ItemData(aid.ID)?.Cooldowns * (aid.ID > 1000000 ? 0.9f : 1.0f)) ?? 5,
        _ => 5,
    };

    private Lumina.Excel.GeneratedSheets.Action? ActionData(uint id) => _actionsSheet?.GetRow(id);
    private Lumina.Excel.GeneratedSheets.Item? ItemData(uint id) => _itemsSheet?.GetRow(id % 500000);

    // registration for different kinds of actions
    public void RegisterSpell(ActionID aid, bool isPhysRanged = false, int maxCharges = 1, float instantAnimLock = 0.6f, float castAnimLock = 0.1f)
    {
        var data = ActionData(aid.ID);
        var def = new ActionDefinition(aid)
        {
            AllowedClasses = SpellAllowedClasses(data),
            MinLevel = SpellMinLevel(data),
            UnlockLink = SpellUnlockLink(data),
            AllowedTargets = SpellAllowedTargets(data),
            Range = SpellRange(data, isPhysRanged),
            CastTime = SpellBaseCastTime(data),
            MainCooldownGroup = SpellMainCDGroup(data),
            ExtraCooldownGroup = SpellExtraCDGroup(data),
            Cooldown = SpellBaseCooldown(data),
            MaxChargesAtCap = maxCharges,
            InstantAnimLock = instantAnimLock,
            CastAnimLock = castAnimLock,
        };
        Register(aid, def);
    }

    public void RegisterSpell<AID>(AID aid, bool isPhysRanged = false, int maxCharges = 1, float instantAnimLock = 0.6f, float castAnimLock = 0.1f) where AID : Enum
        => RegisterSpell(ActionID.MakeSpell(aid), isPhysRanged, maxCharges, instantAnimLock, castAnimLock);

    private void Register(ActionID aid, ActionDefinition definition) => _definitions.Add(aid, definition);

    private void RegisterPotion(ActionID aid)
    {
        var baseId = aid.ID % 500000;
        var item = ItemData(baseId);
        var itemAction = item?.ItemAction.Value;
        var spellId = itemAction?.Type ?? 0;
        var cdgroup = SpellMainCDGroup(spellId);
        float cooldown = item?.Cooldowns ?? 0;
        var targets = SpellAllowedTargets(spellId);
        var range = SpellRange(spellId);
        var castTime = item?.CastTimes ?? 2;
        var aidNQ = new ActionID(ActionType.Item, baseId);
        _definitions[aidNQ] = new(aidNQ)
        {
            AllowedTargets = targets,
            Range = range,
            CastTime = castTime,
            MainCooldownGroup = cdgroup,
            Cooldown = cooldown,
            InstantAnimLock = 1.1f
        };
        var aidHQ = new ActionID(ActionType.Item, baseId + 1000000);
        _definitions[aidHQ] = new(aidHQ)
        {
            AllowedTargets = targets,
            Range = range,
            CastTime = castTime,
            MainCooldownGroup = cdgroup,
            Cooldown = cooldown * 0.9f,
            InstantAnimLock = 1.1f
        };
    }

    private void RegisterBozja(BozjaHolsterID id)
    {
        var normalAction = BozjaActionID.GetNormal(id);
        bool isItem = normalAction == BozjaActionID.GetHolster(id);
        RegisterSpell(normalAction, instantAnimLock: isItem ? 1.1f : 0.6f);
        if (!isItem)
        {
            var aid1 = ActionID.MakeBozjaHolster(id, 0);
            _definitions[aid1] = new(aid1) { AllowedTargets = ActionTargets.Self, InstantAnimLock = 2.1f };
            var aid2 = ActionID.MakeBozjaHolster(id, 1);
            _definitions[aid2] = new(aid2) { AllowedTargets = ActionTargets.Self, InstantAnimLock = 2.1f };
        }
    }
}
