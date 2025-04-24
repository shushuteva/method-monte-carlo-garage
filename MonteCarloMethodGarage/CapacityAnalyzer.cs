using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloMethodGarage
{
    public class CapacityAnalyzer
    {
        private int minCapacity;
        private int maxCapacity;
        private int capacityStep;
        private double constructionCostPerSpot;
        private double annualMaintenanceCostPerSpot;
        private double revenuePerCarPerDay;
        private int yearsToAnalyze;
        private double discountRate;

        public CapacityAnalyzer(
            int minCapacity,
            int maxCapacity,
            int capacityStep,
            double constructionCostPerSpot,
            double annualMaintenanceCostPerSpot,
            double revenuePerCarPerDay,
            int yearsToAnalyze,
            double discountRate)
        {
            this.minCapacity = minCapacity;
            this.maxCapacity = maxCapacity;
            this.capacityStep = capacityStep;
            this.constructionCostPerSpot = constructionCostPerSpot;
            this.annualMaintenanceCostPerSpot = annualMaintenanceCostPerSpot;
            this.revenuePerCarPerDay = revenuePerCarPerDay;
            this.yearsToAnalyze = yearsToAnalyze;
            this.discountRate = discountRate;
        }

        public Dictionary<int, double> AnalyzeOptimalCapacity(
           Func<int, double> utilizationRateFunc,
           Func<int, int> carsLostFunc)
        {
            Dictionary<int, double> npvByCapacity = new Dictionary<int, double>();

            for (int capacity = minCapacity; capacity <= maxCapacity; capacity += capacityStep)
            {
                // Изчисляване на нетната настояща стойност за този капацитет
                double npv = CalculateNPV(capacity, utilizationRateFunc(capacity), carsLostFunc(capacity));
                npvByCapacity[capacity] = npv;
            }

            return npvByCapacity;
        }


        private double CalculateNPV(int capacity, double utilizationRate, int carsLostPerDay)
        {
            // Начална инвестиция (строителни разходи)
            double initialInvestment = capacity * constructionCostPerSpot;

            // Изчисляване на дисконтираните парични потоци
            double npv = -initialInvestment;

            for (int year = 1; year <= yearsToAnalyze; year++)
            {
                // Годишни приходи при дадена степен на използваемост
                double annualRevenue = 365 * capacity * utilizationRate * revenuePerCarPerDay;

                // Добавяне на загубените приходи от отказаните автомобили
                double lostRevenue = 365 * carsLostPerDay * revenuePerCarPerDay;

                // Годишни разходи за поддръжка
                double annualMaintenance = capacity * annualMaintenanceCostPerSpot;

                // Нетен паричен поток за годината
                double cashFlow = annualRevenue - annualMaintenance;

                // Дисконтиране на паричния поток
                npv += cashFlow / Math.Pow(1 + discountRate, year);
            }

            return npv;
        }
    }
}
