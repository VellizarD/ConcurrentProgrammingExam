using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConcurrentPrgrammingExam
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine($"Simulation Started, current time: {DateTime.Now}\n");
            using (StreamWriter writer = new StreamWriter("C:\\Users\\Vili\\source\\repos\\ConcurrentPrgrammingExam\\ConcurrentPrgrammingExam\\Area51Elevator.txt"))
            {
                var syncWriter = StreamWriter.Synchronized(writer);
                var elevator = new Elevator(syncWriter);
                var building = new Building(elevator);
                List<Agent> agents = new List<Agent>
                {
                    new Agent(1 + "", SecurityLevel.Top_Secret, building),
                    new Agent(2 + "", SecurityLevel.Confidential, building),
                    new Agent(3 + "", SecurityLevel.Secret, building),
                    new Agent(4 + "", SecurityLevel.Top_Secret, building),
                    new Agent(5 + "", SecurityLevel.Confidential, building),
                    new Agent(6 + "", SecurityLevel.Secret, building),
                    new Agent(7 + "", SecurityLevel.Top_Secret, building),
                    new Agent(8 + "", SecurityLevel.Confidential, building),
                    new Agent(9 + "", SecurityLevel.Secret, building),
                    new Agent(10 + "", SecurityLevel.Top_Secret, building),
                    new Agent(11 + "", SecurityLevel.Confidential, building),
                    new Agent(12 + "", SecurityLevel.Secret, building)
                };

                syncWriter.WriteLine($"Simulation Started, current time: {DateTime.Now}");

                agents.ForEach(x => x.RoamThreadWorker());

                while (agents.Any(x => !x.AtHome))
                { }

                syncWriter.WriteLine($"Simulation Ended, current time: {DateTime.Now}");
            }
            Console.WriteLine($"Simulation Ended, current time: {DateTime.Now}");
            Console.Read();
        }
    }
}
