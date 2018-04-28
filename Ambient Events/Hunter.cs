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
    public class Hunter
    {
        public bool Finished = false;
        //List<Ped> Hunters = new List<Ped>();
        public Ped HunterPed;
        public Vehicle HunterCar;
        public Ped HunterDog;
        public static int HunterRLGroup = World.AddRelationshipGroup("LWHUNTER");
        public float DespawnRange = 300f;
        public int TimesTold = 0;
        public int Kills = 0;
        public bool PlayedCall = false;

        public bool Notified = false;
        //public Ped target=null;
        public Hunter(Vector3 place)
        {

            Function.Call(Hash.REQUEST_MISSION_AUDIO_BANK, "SCRIPT\\HUNTING_2_ELK_CALLS", 0, -1);

            if (LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Called for Hunter Event");
            /*
            if( place == Vector3.Zero)
            {
                for (int i = 0; i < 50; i++)
                {
                    if (place == Vector3.Zero)
                    {
                        place = LivelyWorld.GenerateSpawnPos(fix, LivelyWorld.Nodetype.Offroad, false);// World.GetSafeCoordForPed(Game.Player.Character.Position.Around(100), false);
                    }
                }
            }*/



            if (LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Found suitable place, spawning Hunter");
            HunterPed = World.CreatePed(PedHash.Hunter, place);
            Vector3 pos = HunterPed.Position;


            if (LivelyWorld.DebugBlips)
            {
                HunterPed.AddBlip();
                HunterPed.CurrentBlip.Sprite = BlipSprite.Hunting;
                HunterPed.CurrentBlip.Color = BlipColor.Yellow;
                HunterPed.CurrentBlip.IsShortRange = true;
                HunterPed.CurrentBlip.Name = "Hunter";

            }
            HunterPed.AlwaysKeepTask = true;
            HunterPed.Accuracy = 100;
            //if(Game.Player.Character.Position.DistanceTo(place)>50F) DespawnRange = Game.Player.Character.Position.DistanceTo(place)*1.5f;
            //HunterPed.FiringPattern = FiringPattern.SingleShot;
            HunterPed.Weapons.Give(WeaponHash.SniperRifle, 999, true, true);
            HunterPed.Weapons.Current.SetComponent(WeaponComponent.AtArSupp02, true);

             if(LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Set up");
            World.SetRelationshipBetweenGroups(Relationship.Respect, HunterRLGroup, Game.GenerateHash("CIVMALE"));
            World.SetRelationshipBetweenGroups(Relationship.Respect, Game.GenerateHash("CIVMALE"), HunterRLGroup);

            World.SetRelationshipBetweenGroups(Relationship.Respect, Game.GenerateHash("CIVFEMALE"), HunterRLGroup);
            World.SetRelationshipBetweenGroups(Relationship.Respect, HunterRLGroup, Game.GenerateHash("CIVFEMALE"));

            World.SetRelationshipBetweenGroups(Relationship.Respect, Game.GenerateHash("COP"), HunterRLGroup);
            World.SetRelationshipBetweenGroups(Relationship.Respect, HunterRLGroup, Game.GenerateHash("COP"));


            World.SetRelationshipBetweenGroups(Relationship.Hate, HunterRLGroup, Game.GenerateHash("WILD_ANIMAL"));
            //World.SetRelationshipBetweenGroups(Relationship.Hate, Game.GenerateHash("WILD_ANIMAL"), HunterRLGroup);
            //World.SetRelationshipBetweenGroups(Relationship.Hate, Game.GenerateHash("DEER"), HunterRLGroup);        
            World.SetRelationshipBetweenGroups(Relationship.Hate, HunterRLGroup, Game.GenerateHash("DEER"));
             if(LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - animal relationships set");
            //World.SetRelationshipBetweenGroups(Relationship.Hate, HunterRLGroup, Game.GenerateHash("PLAYER"));
            //World.SetRelationshipBetweenGroups(Relationship.Hate, Game.GenerateHash("PLAYER"), HunterRLGroup);

            //Function.Call(Hash.SET_PED_STEALTH_MOVEMENT, HunterPed, 1,0);
            //Function.Call(Hash.SET_PED_TARGET_LOSS_RESPONSE, HunterPed, 3);

            if (LivelyWorld.RandomInt(0, 10) <= 5)
            {
                HunterCar = World.CreateVehicle(LivelyWorld.HuntingTrucks[LivelyWorld.RandomInt(0,LivelyWorld.HuntingTrucks.Count-1)], HunterPed.Position+(HunterPed.ForwardVector*-8)); //World.GetNextPositionOnStreet(HunterPed.Position)
                //LivelyWorld.MoveEntitytoNearestRoad(HunterCar,false,true);
                HunterCar.Position = HunterCar.Position + (HunterCar.RightVector * 3);


                if (LivelyWorld.DebugBlips)
                {
                    HunterCar.AddBlip();
                    HunterCar.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                    HunterCar.CurrentBlip.Color = BlipColor.Yellow;
                    HunterCar.CurrentBlip.IsShortRange = true;
                    HunterCar.CurrentBlip.Name = "Hunter's car";

                }
            }
            else
            {
                HunterDog = World.CreatePed(PedHash.Retriever,HunterPed.Position.Around(0));
                HunterDog.RelationshipGroup = HunterRLGroup;
                HunterDog.AlwaysKeepTask = true;
                HunterDog.BlockPermanentEvents = true;
            }




            HunterPed.RelationshipGroup = HunterRLGroup;
            int patience = 0;

            while (patience<500 && (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "move_ped_crouched") || !Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, "move_ped_crouched_strafing")))
            {
                patience++;
                Function.Call(Hash.REQUEST_ANIM_SET, "move_ped_crouched");
                Function.Call(Hash.REQUEST_ANIM_SET, "move_ped_crouched_strafing");
            }
            Script.Wait(300);
            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, HunterPed, "move_ped_crouched", 1048576000);
            Function.Call(Hash.SET_PED_STRAFE_CLIPSET, HunterPed, "move_ped_crouched_strafing");

            Function.Call(Hash.SET_PED_SEEING_RANGE, HunterPed, 150f);
            Function.Call(Hash.SET_PED_VISUAL_FIELD_MIN_ELEVATION_ANGLE, HunterPed, -40f);
            Function.Call(Hash.SET_PED_VISUAL_FIELD_MAX_ELEVATION_ANGLE, HunterPed, 5f);
            Function.Call(Hash.SET_PED_SHOOT_RATE, HunterPed, 50);
            Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, HunterPed, 0);
            Function.Call(Hash.SET_PED_TARGET_LOSS_RESPONSE, HunterPed, 0);

            Function.Call(Hash.TASK_WANDER_IN_AREA, HunterPed, pos.X, pos.Y, pos.Z, 100f, 2f, 3f);
            //Function.Call(Hash.TASK_WANDER_STANDARD, HunterPed, 20f,0);
             if(LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - spawn finished");
        }
        public void HelpHunter()
        {

            TimesTold++;
            if (TimesTold < 5)
            {
                Vector3 pos = World.GetSafeCoordForPed(Game.Player.Character.Position+(Game.Player.Character.ForwardVector*40), false);
                HunterPed.Task.RunTo(pos);
            }
            else
            {
                TimesTold = 0;
                LivelyWorld.AddQueuedConversation("~b~[Hunter]~w~You're not very good as a Spotter.");
            }

        }
        int patience=0;
        public void Process()
        {
            if ((!Game.Player.Character.IsInRangeOf(HunterPed.Position, DespawnRange) || !HunterPed.IsAlive) && !Finished) Finished = true;
            if(LivelyWorld.CanWeUse(HunterDog) && !HunterDog.IsInRangeOf(HunterPed.Position,5f) && HunterDog.IsStopped) Function.Call(Hash.TASK_GO_TO_ENTITY, HunterDog,HunterPed , -1, 2f, 1f, 0f, 0);

            if (!Notified && Game.Player.Character.IsStopped && Game.Player.Character.IsInRangeOf(HunterPed.Position, 8f))
            {
                Notified = true;
                LivelyWorld.AddQueuedConversation("~b~[Hunter]~w~: Hey man wandering alone in the woods, why don't you give me a hand here. If you spot any animal, tell me, ok?");
                LivelyWorld.AddQueuedHelpText("If you spot prey, press ~INPUT_CONTEXT~ to tell the ~b~Hunter~w~.");
                World.SetRelationshipBetweenGroups(Relationship.Like, HunterRLGroup, Game.GenerateHash("PLAYER"));
                World.SetRelationshipBetweenGroups(Relationship.Like, Game.GenerateHash("PLAYER"), HunterRLGroup);
            }
            if (!HunterPed.IsInCombat && HunterPed.IsStopped)
            {
                patience++;
                if (!PlayedCall && LivelyWorld.RandomInt(0, 10) <= 5)
                {
                    Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "PLAYER_CALLS_ELK_MASTER", HunterPed, 0, 0, 0);
                    PlayedCall = true;
                }
            }
            else if (PlayedCall) PlayedCall = false;
            if (HunterPed.IsInCombat)
            {
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (ped.HeightAboveGround < 3f && !ped.IsHuman && !ped.IsAlive && HunterPed.IsInCombatAgainst(ped))
                    {
                        Function.Call(Hash._0x0DC7CABAB1E9B67E, ped, true); //Load Collision

                        //target = ped;
                        TaskSequence seq = new TaskSequence();

                        //Function.Call(Hash._PLAY_AMBIENT_SPEECH1, 0, "KILLED_ALL", "SPEECH_PARAMS_FORCE");
                        Function.Call(Hash.TASK_GO_TO_ENTITY, 0, ped, -1, 1f, 3f, 0f, 0);
                        Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, 0, false);

                        seq.Close();
                        HunterPed.Task.PerformSequence(seq);
                        seq.Dispose();
                        if (TimesTold > 0)
                        {
                            LivelyWorld.AddQueuedConversation("~b~[Hunter]~w~: Thanks, man.~n~~g~$10~w~ for your help.");
                            Game.Player.Money += 10;
                            TimesTold = 0;
                        }
                        break;
                    }
                }
            }
            else
            {                
                if (patience > 5)
                {
                    //HunterPed.RelationshipGroup = HunterRLGroup;
                    foreach (Ped ped in World.GetNearbyPeds(HunterPed, 5f))
                        if (ped.IsDead)
                        {
                            if (Game.Player.Character.IsInRangeOf(HunterPed.Position, 20f))
                            {
                                Kills++;

                                LivelyWorld.AddQueuedConversation("~b~[Hunter]~w~: That's one for the pot!");
                                if (Kills > 4 && LivelyWorld.CanWeUse(HunterCar)) { LivelyWorld.AddQueuedConversation("~b~[Hunter]~w~:That's enough for today."); Finished = true; } else if (Kills > 1) LivelyWorld.AddQueuedConversation("~b~[Hunter]~w~: " + Kills + " already!");
                            }
                            if (LivelyWorld.CanWeUse(HunterCar))
                            {
                                Function.Call(GTA.Native.Hash.SET_ENTITY_LOAD_COLLISION_FLAG, ped, true); //Load Collision

                                Function.Call(Hash.SET_ENTITY_RECORDS_COLLISIONS, ped, true); //Load Collision

                                ped.IsPersistent = true;
                                LivelyWorld.TemporalPersistence.Add(ped);
                                ped.Position = HunterCar.Position + (HunterCar.ForwardVector * -2)+(HunterCar.UpVector*2);
                                Function.Call(Hash.SET_PED_TO_RAGDOLL, ped, 2000, 2000, 3, true, true, false);
                                Function.Call(Hash.CREATE_NM_MESSAGE, 1151);
                                Function.Call(Hash.GIVE_PED_NM_MESSAGE, ped, true);



                            }
                            else ped.Delete();
                            break;
                        }
                    HunterPed.BlockPermanentEvents = false;
                    Vector3 pos = HunterPed.Position;
                    Function.Call(Hash.TASK_WANDER_IN_AREA, HunterPed, pos.X, pos.Y, pos.Z, 100f, 2f, 3f);
                    patience = 0;
                    TimesTold = 0;
                }

            }
        }
        public void Clear()
        {
            if (LivelyWorld.CanWeUse(HunterPed))
            {
                HunterPed.RelationshipGroup = Game.GenerateHash("CIVMALE");

                if (HunterPed.CurrentBlip.Exists()) HunterPed.CurrentBlip.Color = BlipColor.White;
                Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, HunterPed, 0.0f);
                Function.Call(Hash.RESET_PED_STRAFE_CLIPSET, HunterPed);
                HunterPed.IsPersistent = false;

                if (LivelyWorld.CanWeUse(HunterCar))
                {
                    LivelyWorld.TemporalPersistence.Add(HunterCar);
                    TaskSequence seq = new TaskSequence();
                    Function.Call(Hash.TASK_PAUSE, 0, LivelyWorld.RandomInt(2, 4) * 1000);

                    Function.Call(Hash.TASK_ENTER_VEHICLE, 0, HunterCar, 20000, -1, 1f, 1, 0);
                    Function.Call(Hash.TASK_PAUSE, 0, LivelyWorld.RandomInt(2, 4) * 1000);
                    Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, HunterCar, 30f, 1 + 2 + 4 + 8 + 16 + 32);
                    seq.Close();
                    HunterPed.Task.PerformSequence(seq);
                    seq.Dispose();
                    HunterCar.IsPersistent = false;
                }

            }
            if (LivelyWorld.CanWeUse(HunterDog))
            {

                HunterDog.IsPersistent = false;
            }
        }
    }
}
