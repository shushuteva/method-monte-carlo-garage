using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarloMethodGarage
{
    //Клас, който представялва резултати от една симулация
    public class SimulationResult
    {
        //Максимален обем
        public int MaxOccupancy { get; set; }
        //Брой върнати коли
        public int TurnedAwayCars { get; set; }
        //Средна заетост на гаража
        public double AverageOccupancy { get; set; }
        public double UtilizationRate { get; set; }
        //Заетост по минута
        public int[] OccupancyPerMinute { get; set; }
        //Върнати коли по час
        public Dictionary<int,int> HourlyRejections { get; set; }

    }
}
