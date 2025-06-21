using InsuranceQuoter_Service.Exceptions;

namespace InsuranceQuoter_Service.CompanyProduct;

public static class ProductInfoFactory
{
    private static readonly List<ProductInfoBase> _allProducts;
 
    static ProductInfoFactory()
    {
        Type baseType = typeof(ProductInfoBase);
 
        _allProducts = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .Select(t => (ProductInfoBase)Activator.CreateInstance(t)!)
            .ToList();
    }

    public static ProductInfoBase GetProductInfo(string companyName)
    {
        ProductInfoBase? match = _allProducts.FirstOrDefault(p =>
            p.CompanyName.Equals(companyName, StringComparison.OrdinalIgnoreCase));
 
        return match ?? throw new CompanyNotFoundException(companyName);
    }

    public static List<ProductInfoBase> GetAllProducts() => _allProducts;
}
