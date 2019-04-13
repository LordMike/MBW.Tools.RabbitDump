using System;
using Microsoft.Extensions.DependencyInjection;

namespace MBW.Tools.RabbitDump.Utilities
{
    static class Extensions
    {
        public static IServiceCollection AddSingleton<T>(this IServiceCollection services, Type implementation)
        {
            return services.AddSingleton(typeof(T), implementation);
        }

        public static bool TryDispose(this IDisposable disposable)
        {
            try
            {
                disposable?.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}