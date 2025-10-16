using System;
using MetroManager.Domain.Entities;

namespace MetroManager.Application.Factories
{
    public static class IssueFactory
    {
        public static Issue CreateNew(
            string category,
            string? subcategory,
            string description,
            string locationText,
            string? locationLink,
            double? latitude,
            double? longitude,
            bool isAnonymous,
            string? createdByUserIdOrAnonymousId)
        {
            return new Issue
            {
                PublicId = $"ISS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
                Category = category,
                Subcategory = subcategory,
                Description = description,
                LocationText = locationText,
                LocationLink = locationLink,
                Latitude = latitude,
                Longitude = longitude,
                IsAnonymous = isAnonymous,
                CreatedByUserId = createdByUserIdOrAnonymousId,
                CreatedUtc = DateTime.UtcNow
            };
        }
    }
}
