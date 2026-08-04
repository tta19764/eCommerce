namespace ProductApi.Domain.Products;

/// <summary>
/// Product fulfillment type used to distinguish physical and digital catalog items.
/// </summary>
public enum ProductType
{
    Physical = 1,
    DigitalDownload = 2,
    LicenseKey = 3,
    Service = 4,
    Subscription = 5,
    Bundle = 6
}
