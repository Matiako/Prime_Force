using Godot;
using PrimeForce.Combat.Interfaces;
using PrimeForce.Combat.Systems;
using PrimeForce.Core.Interfaces;
using PrimeForce.Localization.Interfaces;
using PrimeForce.Localization.Providers;
using PrimeForce.MathEngine.Generators;
using PrimeForce.MathEngine.Interfaces;
using PrimeForce.SaveSystem.Interfaces;
using PrimeForce.SaveSystem.Serializers;

namespace PrimeForce.Core.Services;

/// <summary>
/// Godot Autoload — service registry and composition root.
/// All concrete class instantiations happen here and nowhere else.
///
/// Project → Autoload: path res://Scripts/Core/Services/GameServices.cs, name GameServices.
/// </summary>
public partial class GameServices : Node
{
    public static GameServices Instance { get; private set; } = null!;

    private readonly Dictionary<Type, object> _services = new();

    public override void _Ready()
    {
        Instance = this;
        Bootstrap();
    }

    public override void _ExitTree()
    {
        if (_services.TryGetValue(typeof(PlayerProgressionManager), out var mgr))
            ((PlayerProgressionManager)mgr).Dispose();
    }

    // ── Registry ─────────────────────────────────────────────────────────────

    public void Register<TInterface>(TInterface implementation) where TInterface : notnull
        => _services[typeof(TInterface)] = implementation;

    public TInterface Get<TInterface>() where TInterface : notnull
    {
        if (_services.TryGetValue(typeof(TInterface), out var service))
            return (TInterface)service;
        throw new InvalidOperationException(
            $"[GameServices] '{typeof(TInterface).Name}' is not registered.");
    }

    public bool IsRegistered<TInterface>() => _services.ContainsKey(typeof(TInterface));

    // ── Composition Root ──────────────────────────────────────────────────────

    private void Bootstrap()
    {
        var eventBus     = new SimpleEventBus();
        var addSubGen    = new AdditionSubtractionChallengeGenerator();
        var mulDivGen    = new MultiplicationDivisionChallengeGenerator();
        var localization = new GodotLocalizationProvider();
        var saveSystem   = new JsonSaveSystem();
        var progression  = new PlayerProgressionManager(saveSystem, eventBus);

        // Composite generator: Func<int> captures progression by closure — no direct coupling
        var levelAwareGen = new LevelAwareChallengeGenerator(
            lowLevelGenerator:  addSubGen,
            highLevelGenerator: mulDivGen,
            getCurrentLevel:    () => progression.Data.Level,
            threshold:          5);

        var calculator = new BasicCombatCalculator(levelAwareGen, eventBus);

        Register<IEventBus>(eventBus);
        Register<IMathChallengeGenerator>(levelAwareGen);
        Register<ICombatCalculator>(calculator);
        Register<IChallengeAnswerReceiver>(calculator);
        Register<ILocalizationProvider>(localization);
        Register<ISaveSystem>(saveSystem);
        Register<PlayerProgressionManager>(progression);
    }
}
