using System;
using System.Collections.Generic;
using System.Timers;

namespace PlantSimulator
{

    class Plant
    {
        //VARIABLES - SETTINGS
        public MapSmall map;
        public string name { get; set; } = "";
        public string species { get; set; } = ""; //genus of the plant
        public int xLocation { get; set; } = 0;
        public int zLocation { get; set; } = 0;

        //VARIABLES - AGING AND GROWTH STAGE
        public float total_age { get; set; } = 0f; //total age
        public float cur_age { get; set; } = 0f; //current age of the plant 
        public float cur_growth { get; set; } = 0f;
        public int maxLifecycles { get; set; } = 2; //how many life cycles this plant can have, it's lifespan.
        public int cur_GrowthStage = 3; //3 for adult
        public int cur_PlantStage = 0;
        public int lifecycles { get; set; } = 0; //how many life cycles has occurred

        //VARIABLES - GROWTH HEIGHT AND WIDTH
        public float maxGrowth_Height { get; set; } = 0f; //max height in cm
        public float maxGrowth_Width { get; set; } = 0f; //min height in cm
        public float currentGrowth_height { get; set; } = 0f;
        public float currentGrowth_width { get; set; } = 0f;
        public float growthRate { get; set; } = 0f;

        //VARIABLES - LIFESTAGES
        public Dictionary<int, float> growthStage { get; set; }
        public Dictionary<int, float> plantStage { get; set; }

        //GROWING CONDITIONS
        public List<int> preferredSoilTypes {get; set;} //array of preferred soil types
        public List<int> soilMoistureRange { get; set; } //preferred max and min soil moisture %

        //GROWING TEMPERATURES
        public List<int> tempRange_germination { get; set; } //preferred max and min temperature C
        public List<int> tempRange_seedling { get; set; } //preferred max and min temperature C
        public List<int> tempRange_foliageGrowth { get; set; } //preferred max and min temperature C
        public List<int> tempRange_flowering { get; set; } //preferred max and min temperature C
        public List<int> tempRange_fruiting { get; set; } //preferred max and min temperature C
        public List<int> tempRange_ripening { get; set; } //preferred max and min temperature C
        public List<int> tempRange_dormancy { get; set; } //preferred max and min temperature C

        public Plant(MapSmall map,
            string name, 
            string species, 
            int xLocation,
            int zLocation,
            int maxLifecycles,
            int total_age,
            int cur_GrowthStage,
            int cur_PlantStage, 
            int lifecycles, //how many years/life cycles has this plant lived, how many times this plant has bloomed and gone dormant
            float maxGrowth_Height,
            float maxGrowth_Width,
            //growth values
            float growthLength_germinating, //how long to become growthStage 1 (germinating)
            float growthLength_seedling, //how long to become growthStage 2 (seedling)
            float growthLength_adult, //how long to become growthStage 3 (adult)
            //stage values
            float stageLength_foliage, //how long to become stage 1 (foliage growth)
            float stageLength_flowering, //how long to become stage 2 (flowering)
            float stageLength_fruiting, //how long to become stage 3 (fruiting)
            float stageLength_ripening, //how long to become stage 4 (ripening)
            float stageLength_dormant, //how long to become stage 5 (enter dormancy)
            //float stageLength_endDormancy, //how long dormancy lasts (6)
            float currentGrowth_height,
            float currentGrowth_width,
            float growthRate,
            List<int> preferredSoilTypes,
            List<int> soilMoistureRange,  //MIN, MAX
            List<int> tempRange_germination,
            List<int> tempRange_seedling,
            List<int> tempRange_foliageGrowth,
            List<int> tempRange_flowering,
            List<int> tempRange_fruiting,
            List<int> tempRange_ripening,
            List<int> tempRange_dormancy
            )
        {
            this.map = map;
            this.name = name;
            this.species = species;
            this.xLocation = xLocation;
            this.zLocation = zLocation;
            this.total_age = total_age;
            this.maxLifecycles = maxLifecycles;
            this.cur_GrowthStage = cur_GrowthStage;
            this.cur_PlantStage = cur_PlantStage;
            this.lifecycles = lifecycles;

            // Initialize growth stages
            growthStage = new Dictionary<int, float>
            {
                {0, 0f},                     // Seed
                {1, growthLength_germinating}, // Germinating
                {2, growthLength_seedling},    // Seedling
                {3, growthLength_adult}        // Adult
            };

            // Initialize plant stages
            plantStage = new Dictionary<int, float>
            {
                {0, stageLength_foliage},      // Foliage growth
                {1, stageLength_flowering},    // Flowering
                {2, stageLength_fruiting},     // Fruiting
                {3, stageLength_ripening},     // Ripening
                {4, stageLength_dormant},      // Dormant
            };

            // Set maximum dimensions
            this.maxGrowth_Height = maxGrowth_Height; //in cm
            this.maxGrowth_Width = maxGrowth_Width; //in cm
            this.currentGrowth_height = currentGrowth_height;
            this.currentGrowth_width = currentGrowth_width;
            this.growthRate = growthRate;

            //Set soil types
            this.preferredSoilTypes = preferredSoilTypes;
            this.soilMoistureRange = soilMoistureRange;  //MIN, MAX
            //Set temperatures
            this.tempRange_germination = tempRange_germination;
            this.tempRange_seedling = tempRange_seedling;
            this.tempRange_foliageGrowth = tempRange_foliageGrowth;
            this.tempRange_flowering = tempRange_flowering;
            this.tempRange_fruiting = tempRange_fruiting;
            this.tempRange_ripening = tempRange_ripening;
            this.tempRange_dormancy = tempRange_dormancy;
        }
        public void PlantEmulate(int time)
        {
            PlantGrowth(time);
            PlantGrow(time);
            //add plant to the map
            //PlantCheckSoil();
        }

