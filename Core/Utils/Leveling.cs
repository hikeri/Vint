﻿using LinqToDB;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Experience;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.Server.Experience;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.ECS.Templates.Battle.Reward;
using Vint.Core.ECS.Templates.Graffiti;
using Vint.Core.ECS.Templates.Hulls;
using Vint.Core.ECS.Templates.Skins;
using Vint.Core.ECS.Templates.Weapons.Market;
using Vint.Core.Server;

namespace Vint.Core.Utils;

public static class Leveling {
    public static int GetRank(long xp) {
        List<int> xpPerRank = ConfigManager
            .GetComponent<RanksExperiencesConfigComponent>("ranksconfig")
            .RanksExperiences
            .Prepend(0)
            .OrderBy(x => x)
            .ToList();

        return Math.Max(xpPerRank.IndexOf(xpPerRank.LastOrDefault(x => x <= xp)) + 1, 1);
    }

    public static int GetLevel(long xp) {
        List<int> experiencePerLevel = ConfigManager
            .GetComponent<UpgradeLevelsComponent>("garage")
            .LevelsExperiences
            .Prepend(0)
            .OrderBy(x => x)
            .ToList();

        int levelIndex = experiencePerLevel.IndexOf(experiencePerLevel.LastOrDefault(x => x <= xp));
        return Math.Max(levelIndex + 1, 0);
    }

    public static int GetSeasonPlace(long userId) {
        using DbConnection db = new();

        return db.SeasonStatistics
                   .Select(seasonStats => new { Id = seasonStats.PlayerId, seasonStats.Reputation })
                   .OrderByDescending(p => p.Reputation)
                   .ToList()
                   .Select((player, index) => new { player.Id, Index = index })
                   .Single(p => p.Id == userId)
                   .Index +
               1;
    }

    public static IEntity? GetLevelUpRewards(IPlayerConnection connection) {
        using DbConnection db = new();

        List<IEntity> rewards = [];
        List<IEntity> entities = connection.SharedEntities.ToList();
        Player player = connection.Player;

        var hulls = db.Hulls.Where(hull => hull.PlayerId == player.Id).Select(hull => new { hull.Id, hull.Xp }).ToList();
        var weapons = db.Weapons.Where(weapon => weapon.PlayerId == player.Id).Select(weapon => new { weapon.Id, weapon.Xp }).ToList();

        foreach (IEntity child in entities.Where(entity => entity.TemplateAccessor?.Template
                                                               is ChildGraffitiMarketItemTemplate
                                                               or HullSkinMarketItemTemplate
                                                               or WeaponSkinMarketItemTemplate)) {
            if (connection.OwnsItem(child)) continue;

            int rewardLevel = ConfigManager.GetComponent<MountUpgradeLevelRestrictionComponent>(child.TemplateAccessor!.ConfigPath!).RestrictionValue;

            if (rewardLevel == 0) continue;

            long parentId = child.GetComponent<ParentGroupComponent>().Key;
            long parentXp = hulls.SingleOrDefault(hull => hull.Id == parentId)?.Xp ??
                            weapons.SingleOrDefault(weapon => weapon.Id == parentId)?.Xp ?? 0;

            if (parentXp == 0) continue;

            int parentLevel = GetLevel(parentXp);

            if (parentLevel < rewardLevel) continue;

            connection.PurchaseItem(child, 1, 0, false, false);
            rewards.Add(child);
        }

        return rewards.Count == 0 ? null : new LevelUpUnlockBattleRewardTemplate().Create(rewards);
    }

    public static void UpdateItemXp(IEntity marketItem, IPlayerConnection connection, long delta) {
        IEntity userItem = marketItem.GetUserEntity(connection);

        if (!userItem.HasComponent<UserGroupComponent>()) return;

        EntityTemplate? template = marketItem.TemplateAccessor?.Template;
        long playerId = userItem.GetComponent<UserGroupComponent>().Key;
        long xp = 0;

        using (DbConnection db = new())
            switch (template) {
                case TankMarketItemTemplate:
                    db.Hulls
                        .Where(hull => hull.PlayerId == playerId && hull.Id == marketItem.Id)
                        .Set(hull => hull.Xp, hull => hull.Xp + delta)
                        .Update();
                    break;

                case WeaponMarketItemTemplate:
                    db.Weapons
                        .Where(weapon => weapon.PlayerId == playerId && weapon.Id == marketItem.Id)
                        .Set(weapon => weapon.Xp, weapon => weapon.Xp + delta)
                        .Update();
                    break;
            }

        userItem.ChangeComponent<ExperienceItemComponent>(component => xp = component.Experience += delta);
        userItem.RemoveComponent<ExperienceToLevelUpItemComponent>();
        userItem.RemoveComponent<UpgradeLevelItemComponent>();
        userItem.AddComponent(new ExperienceToLevelUpItemComponent(xp));
        userItem.AddComponent(new UpgradeLevelItemComponent(xp));
    }

    public static Dictionary<IEntity, int> GetFirstLeagueEntranceReward(League league) => league switch {
        League.Training => new Dictionary<IEntity, int> { { GlobalEntities.GetEntity("containers", "Cardsbronze"), 1 } },
        League.Bronze => new Dictionary<IEntity, int> { { GlobalEntities.GetEntity("containers", "Cardsbronze"), 5 } },
        League.Silver => new Dictionary<IEntity, int> { { GlobalEntities.GetEntity("containers", "Cardssilver"), 5 } },
        League.Gold => new Dictionary<IEntity, int> { { GlobalEntities.GetEntity("containers", "Cardsgold"), 5 } },
        League.Master => new Dictionary<IEntity, int> { { GlobalEntities.GetEntity("containers", "Cardsmaster"), 5 } },
        _ => new Dictionary<IEntity, int>()
    };
}