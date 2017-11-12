using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Deepflow.Ingestion.Service.Metrics
{
    public interface IMetricsReporter
    {
        Task<T> Run<T>(string name, Func<Task<T>> task);
        Task Run(string name, Func<Task> task);
        Task Run(string name, Task task);
        Task Run(string name, IEnumerable<Task> tasks);
        Task<T> Run<T>(string name, Task<T> task);
    }
}