        //PLANT AGE FUNCTIONS
        public void PlantGrowth(int time)
        {
            this.total_age += time; //always age up
            if (cur_GrowthStage > 0 && cur_GrowthStage < 3) //if plant is not a seed and not an adult
            {
                this.cur_age += time; //increment age days
                PlantCheckStage();//check stage
            }
            else if (cur_GrowthStage >= 3) //if adult, begin to go through stages of growth
            {
                this.cur_growth += time;
                PlantCheckStage();//check stage
            }
        }
        public void PlantCheckStage()
        {
            if (plantStage.ContainsKey(cur_PlantStage) && cur_growth >= plantStage[cur_PlantStage] && cur_GrowthStage >= 3) //only increment if adult
            {
                PlantStageUp();
            }
            if (growthStage.ContainsKey(cur_GrowthStage) && cur_age >= growthStage[cur_GrowthStage] && cur_GrowthStage < 3) //if current age is larger or equal to the growthStage number (meaning it has reached the end of this stage)
            {
                PlantAgeUp();
            }
        }
        public void PlantStageUp()
        {
            //increment the cur_plantStage
            cur_PlantStage += 1;
            cur_growth = 0; //reset counter
            if (cur_PlantStage >= 5 && cur_growth >= plantStage[5]) //if current stage age is greater than the oldest stage age for dormancy, reset
            {
                //reset growing and plantstage
                cur_PlantStage = 0;
                cur_growth = 0;
                lifecycles += 1; //age up the plant
            }
        }
        public void PlantAgeUp()
        {
            //increment the cur_plantStage
            if (cur_GrowthStage < 3) //check to stop growing if adult plant
            {
                cur_GrowthStage += 1;
                cur_age = 0; //reset growth days in this stage
            }
        }

        //PLANT GROWTH FUNCTIONS
        public void PlantGrow(int time)
        {
            //while foliage growth stage or not adult grow IF cur_PlantStage == 1 or 0
            //AND if currentGrowth_height != maxGrowth_Height
            //AND if currentGrowth_width != currentGrowth_width
            if ((cur_PlantStage == 1 || cur_PlantStage  == 0 ) && currentGrowth_height < maxGrowth_Height && currentGrowth_width < maxGrowth_Width)
            {
                //increase height and width
                currentGrowth_height += growthRate * time;
                currentGrowth_width += (growthRate * time)/2; //width increases slower
                //stop growing if max reached
            }
        }
        public void PlantCheckSoil()
        {
            //check map's soil type at plant's location
            int soilCheck_T = map.GetSoilType(xLocation, zLocation);
            Console.WriteLine("Soil type is " + soilCheck_T);
            foreach (var x in preferredSoilTypes)
            {
                if (x == soilCheck_T)
                {
                    break;
                }
                else
                {
                    growthRate = growthRate / 2; //growing this plant on soil that is not good for them reduces growing rate by HALF.
                }
            }
            //now check the map's soil moisture
            int soilCheck_M = map.GetSoilMoisture(xLocation, zLocation);
            float dev;
            if (soilCheck_M < this.soilMoistureRange[0]) // Less than min
            {
                dev = (float)(this.soilMoistureRange[0] - soilCheck_M) / (this.soilMoistureRange[1] - this.soilMoistureRange[0]);
            }
            else if (soilCheck_M > this.soilMoistureRange[1]) // Greater than max
            {
                dev = (float)(soilCheck_M - this.soilMoistureRange[1]) / (this.soilMoistureRange[1] - this.soilMoistureRange[0]);
            }
            else
            {
                dev = 0f; // Inside range
            }
            float penaltyFactor = 0.7f;
            float previousGrowthRate = growthRate; // Store the previous growth rate
            growthRate = growthRate * (1 - penaltyFactor * dev);
            Console.WriteLine($"Soil Moisture: {soilCheck_M}, Deviation: {dev}, Penalty Factor: {penaltyFactor}");
            Console.WriteLine($"Previous Growth Rate: {previousGrowthRate}, Adjusted Growth Rate: {growthRate}");
        }
    }
    class Program
    {
        static Timer clock;
        static int secondsElapsed = 0;
        static public int seconds;
        static Plant tomatoPlant;

