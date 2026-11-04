using System;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Graftcode;
using SimplCommerce.Module.Core.Graftcode;
using SimplCommerce.Module.Core.Models;

namespace SimplCommerce.Module.Core.Areas.Core.Controllers
{
    public class CountryApiController
    {
        public static string[] ListCountries(string shippingEnabled = "") =>
            GraftAsync.Run(async () =>
            {
                await using var context = GraftCoreDb.CreateContext();
                var query = context.Set<Country>().AsNoTracking();

                if (!string.IsNullOrWhiteSpace(shippingEnabled))
                {
                    if (!bool.TryParse(shippingEnabled, out var enabled))
                    {
                        throw new InvalidOperationException("'shippingEnabled' must be true or false.");
                    }

                    query = query.Where(x => x.IsShippingEnabled == enabled);
                }

                var countries = await query
                    .OrderBy(x => x.Name)
                    .ToListAsync();
                return countries.Select(FormatCountry).ToArray();
            });

        public static string GetCountryCard(string countryId) =>
            GraftAsync.Run(async () =>
            {
                Require(countryId, nameof(countryId));
                await using var context = GraftCoreDb.CreateContext();
                var country = await context.Set<Country>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == countryId);
                return country == null ? string.Empty : FormatCountry(country);
            });

        public static string CreateCountry(
            string countryId,
            string name,
            string code3,
            bool shippingEnabled,
            bool billingEnabled,
            bool cityEnabled,
            bool zipCodeEnabled,
            bool districtEnabled) =>
            GraftAsync.Run(async () =>
            {
                Require(countryId, nameof(countryId));
                Require(name, nameof(name));

                await using var context = GraftCoreDb.CreateContext();
                var exists = await context.Set<Country>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == countryId);
                if (exists)
                {
                    throw new InvalidOperationException($"Country '{countryId}' already exists.");
                }

                var country = new Country(countryId)
                {
                    Name = name,
                    Code3 = code3,
                    IsShippingEnabled = shippingEnabled,
                    IsBillingEnabled = billingEnabled,
                    IsCityEnabled = cityEnabled,
                    IsZipCodeEnabled = zipCodeEnabled,
                    IsDistrictEnabled = districtEnabled
                };

                context.Set<Country>().Add(country);
                await context.SaveChangesAsync();

                return FormatCountry(country);
            });

        public static string UpdateCountry(
            string countryId,
            string name,
            string code3,
            bool shippingEnabled,
            bool billingEnabled,
            bool cityEnabled,
            bool zipCodeEnabled,
            bool districtEnabled) =>
            GraftAsync.Run(async () =>
            {
                Require(countryId, nameof(countryId));
                Require(name, nameof(name));

                await using var context = GraftCoreDb.CreateContext();
                var country = await context.Set<Country>()
                    .FirstOrDefaultAsync(x => x.Id == countryId);
                if (country == null)
                {
                    return string.Empty;
                }

                country.Name = name;
                country.Code3 = code3;
                country.IsShippingEnabled = shippingEnabled;
                country.IsBillingEnabled = billingEnabled;
                country.IsCityEnabled = cityEnabled;
                country.IsZipCodeEnabled = zipCodeEnabled;
                country.IsDistrictEnabled = districtEnabled;

                await context.SaveChangesAsync();
                return FormatCountry(country);
            });

        public static bool DeleteCountry(string countryId) =>
            GraftAsync.Run(async () =>
            {
                Require(countryId, nameof(countryId));

                await using var context = GraftCoreDb.CreateContext();
                var country = await context.Set<Country>()
                    .FirstOrDefaultAsync(x => x.Id == countryId);
                if (country == null)
                {
                    return false;
                }

                try
                {
                    context.Set<Country>().Remove(country);
                    await context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException)
                {
                    throw new InvalidOperationException(
                        $"The country {country.Name} can't not be deleted because it is referenced by other tables");
                }
            });

        private static string FormatCountry(Country country) =>
            string.Join('|',
                country.Id ?? string.Empty,
                country.Name ?? string.Empty,
                country.Code3 ?? string.Empty,
                country.IsShippingEnabled.ToString(CultureInfo.InvariantCulture),
                country.IsBillingEnabled.ToString(CultureInfo.InvariantCulture),
                country.IsCityEnabled.ToString(CultureInfo.InvariantCulture),
                country.IsZipCodeEnabled.ToString(CultureInfo.InvariantCulture),
                country.IsDistrictEnabled.ToString(CultureInfo.InvariantCulture));

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"'{name}' is required.");
            }
        }
    }
}
