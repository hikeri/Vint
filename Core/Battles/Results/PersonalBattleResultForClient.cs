using Vint.Core.Battles.Player;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Battle.Rewards;
using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Results;

public class PersonalBattleResultForClient {
    public PersonalBattleResultForClient(IPlayerConnection connection, IEntity prevLeague, double reputationDelta) {
        Database.Models.Player player = connection.Player;
        Preset preset = player.CurrentPreset;
        BattlePlayer battlePlayer = connection.BattlePlayer!;
        IEntity userHull = preset.Hull.GetUserEntity(connection);
        IEntity userWeapon = preset.Weapon.GetUserEntity(connection);

        int battleScore = battlePlayer.GetBattleUserScoreWithBonus();
        float battleSeriesMultiplier = battlePlayer.GetBattleSeriesMultiplier();
        using DbConnection db = new();

        TankInitExp = (int)userHull.GetComponent<ExperienceItemComponent>().Experience - battleScore;
        TankFinalExp = userHull.GetComponent<ExperienceToLevelUpItemComponent>().FinalLevelExperience;
        TankExp = (int)db.Hulls
            .Where(hull => hull.PlayerId == player.Id && hull.Id == preset.Hull.Id)
            .Select(hull => hull.Xp)
            .Single();
        TankLevel = Leveling.GetLevel(TankExp);

        WeaponInitExp = (int)userWeapon.GetComponent<ExperienceItemComponent>().Experience - battleScore;
        WeaponFinalExp = userWeapon.GetComponent<ExperienceToLevelUpItemComponent>().FinalLevelExperience;
        WeaponExp = (int)db.Weapons
            .Where(weapon => weapon.PlayerId == player.Id && weapon.Id == preset.Weapon.Id)
            .Select(weapon => weapon.Xp)
            .Single();
        WeaponLevel = Leveling.GetLevel(WeaponExp);

        RankExp = (int)player.Experience;
        RankExpDelta = battleScore;

        ReputationDelta = reputationDelta;
        Reputation = player.Reputation;
        PrevLeague = prevLeague;
        League = player.LeagueEntity;
        LeaguePlace = Leveling.GetSeasonPlace(player.Id);

        Container = League.GetComponent<ChestBattleRewardComponent>().Chest;
        ContainerScoreDelta = battleScore;
        ContainerScore = (int)player.GameplayChestScore;
        ContainerScoreMultiplier = battleSeriesMultiplier;

        ItemsExpDelta = battleScore;
        Reward = Leveling.GetLevelUpRewards(connection);

        UserTeamColor = connection.User.GetComponent<TeamColorComponent>().TeamColor;
        TeamBattleResult = battlePlayer.TeamBattleResult;

        CurrentBattleSeries = connection.BattleSeries;
        ScoreBattleSeriesMultiplier = battleSeriesMultiplier;
    }

    public int CurrentBattleSeries { get; set; }
    public int MaxBattleSeries => 5;
    public int RankExp { get; set; }
    public int RankExpDelta { get; set; }
    public int WeaponExp { get; set; }
    public int TankLevel { get; set; }
    public int WeaponLevel { get; set; }
    public int WeaponInitExp { get; set; }
    public int WeaponFinalExp { get; set; }
    public int TankExp { get; set; }
    public int TankInitExp { get; set; }
    public int TankFinalExp { get; set; }
    public int ItemsExpDelta { get; set; }
    public int ContainerScore { get; set; }
    public int ContainerScoreDelta { get; set; }
    public int ContainerScoreLimit => 1000;
    public int LeaguePlace { get; set; }
    public double Reputation { get; set; }
    public double ReputationDelta { get; set; }
    public float ContainerScoreMultiplier { get; set; }
    public float ScoreBattleSeriesMultiplier { get; set; }
    public IEntity Container { get; set; }
    public IEntity League { get; set; }
    public IEntity PrevLeague { get; set; }
    public IEntity? Reward { get; set; }
    public TeamColor UserTeamColor { get; set; }
    public TeamBattleResult TeamBattleResult { get; set; }

    /*Energy is not used in the game anymore*/
    public int Energy => 0;
    public int EnergyDelta => 0;
    public int CrystalsForExtraEnergy => 0;
    public EnergySource? MaxEnergySource => null;
}