        static void Main(string[] args)
        {
            clock = new Timer(1000); //5000ms is 1 second of the timer
            clock.Elapsed += OnTimerElapsed;
            clock.Start();
            MapSmall map1 = new MapSmall("C:/Users/ionam/Desktop/PlantSimulator/PlantSimulator/map2.txt", "north");
            map1.PrintMap();
            //PLANT TOMATO
            tomatoPlant = new Plant(
                map: map1,
                name: "Tomato",
                species: "Solanum",
                xLocation: 0,
                zLocation: 1,// 1 for testing sandy soil
                maxLifecycles: 1,
                total_age: 0,
                cur_GrowthStage: 1,//seedling for testing
                cur_PlantStage: 0,
                lifecycles: 0,
                maxGrowth_Height: 20.0f,
                maxGrowth_Width: 15.0f,
                growthLength_germinating: 7.5f, //1, takes 7.5 days to germinate into seedling
                growthLength_seedling: 100f, //2, takes 100 days to become an adult
                growthLength_adult: 0f, //3, now an adult!
                stageLength_foliage: 0f, //1, doesnt need 
                stageLength_flowering: 20f, //2, how long tomato flowers for
                stageLength_fruiting: 25f, //3, how long tomato fruits for
                stageLength_ripening: 18.5f, //4, how long it takes to ripen fruit
                stageLength_dormant: 63f, //5, dormant for 63 days
                currentGrowth_height: 0f,
                currentGrowth_width: 0f,
                growthRate: 1f,
                preferredSoilTypes: new List<int> { 0 },
                soilMoistureRange: new List<int> { 60, 80 }, //MIN, MAX
                tempRange_germination: new List<int> { 16, 22 },
                tempRange_seedling: new List<int> { 21, 27 },
                tempRange_foliageGrowth: new List<int> { 21, 27 },
                tempRange_flowering: new List<int> { 18, 24 },
                tempRange_fruiting: new List<int> { 18, 25 },
                tempRange_ripening: new List<int> { 20, 26 },
                tempRange_dormancy: new List<int> { 10, 15 }
            ) ;
            tomatoPlant.PlantCheckSoil();
            Console.WriteLine("The plant we see is the " + tomatoPlant.name + " and it's genus is : " + tomatoPlant.species);
            Console.WriteLine("Plant's current age (Seconds): " + tomatoPlant.cur_age);
            map1.AddOccupier(tomatoPlant.xLocation, tomatoPlant.zLocation, tomatoPlant);
            map1.PrintMap();

            Console.ReadLine();

            while (true)
            {
                Console.Write("Enter command: ");
                string cmd = Console.ReadLine();

                switch (cmd.ToLower())
                {
                    case "age":
                        Console.WriteLine($"Plant's current age (Seconds): {tomatoPlant.cur_age}");
                        break;
                    case "exit":
                        Console.WriteLine("Exiting simulation...");
                        clock.Stop(); // Stop the timer
                        return; // Exit the program
                    case "lifecycles":
                        Console.WriteLine($"Plant's lifecycles: {tomatoPlant.lifecycles}");
                        break;
                    case "growth rate":
                        Console.WriteLine($"Plant's growth rate: {tomatoPlant.growthRate}");
                        break;
                    case "size":
                        Console.WriteLine($"Plant's current height: {tomatoPlant.currentGrowth_height} and it's width: {tomatoPlant.currentGrowth_width}");
                        break;
                    default:
                        Console.WriteLine("Unknown command. Try 'age', 'size', 'growth rate', 'lifecycles' or 'exit'.");
                        break;
                }
            }
        }

        static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            secondsElapsed++;

            // Convert seconds to hours, minutes, and seconds
            int hours = secondsElapsed / 3600;
            int minutes = (secondsElapsed % 3600) / 60;
            seconds = secondsElapsed % 60;

            tomatoPlant.PlantEmulate(10); //increase aging by one second per each second

            //Console.Clear();
            Console.WriteLine($"Plant's total age (Seconds): {tomatoPlant.total_age}");
            Console.WriteLine($"Plant's current age (Seconds): {tomatoPlant.cur_age}");
            Console.WriteLine($"Plant's current growth age (Seconds): {tomatoPlant.cur_growth}");
            Console.WriteLine($"Plant's current growth stage: {tomatoPlant.cur_GrowthStage}");
            Console.WriteLine($"Plant's current adult stage: {tomatoPlant.cur_PlantStage}");
            Console.WriteLine($"Plant's current height (cm): {tomatoPlant.currentGrowth_height}");
            Console.WriteLine($"Plant's current width (cm): {tomatoPlant.currentGrowth_width}");
            //Console.WriteLine($"{hours:D2}:{minutes:D2}:{seconds:D2}"); // Formatted as HH:MM:SS
        }
    }
}
