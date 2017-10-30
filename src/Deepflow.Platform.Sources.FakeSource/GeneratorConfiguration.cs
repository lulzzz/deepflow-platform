namespace Deepflow.Platform.Sources.FakeSource
{
    public class GeneratorConfiguration
    {
        public GeneratorPlugin? GeneratorPlugin { get; set; }
        public int SecondsInterval { get; set; }
    }

    public enum GeneratorPlugin
    {
        Deterministic,
        Random
    }
}
