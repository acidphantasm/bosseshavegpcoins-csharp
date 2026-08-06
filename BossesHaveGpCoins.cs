namespace BossesHaveGpCoins;

using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class BossesHaveGpCoins(
    BotTable botTable,
    BossesHaveGpCoinsConfig config)
    : IOnLoad
{
    
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        EditBots();
        
        return Task.CompletedTask;
    }
    
    private void EditBots()
    {
        var bots = botTable.Types;

        foreach (var (key, botType) in bots)
        {
            var botName = key.ToLowerInvariant();
            var isBoss = botName.Contains("boss") || config.IncludeFollowers && botName.Contains("follower");
            
            if (!isBoss) continue;
            var bossPockets = botType.BotInventory.Items.Pockets;
            var totalBossPocketValues = bossPockets.Sum( kvp => kvp.Value);

            double value = 0;
            double guess = 0;
            double rollChance = 0;

            guess = config.GpCoinChance / 100 * totalBossPocketValues;
            value = Math.Round((config.GpCoinChance / 100) * (totalBossPocketValues + guess));
            bossPockets.TryAdd(ItemTpl.MONEY_GP_COIN, value);
        }
    }
}


