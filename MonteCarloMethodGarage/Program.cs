namespace MonteCarloMethodGarage
{
    internal class Program
    {
        //Изчиславане на процентност от списък с числа
        static int CalculatePercentile(List<int> numbers, int percentile)
        {
            var sorted = numbers.OrderBy(n => n).ToList();
            int index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
            return sorted[Math.Max(0, index)];
        }

        //изчисляване на препоръчителен капацитет
        static int CalculateRecommendedCapacity(List<int> maxOccupancies, List<int> turnedAwayCars, double targetServiceRate)
        {
            int totalCars = 0;
            int rejectedCars = turnedAwayCars.Sum();
            int estimatedDemand = maxOccupancies.Max() + rejectedCars;


            //намираме минималния капацитет за покриване на целта за обслужване
            return (int)Math.Ceiling((double)estimatedDemand * rejectedCars);
        }

        //Запазване на данните в CSV
        static void SaveToCSV(int[] occupancyByMinute, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("Ден,Час,Минута,Заетост");

                for (int i = 0; i < occupancyByMinute.Length; i++)
                {
                    int day = i / (24 * 60) + 1;
                    int hour = (i % (24 * 60)) / 60;
                    int minute = i % 60;
                    sw.WriteLine($"{day},{hour},{minute},{occupancyByMinute[i]}");
                }
            }
        }

        //Създаване на проста хистограма в конзолата
        static void CreateHistogram(List<int> values)
        {
            //намираме минималната и максималната стойност
            int min = values.Min();
            int max = values.Max();

            //създаваме 10 интервала
            int bucketCount = 10;
            double bucketSize = (max - min) / (double)bucketCount;
            if (bucketSize < 1) bucketSize = 1;

            int[] buckets = new int[bucketCount];

            //разпределяне на стойностите по интервали
            foreach (int value in values)
            {
                int bucketIndex = (int)((value - min) / bucketSize);
                if (bucketIndex == bucketCount) bucketIndex--; // Корекция за максималната стойност
                buckets[bucketIndex]++;
            }


            // Намиране на най-големия брой за мащабиране
            int maxCount = buckets.Max();
            if (maxCount == 0) maxCount = 1;

            // Показване на хистограмата
            for (int i = 0; i < bucketCount; i++)
            {
                int lowerBound = min + (int)(i * bucketSize);
                int upperBound = min + (int)((i + 1) * bucketSize);

                // Мащабиране на хистограмата до 50 символа
                int barLength = (int)((double)buckets[i] / maxCount * 50);
                string bar = new string('█', barLength);

                Console.WriteLine($"{lowerBound} - {upperBound}: {bar} ({buckets[i]})");
            }
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("Монте Карло симулация на гараж");
            Console.WriteLine("==============================");

            //параметри на симулация
            int garageCapacity = 100; // браой места в гаража
            int simulationDays = 30; //дни на симулация
            int minutesPerDay = 24 * 60;//минути на денонощие
            int numberOfSimulations = 1000; //брой симулации

            //параметри за модел на пристигане
            double arrivalRateMorningPeak = 0.3;    // Средно пристигане на автомобили/минута (сутрешен пик)
            double arrivalRateEvening = 0.2;        // Средно пристигане на автомобили/минута (вечерно време)
            double arrivalRateNormal = 0.1;         // Средно пристигане на автомобили/минута (нормално време)

            int averageStayDurationMinutes = 240;   // Средна продължителност на престой (минути)
            double stayDurationStdDev = 60;         // Стандартно отклонение на продължителността (минути)


            // Структури за съхранение на резултатите
            List<int> maxOccupancies = new List<int>();
            List<int> totalTurnedAwayCars = new List<int>();
            List<double> averageOccupancies = new List<double>();
            List<double> utilizationRates = new List<double>();

            // Генератор на случайни числа
            Random random = new Random();


            // Извършване на симулациите
            for (int sim = 0; sim < numberOfSimulations; sim++)
            {
                // Създаване на нова симулация за гаража
                var garageSimulation = new GarageSimulation(
                    garageCapacity,
                    arrivalRateMorningPeak,
                    arrivalRateEvening,
                    arrivalRateNormal,
                    averageStayDurationMinutes,
                    stayDurationStdDev,
                    random
                );

                // Изпълнение на симулацията
                var simulationResults = garageSimulation.RunSimulation(simulationDays);

                // Запазване на резултатите
                maxOccupancies.Add(simulationResults.MaxOccupancy);
                totalTurnedAwayCars.Add(simulationResults.TurnedAwayCars);
                averageOccupancies.Add(simulationResults.AverageOccupancy);
                utilizationRates.Add(simulationResults.UtilizationRate);

                // Запазване на данните от първите 5 симулации за визуализация
                if (sim < 5)
                {
                    SaveToCSV(simulationResults.OccupancyPerMinute, $"garage_simulation_{sim + 1}.csv");
                }
            }

            // Анализ на резултатите
            double avgMaxOccupancy = maxOccupancies.Average();
            double avgTurnedAway = totalTurnedAwayCars.Average();
            double avgOccupancy = averageOccupancies.Average();
            double avgUtilizationRate = utilizationRates.Average();
            int turnedAwayPercentile95 = CalculatePercentile(totalTurnedAwayCars, 95);

            // Показване на резултатите
            Console.WriteLine($"\nРезултати от {numberOfSimulations} симулации за {simulationDays} дни:");
            Console.WriteLine($"Капацитет на гаража: {garageCapacity} места");
            Console.WriteLine($"Средна максимална заетост: {avgMaxOccupancy:F1} автомобила");
            Console.WriteLine($"Процент използваемост: {avgUtilizationRate:P2}");
            Console.WriteLine($"Средно отказани автомобили за период: {avgTurnedAway:F1}");
            Console.WriteLine($"95-ти перцентил на отказани автомобили: {turnedAwayPercentile95}");

            int recommendedCapacity = CalculateRecommendedCapacity(
               maxOccupancies,
               totalTurnedAwayCars,
               0.95 // Целеви процент обслужени автомобили (95%)
           );

            Console.WriteLine($"\nПрепоръчителен капацитет за покриване на 95% от търсенето: {recommendedCapacity} места");

            // Разпределение по часове
            Console.WriteLine("\nХистограма на отказаните автомобили:");
            CreateHistogram(totalTurnedAwayCars);

            Console.WriteLine("\nДанните за заетостта от първите 5 симулации са запазени в CSV файлове.");
            Console.WriteLine("Натиснете който и да е клавиш за изход...");
            Console.ReadKey();
        }


    }
}
