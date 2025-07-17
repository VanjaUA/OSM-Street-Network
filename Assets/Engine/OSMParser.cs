#region 

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

#endregion

namespace OSMStreetNetwork
{
    public class OSMParser
    {
        [System.Serializable]
        public class OSMNode
        {
            public long Id;
            public double Lat;
            public double Lon;
        }

        [System.Serializable]
        public class OSMWay
        {
            public long Id;
            public List<long> NodeRefs = new List<long>();
            public Dictionary<string, string> Tags = new Dictionary<string, string>();
            public List<string> AllTags = new();
        }

        private string _osmDataFilename;

        public Dictionary<long, OSMNode> Nodes = new Dictionary<long, OSMNode>();
        public List<OSMWay> RoadWays = new List<OSMWay>();
        public List<OSMWay> FootwayWays = new List<OSMWay>();

        public double OriginLat = 47.3752484;
        public double OriginLon = 8.513546;
        public float MapScale = 100000f;

        public OSMParser(string OSMDataFilename)
        {
            _osmDataFilename = OSMDataFilename;
            LoadOSMData();
        }

        public void LoadOSMData()
        {
            string rawJson = Resources.Load<TextAsset>(_osmDataFilename).text;
            JObject root = JObject.Parse(rawJson);

            // Parse elements array
            foreach (var element in root["elements"])
            {
                string type = (string)element["type"];
                if (type == "node")
                {
                    var node = new OSMNode
                    {
                        Id = (long)element["id"],
                        Lat = (double)element["lat"],
                        Lon = (double)element["lon"]
                    };
                    Nodes[node.Id] = node;
                }
                else if (type == "way")
                {
                    var way = new OSMWay
                    {
                        Id = (long)element["id"]
                    };

                    // Node references
                    foreach (var nd in element["nodes"])
                    {
                        way.NodeRefs.Add((long)nd);
                    }

                    // Tags
                    var tags = element["tags"];
                    if (tags != null)
                    {
                        foreach (var tag in tags)
                        {
                            way.Tags[(string)((JProperty)tag).Name] = (string)((JProperty)tag).Value;
                            way.AllTags.Add((string)((JProperty)tag).Name + "    " + (string)((JProperty)tag).Value);
                        }
                    }

                    // Filter for highways only
                    if (way.Tags.ContainsKey("highway"))
                    {
                        if (way.Tags["highway"] == "residential")
                        {
                            RoadWays.Add(way);
                            continue;
                        }
                        if (way.Tags["highway"] == "tertiary")
                        {
                            RoadWays.Add(way);
                            continue;
                        }
                        if (way.Tags["highway"] == "footway" && way.Tags.ContainsKey("footway")
                            && way.Tags.Count > 1) //To check if proper footway
                        {
                            FootwayWays.Add(way);
                            continue;
                        }
                    }
                }
            }

            Debug.Log($"Loaded {Nodes.Count} nodes and {RoadWays.Count} highway ways.");
        }

        public Vector3 LatLonToUnity(double lat, double lon)
        {
            float x = (float)((lon - OriginLon) * MapScale * Mathf.Cos((float)OriginLat * Mathf.Deg2Rad));
            float z = (float)((lat - OriginLat) * MapScale);
            return new Vector3(x, 0, z);
        }

    }
}