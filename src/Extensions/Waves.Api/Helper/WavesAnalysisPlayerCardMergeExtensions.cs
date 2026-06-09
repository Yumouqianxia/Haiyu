using Waves.Api.Models.Record;
using Waves.Api.Models.Wrappers;

namespace Waves.Api.Helper;

public static class WavesAnalysisPlayerCardMergeExtensions
{
    extension(WavesAnalysisPlayerCardItem item)
    {
        /// <summary>
        /// 合并去重
        /// </summary>
        /// <param name="existing"></param>
        /// <returns></returns>
        public WavesAnalysisPlayerCardItem Merge(WavesAnalysisPlayerCardItem? existing)
        {
            if (existing == null)
                return item;

            var newResources = item.Resource?.ToList() ?? [];
            var existingResources = existing.Resource?.ToList() ?? [];

            var merged = RecordHelper.SyncCompareLists(newResources, existingResources);

            return new WavesAnalysisPlayerCardItem
            {
                PoolType = item.PoolType,
                Resource = merged.OrderByDescending(x => x.RecordTime),
            };
        }
    }
}
