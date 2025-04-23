using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloMethodGarage
{
    //клас за симулация на гаража
    public class GarageSimulation
    {
        //капацитет
        private int capacity;
        //процент на пристигащи сутрин
        private double arrivalRateMorningPeak;
        //процент на пристигащи вечер
        private double arrivalRateEveningPeak;
        //процент на пристигащи нормално (време)
        private double arrivalRateNormal;
        //среден престой
        private int averageStayDuration;
        private double averageDurationStdDev;
        private Random random;

        //конструктор за инициализация на полетата
        public GarageSimulation(
            int capacity, 
            double arrivalMorning, 
            double arrivalEvening, 
            double arrivalNormal, 
            int averageDuration,
            double satyDurationStd,
            Random random
        )
        {
            this.capacity = capacity;
            this.arrivalRateMorningPeak = arrivalMorning;
            this.arrivalRateEveningPeak = arrivalEvening;
            this.arrivalRateNormal = arrivalNormal;
            this.averageStayDuration = averageDuration;
            this.averageDurationStdDev = satyDurationStd;
            this.random = random;
        }

        //метод за изпълнение на симулация за определен брой дни
        public SimulationResult RunSimulation(int days)
        {
            int totalMinutes = 24 * 60 * days;
            int[] occupancy = new int[totalMinutes];

            //Текуща заетост на гаража
            int currentOccupancy = 0;
            int maxOccupancy = 0;
            int turnedAwayCars = 0;

            //Структура от данни, която записва плановите излизания
            List<int>[] plannedDepartures = new List<int>[totalMinutes];
            for (int i = 0; i < totalMinutes; i++)
            {
                plannedDepartures[i] = new List<int>();
            }

            //Записване на броя отказани автомобили по часове
            Dictionary<int,int> hourlyrejections = new Dictionary<int,int>();
            for (int hour = 0; hour < 24; hour++)
            {
                hourlyrejections[hour] = 0;
            }

            for (int minute = 0; minute < totalMinutes; minute++)
            {
                int currentDay = minute / (24 * 60);
                int currentHour = (minute % (24 * 60))/60;

                if (plannedDepartures[minute].Count > 0)
                {
                    currentOccupancy -= plannedDepartures[minute].Count;
                    if (currentOccupancy<0)
                    {
                        currentOccupancy = 0;
                    }
                }

                double currentArrivalRate = GetArrivalRate(currentHour);
                int newCars= GeneratePoissonRandom(currentArrivalRate);


                for (int i = 0; i < newCars; i++)
                {
                    if (currentOccupancy < capacity)
                    {
                        currentOccupancy++;

                        int stayDuration = GenerateNormalStayDuration();

                        int departureMinute = minute + stayDuration;
                        if (departureMinute < totalMinutes)
                        {
                            plannedDepartures[departureMinute].Add(1);
                        }
                    }
                    else
                    {
                        // Гаражът е пълен, автомобилът се отказва
                        turnedAwayCars++;
                        hourlyrejections[currentHour]++;
                    }
                }

                maxOccupancy = Math.Max(maxOccupancy, currentOccupancy);

                occupancy[minute] = currentOccupancy;
            }


            // Изчисляване на средната заетост и процента използваемост
            double averageOccupancy = occupancy.Average();
            double utilizationRate = averageOccupancy / capacity;

            return new SimulationResult
            {
                MaxOccupancy = maxOccupancy,
                TurnedAwayCars = turnedAwayCars,
                AverageOccupancy = averageOccupancy,
                UtilizationRate = utilizationRate,
                OccupancyPerMinute = occupancy,
                HourlyRejections = hourlyrejections
            };
        }


        //определяне на скоростта на пристигане според часа на денонощието
        private double GetArrivalRate(int hour)
        {
            //сутрешен пик
            if (hour >= 7 && hour <= 10)
            {
                return arrivalRateMorningPeak;
            }

            //вечерен пик
            if (hour >= 16 && hour <= 19)
            {
                return arrivalRateEveningPeak;
            }

            //нощен пик малка заетост
            if (hour >= 22 || hour <=6)
            {
                return arrivalRateNormal * 0.3;
            }

            return arrivalRateNormal;
        }

        //Генериране на случайно число с Поасоново разпределение
        private int GeneratePoissonRandom(double lambda)
        {
            double L = Math.Exp(-lambda);
            double p = 1.0;
            int k = 0;


            do
            {
                k++;
                p *= random.NextDouble();
            }
            while (p>L);

            return k - 1;
        }

        private int GenerateNormalStayDuration()
        {
            // Box-Muller трансформация за генериране на нормално разпределени числа
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();

            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            int duration = (int)(averageStayDuration + z * averageDurationStdDev);

            // Минимална продължителност на престой
            return Math.Max(15, duration);
        }
    }
}
