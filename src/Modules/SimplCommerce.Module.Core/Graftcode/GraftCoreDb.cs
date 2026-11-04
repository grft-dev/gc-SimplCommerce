using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SimplCommerce.Infrastructure;
using SimplCommerce.Infrastructure.Modules;
using SimplCommerce.Module.Core.Data;
using SimplCommerce.Module.Core.Models;

namespace SimplCommerce.Module.Core.Graftcode
{
    internal static class GraftCoreDb
    {
        private const string ConnectionVariable = "SIMPLCOMMERCE_CORE_CN";
        private static readonly object Sync = new();
        private static bool _coreModuleRegistered;

        public static SimplDbContext CreateContext()
        {
            EnsureCoreModuleRegistered();
            var options = new DbContextOptionsBuilder<SimplDbContext>()
                .UseSqlServer(Required(ConnectionVariable))
                .Options;
            return new SimplDbContext(options);
        }

        public static string Required(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing {key} environment variable.");
            }

            return value;
        }

        private static void EnsureCoreModuleRegistered()
        {
            if (_coreModuleRegistered)
            {
                return;
            }

            lock (Sync)
            {
                if (_coreModuleRegistered)
                {
                    return;
                }

                if (!GlobalConfiguration.Modules.Any(m => m.Assembly == typeof(Country).Assembly))
                {
                    GlobalConfiguration.Modules.Add(new ModuleInfo
                    {
                        Id = "Core",
                        Name = "Core",
                        IsBundledWithHost = true,
                        Version = typeof(Country).Assembly.GetName().Version ?? new Version(1, 0, 0, 0),
                        Assembly = typeof(Country).Assembly
                    });
                }

                _coreModuleRegistered = true;
            }
        }
    }
}
