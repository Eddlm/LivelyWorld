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
    public class Replacer
    {

        string EventName;
        string AreaOrZone = "all";
        public string SourceVehicle = "all";
        string TargetVehicle;
        int VehiclesReplaced = 0;
        int Cooldown = 0;
        string Time = "all";
        bool ShouldBeTuned = false;
        public Replacer(string source, string target, bool tuned, string timeframe, string area, string eventname)
        {
            EventName = eventname;
            if (source.Length > 0) SourceVehicle = source.ToLowerInvariant(); else SourceVehicle = "all";
            TargetVehicle = target.ToLowerInvariant();
            ShouldBeTuned = tuned;

            if (area.Length > 0) AreaOrZone = area.ToLowerInvariant();
            if (timeframe.Length > 0) Time = timeframe;

             if(LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - added replacer ("+source+">"+target+")");
        }


        public bool Process()
        {
            if (VehiclesReplaced > 2)
            {
               if(LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~"+TargetVehicle+ " replacement entered cooldown");
                Cooldown = Game.GameTime + 60000;
                VehiclesReplaced = 0;
                return false;
            }
            if (Cooldown < Game.GameTime)
            {
                //if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify(SourceVehicle + " - " + TargetVehicle + " - " + AreaOrZone + " - " + Time + " turn");
                Vector3 PlayerPos = Game.Player.Character.Position;

                //UI.Notify(World.GetZoneName(PlayerPos).ToLowerInvariant()+"-" + AreaOrZone);
                if (AreaOrZone == "all" || LivelyWorld.IsInNamedArea(Game.Player.Character, AreaOrZone))
                {
                    //if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify(SourceVehicle + " - "+AreaOrZone);

                    if (Time == "all" || (LivelyWorld.IsNightTime() && Time == "night") || (!LivelyWorld.IsNightTime() && Time == "day"))
                    {
                        //if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~g~ correct timeframe");


                        //if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~g~" + TargetVehicle + " - getting all vehicles");

                        foreach (Vehicle v in LivelyWorld.AllVehicles)
                        {//
                            if (LivelyWorld.CanWeUse(v) && !v.IsPersistent && (!v.IsOnScreen || !LivelyWorld.WouldPlayerNoticeChangesHere(v.Position)) && !LivelyWorld.BlacklistedVehicles.Contains(v)  && !Game.Player.Character.IsInRangeOf(v.Position, 10f) && !LivelyWorld.LastDriverIsPed(v, Game.Player.Character))
                            {
                                //if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Got a "+v.FriendlyName);
                                if (v.ClassType== (VehicleClass)Function.Call<int>(Hash.GET_VEHICLE_CLASS_FROM_NAME, Game.GenerateHash(TargetVehicle)) && (SourceVehicle == "all" || v.Model == Game.GenerateHash(SourceVehicle) || v.FriendlyName.ToString().ToLowerInvariant() == SourceVehicle.ToLowerInvariant() ))
                                {
                                    if(LivelyWorld.DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - replacing " + SourceVehicle + " with a " + TargetVehicle + "");
                                    if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~g~" + SourceVehicle + " replaced with " + TargetVehicle);
                                    LivelyWorld.ReplaceVehicle(v, TargetVehicle, ShouldBeTuned);
                                    VehiclesReplaced++;
                                    return true;
                                }

                            }
                        }
                    }
                }
            }
            else if (LivelyWorld.Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~"+SourceVehicle + " - " + TargetVehicle + " - " + AreaOrZone + " - " + Time + " is on cooldown");
            return false;
        }
    }
}
