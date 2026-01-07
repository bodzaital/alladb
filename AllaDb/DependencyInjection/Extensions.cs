using Microsoft.Extensions.DependencyInjection;

namespace AllaDb.DependencyInjection;

public static class Extensions
{
	public static IServiceCollection AddAllaDb(this IServiceCollection services, Action<AllaOptions> setupAction)
	{
		services.AddScoped<IAlla, Alla>();
		services.Configure(setupAction);

		return services;
	}
}