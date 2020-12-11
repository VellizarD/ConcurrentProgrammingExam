using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using writer = System.Console;
using System.Collections.Concurrent;

namespace ConcurrentPrgrammingExam
{
    public enum SecurityLevel { Confidential, Secret, Top_Secret };
    public class Agent
    {
        #region Fields

        public static Mutex queueMutex = new Mutex();
        public static AutoResetEvent elevatorSignal = new AutoResetEvent(false);
        public static AutoResetEvent agentSignal = new AutoResetEvent(false);
        public ManualResetEvent atHomeEvent = new ManualResetEvent(false);

        public Elevator elevator;
        public Building building;
        public Floor[] accessableFloors;
        public (Agent, Floor) threadParams;
        public Floor currentFloor = Floor.G;

        public SecurityLevel secLevel;

        public string name;
        public Random rand = new Random();
        public int workDone = 0;
        public bool GoHomeTimeFlag = false;

        public bool AtHome
        {
            get { return atHomeEvent.WaitOne(0); }
        }

        #endregion Fields
        #region Constructor
        public Agent(string name, SecurityLevel secLevel, Building building)
        {
            this.building = building;
            elevator = building.elevator;
            elevatorSignal = elevator.elevatorSignal;
            agentSignal = elevator.agentSignal;
            this.name = name;
            this.secLevel = secLevel;
            accessableFloors = secLevel switch
            {
                SecurityLevel.Confidential => new Floor[] { Floor.G },
                SecurityLevel.Secret => new Floor[] { Floor.G, Floor.S },
                SecurityLevel.Top_Secret => new Floor[] { Floor.G, Floor.S, Floor.T1, Floor.T2 },
                _ => throw new ArgumentException("This Security Level is currently not supported!")
            };
        }
        #endregion Constructor

        //  public TextWriter writer;
        public void EnterElevator()
        {
            elevator.Enter(this);
            Thread.Sleep(500);
            writer.WriteLine($"Agent {name} entered the elevator!");
            building.removeFromQueue(this);
            ChooseDestination();
        }
        public void LeaveElevator()
        {
            Thread.Sleep(500);
            elevator.Leave();
            writer.WriteLine($"Agent {name} left the elevator!");
            currentFloor = elevator.GetCurrentFloor();
            writer.WriteLine($"Agent {name} is doing some work at floor {currentFloor}! Work Done: {workDone}%\n");
            queueMutex.ReleaseMutex();
        }
        public void CallElevator()
        {
            if (building.floorQueue.Where(x => x.Key.Equals(currentFloor)).First().Value.Count > 0)
            {
                Thread.Sleep(500);
                writer.WriteLine($"- Agent {name} gets in the queue of floor [{currentFloor}]");
            }
            building.addToQueue(this);

            Agent result;
            do
            {
                building.floorQueue.Where(x => x.Key.Equals(currentFloor)).First().Value.TryPeek(out result);
            }
            while (!result.Equals(this));
            writer.WriteLine($"Agent {name} is calling the elevator from {currentFloor}!");
            Thread.Sleep(500);
            queueMutex.WaitOne();
            elevator.ElevatorThreadWorker((null, currentFloor));
            agentSignal.WaitOne();
        }

        public void ChooseDestination()
        {
            if (!GoHomeTimeFlag)
            {
                Thread.Sleep(500);
                do
                {
                    Floor destination = PressButton();
                    Thread.Sleep(500);
                    writer.WriteLine($"Agent {name} pressed button for floor {destination}!");
                    elevator.SetDestination(destination);
                    elevator.ElevatorThreadWorker((this, destination));
                    agentSignal.WaitOne();
                } while (elevator.door == Door.Closed);
            }
            else
            {
                Thread.Sleep(500);
                do
                {
                    writer.WriteLine($"Agent {name} pressed button for floor {Floor.G}!");
                    elevator.SetDestination(Floor.G);
                    elevatorSignal.Set();
                } while (elevator.door == Door.Closed);

            }
        }

        public void GoHome()
        {
            writer.WriteLine($"It's time for Agent {name} to go home!");
            Thread.Sleep(500);
            if (currentFloor != Floor.G)
            {
                GoHomeTimeFlag = true;
                CallElevator();
                Thread.Sleep(500);
                writer.WriteLine($"Agent {name} is leaving the building!");
                queueMutex.ReleaseMutex();
            }
            else
            {
                Thread.Sleep(500);
                writer.WriteLine($"Agent {name} is leaving the building!");
            }
            atHomeEvent.Set();
        }

        protected void Roam()
        {
            writer.WriteLine($"Agent {name} [{secLevel}] entered the building!");
            Thread.Sleep(500);
            while (workDone != 100)
            {
                CallElevator();
                EnterElevator();
                LeaveElevator();
                DoSomeWork();
            }
            GoHome();
        }
        public void RoamThreadWorker()
        {
            var t = new Thread(Roam);
            t.Start();
        }

        public void DoSomeWork()
        {
            Thread.Sleep(3000);
            var randNum = rand.Next(10, 30);
            if (workDone + randNum > 100)
                workDone = 100;
            else
                workDone += randNum;
        }
        public Floor PressButton()
        {
            Thread.Sleep(500);
            List<Floor> availableButtonsList = elevator.GetAvailableButtons().ToList();
            int randomNum = rand.Next(1000);
            int index = randomNum % 3;
            Floor chosenFloor = availableButtonsList[index];
            return chosenFloor;
        }

    }

}