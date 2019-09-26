# Snappy for .NET #

Snappy.NET is a .NET wrapper for [Snappy compression algorithm](https://google.github.io/snappy/).
It includes raw block compression as well as an implementation of Snappy framing format.

* Documentation: [Home](https://snappy.machinezoo.com/), [Tutorial for .NET](https://snappy.machinezoo.com/#net)
* Download: see [Tutorial for .NET](https://snappy.machinezoo.com/#net)
* Sources: [GitHub](https://github.com/robertvazan/snappy.net), [Bitbucket](https://bitbucket.org/robertvazan/snappy.net)
* Issues: [GitHub](https://github.com/robertvazan/snappy.net/issues), [Bitbucket](https://bitbucket.org/robertvazan/snappy.net/issues)
* License: [BSD license](https://opensource.org/licenses/BSD-3-Clause)
* See also [upstream Snappy](https://google.github.io/snappy/)

***

```csharp
using (var file = File.OpenWrite("mydata.sz"))
using (var compressor = new SnappyStream(file, CompressionMode.Compress))
using (var writer = new StreamWriter(compressor))
    writer.WriteLine("Hello World!");
```

