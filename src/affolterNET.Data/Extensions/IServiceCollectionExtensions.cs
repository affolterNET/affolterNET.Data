using affolterNET.Data.Interfaces;
using affolterNET.Data.Interfaces.SessionHandler;
using affolterNET.Data.SessionHandler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace affolterNET.Data.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddAffolterNETDataServices(
            this IServiceCollection services,
            IConfiguration cfg,
            string connString,
            EnumHistoryMode historyMode = EnumHistoryMode.None,
            string? historyTableName = null)
        {
            var factory = new SqlServerConnectionFactory(connString);
            services.AddSingleton<IDbConnectionFactory>(factory);
            services.AddScoped<ISqlSessionHandler, SqlSessionHandler>();
            services.AddSingleton<ISqlSessionFactory>(provider => new SqlSessionFactory(factory));
            services.AddTransient<IHistorySaver, HistorySaver>(sp => new HistorySaver(connString, historyMode, historyTableName));

            return services;
        }

        public static IServiceCollection AddAffolterNETDataServicesNpgsql(
            this IServiceCollection services,
            string connString)
        {
            var factory = new NpgsqlConnectionFactory(connString);
            services.AddSingleton<IDbConnectionFactory>(factory);
            services.AddScoped<ISqlSessionHandler, SqlSessionHandler>();
            services.AddSingleton<ISqlSessionFactory>(provider => new SqlSessionFactory(factory));

            return services;
        }
    }
}