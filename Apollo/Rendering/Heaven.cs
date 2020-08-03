using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Apollo.Core;
using Apollo.Structures;

namespace Apollo.Rendering {
    public static class Heaven { 
        static ConcurrentQueue<List<Signal>> signalQueue = new ConcurrentQueue<List<Signal>>();

        static SortedDictionary<long, List<Action>> jobs = new SortedDictionary<long, List<Action>>();
        static ConcurrentQueue<(long, Action)> jobQueue = new ConcurrentQueue<(long, Action)>();

        static long lastRender = -1;
        static bool shouldRender = false;
        static long prev;

        static long MSToTicks(double ms) => (long)(ms / 1000 * Stopwatch.Frequency);

        public static void MIDIEnter(List<Signal> n) {
            signalQueue.Enqueue(n);
            Wake();
        }
        
        public static void Schedule(Action job, double time) {
            jobQueue.Enqueue((MSToTicks(time), job));
            Wake();
        }

        static Task RenderThread; 

        static void Wake() {
            if (RenderThread?.IsCompleted == false) return;

            RenderThread = Task.Run(() => {
                prev = Program.TimeSpent.ElapsedTicks - 1;
                
                while (jobQueue.Any() || jobs.Any() || signalQueue.Any() || shouldRender) {
                    while (jobQueue.TryDequeue(out (long Time, Action Job) task)) {
                        long target = prev + task.Time + 1;

                        if (!jobs.ContainsKey(target))
                            jobs[target] = new List<Action>();

                        jobs[target].Add(task.Job);
                    }

                    long last = prev;
                    prev = Program.TimeSpent.ElapsedTicks;

                    if (prev - last >= MSToTicks(10)) {
                        // TODO Heaven Lagspike occurred! Maybe indicate?
                    }

                    foreach (long i in jobs.Keys.TakeWhile(i => i <= prev).ToList()) {
                        foreach (Action job in jobs[i])
                            job.Invoke();
                        
                        jobs.Remove(i);
                    }

                    bool changed = false;

                    while (signalQueue.TryDequeue(out List<Signal> n))
                        n.ForEach(i => {
                            if (i.Source != null) {
                                i.Source.Render(i);
                                changed = true;
                            }
                        });

                    if (changed) shouldRender = true;

                    if (shouldRender && prev > lastRender + MSToTicks(33)) {  // TODO Heaven Read MAX FPS from Preferences
                        Screen.Draw();
                        lastRender = prev;
                        shouldRender = false;
                    }
                }
            });
        }
    }
}