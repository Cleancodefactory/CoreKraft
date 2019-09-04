using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Utilities.Generic.Topologies
{
    public static class IDependableDictExtensions
    {
        public static IDictionary<string, IDependable<T>> SortByDependencyOrderIndex<T>(this IDictionary<string, IDependable<T>> dict)
        {
            return dict.OrderBy(x => x.Value.DependencyOrderIndex).ToDictionary(x => x.Key, x => x.Value);
        }

        public static IDictionary<string, IDependable<T>> SortByDependencies<T>(this IDictionary<string, IDependable<T>> dict)
        {
            void Visit(IDependable<T> item, List<KeyValuePair<string, IDependable<T>>> sorted, Dictionary<string, bool> visited)
            {
                var alreadyVisited = visited.ContainsKey(item.Key) && visited[item.Key] == true;

                if (alreadyVisited)
                {
                    throw new ArgumentException($"Cyclic dependency found!");//Improve this!!!! 
                }
                else
                {
                    if (!visited.ContainsKey(item.Key)) visited.Add(item.Key, true);
                    else visited[item.Key] = true;

                    IDictionary<string, IDependable<T>> dependencies = item.Dependencies;

                    if (dependencies != null)
                    {
                        foreach (var dependency in dependencies)
                        {
                            Visit(dependency.Value, sorted, visited);
                        }
                    }

                    visited[item.Key] = false;
                    var newa = new KeyValuePair<string, IDependable<T>>(item.Key, item);
                    if (!sorted.Contains(newa)) sorted.Add(newa);
                }
            }

            var sorted_ = new List<KeyValuePair<string, IDependable<T>>>();
            var visited_ = new Dictionary<string, bool>();

            dict = dict.OrderBy(x => x.Value.Dependencies.Count()).ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in dict)
            {
                Visit(item.Value, sorted_, visited_);
            }

            return sorted_.ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
