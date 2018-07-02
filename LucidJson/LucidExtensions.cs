using LucidJson.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LucidJson
{
    public static class LucidExtensions
    {
        /// <summary>
        /// Continues an action on the thread the task was started on, no run on. 
        /// Useful with operations, where a task is originated on the UI thread and you
        /// want to 'ContinueOn' after the task is complete on that same UI thread.
        /// </summary>
        /// <param name="task">The task to continue on from.</param>
        /// <param name="action">The action to perform</param>
        public static void ContinueOn<T>(this Task<T> task, Action<Task<T>> action)
        {
            task.ContinueWith(action, TaskScheduler.FromCurrentSynchronizationContext()).ConfigureAwait(false);
        }

        /// <summary>
        /// Continues an action on the thread the task was started on, no run on. 
        /// Useful with operations, where a task is originated on the UI thread and you
        /// want to 'ContinueOn' after the task is complete on that same UI thread.
        /// </summary>
        /// <param name="task">The task to continue on from.</param>
        /// <param name="action">The action to perform</param>
        public static void ContinueOn(this Task task, Action<Task> action)
        {
            task.ContinueWith(action, TaskScheduler.FromCurrentSynchronizationContext()).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns an enumeration of sliding windows over a collection. A sliding window, is a window
        /// which has been sliding over a collection. Windows will have items in common.  
        /// 
        /// For example, sliding windows of size two over the collection [1,2,3,4,5] would return 4 windows:
        /// [1,2], [2,3], [3,4], [4,5]
        ///
        /// </summary>
        /// <typeparam name="T">The type of item to slide windows over</typeparam>
        /// <param name="items">The items to window</param>
        /// <param name="windowSize">The size of each window</param>
        /// <returns>An enumeration of windows of the specified size of the item collection provided</returns>
        public static IEnumerable<IEnumerable<T>> SlidingWindow<T>(this IEnumerable<T> items, int windowSize)
        {
            var window = new Queue<T>();
            int size = 0;

            foreach (var item in items)
            {
                size++;
                window.Enqueue(item);
                if (size == windowSize)
                {
                    yield return window.ToArray();
                    window.Dequeue();
                    size--;
                }
            }
        }

        /// <summary>
        /// Returns an enumeration of tumbling windows over a collection. A tumbling window, is a window
        /// which shares no elements with any other windows. 
        /// 
        /// For example, tumbling windows of size two over the collection [1,2,3,4,5] would return 3 windows:
        /// [1,2], [3,4], [5]
        ///
        /// </summary>
        /// <typeparam name="T">The type of item to tumble windows over</typeparam>
        /// <param name="items">The items to window</param>
        /// <param name="windowSize">The size of each window</param>
        /// <returns>An enumeration of windows of the specified size of the item collection provided</returns>
        public static IEnumerable<IEnumerable<T>> TumblingWindow<T>(this IEnumerable<T> items, int windowSize)
        {
            var index = 0;
            var window = new List<T>(windowSize);
            foreach (var item in items)
            {
                window.Add(item);
                index++;

                if (index % windowSize == 0)
                {
                    yield return window.ToArray();
                    window.Clear();
                }
            }

            if (window.Count > 0)
                yield return window;
        }

        /// <summary>
        /// Converts the provided item to a map. If a schema is provided, the schema is associated with the newly created map.
        /// </summary>
        /// <typeparam name="T">The type of object to convert</typeparam>
        /// <param name="obj">An instance of the type T to convert</param>
        /// <param name="schema">An optional schema to associate with the returned map</param>
        /// <returns>A map representing the provided item of type T</returns>
        public static Map AsMap<T>(this T obj, MapSchema schema = null)
        {
            return Map.ParseJson(JsonConvert.SerializeObject(obj, typeof(T), Map.SerializerSettings(schema)));
        }

        /// <summary>
        /// Converts a single item to an enumeration of the single item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object to convert to an enumeration</param>
        /// <returns>The enumeration</returns>
        public static IEnumerable<T> ItemAsEnumerable<T>(this T obj)
        {
            yield return obj;
        }

    }
}
