namespace FarmBooks.Core;

public static class TaxConstants
{
    public const decimal VatRate = 0.15m;

    public static decimal VatMultiplier => 1m + VatRate;
}
