namespace GithubPortfolio.CacheServices
{
    using System;
    using System.Collections.Generic;
    using GithubService; 

    public class CachedPortfolio
    {
        // רשימת נתוני הפורטפוליו
        public IEnumerable<PortfolioRepositoryDto> Repositories { get; set; } = new List<PortfolioRepositoryDto>();

        // חותמת הזמן שבה נשלף המידע בהצלחה בפעם האחרונה
        public DateTimeOffset LastFetchedTime { get; set; } = DateTimeOffset.MinValue;
    }
}
