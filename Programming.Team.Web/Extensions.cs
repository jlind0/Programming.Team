using Programming.Team.ViewModels;
using Programming.Team.ViewModels.Admin;
using System.Runtime.CompilerServices;

namespace Programming.Team.Web
{
    public static class Extensions
    {
        const string KeyedServiceName = nameof(KeyedServiceName);
        public static void AddTransientAndInit<TViewModel>(this IServiceCollection services)
            where TViewModel: class, IInitializable
        {
            services.AddKeyedTransient<TViewModel>(KeyedServiceName);
            services.AddTransient(sp =>
            {
                var viewModel = sp.GetKeyedService<TViewModel>(KeyedServiceName) ?? throw new InvalidOperationException();
                viewModel.Initialize();
                return viewModel;
            });
        }
    }
}
