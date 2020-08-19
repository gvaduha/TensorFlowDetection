using Newtonsoft.Json;
using System;

namespace gvaduha.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NeuralNetConfiguration
    {
        [JsonIgnore]
        public byte[] Model { get; set; } //HACK: Make set private

        // Optional NN model file path
        [JsonProperty("mfile")]
        public string LocalModelFile { get; private set; }

        // Группировка KEY:classId -> VALUE:groupId (раньше из DETECTED_CLASS)
        [JsonProperty("dc")]
        public List<DetectedObjectTypeGrouping>  DetectorObjectClass { get; set; } //HACK: Make set private

        [JsonProperty("c")]
        public List<NeuralNetToDomainMappingClass> NnToDomainClassMapping { get; private set; }

        [JsonProperty("mp", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0.5d)]
        public double MinScore { get; private set; }

        [JsonProperty("mifu", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(80)]
        public int MinIntersectionForUnion { get; private set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ObjectDetectorConfig
    {
        [JsonProperty("nncfg")]
        public NeuralNetConfiguration NeuralNetConfiguration;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ObjectDetectorServiceSettings
    {
        [JsonProperty("detcycle")]
        public int DetectionCycleIntervalMilliseconds { get; set; }
        [JsonProperty("gpus")]
        public string[] ObjectDetectionDevices { get; set; }
        [JsonProperty("vsalive")]
        public TimeSpan VideoSourcesAliveCheckInterval { get; set; }
        [JsonProperty("vsshft")]
        public double VideoSourceShiftTolerance { get; set; }
        [JsonProperty("vsshftchk")]
        public TimeSpan VideoSourceShiftCheckInterval { get; set; }
        [JsonProperty("camtune")]
        public TimeSpan CamPictureTuningInterval { get; set; }
    }
}
