// See https://aka.ms/new-console-template for more information

using YoloSerializer.Core.Models;

var targetAssembly = typeof(PlayerData).Assembly; // example

// read all types from the target assembly
// check if they implement IYoloSerializable
// if they do, generate a serializer for them using Scriban
// if they don't, skip them
