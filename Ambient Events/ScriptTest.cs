using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Xml;
using Lively_World;
using System.IO;
using System.Diagnostics;

enum EmergencyType
{
    FIRETRUCK = 0,// 0
    AMBULANCE,
    POLICE,

};

enum WeatherType
{
    EXTRA_SUNNY = -1750463879,
    CLEAR = 916995460,
    NEUTRAL = -1530260698,
    SMOG = 282916021,
    FOGGY = -1368164796,
    OVERCAST = -1148613331,
    CLOUDS = 821931868,
    CLEARING = 1840358669,
    RAIN = 1420204096,
    THUNDER = -1233681761,
    SNOW = -273223690,
    BLIZZARD = 669657108,
    LIGHT_SNOW = 603685163,
    X_MAS = -1429616491
};


/// <summary>
/// 
/// To do:
/// Have a hidden spawn for the Carjacker
/// add FadeIn to most suff that spawns
/// 
/// Fixes:
/// Fixed peds driving out too fast and ignoring traffic lights
/// Made flatbeds only spawn cargo if they're slow or stopped
/// Fixed the spawner patience
/// Fixed the Replacer, it was replacing persistent vehicles too
/// </summary>


//Events are big and scarce. Scenarios are small, happen frequently and serve as details more than anything.
public enum EventType
{

    Carjacker,
    EmergencyRushing,
    Tow,
    Racer,
    Deal,
    Hunter,
    GangDriveby,
};

public enum DebugLevel
{
    None, EventsAndScenarios, Everything
}

public enum ScenarioType
{
    PoliceCarCarryingBike,
    CarCarryingBike,
    DriverOvertake,
    PlayerCoolCarPhoto,
    AmbientTuner,
    DriverRushing,
    Taxi,
    ImprovedTowtrucks,
    AnimalTrophies,
    VehicleInteraction,
    ImprovedFreight,
    ImprovedFlatbeds,
    BarnFinds,
    StoppedAtLightsInteraction,
    LoudRadio,
    StrippedCar,
}
public class LivelyWorld : Script
{

    int Generator = -1;
    int EmergencyScore = 0;
    int Crimescore = 0;

    public string ScriptName = "Lively World";
    string ScriptVer = "0.6";

    public static bool DebugOutput = false;

    public static DebugLevel Debug = DebugLevel.None;
    public static bool DebugBlips = false;

    public static int ReplacerTime = 0;
    public static int VehMonitorTime;
    public static int BlackistedEventsTime;
    int SmallEventCooldownTime = Game.GameTime + 30000;
    int EventCooldownTime = Game.GameTime + 60000;
     int Interval = 1000;


    Ped CoolCarPed = null;

    public static List<Vector3> AmbientHeliLanding = new List<Vector3>
    {
        new Vector3 (-144,-593,211),
    };
    public static List<EventType> BlacklistedEvents = new List<EventType>();

    public static List<ScenarioType> ScenarioFlow = new List<ScenarioType>();
    public static List<EventType> Eventflow = new List<EventType>();

    public static List<VehicleHash> HuntingTrucks = new List<VehicleHash>
        {
            VehicleHash.Picador,VehicleHash.Dubsta3,VehicleHash.Bison,VehicleHash.Rebel, VehicleHash.Rebel2,VehicleHash.Sadler,VehicleHash.BobcatXL,VehicleHash.Sandking,VehicleHash.Sandking2
        };

    public static List<EventType> CurrentlyAllowedEvents = new List<EventType>();

    public static List<ScenarioType> CurrentlyAllowedScenarios = new List<ScenarioType>();
    public static List<string> GangAreas = new List<string> { "CHAMH", "DAVIS", "RANCHO", "CYPRE" };
    public static int BlackistedImportantEventsTime;
    public static int BlackistedImportantEventsCooldown;

    //static public  List<EventType> BlacklistedImportantEvents = new List<EventType>();

    //int FirstEventCooldown = Game.GameTime+RandomInt(1,4);


    public static List<EventType> DisabledEvents = new List<EventType>();
    public static List<ScenarioType> DisabledScenarios = new List<ScenarioType>();


    Vector3 NoEventsHere = Vector3.Zero;
    Vector3 NoEventsHereFar = Vector3.Zero;


    int BlacklistCooldown = Game.GameTime; //30 secs

    //Settings
    bool VehicleReplacer = true;
    bool TrafficInjector = true;

    bool TruckRespray = true;


    float InteractionRange = 100f;
    float SpawnerRange = 100f;

    int CriminalEventProb = 10;
    int AccidentEventProb = 30;

    public static List<Entity> FadeIn = new List<Entity>();


    public static int CriminalRelGroup = World.AddRelationshipGroup("CriminalRelGroup");
    public static int NeutralRLGroup = World.AddRelationshipGroup("LWNEUTRAL");
    public static int BallasRLGroup = World.AddRelationshipGroup("lwballas");
    public static int EventFrecuency = 100;
    public static int EventCooldown = 60000;

    public Vector3 CityCenter = new Vector3(-53, -878, 40);
    public Vector3 BennysMotorworks = new Vector3(-184, -1297, 30);

    public static List<Model> DrugCars = new List<Model> { "blista", "blista2", "moonbeam", "glendale", "dukes", "cavalcade", "rhapsody", "blade", "faction2", "retinue", "picador", "emperor", "voodoo", "regina" };

    public static List<Model> Respraymodels = new List<Model> { "mule", "mule2", "benson", "packer", "hauler", "pounder", "phantom", "roadkiller" };
    public static List<string> BlacklistedAreas = new List<string> { "golf", "armyb", "jail", "airp" };

    public static List<string> NearBeachAreas = new List<string> { "paleto", "delbe", "delpe", "delsol", "vcana", "", "" };

    public static List<Replacer> ReplacersList = new List<Replacer>();
    public static List<TrafficSpawner> TrafficSpawnerList = new List<TrafficSpawner>();

    public static List<TaxiEvent> Taxis = new List<TaxiEvent>();
    public static List<DrugDeal> DrugDeals = new List<DrugDeal>();
    public static List<Hunter> Hunters = new List<Hunter>();
    public static List<Entity> TemporalPersistence = new List<Entity>();


    public static Vehicle[] AllVehicles;
    //public static List<Vehicle> AllVehicles = new List<Vehicle>();
    //public static List<Vehicle> MonitoredVehicles = new List<Vehicle>();
    public static List<Ped> AllPeds = new List<Ped>();
    public static List<Model> MonitoredModels = new List<Model>();

    public static List<Prop> Wrecks = new List<Prop>();
    public static List<Model> WreckModels = new List<Model> {"prop_rub_carwreck_1",
"prop_rub_carwreck_2",
"prop_rub_carwreck_3",
"prop_rub_carwreck_4",
"prop_rub_carwreck_5",
"prop_rub_carwreck_6",
"prop_rub_carwreck_7",
"prop_rub_carwreck_8",
"prop_rub_carwreck_9",

"prop_rub_carwreck_10",
"prop_rub_carwreck_11",
"prop_rub_carwreck_12",

"prop_rub_carwreck_13",
"prop_rub_carwreck_14",
"prop_rub_carwreck_15",

"prop_rub_carwreck_16",
"prop_rub_carwreck_17",
    };
    int WrecksChecker = 0;
    public static List<Model> WreckCarModels = new List<Model>();
    public static List<Model> LostModels = new List<Model> {"G_M_Y_Lost_01",
"G_M_Y_Lost_02",
"G_M_Y_Lost_03",
"G_F_Y_Lost_01" };
    public static List<Model> BallasModels = new List<Model>
    {
        "G_F_Y_Ballas_01","G_M_Y_BallaEast_01","G_M_Y_BallaOrig_01","G_M_Y_BallaSout_01"
    };
    public static List<Model> FamiliesModels = new List<Model>
    {
        "A_M_M_OG_Boss_01","G_F_Y_Families_01","G_M_Y_FamCA_01","G_M_Y_FamDNF_01","G_M_Y_FamFOR_01"
    };
    public static List<Model> VagosModels = new List<Model>
    {
        "A_M_Y_MexThug_01","G_M_Y_MexGoon_01","G_M_Y_MexGoon_02","G_M_Y_MexGoon_03","G_F_Y_Vagos_01","mp_m_g_vagfun_01"
    };

    public static List<Vehicle> BlacklistedVehicles = new List<Vehicle>();
    public static List<Model> BlacklistedModels = new List<Model>();

    public static List<string> NormalVehicleModel = new List<string>
    {
    "baller",
    "baller2",
    "blista",
    "cavalcade2",
    "daemon",
    "dubsta",
    "f620",
    "felon",
    "fugitive",
    };
    public static List<Model> BobCatSecurity = new List<Model>
    {
        "bsgranger","bsfugitive","bspony"
    };
    public static List<Vector3> OceanSpawns = new List<Vector3> {

        new Vector3 (-2488,-1778,0),
 new Vector3(-3164,-483,3),
 new Vector3(-4168,1818,1),
 new Vector3(-3968,3693,0),
 new Vector3(-2272,6191,2),
 new Vector3(-1041,7789,0),
 new Vector3(1605,7623,2),
 new Vector3(3721,7004,1),
 new Vector3(4548,5168,0),
 new Vector3(4779,2655,1),
 new Vector3(4671,-255,0),
 new Vector3(3587,-3036,0),
 new Vector3(1935,-4183,2),
 new Vector3(-1539,-4879,2),
 new Vector3(-3880,-2390,1),
};
    public List<Model> Bennys = new List<Model>
    {

    };

    /*
        "comet",
    "vacca2",
    "dominator3",
    "elegy",
    "elegy4",
    "minivan2",
    "faction3",
        "faction2",
    "banshee2",
    "sultanrs",
    "specter2",
        */
    public List<Model> Racecars = new List<Model>
    {

    };

    public static float AngleBetweenVectors(Vector3 vec, Vector3 vec2)
    {
        Vector3 h = vec - vec2;
        return h.ToHeading();
        //return Function.Call<float>(Hash.GET_ANGLE_BETWEEN_2D_VECTORS, vec.X, vec.Y, vec2.X, vec2.Y);
    }
    public static List<Model> RacerModel = new List<Model>
        {
        "a_m_m_eastsa_01","a_m_m_eastsa_02","a_m_m_malibu_01","a_m_m_mexcntry_01","a_m_m_mexlabor_01","a_m_m_og_boss_01","a_m_m_polynesian_01","a_m_m_soucent_01","a_m_m_soucent_03",
        "a_m_m_soucent_04","a_m_m_stlat_02","s_m_m_bouncer_01","s_m_m_lifeinvad_01","u_m_m_aldinapoli","u_m_m_bikehire_01","u_m_m_filmdirector","u_m_m_rivalpap","u_m_m_willyfist",
        "u_m_y_baygor","u_m_y_chip","u_m_y_cyclist_01","u_m_y_fibmugger_01","u_m_y_guido_01","u_m_y_gunvend_01","u_m_y_hippie_01","u_m_y_paparazzi","u_m_y_party_01","u_m_y_sbike",
        "u_m_y_tattoo_01",
    };
    public LivelyWorld()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        LoadSettings();
        World.SetRelationshipBetweenGroups(Relationship.Hate, CriminalRelGroup, Function.Call<int>(GTA.Native.Hash.GET_HASH_KEY, "COP"));
        World.SetRelationshipBetweenGroups(Relationship.Hate, Function.Call<int>(GTA.Native.Hash.GET_HASH_KEY, "COP"), CriminalRelGroup);



        World.SetRelationshipBetweenGroups(Relationship.Hate, RacersRLGroup, Game.GenerateHash("COP"));
        World.SetRelationshipBetweenGroups(Relationship.Neutral, RacersRLGroup, Game.GenerateHash("PLAYER"));
        NoEventsHereFar = Game.Player.Character.Position;
        NoEventsHere = Game.Player.Character.Position;


