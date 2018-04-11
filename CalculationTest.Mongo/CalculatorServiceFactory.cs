using System;

namespace CalculationTest.Mongo
{
    public enum CalculatorServiceType : short
    {
        Standard,
        Clustered
    }

    public static class CalculatorServiceFactory
    {
        public static ICalculatorService Create(CalculatorServiceType serviceType)
        {
            switch (serviceType)
            {
                case CalculatorServiceType.Standard:
                    return new CalculatorService();
                case CalculatorServiceType.Clustered:
                    return new ClusteredCalculatorService();
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
            }
        }
    }
}