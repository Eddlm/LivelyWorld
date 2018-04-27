using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lively_World
{
   public class DrugDeal
    {
        int DealerRLGroup = World.AddRelationshipGroup("DealerRLGroup");

        List<Ped> Goons = new List<Ped>();
        Ped Dealer;
        Ped Buyer;
        Vehicle car;
        public bool Finished = false;
        Vector3 center;
        bool Ruined = false;
        float Distance=300;
        bool GangDeal = false;
        bool StoleCar = false;
        int TimeAlive = 0;
        public DrugDeal(Vector3 place, bool gangs)
        {
             if(LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Called for DrugDeal Event");
            center = place;

            TimeAlive = Game.GameTime+60000;
            //Distance = place.DistanceTo(Game.Player.Character.Position);
            GangDeal = gangs;
            if(LivelyWorld.Debug >= DebugLevel.EventsAndScenarios)  UI.Notify("~b~Deal spawned");
            //LivelyWorld.BlacklistedImportantEvents.Add(EventType.Deal);
            Vector3 carpos = World.GetSafeCoordForPed(center, false);

            float Heading = LivelyWorld.AngleBetweenVectors(place, carpos);
            OutputArgument outArgA = new OutputArgument();
            OutputArgument outArgB = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, place.X, place.Y, place.Z, outArgA, outArgB, 0, 1077936128, 0))
            {
                Heading = outArgB.GetResult<float>();
            }
                if (gangs)
            {

                car = World.CreateVehicle("gburrito", carpos, Heading + LivelyWorld.RandomInt(-20, 20));// LivelyWorld.AngleBetweenVectors(place, carpos));
                if(!LivelyWorld.CanWeUse(car))
                {
                    Finished = true;
                    return;
                }
                if (!LivelyWorld.CarCanSeePos(car, car.Position + (car.ForwardVector * -10), 0)) car.Position = car.Position + (car.ForwardVector * 3);

                //Dealer
                Ped newped = World.CreatePed("G_M_Y_Lost_01", car.Position + (car.ForwardVector * -4));
                if (!LivelyWorld.CanWeUse(newped))
                {
                    Finished = true;
                    return;
                }
                newped.RelationshipGroup = LivelyWorld.NeutralRLGroup;
                newped.Weapons.Give(WeaponHash.Pistol, 30, false, true);

                newped.AlwaysKeepTask = true;
                Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, newped, "WORLD_HUMAN_STAND_IMPATIENT", 1000, true);
                newped.Heading = car.Heading + 180f;//LivelyWorld.AngleBetweenVectors(carpos, place);
                Dealer = newped;


                //Goons
                newped = World.CreatePed("G_M_Y_Lost_01", car.Position + (car.ForwardVector * -3) + (car.RightVector * -2));
                if (!LivelyWorld.CanWeUse(newped))
                {
                    Finished = true;
                    return;
                }
                newped.RelationshipGroup = LivelyWorld.NeutralRLGroup;
                newped.Weapons.Give(WeaponHash.AssaultRifle, 30, true, true);
                newped.AlwaysKeepTask = true;
                newped.Heading = LivelyWorld.AngleBetweenVectors(carpos,place);

                //Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, newped, "PROP_HUMAN_STAND_IMPATIENT", 5000, true);
                Goons.Add(newped);

                newped = World.CreatePed("G_M_Y_Lost_01", car.Position + (car.ForwardVector * -3) + (car.RightVector * 2));
                if (!LivelyWorld.CanWeUse(newped))
                {
                    Finished = true;
                    return;
                }
                newped.RelationshipGroup = LivelyWorld.NeutralRLGroup;
                newped.Weapons.Give(WeaponHash.AssaultRifle, 30, true, true);
                newped.AlwaysKeepTask = true;
                newped.Heading = LivelyWorld.AngleBetweenVectors(carpos, place);
                //Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, newped, "WORLD_HUMAN_SMOKING", 5000, true);
                Goons.Add(newped);


                //Buyer
                newped = null;
                newped = World.CreateRandomPed(car.Position + (car.ForwardVector * -7));
                if (!LivelyWorld.CanWeUse(newped))
                {
                    Finished = true;
                    return;
                }
                newped.RelationshipGroup = LivelyWorld.NeutralRLGroup;
                //newped.SetConfigFlag(17, true);
                newped.AlwaysKeepTask = true;
                
                Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, newped, "CODE_HUMAN_CROSS_ROAD_WAIT", 1000, true);
                newped.Heading = car.Heading;//LivelyWorld.AngleBetweenVectors(place, carpos);

                car.OpenDoor(VehicleDoor.Trunk, false, false);
                Buyer = newped;
                //PedsInvolved.Add(newped);
            }
            else
            {
                center = place;

                Model model = LivelyWorld.DrugCars[LivelyWorld.RandomInt(0, LivelyWorld.DrugCars.Count - 1)];
                while(!model.IsVehicle) model = LivelyWorld.DrugCars[LivelyWorld.RandomInt(0, LivelyWorld.DrugCars.Count - 1)];

                car = World.CreateVehicle(model, carpos, LivelyWorld.AngleBetweenVectors(place, carpos));
                if (!LivelyWorld.CanWeUse(car))
                {
                    Finished = true;
                    return;
                }
                if (!LivelyWorld.CarCanSeePos(car, car.Position + (car.ForwardVector * -10), 0)) car.Position = car.Position + (car.ForwardVector * 3);

                Ped newped = World.CreatePed("s_m_y_dealer_01", car.Position + (car.ForwardVector * -4));
                if (!LivelyWorld.CanWeUse(newped))
                {
                    Finished = true;
                    return;
                }
                newped.RelationshipGroup = LivelyWorld.NeutralRLGroup;
                newped.Weapons.Give(WeaponHash.Pistol,30,false,true);
                newped.AlwaysKeepTask = true;
                //newped.Task.StartScenario("WORLD_HUMAN_SMOKING", newped.Position);
                newped.Heading = car.Heading+180f;//LivelyWorld.AngleBetweenVectors(carpos, place);
                Dealer = newped; // PedsInvolved.Add(newped);


                newped = null;
                newped = World.CreateRandomPed(car.Position + (car.ForwardVector * -7));
                if (!LivelyWorld.CanWeUse(newped))
                {
                    Finished = true;
                    return;
                }
                newped.RelationshipGroup = LivelyWorld.NeutralRLGroup;
                //newped.SetConfigFlag(17, true);
                newped.AlwaysKeepTask = true;
                //newped.Task.StartScenario("WORLD_HUMAN_SMOKING", newped.Position);
                newped.Heading = car.Heading;//LivelyWorld.AngleBetweenVectors(place, carpos);
                Buyer = newped;

                car.OpenDoor(VehicleDoor.Trunk, false, false);
                // PedsInvolved.Add(newped);
            }

            Dealer.RelationshipGroup = DealerRLGroup;
            foreach (Ped ped in Goons) ped.RelationshipGroup = DealerRLGroup;

            if (LivelyWorld.DebugBlips)
            {
                Dealer.AddBlip();
                Dealer.CurrentBlip.Scale = 0.7f;
                Dealer.CurrentBlip.IsShortRange = true;
                Dealer.CurrentBlip.Name = "Deal";
            }
            if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios)
            {
                UI.Notify("~b~Spawned Deal");
            }

        }
        public void Process()
        {

            if (!StoleCar && Game.Player.Character.IsInVehicle(car))
            {
                StoleCar = true;

                if (GangDeal)
                {
                    Game.Player.Character.Weapons.Give(WeaponHash.SawnOffShotgun, 50, false, true);
                    Game.Player.Character.Weapons.Give(WeaponHash.AssaultRifle, 250, false, true);
                    Game.Player.Character.Weapons.Give(WeaponHash.Molotov, 8, false, true);

                    LivelyWorld.DisplayHelpTextThisFrame("You found some weapons inside the van.");
                }
                else
                {
                    int money = LivelyWorld.RandomInt(200, 1000);
                    Game.Player.Money+=money;

                    LivelyWorld.DisplayHelpTextThisFrame("You found some money in the glove compartment.");

                }
            }

            if(!Ruined)
            {
                if (TimeAlive < Game.GameTime)
                {
                    TaskSequence seq = new TaskSequence();

                    Function.Call(Hash.TASK_ENTER_VEHICLE, 0, car, 20000, -1, 1f, 1, 0);

                    Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, car, 15f, 1 + 2 + 8 + 32 + 128 + 256);
                    seq.Close();
                    Dealer.Task.PerformSequence(seq);
                    seq.Dispose();
                    if (LivelyWorld.CanWeUse(car)) car.CloseDoor(VehicleDoor.Trunk, false);
                    Finished = true;
                }
                if(LivelyWorld.isCopInRange(Dealer.Position,40) || LivelyWorld.isCopVehicleRange(Dealer.Position, 40)) Dealer.Task.FightAgainst(Game.Player.Character);
                if (Game.Player.Character.IsInRangeOf(Dealer.Position, 40f) && Dealer.IsOnScreen && World.GetRelationshipBetweenGroups(DealerRLGroup, Game.Player.Character.RelationshipGroup)!= Relationship.Hate)
                {
                    World.SetRelationshipBetweenGroups(Relationship.Hate, DealerRLGroup, Function.Call<int>(GTA.Native.Hash.GET_HASH_KEY, "PLAYER"));
                    World.SetRelationshipBetweenGroups(Relationship.Hate, Function.Call<int>(GTA.Native.Hash.GET_HASH_KEY, "PLAYER"), DealerRLGroup);
                }
                if (Dealer.IsInCombat)
                    {
                        if (!Buyer.IsFleeing) Buyer.Task.ReactAndFlee(Dealer);
                        Ruined = true;
                        if (Goons.Count > 0 && LivelyWorld.CanWeUse(car))
                        {
                            TaskSequence seq = new TaskSequence();
                            Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, car, 30f, 4 + 8 + 16 + 32);
                            seq.Close();
                            Dealer.Task.PerformSequence(seq);
                            seq.Dispose();
                        }
                    }
                if (!Game.Player.Character.IsInRangeOf(car.Position, Distance * 1.5f) || (!car.IsAlive || StoleCar)) Finished = true;
            }
            else if (!Game.Player.Character.IsInRangeOf(car.Position, 100f) || (!car.IsAlive || StoleCar)) Finished = true;
        }
        public void Clear()
        {
            if (LivelyWorld.CanWeUse(Dealer))
            {
                if (Dealer.CurrentBlip.Exists()) Dealer.CurrentBlip.Color = BlipColor.White;
                Dealer.IsPersistent = false;                
            }
            if (LivelyWorld.CanWeUse(Buyer)) Buyer.IsPersistent = false;
            foreach (Ped ped in Goons) if(LivelyWorld.CanWeUse(ped)) ped.IsPersistent = false;
            if (LivelyWorld.CanWeUse(car)) car.IsPersistent = false;
        }
    }
}
