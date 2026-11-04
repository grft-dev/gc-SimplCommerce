using System;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Graftcode;
using SimplCommerce.Module.Core.Graftcode;
using SimplCommerce.Module.Core.Models;

namespace SimplCommerce.Module.Core.Areas.Core.Controllers
{
    public class DistrictApiController
    {
        public static string[] ListDistricts(string stateOrProvinceId) =>
            GraftAsync.Run(async () =>
            {
                var id = ParseId(stateOrProvinceId, nameof(stateOrProvinceId));
                await using var context = GraftCoreDb.CreateContext();
                var districts = await context.Set<District>()
                    .AsNoTracking()
                    .Where(x => x.StateOrProvinceId == id)
                    .OrderBy(x => x.Name)
                    .ToListAsync();
                return districts.Select(FormatDistrict).ToArray();
            });

        public static string GetDistrictCard(string districtId) =>
            GraftAsync.Run(async () =>
            {
                var id = ParseId(districtId, nameof(districtId));
                await using var context = GraftCoreDb.CreateContext();
                var district = await context.Set<District>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
                return district == null ? string.Empty : FormatDistrict(district);
            });

        public static string CreateDistrict(
            string stateOrProvinceId,
            string name,
            string type,
            string location) =>
            GraftAsync.Run(async () =>
            {
                var id = ParseId(stateOrProvinceId, nameof(stateOrProvinceId));
                Require(name, nameof(name));

                await using var context = GraftCoreDb.CreateContext();
                var stateExists = await context.Set<StateOrProvince>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == id);
                if (!stateExists)
                {
                    throw new InvalidOperationException($"State or province '{stateOrProvinceId}' was not found.");
                }

                var district = new District
                {
                    StateOrProvinceId = id,
                    Name = name,
                    Type = type,
                    Location = location
                };

                context.Set<District>().Add(district);
                await context.SaveChangesAsync();
                return FormatDistrict(district);
            });

        private static string FormatDistrict(District district) =>
            string.Join('|',
                district.Id.ToString(CultureInfo.InvariantCulture),
                district.StateOrProvinceId.ToString(CultureInfo.InvariantCulture),
                district.Name ?? string.Empty,
                district.Type ?? string.Empty,
                district.Location ?? string.Empty);

        private static long ParseId(string value, string name)
        {
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                throw new InvalidOperationException($"'{name}' must be a valid integer.");
            }

            return parsed;
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"'{name}' is required.");
            }
        }
    }
}
