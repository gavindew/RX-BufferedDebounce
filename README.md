# RX-BufferedDebounce
Debounced Buffer for Rx.Net

The bufferred debounce collects events and emits a list of the buffered events when either no events have been received for the debounceTimeout OR the maximumTimeBeforeBufferEmitted, whichever comes first.

The reason for the maximumTimeBeforeBufferEmitted is that continuously arriving events would prevent the debounce from ever emitting the buffer, so in this case, after the maximumTimeBeforeBufferEmitted
the buffer is emitted and the process starts again.

This project is used more as a learning aid, to play around with the buffered debounce and see how it buffers events and then emits them. It does not generate a nuget package for general consumption.

*Method Signature*
```C#
public static IObservable<IList<T>> BufferedDebounce<T>(
		this IObservable<T> observable, 
		TimeSpan debounceTimeout, 
		TimeSpan maximumTimeBeforeBufferEmitted)
```

*Example*
```C#
    observable
		.BufferedDebounce(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
		.Subscribe(x => { Console.WriteLine(string.Join(",", x)); });

```