        File.WriteAllText(@"scripts\LivelyWorldDebug.txt", "Script started -" + DateTime.Now + "-GameVer " + Game.Version.ToString());

        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Debug output is ON"); else File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Debug output is OFF");

    }
    public static bool AnyVehicleNear(Vector3 pos, float radius)
    {
        return Function.Call<bool>(Hash.IS_ANY_VEHICLE_NEAR_POINT, pos.X, pos.Y, pos.Z, radius);
    }

    void LogThis(string text, bool ToFile, bool ToNotification)
    {
        if (ToFile) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - "+ text);
        if (ToNotification) UI.Notify( "SpawnAnimalTrophy()");

    }
    void SpawnAnimalTrophy()
    {
        LogThis("Trying AnimalTrophy", DebugOutput, Debug >= DebugLevel.Everything);
        foreach (Vehicle veh in AllVehicles)
        {
            if (CanWeUse(veh) && !DecorExistsOn("LWIgnore", veh) && HuntingTrucks.Contains((VehicleHash)veh.Model.Hash) &&
                veh.IsStopped && veh.IsInRangeOf(Game.Player.Character.Position, InteractionRange) &&
                !BlacklistedVehicles.Contains(veh) )
            {
                veh.IsPersistent = true;
                if ((VehicleHash)veh.Model.Hash == VehicleHash.BobcatXL && veh.IsExtraOn(1)) continue;
                if ((VehicleHash)veh.Model.Hash == VehicleHash.Sadler && (veh.IsExtraOn(6) || veh.IsExtraOn(7))) continue;
                if ((VehicleHash)veh.Model.Hash == VehicleHash.Sandking2 && veh.GetMod(VehicleMod.Roof) == 4) continue;
                LogThis(" - Found " + veh.DisplayName, DebugOutput, Debug >= DebugLevel.Everything); //                LogThis(, DebugOutput, Debug >= DebugLevel.Everything);


                //if (veh.Speed > 2f) veh.FreezePosition = true;

                Function.Call(GTA.Native.Hash._0x0DC7CABAB1E9B67E, veh, true);
                Vector3 pos = veh.Position + (veh.UpVector * 1) + (veh.ForwardVector * -2);

                if (RandomInt(0, 10) <= 5)
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", " - Spawning rabbits");
                    
                    for (int i = 0; i < 5; i++)
                    {
                        Ped ped = World.CreatePed(PedHash.Rabbit, pos, veh.Heading);
                        Function.Call(Hash._0x0DC7CABAB1E9B67E, ped, true);
                        Script.Wait(100);
                        ped.Velocity = veh.Velocity;
                        Function.Call(Hash.SET_PED_TO_RAGDOLL, ped, 2000, 2000, 3, true, true, false);
                        Function.Call(Hash.CREATE_NM_MESSAGE, 1151);
                        Function.Call(Hash.GIVE_PED_NM_MESSAGE, ped, true);
                        ped.ApplyDamage(900);
                        //ped.AttachTo(veh, 0);

                        TemporalPersistence.Add(ped);//ped.IsPersistent = false;
                    }
                }
                else
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", " - Spawning deer");

                    Ped ped = World.CreatePed(PedHash.Deer, pos, veh.Heading);
                    Function.Call(GTA.Native.Hash._0x0DC7CABAB1E9B67E, ped, true);
                    Script.Wait(100);
                    ped.Velocity = veh.Velocity;
                    Function.Call(Hash.SET_PED_TO_RAGDOLL, ped, 2000, 2000, 3, true, true, false);
                    Function.Call(Hash.CREATE_NM_MESSAGE, 1151);
                    Function.Call(Hash.GIVE_PED_NM_MESSAGE, ped, true);
                    ped.ApplyDamage(900);
                    //ped.AttachTo(veh, 0);

                    TemporalPersistence.Add(ped);//ped.IsPersistent = false;
                }

                if (DebugBlips)
                {
                    if (!veh.CurrentBlip.Exists())
                    {
                        veh.AddBlip();
                        veh.CurrentBlip.Sprite = BlipSprite.Hunting;
                        veh.CurrentBlip.Color = BlipColor.Yellow;
                        veh.CurrentBlip.IsShortRange = true;
                    }
                }
                if (Debug >= DebugLevel.EventsAndScenarios)
                {
                    UI.Notify("~b~Spawned trophy on " + veh.FriendlyName);
                }


                if (!CanWeUse(veh.Driver) && RandomInt(0, 10) <= 5) //
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", " - Spawning Hunter nearby");

                    Ped hunter = World.CreatePed(PedHash.Hunter, World.GetSafeCoordForPed(veh.Position.Around(2), false), RandomInt(0, 350));
                    Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, hunter, "WORLD_HUMAN_SMOKING", 5000, true);
                    //hunter.IsPersistent = false;

                    TemporalPersistence.Add(hunter);
                }
                TemporalPersistence.Add(veh);

                BlacklistedVehicles.Add(veh);
                Script.Wait(1000);
                if (veh.FreezePosition) veh.FreezePosition = false;
                //BlacklistedImportantEvents.Add(EventType.AnimalTrophy);
                if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", " - Finished");
                // AnimalTrophyCooldown = Game.GameTime + (1000 * 60 * 10);
                break;
            }
        }
    }
    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
    {
        Vector3 P = x * Vector3.Normalize(B - A) + A;
        return P;
    }

    void SpawnCarInteraction(Vehicle v, Ped p)
    {
        Vehicle veh = v;

        /*
        foreach (Vehicle v1 in AllVehicles)
        {
            if (CanWeUse(v1) && v1.IsInRangeOf(Game.Player.Character.Position, InteractionRange) && !WouldPlayerNoticeChangesHere(v1.Position) && !v1.IsPersistent && v1.Model.IsCar && !CanWeUse(v1.Driver) && v1.IsStopped && !v1.EngineRunning && v1.HeightAboveGround > 0.4f)
            {
                veh = v1;
                break;
            }
        }*/
        if (CanWeUse(veh))
        {
            Ped ped = p;// World.CreateRandomPed(veh.Position.Around(5));
            TemporalPersistence.Add(ped);
            TemporalPersistence.Add(veh);

            //ped.SetNoCollision(veh, true);
            //veh.SetNoCollision(ped, true);

            if (CanWeUse(veh) && CanWeUse(ped))
            {
                int scenarioint = RandomInt(0, 3);


                TaskSequence seq = new TaskSequence();


                switch (scenarioint)
                {
                    case 0:
                        {
                            //veh.EngineHealth = 200;
                            //veh.OpenDoor(VehicleDoor.Hood, false, false);
                            veh.OpenDoor(VehicleDoor.FrontLeftDoor, true, true);
                            Vector3 pos = veh.Position + (veh.ForwardVector * (veh.Model.GetDimensions().Y * 0.54f));
                            float heading = veh.Heading;
                            ped.Heading = veh.Heading;
                            ped.Position = pos;
                            Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, 0, "WORLD_HUMAN_VEHICLE_MECHANIC", 20000, true);
                            break;
                        }
                    case 1:
                        {
                            Vector3 pos = veh.Position + (veh.ForwardVector * -0.5f) + veh.RightVector * (veh.Model.GetDimensions().X * 0.6f);
                            ped.Heading = veh.Heading - 90;
                            ped.Position = pos;
                            Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, 0, "WORLD_HUMAN_LEANING", 20000, true);
                            break;
                        }
                    case 2:
                        {
                            Vector3 pos = veh.Position + (veh.ForwardVector * -0.5f) + veh.RightVector * (veh.Model.GetDimensions().X * 0.7f);
                            ped.Heading = veh.Heading + 90;
                            ped.Position = pos;
                            Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, 0, "WORLD_HUMAN_MAID_CLEAN", 20000, true);
                            break;
                        }
                }
                Function.Call(Hash.TASK_ENTER_VEHICLE, 0, veh, 20000, -1, 1f, 1, 0);
                Function.Call(Hash.TASK_PAUSE, 0, RandomInt(2, 4) * 1000);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, veh, 20f, 1 + 2 + 4 + 8 + 16 + 32 + 128 + 256);
                seq.Close();
                ped.Task.PerformSequence(seq);
                seq.Dispose();
                ped.BlockPermanentEvents = false;

                //Function.Call(Hash.TASK_START_SCENARIO_AT_POSITION, ped, "WORLD_HUMAN_VEHICLE_MECHANIC", pos.X, pos.Y, pos.Z, heading, 50000, false, false);
            }
        }
    }


    static public List<Model> CarrierVehicles = new List<Model> { "sturdy2", "flatbed", "ramptruck", "barracks4", "ramptruck2", "skylift", "wastelander", "mule5" };

    void SmartAttach(Vehicle carrier, Vehicle veh)
    {
        Vector3 relativePos = new Vector3(0, -2, 0.7f + (veh.Model.GetDimensions().Z / 5)); //
        veh.AttachTo(carrier, 0, relativePos, new Vector3(0, 0, 0));
        veh.Detach();
        veh.Speed = carrier.Speed;

        Script.Wait(500);

        Vector3 hook = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, carrier, veh.Position.X, veh.Position.Y, veh.Position.Z);
        veh.AttachTo(carrier, 0, hook, new Vector3(0, 0, 0));
    }


    public static bool IsEntityAheadEntity(Entity e1, Entity e2)
    {
        Vector3 pos = e1.Position;
        return Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, e2, pos.X, pos.Y, pos.Z).Y > 1;
    }
    public static Vector3 GetEntityOffset(Entity ent, float ahead, float right)
    {
        return ent.Position + (ent.ForwardVector * ahead) + (ent.RightVector * right);
    }

    enum PlayerContext
    {
        IdleFoot, IdleDriving, BusyDrivingNormal, BusyDrivingFast, BusyCombat, VeryBusy
    }

    PlayerContext CurrentPlayerContext = PlayerContext.IdleFoot;

    public void RebuildPools()
    {


        CurrentlyAllowedEvents.Clear();
        CurrentlyAllowedScenarios.Clear();
        Vehicle PlayerCar = Game.Player.LastVehicle;


        //Context
        if (Function.Call<bool>(Hash.IS_PED_USING_ACTION_MODE, Game.Player.Character))
        {
            CurrentPlayerContext = PlayerContext.BusyCombat;

        }

        if (CanWeUse(PlayerCar))
        {
            if (Game.Player.Character.IsInVehicle(PlayerCar))
            {
                if (PlayerCar.Speed > 20)
                {
                    if (PlayerCar.Speed > 40)
                    {
                        CurrentPlayerContext = PlayerContext.BusyDrivingFast;
                    }
                    else
                    {
                        CurrentPlayerContext = PlayerContext.BusyDrivingNormal;
                    }
                }
                else
                {
                    CurrentPlayerContext = PlayerContext.IdleDriving;

                }
            }
            else CurrentPlayerContext = PlayerContext.IdleFoot;

        }
        else if (Game.Player.Character.IsOnFoot)
        {
            CurrentPlayerContext = PlayerContext.IdleFoot;
        }


        //Advanced Context
        switch (CurrentPlayerContext)
        {
            case PlayerContext.VeryBusy:
                {
                    //  Nothing
                    break;
                }
            case PlayerContext.BusyCombat:
                {
                    CurrentlyAllowedEvents.Add(EventType.EmergencyRushing);

                    break;
                }
            case PlayerContext.BusyDrivingFast:
                {
                    CurrentlyAllowedEvents.Add(EventType.EmergencyRushing);

                    break;
                }
            case PlayerContext.BusyDrivingNormal:
                {
                    CurrentlyAllowedScenarios.Add(ScenarioType.ImprovedFreight);
                    CurrentlyAllowedScenarios.Add(ScenarioType.ImprovedFlatbeds);
                    CurrentlyAllowedScenarios.Add(ScenarioType.ImprovedTowtrucks);
                    CurrentlyAllowedScenarios.Add(ScenarioType.AnimalTrophies);

                    CurrentlyAllowedScenarios.Add(ScenarioType.DriverOvertake);

                    CurrentlyAllowedEvents.AddRange(new List<EventType> { EventType.Racer, EventType.Deal, EventType.EmergencyRushing });

                    break;
                }
            case PlayerContext.IdleDriving:
                {

                    CurrentlyAllowedEvents.AddRange(new List<EventType> { EventType.Racer, EventType.Deal, EventType.EmergencyRushing, EventType.Hunter, EventType.GangDriveby, });
                    foreach (ScenarioType d in Enum.GetValues(typeof(ScenarioType)).Cast<ScenarioType>())
                    {
                        CurrentlyAllowedScenarios.Add(d);
                    }

                    break;
                }
            case PlayerContext.IdleFoot:
                {

                    foreach (ScenarioType d in Enum.GetValues(typeof(ScenarioType)).Cast<ScenarioType>())
                    {
                        CurrentlyAllowedScenarios.Add(d);
                    }
                    foreach (EventType d in Enum.GetValues(typeof(EventType)).Cast<EventType>())
                    {
                        CurrentlyAllowedEvents.Add(d);
                    }
                    break;
                }
        }


        foreach (ScenarioType d in DisabledScenarios) if (CurrentlyAllowedScenarios.Contains(d)) CurrentlyAllowedScenarios.Remove(d);
        foreach (EventType d in DisabledEvents) if (DisabledEvents.Contains(d)) CurrentlyAllowedEvents.Remove(d);

        //Fixes
        if (CurrentlyAllowedEvents.Contains(EventType.Hunter))
        {
            if (new List<string> { "CANNY", "MTJOSE", "DESRT", "CMSW", "ZANCUDO", "LAGO", "GREATC", "PALHIGH", "CCREAK", "MTCHIL" }.Contains(World.GetZoneNameLabel(Game.Player.Character.Position)) == false)
            {
                CurrentlyAllowedEvents.Remove(EventType.Hunter);
            }
        }
        if (CurrentlyAllowedScenarios.Contains(ScenarioType.AnimalTrophies))
        {
            if (new List<string> { "ALAMO", "PALETO", "SANDY" }.Contains(World.GetZoneNameLabel(Game.Player.Character.Position)) == false)
            {
                CurrentlyAllowedScenarios.Remove(ScenarioType.AnimalTrophies);
            }
        }
        if (CurrentlyAllowedEvents.Contains(EventType.GangDriveby))
        {
            if (!GangAreas.Contains(World.GetZoneNameLabel(Game.Player.Character.Position)))
            {
                CurrentlyAllowedEvents.Remove(EventType.GangDriveby);
            }
        }

        AddScenarioProbMultipliers();

    }
    public static void TemporalPullover(Ped p)
    {
        Vehicle veh = p.CurrentVehicle;

        if (RandomInt(0, 10) > 5)
        {
            veh.EngineHealth = 1;
        }
        else
        {
            veh.BurstTire(RandomInt(0, 4));
        }

        veh.LeftIndicatorLightOn = true;
        veh.RightIndicatorLightOn = true;
        Vector3 pos = GetEntityOffset(veh, 30, 0);
        Vector3 park = World.GetNextPositionOnSidewalk(GetEntityOffset(veh, 50, 0)) + (veh.RightVector * 4);
        float speed = veh.Speed;
        TaskSequence seq = new TaskSequence();
        Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, veh, pos.X, pos.Y, pos.Z, speed / 2, 1 + 2 + 4 + 8 + 16, 5f);
        Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, veh, park.X, park.Y, park.Z, 5f, 1 + 2 + 4 + 8 + 16 + 4456509, 3f);
        Function.Call(Hash.TASK_PAUSE, 0, RandomInt(3, 7) * 1000);
        Function.Call(Hash.TASK_LEAVE_VEHICLE, 0, veh, 262144);
        Function.Call(Hash.TASK_USE_MOBILE_PHONE_TIMED, 0, RandomInt(5, 10) * 1000);
        Function.Call(Hash.TASK_WANDER_STANDARD, 0, 10f, 10);

        //Function.Call(Hash.TASK_WANDER_IN_AREA, HunterPed, pos.X, pos.Y, pos.Z, 100f, 2f, 3f);

        //Function.Call(Hash.TASK_ENTER_VEHICLE, 0, veh, 20000, -1, 1f, 1, 0);
        //Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, veh, 15f, 1 + 2 + 4 + 8 + 16 + 32);
        seq.Close();
        p.Task.PerformSequence(seq);
        seq.Dispose();

        p.BlockPermanentEvents = false;
        SetDecorBool("HandledByTow", veh, false);

    }

    List<Rope> TrailerRopes = new List<Rope>();
    List<VehicleColor> MatteColor = new List<VehicleColor>();
    List<Vehicle> BarnCars = new List<Vehicle>();

    void Barn(Vehicle v)
    {
        if (CanWeUse(v))
        {
            File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Rendering " + v.FriendlyName + " as wrecked");
            if (MatteColor.Count == 0)
            {
                foreach (VehicleColor color in Enum.GetValues(typeof(VehicleColor)))
                {
                    if (color.ToString().ToLowerInvariant().Contains("matte") || color.ToString().ToLowerInvariant().Contains("worn")) MatteColor.Add(color);
                }
            }

            if (MatteColor.Count > 0)
            {
                VehicleColor color = MatteColor[RandomInt(0, MatteColor.Count - 1)];
                v.PrimaryColor = color;
                v.SecondaryColor = color;
            }
            else
            {
                v.PrimaryColor = VehicleColor.MatteBlack;
                v.SecondaryColor = VehicleColor.MatteBlack;

            }
            v.IsDriveable = false;
            v.PearlescentColor = VehicleColor.MatteBlack;
            v.EngineCanDegrade = true;
            v.EngineHealth = 500;
            v.EngineRunning = false;

            v.ApplyDamage(v.Position + Vector3.WorldDown, 20f, 10);
            Function.Call(Hash.SET_ENTITY_RENDER_SCORCHED, v, true);
            //Function.Call(Hash.SET_VEHICLE_DOOR_BROKEN, v, 4, true);

            for (int i = -2; i < 10; i++)
            {
                Function.Call(Hash.SET_VEHICLE_DOOR_BROKEN, v, i, true); //if (i != 1) 
            }
            int tire = RandomInt(0, 4);
            if (RandomInt(0, 10) >= 5) Function.Call(Hash.SET_VEHICLE_TYRE_BURST, v, tire, true, 1000);
            else Function.Call(Hash.SET_VEHICLE_TYRE_BURST, v, tire, false, 300);
            BlacklistedVehicles.Add(v);
            SetDecorBool("IsAWreck", v, true);
        }
    }


    bool IsEntityApproachingEntity(Entity entity_one, Entity entity_two)
    {
        Vector3 pos_one = entity_one.Position + entity_one.Velocity;
        Vector3 pos_two = entity_two.Position + entity_two.Velocity;


        if (pos_one.DistanceTo(pos_two) < entity_one.Position.DistanceTo(entity_two.Position)) return true;

        return false;
    }

    int ForcedScenario = -1;


    Vehicle AttachBikeToCar(Vehicle v, Model cycleModel)
    {
        if (CanWeUse(v))
        {
            Vehicle cycle = World.CreateVehicle(cycleModel, v.Position.Around(30));
            if (CanWeUse(cycle))
            {
                float back = (v.Position.DistanceTo(v.GetBoneCoord("taillight_l")));
                //Vector3 Bumperpos = ;
                cycle.Alpha = 0;
                FadeIn.Add(cycle);
                  cycle.AttachTo(v, 0, new Vector3(0, -(back+0.05f), 0.5f), new Vector3(0, 20, 90)); // new Vector3(0, -(v.Model.GetDimensions().Y / 2.1f), 0.3f)
                //AttachPhysically(cycle, v, Vector3.Zero, new Vector3(0, -(back + 0.05f), 0.5f), new Vector3(0, 20, 90), 0, 0, 100f, true, false);
            }
            return cycle;
        }
        return null;
    }
    void ProcessCheats()
    {

        if (WasCheatStringJustEntered("getzone")) UI.Notify(World.GetZoneNameLabel(Game.Player.Character.Position));
        if (WasCheatStringJustEntered("lwattachcycle"))
        {
            Vehicle v = Game.Player.Character.CurrentVehicle;
            Vehicle cycleCar = AttachBikeToCar(v,Game.GetUserInput(20));
            cycleCar.Velocity = v.Velocity;
            if (CanWeUse(cycleCar)) cycleCar.IsPersistent = false;
        }

        if (WasCheatStringJustEntered("rambar"))
        {
            Vehicle v = Game.Player.Character.CurrentVehicle;

            if (CanWeUse(v))
            {
                Prop bar = World.CreateProp("imp_prop_impexp_front_bars_01b", v.Position.Around(3), true, true);
                bar.AttachTo(v, 0, new Vector3(bar.Model.GetDimensions().X / 4, (v.Model.GetDimensions().Y / 2.3f), -0.2f), new Vector3(0, 0, 0));
                //cycle.PrimaryColor = VehicleColor.MatteBlack;
                bar.IsPersistent = false;
            }
        }
        if (WasCheatStringJustEntered("strip"))
        {
            UI.Notify("Stripping " + Game.Player.Character.LastVehicle.FriendlyName);
            StripOfAllPossible(Game.Player.Character.LastVehicle, true, true, true, true, false, true);
        }
        if (WasCheatStringJustEntered("generator"))
        {

            if (Function.Call<bool>(Hash.DOES_SCRIPT_VEHICLE_GENERATOR_EXIST, Generator))
            {
                Function.Call<bool>(Hash.DELETE_SCRIPT_VEHICLE_GENERATOR, Generator);
                UI.Notify("Removed generator");
            }
            else
            {
                string car = Game.GetUserInput(30);
                Vector3 coords = Game.Player.Character.Position;
                Generator = Function.Call<int>(Hash.CREATE_SCRIPT_VEHICLE_GENERATOR, coords.X, coords.Y, coords.Z, Game.Player.Character.Heading, 5.0f, 3.0f, Game.GenerateHash(car), -1, -1, -1, -1, true, true, true, true, true, -1);
                UI.Notify("Created generator");
            }
        }

        if (WasCheatStringJustEntered("override"))
        {

            Vector3 pos = Game.Player.Character.Position;
            int zoneid = Function.Call<int>(Hash.GET_ZONE_AT_COORDS, pos.X, pos.Y, pos.Z);
            string CarHash = Game.GetUserInput(20);

            Model carmodel = CarHash;

            Function.Call<int>(Hash.OVERRIDE_POPSCHEDULE_VEHICLE_MODEL, Function.Call<int>(Hash.GET_ZONE_POPSCHEDULE, zoneid), Game.GenerateHash(CarHash));
            carmodel.Request();

            //UI.Notify("Done.");
        }

        if (WasCheatStringJustEntered("pullover"))
        {
            Ped p = null;
            foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character.Position + Game.Player.Character.ForwardVector * 20f, 10f)) if (!ped.IsOnFoot) { p = ped; break; }

            if (CanWeUse(p)) TemporalPullover(p);
        }

        if (Duel) HandleDuel();
        if (WasCheatStringJustEntered("lwduel"))
        {
            Duel = true;
        }
        if (WasCheatStringJustEntered("lwscenario"))
        {
            string input = Game.GetUserInput(30);

            foreach (ScenarioType d in Enum.GetValues(typeof(ScenarioType)).Cast<ScenarioType>())
            {
                if (d.ToString().ToLowerInvariant() == input.ToLowerInvariant())
                {
                    ForcedScenario = (int)d;
                    break;
                }
            }

        }

        if (WasCheatStringJustEntered("lwevent"))
        {
            string input = Game.GetUserInput(30);

            foreach (EventType d in Enum.GetValues(typeof(EventType)).Cast<EventType>())
            {
                if (d.ToString().ToLowerInvariant() == input.ToLowerInvariant())
                {
                    ForcedEvent = (int)d;
                    break;
                }
            }

        }
        if (WasCheatStringJustEntered("lwracers"))
        {
            if (Racers.Count == 0) CreateRacers(); else UI.Notify("racers still exist");
        }
        if (WasCheatStringJustEntered("lwwerecar"))
        {
            werecar = Game.Player.Character.LastVehicle;
            UI.Notify("werecar");
        }

        HandleWereCar();
        if (WasCheatStringJustEntered("lwbarn"))
        {
            //  UI.Notify("barnin'");

            Vehicle v = Game.Player.Character.CurrentVehicle;
            if (!CanWeUse(v)) v = Game.Player.Character.LastVehicle;

            if (!CanWeUse(v))
            {
                return;
            }
            Barn(v);
            BarnCars.Add(v);
        }
        //Vector3 GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS(Entity entity, float posX, float posY, float posZ) // 2274BC1C4885E333 6477EC9E

        /*
        if (Game.IsControlJustPressed(2,GTA.Control.Context))
        {
            Ped p = World.GetClosestPed(GetEntityOffset(Game.Player.Character, 20, 0),20f);
            if(CanWeUse(p) && CanWeUse(p.CurrentVehicle))
            {
                Vehicle veh = p.CurrentVehicle;
                Vector3 pos = GetEntityOffset(veh, 30, 0);
                Vector3 park = World.GetNextPositionOnStreet(GetEntityOffset(veh, 50, 0))+(veh.RightVector*10);

                TaskSequence seq = new TaskSequence();
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, veh, pos.X, pos.Y, pos.Z, 8f, 8, 2f);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, veh, park.X, park.Y, park.Z, 3f, 4456509, 2f);

                Function.Call(Hash.TASK_PAUSE, 0, RandomInt(2, 4) * 1000);
                Function.Call(Hash.TASK_LEAVE_VEHICLE, 0, veh, 262144);
                Function.Call(Hash.TASK_PAUSE, 0, RandomInt(2, 4) * 1000);

                Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, 0, "WORLD_HUMAN_LEANING",5000f, false);

                Function.Call(Hash.TASK_ENTER_VEHICLE, 0, veh, 20000, -1, 1f, 1, 0);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, veh, 30f, 1 + 2 + 4 + 8 + 16 + 32);
                seq.Close();
                p.Task.PerformSequence(seq);
                seq.Dispose();

                p.BlockPermanentEvents = false;
            }

        }
        */


        /* foreach (Rope r in TrailerRopes)
         {
             if (!Game.Player.Character.IsInRangeOf(r.GetVertexCoord(1), 200f)) { r.Delete(); break; }
         }*/

        if (1 != 1)
        {
            Vehicle v = Game.Player.Character.CurrentVehicle;

            if (CanWeUse(v) && Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, v, true).X) > 3f)
            {


                /*
                TASK_LEAVE_VEHICLE(Ped ped, Vehicle vehicle, int flags) // D3DBCE61A490BE02 7B1141C6
            Flags from decompiled scripts:
            0 = normal exit and closes door.
            1 = normal exit and closes door.
            16 = teleports outside, door kept closed.
            64 = normal exit and closes door, maybe a bit slower animation than 0.
            256 = normal exit but does not close the door.
            4160 = ped is throwing himself out, even when the vehicle is still.
            262144 = ped moves to passenger seat first, then exits normally
                */
                /*
                v.OpenDoor(VehicleDoor.FrontRightDoor, true, false);
                            Script.Wait(300);
                            Ped p = v.GetPedOnSeat(VehicleSeat.RightFront);
                            //if (CanWeUse(p)) p.Kill();

                            Function.Call(Hash.TASK_LEAVE_VEHICLE, p, v, 4160);

                            //p.Position = v.Position+v.RightVector;
                            Script.Wait(500);
                            Function.Call(Hash.SET_PED_TO_RAGDOLL, p, 2000, 2000, 3, true, true, false);
                            Function.Call(Hash.CREATE_NM_MESSAGE, 1151);
                            Function.Call(Hash.GIVE_PED_NM_MESSAGE, p, true);
                            UI.Notify("ragdolled");
                            */
            }
        }

        if (WasCheatStringJustEntered("car"))
        {
            UI.Notify("Current car " + Game.Player.Character.CurrentVehicle.DisplayName);
        }
        if (WasCheatStringJustEntered("lwpos"))
        {
            Vector3 pos = Game.Player.Character.Position;
            string t = "";
            t = t + "Vector3 (" + pos.X + "," + pos.Y + "," + pos.Z + ");";
            File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + t);

            UI.Notify(t);

        }
        if (WasCheatStringJustEntered("lwcarjacker"))
        {
            UI.Notify("Called for a carjacker event.");
            CarjackerEnabled = true;
        }

        if (WasCheatStringJustEntered("lwtraffictest"))
        {
            UI.Notify("~b~Calling for all injected vehicles:");
            foreach (TrafficSpawner t in TrafficSpawnerList)
            {
                t.Cooldown = Game.GameTime + (RandomInt(1, 10) * 1000);
                //UI.Notify(t.SourceVehicle.ToString());
            }
        }
        if (WasCheatStringJustEntered("lwtow"))
        {
            SpawnTow();
            UI.Notify("Called for a Tow event.");

        }
        if (WasCheatStringJustEntered("lwhook"))
        {
            UI.Notify("received.");
            Vehicle veh = Game.Player.Character.CurrentVehicle;

            if (CanWeUse(veh))
            {
                foreach (Vehicle v in World.GetNearbyVehicles(veh.Position, 5f))
                {
                    if (v.Handle != veh.Handle)
                    {
                        if (v.IsAttachedTo(veh))
                        {
                            v.Detach();
                        }
                        else
                        {
                            //attach respecting offset Vector3 relativePos = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, veh, v.Position.X,v.Position.Y,v.Position.Z);
                            Vector3 relativePos = new Vector3(0, -2, 0.7f + (v.Model.GetDimensions().Z / 5)); //

                            /*v.Position = veh.Position + (veh.ForwardVector*-2)+(veh.UpVector*3f);
                            v.Heading = veh.Heading;
                            v.Speed = veh.Speed;*/
                            v.AttachTo(veh, 0, relativePos, new Vector3(0, 0, 0));
                            v.Detach();
                            Script.Wait(1000);

                            Vector3 hook = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, veh, v.Position.X, v.Position.Y, v.Position.Z);
                            //relativePos = new Vector3(relativePos.X, -relativePos.Y, -relativePos.Z);
                            v.AttachTo(veh, 0, hook, new Vector3(0, 0, 0));
                        }
                    }
                }
            }

        }

        if (WasCheatStringJustEntered("lwdebugoutput"))
        {
            DebugOutput = !DebugOutput;
            UI.Notify("~b~Debug output is now " + DebugOutput);
        }

        if (WasCheatStringJustEntered("lwdrivebyb"))
        {
            UI.Notify("DriveBy Spawned (Ballas)");
            SpawnGangDriveBy(Gang.Ballas, World.GetClosestPed(Game.Player.Character.Position.Around(30f), 20f));
        }
        if (WasCheatStringJustEntered("lwdrivebyf"))
        {
            UI.Notify("DriveBy Spawned (Families)");
            SpawnGangDriveBy(Gang.Families, World.GetClosestPed(Game.Player.Character.Position.Around(30f), 20f));
        }
        if (WasCheatStringJustEntered("lwdrivebyl"))
        {
            UI.Notify("DriveBy Spawned  (Lost)");
            SpawnGangDriveBy(Gang.Lost, World.GetClosestPed(Game.Player.Character.Position.Around(30f), 20f));
        }
        if (WasCheatStringJustEntered("lwdrivebyv"))
        {
            UI.Notify("DriveBy Spawned (Vagos)");
            SpawnGangDriveBy(Gang.Vagos, World.GetClosestPed(Game.Player.Character.Position.Around(30f), 20f));
        }

        if (WasCheatStringJustEntered("lwzonename"))
        {
            string text = "";
            text += "Zone:" + GetZoneName(Game.Player.Character.Position);
            text += "~n~Street:" + World.GetStreetName(Game.Player.Character.Position);
            text += "~n~Zone:" + World.GetZoneName(Game.Player.Character.Position);
            text += "~n~ZoneLabel:" + World.GetZoneNameLabel(Game.Player.Character.Position);

            UI.Notify(text);
        }
        if (WasCheatStringJustEntered("lwhunter"))
        {
            UI.Notify("Called for a Hunter event.");

            Vector3 pos = World.GetSafeCoordForPed(Game.Player.Character.Position.Around(100), false); //GenerateSpawnPos(Game.Player.Character.Position.Around(50),Nodetype.Offroad,false)
            Hunters.Add(new Hunter(pos));
        }
        if (WasCheatStringJustEntered("lwanimaltrophy"))
        {
            UI.Notify("Called for an Animal Trophy event.");

            SpawnAnimalTrophy();
        }
        if (WasCheatStringJustEntered("lwambulance"))
        {
            UI.Notify("Called for an Ambulance event.");

            SpawnEmergencyVehicle(EmergencyType.AMBULANCE);
        }
        if (WasCheatStringJustEntered("lwpolice"))
        {
            UI.Notify("Called for a Police event.");

            SpawnEmergencyVehicle(EmergencyType.POLICE);
        }
        if (WasCheatStringJustEntered("lwfiretruck"))
        {
            UI.Notify("Called for a Firetruck event.");
            SpawnEmergencyVehicle(EmergencyType.FIRETRUCK);
        }
        if (WasCheatStringJustEntered("lwdebug"))
        {
            Debug++;
            if (Debug > DebugLevel.Everything) Debug = DebugLevel.None;
            UI.Notify("~b~Debug set to " + Debug.ToString());
        }
        if (WasCheatStringJustEntered("lwblips")) { DebugBlips = !DebugBlips; UI.Notify("~b~DebugBlips set to " + DebugBlips.ToString()); }

        if (WasCheatStringJustEntered("lwtaxi"))
        {
            UI.Notify("Called for a Taxi event.");

            SpawnTaxiEvent();
        }
        if (WasCheatStringJustEntered("lwcarmeet"))
        {
            SpawnCarMeet();
        }

        if (WasCheatStringJustEntered("lwdealp"))
        {
            UI.Notify("Called for a Deal event (private).");

            SpawnDrugDeal(false);
        }
        if (WasCheatStringJustEntered("lwdeal"))
        {
            UI.Notify("Called for a Deal event. (Automatic)");

            SpawnDrugDeal(IsInNamedArea(Game.Player.Character, "desrt"));
        }
        if (WasCheatStringJustEntered("lwdealg"))
        {
            UI.Notify("Called for a Gang Deal event. (Gang)");
            SpawnDrugDeal(true);
        }
    }





    public static bool DecorExistsOn(string decor, Entity e)
    {
        if (!CanWeUse(e)) return false;
        return Function.Call<bool>(Hash.DECOR_EXIST_ON, e, decor);
    }

    public static float GetDecorFloat(string decor, Entity e)
    {
        if (!CanWeUse(e)) return -2;
        return Function.Call<float>(Hash._DECOR_GET_FLOAT, e, decor);
    }

    public static int GetDecorInt(string decor, Entity e)
    {
        if (!CanWeUse(e)) return -2;
        return Function.Call<int>(Hash.DECOR_GET_INT, e, decor);
    }
    public static bool GetDecorBool(string decor, Entity e)
    {
        if (!CanWeUse(e)) return false;
        return Function.Call<bool>(Hash.DECOR_GET_BOOL, e, decor);
    }

    public static void SetDecorBool(string decor, Entity e, bool i)
    {
        if (!CanWeUse(e)) return;
        Function.Call(Hash.DECOR_SET_BOOL, e, decor, i);
    }
    public static void SetDecorInt(string decor, Entity e, int i)
    {
        if (!CanWeUse(e)) return;
        Function.Call(Hash.DECOR_SET_INT, e, decor, i);
    }

    public static void SetDecorFloat(string decor, Entity e, float i)
    {
        if (!CanWeUse(e)) return;
        Function.Call(Hash._DECOR_SET_FLOAT, e, decor, i);
    }

    public unsafe static byte* FindPattern(string pattern, string mask)
    {
        ProcessModule module = Process.GetCurrentProcess().MainModule;

        ulong address = (ulong)module.BaseAddress.ToInt64();
        ulong endAddress = address + (ulong)module.ModuleMemorySize;

        for (; address < endAddress; address++)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (mask[i] != '?' && ((byte*)address)[i] != pattern[i])
                {
                    break;
                }
                else if (i + 1 == pattern.Length)
                {
                    return (byte*)address;
                }
            }
        }

        return null;
    }
    void UnlockDecorator()
    {

        unsafe
        {
            IntPtr addr = (IntPtr)FindPattern("\x40\x53\x48\x83\xEC\x20\x80\x3D\x00\x00\x00\x00\x00\x8B\xDA\x75\x29",
                            "xxxxxxxx????xxxxx");
            if (addr != IntPtr.Zero)
            {
                byte* g_bIsDecorRegisterLockedPtr = (byte*)(addr + *(int*)(addr + 8) + 13);
                *g_bIsDecorRegisterLockedPtr = 0;
            }

        }
    }
    void LockDecotator()
    {
        unsafe
        {
            IntPtr addr = (IntPtr)FindPattern("\x40\x53\x48\x83\xEC\x20\x80\x3D\x00\x00\x00\x00\x00\x8B\xDA\x75\x29",
                            "xxxxxxxx????xxxxx");
            if (addr != IntPtr.Zero)
            {
                byte* g_bIsDecorRegisterLockedPtr = (byte*)(addr + *(int*)(addr + 8) + 13);
                *g_bIsDecorRegisterLockedPtr = 1;
            }

        }
    }

    Vector3 LastWreck = Vector3.Zero;
    void ProcessLists()
    {
        if (BlacklistedVehicles.Count > 50)
        {
            BlacklistedVehicles.RemoveRange(0, 5);
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~ BlacklistedVehicles list pruned .");
        }


        if (WrecksChecker < Game.GameTime)
        {
            if (!CurrentlyAllowedScenarios.Contains(ScenarioType.BarnFinds)) return;
            if (Game.Player.Character.IsInRangeOf(LastWreck, 100f)) return;
            //UI.Notify("Wrecker started");
           if(DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Checking for wrecks");
            int i = 0;
            foreach (Prop p in World.GetAllProps())
            {
                if (Game.Player.Character.IsInRangeOf(p.Position, 50f)) continue;

                //(!Function.Call<bool>(Hash.WOULD_ENTITY_BE_OCCLUDED, Game.GenerateHash("blista"), pos.X, pos.Y, pos.Z, true)) 
                if (Function.Call<bool>(Hash.IS_ENTITY_OCCLUDED, p) && WreckModels.Contains(p.Model) && BarnCars.Count < 10)
                {
                    int max = 0;
                    foreach (Vehicle v in BarnCars)
                    {
                        if (v.IsInRangeOf(p.Position, 10f))
                        {
                            max++;
                            if (max >= 2)
                            {
                                break;
                            }
                        }
                    }
                    if (RandomInt(0, i) < 2 && max < 2)
                    {
                        Vector3 pos = p.Position + Vector3.WorldUp;
                        float h = p.Heading;
                        p.Delete();

                        Vehicle v = World.CreateVehicle(WreckCarModels[RandomInt(0, WreckCarModels.Count - 1)], pos, h);


                        v.AddBlip();
                        v.CurrentBlip.Sprite = BlipSprite.Dart;
                        // v.CurrentBlip.Scale = 0.5f;
                        v.CurrentBlip.IsShortRange = true;

                        if (v.ClassType == VehicleClass.Muscle || v.ClassType == VehicleClass.SportsClassics || v.ClassType == VehicleClass.Sports)
                        {
                            if (RandomInt(0, 10) < 4) v.SetMod(VehicleMod.Engine, 2, false);

                        }
                        Barn(v);

                        //v.IsPersistent = false;

                        Script.Wait(10);
                        BarnCars.Add(v);
                        BlacklistedVehicles.Add(v);
                        TemporalPersistence.Add(v);
                        i++;

                        LastWreck = v.Position;
                    }
                }
            }
            if (i > 1)
            {
                //Function.Call(Hash.FLASH_MINIMAP_DISPLAY);
                if (Game.Player.Character.Velocity.Length() > 10f) WrecksChecker = Game.GameTime + 10000;
                else WrecksChecker = Game.GameTime + (60000 * 3);
            }
            else
            {
                if (Game.Player.Character.Velocity.Length() > 10f) WrecksChecker = Game.GameTime + 4000;
                else WrecksChecker = Game.GameTime + 50000;
            }

            // UI.Notify("Wrecker finished");
        }
        //Blacklisted events cooldown
        if (BlacklistedEvents.Count > 0)
        {
            if (Debug >= DebugLevel.Everything)
            {
                string text = "";
                foreach (EventType ev in BlacklistedEvents) text = text + " | " + ev.ToString();
                DisplayHelpTextThisFrame("Blacklisted events:" + text);
            }
            if (BlackistedEventsTime < Game.GameTime)
            {
                if (BlackistedEventsTime == 0) BlackistedEventsTime = Game.GameTime + 40000;
                else
                {
                    BlackistedEventsTime = BlackistedEventsTime = Game.GameTime + 40000;
                    BlacklistedEvents.RemoveAt(0);
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Blacklisted events cleared");
                }
            }
        }

        //Blacklisted Important events cooldown
        /*
        if (BlacklistedImportantEvents.Count > 1)
        {
            if (Debug >= DebugLevel.EventsAndScenarios)
            {
                string text = "";
                foreach (EventType ev in BlacklistedImportantEvents) text = text + " | " + ev.ToString();
                UI.ShowSubtitle("Blacklisted Important events:" + text);
            }
            if (BlackistedImportantEventsTime < Game.GameTime)
            {
                if (BlackistedImportantEventsTime == 0) BlackistedImportantEventsTime = Game.GameTime + (60000 * BlackistedImportantEventsCooldown);//change to 5 in release
                else
                {
                    BlackistedImportantEventsTime = BlackistedImportantEventsTime = Game.GameTime + (60000 * BlackistedImportantEventsCooldown); //change to 5 in release
                    BlacklistedImportantEvents.RemoveAt(0);
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Blacklisted Important events cleared");
                }
            }
        }*/


        //Get all neccesary vehicles 
        if (VehMonitorTime < Game.GameTime)
        {
            VehMonitorTime = Game.GameTime + 5000;

            string names = "";


            /*
            foreach (Vehicle v in World.GetAllVehicles())
            {
                AllVehicles.Add(v);
                if (!v.PreviouslyOwnedByPlayer && MonitoredModels.Contains(v.Model))//!v.PreviouslyOwnedByPlayer && 
                {
                    if (!BlacklistedModels.Contains(v.Model))
                    {
                        MonitoredVehicles.Add(v);
                        names = names + " |  ~g~" + v.FriendlyName + "~w~";
                    }
                    else
                    {
                        names = names + " | ~o~" + v.FriendlyName + "~w~";
                    }
                }
                if (TruckRespray && Respraymodels.Contains(v.Model)&& !v.IsPersistent && !BlacklistedVehicles.Contains(v) && !Game.Player.Character.IsInRangeOf(v.Position, 40f)) { ResprayTruck(v); BlacklistedVehicles.Add(v); }
            }

            */
            if (BlacklistedModels.Count > 5)
            {
                BlacklistedModels.RemoveAt(0);
                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~y~Blacklist cleaned");
            }

            AllPeds.Clear();
            foreach (Ped p in World.GetAllPeds())
            {
                if (!p.IsPlayer && !p.IsPersistent)
                {
                    AllPeds.Add(p);
                }
            }
        }

    }
    Camera CoolCarCam = null;
    bool PhotoDone = false;
    float camHeight = 0f;

    Prop Phone = null;
    Vehicle coolcar = null;

    int BarnFindTutorial = 0;
    void ProcessCurrentEvents()
    {


        if (CanWeUse(Overtaker))
        {
            if (CanWeUse(Overtaken))
            {

                if (GetOffset(Overtaken, Overtaker).X > 0)
                {
                    Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 0, true);
                    Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 1, false);
                }
                else
                {
                    Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 1, true);
                    Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 0, false);
                }

            }
            if (!CanWeUse(Overtaken) || IsPosAheadEntity(Overtaken, Overtaker.Position) > 10f || !Overtaken.IsInRangeOf(Overtaker.Position, 50f))
            {

                if (Overtaker.CurrentBlip.Exists()) Overtaker.CurrentBlip.Remove();
                Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 1, false);
                Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 0, false);

                Overtaker.Task.ClearAll();
              if(Debug>= DebugLevel.EventsAndScenarios)  UI.Notify(Overtaker.CurrentVehicle.FriendlyName + " overtake finished");
                Overtaker.IsPersistent = false;
                Overtaker.DrivingStyle = DrivingStyle.Normal;
                Overtaker = null;
                if (CanWeUse(Overtaken) && Overtaken.IsPersistent) Overtaken.IsPersistent = false;
                Overtaken = null;
            }

        }
        if (CurrentlyAllowedScenarios.Contains(ScenarioType.PlayerCoolCarPhoto) && CanWeUse(CoolCarPed))
        {
            World.DrawMarker(MarkerType.ChevronUpx1, CoolCarPed.Position + (CoolCarPed.UpVector * 2), Vector3.Zero, -Vector3.WorldDown, new Vector3(1f, 1f, -1f), Color.SkyBlue);
            Vehicle v = Game.Player.Character.LastVehicle;
            if (!PhotoDone && CanWeUse(v))
            {
                if (CoolCarCam == null)
                {
                    foreach (Prop p in World.GetNearbyProps(CoolCarPed.GetBoneCoord(Bone.SKEL_Head), 1f))
                        if (CanWeUse(p) && p.IsAttachedTo(CoolCarPed))
                        {
                            //  p.IsVisible = false;
                            CoolCarCam = World.CreateCamera(Game.Player.Character.Position + Vector3.WorldUp, Vector3.Zero, GameplayCamera.FieldOfView);


                            if (!CoolCarPed.IsInRangeOf(v.Position, 10f)) CoolCarCam.FieldOfView -= 20;
                            Function.Call(Hash.ATTACH_CAM_TO_ENTITY, CoolCarCam, p, 0f, -0.01f, 0.1f, true);
                            CoolCarCam.Rotation = p.Rotation;
                            if (CanWeUse(coolcar))
                            {
                                CoolCarCam.PointAt(coolcar);
                            }
                            else
                            {
                                // CoolCarCam.AttachTo(p, Vector3.Zero);
                                CoolCarCam.PointAt(p, new Vector3(0, -0.2f, 0.1f));
                                p.IsVisible = false;
                            }


                            Phone = p;
                            break;
                        }
                }
                else
                {
                    if (IsNightTime() && CanWeUse(Phone)) World.DrawSpotLightWithShadow(CoolCarCam.Position, Phone.ForwardVector, Color.White, 5f, 0.8f, 10f, 30f, 10f);// 100f, 1f, 1f, 10f, 90f
                    if (CoolCarCam.Position.Z < camHeight && camHeight > CoolCarPed.GetBoneCoord(Bone.SKEL_Head).Z)
                    {
                        Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
                        Function.Call(Hash.HIDE_HELP_TEXT_THIS_FRAME);
                        for (int i = 0; i < 200; i++)
                        {
                            if (Function.Call<bool>(Hash.IS_HUD_COMPONENT_ACTIVE, i)) Function.Call(Hash.HIDE_HUD_COMPONENT_THIS_FRAME, i);
                        }

                        World.RenderingCamera = CoolCarCam;

                        SendKeys.SendWait("{F12}");
                        while (World.RenderingCamera != CoolCarCam) Script.Yield();
                        //Game.Pause(true);
                        //  Script.Wait(1000);
                        //Game.Pause(false);

                        World.RenderingCamera = null;
                        PhotoDone = true;
                        camHeight = 0f;
                        return;
                    }
                    else
                    {
                        camHeight = CoolCarCam.Position.Z;
                    }
                }
            }

            if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, CoolCarPed, 89))
            {

                int score = 0;
                string stringScore = "~g~";
                if (!v.PearlescentColor.ToString().Contains("black"))
                {
                    stringScore += "+Pearlescent";
                    score += 10;
                }
                if (Math.Abs(v.SteeringAngle) > 5)
                {
                    stringScore += "~n~Turned Wheels";
                    score += 5;
                }

                if (v.Livery > -1 || v.GetMod(VehicleMod.Livery) != -1)
                {
                    stringScore += "+Livery";
                    score += 20;
                }
                UI.Notify(stringScore);
                string message = "";

                if (v.FriendlyName != "")
                {
                    message = "Check out this cool ~b~" + v.FriendlyName + "~w~ I found on ~y~" + World.GetStreetName(v.Position) + "~w~ today!";
                }
                else
                {
                    message = "Check out this cool car I found on ~y~" + World.GetStreetName(v.Position) + "~w~ today!";
                }
                string sex = "GuyOfLS";
                if (CoolCarPed.Gender == Gender.Female) sex = "GirlOfLS";
                AddNotification("CHAR_LIFEINVADER", "~r~@SAN_ANDREAS_LIFE", "~r~" + sex, message);


                coolcar = null;
                CoolCarPed = null;
                PhotoDone = false;
                camHeight = 0f;
                if (CoolCarCam != null) CoolCarCam.Destroy();
                CoolCarCam = null;

                return;
            }
        }


        //Each second
        if (Interval < Game.GameTime)
        {
            Interval = Game.GameTime + 1000;

            if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Bennys");





            if (CanWeUse(Overtaker))
            {
                if (CanWeUse(Overtaken))
                {

                    if (GetOffset(Overtaken, Overtaker).X > 0)
                    {
                        Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 0, true);
                        Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 1, false);
                    }
                    else
                    {
                        Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 1, true);
                        Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, Overtaker.CurrentVehicle, 0, false);
                    }

                }

            }

            //if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~dice says bennys");

            //Benny's Motorworks
            if (!ScenarioFlow.Contains(ScenarioType.AmbientTuner) && CurrentlyAllowedScenarios.Contains(ScenarioType.AmbientTuner) && Bennys.Count > 0
            && Game.Player.Character.IsInRangeOf(BennysMotorworks, 200f) && !CanPedSeePos(Game.Player.Character, BennysMotorworks, true))
            {
                Model model = Bennys[RandomInt(0, Bennys.Count - 1)];
                if (model.IsValid)
                {
                    Vehicle veh = World.CreateVehicle(Bennys[RandomInt(0, Bennys.Count)], BennysMotorworks, 0);
                    if (CanWeUse(veh))
                    {
                        RandomTuning(veh, true, false, true, false, false);
                        Ped ped = veh.CreateRandomPedOnSeat(VehicleSeat.Driver);
                        ped.AlwaysKeepTask = true;

                        Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, ped, veh, 15f, 1 + 2 + 4 + 8 + 16 + 32);
                        if (!veh.CurrentBlip.Exists() && DebugBlips)
                        {
                            veh.AddBlip();
                            veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                            veh.CurrentBlip.Scale = 0.7f;
                            veh.CurrentBlip.Color = BlipColor.Green;
                            veh.CurrentBlip.IsShortRange = true;
                            veh.CurrentBlip.Name = "Custom Vehicle";
                        }
                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Benny's car spawned");

                        ped.IsPersistent = false;
                        veh.IsPersistent = false;
                        EventCooldownTime = EventCooldownTime + 20000;
                        ScenarioFlow.Add(ScenarioType.AmbientTuner);
                        //         BennysCooldown = Game.GameTime + (1000 * 60 * 5);
                        return;
                    }
                }
            }
            /*
            foreach(Vehicle car in AllVehicles)
            {
                if((CanWeUse(car) && (car.ClassType == VehicleClass.Muscle || car.ClassType == VehicleClass.Super || car.ClassType == VehicleClass.Sports) && car.IsInRangeOf(Game.Player.Character.Position, 40f) && car.Speed<20f))
                {
                    Vector3 p = car.Position;

                    if (!Function.Call<bool>(Hash.IS_SHOCKING_EVENT_IN_SPHERE, 113, p.X, p.Y, p.Z, 30f))
                    {
                        Function.Call(Hash.ADD_SHOCKING_EVENT_FOR_ENTITY, 113, car, 30f);
                     //   UI.Notify("Added cool car event for " + car.FriendlyName);
                    }
                }
            }            
            Vector3 pPos = Game.Player.Character.Position;
            if (Game.Player.Character.IsOnFoot && Game.Player.Character.Weapons.Current.Hash != WeaponHash.Unarmed)
            {
                if (!Function.Call<bool>(Hash.IS_SHOCKING_EVENT_IN_SPHERE, 112, pPos.X, pPos.Y, pPos.Z, 30f))
                {
                    Function.Call(Hash.ADD_SHOCKING_EVENT_FOR_ENTITY, 112, Game.Player.Character.Weapons.CurrentWeaponObject, 5f);
            //        UI.ShowSubtitle("Visible gun event respawned.");
                }
            }
            */
            if (CurrentlyAllowedScenarios.Contains(ScenarioType.PlayerCoolCarPhoto))
            {

                Vehicle v = Game.Player.Character.LastVehicle;
                if (CanWeUse(v) && (v.Livery != 0 || new List<VehicleClass> { VehicleClass.Super, VehicleClass.Sports, VehicleClass.Muscle }.Contains(v.ClassType) || v.GetMod(VehicleMod.Livery) != -1))
                {
                    Vector3 p = v.Position;

                    if (!Function.Call<bool>(Hash.IS_SHOCKING_EVENT_IN_SPHERE, 113, p.X, p.Y, p.Z, 30f))
                    {
                        Function.Call(Hash.ADD_SHOCKING_EVENT_FOR_ENTITY, 113, v, 30f);
                    }
                }
                if (!CanWeUse(CoolCarPed) && !PhotoDone)
                {
                    foreach (Ped p in AllPeds)
                    {
                        if (CanWeUse(p) && p.IsInRangeOf(Game.Player.Character.Position, 40f) && p.IsInRangeOf(v.Position, 30f) && Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, p, 89))
                        {

                            /*
                            foreach (Vehicle candidate in AllVehicles)
                            {
                                if (CanWeUse(candidate))
                                {
                                    Vector3 candP = v.Position;
                                    if (Function.Call<bool>(Hash.IS_SHOCKING_EVENT_IN_SPHERE, 113, candP.X, candP.Y, candP.Z, 5f))
                                    {
                                        CoolCarPed = p;

                                        coolcar = candidate;
                                        SetDecorBool("Ignore", p, true);
                                        if (p.Gender == Gender.Male) UI.ShowSubtitle("~b~[Guy]~w~ Cool car man, mind if I take a photo?", 3000); else UI.ShowSubtitle("~b~[Girl]~w~ Cool car man, mind if I take a photo?", 3000);

                                        // UI.Notify(Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, p, "amb@world_human_tourist_mobile_car@male@base", "").ToString());
                                        break;
                                    }
                                }
                            }
                            */
                            CoolCarPed = p;
                            coolcar = v;
                            if (p.Gender == Gender.Male) UI.ShowSubtitle("~b~[Guy]~w~ Cool car man, mind if I take a photo?", 3000); else UI.ShowSubtitle("~b~[Girl]~w~ Cool car man, mind if I take a photo?", 3000);

                        }

                    }

                }
            }

            //Handlers
            if (TemporalPersistence.Count > 0)
            {
                if (Debug >= DebugLevel.Everything) UI.Notify("Persistent entities: " + TemporalPersistence.Count);
                for (int i = 0; i < TemporalPersistence.Count - 1; i++)
                {
                    if (!CanWeUse(TemporalPersistence[i]))
                    {
                        TemporalPersistence.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        if (!TemporalPersistence[i].IsInRangeOf(Game.Player.Character.Position, 200f))
                        {
                            if (Debug >= DebugLevel.Everything) UI.Notify("Removing persistent ~b~" + TemporalPersistence[i].ToString() + ""); // 
                            TemporalPersistence[i].IsPersistent = false;
                            TemporalPersistence.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            if (BarnCars.Count > 0)
            {

                foreach (Vehicle ve in BarnCars)
                {
                    if (CanWeUse(ve))
                    {
                        if (BarnFindTutorial < Game.GameTime && Game.Player.Character.IsInRangeOf(ve.Position, 20f) && !ve.IsOccluded)
                        {
                            BarnFindTutorial = Game.GameTime + 40000;
                            AddQueuedHelpText("You can fix this car in any LS Customs if you manage to move it there.");
                        }
                        if (ve.EngineHealth > 500)
                        {
                            Function.Call(Hash.SET_ENTITY_RENDER_SCORCHED, ve, false);
                            ve.EnginePowerMultiplier = 1;
                            ve.IsDriveable = true;
                            if (ve.CurrentBlip.Exists()) ve.CurrentBlip.Remove();
                            //UI.Notify("fixed");
                            BarnCars.Remove(ve);
                            break;
                        }
                    }
                    else
                    {
                        //UI.Notify("removed");
                        BarnCars.Remove(ve);
                        break;
                    }
                }
            }
            else
            {
                // UI.ShowSubtitle("barn is empty");

            }

            // if (Racers.Count > 0) HandleRacers();


            HandleCarjacker();
            int prob = 1;
            if (IsNightTime()) prob += 2;
            if (IsInNamedArea(Game.Player.Character, "city") && RandomInt(0, 10) <= 2)

                //if(Game.Player.Character.IsInRangeOf(NoEventsHere,500f)






                if (Taxis.Count > 0)
                {
                    if (Taxis[0].Finished)
                    {
                        Taxis[0].Clear();
                        Taxis.RemoveAt(0);
                    }
                    else foreach (TaxiEvent t in Taxis) t.Process();
                }

            if (DrugDeals.Count > 0)
            {
                if (DrugDeals[0].Finished)
                {
                    DrugDeals[0].Clear();
                    DrugDeals.RemoveAt(0);
                }
                else foreach (DrugDeal deal in DrugDeals) deal.Process();
            }


            if (Hunters.Count > 0)
            {
                if (Hunters[0].Finished)
                {
                    Hunters[0].Clear();
                    Hunters.RemoveAt(0);
                }
                else foreach (Hunter h in Hunters) h.Process();
            }
        }
    }


    int InteractionCooldown = 20000;
    int InteractionFrecuency = 50;

    void EventGenerator()
    {
        //Ambient
        if (SmallEventCooldownTime < Game.GameTime) //Interactive World events
        {
            SmallEventCooldownTime = Game.GameTime + InteractionCooldown; //future InteractionCooldown 

            if (!BlacklistedAreas.Contains(World.GetZoneNameLabel(Game.Player.Character.Position).ToLowerInvariant()))
            {
                if (RandomInt(0, 100) < InteractionFrecuency) Scenarios();
            }
            else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Player is in restricted area, Scenarios disabled.");
        }

        //Spawner
        if (EventCooldownTime < Game.GameTime) //Spawner events
        {
            EventCooldownTime = Game.GameTime + EventCooldown;
            if (!BlacklistedAreas.Contains(World.GetZoneNameLabel(Game.Player.Character.Position).ToLowerInvariant()))
            {
                if (AllVehicles.Length > 10) HandleEvents();
                else if (Debug >= DebugLevel.EventsAndScenarios) { UI.Notify("~b~[Ambient Events]~w~: Not enough traffic to spawn anything."); EventCooldownTime = Game.GameTime + 60000; }

            }
            else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Player is in restricted area, SpawnerEvents disabled.");
        }

        //Car replacer
        if (ReplacerTime < Game.GameTime) //Replacers
        {
            ReplacerTime = Game.GameTime + 10000;
            if (TrafficInjector)
            {
                foreach (TrafficSpawner tr in TrafficSpawnerList)
                    if (tr.Process())
                    {
                        ReplacerTime = Game.GameTime + 5000;
                        break;
                    }
            }
            if (VehicleReplacer) foreach (Replacer ev in ReplacersList) if (!BlacklistedModels.Contains(Game.GenerateHash(ev.SourceVehicle))) if (ev.Process()) break;
        }
    }
    public static double AngleBetween(Vector3 u, Vector3 v, bool returndegrees)
    {
        double toppart = 0;
        for (int d = 0; d < 3; d++) toppart += u[d] * v[d];

        double u2 = 0; //u squared
        double v2 = 0; //v squared
        for (int d = 0; d < 3; d++)
        {
            u2 += u[d] * u[d];
            v2 += v[d] * v[d];
        }

        double bottompart = 0;
        bottompart = Math.Sqrt(u2 * v2);


        double rtnval = Math.Acos(toppart / bottompart);
        if (returndegrees) rtnval *= 360.0 / (2 * Math.PI);
        return rtnval;
    }

    int t = 0;
    int hiccupdetector = Game.GameTime;


    int GametimeMinute = 0;
    int GametimeThirtySecs = 0;
    int AllVehiclesCurrent = 0;
    int GameTimeVeryShort = 0;
    int GameTimeAllCars = 0;
    List<Model> CopVehicles = new List<Model>();


    public static void DrawLine(Vector3 from, Vector3 to, Color color)
    {
        Function.Call(Hash.DRAW_LINE, from.X, from.Y, from.Z, to.X, to.Y, to.Z, color.R, color.G, color.B, color.A);
    }
    void OnTick(object sender, EventArgs e)
    {
        /*
        DisplayHelpTextThisFrame(GetRoadFlags(Game.Player.Character.Position));
        Vector3 poss = GenerateSpawnPos(Game.Player.Character.Position, Nodetype.Road, false);
        World.DrawMarker(MarkerType.ChevronUpx1, poss + (Vector3.WorldUp * 2), Vector3.Zero, -Vector3.WorldDown, new Vector3(10f, 10f, -10f), Color.DarkRed);
                DrawLine(Game.Player.Character.Position, poss, Color.Red);

        */
        if (GametimeMinute < Game.GameTime)
        {
            GametimeMinute = Game.GameTime + 60000;

            if (Eventflow.Count > 0) Eventflow.RemoveAt(0);
        }

        if (GametimeThirtySecs < Game.GameTime)
        {
            GametimeThirtySecs = Game.GameTime + 30000;
            if (ScenarioFlow.Count > 2) ScenarioFlow.RemoveAt(0);
            else if (ScenarioFlow.Count>0 && RandomInt(0, 10)<2) ScenarioFlow.RemoveAt(0);
        }

        if (GameTimeVeryShort < Game.GameTime)
        {
            GameTimeVeryShort = Game.GameTime + 5;
            if (FadeIn.Count > 0)
            {
                Entity fadeInE = FadeIn[0];

                if (CanWeUse(FadeIn[0]))
                {
                    if (fadeInE.Alpha < 255)
                    {
                        int fadeInt = fadeInE.Alpha + 10;
                        if (fadeInt >= 255) fadeInE.Alpha = 255; else fadeInE.Alpha = fadeInt;
                    }
                    else
                    {
                        //UI.Notify("finished " + fadeInE);
                        FadeIn.RemoveAt(0);
                    }
                }
                else
                {
                    FadeIn.RemoveAt(0);
                }
            }
        }

        //    UI.ShowSubtitle(Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, Game.Player.Character, "amb@world_human_tourist_mobile_car@male@base", "base").ToString(), 40);

        if (GameTimeAllCars < Game.GameTime)
        {
            GameTimeAllCars = Game.GameTime + 400;
            AllVehiclesChecker();

        }


        if (GameTimeRefLong < Game.GameTime)
        {
            GameTimeRefLong = Game.GameTime + 5000;

            if (CopVehicles.Count > 3) CopVehicles.RemoveAt(0);


            RebuildPools();
            if (Debug >= DebugLevel.Everything) UI.ShowSubtitle("Context: ~b~" + CurrentPlayerContext.ToString());

        }

        if (Game.GameTime > hiccupdetector + 500)
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~r~Game hiccup detected");
            if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - HICCUP DETECTED (" + (Game.GameTime - hiccupdetector) + "ms) - check the output above this to find the culprit");
        }

        hiccupdetector = Game.GameTime;
        //DisplayHelpTextThisFrame(AnyVehicleNear(Game.Player.Character.Position, 30f).ToString());

        /*
        if(CanWeUse(Game.Player.Character.CurrentVehicle))
        {
            if (t < Game.GameTime)
            {
                t = Game.GameTime + 500;
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");
                if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core"))
                {
                    Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
                    Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "exp_grd_vehicle_post", Game.Player.Character.CurrentVehicle, 0.0f, 0.0f, 0.0f, 0, 0, 0, 0.7f, 0, 1, 0);
                }
            }

        }
        */

        //DisplayHelpTextThisFrame("Heading:" +Game.Player.Character.Heading+", difference: " +(AngleBetweenVectors(Game.Player.Character.Position, GameplayCamera.Position)).ToString());

        HandleNotifications();
        HandleConversation();
        HandleMessages();
        if (Game.IsControlJustPressed(2, GTA.Control.Context))
        {
            if (Hunters.Count > 0 && Hunters[0].HunterPed.IsInRangeOf(Game.Player.Character.Position, 15) && !Hunters[0].HunterPed.IsInCombat)
            {
                LivelyWorld.AddQueuedConversation("~b~[" + Game.Player.Name + "]:~w~ Hey pal, that direction.");
                Hunters[0].HelpHunter();
            }
        }

        EventGenerator();
        ProcessCheats();
        ProcessLists();
        ProcessCurrentEvents();
    }


    Ped Overtaker = null;
    Ped Overtaken = null;

    int HandleFlabedsCooldown = 0;
    int FreightTruckCooldown = 0;
    void AllVehiclesChecker()
    {
        if (AllVehicles == null) AllVehicles = World.GetAllVehicles();
        int Towed = 0;
        if (AllVehicles.Length > 0)
        {
            for (int i = 0; i < 20; i++)
            {
                AllVehiclesCurrent++;
                if (Debug >= DebugLevel.Everything) UI.ShowSubtitle(AllVehiclesCurrent.ToString() + " of " + AllVehicles.Length, 500);
                if (AllVehiclesCurrent > AllVehicles.Length - 1)
                {
                    AllVehiclesCurrent = 0;
                    AllVehicles = World.GetAllVehicles();
                    //  UI.ShowSubtitle("Refreshed",500);
                }
                Vehicle veh = AllVehicles[AllVehiclesCurrent];
                if (CanWeUse(veh)  && !BlacklistedVehicles.Contains(veh) && !veh.PreviouslyOwnedByPlayer && !veh.IsPersistent && !DecorExistsOn("LWIgnore", veh)) // 
                {
                    if (!veh.IsInRangeOf(Game.Player.Character.Position, InteractionRange) && !IsEntityApproachingEntity(Game.Player.Character, veh))
                    {
                        continue;
                    }
                    if (CurrentlyAllowedScenarios.Contains(ScenarioType.ImprovedTowtrucks) && !ScenarioFlow.Contains(ScenarioType.ImprovedTowtrucks) && Towed < 4 && veh.HasBone("tow_arm"))
                    {
                        Ped p = GetOwner(veh);
                         if (!CanWeUse(p) || p.IsPlayer) continue;
                        if (!veh.IsPersistent && !CanWeUse(Function.Call<Vehicle>(GTA.Native.Hash.GET_ENTITY_ATTACHED_TO_TOW_TRUCK, veh))) // (veh.Model == "towtruck" || veh.Model == "towtruck2")
                        {
                            BlacklistedVehicles.Add(veh);
                            AttachRandomCarToTow(veh);
                            Towed++;
                            continue;
                        }
                    }

                    if (!CanWeUse(Overtaker) && CurrentlyAllowedScenarios.Contains(ScenarioType.DriverOvertake) && !ScenarioFlow.Contains(ScenarioType.DriverOvertake) && veh.Speed > 20f && IsRoadBusy(veh.Position) < 10)
                    {
                        if (veh.Model.IsCar || veh.Model.IsBike || veh.Model.IsBicycle)
                        {
                            if (CanWeUse(veh.Driver) && !veh.Driver.IsPlayer && !veh.Driver.IsInCombat && veh.IsInRangeOf(Game.Player.Character.Position, InteractionRange))
                            {
                                RaycastResult D = World.Raycast(veh.Position + (veh.ForwardVector * (veh.Model.GetDimensions().Y / 2)), veh.ForwardVector * 30f, 30f, IntersectOptions.Everything, veh);
                                if (CanWeUse(D.HitEntity) && D.HitEntity.IsInRangeOf(veh.Position, 20f) && (D.HitEntity.Model.IsVehicle))
                                {
                                    Overtaken = (D.HitEntity as Vehicle).Driver;
                                    Overtaker = veh.Driver;
                                    Overtaker.IsPersistent = true;
                                    Overtaker.AlwaysKeepTask = true;
                                    if(Debug>= DebugLevel.EventsAndScenarios) UI.Notify(veh.FriendlyName + " overtakes "+ (D.HitEntity as Vehicle).FriendlyName);
                                    Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, Overtaker, veh, veh.Speed + 20f, 2 + 4 + 8 + 16 + 32 + 128 + 512);
                                    Function.Call(GTA.Native.Hash.SET_DRIVER_ABILITY, Overtaker, 100f);
                                    if (!Overtaken.IsPersistent) Overtaken.IsPersistent = true;
                                    BlacklistedVehicles.Add(veh);


                                    if (DebugBlips && !veh.CurrentBlip.Exists())
                                    {
                                        veh.AddBlip();
                                        veh.CurrentBlip.Sprite = BlipSprite.RaceCar;
                                        //veh.CurrentBlip.Scale = 0.7f;
                                        veh.CurrentBlip.IsShortRange = true;
                                        veh.CurrentBlip.Name = "Overtaking " + veh.FriendlyName;
                                        veh.CurrentBlip.Color = BlipColor.Green;
                                    }
                                    ScenarioFlow.Add(ScenarioType.DriverOvertake);

                                    continue;
                                }
                            }
                        }
                    }

                    //Better dispatch
                    if (veh.Model.IsCar && isCopVehicleRange(veh.Position, veh.Model.GetDimensions().X))
                    {
                        if (!CopVehicles.Contains(veh.Model)) CopVehicles.Add(veh.Model);
                
                    //Police carrying bikes
                    if (new Model("cycle").IsValid && CurrentlyAllowedScenarios.Contains(ScenarioType.PoliceCarCarryingBike) && !ScenarioFlow.Contains(ScenarioType.PoliceCarCarryingBike) && !veh.IsAttached())
                    {
                        int Chance = 5;
                        if (NearBeachAreas.Contains(World.GetZoneNameLabel(veh.Position))) Chance = 30;
                        int Rand = RandomInt(0, 100);
                        if (Rand < Chance)
                        {
                                if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Attaching cop bike to " + veh.DisplayName );
                                Model BikeModel = "cycle";
                                if (BikeModel.IsValid)
                                {
                                    Vehicle bike = AttachBikeToCar(veh, BikeModel);
                                    if (CanWeUse(bike))
                                    {
                                        bike.Alpha = 0;
                                        FadeIn.Add(bike);
                                        BlacklistedVehicles.Add(veh);
                                        if (Debug >= DebugLevel.EventsAndScenarios)
                                        {
                                            UI.Notify("Spawned a police bicycle on " + veh.FriendlyName + " (~g~" + Chance + "%~w~ chance)");
                                        }
                                        
                                        if (!veh.CurrentBlip.Exists())
                                        {
                                            veh.AddBlip();
                                            veh.CurrentBlip.Color = BlipColor.Blue;
                                            veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                            veh.CurrentBlip.Scale = 0.7f;
                                            veh.CurrentBlip.IsShortRange = true;
                                            veh.CurrentBlip.Name = "Bike Rack";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Debug >= DebugLevel.EventsAndScenarios)
                                {
                                    UI.Notify("Failed chance to spawn a police bicycle on " + veh.FriendlyName + " (~r~" + Chance + "%~w~ chance)");
                                }
                            }
                            ScenarioFlow.Add(ScenarioType.PoliceCarCarryingBike);
                            continue;
                        }
                    }

                    if (CurrentlyAllowedScenarios.Contains(ScenarioType.CarCarryingBike) && !ScenarioFlow.Contains(ScenarioType.CarCarryingBike) &&  !veh.IsAttached())
                    {
                        if (new List<VehicleClass> { VehicleClass.OffRoad, VehicleClass.Sedans, VehicleClass.Super }.Contains(veh.ClassType)==false) continue;
                        int Chance = 2;
                        if (NearBeachAreas.Contains(World.GetZoneNameLabel(veh.Position))) Chance = 30;
                        int Rand = RandomInt(0, 100);
                        if (Chance > Rand)
                        {
                            if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Attaching normal bike to " + veh.DisplayName);

                            Model BikeModel = "";

                            List<Model> cycles = new List<Model> { "bmx", "bmx2", "scorcher" };

                            for (int tries = 0; tries < 20; tries++) if (!BikeModel.IsValid) BikeModel = cycles[RandomInt(0, cycles.Count - 1)];
                            //if (!BikeModel.IsValid || RandomInt(0, 100) > 40) BikeModel = "bmx";
                            if (BikeModel.IsValid)
                            {
                                Vehicle bike = AttachBikeToCar(veh, BikeModel);
                                if (CanWeUse(bike))
                                {
                                    bike.Alpha = 0;
                                    FadeIn.Add(bike);
                                    BlacklistedVehicles.Add(veh);
                                    if (Debug >= DebugLevel.EventsAndScenarios)
                                    {
                                        UI.Notify("Spawned a bicycle on " + veh.FriendlyName + " (~g~" + Chance + "%~w~ chance)");
                                    }

                                    if (DebugBlips && !veh.CurrentBlip.Exists())
                                    {
                                        veh.AddBlip();
                                        veh.CurrentBlip.Color = BlipColor.White;
                                        veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                        veh.CurrentBlip.Scale = 0.7f;
                                        veh.CurrentBlip.IsShortRange = true;
                                        veh.CurrentBlip.Name = "Bike Rack";
                                    }
                                    ScenarioFlow.Add(ScenarioType.CarCarryingBike);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (Debug >= DebugLevel.EventsAndScenarios)
                            {
                                UI.Notify("Failed chance to spawn a  bicycle on " + veh.FriendlyName + " (~r~" + Chance + "%~w~ chance)");
                            }
                        }
                    }



                    //IsInNamedArea(Game.Player.Character, "Senora Fwy") || IsInNamedArea(Game.Player.Character, "Great Ocean Hwy")
                    if (CurrentlyAllowedScenarios.Contains(ScenarioType.ImprovedFreight) && !ScenarioFlow.Contains(ScenarioType.ImprovedFreight) && veh.HasBone("attach_female") && veh.ClassType == VehicleClass.Commercial &&
                        !BlacklistedVehicles.Contains(veh) && !Game.Player.Character.IsInRangeOf(veh.Position, 20f) && !veh.IsPersistent && CanWeUse(veh.Driver))
                    {//(veh.Model == "juggernaut" || veh.Model == "packer" || veh.Model == "hauler" || veh.Model == "phantom" || veh.Model == "roadkiller") !DecorExistsOn("LWIgnore", veh) 
                        Vehicle trailer = GetTrailer(veh);

                        if (!CanWeUse(trailer) && !Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, veh))
                        {
                            veh.IsPersistent = true;
                            Script.Wait(200);
                            Vector3 pos = veh.Position + (veh.ForwardVector * -10);
                            TemporalPersistence.Add(veh);

                            trailer = World.CreateVehicle(VehicleHash.FreightTrailer, pos, veh.Heading);
                            trailer.Speed = veh.Speed;
                            TemporalPersistence.Add(trailer);

                            if (DebugBlips)
                            {
                                if (!veh.CurrentBlip.Exists())
                                {
                                    veh.AddBlip();
                                    veh.CurrentBlip.Color = BlipColor.White;
                                    veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    veh.CurrentBlip.IsShortRange = false;
                                    veh.CurrentBlip.Name = "Freight Truck";
                                }
                                if (!trailer.CurrentBlip.Exists())
                                {
                                    trailer.CurrentBlip.Color = BlipColor.White;
                                    trailer.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    trailer.CurrentBlip.IsShortRange = false;
                                    trailer.CurrentBlip.Name = "Freight trailer";
                                }
                            }
                            if (RandomInt(0, 10) <= 5)
                            {
                                Vehicle cargo = World.CreateVehicle(VehicleHash.Annihilator, pos + new Vector3(0, 3, 5), veh.Heading);
                                TemporalPersistence.Add(cargo);
                                cargo.ToggleExtra(1, false);
                                cargo.AttachTo(trailer, 0, (new Vector3(0, 2, -0.4f)), new Vector3(0, 0, 0));
                                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("[FreighTrucks] Freight (annihilator) spawned on a " + veh.FriendlyName + ".");
                            }
                            else
                            {
                                Vehicle cargo = World.CreateVehicle(VehicleHash.Buzzard2, pos + new Vector3(0, 3, 5), veh.Heading);
                                TemporalPersistence.Add(cargo);
                                cargo.ToggleExtra(1, false);
                                cargo.AttachTo(trailer, 0, (new Vector3(0, -5, -0.3f)), new Vector3(0, 0, 0));

                                cargo = World.CreateVehicle(VehicleHash.Buzzard2, pos + new Vector3(0, 3, 5), veh.Heading);
                                TemporalPersistence.Add(cargo);
                                cargo.ToggleExtra(1, false);
                                cargo.AttachTo(trailer, 0, (new Vector3(0, 5, -0.3f)), new Vector3(0, 0, 0));
                                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("[FreighTrucks] Freight (helis) spawned on a " + veh.FriendlyName + ".");
                            }

                            Function.Call(Hash.ATTACH_VEHICLE_TO_TRAILER, veh, trailer, 50);
                            FreightTruckCooldown = Game.GameTime + (1000 * 60 * RandomInt(2, 6));
                            BlacklistedVehicles.Add(veh);
                            continue;
                        }
                    }

                    if (CurrentlyAllowedScenarios.Contains(ScenarioType.ImprovedFlatbeds) && !ScenarioFlow.Contains(ScenarioType.ImprovedFlatbeds) && veh.Speed < 20f && HandleFlabedsCooldown < Game.GameTime  && !CanWeUse(GetAttachedVehicle(veh, false)) &&
                        !BlacklistedVehicles.Contains(veh) && (!veh.IsOnScreen || !Game.Player.Character.IsInRangeOf(veh.Position, 50f)) && (CarrierVehicles.Contains(veh.Model) && CanWeUse(GetOwner(veh))))
                    {
                        if (Debug >= DebugLevel.Everything) UI.Notify("Got carrier, " + veh.FriendlyName);
                        string vehicle = RandomNormalVehicle();

                        veh.IsPersistent = true;
                        Vehicle cargo = World.CreateVehicle(vehicle, veh.Position + (veh.ForwardVector * -5f));

                        if (CanWeUse(cargo))
                        {
                            cargo.Alpha = 0;
                            FadeIn.Add(cargo);
                            cargo.PlaceOnGround();
                            //cargo.Speed = veh.Speed;

                            //veh.Speed = veh.Speed / 2f; //veh.FreezePosition = true;
                            HandleFlabedsCooldown = Game.GameTime + (1000 * RandomInt(2, 6));


                            // cargo.IsPersistent = false;
                            Attach(veh, cargo);

                            if (!veh.CurrentBlip.Exists() && DebugBlips)
                            {
                                veh.AddBlip();
                                veh.CurrentBlip.Color = BlipColor.White;
                                veh.CurrentBlip.Name = "Carrier";
                                veh.CurrentBlip.IsShortRange = true;
                            }
                            // veh.FreezePosition = false;
                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("[Carriers] Attached " + cargo.FriendlyName + " on " + veh.FriendlyName + ".");
                            TemporalPersistence.Add(cargo);
                            TemporalPersistence.Add(veh);
                            BlacklistedVehicles.Add(veh);

                            ScenarioFlow.Add(ScenarioType.ImprovedFlatbeds);
                            continue;

                        }
                    }

                    if (TruckRespray && Respraymodels.Contains(veh.Model) && !veh.IsPersistent && !BlacklistedVehicles.Contains(veh) && !Game.Player.Character.IsInRangeOf(veh.Position, 40f))
                    {
                        ResprayTruck(veh);
                        BlacklistedVehicles.Add(veh);
                    }



                    if (CurrentlyAllowedScenarios.Contains(ScenarioType.StrippedCar) && !ScenarioFlow.Contains(ScenarioType.StrippedCar) && IsSuitableForPlayerToExperience(veh.Position, 150f) &&
                        !WouldPlayerNoticeChangesHere(veh.Position) && (GangAreas.Contains(World.GetZoneNameLabel(veh.Position)) && new List<VehicleClass> { VehicleClass.Emergency }.Contains(veh.ClassType) == false))
                    {
                        if (!CanWeUse(veh.Driver))
                        {
                            StripOfAllPossible(veh, true, true, true, true, true, true);

                            if (DebugBlips && !veh.CurrentBlip.Exists())
                            {
                                veh.AddBlip();
                                veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                veh.CurrentBlip.Scale = 0.7f;
                                veh.CurrentBlip.IsShortRange = true;
                                veh.CurrentBlip.Name = "Stripped " + veh.FriendlyName;
                                veh.CurrentBlip.Color = BlipColor.Red;
                            }
                            //veh.PrimaryColor = VehicleColor.WornBrown;
                            // veh.SecondaryColor = VehicleColor.WornBrown;
                            BlacklistedVehicles.Add(veh);
                            ScenarioFlow.Add(ScenarioType.StrippedCar);
                            continue;
                        }
                    }

                    if (CurrentlyAllowedScenarios.Contains(ScenarioType.LoudRadio) && !ScenarioFlow.Contains(ScenarioType.LoudRadio) && IsSuitableForPlayerToExperience(veh.Position, 40f))
                    {
                        if (30 < RandomInt(0, 100))
                        {
                            if (!veh.IsPersistent && veh.EngineRunning && CanWeUse(veh.Driver))
                            {
                                veh.IsRadioEnabled = true;

                                Function.Call(Hash.SET_VEHICLE_RADIO_LOUD, veh, true);
                                if (DebugBlips && !veh.CurrentBlip.Exists())
                                {
                                    veh.AddBlip();
                                    veh.CurrentBlip.Sprite = BlipSprite.SonicWave;
                                    veh.CurrentBlip.Scale = 0.5f;
                                    veh.CurrentBlip.IsShortRange = true;
                                    veh.CurrentBlip.Name = "RadioBlasting " + veh.FriendlyName;
                                    veh.CurrentBlip.Color = BlipColor.Green;
                                }
                            }
                        }
                        ScenarioFlow.Add(ScenarioType.LoudRadio);
                        continue;
                    }


                    if (CurrentlyAllowedScenarios.Contains(ScenarioType.StoppedAtLightsInteraction) && !ScenarioFlow.Contains(ScenarioType.StoppedAtLightsInteraction) && !veh.IsPersistent && IsSuitableForPlayerToExperience(veh.Position, 20f))
                    {
                        if (veh.IsStoppedAtTrafficLights)
                        {
                            Ped driver = veh.Driver;
                            if (CanWeUse(driver) && !driver.IsPlayer)
                            {
                                if (veh.GetModCount(VehicleMod.Hydraulics) > 0)
                                {
                                    veh.ApplyForceRelative(veh.UpVector * 5);
                                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("LowriderJump for " + veh.FriendlyName);
                                }
                                else if (new List<VehicleClass> { VehicleClass.Muscle, VehicleClass.Sports, VehicleClass.SportsClassics, VehicleClass.Super }.Contains(veh.ClassType))
                                {
                                    Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, driver, veh, 30, 3000);
                                    ScenarioFlow.Add(ScenarioType.StoppedAtLightsInteraction);
                                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Burnout for " + veh.FriendlyName);
                                }
                                else
                                {
                                    veh.SoundHorn(2000);
                                    ScenarioFlow.Add(ScenarioType.StoppedAtLightsInteraction);
                                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Honk for " + veh.FriendlyName);
                                }
                            }
                            continue;
                        }
                    }
                }
            }
        }
        else AllVehicles = World.GetAllVehicles();
        //if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify(MonitoredVehicles.Count + " specific cars being monitored of " + AllVehicles.Count + ". ~n~(" + names + ")");

    }
    void AttachRandomCarToTow(Vehicle tow)
    {
        if (CanWeUse(Function.Call<Vehicle>(GTA.Native.Hash.GET_ENTITY_ATTACHED_TO_TOW_TRUCK, tow))) return;
        Script.Wait(500);
        //Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER, towdriver, tow, 15f, (1 + 24 + 16 + 32 + 262144));

        Model model = GetRandomVehicleHash();
        if (model.Hash == tow.Model.Hash) model = "blista";
        Vehicle towed = World.CreateVehicle(model, tow.Position - (tow.ForwardVector * (tow.Model.GetDimensions().Y+0.5f)), tow.Heading);

        if (CanWeUse(towed))
        {
            //Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, tow, 4f);
            Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, towed, tow.Speed);

            Function.Call(Hash.ATTACH_VEHICLE_TO_TOW_TRUCK, tow, towed, false, 0, 0, 0);
            Function.Call(Hash._SET_TOW_TRUCK_CRANE_RAISED, tow, 1f);

            if (DebugBlips && !tow.CurrentBlip.Exists())
            {
                tow.AddBlip();
                //tow.CurrentBlip.Color = BlipColor.Green;
                tow.CurrentBlip.Sprite = BlipSprite.TowTruck;
                tow.CurrentBlip.Scale = 0.7f;
                tow.CurrentBlip.IsShortRange = true;
            }
            tow.EngineRunning = true;
            tow.SirenActive = true;
            tow.IsPersistent = false;
            //towdriver.IsPersistent = false;
            towed.IsPersistent = false;

            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Tow + vehicle spawned");
            BlacklistedVehicles.Add(tow);
            BlacklistedVehicles.Add(towed);

            towed.Alpha = 0;
            FadeIn.Add(towed);
        }
        //towed.Heading = tow.Heading;



    }

    public Vehicle CarjackerTarget = null;
    public Ped Carjacker = null;
    bool CarjackerEnabled;
    int CarjackerPhase = 0;
    int LookOutTime = 0;
    public void HandleCarjacker()
    {

        if (!CanWeUse(CarjackerTarget) && AllVehicles.Length < 10)
        {
            CarjackerEnabled = false;
            CarjackerPhase = 0;
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~r~Carjacker event cancelled, not enough vehicles.");
            Carjacker = null;
            CarjackerTarget = null;
            return;

        }

        if (CarjackerEnabled)
        {
            if (CanWeUse(Carjacker) && CanWeUse(CarjackerTarget) && Carjacker.IsSittingInVehicle(CarjackerTarget))
            {
                if (Carjacker.RelationshipGroup != CriminalRelGroup) Carjacker.RelationshipGroup = CriminalRelGroup;
            }
            if (!CanWeUse(CarjackerTarget))
            {
                foreach (Vehicle veh in AllVehicles)
                {

                    if (CanWeUse(veh) && !isCopInRange(veh.Position, 100f) && !DecorExistsOn("LWIgnore", veh) && veh.IsDriveable && !HasOwner(veh, true) && veh.Health > 500 && veh.IsOnAllWheels && new List<VehicleClass> { VehicleClass.Compacts, VehicleClass.Muscle, VehicleClass.Sports, VehicleClass.SportsClassics, VehicleClass.Super }.Contains(veh.ClassType) && Function.Call<bool>(Hash.IS_ENTITY_OCCLUDED, veh) && veh.IsStopped && !CanWeUse(veh.Driver) && veh.IsInRangeOf(Game.Player.Character.Position, 60f))
                    {
                        CarjackerTarget = veh;
                        CarjackerTarget.IsPersistent = true;
                        BlacklistedVehicles.Add(CarjackerTarget);
                        TemporalPersistence.Add(CarjackerTarget);
                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~[Carjacker]~w~ got vehicle to jack, " + veh.FriendlyName);
                        break;
                    }
                }
                return;
            }

            if (CanWeUse(Carjacker))
            {
                if (Carjacker.IsInCombat || Carjacker.IsDead || !Carjacker.IsInRangeOf(Game.Player.Character.Position, 200f))
                {
                    CarjackerEnabled = false;
                    CarjackerPhase = 0;
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~r~Carjacker event cancelled, carjacker interrupted.");
                    if (Carjacker.CurrentBlip.Exists()) Carjacker.CurrentBlip.Remove();

                    return;
                }
            }
            if (CarjackerPhase > 0 && (!CanWeUse(CarjackerTarget) || !CanWeUse(Carjacker)))
            {
                CarjackerEnabled = false;
                CarjackerPhase = 0;
                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~r~Carjacker event cancelled.");
                Carjacker = null;
                CarjackerTarget = null;
                return;
            }




            switch (CarjackerPhase)
            {
                case 0:
                    {

                        if (!CanWeUse(Carjacker))
                        {
                            Vector3 pos = World.GetSafeCoordForPed(CarjackerTarget.Position.Around(20f)); // Game.Player.Character.Position.Around(5);//GetQuietPlace();
                            if (pos == Vector3.Zero) pos = GenerateSpawnPos(CarjackerTarget.Position.Around(30f), Nodetype.Offroad, true);
                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("creating carjacker...");

                            Carjacker = World.CreatePed("s_m_y_dealer_01", pos, 0);
                            Carjacker.AlwaysKeepTask = true;
                            Carjacker.Weapons.Give(WeaponHash.Pistol, 60, false, true);

                            Carjacker.Alpha = 0;
                            FadeIn.Add(Carjacker);

                            if (DebugBlips && !Carjacker.CurrentBlip.Exists())
                            {
                                Carjacker.AddBlip();
                                Carjacker.CurrentBlip.Scale = 0.5f;
                                Carjacker.CurrentBlip.IsShortRange = true;
                            }

                            //CarjackerPhase++;
                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("carjacker created");
                        }
                        else
                        {
                            CarjackerPhase++;
                        }
                        break;
                    }
                case 1:
                    {


                        if (!Carjacker.IsInRangeOf(CarjackerTarget.Position, 5f))
                        {
                            if (IsIdle(Carjacker))
                            {
                                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~[Carjacker]~w~ tasked to go to target");

                                if (Carjacker.IsInRangeOf(CarjackerTarget.Position, 30f)) Function.Call(Hash.TASK_GO_TO_ENTITY, Carjacker, CarjackerTarget, -1, 3f, 1f, 1073741824, 0);
                                else Function.Call(Hash.TASK_GO_TO_ENTITY, Carjacker, CarjackerTarget, -1, 25f, 2f, 1073741824, 0);

                            }
                            //Function.Call(Hash.TASK_PAUSE, 0, RandomInt(3, 8) * 1000);
                        }
                        else CarjackerPhase++;
                        break;
                    }
                case 2:
                    {


                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~[Carjacker]~w~ looking around");
                        LookOutTime = Game.GameTime + 10000;
                        TaskSequence seq = new TaskSequence();


                        Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, 0, "CODE_HUMAN_CROSS_ROAD_WAIT", 8000, true);

                        seq.Close();
                        Carjacker.Task.PerformSequence(seq);
                        seq.Dispose();
                        Carjacker.BlockPermanentEvents = false;
                        CarjackerPhase++;
                        break;
                    }
                case 3:
                    {


                        if (LookOutTime < Game.GameTime)
                        {

                            if (isCopInRange(CarjackerTarget.Position, 30f) || (IsEntityAheadEntity(Game.Player.Character, Carjacker) && CanPedSeePed(Carjacker, Game.Player.Character, false)))
                            {
                                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~[Carjacker]~w~ cops/player nearby, jacker leaves the scene");
                                Function.Call(Hash.TASK_WANDER_STANDARD, Carjacker, 100f, 10f);
                                if (Carjacker.CurrentBlip.Exists()) Carjacker.CurrentBlip.Color = BlipColor.White;

                            }
                            else
                            {
                                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~[Carjacker]~w~ no cops nearby, jacking");

                                TaskSequence seq = new TaskSequence();
                                Function.Call(Hash.TASK_ENTER_VEHICLE, 0, CarjackerTarget, 20000, -1, 1f, 1, 0);
                                Function.Call(Hash.TASK_PAUSE, 0, RandomInt(2, 4) * 1000);

                                Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, CarjackerTarget, 30f, 4 + 8 + 16 + 32);

                                seq.Close();
                                Carjacker.Task.PerformSequence(seq);
                                seq.Dispose();
                                // Carjacker.BlockPermanentEvents = false;

                            }
                            CarjackerPhase++;
                        }
                        break;
                    }
                case 4:
                    {


                        if (Carjacker.IsJacking) Carjacker.RelationshipGroup = CriminalRelGroup;

                        if (Carjacker.IsInVehicle(CarjackerTarget) || !Carjacker.IsInRangeOf(Game.Player.Character.Position, 100f))
                        {
                            if (Carjacker.CurrentBlip.Exists()) Carjacker.CurrentBlip.Color = BlipColor.White;

                            TemporalPersistence.Add(Carjacker);
                            TemporalPersistence.Add(CarjackerTarget);
                            //   CarjackerTarget.IsPersistent = false;
                            // Carjacker.IsPersistent = false;
                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~[Carjacker]~w~ Carjacker event finished");
                            CarjackerEnabled = false;
                            CarjackerPhase = 0;
                        }
                        break;
                    }
            }
        }
        else if (CarjackerPhase != 0) CarjackerPhase = 0;
    }

    public bool IsIdle(Ped ped)
    {
        if (ped.IsInCombat || !ped.IsStopped || ped.IsRagdoll) return false; else return true;
    }
    public static Random rnd = new Random();
    public static int RandomInt(int min, int max)
    {
        max++;
        return rnd.Next(min, max);
    }

    public enum Gang { Lost, Ballas, Vagos, Families };
    public void SpawnGangDriveBy(Gang g, Ped victim)
    {


        if (!CanWeUse(victim)) return;
        victim.IsPersistent = true;

        if (Debug >= DebugLevel.Everything) UI.Notify("Victim exists");

        Vector3 pos = GenerateSpawnPos(Game.Player.Character.Position.Around(100), Nodetype.Road, false);
        Vehicle veh = null;
        List<Ped> Peds = new List<Ped>();

        if (Debug >= DebugLevel.Everything) UI.Notify("setting rlgroups");
        int HateRLGroup = World.AddRelationshipGroup("hatetemp");
        int VictimRLGroup = World.AddRelationshipGroup("victimtemp");
        victim.RelationshipGroup = VictimRLGroup;
        foreach (Ped p in World.GetNearbyPeds(victim, 40f))
        {
            World.SetRelationshipBetweenGroups(Relationship.Respect, VictimRLGroup, p.RelationshipGroup);
            World.SetRelationshipBetweenGroups(Relationship.Respect, p.RelationshipGroup, VictimRLGroup);
        }


        World.SetRelationshipBetweenGroups(Relationship.Hate, HateRLGroup, victim.RelationshipGroup);
        World.SetRelationshipBetweenGroups(Relationship.Hate, victim.RelationshipGroup, HateRLGroup);
        if (Debug >= DebugLevel.Everything) UI.Notify("checking kind of gang");

        switch (g)
        {
            case Gang.Ballas:
                {
                    veh = World.CreateVehicle("tornado", pos);
                    veh.PrimaryColor = VehicleColor.MetallicPurple;
                    veh.SecondaryColor = VehicleColor.MatteDesertBrown;

                    for (int i = 0; i < veh.PassengerSeats + 1; i++)
                    {
                        Ped ped = World.CreatePed(BallasModels[RandomInt(0, BallasModels.Count - 1)], veh.Position.Around(5));
                        ped.RelationshipGroup = HateRLGroup;
                        ped.Weapons.Give(WeaponHash.Pistol, 99, true, true);

                        Peds.Add(ped);
                    }
                    break;
                }
            case Gang.Lost:
                {
                    veh = World.CreateVehicle("gburrito", pos);
                    veh.PrimaryColor = VehicleColor.MatteBlack;
                    for (int i = 0; i < veh.PassengerSeats + 1; i++)
                    {
                        Ped ped = World.CreatePed(LostModels[RandomInt(0, LostModels.Count - 1)], veh.Position.Around(5));
                        ped.RelationshipGroup = HateRLGroup;
                        ped.Weapons.Give(WeaponHash.Pistol, 99, true, true);
                        Peds.Add(ped);
                    }
                    break;
                }
            case Gang.Vagos:
                {
                    veh = World.CreateVehicle("buccaneer", pos);
                    veh.PrimaryColor = VehicleColor.MetallicTaxiYellow;
                    veh.SecondaryColor = VehicleColor.MetallicTaxiYellow;

                    for (int i = 0; i < veh.PassengerSeats + 1; i++)
                    {
                        Ped ped = World.CreatePed(VagosModels[RandomInt(0, VagosModels.Count - 1)], veh.Position.Around(5));
                        ped.RelationshipGroup = HateRLGroup;
                        ped.Weapons.Give(WeaponHash.Pistol, 99, true, true);
                        Peds.Add(ped);
                    }
                    break;
                }
            case Gang.Families:
                {
                    veh = World.CreateVehicle("baller", pos);
                    veh.PrimaryColor = VehicleColor.MetallicGreen;
                    for (int i = 0; i < veh.PassengerSeats + 1; i++)
                    {
                        Ped ped = World.CreatePed(FamiliesModels[RandomInt(0, FamiliesModels.Count - 1)], veh.Position.Around(5));
                        ped.RelationshipGroup = HateRLGroup;
                        ped.Weapons.Give(WeaponHash.Pistol, 99, true, true);
                        Peds.Add(ped);
                    }
                    break;
                }
        }
        if (Debug >= DebugLevel.Everything) UI.Notify("setting up gang");

        if (DebugBlips)
        {
            veh.AddBlip();
            veh.CurrentBlip.Sprite = BlipSprite.GunCar;
            veh.CurrentBlip.IsShortRange = true;
        }
        SetPedsIntoVehicle(Peds, veh);

        Script.Wait(1000);
        Ped driver = veh.Driver;
        if (!CanWeUse(driver))
        {
            if (Debug >= DebugLevel.Everything) UI.Notify("No driver!");
            driver = Peds[0];
        }
        MoveEntitytoNearestRoad(veh, true, true);
        if (Debug >= DebugLevel.Everything) UI.Notify("tasking gang");

        TaskSequence seq = new TaskSequence();
        Function.Call(Hash.TASK_ENTER_VEHICLE, 0, veh, 20000, -1, 1f, 1, 0);
        Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, veh, victim, 4, 20f, 4 + 8 + 16 + 32, 20f, 10f, 2f, true, 1101004800);
        Function.Call(Hash.TASK_PAUSE, 0, RandomInt(2, 4) * 1000);
        Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, veh, victim, 8, 60f, 4 + 8 + 16 + 32, 20f, 10f, 2f, true, 1101004800);
        seq.Close();
        driver.Task.PerformSequence(seq);
        seq.Dispose();

        foreach (Ped ped in Peds) TemporalPersistence.Add(ped);
        TemporalPersistence.Add(veh);
        TemporalPersistence.Add(victim);

        if (!victim.CurrentBlip.Exists() && !victim.IsPlayer)
        {
            victim.AddBlip();
            victim.CurrentBlip.Color = BlipColor.White;
            victim.CurrentBlip.Scale = 0.7f;
            victim.CurrentBlip.Name = "Victim";
            victim.CurrentBlip.IsShortRange = true;
        }
        /*
        int id = 0;
        Function.Call(Hash.CREATE_INCIDENT_WITH_ENTITY, 7, victim, 2, 3f, id);
        Function.Call(Hash.ADD_SHOCKING_EVENT_FOR_ENTITY, 85, veh, 40f);
        */
        if (Debug >= DebugLevel.Everything) UI.Notify("finished");
        // GangDrivebyCooldown = Game.GameTime + 60 * 1000 * (RandomInt(2, 5));
    }
    public static void RandomTuning(Vehicle veh, bool parts, bool changecolor, bool livery, bool neons, bool horn)
    {
        Function.Call(Hash.SET_VEHICLE_MOD_KIT, veh, 0);

        //Change color
        if (changecolor)
        {
            var color = Enum.GetValues(typeof(VehicleColor));
            Random random = new Random();
            veh.PrimaryColor = (VehicleColor)color.GetValue(random.Next(color.Length));

            Random random2 = new Random();
            veh.SecondaryColor = (VehicleColor)color.GetValue(random2.Next(color.Length));

        }
        if (livery) if (veh.LiveryCount > 0) veh.Livery = RandomInt(0, veh.LiveryCount);
        if (parts)
        {
            //Change tuning parts
            foreach (int mod in Enum.GetValues(typeof(VehicleMod)).Cast<VehicleMod>())
            {
                if (mod == (int)VehicleMod.Livery && !livery) continue;
                if (mod == (int)VehicleMod.Horns && !horn) continue;
                veh.SetMod((VehicleMod)mod, RandomInt(0, veh.GetModCount((VehicleMod)mod)), false);
            }

        }

        if (neons)
        {

            //Color neoncolor = Color.FromArgb(0, Util.GetRandomInt(0, 255), Util.GetRandomInt(0, 255), Util.GetRandomInt(0, 255));

            Color neoncolor = Color.FromKnownColor((KnownColor)RandomInt(0, Enum.GetValues(typeof(KnownColor)).Cast<KnownColor>().Count()));
            veh.NeonLightsColor = neoncolor;

            veh.SetNeonLightsOn(VehicleNeonLight.Front, true);
            veh.SetNeonLightsOn(VehicleNeonLight.Back, true);
            veh.SetNeonLightsOn(VehicleNeonLight.Left, true);
            veh.SetNeonLightsOn(VehicleNeonLight.Right, true);

        }
    }
    public static bool CanPedSeePed(Ped watcher, Ped target, bool WatcherIsPlayer)
    {
        RaycastResult ray;
        if (WatcherIsPlayer)
        {
            if (watcher.IsInRangeOf(target.Position, 10f)) return true;
            ray = World.Raycast(GameplayCamera.Position, target.Position, Game.Player.Character.Position.DistanceTo(target.Position), IntersectOptions.Map, Game.Player.Character);
        }
        else
        {
            ray = World.Raycast(watcher.Position, target.Position, IntersectOptions.Map);
        }
        if (!ray.DitHitAnything || ray.HitCoords.DistanceTo(target.Position) < 10f) return true;
        return false;
    }

    public static bool CanPedSeePos(Ped watcher, Vector3 target, bool WatcherIsPlayer)
    {
        //if (!watcher.IsInRangeOf(target, 150f)) return false;

        RaycastResult ray;
        if (WatcherIsPlayer)
        {
            if (watcher.IsInRangeOf(target, 10f)) return true;
            ray = World.Raycast(GameplayCamera.Position, target, GameplayCamera.Position.DistanceTo(target) + 5, IntersectOptions.Map);
        }
        else
        {
            ray = World.Raycast(watcher.Position, target, IntersectOptions.Map);
        }



        if (ray.DitHitAnything)
        {
            //UI.Notify("cannot see");
            return false; //&& ray.HitCoords.DistanceTo(target) > 10f && ray.HitCoords.DistanceTo(watcher.Position) < 10f
        }
        //UI.Notify("can see");

        return true;
    }


    public int IsPosAheadEntity(Entity e, Vector3 pos)
    {
        return (int)Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, e, pos.X, pos.Y, pos.Z).Y;
    }
    public static float RoadTravelDistance(Vector3 pos, Vector3 destination)
    {
        return Function.Call<float>(Hash.CALCULATE_TRAVEL_DISTANCE_BETWEEN_POINTS, pos.X, pos.Y, pos.Z, destination.X, destination.Y, destination.Z);
    }
    public bool IsSuitableForPlayerToExperience(Vector3 pos, float noticeRadius)
    {
        if (pos.DistanceTo(Game.Player.Character.Position) > noticeRadius) return false;
        if (Game.Player.Character.Velocity.Length() > 1f)
        {
            if (IsPosAheadEntity(Game.Player.Character, pos) > (Game.Player.Character.Velocity.Length() * 2)) return true;
        }
        if (Game.Player.Character.IsOnFoot) return true;

        return false;
    }


    public bool IsPlayerApproachingPos(Vector3 pos)
    {
        
        if (IsPosAheadEntity(Game.Player.Character, pos)>10 && Game.Player.Character.IsInRangeOf(pos, InteractionRange)) return true;
        return false;
    }
    public static bool WouldPlayerNoticeChangesHere(Vector3 pos)
    {
        if (Game.Player.Character.IsInRangeOf(pos, 10f)) return true;
        if (Function.Call<bool>(Hash.WOULD_ENTITY_BE_OCCLUDED, Game.GenerateHash("firetruk"), pos.X, pos.Y, pos.Z, true)) return false; else return true;
        //if (CanPedSeePos(Game.Player.Character, pos, true)) return true;
        return false;
    }
    public static bool isCopInRange(Vector3 Location, float Range)
    {
        return Function.Call<bool>(Hash.IS_COP_PED_IN_AREA_3D, Location.X - Range, Location.Y - Range, Location.Z - Range, Location.X + Range, Location.Y + Range, Location.Z + Range);
    }
    public static bool isCopVehicleRange(Vector3 Location, float Range)
    {
        return Function.Call<bool>(Hash.IS_COP_VEHICLE_IN_AREA_3D, Location.X - Range, Location.Y - Range, Location.Z - Range, Location.X + Range, Location.Y + Range, Location.Z + Range);
    }

    public static bool WasCheatStringJustEntered(string cheat)
    {
        return Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash(cheat));
    }





    Vehicle DuelTruck = null;
    Ped DuelDriver = null;
    Vehicle DuelTrailer = null;
    bool Duel = false;
    void HandleDuel()
    {
        if (CanWeUse(DuelTruck) && CanWeUse(DuelTrailer) && CanWeUse(DuelDriver))
        {
            if (DuelTruck.IsInRangeOf(Game.Player.Character.Position, 40f))
            {

                DuelTruck.SoundHorn(2000);

                if (TemporalPersistence.Count < 10)
                {
                    TemporalPersistence.Add(DuelDriver);
                    DuelTruck.IsPersistent = false;
                    DuelTrailer.IsPersistent = false;

                }
                else
                {
                    DuelTruck.IsPersistent = false;
                    DuelTrailer.IsPersistent = false;
                    DuelDriver.IsPersistent = false;

                }
                //UI.Notify("Finished Duel");
                Duel = false;

                DuelTruck = null;
                DuelDriver = null;
                DuelTrailer = null;
            }
        }
        else
        {
            Vector3 spawn = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(200));

            Model m = "roadkiller";

            if (!m.IsValid) m = "phantom";
            DuelTruck = World.CreateVehicle(m, spawn);

            MoveEntitytoNearestRoad(DuelTruck);

            Script.Wait(0);


            DuelDriver = DuelTruck.CreateRandomPedOnSeat(VehicleSeat.Driver);

            DuelTrailer = World.CreateVehicle(VehicleHash.Tanker2, DuelTruck.Position + (DuelTruck.ForwardVector * -5), DuelTruck.Heading);

            Function.Call(Hash.ATTACH_VEHICLE_TO_TRAILER, DuelTruck, DuelTrailer, 10);


            DuelTruck.AddBlip();
            //UI.Notify("Spawned duel");
            DuelTruck.EnginePowerMultiplier = 400;


            Entity e = Game.Player.Character;

            if (Game.Player.Character.IsOnFoot && CanWeUse(Game.Player.Character.LastVehicle) && Game.Player.Character.IsInRangeOf(Game.Player.Character.LastVehicle.Position, 30f)) e = Game.Player.Character.LastVehicle;
            TaskSequence seq = new TaskSequence();

            Vector3 pos = DuelTruck.Position + (DuelTruck.ForwardVector * 20);
            Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, DuelTruck, pos.X, pos.Y, pos.Z, 200f, 4 + 8 + 16 + 32, 5f);

            Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, DuelTruck, e, 4, 15f, 4 + 8 + 16 + 32, 20f, 20f, false);
            Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, DuelTruck, e, 4, 200f, 16777216, 3f, 20f, false);

            Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, DuelTruck, 32, 1000);
            Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, DuelTruck, 25f, 1 + 2 + 4 + 8 + 16 + 32 + 128);
            seq.Close();
            DuelDriver.Task.PerformSequence(seq);
            seq.Dispose();



            DuelTruck.PrimaryColor = VehicleColor.MatteDesertTan;
            DuelTrailer.PrimaryColor = VehicleColor.MatteDesertTan;

            DuelTruck.DirtLevel = 15f;
            DuelTrailer.DirtLevel = 15f;

            DuelDriver.Alpha = 0;
            BlacklistedVehicles.Add(DuelTruck);
        }
    }

    enum WereCarBehavior { Roaming, GoingToPosition, Taunting, Attacking };
    Vehicle werecar = null;
    Ped weredriver = null;
    WereCarBehavior wcarstate = WereCarBehavior.Roaming;

    Entity victim = null;

    bool HitThePlayer = false;
    bool UseRocket = false;
    bool CheckDoorBehavior = false;
    public void HandleWereCar()
    {
        if (CanWeUse(werecar))
        {


            if (UseRocket)
            {
                Vector3 victimpos = Game.Player.Character.Position;
                if (Function.Call<bool>((Hash)0x36D782F68B309BDA, werecar) && Math.Abs(Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, werecar, victimpos.X, victimpos.Y, victimpos.Z).X) < 5f &&
                   Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_ROTATION_VELOCITY, werecar, true).Z) < 0.3f && Math.Abs(Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, werecar, true).Y) > 1f)
                {
                    UI.Notify("~y~Special ability!");
                    Function.Call((Hash)0x81E1552E35DC3839, werecar, true);
                }
            }
            if (werecar.IsInRangeOf(Game.Player.Character.Position, 10f) && Game.Player.Character.IsRagdoll && !HitThePlayer) HitThePlayer = true;
            if (!werecar.IsPersistent) werecar.IsPersistent = true;
            if (!CanWeUse(werecar.Driver))
            {
                if (CanWeUse(weredriver)) weredriver.Delete();
                werecar.IsAxlesStrong = true;
                Ped p = werecar.CreateRandomPedOnSeat(VehicleSeat.Driver);
                p.IsVisible = false;
                p.IsInvincible = true;
                p.BlockPermanentEvents = true;
                // UI.Notify("~o~Something stalks you from the dark.");
                weredriver = p;
                Function.Call(Hash.SET_VEHICLE_LIGHTS, werecar, 1);
                werecar.EnginePowerMultiplier = 1000;
                return;
            }

            if (werecar.IsDead || werecar.EngineHealth < 300)
            {
                if (CanWeUse(werecar.Driver)) werecar.Driver.Delete();
                if (CanWeUse(weredriver)) weredriver.Delete();

                werecar.IsPersistent = false;

                werecar = null;
                // UI.Notify("~g~Defeated!");
                return;
            }
            switch (wcarstate)
            {
                case WereCarBehavior.Roaming:
                    {
                        if (werecar.Driver.TaskSequenceProgress == -1)
                        {
                            Vector3 Destination = werecar.Position.Around(30f);
                            TaskSequence seq = new TaskSequence();
                            Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, werecar, Destination.X, Destination.Y, Destination.Z, 5f, 4 + 8 + 16 + 32 + 4194304, 5f);
                            Function.Call(Hash.TASK_PAUSE, 0, 3000);
                            seq.Close();
                            werecar.Driver.Task.PerformSequence(seq);
                            seq.Dispose();


                        }



                        if (Game.Player.Character.IsInRangeOf(werecar.Position, 30))
                        {

                            Vector3 Destination = werecar.Position;

                            TaskSequence seq = new TaskSequence();

                            //TASK_VEHICLE_MISSION_PED_TARGET(Ped ped, Vehicle vehicle, Ped pedTarget, int mode, float maxSpeed, int drivingStyle, float minDistance, float p7, BOOL p8)
                            Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, werecar, Game.Player.Character, 4, 200f, 4 + 8 + 16 + 32 + 4194304, Game.Player.Character.Position.DistanceTo(werecar.Position) - 10f, 20f, false);
                            Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, werecar, 6, 2000);
                            //Function.Call(Hash.TASK_PAUSE, 0, 2000);

                            Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, werecar, Game.Player.Character, 4, 10f, 16777216, 20f, 100f, false);

                            //Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, werecar, Destination.X, Destination.Y, Destination.Z, 15f, 4 + 8 + 16 + 32 + 16777216, 30f);

                            seq.Close();
                            werecar.Driver.Task.PerformSequence(seq);
                            seq.Dispose();
                            //  UI.Notify("~o~You angered it...");

                            Function.Call(Hash.SET_VEHICLE_LIGHTS, werecar, 0);
                            wcarstate = WereCarBehavior.Attacking;

                        }

                        break;
                    }
                case WereCarBehavior.GoingToPosition:
                    {
                        if (weredriver.TaskSequenceProgress == -1)
                        {
                            UseRocket = false;

                            if (!werecar.IsInRangeOf(Game.Player.Character.Position, 20f) && werecar.IsInRangeOf(Game.Player.Character.Position, 70f))
                            {
                                if (CarCanSeePos(werecar, Game.Player.Character.Position, 1))
                                {
                                    wcarstate = WereCarBehavior.Attacking;
                                }
                                else
                                {
                                    UI.Notify("the beast looks for you.");

                                    TaskSequence seq = new TaskSequence();
                                    Vector3 Destination = LerpByDistance(Game.Player.Character.Position, werecar.Position, werecar.Position.DistanceTo(Game.Player.Character.Position) + 40);
                                    Destination = World.GetSafeCoordForPed(Destination, true);
                                    if (Destination == Vector3.Zero) Destination = GenerateSpawnPos(LerpByDistance(Game.Player.Character.Position, werecar.Position, 60), Nodetype.AnyRoad, false);// Destination = werecar.Position.Around(20);

                                    Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, werecar, Destination.X, Destination.Y, Destination.Z, 15f, 4 + 8 + 16 + 32 + 4194304, 20f);
                                    //Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, werecar, Game.Player.Character, 8, 200f, 16777216, 5f, 100f, false);

                                    seq.Close();
                                    werecar.Driver.Task.PerformSequence(seq);
                                    seq.Dispose();
                                }
                            }
                            else
                            {
                                UI.Notify("~o~The beast repositions itself.");

                                TaskSequence seq = new TaskSequence();
                                Vector3 Destination = GenerateSpawnPos(werecar.Position.Around(20), Nodetype.AnyRoad, false);
                                float dist = werecar.Position.DistanceTo(Game.Player.Character.Position) * 0.5f;
                                if (dist < 30f) dist = 30f;
                                Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, werecar, Game.Player.Character, 4, 10f, 4 + 8 + 16 + 32 + 4194304, dist, 15f, false);
                                //                                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, werecar, Destination.X, Destination.Y, Destination.Z, 15f, 4 + 8 + 16 + 32 + 4194304, 20f);
                                seq.Close();
                                werecar.Driver.Task.PerformSequence(seq);
                                seq.Dispose();
                            }
                        }

                        break;
                    }
                case WereCarBehavior.Taunting:
                    {


                        break;
                    }
                case WereCarBehavior.Attacking:
                    {

                        if (werecar.Velocity.Length() < 1f)
                        {
                            if (werecar.IsDoorOpen(VehicleDoor.FrontLeftDoor)) werecar.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                            if (werecar.IsDoorOpen(VehicleDoor.FrontRightDoor)) werecar.CloseDoor(VehicleDoor.FrontRightDoor, false);
                        }

                        if (!werecar.IsInRangeOf(Game.Player.Character.Position, 300f))
                        {

                            wcarstate = WereCarBehavior.Roaming;
                            Function.Call(Hash.SET_VEHICLE_LIGHTS, werecar, 1);
                            break;
                        }

                        if (werecar.Velocity.Length() > 5f && werecar.IsInRangeOf(Game.Player.Character.Position, 30f) && CheckDoorBehavior)
                        {
                            Vector3 victimpos = Game.Player.Character.Position;

                            float offset = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, werecar, victimpos.X, victimpos.Y, victimpos.Z).X;

                            if (Math.Abs(offset) < 10)
                            {
                                if (offset > 0.5f)
                                {
                                    werecar.OpenDoor(VehicleDoor.FrontRightDoor, false, false);
                                    CheckDoorBehavior = false;

                                }
                                else if (offset < -0.5f)
                                {
                                    werecar.OpenDoor(VehicleDoor.FrontLeftDoor, false, false);
                                    CheckDoorBehavior = false;

                                }
                            }
                        }

                        if (werecar.Velocity.Length() > 10f && CheckForObstaclesAhead(werecar))// !CanPedSeePed(Game.Player.Character, weredriver, false))
                        {
                            Vector3 victimpos = Game.Player.Character.Position;

                            if (Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, werecar, victimpos.X, victimpos.Y, victimpos.Z).X > 5f)
                            {
                                //     UI.Notify("~o~The beast sees through your cheats.");
                            }

                            TaskSequence seq = new TaskSequence();
                            Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, werecar, 31, 4000);
                            Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, werecar, Game.Player.Character, 4, 10f, 4194304, werecar.Position.DistanceTo(Game.Player.Character.Position) * 0.7f, 15f, false);

                            seq.Close();
                            werecar.Driver.Task.PerformSequence(seq);
                            seq.Dispose();
                            break;
                        }


                        if (werecar.Driver.TaskSequenceProgress == -1)
                        {
                            //Vector3 Destination = werecar.Position;
                            if (werecar.Velocity.Length() < 3f)
                            {

                                TaskSequence seq = new TaskSequence();

                                Vector3 victimpos = Game.Player.Character.Position;
                                if (Math.Abs(Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, werecar, victimpos.X, victimpos.Y, victimpos.Z).X) > 5f)
                                {

                                    if (Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, werecar, victimpos.X, victimpos.Y, victimpos.Z).Y < 5f)
                                    {
                                        UI.Notify("isbehind");
                                        Vector3 pos = Vector3.Zero;// LerpByDistance(Game.Player.Character.Position, werecar.Position, Game.Player.Character.Position.DistanceTo(werecar.Position) + 30);
                                        //pos= World.GetSafeCoordForPed(pos, true);
                                        //if (pos == Vector3.Zero) pos = GenerateSpawnPos(LerpByDistance(Game.Player.Character.Position, werecar.Position, Game.Player.Character.Position.DistanceTo(werecar.Position) + 30), Nodetype.AnyRoad, false);


                                        if (Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, werecar, victimpos.X, victimpos.Y, victimpos.Z).X > 0f)
                                        {
                                            pos = werecar.Position + (werecar.RightVector * 10);
                                        }
                                        else
                                        {
                                            pos = werecar.Position + (werecar.RightVector * -10);
                                        }
                                        pos += (werecar.ForwardVector * -5);
                                        Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, werecar, pos.X, pos.Y, pos.Z, 5f, 1024 + 16777216, 3f);
                                        //Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, werecar, 6, 2000);
                                    }
                                    else
                                    {
                                        UI.Notify("isahead");
                                        Vector3 pos = LerpByDistance(Game.Player.Character.Position, werecar.Position, Game.Player.Character.Position.DistanceTo(werecar.Position) * 0.6f);
                                        Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, werecar, pos.X, pos.Y, pos.Z, 30f, 16777216, 20f);
                                    }
                                }
                                else if (Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, werecar, victimpos.X, victimpos.Y, victimpos.Z).Y > 0f)
                                {
                                    Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, werecar, 30, 1000);
                                    Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, werecar, 23, 2000);
                                }
                                Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, 0, werecar, Game.Player.Character, 4, 200f, 16777216, 5f, 100f, false);
                                Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, werecar, 32, 1000);
                                Function.Call(Hash.TASK_VEHICLE_TEMP_ACTION, 0, werecar, 6, 2000);
                                //     UI.Notify("It attacks!");
                                HitThePlayer = false;
                                UseRocket = true;
                                CheckDoorBehavior = true;
                                seq.Close();
                                werecar.Driver.Task.PerformSequence(seq);
                                seq.Dispose();
                                wcarstate = WereCarBehavior.GoingToPosition;

                            }
                        }
                        break;
                    }
            }
        }

    }
    public static bool CheckForObstaclesAhead(Vehicle v)
    {

        if (v.Speed < 5f) return false;
        Vector3 direction = v.ForwardVector * (v.Speed / 2);// v.Position+(v.RightVector * -1) + (v.Velocity);// ChaserPed.CurrentVehicle.Velocity;
        Vector3 directionleft = (v.RightVector * -1) + (v.Velocity / 2);// ChaserPed.CurrentVehicle.Velocity;
        Vector3 directionright = (v.RightVector * 1) + (v.Velocity / 2);// ChaserPed.CurrentVehicle.Velocity;
                                                                        //Vector3 direction = ChaserPed.CurrentVehicle.Position + ChaserPed.CurrentVehicle.ForwardVector * (ChaserPed.CurrentVehicle.Speed*2f);// ChaserPed.CurrentVehicle.Velocity;
        Vector3 origin = v.Position + (v.ForwardVector * (v.Model.GetDimensions().Y / 2));

        RaycastResult cast = World.Raycast(origin, direction, 2f, IntersectOptions.Map);
        RaycastResult castleft = World.Raycast(origin, directionleft, 2f, IntersectOptions.Map);
        RaycastResult castright = World.Raycast(origin, directionright, 2f, IntersectOptions.Map);

        //DrawLine(origin, cast.HitCoords);
        //DrawLine(origin, castleft.HitCoords);
        // DrawLine(origin, castright.HitCoords);
        if ((cast.DitHitAnything && Math.Abs(cast.SurfaceNormal.Z) < 0.3)
            || (castleft.DitHitAnything && Math.Abs(castleft.SurfaceNormal.Z) < 0.3)
            || (castright.DitHitAnything && Math.Abs(castright.SurfaceNormal.Z) < 0.3))
        {

            //UI.ShowSubtitle("WALL", 1000);
            return true;
        }

        return false;
    }
    public static void ReplaceVehicle(Vehicle v, Model target, bool tuning)
    {
        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Replacing" + v.DisplayName + " with " + target.ToString());

        //LivelyWorld.BlacklistedModels.Add(v.Model.ToString());
        if (CanWeUse(v))
        {
            Vector3 pos = v.Position;
            float speed = v.Speed;
            float heading = v.Heading;
            Ped ped = v.Driver;
            bool HadDriver = LivelyWorld.CanWeUse(ped);
            Vehicle possibletrailer = LivelyWorld.GetTrailer(v);

            if (CanWeUse(possibletrailer))
            {
                if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - truck had trailer");

                Function.Call(Hash.DETACH_VEHICLE_FROM_TRAILER, v);
                possibletrailer.IsPersistent = true;
                Script.Wait(500);

            }
            v.Delete();

            Vehicle veh = World.CreateVehicle(target, pos, heading);

            if (LivelyWorld.CanWeUse(veh))
            {
                if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - replacer car created");

                veh.Speed = speed;

                if (!veh.CurrentBlip.Exists() && LivelyWorld.DebugBlips)
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - adding blip");

                    veh.AddBlip();
                    veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                    veh.CurrentBlip.Scale = 0.7f;
                    veh.CurrentBlip.Color = BlipColor.White;
                    veh.CurrentBlip.IsShortRange = true;
                    veh.CurrentBlip.Name = veh.FriendlyName;
                }

                if (tuning) LivelyWorld.RandomTuning(veh, true, false, true, LivelyWorld.IsNightTime(), false);
                //LivelyWorld.ReplacerTime = Game.GameTime + 30000;


                if (LivelyWorld.CanWeUse(ped)) ped.SetIntoVehicle(veh, VehicleSeat.Driver);
                else
                    if (HadDriver)
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - creating driver for it");

                    veh.CreateRandomPedOnSeat(VehicleSeat.Driver);
                    veh.Driver.IsPersistent = false;
                    veh.EngineRunning = true;
                    if (LivelyWorld.IsNightTime()) veh.LightsOn = true;
                }

                if (LivelyWorld.CanWeUse(possibletrailer))
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - arraching trailer to new car");

                    Function.Call(GTA.Native.Hash.ATTACH_VEHICLE_TO_TRAILER, veh, possibletrailer, 10);

                    possibletrailer.IsPersistent = false;
                }
                BlacklistedVehicles.Add(veh);
                veh.IsPersistent = false;
            }

        }

    }
    public static bool IsInNamedArea(Entity e, string name)
    {
        string area = name.ToLowerInvariant();
        return GetMapAreaAtCoords(e.Position).ToLowerInvariant() == area || World.GetZoneName(e.Position).ToLowerInvariant() == area || World.GetZoneNameLabel(e.Position).ToLowerInvariant() == area || World.GetStreetName(e.Position).ToLowerInvariant() == area;
    }
    public static Vehicle GetTrailer(Vehicle veh)
    {
        OutputArgument outputArgument = new OutputArgument();
        if (Function.Call<bool>(Hash.GET_VEHICLE_TRAILER_VEHICLE, veh, outputArgument))
        {
            Vehicle trailer = outputArgument.GetResult<Vehicle>();

            if (!CanWeUse(trailer))
            {
                foreach (Vehicle ptrailer in World.GetNearbyVehicles(veh.Position + (veh.ForwardVector * -5), 20f))
                {
                    if ((VehicleHash)ptrailer.Model.Hash == VehicleHash.FreightTrailer) { return ptrailer; }
                    if (ptrailer.Model == "trailers2" || (VehicleHash)ptrailer.Model.Hash == VehicleHash.Trailers2) { return ptrailer; }
                    if (ptrailer.Model == "tanker" || (VehicleHash)ptrailer.Model.Hash == VehicleHash.Tanker) { return ptrailer; }
                }
                if (!CanWeUse(trailer))
                {
                    foreach (Vehicle ptrailer in World.GetNearbyVehicles(veh.Position + (veh.ForwardVector * -5), 20f))
                    {
                        if (Function.Call<bool>(GTA.Native.Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, ptrailer)) { return ptrailer; }
                    }
                }
            }
            else return outputArgument.GetResult<Vehicle>();
        }
        else
        {
            return null;
        }
        return null;
    }

    public static Vehicle GetTrailer(Vehicle veh, Model trailermodel)
    {
        OutputArgument outputArgument = new OutputArgument();
        if (Function.Call<bool>(Hash.GET_VEHICLE_TRAILER_VEHICLE, veh, outputArgument))
        {
            Vehicle trailer = outputArgument.GetResult<Vehicle>();

            if (!CanWeUse(trailer))
            {
                foreach (Vehicle ptrailer in World.GetNearbyVehicles(veh.Position + (veh.ForwardVector * -5), 20f))
                {

                    if (ptrailer.Model == trailermodel || ptrailer.Model.Hash == Game.GenerateHash(trailermodel.ToString())) { return ptrailer; }
                }
                if (!CanWeUse(trailer))
                {
                    foreach (Vehicle ptrailer in World.GetNearbyVehicles(veh.Position + (veh.ForwardVector * -5), 20f))
                    {
                        if (Function.Call<bool>(GTA.Native.Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, ptrailer) && ptrailer.Model == trailermodel) { return ptrailer; }
                    }
                }
            }
            else return outputArgument.GetResult<Vehicle>();
        }
        else
        {
            return null;
        }
        return null;
    }
    void SpawnCarMeet()
    {
        for (int i = 0; i < 20; i++)
        {

        }
    }

    public static Vector3 GetQuietPlace()
    {
        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - QuietPlace requested");

        Vector3 pos = GenerateSpawnPos(World.GetSafeCoordForPed(Game.Player.Character.Position.Around(50f), false), Nodetype.Offroad, false);

        for (int i = 0; i < 50; i++)
        {

            if (World.GetNearbyPeds(pos, 20f).Length < 2 && GenerateSpawnPos(pos, Nodetype.Road, false).DistanceTo(pos) > 50f)
            {
                return pos;
                //pos = World.GetSafeCoordForPed(temp.Around(20), true);
                //if(pos==Vector3.Zero) pos = temp;
            }
            else
            {
                pos = GenerateSpawnPos(World.GetSafeCoordForPed(Game.Player.Character.Position.Around(50+(i*2)), false), Nodetype.Offroad, false);
            }
        }


        return Vector3.Zero;
    }

    void SpawnDrugDeal(bool gang)
    {
        Vector3 pos = GetQuietPlace();
        if (pos != Vector3.Zero)
        {
            DrugDeals.Add(new DrugDeal(pos, gang));
            //   DrugDealCooldown = Game.GameTime + (1000 * 60 * 5);
        }
        else
        {
            //  DrugDealCooldown = Game.GameTime + (3000);

        }
    }
    void SpawnTaxiEvent()
    {
        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - SpawnTaxiEvent()");

        Ped tdriver = null;
        Ped Hitch = null;
        Vehicle vtaxi = null;
        foreach (Vehicle taxi in AllVehicles)
        {
            if (!DecorExistsOn("LWIgnore", taxi) && new List<Model> { "taxi", "taxi20", "taxi21", "taxiesperanto" }.Contains(taxi.Model) && CanWeUse(taxi.Driver) && taxi.IsInRangeOf(Game.Player.Character.Position, 100f))
            {
                vtaxi = taxi;
                if (Debug >= DebugLevel.Everything) UI.Notify("got taxi");
                if (CanWeUse(taxi.Driver) && !taxi.Driver.IsPlayer && !taxi.Driver.IsPersistent)
                {
                    if (Debug >= DebugLevel.Everything) UI.Notify("got driver");
                    tdriver = taxi.Driver;

                    if (DoesVehicleHavePassengers(taxi))
                    {
                        foreach (Ped ped in World.GetNearbyPeds(taxi.Position, 5f))//taxi.Position+(taxi.ForwardVector*50)
                        {
                            if (ped.IsInVehicle(taxi) && ped.Handle != taxi.Driver.Handle && ped.IsHuman && !ped.IsPersistent && !ped.IsPlayer)
                            {
                                if (Debug >= DebugLevel.Everything) UI.Notify("got hitch");
                                Hitch = ped;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Vector3 search = taxi.Position + (taxi.ForwardVector * 20) + (taxi.RightVector * 10);
                        foreach (Ped ped in World.GetNearbyPeds(search, 50f))//taxi.Position+(taxi.ForwardVector*50)
                        {
                            if (ped.IsOnFoot && ped.IsAlive && ped.Handle != taxi.Driver.Handle && ped.IsHuman && !ped.IsPersistent && !ped.IsPlayer)
                            {
                                if (Debug >= DebugLevel.Everything) UI.Notify("got hitch");
                                Hitch = ped;
                                break;
                            }
                        }
                    }

                }
            }
        }
        if (CanWeUse(Hitch) && CanWeUse(vtaxi) && CanWeUse(tdriver))
        {
            Taxis.Add(new TaxiEvent(Hitch, tdriver, vtaxi));
            TaxigCooldown = Game.GameTime + 50000;
            ScenarioFlow.Add(ScenarioType.Taxi);

        }
    }
    void ResprayTruck(Vehicle veh)
    {
        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - ResprayTruck() " + veh.DisplayName);

        if (CanWeUse(veh))
        {
            if ((veh.Model == "mule" || veh.Model == "mule2"))
            {
                if (veh.IsExtraOn(2)) if (RandomInt(0, 10) <= 5) veh.PrimaryColor = VehicleColor.WornGreen; else veh.PrimaryColor = VehicleColor.WornWhite;
                else if (veh.IsExtraOn(3)) veh.PrimaryColor = VehicleColor.MetallicFormulaRed;
                else if (veh.IsExtraOn(4)) veh.PrimaryColor = VehicleColor.MetallicGoldenBrown;
                else if (veh.IsExtraOn(6)) veh.PrimaryColor = VehicleColor.MetallicGoldenBrown;
                else if (veh.IsExtraOn(7)) veh.PrimaryColor = VehicleColor.BrushedGold;
                if (Debug >= DebugLevel.Everything) UI.Notify("~b~resprayed Mule to " + veh.PrimaryColor.ToString());

            }

            if (veh.Model == "pounder")
            {
                if (veh.IsExtraOn(1)) veh.PrimaryColor = VehicleColor.MetallicFrostWhite;
                else if (veh.IsExtraOn(2)) veh.PrimaryColor = VehicleColor.MetallicRaceYellow;
                if (Debug >= DebugLevel.Everything) UI.Notify("~b~resprayed Pounder to " + veh.PrimaryColor.ToString());

            }
            if (veh.Model == "benson")
            {

                if (veh.IsExtraOn(1)) veh.PrimaryColor = VehicleColor.MetallicGoldenBrown;
                else if (veh.IsExtraOn(2)) veh.PrimaryColor = VehicleColor.MetallicBlueSilver;
                else if (veh.IsExtraOn(3)) veh.PrimaryColor = VehicleColor.MetallicBlueSilver;
                else if (veh.IsExtraOn(4)) veh.PrimaryColor = VehicleColor.PoliceCarBlue;
                else if (veh.IsExtraOn(5)) veh.PrimaryColor = VehicleColor.MetallicGreen;
                else if (veh.IsExtraOn(6)) veh.PrimaryColor = VehicleColor.MetallicFrostWhite;
                else if (veh.IsExtraOn(7)) veh.PrimaryColor = VehicleColor.MetallicFrostWhite;
                if (Debug >= DebugLevel.Everything) UI.Notify("~b~Resprayed Benson to " + veh.PrimaryColor.ToString());

            }
            if (veh.Model == "packer" || veh.Model == "hauler" || veh.Model == "phantom" || veh.Model == "roadkiller")
            {

                if (Debug >= DebugLevel.Everything) UI.Notify("found " + veh.FriendlyName);
                if (Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, veh))
                {
                    if (Debug >= DebugLevel.Everything) UI.Notify("has trailer");
                    Vehicle vehtrailer = null;

                    vehtrailer = GetTrailer(veh);
                    if (!CanWeUse(vehtrailer))
                        foreach (Vehicle ptrailer in World.GetNearbyVehicles(veh.Position + (veh.ForwardVector * -5), 20f))
                        {
                            if (ptrailer.Model == "trailers2" || (VehicleHash)ptrailer.Model.Hash == VehicleHash.Trailers2) { vehtrailer = ptrailer; return; }
                            if (ptrailer.Model == "tanker" || (VehicleHash)ptrailer.Model.Hash == VehicleHash.Tanker) { vehtrailer = ptrailer; return; }
                        }

                    if (CanWeUse(vehtrailer))
                    {
                        if (Debug >= DebugLevel.Everything) UI.Notify("correct trailer model");
                        if ((VehicleHash)vehtrailer.Model.Hash == VehicleHash.Tanker || (VehicleHash)vehtrailer.Model.Hash == VehicleHash.Tanker2) veh.PrimaryColor = VehicleColor.Orange;
                        if (vehtrailer.LiveryCount > 0)
                        {
                            if (Debug >= DebugLevel.Everything) UI.Notify("~g~changed color");
                            switch (vehtrailer.Livery)
                            {
                                case 0:
                                    {
                                        if (Debug >= DebugLevel.Everything) UI.Notify("~g~gold");
                                        veh.PrimaryColor = VehicleColor.PureGold;
                                        break;
                                    }
                                case 2:
                                    {
                                        if (Debug >= DebugLevel.Everything) UI.Notify("~g~yellow");
                                        veh.PrimaryColor = VehicleColor.MetallicTaxiYellow;

                                        break;
                                    }
                                case 3:
                                    {
                                        if (Debug >= DebugLevel.Everything) UI.Notify("~g~red");

                                        veh.PrimaryColor = VehicleColor.MetallicBlazeRed;
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
        }
    }



    int PedRushingCooldown = 0;
    int TaxigCooldown = 0;
    int CarInteractionCooldown = 0;
    int EmergencyRushingCooldown = 0;

    void Scenarios()
    {
        if (CurrentlyAllowedScenarios.Count() == 0) return;
        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("AmbientSmallEvents()");


        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Scenarios() ");

        //Interaction Events
        List<ScenarioType> TempScenarioList = CurrentlyAllowedScenarios;
        if (TempScenarioList.Contains(ScenarioType.BarnFinds)) TempScenarioList.Remove(ScenarioType.BarnFinds);
        if (TempScenarioList.Contains(ScenarioType.ImprovedFlatbeds)) TempScenarioList.Remove(ScenarioType.ImprovedFlatbeds);
        if (TempScenarioList.Contains(ScenarioType.ImprovedFreight)) TempScenarioList.Remove(ScenarioType.ImprovedFreight);
        if (TempScenarioList.Contains(ScenarioType.ImprovedTowtrucks)) TempScenarioList.Remove(ScenarioType.ImprovedTowtrucks);
        if (TempScenarioList.Contains(ScenarioType.AmbientTuner)) TempScenarioList.Remove(ScenarioType.AmbientTuner);
        if (TempScenarioList.Contains(ScenarioType.PlayerCoolCarPhoto)) TempScenarioList.Remove(ScenarioType.PlayerCoolCarPhoto);
        if (TempScenarioList.Contains(ScenarioType.DriverOvertake)) TempScenarioList.Remove(ScenarioType.DriverOvertake);

        if (TempScenarioList.Count == 0) return;
        ScenarioType ScenarioSelector = TempScenarioList[RandomInt(0, TempScenarioList.Count - 1)];

        if (ForcedScenario > -1)
        {
            ScenarioSelector = (ScenarioType)ForcedScenario;
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~g~Forced Scenario: ~b~" + ScenarioSelector.ToString());

        }
        else
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Chosen Scenario: ~b~" + ScenarioSelector.ToString());
        }


        if (ScenarioFlow.Contains(ScenarioSelector))
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~Cancelled, already done recently");
            return;
        }
        switch (ScenarioSelector)
        {
            default:
                {
                    SmallEventCooldownTime = Game.GameTime + 500;
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~y~Wrong scenario selected, retrying");
                    return;
                }
            case ScenarioType.DriverRushing: //PedRushing
                {
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Trying PedRushing");
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - PedRushing");

                    if (!DisabledScenarios.Contains(ScenarioType.DriverRushing))
                    {
                        if (Game.GameTime > PedRushingCooldown)
                        {
                            foreach (Vehicle veh in AllVehicles)
                            {
                                if (CanWeUse(veh) && veh.IsInRangeOf(Game.Player.Character.Position, 40f) && WouldPlayerNoticeChangesHere(veh.Position) && !DecorExistsOn("LWIgnore", veh) &&
                                        veh.EngineRunning && !isCopVehicleRange(veh.Position, 5f) && !BlacklistedVehicles.Contains(veh) && !veh.IsPersistent)
                                {
                                    if (CanWeUse(veh.Driver) && !veh.Driver.IsPlayer && !veh.Driver.IsPersistent && !veh.Driver.IsInCombat)
                                    {
                                        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - correct");

                                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Driver rushing triggered");
                                        Ped ped = veh.Driver;
                                        ped.Task.ClearAll();
                                        ped.DrivingSpeed = 120f;
                                        ped.AlwaysKeepTask = true;
                                        Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, ped, veh, 60f, 1 + 2 + 4 + 8 + 16 + 32);

                                        if (DebugBlips && !ped.CurrentBlip.Exists())
                                        {
                                            ped.AddBlip();
                                            ped.CurrentBlip.Color = BlipColor.Yellow;
                                            ped.CurrentBlip.Scale = 0.7f;
                                            ped.CurrentBlip.IsShortRange = true;
                                        }
                                        PedRushingCooldown = Game.GameTime + (RandomInt(40, 120) * 1000);
                                        //BlacklistedEvents.Add(EventType.DriverRushing);
                                        BlacklistedVehicles.Add(veh);
                                        ped.IsPersistent = true;
                                        TemporalPersistence.Add(ped);
                                        ScenarioFlow.Add(ScenarioType.DriverRushing);
                                        break;
                                    }
                                }
                            }
                        }
                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~PedRushing is on cooldown (" + (Game.GameTime - PedRushingCooldown) / 1000 + "s)");
                    }
                    else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~PedRushing event is disabled.");
                    break;
                }
            case ScenarioType.Taxi: //Taxi
                {
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Trying Taxi");
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Taxi scenario");

                    if (!DisabledScenarios.Contains(ScenarioType.Taxi))
                    {
                        if (TaxigCooldown < Game.GameTime)
                        {
                            SpawnTaxiEvent();
                        }
                        else
                        {
                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~Taxi is on cooldown (" + (Game.GameTime - TaxigCooldown) / 1000 + "s)");
                        }
                    }
                    else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~Taxi event is disabled.");


                    break;
                }
            case ScenarioType.VehicleInteraction: //DrivingOut / Interaction
                {
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Trying car interaction");
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - car interaction");
                    if (!DisabledScenarios.Contains(ScenarioType.VehicleInteraction))
                    {
                        if (CarInteractionCooldown < Game.GameTime)
                        {
                            List<Vehicle> DriveOut = new List<Vehicle>();
                            List<Vehicle> VehicleScenario = new List<Vehicle>();
                            List<Vehicle> Pullover = new List<Vehicle>();
                            foreach (Vehicle veh in AllVehicles)
                            {
                                if (!IsSuitableForPlayerToExperience(veh.Position, 50f)) continue;
                                if (RandomInt(0, 10) <= 10)
                                {
                                    if (CanWeUse(veh) && CanWeUse(veh.Driver) && !veh.IsAttached() && veh.Speed > 20 && WouldPlayerNoticeChangesHere(veh.Position) && !BlacklistedVehicles.Contains(veh) && veh != Game.Player.Character.CurrentVehicle &&
                                        veh.IsAlive && veh.Driver.IsAlive && !veh.Driver.IsInCombat)
                                    {
                                        Pullover.Add(veh);
                                    }
                                    //     return;
                                }
                                if (CanWeUse(veh) && !DecorExistsOn("LWIgnore", veh) && Function.Call<bool>(Hash.IS_ENTITY_OCCLUDED, veh) && veh.IsAlive && !veh.EngineRunning && veh.EngineHealth>900 && veh.IsStopped && !CanWeUse(veh.Driver) && veh.ClassType != VehicleClass.Emergency && veh.IsInRangeOf(Game.Player.Character.Position, 80f) &&
                                    !veh.IsPersistent && !LastDriverIsPed(veh, Game.Player.Character) && !BlacklistedVehicles.Contains(veh))
                                {

                                    if (veh.Model.IsCar && !WouldPlayerNoticeChangesHere(veh.Position))
                                    {
                                        DriveOut.Add(veh);
                                    }
                                    else if (veh.Model.IsCar) //Driving out
                                    {
                                        VehicleScenario.Add(veh);
                                    }
                                }
                            }

                            List<Vehicle> All = new List<Vehicle>();
                            All.AddRange(Pullover);
                            All.AddRange(VehicleScenario);
                            All.AddRange(DriveOut);
                            if (All.Count == 0) break;
                            Vehicle Protag = All[RandomInt(0, All.Count - 1)];

                            if (Pullover.Contains(Protag))
                            {

                                Protag.Driver.IsPersistent = true;
                                TemporalPersistence.Add(Protag.Driver);
                                TemporalPullover(Protag.Driver);
                                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("pullover " + Protag.FriendlyName);
                                BlacklistedVehicles.Add(Protag);
                                ScenarioFlow.Add(ScenarioType.VehicleInteraction);


                                if (DebugBlips && !Protag.Driver.CurrentBlip.Exists())
                                {
                                    Protag.Driver.AddBlip();
                                    Protag.Driver.CurrentBlip.Color = BlipColor.Green;
                                    Protag.Driver.CurrentBlip.Scale = 0.7f;
                                    Protag.Driver.CurrentBlip.IsShortRange = true;
                                    Protag.Driver.CurrentBlip.Name = "Car interaction (pullover)";
                                }
                                break;
                            }
                            else if (DriveOut.Contains(Protag))
                            {

                                foreach (Ped ped in World.GetNearbyPeds(Protag.Position, 30f))
                                {
                                    if (ped.IsHuman && ped.IsAlive && !ped.IsPersistent && !ped.IsPlayer && ped.IsOnFoot && !ped.IsInCombat)
                                    {
                                        RaycastResult Ahead = World.Raycast(Protag.Position + (Protag.ForwardVector * (Protag.Model.GetDimensions().Y / 2)), Protag.ForwardVector * 5f, 5f, IntersectOptions.Map, Protag);

 
                                        RaycastResult Behind = World.Raycast(Protag.Position - (Protag.ForwardVector*(Protag.Model.GetDimensions().Y/2)), Protag.ForwardVector * -20f, 20f, IntersectOptions.Map, Protag);

                                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Ped driving out triggered");
                                        ped.AlwaysKeepTask = true;
                                        Protag.LockStatus = VehicleLockStatus.Unlocked;
                                        Protag.NeedsToBeHotwired = false;
                                        TaskSequence seq = new TaskSequence();
                                        Function.Call(Hash.TASK_ENTER_VEHICLE, 0, Protag, 20000, -1, 1f, 1, 0);
                                        Function.Call(Hash.TASK_PAUSE, 0, RandomInt(2, 4) * 1000);
                                        if (Ahead.DitHitAnything)
                                        {
                                            Vector3 BackPos = Protag.Position + (Protag.ForwardVector * -20f);
                                            if (Behind.DitHitAnything) BackPos = Behind.HitCoords;
                                            Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, Protag, BackPos.X, BackPos.Y, BackPos.Z, 5f,1+2+ 4 + 8 + 16 + 32+512+1024, 5f);

                                          //  Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, Protag, 20f, 1 + 2 + 4 + 8 + 16 + 32 + 128 + 256);

                                        }

                                        Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, 0, Protag, 20f, 1 + 2 + 4 + 8 + 16 + 32 + 128 + 256);
                                        seq.Close();
                                        ped.Task.PerformSequence(seq);
                                        seq.Dispose();
                                        ped.BlockPermanentEvents = false;

                                        if (DebugBlips && !ped.CurrentBlip.Exists())
                                        {
                                            ped.AddBlip();
                                            ped.CurrentBlip.Color = BlipColor.Green;
                                            ped.CurrentBlip.Scale = 0.7f;
                                            ped.CurrentBlip.IsShortRange = true;
                                            ped.CurrentBlip.Name = "Car interaction (driving out)";
                                        }
                                        CarInteractionCooldown = Game.GameTime + 3000;// BlacklistedEvents.Add(EventType.PedDrivingOut);
                                        BlacklistedVehicles.Add(Protag);
                                        ScenarioFlow.Add(ScenarioType.VehicleInteraction);
                                        break;

                                    }
                                }
                            }
                            else if (VehicleScenario.Contains(Protag))
                            {
                                Ped ped = World.CreateRandomPed(Protag.Position.Around(10));
                                TemporalPersistence.Add(ped);

                                ped.AlwaysKeepTask = true;
                                Protag.IsPersistent = true;
                                Protag.LockStatus = VehicleLockStatus.Unlocked;
                                Protag.NeedsToBeHotwired = false;
                                TemporalPersistence.Add(Protag);

                                ped.SetNoCollision(Protag, true);
                                Protag.SetNoCollision(ped, true);

                                if (CanWeUse(Protag) && CanWeUse(ped))
                                {

                                    if (DebugBlips && !ped.CurrentBlip.Exists())
                                    {
                                        ped.AddBlip();
                                        ped.CurrentBlip.Color = BlipColor.Green;
                                        ped.CurrentBlip.Scale = 0.7f;
                                        ped.CurrentBlip.IsShortRange = true;
                                        ped.CurrentBlip.Name = "Car interaction (Scenario)";
                                    }
                                    BlacklistedVehicles.Add(Protag);
                                    CarInteractionCooldown = Game.GameTime + 30000;//BlacklistedEvents.Add(EventType.VehicleInteraction);
                                    Script.Wait(500);
                                    SpawnCarInteraction(Protag, ped);
                                    ScenarioFlow.Add(ScenarioType.VehicleInteraction);

                                    break;
                                }
                            }
                        }
                        else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~Car interactions are on cooldown (" + (Game.GameTime - CarInteractionCooldown) / 1000 + "s)");
                    }
                    else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~Car interactions are disabled.");
                    break;
                }
            case ScenarioType.AnimalTrophies:
                {
                    if (!DisabledScenarios.Contains(ScenarioType.AnimalTrophies))
                    {
                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Spawning trophy");
                        SpawnAnimalTrophy();
                    }
                    break;
                }

        }
        if (!ScenarioFlow.Contains(ScenarioSelector))
        {

            SmallEventCooldownTime = Game.GameTime + 2000;
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~" + ScenarioSelector.ToString() + " did not spawn.");
        }
        else
        {
            if (ForcedScenario != -1) ForcedScenario = -1;
        }
    }

    public static bool LastDriverIsPed(Vehicle veh, Ped p)
    {
        if (!CanWeUse(veh)) return false;

        Ped ped = Function.Call<Ped>(Hash.GET_LAST_PED_IN_VEHICLE_SEAT, veh, -1);
        if (CanWeUse(ped) && CanWeUse(p))
        {
            if (ped.Handle == p.Handle) return true;
        }
        return false;
    }


    bool HasOwner(Vehicle veh, bool IsNearby)
    {
        if (!CanWeUse(veh)) return false;
        if (CanWeUse(veh.Driver)) return true;
        else
        {
            Ped ped = Function.Call<Ped>(Hash.GET_LAST_PED_IN_VEHICLE_SEAT, veh, -1);
            if (!IsNearby) return CanWeUse(ped);
            else return (CanWeUse(ped) && ped.IsInRangeOf(veh.Position, 10f));
        }
    }
    public static bool CarCanSeePos(Vehicle veh, Vector3 pos, int height_offset)
    {
        //if (veh.Position.DistanceTo(pos) < 50f) return true;
        if (veh.Position.DistanceTo(pos) < 100f)
        {
            RaycastResult raycast = World.Raycast(veh.Position + new Vector3(0, 0, height_offset), pos + new Vector3(0, 0, height_offset), IntersectOptions.Map);

            if (!raycast.DitHitAnything) return true;
        }
        return false;
    }

    void OnKeyDown(object sender, KeyEventArgs e)
    {

    }
    void OnKeyUp(object sender, KeyEventArgs e)
    {

    }

    void CauseAccident()
    {
        foreach (Vehicle veh in World.GetNearbyVehicles(PlayerPed().Position, 20f))
        {
            if (CanWeUse(veh) && veh.Speed > 5f && CanWeUse(veh.GetPedOnSeat(VehicleSeat.Driver)) && veh.Speed > 20f && veh.GetPedOnSeat(VehicleSeat.Driver).Handle != PlayerPed().Handle)
            {
                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Causing accident to " + veh.FriendlyName);
                veh.ApplyForceRelative(new Vector3(0f, 2f, 0f), new Vector3(0f, 0f, 1f));
                Function.Call(Hash.SET_VEHICLE_REDUCE_GRIP, veh, true);
                Script.Wait(500);
                Function.Call(Hash.SET_VEHICLE_REDUCE_GRIP, veh, false);
                break;
            }
        }
    }
    bool IsRaining()
    {
        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - IsRaining()");

        int weather = Function.Call<int>(GTA.Native.Hash._0x564B884A05EC45A3); //get current weather hash
        switch (weather)
        {
            case (int)WeatherType.BLIZZARD:
                {
                    return true;
                }
            case (int)WeatherType.CLEARING:
                {
                    return true;
                }
            case (int)WeatherType.FOGGY:
                {
                    return true;
                }
            case (int)WeatherType.RAIN:
                {
                    return true;
                }
            case (int)WeatherType.NEUTRAL:
                {
                    return true;
                }
            case (int)WeatherType.THUNDER:
                {
                    return true;
                }
            case (int)WeatherType.LIGHT_SNOW:
                {
                    return true;
                }
            case (int)WeatherType.SNOW:
                {
                    return true;
                }
            case (int)WeatherType.X_MAS:
                {
                    return true;
                }
        }
        return false;
    }
    public static bool IsNightTime()
    {

        int hour = Function.Call<int>(Hash.GET_CLOCK_HOURS);
        return (hour > 20 || hour < 7);
    }


    //int GangDrivebyCooldown = Game.GameTime+40000;
    //int HunterCooldown = Game.GameTime + 40000;
    //int SpawnEmegencyRushingCooldown = Game.GameTime + 30000;
    //int DrugDealCooldown = Game.GameTime + 120000;
    //int AnimalTrophyCooldown = Game.GameTime + 4000;
    // int RacerCooldown = Game.GameTime + 300000;
    //int BennysCooldown = Game.GameTime + 5000;

    int ForcedEvent = -1;


    int GameTimeRefLong = 0;
    void HandleEvents()
    {

        if (CurrentlyAllowedEvents.Count == 0) return;
        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - HandleSpawnerEvents()");

        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~HandleSpawnerEvents()");
        if (RandomInt(0, 100) >= EventFrecuency)
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~dice says no spawner now");
            return;
        }
        else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~g~dice says events will spawn");

        EventType eventspawnerint = CurrentlyAllowedEvents[RandomInt(0, CurrentlyAllowedEvents.Count - 1)];
        if (ForcedEvent > -1)
        {
            eventspawnerint = (EventType)ForcedEvent;
            UI.Notify("~g~Forced event: ~b~" + eventspawnerint.ToString());
        }
        else
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Chosen event: ~b~" + eventspawnerint.ToString());
        }
        if (Eventflow.Contains(eventspawnerint))
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~Cancelled, already done recently");
            return;
        }
        switch (eventspawnerint)
        {
            case EventType.EmergencyRushing:
                {
                    //Ambient emergency rushing
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Trying emergency");
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - emergency scenario");

                    if (EmergencyRushingCooldown > Game.GameTime)
                    {
                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~EmergencyRushing is on cooldown (" + (Game.GameTime - EmergencyRushingCooldown) / 1000 + "s)");
                        return;
                    }
                    if (!DisabledEvents.Contains(EventType.EmergencyRushing))
                    {
                        foreach (Vehicle veh in AllVehicles)
                        {
                            if (!DecorExistsOn("LWIgnore", veh) && isCopVehicleRange(veh.Position, 1f) && !BlacklistedVehicles.Contains(veh) && !veh.IsPersistent && !LastDriverIsPed(veh, Game.Player.Character)) //Cop events
                            {
                                if (!veh.EngineRunning && veh.IsStopped)
                                {
                                    foreach (Ped ped in World.GetNearbyPeds(veh.Position, 60f))
                                    {
                                        if (ped.IsHuman && ped.IsAlive && !ped.IsPersistent && !ped.IsPlayer && isCopInRange(ped.Position, 1f) && ped.IsOnFoot && !ped.IsInCombat)
                                        {
                                            if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - correct");

                                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Cop driving out triggered");
                                            ped.IsPersistent = true;
                                            TemporalPersistence.Add(ped);

                                            ped.AlwaysKeepTask = true;
                                            veh.LockStatus = VehicleLockStatus.Unlocked;
                                            veh.NeedsToBeHotwired = false;
                                            veh.SirenActive = true;

                                            Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, ped, veh, 60f, 1 + 2 + 4 + 8 + 16 + 32);
                                            if (DebugBlips && !ped.CurrentBlip.Exists())
                                            {
                                                ped.AddBlip();
                                                ped.CurrentBlip.Color = BlipColor.Blue;
                                                ped.CurrentBlip.IsFlashing = true;
                                                ped.CurrentBlip.Scale = 0.7f;
                                                ped.CurrentBlip.IsShortRange = true;
                                            }
                                            ped.IsPersistent = true;
                                            TemporalPersistence.Add(ped);
                                            EmergencyRushingCooldown = Game.GameTime + 50000;
                                            BlacklistedVehicles.Add(veh);
                                            Eventflow.Add(EventType.EmergencyRushing);
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    if (CanWeUse(veh.Driver) && !veh.Driver.IsPlayer && !veh.Driver.IsInCombat)
                                    {
                                        if (isCopInRange(veh.Driver.Position, 1f))
                                        {
                                            if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - correct");

                                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Cop rushing to emergency triggered");
                                            Ped ped = veh.Driver;
                                            ped.AlwaysKeepTask = true;
                                            veh.SirenActive = true;
                                            ped.MaxDrivingSpeed = 120f;
                                            Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, ped, veh, 60f, 1 + 2 + 4 + 8 + 16 + 32);

                                            if (DebugBlips && !ped.CurrentBlip.Exists())
                                            {
                                                ped.AddBlip();
                                                ped.CurrentBlip.Color = BlipColor.Blue;
                                                ped.CurrentBlip.IsFlashing = true;
                                                ped.CurrentBlip.Scale = 0.7f;
                                                ped.CurrentBlip.IsShortRange = true;
                                            }
                                            EmergencyRushingCooldown = Game.GameTime + 50000;
                                            BlacklistedVehicles.Add(veh);
                                            Eventflow.Add(EventType.EmergencyRushing);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~EmergencyRushing is disabled.");

                    if (!Eventflow.Contains(EventType.EmergencyRushing))
                    {
                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Failed to achieve ambient emergency, spawning one");
                        SpawnEmergencyVehicle(EmergencyType.AMBULANCE);
                        Eventflow.Add(EventType.EmergencyRushing);
                    }
                    break;
                }
            case EventType.GangDriveby:
                {
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~dice says driveby");
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Driveby");

                    //DriveBy
                    if (!DisabledEvents.Contains(EventType.GangDriveby))
                    {
                        if (!IsNightTime() && Game.Player.WantedLevel == 0)
                        {
                            Eventflow.Add(EventType.GangDriveby);

                            switch (World.GetZoneNameLabel(Game.Player.Character.Position))
                            {
                                case "RANCHO":
                                case "CYPRE": //Vagos territory
                                    {
                                        BlacklistedEvents.Add(EventType.GangDriveby);
                                        Ped ped = null;
                                        foreach (Ped p in AllPeds) if (CanWeUse(p) && p.IsAlive && p.IsHuman && p.IsOnFoot && p.IsInRangeOf(Game.Player.Character.Position, 60f)) { ped = p; break; }
                                        if (CanWeUse(ped)) if (RandomInt(0, 10) <= 5) SpawnGangDriveBy(Gang.Ballas, ped); else SpawnGangDriveBy(Gang.Families, ped);
                                        break;
                                    }
                                case "DAVIS": //Ballas territory
                                    {
                                        BlacklistedEvents.Add(EventType.GangDriveby);
                                        Ped ped = null;
                                        foreach (Ped p in AllPeds) if (CanWeUse(p) && p.IsAlive && p.IsHuman && p.IsOnFoot && p.IsInRangeOf(Game.Player.Character.Position, 60f)) { ped = p; break; }
                                        if (CanWeUse(ped)) if (RandomInt(0, 10) <= 5) SpawnGangDriveBy(Gang.Vagos, ped); else SpawnGangDriveBy(Gang.Families, ped);
                                        break;
                                    }
                                case "CHAMH": //Families territory
                                    {
                                        BlacklistedEvents.Add(EventType.GangDriveby);
                                        Ped ped = null;
                                        foreach (Ped p in AllPeds) if (CanWeUse(p) && p.IsAlive && p.IsHuman && p.IsOnFoot && p.IsInRangeOf(Game.Player.Character.Position, 60f)) { ped = p; break; }
                                        if (CanWeUse(ped)) if (RandomInt(0, 10) <= 5) SpawnGangDriveBy(Gang.Ballas, ped); else SpawnGangDriveBy(Gang.Vagos, ped);
                                        break;
                                    }
                                default:
                                    {
                                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Player is not in a valid gang activity area.");
                                        break;
                                    }
                            }
                        }
                    }
                    else if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~Gang Activity is disabled.");
                    break;
                }
            case EventType.Hunter:
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Hunter/trophy");

                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~dice says hunter/trophy");

                    //Hunter && Animal trophy
                    if ( !IsNightTime())
                    {
                        if (Hunters.Count == 0 && (new List<string> { "CANNY", "MTJOSE", "DESRT", "CMSW", "ZANCUDO", "LAGO", "GREATC", "PALHIGH", "CCREAK", "MTCHIL" }.Contains(World.GetZoneNameLabel(Game.Player.Character.Position))))
                        {
                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Trying hunter");
                            // Vector3 pos = World.GetSafeCoordForPed(Game.Player.Character.Position.Around(100), false); //GenerateSpawnPos(Game.Player.Character.Position.Around(50),Nodetype.Offroad,false)

                            Vector3 pos = GenerateSpawnPos(Game.Player.Character.Position.Around(50), Nodetype.Offroad, false);
                            for (int i = 0; i < 10; i++) if(pos.DistanceTo(Game.Player.Character.Position)<50f || BlacklistedAreas.Contains(World.GetZoneNameLabel(pos))) pos= GenerateSpawnPos(Game.Player.Character.Position.Around(50+(i*5)), Nodetype.Offroad, false);
                            if (pos.DistanceTo(Game.Player.Character.Position) < 300f)
                            {

                                Hunters.Add(new Hunter(pos));
                                Eventflow.Add(EventType.Hunter);
                                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~Got pos, spawning hunter");
                            }
                        }
                    }
                    break;
                }
            case EventType.Deal:
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Deal");

                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~dice says Deal");

                    //Deals
                    if (!DisabledEvents.Contains(EventType.Deal))
                    {
                        NoEventsHereFar = Game.Player.Character.Position;
                        SpawnDrugDeal(IsInNamedArea(Game.Player.Character, "desrt"));
                        Eventflow.Add(EventType.Deal);
                    }
                    break;
                }
            case EventType.Racer:
                {

                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Racer");
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~b~dice says Racer");

                    //Racers
                    if (Racecars.Count > 0)
                    {
                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Attempting to spawn racer event...");

                        if (RandomInt(0, 10) >= 10)
                        {
                            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Racer event will be a race between multiple cars");

                            CreateRacers();
                            Eventflow.Add(EventType.Racer);
                        }
                        else
                        {

                            Model model = Racecars[RandomInt(0, Racecars.Count - 1)];
                            int p = 0;
                            while (!model.IsValid && p < 5)
                            {
                                p++;
                                model = Racecars[RandomInt(0, Racecars.Count - 1)];
                                Script.Wait(0);
                            }
                            if (model.IsValid)
                            {
                                Vehicle veh = World.CreateVehicle(model, FindHiddenSpot(50, true, true), RandomInt(0, 360));
                                if (CanWeUse(veh))
                                {
                                    RandomTuning(veh, true, false, true, IsNightTime(), false);
                                    Ped ped = veh.CreateRandomPedOnSeat(VehicleSeat.Driver);
                                    ped.AlwaysKeepTask = true;
                                    MoveEntitytoNearestRoad(veh, true, true);


                                    if (RandomInt(0, 10) >= 5)
                                    {
                                        Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, ped, veh, 10f, 1 + 2 + 8 + 32);
                                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Event will be a lone tuner vehicle");
                                    }
                                    else
                                    {
                                        Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, ped, veh, 30f, 1 + 2 + 4 + 8 + 16 + 32 + 512);
                                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Event will be a lone racer");
                                        ped.RelationshipGroup = RacersRLGroup;
                                    }

                                    if (!veh.CurrentBlip.Exists() && DebugBlips)
                                    {
                                        veh.AddBlip();
                                        veh.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                        veh.CurrentBlip.Scale = 0.7f;
                                        veh.CurrentBlip.Color = BlipColor.Yellow;
                                        veh.CurrentBlip.IsShortRange = true;
                                        veh.CurrentBlip.Name = "Racer";
                                    }
                                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Racing car spawned");

                                    //ped.IsPersistent = false;
                                    //veh.IsPersistent = false;
                                    TemporalPersistence.Add(ped);
                                    TemporalPersistence.Add(veh);
                                    BlacklistedEvents.Add(EventType.Racer);
                                    //EventCooldownTime = EventCooldownTime + 10000;
                                    Eventflow.Add(EventType.Racer);
                                    //     RacerCooldown = Game.GameTime + (1000 * 60 * 10);
                                    break;
                                }
                            }
                        }

                    }
                    break;
                }
            case EventType.Carjacker:
                {
                    CarjackerEnabled = true;
                    break;
                }
        }

        if (!Eventflow.Contains(eventspawnerint))
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("~o~" + eventspawnerint.ToString() + "did not spawn.");
            EventCooldownTime = Game.GameTime + 3000;
        }
        else
        {
            if (ForcedEvent != -1) ForcedEvent = -1;
        }
    }

    public static Vector3 GetOffset(Entity reference, Entity ent)
    {

        Vector3 pos = ent.Position;
        return Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, reference, pos.X, pos.Y, pos.Z);

    }
    void AddScenarioProbMultipliers()
    {

        int Random = RandomInt(0, 100);
        EmergencyScore = AccidentEventProb;
        Crimescore = CriminalEventProb;

        if (IsRaining())
        {
            EmergencyScore = +30;
        }
        if (IsNightTime() || GangAreas.Contains(World.GetZoneNameLabel(Game.Player.Character.Position))) Crimescore = +30;


        //        Crimescore += RandomInt((Crimescore * -1) / 2, Crimescore / 2);
        //      EmergencyScore += RandomInt((EmergencyScore * -1) / 2, EmergencyScore / 2);

        //If the 0-100 dice allows it, double their likeliness. Else, completely remove them, to make sure they're not common
        if (Crimescore > Random)
        {
            if (CurrentlyAllowedEvents.Contains(EventType.Carjacker)) CurrentlyAllowedEvents.Add(EventType.Carjacker);
            if (CurrentlyAllowedEvents.Contains(EventType.Deal)) CurrentlyAllowedEvents.Add(EventType.Deal);
            if (CurrentlyAllowedEvents.Contains(EventType.EmergencyRushing)) CurrentlyAllowedEvents.Add(EventType.EmergencyRushing);
            if (CurrentlyAllowedEvents.Contains(EventType.GangDriveby)) CurrentlyAllowedEvents.Add(EventType.GangDriveby);

        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                if (CurrentlyAllowedEvents.Contains(EventType.EmergencyRushing)) CurrentlyAllowedEvents.Remove(EventType.EmergencyRushing);
                if (CurrentlyAllowedEvents.Contains(EventType.Carjacker)) CurrentlyAllowedEvents.Remove(EventType.Carjacker);
                if (CurrentlyAllowedEvents.Contains(EventType.Deal)) CurrentlyAllowedEvents.Remove(EventType.Deal);
                if (CurrentlyAllowedEvents.Contains(EventType.EmergencyRushing)) CurrentlyAllowedEvents.Remove(EventType.EmergencyRushing);
                if (CurrentlyAllowedEvents.Contains(EventType.GangDriveby)) CurrentlyAllowedEvents.Remove(EventType.GangDriveby);
                if (CurrentlyAllowedEvents.Contains(EventType.Racer)) CurrentlyAllowedEvents.Remove(EventType.Racer);

            }
        }
        if (EmergencyScore > Random)
        {
            if (CurrentlyAllowedEvents.Contains(EventType.EmergencyRushing))
            {
                CurrentlyAllowedEvents.Add(EventType.EmergencyRushing);
            }
            if (CurrentlyAllowedEvents.Contains(EventType.Tow)) CurrentlyAllowedEvents.Add(EventType.Tow);
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                if (CurrentlyAllowedEvents.Contains(EventType.EmergencyRushing)) CurrentlyAllowedEvents.Remove(EventType.EmergencyRushing);
            }

        }

        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Requirement: " + Random + "~n~criminal: " + Crimescore + "~n~accident: " + EmergencyScore);

    }
    void CreateCriminalEvent()
    {
        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("CreateCriminalEvent()");
        if (RandomInt(0, 10) <= 5)
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Carjacker spawned.");

            CarjackerEnabled = true;
        }
        else
        {
            if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("[Event] Busy copcar spawned.");
            //SpawnChase(FindHiddenSpot(50,true),false);
            if (!BlacklistedEvents.Contains(EventType.EmergencyRushing))
            {
                SpawnEmergencyVehicle(EmergencyType.POLICE);
                BlacklistedEvents.Add(EventType.EmergencyRushing);
            }
        }

    }

    Vector3 GetclosestMajorVehNode(Vector3 reference)
    {
        Vector3 node = reference;

        OutputArgument outArgA = new OutputArgument();
        OutputArgument outArgB = new OutputArgument();


        if (Function.Call<bool>(Hash.GET_CLOSEST_MAJOR_VEHICLE_NODE, reference.X, reference.Y, reference.Z, outArgA, 3.0f, 0))
        {
            node = outArgA.GetResult<Vector3>(); //Get position
        }
        return node;
    }


    void SpawnChase(Vector3 pos, bool big)
    {
        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("chase spawned");
        Vehicle criminalveh;
        Ped criminalped;

        Vehicle copveh;
        Ped copped;

        if (big)
        {

        }
        else
        {
            criminalveh = World.CreateVehicle(RandomNormalVehicle(), FindHiddenSpot(50, true, false), 0f);
            criminalped = Function.Call<Ped>(Hash.CREATE_RANDOM_PED_AS_DRIVER, criminalveh, true);
            PreparePed(criminalped, true);
            Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER, criminalped, criminalveh, 90f, (4 + 16 + 32 + 262144));
            criminalped.RelationshipGroup = CriminalRelGroup;
            MoveEntitytoNearestRoad(criminalveh);

            copveh = World.CreateVehicle(GetRandomPoliceVehicle(), criminalveh.Position + (criminalveh.ForwardVector * -5), criminalveh.Heading);
            copped = Function.Call<Ped>(Hash.CREATE_RANDOM_PED_AS_DRIVER, copveh, true);
            copveh.SirenActive = true;
            PreparePed(copped, false);
            copped.Task.VehicleChase(criminalped);
            copped.Weapons.Give(WeaponHash.Pistol, -1, true, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, copped, 2, false);
            copped.RelationshipGroup = Function.Call<int>(GTA.Native.Hash.GET_HASH_KEY, "COP");
            Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, criminalveh, 10f);
            Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, copveh, 10f);

            criminalped.Task.FleeFrom(copped);

            copveh.AddBlip();


            Script.Wait(5000);
            copveh.IsPersistent = false;
            criminalveh.IsPersistent = false;
            copped.IsPersistent = false;
            criminalped.IsPersistent = false;
        }
    }



    Vector3 FindHiddenSpot(float distance, bool road, bool AheadPlayer)
    {
        Vector3 spot = World.GetNextPositionOnStreet(PlayerPed().Position.Around(distance));

        if (road)
        {
            for (int i = 0; i < 20; i++)
            {
                spot = World.GetNextPositionOnStreet(PlayerPed().Position.Around(distance + (i * 2)));
                RaycastResult raycast = World.Raycast(PlayerPed().Position + new Vector3(0, 0, 4), spot + new Vector3(0, 0, 4), IntersectOptions.Map);
                if (raycast.DitHitAnything || PlayerPed().Position.DistanceTo(spot) > 100f)
                {
                    if (!AheadPlayer || IsPosAheadEntity(Game.Player.Character, spot) > 0)
                    {
                        if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("[HiddenSpot] Found place, " + i + "º try");

                    }
                    break;
                }
                else if (i == 19 && Debug >= DebugLevel.EventsAndScenarios) UI.Notify("[HiddenSpot] ~r~Didn't find appropiate place");
            }
            return spot;
        }
        else
        {
            for (int i = 0; i < 20; i++)
            {
                spot = World.GetSafeCoordForPed(PlayerPed().Position.Around(distance));
                RaycastResult raycast = World.Raycast(PlayerPed().Position, spot + new Vector3(0, 0, 1), IntersectOptions.Map);
                if (raycast.DitHitAnything || PlayerPed().Position.DistanceTo(spot) > 100f)
                {
                    if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("[HiddenSpot] Found place, " + i + "º try");
                    break;
                }
                else if (i == 19 && Debug >= DebugLevel.EventsAndScenarios) UI.Notify("[HiddenSpot] ~r~Didn't find appropiate place");
            }
            return spot;
        }
    }

    void CreateAccidentEvent()
    {
        if (!BlacklistedEvents.Contains(EventType.EmergencyRushing))
        {
            int type = RandomInt(1, 2);
            switch (type)
            {
                case 1: SpawnEmergencyVehicle(EmergencyType.FIRETRUCK); break;
                case 2: SpawnEmergencyVehicle(EmergencyType.AMBULANCE); break;
                case 3: CauseAccident(); break;
                case 4: CauseAccident(); break;
                case 5: CauseAccident(); break;
            }
            BlacklistedEvents.Add(EventType.EmergencyRushing);
        }

    }


    public static void MoveEntitytoNearestRoad(Entity E)
    {
        if (CanWeUse(E))
        {
            OutputArgument outArgA = new OutputArgument();
            OutputArgument outArgB = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, E.Position.X, E.Position.Y, E.Position.Z, outArgA, outArgB, 0, 1077936128, 0))
            {
                E.Heading = outArgB.GetResult<float>(); //getting heading
            }

            if (Function.Call<bool>(Hash.GET_CLOSEST_MAJOR_VEHICLE_NODE, E.Position.X, E.Position.Y, E.Position.Z, outArgA, outArgB, 3.0f, 0))
            {
                E.Position = outArgA.GetResult<Vector3>(); //Get position
            }
        }
    }

    static public void MoveEntitytoNearestRoad(Entity E, bool move, bool heading)
    {
        OutputArgument outArgA = new OutputArgument();
        OutputArgument outArgB = new OutputArgument();


        if (Function.Call<bool>(Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, E.Position.X, E.Position.Y, E.Position.Z, outArgA, outArgB, 1, 1077936128, 0))
        {
            if (move) E.Position = outArgA.GetResult<Vector3>(); //Get position
            if (heading) E.Heading = outArgB.GetResult<float>(); //getting heading
        }
    }
    Ped PlayerPed()
    {
        return Game.Player.Character;
    }

    static public bool CanWeUse(Entity entity)
    {
        return entity != null && entity.Exists();
    }


    static public string RandomNormalVehicle()
    {
        return NormalVehicleModel[RandomInt(0, NormalVehicleModel.Count - 1)].ToString();
    }

    string RandomVehInMemory()
    {


        OutputArgument outArgA = new OutputArgument();
        OutputArgument outArgB = new OutputArgument();

        Function.Call(Hash.GET_RANDOM_VEHICLE_MODEL_IN_MEMORY, true, outArgA, outArgB);

        return outArgA.GetResult<string>() + outArgB.GetResult<string>();
    }


    Model GetRandomPoliceVehicle()
    {

        if (CopVehicles.Count > 0) return CopVehicles[RandomInt(0, CopVehicles.Count - 1)];
        if (PlayerPed().IsInRangeOf(CityCenter, 3000))
        {
            switch (RandomInt(0, 3))
            {
                case 0:
                    {
                        return "police";
                    }
                case 1:
                    {
                        return "police2";
                    }
                case 2:
                    {
                        return "police3";
                    }
                case 3:
                    {
                        return "police4";
                    }
            }
        }
        else
        {
            switch (RandomInt(1, 2))
            {
                case 1:
                    {
                        return "sheriff";
                    }
                case 2:
                    {
                        return "sheriff2";
                    }
            }
        }
        return "police";
    }

    void SpawnTow()
    {
        if (!BlacklistedEvents.Contains(EventType.Tow))
        {
            if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Spawning tow event");

            //FindHiddenSpot(100f, true);
            Vehicle tow = null;
            foreach (Vehicle v in AllVehicles) if (!DecorExistsOn("Ignore", v) && !v.PreviouslyOwnedByPlayer && v.Model == VehicleHash.TowTruck && !v.IsPersistent && !WouldPlayerNoticeChangesHere(v.Position)) { tow = v; break; }

            if (!CanWeUse(tow))
            {
                int patience = 0;
                Vector3 hiddenpos = Vector3.Zero;


                while (patience < 30 && (hiddenpos == Vector3.Zero || !WouldPlayerNoticeChangesHere(hiddenpos)))
                {
                    hiddenpos = GenerateSpawnPos(Game.Player.Character.Position.Around(10 + (patience * 10)), Nodetype.Road, false);
                    patience++;
                }
                if (WouldPlayerNoticeChangesHere(hiddenpos) || Game.Player.Character.IsInRangeOf(hiddenpos, 20f))
                {
                    if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Didn't find any proper hidden position for the tow.");
                    return;
                }
                tow = World.CreateVehicle(VehicleHash.TowTruck, hiddenpos, 0);
            }


            Ped towdriver = tow.Driver;

            if (!CanWeUse(towdriver)) towdriver = Function.Call<Ped>(Hash.CREATE_RANDOM_PED_AS_DRIVER, tow, true);

            if (CanWeUse(towdriver) && CanWeUse(tow))
            {
                PreparePed(towdriver, false);

                MoveEntitytoNearestRoad(tow, true, true);

                Script.Wait(500);
                Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER, towdriver, tow, 15f, (1 + 24 + 16 + 32 + 262144));

                Model model = GetRandomVehicleHash();
                if (model.Hash == tow.Model.Hash) model = "blista";
                Vehicle towed = World.CreateVehicle(model, tow.Position + (tow.ForwardVector * -5), tow.Heading);
                //towed.Heading = tow.Heading;


                Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, tow, 4f);
                Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, towed, 4f);

                Function.Call(Hash.ATTACH_VEHICLE_TO_TOW_TRUCK, tow, towed, false, 0, 0, 0);
                Function.Call(Hash._SET_TOW_TRUCK_CRANE_RAISED, tow, 1f);

                if (DebugBlips && !tow.CurrentBlip.Exists())
                {
                    tow.AddBlip();
                    //tow.CurrentBlip.Color = BlipColor.Green;
                    tow.CurrentBlip.Sprite = BlipSprite.TowTruck;
                    tow.CurrentBlip.Scale = 0.7f;
                    tow.CurrentBlip.IsShortRange = true;

                }
                tow.EngineRunning = true;
                tow.SirenActive = true;
                tow.IsPersistent = false;
                towdriver.IsPersistent = false;
                towed.IsPersistent = false;

                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Tow + vehicle spawned");
                BlacklistedVehicles.Add(tow);
                BlacklistedVehicles.Add(towed);
            }
            else
            {
                if (Debug >= DebugLevel.EventsAndScenarios) UI.Notify("Tow + vehicle failed to spawn");

            }
            BlacklistedEvents.Add(EventType.Tow);

        }
    }
    public static Ped GetOwner(Vehicle v)
    {
        OutputArgument Owner = new OutputArgument();

        Function.Call(Hash._GET_VEHICLE_OWNER, v, Owner);

        Ped Driver = Owner.GetResult<Ped>();

        if (CanWeUse(Driver)) return Driver; else return null;
    }
    public static Model GetRandomVehicleHash()
    {
        foreach (Vehicle veh in World.GetAllVehicles())
        {
            if (veh.Model.IsCar) return veh.Model;
        }
        return "blista";
    }

    void SpawnEmergencyVehicle(EmergencyType E)
    {

        List<Model> VehicleModel = new List<Model> { "police", };

        List<Model> Ambulances = new List<Model> { "ambulance", "ambulance2", "ambulance3" };
        List<Model> Cops = new List<Model> { "riot", "riot2", "riot2" };

        Model type = 0;// (int)VehicleHash.Police;
        switch (E)
        {
            case EmergencyType.AMBULANCE:
                {
                    VehicleModel = new List<Model> { "ambulance", "ambulance2", "ambulance3" };
                    /// type = Ambulances[RandomInt(0, Ambulances.Count - 1)];
                    break;
                }
            case EmergencyType.FIRETRUCK:
                {
                    VehicleModel = new List<Model> { "firetruk", "riot3", };

                    // type =  (int)VehicleHash.FireTruck;
                    break;
                }
            case EmergencyType.POLICE:
                {
                    VehicleModel = Cops;
                    //if (CopVehicles.Count > 0) VehicleModel = CopVehicles;
                    //else type = GetRandomPoliceVehicle();
                    break;
                }
        }
        for (int patience = 0; patience < 20; patience++) if (!type.IsValid) type = VehicleModel[RandomInt(0, VehicleModel.Count - 1)];

        Vehicle vehicle = null;
        Ped Driver = null;

        foreach (Vehicle veh in World.GetNearbyVehicles(PlayerPed().Position, 30))
        {
            if (veh.Model.Hash == type && CanWeUse(veh.GetPedOnSeat(VehicleSeat.Driver)) && !veh.GetPedOnSeat(VehicleSeat.Driver).IsPersistent && veh.GetPedOnSeat(VehicleSeat.Driver).IsAlive && !veh.GetPedOnSeat(VehicleSeat.Driver).IsInCombat)
            {
                vehicle = veh;
                Driver = veh.GetPedOnSeat(VehicleSeat.Driver);
            }
        }
        if (!CanWeUse(vehicle))
        {
            Vector3 pos = Vector3.Zero;
            for (int newPatience = 0; newPatience < 200; newPatience++) if (pos == Vector3.Zero || (WouldPlayerNoticeChangesHere(pos) && Game.Player.Character.IsInRangeOf(pos, 100f))) pos = GenerateSpawnPos(Game.Player.Character.Position.Around(20 + (newPatience * 2)), Nodetype.Road, false);

            if (pos == Vector3.Zero) return;
            vehicle = World.CreateVehicle(type, pos, 0);


            if (!CanWeUse(vehicle))
            {
                return;
            }
        }
        if (!CanWeUse(Driver))
        {
            switch (E)
            {
                case EmergencyType.POLICE:
                    {
                        Driver = World.CreatePed(PedHash.Cop01SFY, vehicle.Position, 0);
                        if (CanWeUse(Driver))
                        {
                            Driver.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                        }
                        else
                        {
                            Driver = Function.Call<Ped>(Hash.CREATE_RANDOM_PED_AS_DRIVER, vehicle, true);
                            List<Entity> peds = new List<Entity>();

                            int seats = 1;
                            for (int i = 0; i < seats; i++)
                            {
                                Ped p = World.CreatePed(PedHash.Cop01SMY, vehicle.Position.Around(5));
                                peds.Add(p);
                                p.IsPersistent = false;
                            }
                        }
                        break;
                    }
                case EmergencyType.FIRETRUCK:
                    {
                        Driver = Function.Call<Ped>(Hash.CREATE_RANDOM_PED_AS_DRIVER, vehicle, true);
                        List<Ped> peds = new List<Ped>();

                        int seats = vehicle.PassengerSeats;
                        for (int i = 1; i < seats; i++)
                        {
                            Ped p = World.CreatePed(PedHash.Fireman01SMY, vehicle.Position.Around(5));
                            peds.Add(p);
                            p.IsPersistent = false;
                        }
                        SetPedsIntoVehicle(peds, vehicle);
                        break;
                    }
                case EmergencyType.AMBULANCE:
                    {
                        Driver = Function.Call<Ped>(Hash.CREATE_RANDOM_PED_AS_DRIVER, vehicle, true);
                        List<Ped> peds = new List<Ped>();

                        int seats = vehicle.PassengerSeats;
                        for (int i = 1; i < seats; i++)
                        {
                            Ped p = World.CreatePed(PedHash.Paramedic01SMM, vehicle.Position.Around(5));
                            peds.Add(p);
                            p.IsPersistent = false;
                        }
                        SetPedsIntoVehicle(peds, vehicle);
                        break;
                    }
            }
        }
        List<int> CityLiveries = new List<int> { 4, 5, 6 };
        List<int> BlaineLiveries = new List<int> { 3 };
        if (CanWeUse(vehicle) && CanWeUse(Driver))
        {
            if ((vehicle.Model == "ambulance2" || vehicle.Model == "ambulance3"))
            {
                CityLiveries = new List<int> { 4, 5, 6 };
                BlaineLiveries = new List<int> { 3 };
                {
                    if (GetMapAreaAtCoords(vehicle.Position) == "city")
                    {
                        vehicle.Livery = CityLiveries[RandomInt(0, CityLiveries.Count - 1)];
                    }
                    else
                    {
                        vehicle.Livery = BlaineLiveries[RandomInt(0, BlaineLiveries.Count - 1)];
                    }
                }
            }
            if ((vehicle.Model == "firetruk" || vehicle.Model == "riot3"))
            {
                CityLiveries = new List<int> { 2 };
                BlaineLiveries = new List<int> { 1 };
            }
            if (vehicle.Model == "riot2")
            {

                vehicle.SetMod(VehicleMod.Livery, -1, false);
                vehicle.PrimaryColor = VehicleColor.PureWhite;
                vehicle.SecondaryColor = VehicleColor.PureWhite;
            }
            Vector3 Dest = World.GetNextPositionOnStreet(LerpByDistance(vehicle.Position, Game.Player.Character.Position, 300f));
            vehicle.Heading = Function.Call<float>(GTA.Native.Hash.GET_ANGLE_BETWEEN_2D_VECTORS, vehicle.Position.X, vehicle.Position.Y, Game.Player.Character.Position.X, Game.Player.Character.Position.Y);
            MoveEntitytoNearestRoad(vehicle, true, true);
            PreparePed(Driver, false);
            vehicle.SirenActive = true;




            //if (!IsEntityAheadEntity(Game.Player.Character, vehicle)) vehicle.Heading += 180;
            // Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER, Driver, vehicle, 30f, (4 + 16 + 32 + 262144));
            Script.Wait(200);
            Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, vehicle, 20f);

            if (DebugBlips)
            {
                vehicle.AddBlip();
                vehicle.CurrentBlip.Color = BlipColor.Blue;
                vehicle.CurrentBlip.IsFlashing = true;
                vehicle.CurrentBlip.Scale = 0.7f;
                vehicle.CurrentBlip.IsShortRange = true;
                vehicle.CurrentBlip.Name = "Rushing " + vehicle.FriendlyName;
            }
            List<Vehicle> ToUnPersist = new List<Vehicle>();


            ToUnPersist.Add(vehicle);
            EventCooldownTime = Game.GameTime + EventCooldown;
            BlacklistedEvents.Add(EventType.EmergencyRushing);
            vehicle.SirenActive = true;
            //TemporalPersistence.Add(vehicle);
            //TemporalPersistence.Add(vehicle.Driver);



            TaskSequence seq = new TaskSequence();
            Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, vehicle, Dest.X, Dest.Y, Dest.Z, 30f, 4 + 8 + 16 + 32, 10f);
            Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER, 0, vehicle, 30f, (1 + 2 + 4 + 16 + 32 + 262144));
            seq.Close(false);
            vehicle.Driver.Task.PerformSequence(seq);
            seq.Dispose();
            List<Model> Escorts = new List<Model> { "police" };
            int EscortLivery = -1;

            vehicle.Alpha = 0;
            vehicle.Driver.Alpha = 0;
            FadeIn.Add(vehicle);
            FadeIn.Add(vehicle.Driver);
            if (E == EmergencyType.AMBULANCE || E == EmergencyType.FIRETRUCK)
            {
                Escorts = new List<Model> { "emsvan", "emscar", "emscar2", "emssuv" };
                if (GetMapAreaAtCoords(vehicle.Position) == "city")
                {
                    EscortLivery = CityLiveries[RandomInt(0, CityLiveries.Count - 1)];
                }
                else
                {
                    EscortLivery = BlaineLiveries[RandomInt(0, BlaineLiveries.Count - 1)];
                }
            }
            if (E == EmergencyType.POLICE)
            {
                if (GetMapAreaAtCoords(vehicle.Position) == "city")
                {
                    Escorts = new List<Model> { "police", };
                }
                else
                {
                    Escorts = new List<Model> { "sheriff", };
                }
            }

            float FarBehind = vehicle.Model.GetDimensions().Y;
            for (int i = 1; i < RandomInt(2, 4); i++)
            {
                Model emsModel = null;
                for (int patience = 0; patience < 20; patience++) if (!emsModel.IsValid) emsModel = Escorts[RandomInt(0, Escorts.Count - 1)];

                if (emsModel.IsValid)
                {
                    Vehicle v = World.CreateVehicle(emsModel, vehicle.Position + (vehicle.ForwardVector * -FarBehind), vehicle.Heading);
                    Script.Wait(50);
                    if (CanWeUse(v))
                    {
                        FarBehind += v.Model.GetDimensions().Y;
                        if (EscortLivery != -1) v.Livery = EscortLivery;

                        v.CreateRandomPedOnSeat(VehicleSeat.Driver);
                        Script.Wait(50);
                        if (CanWeUse(v.Driver))
                        {
                            v.Driver.AlwaysKeepTask = true;

                            v.Alpha = 0;
                            v.Driver.Alpha = 0;
                            FadeIn.Add(v);
                            FadeIn.Add(v.Driver);
                            // Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, v.Driver, v ,vehicle.Driver, 12, 4f,  4 + 8 + 16 + 32, 1f, 1f, false);
                            //    Function.Call(Hash.TASK_VEHICLE_ESCORT, v.Driver, v, vehicle.Driver, 0, 20f, 4 + 8 + 16 + 32, 1.5f, 5f, 10f);

                            //  Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER, v.Driver, v, 30f, (4 + 16 + 32 + 262144));
                            Function.Call(Hash.SET_VEHICLE_FORWARD_SPEED, v, 15f);
                            //v.Driver.IsPersistent = false;
                            v.SirenActive = true;
                            ToUnPersist.Add(v);
                            if (DebugBlips)
                            {
                                v.AddBlip();
                                v.CurrentBlip.Color = BlipColor.Blue;
                                v.CurrentBlip.IsFlashing = true;
                                v.CurrentBlip.Scale = 0.7f;
                                v.CurrentBlip.IsShortRange = true;
                                v.CurrentBlip.Name = "Rushing " + vehicle.FriendlyName;
                            }

                              seq = new TaskSequence();

                            Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, v, Dest.X, Dest.Y, Dest.Z, 30f, 1 + 2 + 4 + 8 + 16 + 32, 10f);
                            Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER, 0, v, 30f, (1+2+4 + 16 + 32 + 262144));
                            seq.Close(false);
                            v.Driver.Task.PerformSequence(seq);
                            seq.Dispose();
                            /*
                            seq = new TaskSequence();

                            //  Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, v, Dest.X, Dest.Y, Dest.Z, 30f, 4 + 8 + 16 + 32, 10f);
                            //  Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_WANDER,0, v, 30f, (4 + 16 + 32 + 262144));
                            Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, v.Driver, v, vehicle.Driver, 12, 40f, 1 + 2 + 4 + 8 + 16 + 32, 1f, 1f, false);

                            seq.Close(false);
                            v.Driver.Task.PerformSequence(seq);
                            seq.Dispose();*/
                        }
                        else
                        {
                            v.Delete();
                        }
                        //v.IsPersistent = false;
                        // TemporalPersistence.Add(v);

                    }
                }


            }

            foreach (Vehicle v in ToUnPersist)
            {
                if (CanWeUse(v))
                {
                    TemporalPersistence.Add(v);

                    //v.IsPersistent = false;                
                    if (CanWeUse(v.Driver)) TemporalPersistence.Add(v.Driver);//v.Driver.IsPersistent = false;
                }
            }
            //Driver.IsPersistent = false;
            //vehicle.IsPersistent = false;
        }
        else
        {
            if (CanWeUse(vehicle)) vehicle.MarkAsNoLongerNeeded();
            if (CanWeUse(Driver)) Driver.MarkAsNoLongerNeeded();
        }
    }


    void PreparePed(Ped ped, bool BlockEvents)
    {
        ped.IsPersistent = true;
        ped.AlwaysKeepTask = true;
        ped.BlockPermanentEvents = true;
        Function.Call(GTA.Native.Hash.SET_DRIVER_ABILITY, ped, 100f);
        Function.Call(GTA.Native.Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 46, true);
        //ped.Weapons.Give(WeaponHash.Bat, -1, false, true);
    }



    void SetPedsIntoVehicle(List<Ped> Squad, Vehicle Vehicle)
    {

        if (Squad.Count == 0) return;
        int max_seats = Function.Call<int>(GTA.Native.Hash.GET_VEHICLE_MAX_NUMBER_OF_PASSENGERS, Vehicle);
        for (int i = -1; i < max_seats; i++)
        {
            if (i >= Squad.Count - 1)
            {
                break;
            }
            for (int s = -2; s < 20; s++)
            {
                if (Function.Call<bool>(Hash.IS_VEHICLE_SEAT_FREE, Vehicle, s) && (CanWeUse(Squad[i + 1])))
                {
                    Function.Call<bool>(Hash.TASK_ENTER_VEHICLE, Squad[i + 1], Vehicle, 10000, s, 2.0, 16, 0);
                    break;
                }
            }

        }
    }
    bool DoesVehicleHavePassengers(Vehicle Vehicle)
    {
        int max_seats = Function.Call<int>(GTA.Native.Hash.GET_VEHICLE_MAX_NUMBER_OF_PASSENGERS, Vehicle);
        for (int i = 0; i < max_seats; i++)
        {
            if (!Function.Call<bool>(GTA.Native.Hash.IS_VEHICLE_SEAT_FREE, Vehicle, i)) return true;
        }
        return false;
    }

    string GetZoneName(Vector3 pos)
    {
        return Function.Call<string>(GTA.Native.Hash.GET_NAME_OF_ZONE, pos.X, pos.Y, pos.Z);
    }


    void LoadSettings()
    {
        ScriptSettings config = ScriptSettings.Load(@"scripts\LivelyWorld.ini");

        AccidentEventProb = config.GetValue<int>("OPTIONS", "AccidentEventBaseProb", 20);
        CriminalEventProb = config.GetValue<int>("OPTIONS", "CriminalEventBaseProb", 10);
        //BlackistedImportantEventsCooldown = config.GetValue<int>("OPTIONS", "ImportantEventsCooldown", 1);

        EventCooldown = config.GetValue<int>("OPTIONS", "EventCooldown", 60) * 1000;
        EventFrecuency = config.GetValue<int>("OPTIONS", "EventFrecuency", 50);


        InteractionCooldown = config.GetValue<int>("OPTIONS", "InteractionCooldown", 20) * 1000;
        InteractionFrecuency = config.GetValue<int>("OPTIONS", "InteractionFrecuency", 50);

        InteractionRange = config.GetValue<float>("OPTIONS", "InteractionRange", 200f);

        VehicleReplacer = config.GetValue<bool>("OPTIONS", "TrafficReplacer", true);
        TrafficInjector = config.GetValue<bool>("OPTIONS", "TrafficInjector", true);


        TruckRespray = config.GetValue<bool>("OPTIONS", "TruckRespray", true);


        Debug = config.GetValue<DebugLevel>("OPTIONS", "Debugmode", DebugLevel.None);
        DebugBlips = config.GetValue<bool>("OPTIONS", "DebugBlips", false);
        DebugOutput = config.GetValue<bool>("OPTIONS", "DebugOutput", false);


        //XML
        string ConfigFile = @"scripts\\LivelyWorld.xml";

        XmlDocument document = new XmlDocument();
        document.Load(ConfigFile);
        int pat = 0;
        while (document == null && pat < 500)
        {
            document.Load(ConfigFile);
            Script.Wait(0);
        }

        if (document == null) UI.Notify("~o~LivelyWorld couldn't find the xml file.");
        XmlElement root = document.DocumentElement;
        if (Debug >= DebugLevel.EventsAndScenarios) AddNotification("CHAR_SOCIAL_CLUB", "~b~" + ScriptName + " " + ScriptVer, "LOADING", "Loading settings...");
        if (Debug >= DebugLevel.EventsAndScenarios) AddNotification("", "", "", "Replacers:");
        int Replaced = 0;
        foreach (XmlElement e in root.SelectNodes("//Replacer/model"))
        {
            Model source = e.GetAttribute("source");
            Model target = e.GetAttribute("target");
            if ((source.IsValid || source == "") && target.IsValid)
            {
                Replaced++;
                if (source.IsValid) MonitoredModels.Add(source);
                if (Debug >= DebugLevel.EventsAndScenarios) AddNotification("", "", "", "Replacer: " + e.GetAttribute("source") + " > " + e.GetAttribute("target"));
                ReplacersList.Add(new Replacer(e.GetAttribute("source"), e.GetAttribute("target"), e.GetAttribute("tuned") == "true", e.GetAttribute("timeframe"), e.GetAttribute("zone"), e.GetAttribute("source") + " to " + e.GetAttribute("target") + " - " + e.GetAttribute("timeframe")));
            }
        }

        int Spawned = 0;
        foreach (XmlElement e in root.SelectNodes("//Spawner/Traffic/*"))
        {
            Model source = e.GetAttribute("source");
            if ((source.IsValid))
            {
                TerrainType terrain = TerrainType.Road;

                if (e.GetAttribute("terrain") == "air") terrain = TerrainType.Air;
                if (e.GetAttribute("terrain") == "water") terrain = TerrainType.Water;
                if (e.GetAttribute("terrain") == "offroad") terrain = TerrainType.Offroad;
                int cooldown = 3;
                int.TryParse(e.GetAttribute("frecuency"), out cooldown);

                if (cooldown == -1) cooldown = RandomInt(5, 30);

                int prob = 50;
                int.TryParse(e.GetAttribute("probability"), out prob);

                if (prob == -1) prob = RandomInt(20, 80);

                Spawned++;
                if (Debug >= DebugLevel.EventsAndScenarios) AddNotification("", "", "", "Spawner: " + e.GetAttribute("source") + " > " + e.GetAttribute("target")); // if (Debug >= DebugLevel.EventsAndScenarios)
                TrafficSpawnerList.Add(new TrafficSpawner(e.GetAttribute("source"), e.GetAttribute("timeframe"), e.GetAttribute("zone"), terrain, cooldown, prob));
            }
        }
        int Customs = 0;
        string text = "";
        foreach (XmlElement e in root.SelectNodes("//Bennys/model"))
        {
            Model car = e.GetAttribute("source");
            if (car.IsValid)
            {
                Customs++;
                Bennys.Add(e.GetAttribute("source"));
                text += e.GetAttribute("source") + ", ";
            }
        }
        if (Debug >= DebugLevel.EventsAndScenarios) AddNotification("", "", "", "Benny's: " + text);

        int Racers = 0;
        text = "";
        foreach (XmlElement e in root.SelectNodes("//Racers/model"))
        {
            Model car = e.GetAttribute("source");
            if (car.IsValid)
            {
                Racecars.Add(e.GetAttribute("source"));
                text += e.GetAttribute("source") + ", ";
                Racers++;
            }
        }
        if (Debug >= DebugLevel.EventsAndScenarios) AddNotification("", "", "", "Racers:" + text);

        foreach (XmlElement e in root.SelectNodes("//Wrecks/model"))
        {
            Model car = e.GetAttribute("source");
            if (car.IsValid)
            {
                WreckCarModels.Add(e.GetAttribute("source"));
            }
        }
        //Disabled events
        string EventText = "";
        foreach (XmlElement e in root.SelectNodes("//DisabledEvents/*"))
        {
            Array events;
            events = Enum.GetValues(typeof(EventType));

            foreach (var ev in events)
            {
                if (e.InnerText == ev.ToString())
                {
                    DisabledEvents.Add((EventType)ev);
                    EventText += ev.ToString() + ", ";
                }
            }
        }


        string ScenarioText = "";
        foreach (XmlElement e in root.SelectNodes("//DisabledScenarios/*"))
        {
            Array events;
            events = Enum.GetValues(typeof(ScenarioType));

            foreach (var ev in events)
            {
                if (e.InnerText == ev.ToString())
                {
                    DisabledScenarios.Add((ScenarioType)ev);
                    ScenarioText += ev.ToString() + ", ";
                }
            }
        }

        Notify("CHAR_SOCIAL_CLUB", "~b~" + ScriptName, "Loaded Info", Spawned + " vehicle spawners created.~n~" + Replaced + " vehicles injected into traffic.");
        Notify("", "", "", Racers + " Racers detected.~n~" + Customs + " Benny's vehicles detected.");

        if (DisabledEvents.Count > 0) AddNotification("", "", "", "~o~Disabled events: " + EventText);
        if (DisabledScenarios.Count > 0) AddNotification("", "", "", "~o~Disabled Scenarios: " + ScenarioText);

        if (DebugOutput) File.AppendAllText(@"scripts\LivelyWorldDebug.txt", "\n" + DateTime.Now + " - Settings loaded.");

    }
    List<Vehicle> Racers = new List<Vehicle>();
    Vector3 finishline = Vector3.Zero;
    Vector3 hangout = Vector3.Zero;
    bool CleanRacers = false;
    int RacersRLGroup = World.AddRelationshipGroup("RacersRLGroup");

    public void CreateRacers()
    {
        Vector3 spawnpoint = GenerateSpawnPos(Game.Player.Character.Position.Around(200), Nodetype.Road, false);

        Vector3 finishline = Vector3.Zero;





        for (int i = 0; i < RandomInt(1, 3); i++)
        {
            Vehicle v = World.CreateVehicle(Racecars[RandomInt(0, Racecars.Count - 1)], spawnpoint);

            if (finishline == Vector3.Zero)
            {
                finishline = GenerateSpawnPos(LerpByDistance(v.Position, Game.Player.Character.Position, 800), Nodetype.Road, false);
                //  if(hangout == Vector3.Zero)  hangout = GenerateSpawnPos(finishline, Nodetype.Offroad, false);
            }

            MoveEntitytoNearestRoad(v, false, true);
            spawnpoint += v.ForwardVector * -5;
            v.CreateRandomPedOnSeat(VehicleSeat.Driver);
            //Racers.Add(v);
            RandomTuning(v, true, false, true, false, false);
            Function.Call(Hash.SET_ENTITY_LOAD_COLLISION_FLAG, v, true);

            Function.Call(GTA.Native.Hash.SET_VEHICLE_RADIO_ENABLED, v, true);
            Function.Call(GTA.Native.Hash.SET_VEHICLE_RADIO_LOUD, v, true);

            v.RadioStation = RadioStation.ChannelX;
            v.Speed = 5f;
            v.IsAxlesStrong = true;

            if (DebugBlips)
            {
                v.AddBlip();
                v.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                v.CurrentBlip.Color = BlipColor.White;
                v.CurrentBlip.Name = "Racer";
            }

            v.Driver.RelationshipGroup = RacersRLGroup;
            Script.Wait(100);
            //v.Driver.SetConfigFlag(46, false);
            //v.Driver.SetConfigFlag(17, true);

            TemporalPersistence.Add(v.Driver);

            v.IsPersistent = false;


            TaskSequence seq = new TaskSequence();

            Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, v, finishline.X, finishline.Y, finishline.Z, 200f, 4 + 8 + 16 + 32, 10f);
            Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, v, spawnpoint.X, spawnpoint.Y, spawnpoint.Z, 200f, 8 + 16 + 32, 10f);
            seq.Close(true);
            v.Driver.Task.PerformSequence(seq);
            seq.Dispose();
            v.Driver.BlockPermanentEvents = false;

            Function.Call(Hash.ADD_SHOCKING_EVENT_FOR_ENTITY, 86, v, 4000f);


        }

        CleanRacers = false;

        //UI.Notify("Racers spawned");
    }

    public static int IsRoadBusy(Vector3 pos)
    {
        OutputArgument outArgA = new OutputArgument();
        OutputArgument outArgB = new OutputArgument();
        if (Function.Call<bool>(Hash.GET_VEHICLE_NODE_PROPERTIES, pos.X, pos.Y, pos.Z, outArgA, outArgB))
        {
            int busy = outArgA.GetResult<int>();
            int flags = outArgB.GetResult<int>();

            //DisplayHelpTextThisFrame("Busy:" + busy + "~n~Flags:" + flags);

            return busy;
            //BOOL GET_VEHICLE_NODE_PROPERTIES(float x, float y, float z, int *density, int* flags) // 0x0568566ACBB5DEDC 0xCC90110B
        }
        return 0;
    }





    public enum PathnodeFlags
    {
        Slow = 1,
        Two = 2,
        Intersection = 4,
        Eight = 8, SlowTraffic = 12, ThirtyTwo = 32, Freeway = 64, FourWayIntersection = 128, BigIntersectionLeft = 512
    }
    public static string GetRoadFlags(Vector3 pos)
    {
        OutputArgument outArgA = new OutputArgument();
        OutputArgument outArgB = new OutputArgument();
        if (Function.Call<bool>(Hash.GET_VEHICLE_NODE_PROPERTIES, pos.X, pos.Y, pos.Z, outArgA, outArgB))
        {
            int busy = outArgA.GetResult<int>();
            int flags = outArgB.GetResult<int>();

            string d = "";
            foreach (int flag in Enum.GetValues(typeof(PathnodeFlags)).Cast<PathnodeFlags>())
            {

                if ((flag & flags) != 0) d += " " + (PathnodeFlags)flag;
            }
            return d;  // DisplayHelpTextThisFrame("Flags: " + d);
        }
        return "";
    }
    public static bool RoadHasFlag(Vector3 pos, PathnodeFlags flag)
    {
        OutputArgument outArgA = new OutputArgument();
        OutputArgument outArgB = new OutputArgument();
        if (Function.Call<bool>(Hash.GET_VEHICLE_NODE_PROPERTIES, pos.X, pos.Y, pos.Z, outArgA, outArgB))
        {
            int busy = outArgA.GetResult<int>();
            int flags = outArgB.GetResult<int>();
            if ((flags & (int)flag) != 0) return true;
        }
        return false;
    }

    public static bool IntHasFlag(int number, int flag)
    {

        if ((number & (int)flag) != 0) return true;
        return false;
    }
    bool IsPowerOfTwo(ulong x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }


    public static bool IsPlayerNearWater(float range)
    {
        if (IsInNamedArea(Game.Player.Character, "oceana")) return true;
        if (GenerateSpawnPos(Game.Player.Character.Position, Nodetype.Water, false).DistanceTo(Game.Player.Character.Position) < range) return true;

        return false;
    }
    public enum Nodetype { AnyRoad, Road, Offroad, Water }
    public static Vector3 GenerateSpawnPos(Vector3 desiredPos, Nodetype roadtype, bool sidewalk)
    {

        Vector3 finalpos = Vector3.Zero;
        bool ForceOffroad = false;


        OutputArgument outArgA = new OutputArgument();
        int NodeNumber = 1;
        int type = 0;

        if (roadtype == Nodetype.AnyRoad) type = 1;
        if (roadtype == Nodetype.Road) type = 0;
        if (roadtype == Nodetype.Offroad) { type = 1; ForceOffroad = true; }
        if (roadtype == Nodetype.Water) type = 3;


        int NodeID = Function.Call<int>(Hash.GET_NTH_CLOSEST_VEHICLE_NODE_ID, desiredPos.X, desiredPos.Y, desiredPos.Z, NodeNumber, type, 300f, 300f);

        if (ForceOffroad)
        {
            for (int i = 0; i < 100; i++)
            {
                if (!Function.Call<bool>(Hash._GET_IS_SLOW_ROAD_FLAG, NodeID))
                {
                    NodeID = Function.Call<int>(Hash.GET_NTH_CLOSEST_VEHICLE_NODE_ID, desiredPos.X, desiredPos.Y, desiredPos.Z, NodeNumber, type, 300f, 300f);
                    NodeNumber++;
                }
            }
            /*
            while (!Function.Call<bool>(Hash._GET_IS_SLOW_ROAD_FLAG, NodeID) && NodeNumber < 500)
            {
                Script.Wait(1);
                NodeNumber++;
                Vector3 v = desiredPos.Around(NodeNumber);
                NodeID = Function.Call<int>(Hash.GET_NTH_CLOSEST_VEHICLE_NODE_ID, v.X, v.Y, v.Z, NodeNumber, type, 300f, 300f);
            }
        }*/

            //UI.Notify("Final:" +NodeNumber.ToString());
        }

        Function.Call(Hash.GET_VEHICLE_NODE_POSITION, NodeID, outArgA);
        finalpos = outArgA.GetResult<Vector3>();

        if (sidewalk) finalpos = World.GetNextPositionOnSidewalk(finalpos);
        return finalpos;

    }
    public static string GetMapAreaAtCoords(Vector3 pos)
    {
        int MapArea;
        MapArea = Function.Call<int>(Hash.GET_HASH_OF_MAP_AREA_AT_COORDS, pos.X, pos.Y, pos.Z);
        if (MapArea == Game.GenerateHash("city")) return "city";
        if (MapArea == Game.GenerateHash("countryside")) return "countryside";
        return MapArea.ToString();
    }
    void StripOfAllPossible(Vehicle v, bool tyres, bool doors, bool hood, bool boot, bool forceBurned, bool smashWindows)
    {
        if (!CanWeUse(v)) return;


        /*
        VehicleColor NewVehicleC = VehicleColor.MetallicWhite;
        VehicleColor VehicleC = v.PrimaryColor;
        List<string> Modifiers = new List<string> { "Metallic", };

        string ToLookFor = v.PrimaryColor.ToString();      
        foreach (string mod in Modifiers)
        {

            if (VehicleC.ToString().ToLowerInvariant().Contains(color.ToLowerInvariant()))
            {
                ToLookFor = color.ToLowerInvariant();
                break;
            }
        }
        
        foreach (VehicleColor color in Enum.GetValues(typeof(VehicleColor)))
        {
            if (((color.ToString().ToLowerInvariant().Contains("matte") || color.ToString().ToLowerInvariant().Contains("worn")) && color.ToString().ToLowerInvariant().Contains(ToLookFor)))
            {
                NewVehicleC = color;
                UI.Notify("Found color: " + NewVehicleC);

                v.PrimaryColor = NewVehicleC;
                break;
            }
        }
        */

        v.IsDriveable = false;
        if (tyres)
        {
            for (int i = 0; i < 10; i++)
            {
                Function.Call(Hash.SET_VEHICLE_TYRE_BURST, v, i, true, 1000);
            }
        }
        if (smashWindows)
        {
            v.SmashWindow(VehicleWindow.FrontLeftWindow);
            v.SmashWindow(VehicleWindow.FrontRightWindow);
            v.SmashWindow(VehicleWindow.BackLeftWindow);
            v.SmashWindow(VehicleWindow.BackRightWindow);
        }
        if (forceBurned)
        {
            Function.Call(Hash.SET_ENTITY_RENDER_SCORCHED, v, true);
        }

        for (int i = -2; i < 10; i++)
        {
            if (i == 4 && !boot) continue;
            if (i == 5 && !hood) continue;
            if (!doors) continue;
            Function.Call(Hash.SET_VEHICLE_DOOR_BROKEN, v, i, true); //if (i != 1) 
        }

    }

    public static List<String> MessageQueue = new List<String>();
    public static int MessageQueueInterval = 8000;
    public static int MessageQueueReferenceTime = 0;
    public static void HandleMessages()
    {
        if (MessageQueue.Count > 0)
        {
            DisplayHelpTextThisFrame(MessageQueue[0]);
        }
        else
        {
            MessageQueueReferenceTime = Game.GameTime;
        }

        if (Game.GameTime > MessageQueueReferenceTime + MessageQueueInterval)
        {
            if (MessageQueue.Count > 0)
            {
                MessageQueue.RemoveAt(0);
            }
            MessageQueueReferenceTime = Game.GameTime;
        }
    }
    public static void AddQueuedHelpText(string text)
    {
        if (!MessageQueue.Contains(text)) MessageQueue.Add(text);
    }

    public static void ClearAllHelpText(string text)
    {
        MessageQueue.Clear();
    }


    public static List<String> ConversationQueueText = new List<String>();
    public static int ConversationQueueInterval = 8000;
    public static int ConversationQueueReferenceTime = 0;
    public static void HandleConversation()
    {
        if (ConversationQueueText.Count > 0 && Game.GameTime > ConversationQueueReferenceTime)
        {
            int Moretime = ((int)(ConversationQueueText[0].Length * 0.1f) * 1000);
            if (Moretime > 8000) Moretime = 8000;
            ConversationQueueReferenceTime = Game.GameTime + Moretime;
            UI.ShowSubtitle(ConversationQueueText[0], Moretime);
            ConversationQueueText.RemoveAt(0);
        }
    }
    public static void AddQueuedConversation(string text)
    {
        if (!ConversationQueueText.Contains(text)) ConversationQueueText.Add(text);
    }

    public static List<String> NotificationQueueText = new List<String>();
    public static List<String> NotificationQueueAvatar = new List<String>();
    public static List<String> NotificationQueueAuthor = new List<String>();
    public static List<String> NotificationQueueTitle = new List<String>();

    public static int NotificationQueueInterval = 8000;
    public static int NotificationQueueReferenceTime = 0;
    public static void HandleNotifications()
    {
        if (Game.GameTime > NotificationQueueReferenceTime)
        {
            if (NotificationQueueAvatar.Count > 0 && NotificationQueueText.Count > 0 && NotificationQueueAuthor.Count > 0 && NotificationQueueTitle.Count > 0)
            {
                int Moretime = ((int)(NotificationQueueText[0].Length * 0.1f) * 1000);
                if (Moretime > 5000) Moretime = 5000;
                NotificationQueueReferenceTime = Game.GameTime + Moretime;
                Notify(NotificationQueueAvatar[0], NotificationQueueAuthor[0], NotificationQueueTitle[0], NotificationQueueText[0]);
                NotificationQueueText.RemoveAt(0);
                NotificationQueueAvatar.RemoveAt(0);
                NotificationQueueAuthor.RemoveAt(0);
                NotificationQueueTitle.RemoveAt(0);
            }
        }
    }

    public static void AddNotification(string avatar, string author, string title, string text)
    {
        NotificationQueueText.Add(text);
        NotificationQueueAvatar.Add(avatar);
        NotificationQueueAuthor.Add(author);
        NotificationQueueTitle.Add(title);
    }
    public static void CleanNotifications()
    {
        NotificationQueueText.Clear();
        NotificationQueueAvatar.Clear();
        NotificationQueueAuthor.Clear();
        NotificationQueueTitle.Clear();
        NotificationQueueReferenceTime = Game.GameTime;
        Function.Call(Hash._REMOVE_NOTIFICATION, CurrentNotification);
    }

    public static int CurrentNotification;
    public static void Notify(string avatar, string author, string title, string message)
    {
        if (avatar != "" && author != "" && title != "")
        {
            Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, message);
            CurrentNotification = Function.Call<int>(Hash._SET_NOTIFICATION_MESSAGE, avatar, avatar, true, 0, title, author);
        }
        else
        {
            UI.Notify(message);
        }
    }
    public static void DisplayHelpTextThisFrame(string text)
    {
        Function.Call(Hash._SET_TEXT_COMPONENT_FORMAT, "STRING");
        Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
        Function.Call(Hash._0x238FFE5C7B0498A6, 0, 0, 1, -1);
    }
    protected override void Dispose(bool dispose)
    {
        foreach (DrugDeal d in DrugDeals) d.Clear();

        if (CanWeUse(Carjacker)) Carjacker.MarkAsNoLongerNeeded();
        if (CanWeUse(Overtaker)) Overtaker.MarkAsNoLongerNeeded();

        if (CanWeUse(CarjackerTarget) && CarjackerTarget.IsPersistent) CarjackerTarget.MarkAsNoLongerNeeded();
        foreach (TaxiEvent t in Taxis) t.Clear();
        foreach (Hunter h in Hunters) h.Clear();
        foreach (TrafficSpawner tr in TrafficSpawnerList) tr.Clear();

        foreach (Entity e in TemporalPersistence)
        {
            e.IsPersistent = false;
        }

        foreach (Vehicle v in Racers)
        {
            if (CanWeUse(v)) v.IsPersistent = false;

            Ped p = v.Driver;
            if (!CanWeUse(p)) p = Function.Call<Ped>(Hash.GET_LAST_PED_IN_VEHICLE_SEAT, v, -1);
            if (CanWeUse(p)) p.IsPersistent = false;
        }
        base.Dispose(dispose);

        foreach (Rope r in TrailerRopes) if (r.Exists()) r.Delete();
    }


    Vehicle GetAttachedVehicle(Vehicle carrier, bool DetachIfFound)
    {
        Vehicle Carried = null;
        Vehicle OriginalCarrier = carrier;


        if (carrier.Model.IsCar && carrier.HasBone("attach_female") && carrier.Model != "yosemitexl" && carrier.Model != "ramptruck")
        {
            //ui.notify("Carrier " + carrier.FriendlyName + " has an 'attach_female' bone, looking for trailers");

            if (Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, carrier))
            {
                //ui.notify("This carrier has a trailer, getting trailer");
                Vehicle trailer = null; // GetTrailer(ToCarry);
                if (trailer == null)
                {
                    foreach (Vehicle t in World.GetNearbyVehicles(carrier.Position, 30f))
                        if (t.HasBone("attach_male"))
                        {
                            trailer = t;
                            break;
                        }
                }

                if (trailer != null)
                {
                    carrier = trailer;
                    //ui.notify("Trailer found, " + carrier.FriendlyName + "(" + carrier.DisplayName + ")");

                    foreach (Vehicle veh in World.GetNearbyVehicles(carrier.Position, 10f))
                        if (veh != OriginalCarrier && veh != carrier && veh.IsAttachedTo(carrier))
                        {
                            if (DetachIfFound) Detach(carrier, veh);
                            return veh;
                            Carried = veh;
                            //ui.notify("Found ToCarry, " + ToCarry.FriendlyName);
                            break;
                        }
                }
                else
                {
                    //ui.notify("Trailer not found, aborting");
                    return null;
                }
            }
            else
            {
                //ui.notify("This carrier doesn't have trailer, aborting");
                return null;
            }

        }
        else
        {
            //ui.notify("Carrier " + carrier.FriendlyName + " does ~o~NOT~w~ have an 'attach_female' bone, must be a normal car");
            foreach (Vehicle v in World.GetNearbyVehicles(carrier.Position, carrier.Model.GetDimensions().Y))
            {
                if (v.IsAttachedTo(carrier))
                {

                    if (DetachIfFound) Detach(carrier, v);

                    return v;
                }
            }
        }
        return null;
    }

    public static void Detach(Vehicle carrier, Vehicle cargo)
    {
        cargo.Detach();
        if (carrier == Game.Player.Character.CurrentVehicle) UI.Notify("Detaching " + cargo.FriendlyName + " from " + carrier.FriendlyName);

        if (CanWeUse(Game.Player.Character.CurrentVehicle) && carrier == Game.Player.Character.CurrentVehicle)
            if (Game.IsControlPressed(2, GTA.Control.ParachuteTurnLeftOnly))
            {
                //ui.notify("~o~Left");

                cargo.Position = carrier.Position - (carrier.RightVector * carrier.Model.GetDimensions().X);
            }
        if (Game.IsControlPressed(2, GTA.Control.ParachuteTurnRightOnly))
        {
            //ui.notify("~o~Right");
            cargo.Position = carrier.Position + (carrier.RightVector * carrier.Model.GetDimensions().X);
        }
        if (Game.IsControlPressed(2, GTA.Control.ParachutePitchDownOnly))
        {
            //ui.notify("~o~Back");

            cargo.Position = carrier.Position + -(carrier.ForwardVector * carrier.Model.GetDimensions().Y);
            // ToCarry.Position = carrier.Position + (carrier.RightVector * carrier.Model.GetDimensions().X);
        }
    }



    public static void Attach(Vehicle carrier, Vehicle ToCarry)
    {
        //Vehicle ToCarry = null; // Game.Player.Character.CurrentVehicle;
        if (!CanWeUse(carrier)) return;
        Vehicle OriginalCarrier = carrier;

        if (!CanWeUse(ToCarry))
        {
            if (carrier.Model.IsCar && carrier.HasBone("attach_female") && carrier.Model != "yosemitexl" && carrier.Model != "ramptruck")
            {
                //ui.notify("Carrier " + carrier.FriendlyName + " has an 'attach_female' bone, looking for trailers");

                if (Function.Call<bool>(Hash.IS_VEHICLE_ATTACHED_TO_TRAILER, carrier))
                {
                    //ui.notify("This carrier has a trailer, getting trailer");
                    Vehicle trailer = null; // GetTrailer(ToCarry);
                    if (trailer == null)
                    {
                        foreach (Vehicle t in World.GetNearbyVehicles(carrier.Position, 30f))
                            if (t.HasBone("attach_male"))
                            {
                                trailer = t;
                                break;
                            }
                    }

                    if (trailer != null)
                    {
                        carrier = trailer;
                        //ui.notify("Trailer found, " + carrier.FriendlyName + "(" + carrier.DisplayName + ")");

                        foreach (Vehicle veh in World.GetNearbyVehicles(carrier.Position, 10f))
                            if (veh != OriginalCarrier && veh != carrier)
                            {
                                ToCarry = veh;
                                //ui.notify("Found ToCarry, " + ToCarry.FriendlyName);
                                break;
                            }
                    }
                    else
                    {
                        //ui.notify("Trailer not found, aborting");
                        return;
                    }
                }
                else
                {
                    //ui.notify("This carrier doesn't have trailer, aborting");
                    return;
                }

            }
            else
            {
                //ui.notify("Carrier " + carrier.FriendlyName + " does ~o~NOT~w~ have an 'attach_female' bone, must be a normal car");
                foreach (Vehicle v in World.GetNearbyVehicles(carrier.Position, carrier.Model.GetDimensions().Y))
                {
                    if (v.IsAttachedTo(carrier))
                    {

                        Detach(carrier, v);

                        //ui.notify("~o~ToCarry already attached, aborting");

                        return;
                    }
                }

                if (carrier.Model.IsHelicopter)
                {
                    foreach (Vehicle veh in World.GetNearbyVehicles(carrier.Position, 30f))
                        if (veh != OriginalCarrier && veh != carrier)
                        {
                            ToCarry = veh;
                            //ui.notify("Found ToCarry, " + ToCarry.FriendlyName);
                            break;
                        }
                }
                else
                {
                    Vector3 back = -(carrier.ForwardVector * carrier.Model.GetDimensions().Y);

                    if (carrier.Model.IsHelicopter) back = -(carrier.UpVector * 30);
                    RaycastResult ray = World.Raycast(carrier.Position, back, 30f, IntersectOptions.Everything, carrier);


                    if (!ray.DitHitEntity) ray = World.Raycast(carrier.Position - carrier.UpVector, back, 30f, IntersectOptions.Everything, carrier);

                    if (ray.DitHitEntity && ray.HitEntity.Model.IsVehicle)
                    {
                        ToCarry = ray.HitEntity as Vehicle;
                        //ui.notify("Carrier: " + carrier.FriendlyName);
                        //ui.notify("ToCarry: " + ToCarry.FriendlyName);

                    }
                    else
                    {
                        //ui.notify("No vehicle found behind yours.");
                        return;
                    }
                }
            }
        }


        if (!CanWeUse(ToCarry))
        {
            //ui.notify("ToCarry not found, aborting");
            return;
        }

        if (OriginalCarrier == Game.Player.Character.CurrentVehicle) UI.Notify("Attaching " + ToCarry.FriendlyName + " to" + OriginalCarrier.FriendlyName);
        if (ToCarry.IsAttached())
        {

            return;
        }

        Vector3 CarrierOffset = new Vector3(0, -(carrier.Model.GetDimensions().Y / 2f), 0f);// new Vector3(0, -1.4f, 3f + (ToCarry.Model.GetDimensions().Z * 0.35f));
        Vector3 truckoffset = new Vector3(0f, (ToCarry.Model.GetDimensions().Y / 2f), -ToCarry.HeightAboveGround);
        if (!ToCarry.IsOnAllWheels)
        {
            truckoffset = new Vector3(0f, (ToCarry.Model.GetDimensions().Y / 2f), -(ToCarry.Model.GetDimensions().Z * 0.4f));
        }

        float pitch = 0f;
        bool NotMadeToCarry = true;
        if (carrier.Model == "mule5")
        {
            NotMadeToCarry = false;
            CarrierOffset = new Vector3(0, 1.5f, -0.05f);
        }
        if (carrier.Model == "flatbed")
        {
            NotMadeToCarry = false;
            float farback = 0f;

            CarrierOffset = new Vector3(0, 0.5f + farback, 0.4f); // v.Model.GetDimensions().Z * 0.5f  //
        }

        if (carrier.Model == "barracks4" || carrier.Model == "sturdy2")
        {
            NotMadeToCarry = false;

            CarrierOffset = new Vector3(0, 0.9f, 0.88f); // v.Model.GetDimensions().Z * 0.5f  //
        }
        if (carrier.Model == "ramptruck2" || carrier.Model == "ramptruck")
        {
            NotMadeToCarry = false;
            CarrierOffset = new Vector3(0, -1f, 1f); // v.Model.GetDimensions().Z * 0.5f
            pitch = 5;
        }
        if (carrier.Model == "wastelander")
        {
            NotMadeToCarry = false;
            CarrierOffset = new Vector3(0, 1.5f, 1f); // v.Model.GetDimensions().Z * 0.5f
                                                      //pitch = 5;
        }
        if (carrier.Model == "SKYLIFT")
        {
            NotMadeToCarry = false;
            CarrierOffset = new Vector3(0, -2f, -(ToCarry.Model.GetDimensions().Z / 2) + 0.5f); // v.Model.GetDimensions().Z * 0.5f
            truckoffset = new Vector3(0f, 0f, (ToCarry.Model.GetDimensions().Z * 0.4f));
        }
        //ui.notify("Calculated offsets");
        //ui.notify("Is NOT normal vehicle, attaching");


        bool Collision = true;
        if (carrier.Model == "wastelander")
        {
            Collision = false;
            NotMadeToCarry = false;
        }
        if (carrier.Model == "freighttrailer")
        {
            NotMadeToCarry = false;
            CarrierOffset = new Vector3(0, 8f, -1.2f);
        }
        if (carrier.Model == "trflat")
        {
            NotMadeToCarry = false;
            CarrierOffset = new Vector3(0, 3, 0.5f);
        }
        if (carrier.Model == "armytrailer")
        {
            CarrierOffset = new Vector3(0, 0, -1.2f);
            NotMadeToCarry = false;
        }
        if (carrier.Model == "cartrailer" || carrier.Model == "cartrailer2")
        {
            NotMadeToCarry = false;
            CarrierOffset = new Vector3(2.3f, -2.5f, -0.4f);
        }


        if (NotMadeToCarry) truckoffset = truckoffset = new Vector3(0f, (ToCarry.Model.GetDimensions().Y / 2f), 0f);
        Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, ToCarry, carrier, 0, 0, CarrierOffset.X, CarrierOffset.Y, CarrierOffset.Z, truckoffset.X, truckoffset.Y, truckoffset.Z, pitch, 0f, 0f, 5000f, true, true, Collision, false, 2); //+ (v.Model.GetDimensions().Y/2f)
        ToCarry.Velocity = carrier.Velocity;
    }


    void AttachPhysically(Entity e, Entity carrier, Vector3 eOffset, Vector3 carrierOffset,  Vector3 eRotation, int eBone, int carrierBone, float force, bool Fixedrot, bool collision)
    {
        Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, e, carrier, 0, 0, carrierOffset.X, carrierOffset.Y, carrierOffset.Z, eOffset.X, eOffset.Y, eOffset.Z, eRotation.X, eRotation.Y, eRotation.Z, force, Fixedrot, true, collision, false, 2); //+ (v.Model.GetDimensions().Y/2f)
        e.Velocity = carrier.Velocity;
    }
}