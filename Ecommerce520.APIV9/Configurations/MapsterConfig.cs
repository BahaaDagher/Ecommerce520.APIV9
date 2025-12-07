using Mapster;

namespace Ecommerce520.APIV9.Configurations
{
    public static class MapsterConfig
    {
        public static void RegisterMapsterConfig(this IServiceCollection services)
        {
            TypeAdapterConfig<ApplicationUser, ApplicationUserResponse>
                .NewConfig()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");

            TypeAdapterConfig<ApplicationUser, ApplicationUserRequest>
                .NewConfig()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");

            TypeAdapterConfig<ApplicationUser, UserResponse>
                .NewConfig()
                .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");

            TypeAdapterConfig<Product, ProductResponse>
                .NewConfig()
                .Map(dest => dest.ProductSubImages, 
                    src => src.ProductSubImages != null?
                    src.ProductSubImages.Select(ps => ps.Img) :
                    new List<string>())
                .Map(dest => dest.ProductColors, 
                    src => src.ProductColors !=null ?
                    src.ProductColors.Select(ps => ps.Color) : 
                    new List<string>());
        }
    }
}
