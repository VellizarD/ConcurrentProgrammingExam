using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using writer = System.Console;


namespace ConcurrentPrgrammingExam
{
    public enum Floor { G = 1, S, T1, T2 };
    public enum Door { Open, Closed };

    public class Elevator
    {
        public static Mutex mutex = new Mutex();
        public AutoResetEvent elevatorSignal = new AutoResetEvent(false);
        public AutoResetEvent agentSignal = new AutoResetEvent(false);
        public Thread workerThread;

        public List<Floor> floors = new List<Floor>() { Floor.G, Floor.S, Floor.T1, Floor.T2 };
        public Floor currentFloor = Floor.G;
        public Floor destination;

        public Agent agentInElevator = null;

        public (Agent, Floor) threadParams;

        public Door door = Door.Closed;
        //public TextWriter writer;


        public Elevator(TextWriter writer)
        {
            //      this.writer = writer;
        }
        public bool SecutrityCheck()
        {
            Thread.Sleep(1000);
            return agentInElevator.accessableFloors.Contains(currentFloor);
        }
        public void Enter(Agent agent)
        {
            agentInElevator = agent;
        }
        public void Leave()
        {
            agentInElevator = null;
        }
        public void SetDestination(Floor destination)
        {
            this.destination = destination;
        }
        public bool IsAgentInElevator()
        {
            return agentInElevator != null;
        }
        public Floor GetCurrentFloor()
        {
            return currentFloor;
        }
        public void Call()
        {

            if (currentFloor != destination)
            {
                door = Door.Closed;
                if (!IsAgentInElevator())
                    writer.WriteLine($"Elevator was called at {destination}");
                Thread.Sleep(500);
                MoveThroughFloors();
            }
            else
            {
                writer.WriteLine($"Elevator is already at {destination}");
                Thread.Sleep(500);
                OpenDoors();
            }
        }
        public void MoveThroughFloors()
        {
            int count = 0;
            while (currentFloor != destination)
            {
                
                if (currentFloor > destination)
                {
                    Thread.Sleep(1000);
                    count++;
                    writer.WriteLine($"\t{count} second has passed!");
                    currentFloor--;                    
                }
                else if (currentFloor < destination)
                {
                    Thread.Sleep(1000);
                    count++;
                    writer.WriteLine($"\t{count} second has passed!");
                    currentFloor++;
                }
            }
            writer.WriteLine($"\nElevator arrived at {destination}");
            OpenDoors();
        }

        public void OpenDoors()
        {
            if (IsAgentInElevator())
            {
                if (SecutrityCheck())
                {
                    door = Door.Open;
                    Thread.Sleep(500);
                    writer.WriteLine("Elevator opened!");
                    agentSignal.Set();
                    elevatorSignal.WaitOne();
                }
                else
                {
                    writer.WriteLine("		  ----------------------------------------------");
                    writer.WriteLine("\t\t  Δ=> Agent has unsufficient Security Level! <=Δ");
                    writer.WriteLine("\t\tPlease push a button according to your Security Level!");
                    writer.WriteLine("		------------------------------------------------------");
                    Thread.Sleep(500);
                    agentSignal.Set();
                    elevatorSignal.WaitOne();
                }
            }
            else
            {
                Thread.Sleep(500);
                writer.WriteLine("Elevator opened!");
                door = Door.Open;
                agentSignal.Set();
                elevatorSignal.WaitOne();
            }
        }

        public void ElevatorThreadWorker((Agent, Floor) threadParams)
        {
            mutex.WaitOne();
            if (threadParams.Item1 != null)
            {
                Enter(threadParams.Item1);
            }
            SetDestination(threadParams.Item2);
            workerThread = new Thread(Call);
            workerThread.Start();
            mutex.ReleaseMutex();
        }

        public IEnumerable<Floor> GetAvailableButtons()
        {
            foreach (var el in floors)
            {
                if (!el.Equals(currentFloor))
                    yield return el;
            }
        }
    }
}
