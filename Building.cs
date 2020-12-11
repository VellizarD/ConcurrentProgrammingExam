using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentPrgrammingExam
{
  public class Building
    {
        public Dictionary<Floor, ConcurrentQueue<Agent>> floorQueue = new Dictionary<Floor, ConcurrentQueue<Agent>>();
        public Elevator elevator;
        public Building(Elevator elevator)
        {
            this.elevator = elevator;
            floorQueue.Add(Floor.G, new ConcurrentQueue<Agent>());
            floorQueue.Add(Floor.S, new ConcurrentQueue<Agent>());
            floorQueue.Add(Floor.T1, new ConcurrentQueue<Agent>());
            floorQueue.Add(Floor.T2, new ConcurrentQueue<Agent>());
        }

        public void addToQueue(Agent agent)
        {
            foreach (var a in floorQueue)
            {
                if (a.Key.Equals(agent.currentFloor))
                {
                    a.Value.Enqueue(agent);
                }
            }
        }
        public void removeFromQueue(Agent agent)
        {
            foreach (var a in floorQueue)
            {
                if (a.Key.Equals(agent.currentFloor))
                {
                    a.Value.TryDequeue(out _);
                }
            }
        }

    }
}
