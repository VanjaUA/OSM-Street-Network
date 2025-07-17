# OSM-Street-Network

To start the project, go to Assets/Scenes/SampleScene.unity and press play
Main script: Assets/Engine/Starter.cs

To change map load JSON file from https://overpass-turbo.eu/#


Example to overpass turbo code:
  [out:json][timeout:25];
  (
    way["highway"](47.3735,8.5120,47.3756,8.5155);
  );
  (._;>;);
  out body;

  
Put that JSON file to Assets/Resources and write fileName as a param to OSMParser constructor
Example:  _osmParser = new OSMParser("osm_zurich"); // put "osm_zurich2" for bigger map
