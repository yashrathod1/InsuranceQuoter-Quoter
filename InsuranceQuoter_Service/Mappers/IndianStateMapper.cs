using InsuranceQuoter_Service.Exceptions;

namespace InsuranceQuoter_Service.Implementation;

public class IndianStateMapper
{
    public static readonly Dictionary<string, string> StateMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Andhra Pradesh", "AP" },
        { "Arunachal Pradesh", "AR" },
        { "Assam", "AS" },
        { "Bihar", "BH" },
        { "Chhattisgarh", "CG" },
        { "Goa", "GA" },
        { "Gujarat", "GJ" },
        { "Haryana", "HR" },
        { "Himachal Pradesh", "HP" },
        { "Jharkhand", "JH" },
        { "Karnataka", "KA" },
        { "Kerala", "KL" },
        { "Madhya Pradesh", "MP" },
        { "Maharashtra", "MH" },
        { "Manipur", "MN" },
        { "Meghalaya", "ML" },
        { "Mizoram", "MZ" },
        { "Nagaland", "NL" },
        { "Odisha", "OD" },
        { "Punjab", "PB" },
        { "Rajasthan", "RJ" },
        { "Sikkim", "SK" },
        { "Tamil Nadu", "TN" },
        { "Telangana", "TG" },
        { "Tripura", "TR" },
        { "Uttar Pradesh", "UP" },
        { "Uttarakhand", "UK" },
        { "West Bengal", "WB" },
        { "Andaman and Nicobar Islands", "AN" },
        { "Chandigarh", "CH" },
        { "Dadra and Nagar Haveli and Daman and Diu", "DN" },
        { "Delhi", "DL" },
        { "Jammu and Kashmir", "JK" },
        { "Ladakh", "LA" },
        { "Lakshadweep", "LD" },
        { "Puducherry", "PY" }
    };

    public static bool TryGetStateCode(string stateName, out string? code)
    {
        return StateMap.TryGetValue(stateName.Trim(), out code);
    }
}
