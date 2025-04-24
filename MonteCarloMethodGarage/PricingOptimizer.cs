using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloMethodGarage
{
    //Клас за оптимизация на ценообразуването в гаража
    public class PricingOptimizer
    {
        private double basePrice;
        private double peakSurcharge;
        private double weekendDiscount;
        private Random random;

        public PricingOptimizer(double basePrice, double peakSurcharge, double weekendDiscount, Random random)
        {
            this.basePrice = basePrice;
            this.peakSurcharge = peakSurcharge;
            this.weekendDiscount = weekendDiscount;
            this.random = random;
        }

        public Dictionary<string, double> SimulatePricingStrategies(
            int days,
            double elasticy,
            double averageOccupancy,
            int capacity)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            result["Base"] = SimulateRevenue(basePrice, peakSurcharge, weekendDiscount,days,elasticy,averageOccupancy,capacity);


            result["HigherBase"] = SimulateRevenue(basePrice * 1.2, peakSurcharge, weekendDiscount, days, elasticy, averageOccupancy, capacity);
            result["LowerBase"] = SimulateRevenue(basePrice * 0.8, peakSurcharge, weekendDiscount, days, elasticy, averageOccupancy, capacity);
            result["HigherPeak"] = SimulateRevenue(basePrice, peakSurcharge * 1.5, weekendDiscount, days, elasticy, averageOccupancy, capacity);
            result["BiggerWeekendDiscount"] = SimulateRevenue(basePrice, peakSurcharge, weekendDiscount * 1.5, days, elasticy, averageOccupancy, capacity);

            result["DynamicPricing"] = SimulateDynamicPricing(basePrice, days, elasticy, averageOccupancy, capacity);

            return result;



        }


        private double SimulateRevenue(
            double price,
            double surcharge,
            double discount,
            int days,
            double elasticy,
            double averageOccupancy,
            int capacity
            )
        {
            double totalRevenue = 0;


            for (int day = 0; day < days; day++)
            {
                bool isWeekend = day % 7 >= 5;

                for (int hour = 0; hour < 24; hour++)
                {
                    bool isPeak = (hour >= 7 && hour < 10) || (hour >= 16 && hour < 19);

                    double currentPrice = price;
                    if (isPeak)
                    {
                        currentPrice += surcharge;
                    }

                    if (isWeekend)
                    {
                        currentPrice *= (1 - discount);
                    }

                    double baselineOccupancy = averageOccupancy;
                    if (isPeak)
                    {
                        baselineOccupancy *= 1.5;
                    }

                    if (isWeekend)
                    {
                        baselineOccupancy *= 0.7;
                    }


                    double priceRatio = currentPrice / price;
                    double adjustedOccupancy = baselineOccupancy * Math.Pow(priceRatio, -elasticy);

                    adjustedOccupancy = Math.Min(adjustedOccupancy, capacity);

                    double hourlyRevenue = adjustedOccupancy * currentPrice;
                    totalRevenue += hourlyRevenue;
                }
            }

            return totalRevenue;
        }

        private double SimulateDynamicPricing(
            double basePrice,
            int days,
            double elasticy,
            double averageOccupancy,
            int capacity
            )
        {
            double totalRevenue = 0;

            for (int day = 0; day < days; day++)
            {
                bool isWeekend = day % 7 >= 5;

                for (int hour = 0; hour < 24; hour++)
                {

                    double baselineOccupancy = averageOccupancy;
                    if (hour >= 7 && hour < 10) baselineOccupancy *= 1.5;
                    if (hour >= 16 && hour < 19) baselineOccupancy *= 1.4;
                    if (isWeekend) baselineOccupancy *= 0.7;


                    double randomFactor = 0.8 + (random.NextDouble() * 0.4);
                    double expectedOccupancy = baselineOccupancy * randomFactor;

                    double occupancyRatio = expectedOccupancy / capacity;
                    double dynamicPrice = basePrice;

                    if (occupancyRatio > 0.9) dynamicPrice = basePrice * 2.0;       // Много висока заетост
                    else if (occupancyRatio > 0.75) dynamicPrice = basePrice * 1.5; // Висока заетост
                    else if (occupancyRatio > 0.5) dynamicPrice = basePrice * 1.2;  // Средна заетост
                    else if (occupancyRatio < 0.3) dynamicPrice = basePrice * 0.8;  // Ниска заетост

                    // Корекция на заетостта според новата цена и еластичността
                    double priceRatio = dynamicPrice / basePrice;
                    double adjustedOccupancy = expectedOccupancy * Math.Pow(priceRatio, -elasticy);
                    adjustedOccupancy = Math.Min(adjustedOccupancy, capacity);

                    // Изчисляване на прихода
                    double hourlyRevenue = adjustedOccupancy * dynamicPrice;
                    totalRevenue += hourlyRevenue;
                }
            }

            return totalRevenue;
        }
    }
}
