# Snappy for .NET #

Snappy.NET is a .NET wrapper for [Snappy compression algorithm](https://google.github.io/snappy/).
It includes raw block compression as well as an implementation of Snappy framing format.

* [Website](https://snappy.machinezoo.com/)
* [Tutorial for .NET](https://snappy.machinezoo.com/#net)
* [Source code (main repository)](https://bitbucket.org/robertvazan/snappy.net/src/default/)
* [BSD license](https://opensource.org/licenses/BSD-3-Clause)
* [Upstream Snappy](https://google.github.io/snappy/)

***

```csharp
using (var file = File.OpenWrite("mydata.sz"))
using (var compressor = new SnappyStream(file, CompressionMode.Compress))
using (var writer = new StreamWriter(compressor))
    writer.WriteLine("Hello World!");
```

