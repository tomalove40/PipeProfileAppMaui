using Microsoft.Extensions.DependencyInjection;
using PipeProfileAppMaui.Services;

namespace PipeProfileAppMaui.ViewModels
{
    public class ViewModelLocator
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewModelLocator()
        {
            var services = new ServiceCollection();

            // Сначала MainViewModel
            services.AddSingleton<MainViewModel>();

            // Затем RibbonViewModel через фабрику, чтобы DI знал про зависимость
            services.AddSingleton<RibbonViewModel>(sp =>
            {
                var mainVm = sp.GetRequiredService<MainViewModel>();
                return new RibbonViewModel(mainVm);
            });

            services.AddSingleton<GroundViewModel>();
            services.AddSingleton<PipeViewModel>();
            services.AddSingleton<PipeProfileRenderer>();
            services.AddSingleton<RibbonViewModel>();
           _serviceProvider = services.BuildServiceProvider();
        }

        public MainViewModel MainViewModel => _serviceProvider.GetRequiredService<MainViewModel>();
        public GroundViewModel GroundViewModel => _serviceProvider.GetRequiredService<GroundViewModel>();
        public PipeViewModel PipeViewModel => _serviceProvider.GetRequiredService<PipeViewModel>();
        public RibbonViewModel RibbonViewModel => _serviceProvider.GetRequiredService<RibbonViewModel>();
    }
}