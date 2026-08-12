using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Application;

namespace SellerApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services.AddSharedApplication(typeof(DependencyInjection).Assembly);
}
