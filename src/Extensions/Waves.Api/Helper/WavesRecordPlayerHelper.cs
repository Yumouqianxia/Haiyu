using System.Collections.ObjectModel;
using Waves.Api.Helper;
using Waves.Api.Models.Enums;
using Waves.Api.Models.Record;
using Waves.Api.Models.Wrappers;

public static class WavesRecordPlayerHelper
{
    static Dictionary<CardPoolType, (string, bool)> PoolTypeNames { get; } =
        new()
        {
            { CardPoolType.RoleActivity, ("角色活动", true) },
            { CardPoolType.WeaponsActivity, ("武器活动", false) },
            { CardPoolType.RoleResident, ("角色常驻", false) },
            { CardPoolType.WeaponsResident, ("武器常驻", false) },
            { CardPoolType.Beginner, ("新手换取", false) },
            { CardPoolType.BeginnerChoice, ("新手自选", false) },
            { CardPoolType.GratitudeOrientation, ("感恩定向", false) },
            { CardPoolType.CharacterNovice, ("角色新旅", false) },
            { CardPoolType.WeaponNovice, ("武器新旅", false) },
            { CardPoolType.CharacterCollaboration, ("角色联动", true) },
            { CardPoolType.WeaponCollaboration, ("武器联动", false) },
        };

    extension(WavesAnalysisPlayerCardItem analysisItem)
    {
        public GameRecordNavigationItem GetRecordNavItem()
        {
            return new GameRecordNavigationItem()
            {
                DisplayName = PoolTypeNames
                    .GetValueOrDefault((CardPoolType)analysisItem.PoolType, ("未知", false))
                    .Item1,
                Id = analysisItem.PoolType,
            };
        }

        /// <summary>
        /// 有小保底的池子
        /// </summary>
        /// <returns></returns>
        public bool IsFlage()
        {
            return PoolTypeNames
                    .GetValueOrDefault((CardPoolType) analysisItem.PoolType, ("未知", false)).Item2;
        }
    }

    extension(WavesAnalysisPlayerCard card)
    {
        public (string Title, double Score) EvaluateLuck(List<int> upRoleIds)
        {
            var rates = new List<double>();
            var allResources = new List<RecordCardItemWrapper>();

            foreach (var item in card.Items)
            {
                var resources = item.Resource?.ToList() ?? [];
                allResources.AddRange(resources);

                if (item.IsFlage() && resources.Count > 0)
                {
                    var formatted = RecordHelper.FormatStartFive(resources, out _, upRoleIds);
                    if (formatted.Item1?.Count > 0)
                    {
                        rates.Add(RecordHelper.GetGuaranteedRange(formatted.Item1));
                    }
                }
            }

            double guaranteeScore;
            if (rates.Count > 0)
            {
                guaranteeScore = (1 - rates.Average() / 100) * 100;
            }
            else
            {
                guaranteeScore = 50;
            }

            var fiveStars = RecordHelper.FormatRecordFive(allResources);
            double efficiencyScore;
            if (fiveStars.Count > 0)
            {
                var avgPulls = fiveStars.CalculateAvg();
                efficiencyScore = Math.Max(0, (80 - avgPulls) / 40 * 100);
            }
            else
            {
                efficiencyScore = 50;
            }

            var totalPullsScore = Math.Min(allResources.Count / 1000.0, 1.0) * 100;
            var rawScore = guaranteeScore * 0.25 + efficiencyScore * 0.6 + totalPullsScore * 0.15;
            rawScore = Math.Clamp(rawScore, 0, 100);

            var title = rawScore switch
            {
                < 20 => "大非酋",
                < 40 => "非酋",
                < 60 => "平民",
                < 80 => "小欧皇",
                _ => "至尊无敌欧皇"
            };

            return (title, Math.Round(rawScore, 1));
        }
    }
}
