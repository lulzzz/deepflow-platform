namespace Deepflow.Platform.Series.Providers
{
    public interface IValueGenerator
    {
        double GenerateValueBetween(string name, long timeSeconds, long minSeconds, long maxSeconds, double minValue, double maxValue, double variance);
        double GenerateValue(string name, long timeSeconds, double minValue, double maxValue);
    }
}