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
    public enum TerrainType
    {
        Road,Offroad, Air,Water
    }
    public class TrafficSpawner
    {
        float despawnRange=150f;
        TerrainType Terrain;
        string AreaOrZone = "all";
        public string SourceVehicle = "all";
        int ExistingVehicles = 0;
         int CooldownTime = (LivelyWorld.RandomInt(1, 2) * 1000 * 60) + (LivelyWorld.RandomInt(1, 30) * 1000);
        int Prob = 50;
       public int Cooldown = 0;
        string Time = "all";
        Ped ped = null;
        Vehicle veh = null;
        public TrafficSpawner(string source, string timeframe, string area, TerrainType terrain, int freq, int prob)
        {
            SourceVehicle = source.ToLowerInvariant();
            if (freq < 0) freq = 0;
            CooldownTime = (freq * 1000 * 60) + (LivelyWorld.RandomInt(1, 30) * 1000);


            Prob = prob;
            Cooldown = Game.GameTime + CooldownTime;

            Terrain = terrain;
            if (Terrain == TerrainType.Air || Terrain == TerrainType.Water) despawnRange = 500f;
            if (area.Length > 0) AreaOrZone = area.ToLowerInvariant();
            if (timeframe.Length > 0) Time = timeframe;
            if(LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - added TrafficSpawner ("+source+", in "+area+")");
           if(LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify(SourceVehicle+ " spawner, timer:"+ (Cooldown- Game.GameTime) * 0.001f + "s");
        }

        public void Clear()
        {
            if (LivelyWorld.CanWeUse(veh)) veh.IsPersistent = false;
            if (LivelyWorld.CanWeUse(ped)) ped.IsPersistent = false;
        }
        public bool Process()
        {
            
            ExistingVehicles = 0;
            foreach (Vehicle v in LivelyWorld.AllVehicles) if (v.Model == SourceVehicle) ExistingVehicles++;

            if (ExistingVehicles > 4)
            {
                if (LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Too many vehicles of "+SourceVehicle+" already exist, spawn attempt aborted");
                return false;
            }
            if (Terrain == TerrainType.Water && !LivelyWorld.IsPlayerNearWater(200f)) return false;
            if (!LivelyWorld.CanWeUse(veh))
            {
                if (Cooldown < Game.GameTime)
                {
                    Vector3 PlayerPos = Game.Player.Character.Position;
                    if (AreaOrZone == "all" || LivelyWorld.IsInNamedArea(Game.Player.Character, AreaOrZone))
                    {
                        if (Time == "all" || (LivelyWorld.IsNightTime() && Time == "night") || (!LivelyWorld.IsNightTime() && Time == "day"))
                        {

                            if (LivelyWorld.RandomInt(0, 100) > Prob)
                            {
                                Cooldown = Game.GameTime + CooldownTime;
                                if (LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Probability check for " + SourceVehicle + " too low, resetting cooldown without spawning.");
                                return false;
                            }
                            else
                            {
                                if (LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Spawning a " + SourceVehicle + "");
                            }

                            Vector3 spawnpos = Vector3.Zero;
                            int patience = 0;
                          if( patience<3) //  while (Game.GameTime > patience+3000 && (spawnpos==Vector3.Zero || LivelyWorld.WouldPlayerNoticeChangesHere(spawnpos))) //WOULD_ENTITY_BE_OCCLUDED(Hash entityModelHash, float x, float y, float z, BOOL p4)
                            {                                
                                patience++;
                                switch (Terrain)
                                {
                                    case TerrainType.Road:
                                        {
                                            int p = 1;

                                            spawnpos = Vector3.Zero;

                                            while(p<50 &&  (spawnpos == Vector3.Zero || LivelyWorld.WouldPlayerNoticeChangesHere(spawnpos)))
                                            {
                                                p++;
                                                spawnpos=LivelyWorld.GenerateSpawnPos(Game.Player.Character.Position.Around(100 + (p * 10)), LivelyWorld.Nodetype.Road, false);
                                                Script.Yield();
                                            }
                                            break;
                                        }
                                    case TerrainType.Offroad:
                                        {
                                            int p = 1;

                                            while (p < 50 && (spawnpos == Vector3.Zero || LivelyWorld.WouldPlayerNoticeChangesHere(spawnpos)))
                                            {
                                                
                                                p++;
                                                spawnpos = LivelyWorld.GenerateSpawnPos(Game.Player.Character.Position.Around(100+(p*10)), LivelyWorld.Nodetype.Offroad, false);
                                                Script.Yield();
                                            }
                                            break;
                                        }
                                    case TerrainType.Air:
                                        {
                                            int traffic = 0;
                                            foreach (Vehicle v in LivelyWorld.AllVehicles) if (v.HeightAboveGround>20 && (v.Model.IsHelicopter || v.Model.IsPlane)) traffic++;
                                            if (traffic > 10)
                                            {
                                                if (LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - too much air traffic (>5) to spawn more.");
                                                return false;
                                            }
                                            if(new Model(SourceVehicle).IsHelicopter)
                                            {
                                                spawnpos = Game.Player.Character.Position.Around(LivelyWorld.RandomInt(300, 600));

                                            }
                                            else
                                            {
                                                spawnpos = Game.Player.Character.Position.Around(LivelyWorld.RandomInt(500, 800));
                                            }
                                            spawnpos = spawnpos + new Vector3(0, 0, LivelyWorld.RandomInt(50, 100));
                                            break;
                                        }
                                    case TerrainType.Water:
                                        {
                                            int traffic = 0;
                                            foreach (Vehicle v in LivelyWorld.AllVehicles) if (v.Model.IsBoat) traffic++;
                                            if (traffic > 10)
                                            {
                                                if (LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - too much boat traffic (> 5) to spawn more.");
                                                return false;
                                            }
                                            if (World.GetZoneNameLabel(Game.Player.Character.Position) == "OCEANA")
                                            {
                                                foreach(Vector3 v in LivelyWorld.OceanSpawns)
                                                {
                                                    if(Game.Player.Character.IsInRangeOf(v, despawnRange-100f) && !LivelyWorld.AnyVehicleNear(v,100f) && !LivelyWorld.WouldPlayerNoticeChangesHere(v))
                                                    {
                                                        spawnpos = v;
                                                        break;
                                                    }
                                                } 
                                            }
                                            else
                                            {
                                                spawnpos = LivelyWorld.GenerateSpawnPos(Game.Player.Character.Position.Around(200), LivelyWorld.Nodetype.Water, false);
                                            }
                                            break;
                                        }
                                }
                            }

                            if(spawnpos == Vector3.Zero)
                            {
                                if (LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - adequate spawn position not found, aborting traffic spawn.");
                                return false;
                            }

                            float angle = LivelyWorld.AngleBetweenVectors(Game.Player.Character.Position, spawnpos);
                            if(Terrain!= TerrainType.Road) angle =+ LivelyWorld.RandomInt(-90, 90);
                            veh = World.CreateVehicle(SourceVehicle, spawnpos,angle);
                            if (LivelyWorld.CanWeUse(veh))
                            {


                                veh.Position = veh.Position + (veh.RightVector * 2); //Make sure it spawns on the right side of the road, not the left (oncoming)
                                string model = null;
                                if (model == null || new Model(model).IsValid==false)
                                {
                                    ped= veh.CreateRandomPedOnSeat(VehicleSeat.Driver);
                                }
                                else
                                {
                                    ped = World.CreatePed(model, veh.Position.Around(5));
                                    ped.SetIntoVehicle(veh, VehicleSeat.Driver);
                                }

                                if (veh.Model.IsHelicopter || veh.Model.Hash == Game.GenerateHash("osprey"))
                                {

                                    veh.Position = veh.Position + new Vector3(0, 0, LivelyWorld.RandomInt(50, 100));
                                    List<Vector3> LandingTakeOff = new List<Vector3>();
                                    foreach (Vector3 takeoff in LivelyWorld.AmbientHeliLanding)
                                    {
                                        if (Game.Player.Character.IsInRangeOf(takeoff, 1000f))
                                        {
                                            LandingTakeOff.Add(takeoff);
                                        }
                                    }
                                    if (LandingTakeOff.Count>0 && LivelyWorld.RandomInt(0, 10)<30)
                                    {
                                        veh.Position = LandingTakeOff[LivelyWorld.RandomInt(0, LandingTakeOff.Count-1)];
                                        veh.LandingGear = VehicleLandingGear.Deployed;

                                    }
                                    else
                                    {
                                        Function.Call(Hash.SET_HELI_BLADES_FULL_SPEED, veh);
                                        Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, veh, 30f);
                                        veh.LandingGear = VehicleLandingGear.Retracted;

                                    }

                                }
                                else if (veh.Model.IsPlane)
                                {
                                    veh.Position = LivelyWorld.LerpByDistance(Game.Player.Character.Position, veh.Position, 500);
                                    if (veh.Model.GetDimensions().Y > 20f) veh.Position = veh.Position + new Vector3(0, 0, 200);
                                    if (veh.Model.GetDimensions().Y > 40f)
                                    {
                                        veh.Position = veh.Position + new Vector3(0, 0, 200);
                                        despawnRange = 2000f;

                                    }
                                    veh.LandingGear = VehicleLandingGear.Retracted;

                                    Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, veh, 30f);
                                }
                                else if(!veh.Model.IsBoat)
                                {
                                    LivelyWorld.MoveEntitytoNearestRoad(veh, true, true);
                                }



                                if (LivelyWorld.DebugBlips)
                                {
                                    veh.AddBlip();

                                    if (veh.Model.IsPlane) veh.CurrentBlip.Sprite = BlipSprite.Plane;
                                    if (veh.Model.IsBike) veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                    if (veh.Model.IsBoat) veh.CurrentBlip.Sprite = BlipSprite.Boat;
                                    if (veh.Model.IsCar) veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    if (veh.Model.IsHelicopter) veh.CurrentBlip.Sprite = BlipSprite.Helicopter;

                                    veh.CurrentBlip.Color = BlipColor.White;
                                    veh.CurrentBlip.IsShortRange = true;
                                    veh.CurrentBlip.Name = veh.FriendlyName;
                                }
                                AmbientDrive();
                                veh.Alpha = 0;
                                veh.Driver.Alpha = 0;
                                LivelyWorld.FadeIn.Add(veh);
                                LivelyWorld.FadeIn.Add(veh.Driver);
                                if (LivelyWorld.CarrierVehicles.Contains(veh.Model))
                                {
                                    Model vehicle = LivelyWorld.RandomNormalVehicle();
                                    Vehicle cargo = World.CreateVehicle(vehicle, veh.Position + (veh.UpVector * 5f));

                                    if (LivelyWorld.CanWeUse(cargo)) LivelyWorld.Attach(veh, cargo);
                                    LivelyWorld.TemporalPersistence.Add(cargo);
                                }
                                if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~" + SourceVehicle + " spawned (and entered cooldown)");// if(LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) 

                                if (!veh.SirenActive)
                                {
                                      veh.Driver.IsPersistent = false;
                                    veh.IsPersistent = false;

                                }

                            }
                            else return false;
                            
                            return true;
                        }
                    }
                }
                //else if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~Spawner - " + SourceVehicle + " - " + AreaOrZone + " - " + Time + " is on cooldown");
            }
            else if(LivelyWorld.CanWeUse(veh))
            {                
                if (!veh.IsInRangeOf(Game.Player.Character.Position, despawnRange) && !LivelyWorld.WouldPlayerNoticeChangesHere(veh.Position))
                {
                    if (veh.CurrentBlip.Exists()) veh.CurrentBlip.Color = BlipColor.White;
                    veh.MarkAsNoLongerNeeded();
                    veh = null;
                    if (LivelyWorld.CanWeUse(ped))
                    {
                        ped.MarkAsNoLongerNeeded();
                        ped = null;
                    }
                    if (Cooldown < Game.GameTime) Cooldown = Game.GameTime + CooldownTime;
                }
                else
                {
                    if (veh.Speed < 3f && LivelyWorld.CanWeUse(ped))
                    {
                      //  AmbientDrive();
                    }
                }
                if (Cooldown < Game.GameTime) Cooldown = Game.GameTime + 40000;
            }
            return false;
        }

        public void AmbientDrive()
        {
            //if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Tasked " + veh.FriendlyName + " to drive, speed " + veh.Speed);
            if (!LivelyWorld.CanWeUse(ped) || ped.IsInCombat) return;

            TaskSequence seq = new TaskSequence();

            if (veh.Model.IsHelicopter || veh.Model.Hash == Game.GenerateHash("osprey"))
            {
                if (veh.HeightAboveGround < 10f)
                {
                    ped.Position = veh.Position.Around(7f);
                    Function.Call(Hash.TASK_ENTER_VEHICLE, 0, veh, 20000, -1, 1f, 1, 0);
                    Function.Call(Hash.TASK_PAUSE, 0, LivelyWorld.RandomInt(2, 4) * 1000);
                }
                Vector3 pos = veh.Position + (veh.ForwardVector * 5000) + (veh.RightVector * LivelyWorld.RandomInt(-50, 50));

                float speed = 20f;
                if (veh.Model.Hash == Game.GenerateHash("osprey")) speed = 40f;

                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, 0, veh.Handle, pos.X, pos.Y, pos.Z, speed, 0, veh.Model, 6, 60, 10.0);

                //veh.SetHeliYawPitchRollMult(0.2f);
            }
            else if (veh.Model.IsPlane)
            {
                Vector3 pos = LivelyWorld.LerpByDistance(veh.Position, Game.Player.Character.Position, 5000);//veh.Position + (veh.ForwardVector * 300);

                Function.Call(Hash.TASK_PLANE_MISSION, 0, veh, 0, 0, pos.X, pos.Y, pos.Z, 4, 100f, 0f, 90f, 0, 200f);
            }
            else if (veh.Model.IsBoat)
            {
                //Function.Call(Hash.TASK_HELI_MISSION, RecieveOrder.Handle, RecieveOrder.CurrentVehicle.Handle, 0, 0, safepos.X, safepos.Y, safepos.Z, 20, 40f, 1f, 36f, 15, 15, -1f, 1);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, veh, 10f, 1 + 2 + 8 + 16 + 32 + 128 + 256);
            }
            else
            {

                int drivingstyle = 1 + 2 + 8 + 16 + 32 + 128 + 256;
                float speed = 20f;
                if (veh.HasSiren && LivelyWorld.CurrentlyAllowedEvents.Contains(EventType.EmergencyRushing) && !LivelyWorld.BobCatSecurity.Contains(veh.Model) && veh.Model!="coroner")
                {
                    drivingstyle = 4 + 8 + 16 + 32;
                    speed = 25f;
                    veh.SirenActive = true;
                }
                else if(Terrain == TerrainType.Offroad && LivelyWorld.RandomInt(0,10)<=5)
                {
                    drivingstyle = 4 + 8 + 16 + 32 + 128;
                    speed = 40f;
                }
                Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, veh, speed, drivingstyle);

            }
            ped.Task.PerformSequence(seq);
            ped.BlockPermanentEvents = false;
        }
    }
}
