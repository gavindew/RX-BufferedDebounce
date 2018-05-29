using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BufferedDebounce
{
    public static class BufferedDebounceExtension
    {
        /// <summary>
        /// BufferedDebounce collects events into a buffer, which is emitted on the first of the debounce timeout occurring, or the maximum time expiring.
        /// </summary>
        /// <typeparam name="T">Type of element to buffer</typeparam>
        /// <param name="observable">The observable sequence to buffer and debounce</param>
        /// <param name="debounceTimeout">The debounce time period</param>
        /// <param name="maximumTimeBeforeBufferEmitted">The maximum time a sequence may be debounced for before the buffer is emitted anyway</param>
        /// <returns></returns>
        public static IObservable<IList<T>> BufferedDebounce<T>(
            this IObservable<T> observable,
            TimeSpan debounceTimeout,
            TimeSpan maximumTimeBeforeBufferEmitted)
        {
            var lastBufferCreationTime = DateTime.Now;
            var bufferNumber = 0;
            var closeObservable = new Subject<bool>();

            // Will only emit after no event has been received for the debounceTimeInSeconds
            var throttleObservable = observable
                .Throttle(debounceTimeout)
#if DEBUG
                .Do(x => Console.WriteLine($"Debounce time expired for buffer #{bufferNumber}"))
#endif
                .Select(x => bufferNumber);

            // Here, we create an observable sequence which only emits until a throttle event has occurred. So, once a
            // throttle occurs, this sequence is cancelled, but we repeat the sequence immediately thereafter
            // so the effect is that a timeout event will only occur if a throttle event has not occurred
            var timeoutObservable = Observable.Generate(
                    0,                                      // Initial state - Not used
                    (state) => true,                        // Always generate
                    (state) => { return 1; },               // Add 1
                    (state) => { return bufferNumber; },    // Return the buffer number as the result
                    (state) => { return lastBufferCreationTime.Add(maximumTimeBeforeBufferEmitted); })   // Schedule to fire at the maximum time since buffer was created
                .TimeInterval()
#if DEBUG
                .Do(x =>
                {
                    Console.WriteLine($"Maximum time for buffer #{x.Value} has occurred after {x.Interval.TotalSeconds} seconds");
                })
#endif
                .TakeUntil(closeObservable)
                .Select(x => bufferNumber)
                .Repeat();

            // The buffer close observable will fire when either the throttle event or timeout event occurs
            // Whenever an event occurs, it resets to last event time, which the timeout observable sequence will use to
            // determine the next timeout
            // It also emits a value on the close observable, to cancel which observable did not fire
            // in order to close the buffer
            var bufferCloseObservable = throttleObservable
                    .Merge(timeoutObservable)
                    .Do(x =>
                    {
                        // Emit a value to the closeObservable, which will terminate the
                        // Throttle and the timeout, so that they do not fire when the new buffer is 
                        // already created
                        closeObservable.OnNext(true);
                        lastBufferCreationTime = DateTime.Now;
                    });

            // Buffer all incoming events until we get a buffer close event
            var debounceObservable = observable
                .Buffer(bufferCloseObservable)
#if DEBUG
                .Do(x => Console.WriteLine($"Emitting Buffer #{bufferNumber} {string.Join(",", x)}, Opening buffer #{++bufferNumber}"))
#endif
                ;

            return debounceObservable;
        }
    }
}
