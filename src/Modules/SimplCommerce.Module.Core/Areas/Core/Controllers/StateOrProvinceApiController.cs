using System;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Graftcode;
using SimplCommerce.Module.Core.Graftcode;
using SimplCommerce.Module.Core.Models;

namespace SimplCommerce.Module.Core.Areas.Core.Controllers
{
    public class StateOrProvinceApiController
    {
        public static string[] ListAllStatesOrProvinces() =>
            GraftAsync.Run(async () =>
            {
                await using var context = GraftCoreDb.CreateContext();
                var statesOrProvinces = await context.Set<StateOrProvince>()
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .ToListAsync();
                return statesOrProvinces.Select(FormatStateOrProvince).ToArray();
            });

        public static string[] ListStatesOrProvinces(string countryId) =>
            GraftAsync.Run(async () =>
            {
                Require(countryId, nameof(countryId));
                await using var context = GraftCoreDb.CreateContext();
                var statesOrProvinces = await context.Set<StateOrProvince>()
                    .AsNoTracking()
                    .Where(x => x.CountryId == countryId)
                    .OrderBy(x => x.Name)
                    .ToListAsync();
                return statesOrProvinces.Select(FormatStateOrProvince).ToArray();
            });

        public static string GetStateOrProvinceCard(string stateOrProvinceId) =>
            GraftAsync.Run(async () =>
            {
                var id = ParseId(stateOrProvinceId, nameof(stateOrProvinceId));
                await using var context = GraftCoreDb.CreateContext();
                var stateOrProvince = await context.Set<StateOrProvince>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
                return stateOrProvince == null ? string.Empty : FormatStateOrProvince(stateOrProvince);
            });

        public static string CreateStateOrProvince(
            string countryId,
            string name,
            string code,
            string type) =>
            GraftAsync.Run(async () =>
            {
                Require(countryId, nameof(countryId));
                Require(name, nameof(name));

                await using var context = GraftCoreDb.CreateContext();
                var countryExists = await context.Set<Country>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == countryId);
                if (!countryExists)
                {
                    throw new InvalidOperationException($"Country '{countryId}' was not found.");
                }

                var stateOrProvince = new StateOrProvince
                {
                    CountryId = countryId,
                    Name = name,
                    Code = code,
                    Type = type
                };

                context.Set<StateOrProvince>().Add(stateOrProvince);
                await context.SaveChangesAsync();
                return FormatStateOrProvince(stateOrProvince);
            });

        public static string UpdateStateOrProvince(
            string stateOrProvinceId,
            string name,
            string code,
            string type) =>
            GraftAsync.Run(async () =>
            {
                var id = ParseId(stateOrProvinceId, nameof(stateOrProvinceId));
                Require(name, nameof(name));

                await using var context = GraftCoreDb.CreateContext();
                var stateOrProvince = await context.Set<StateOrProvince>()
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (stateOrProvince == null)
                {
                    return string.Empty;
                }

                stateOrProvince.Name = name;
                stateOrProvince.Code = code;
                stateOrProvince.Type = type;

                await context.SaveChangesAsync();
                return FormatStateOrProvince(stateOrProvince);
            });

        public static bool DeleteStateOrProvince(string stateOrProvinceId) =>
            GraftAsync.Run(async () =>
            {
                var id = ParseId(stateOrProvinceId, nameof(stateOrProvinceId));

                await using var context = GraftCoreDb.CreateContext();
                var stateOrProvince = await context.Set<StateOrProvince>()
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (stateOrProvince == null)
                {
                    return false;
                }

                try
                {
                    context.Set<StateOrProvince>().Remove(stateOrProvince);
                    await context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException)
                {
                    throw new InvalidOperationException(
                        $"The state or province {stateOrProvince.Name} can't not be deleted because it is referenced by other tables");
                }
            });

        private static string FormatStateOrProvince(StateOrProvince stateOrProvince) =>
            string.Join('|',
                stateOrProvince.Id.ToString(CultureInfo.InvariantCulture),
                stateOrProvince.CountryId ?? string.Empty,
                stateOrProvince.Name ?? string.Empty,
                stateOrProvince.Code ?? string.Empty,
                stateOrProvince.Type ?? string.Empty);

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
