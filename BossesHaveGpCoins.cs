using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services;

namespace BossesHaveGpCoins;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.acidphantasm.bosseshavegpcoins";
    public override string Name { get; init; } = "Bosses Have GP Coins";
    public override string Author { get; init; } = "acidphantasm";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.10");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class BossesHaveGpCoins(
    DatabaseService databaseService,
    ModHelper modHelper)
    : IOnLoad
{
    private ModConfig? _modConfig;
    
    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        _modConfig = modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config.json");
        
        EditBots();
        
        return Task.CompletedTask;
    }
    
    private void EditBots()
    {
        var bots = databaseService.GetBots().Types;

        foreach (var (key, botType) in bots)
        {
            var botName = key.ToLowerInvariant();
            var isBoss = botName.Contains("boss") || _modConfig.IncludeFollowers && botName.Contains("follower");
            
            if (!isBoss) continue;
            var bossPockets = botType.BotInventory.Items.Pockets;
            var totalBossPocketValues = bossPockets.Sum( kvp => kvp.Value);

            double value = 0;
            double guess = 0;
            double rollChance = 0;

            guess = _modConfig.GpCoinChance / 100 * totalBossPocketValues;
            value = Math.Round((_modConfig.GpCoinChance / 100) * (totalBossPocketValues + guess));
            bossPockets.TryAdd(ItemTpl.MONEY_GP_COIN, value);
        }
    }
}

public class ModConfig
{
    public double GpCoinChance { get; set; }
    public bool IncludeFollowers { get; set; }
}
