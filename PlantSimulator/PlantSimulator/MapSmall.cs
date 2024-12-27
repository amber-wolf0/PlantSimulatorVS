using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantSimulator
{
    class MapSmall
    {
        public struct Coordinate
        {
            int x { get; set; }
            int y { get; set; }

            public Coordinate(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public override string ToString()
            {
                return $"({x}, {y})";
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(x, y);
            }

            public override bool Equals(object obj)
            {
                if (obj is Coordinate coord)
                {
                    return x == coord.x && y == coord.y;
                }
                return false;
            }
        }
        public struct MapPoint
        {
            public int X { get; set; }
            public int Z { get; set; }
            public int YHeight { get; set; }
            public bool IsWater { get; set; }
            public int SoilType { get; set; }

            public int SoilMoisture { get; set; }
            public object Occupier { get; set; } //object that lives in this map point, e.g. a plant

            public MapPoint(int x, int z, int yHeight, bool isWater, int soilType, int soilMoisture, object occupier)
            {
                X = x;
                Z = z;
                YHeight = yHeight;
                IsWater = isWater;
                SoilType = soilType;
                SoilMoisture = soilMoisture;
                Occupier = occupier ?? "none";
            }

            public override string ToString()
            {
                return $"X: {X}, Z: {Z}, YHeight: {YHeight}, IsWater: {IsWater}, SoilType: {SoilType}, SoilMoisture: {SoilMoisture}, Occupier: {Occupier}";
            }
        }

        //public Dictionary<Coordinate, MapSmall> MapChunks { get; set; }  = new Dictionary<Coordinate, MapSmall>(); //store each map as a chunk
        public List<MapPoint> MapPoints { get; set; } = new List<MapPoint>(); //store each map's data
        public List<string> Lines { get; set; } = new List<string>(); //each map line within each map chunk
        public string hemisphere { get; set; } = "";

        public MapSmall (string filepath, string hemisphere)
        {
            this.hemisphere = hemisphere;
            //hemisphere = north, south, equator, north-equator, south-equator
                //if south, flip the seasonal temperatures
                //if equator, has only equator season
                //if north-equator, only uses winter and summer temperatures
                //if south-equator, flips north-equator temperatures

            try
            {
                //EACH MAP FILE:
                //x z yHeight isWater soilType
                //--!!!---WILL ADD BIOME LATER ---!!!---//
                //if yHeight < 0, it is underground/below sea level
                //if isWater, it is filled with water, 0 or 1
                //soilType (0=Neutral, 1=Wet, 2=Dry, 3=Sandy, 4=Clay, 5=Rocky, 6=Arid, 7=Polar)

                // Read all lines from the file
                var lines = File.ReadAllLines(filepath).Skip(1); //skip the first line
                foreach (var line in lines)
                {
                    Lines.Add(line); //store raw line
                    string[] datum = line.Split(' '); //data is space-separated
                    int x = int.Parse(datum[0]);
                    int z = int.Parse(datum[1]);
                    int yHeight = int.Parse(datum[2]);
                    bool isWater = datum[3] == "1";
                    int soilType = int.Parse(datum[4]);
                    int soilMoisture = int.Parse(datum[5]);

                    MapPoint point = new MapPoint(x, z, yHeight, isWater, soilType, soilMoisture, null);
                    //each map point is 1m^2

                    MapPoints.Add(point);
                }
                Console.WriteLine("Map processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading map file: {ex.Message}");
            }
        }

        public void PrintMap()
        {
            // Determine bounds for map dimensions
            int minX = int.MaxValue, maxX = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;

            foreach (var point in MapPoints)
            {
                if (point.X < minX) minX = point.X;
                if (point.X > maxX) maxX = point.X;
                if (point.Z < minZ) minZ = point.Z;
                if (point.Z > maxZ) maxZ = point.Z;
            }

            // Generate and print the map grid
            for (int z = maxZ; z >= minZ; z--) // Reverse Z-axis for top-down view
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var mapPoint = MapPoints.Find(p => p.X == x && p.Z == z);
                    string symbol = " ";  // Default to empty space

                    if (mapPoint.Occupier != null)
                    {
                        // Check if the map point is occupied
                        if (mapPoint.IsWater)
                        {
                            symbol = "~"; // Water
                        }
                        else if (mapPoint.Occupier != null && mapPoint.Occupier.ToString() != "none")
                        {
                            symbol = "O"; // Occupied by an object (Plant or other)
                        }
                        else
                        {
                            // Soil type symbol (if not occupied)
                            switch (mapPoint.SoilType)
                            {
                                case 0: symbol = "."; break; // Neutral
                                case 1: symbol = "*"; break; // Wet
                                case 2: symbol = "^"; break; // Dry
                                case 3: symbol = ":"; break; // Sandy
                                case 4: symbol = "&"; break; // Clay
                                case 5: symbol = "#"; break; // Rocky
                                case 6: symbol = "-"; break; // Arid
                                case 7: symbol = "+"; break; // Polar
                                default: symbol = "?"; break; // Unknown soil type
                            }
                        }
                    }

                    // Print the box containing the symbol
                    Console.Write("+---");
                }
                Console.WriteLine("+"); // End of the top border for this row

                // Print the row content
                for (int x = minX; x <= maxX; x++)
                {
                    var mapPoint = MapPoints.Find(p => p.X == x && p.Z == z);
                    string symbol = " ";  // Default to empty space

                    if (mapPoint.Occupier != null)
                    {
                        // Check if the map point is occupied
                        if (mapPoint.IsWater)
                        {
                            symbol = "~"; // Water
                        }
                        else if (mapPoint.Occupier != null && mapPoint.Occupier.ToString() != "none")
                        {
                            symbol = "O"; // Occupied by an object (Plant or other)
                        }
                        else
                        {
                            // Soil type symbol (if not occupied)
                            switch (mapPoint.SoilType)
                            {
                                case 0: symbol = "."; break; // Neutral
                                case 1: symbol = "*"; break; // Wet
                                case 2: symbol = "^"; break; // Dry
                                case 3: symbol = ":"; break; // Sandy
                                case 4: symbol = "&"; break; // Clay
                                case 5: symbol = "#"; break; // Rocky
                                case 6: symbol = "-"; break; // Arid
                                case 7: symbol = "+"; break; // Polar
                                default: symbol = "?"; break; // Unknown soil type
                            }
                        }
                    }

                    Console.Write($"| {symbol} ");
                }
                Console.WriteLine("|"); // End of the row content

                // Print the bottom border for this row
                for (int x = minX; x <= maxX; x++)
                {
                    Console.Write("+---");
                }
                Console.WriteLine("+");
            }
        }

        public void AddOccupier(int x, int z, object occupier)
        {
            int index = MapPoints.FindIndex(p => p.X == x && p.Z == z);

            // Check if the point was found
            if (index == -1)
            {
                Console.WriteLine("No map point found at the specified coordinates.");
            }
            else
            {
                // Modify the MapPoint by obtaining it by reference
                var mapPoint = MapPoints[index]; // This retrieves the value at the given index
                mapPoint.Occupier = occupier; // Modify the Occupier property
                MapPoints[index] = mapPoint; // Update the list with the modified MapPoint

                Console.WriteLine($"Occupier {occupier} added to MapPoint at ({x}, {z}).");
            }
        }

        public int GetSoilType(int x, int z)
        {
            int index = MapPoints.FindIndex(p => p.X == x && p.Z == z);

            // Check if the point was found
            if (index == -1)
            {
                Console.WriteLine("No map point found at the specified coordinates.");
                return 0;
            }
            else
            {
                return MapPoints[index].SoilType; //return the soil type of the map point
            }
        }
        public int GetSoilMoisture(int x, int z)
        {
            int index = MapPoints.FindIndex(p => p.X == x && p.Z == z);

            // Check if the point was found
            if (index == -1)
            {
                Console.WriteLine("No map point found at the specified coordinates.");
                return 0;
            }
            else
            {
                return MapPoints[index].SoilMoisture; //return the soil type of the map point
            }
        }
    }
}
