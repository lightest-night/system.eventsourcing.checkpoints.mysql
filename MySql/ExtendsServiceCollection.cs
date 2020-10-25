using System;
using LightestNight.System.Data.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LightestNight.System.EventSourcing.Checkpoints.MySql
{
    public static class ExtendsServiceCollection
    {
        public static IServiceCollection AddMySqlCheckpointManagement(this IServiceCollection services,
            MySqlOptionsFactory mySqlOptionsFactory)
        {
            services.AddMySqlData(mySqlOptionsFactory)
                .TryAddSingleton<ICheckpointManager>(sp =>
                    new MySqlCheckpointManager(sp.GetRequiredService<IMySqlConnection>()));

            return services;
        } 
    }
}