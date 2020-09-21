<img src="https://raw.githubusercontent.com/SergiySeletsky/Unified/master/docs/logo.png" width="128" height="128" />

# **Unified Id** - the identity of your data.

[![Pull Requests Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat)](https://github.com/SergiySeletsky/Unified/compare)
[![Gated](https://github.com/SergiySeletsky/Unified/workflows/Gated/badge.svg)](https://github.com/SergiySeletsky/Unified/actions?query=workflow%3AGated)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Unified&metric=alert_status)](https://sonarcloud.io/dashboard?id=Unified)
[![NuGet](https://img.shields.io/nuget/v/Unified)](https://www.nuget.org/packages/Unified)

## Why

What is Unified Id? If GUID is too heavy for you but you need a random global Id that can be used as a string or long basic type, you are in right place.
What are the main advantages?
| Feature | Unified | GUID |
| ------ | ------ | ------ |
| Size | 8 byte (13 as string) | 16 byte (36 as string)
| Partitioning | Build-in | No, external can be added
| Collisions | 0.00000001% in 10B IDs | 50% in 2.7e18 IDs
| Cast | implicit(string, ulong, long) | explicit(byte[], Parse/ToString)
| Generate | Build-in(byte[], string, GUID) | No, only random NewGuid
| Null/Empty-handling | Friendly to Empty | Exception

## How it works

IdGen generates, like Snowflake, 64 bit Id's. The [Sign Bit](https://en.wikipedia.org/wiki/Sign_bit) is unused since this can cause incorrect ordering on some systems that cannot use unsigned types and/or make it hard to get correct ordering. So, in effect, IdGen generates 63 bit Id's. An Id consists of 3 parts:

* Timestamp
* Generator-id
* Sequence 

An Id generated with a **Default** `IdStructure` is structured as follows: 

![Id structure](https://raw.githubusercontent.com/RobThree/IdGen/master/IdGenDocumentation/Media/structure.png)

However, using the `IdStructure` class you can tune the structure of the created Id's to your own needs; you can use 45 bits for the timestamp, 2 bits for the generator-id and 16 bits for the sequence if you prefer. As long as all 3 parts (timestamp, generator and sequence) add up to 63 bits you're good to go!

The **timestamp**-part of the Id should speak for itself; by default this is incremented every millisecond and represents the number of milliseconds since a certain epoch. However, IdGen relies on an [`ITimeSource`](IdGen/ITimeSource.cs) which uses a 'tick' that can be defined to be anything; be it a millisecond (default), a second or even a day or nanosecond (hardware support etc. permitting). By default IdGen uses 2015-01-01 0:00:00Z as epoch, but you can specify a custom epoch too. 

The **generator-id**-part of the Id is the part that you 'configure'; it could correspond to a host, thread, datacenter or continent: it's up to you. However, the generator-id should be unique in the system: if you have several hosts or threads generating Id's, each host or thread should have it's own generator-id. This could be based on the hostname, a config-file value or even be retrieved from an coordinating service. Remember: a generator-id should be unique within the entire system to avoid collisions!

The **sequence**-part is simply a value that is incremented each time a new Id is generated within the same tick (again, by default, a millisecond but can be anything); it is reset every time the tick changes.

## System Clock Dependency

We recommend you use NTP to keep your system clock accurate. IdGen protects from non-monotonic clocks, i.e. clocks that run backwards. The [`DefaultTimeSource`](IdGen/DefaultTimeSource.cs) relies on a 64bit monotonic, increasing only, system counter. However, we still recommend you use NTP to keep your system clock accurate; this will prevent duplicate Id's between system restarts for example.

The [`DefaultTimeSource`](IdGen/DefaultTimeSource.cs) relies on a [`Stopwatch`](https://msdn.microsoft.com/en-us/library/system.diagnostics.stopwatch.aspx) for calculating the 'ticks' but you can implement your own time source by simply implementing the [`ITimeSource`](IdGen/ITimeSource.cs) interface.


## Getting started

Install the [Nuget package](https://www.nuget.org/packages/IdGen) and write the following code:

```c#
using Unified;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string id = UnifiedId.NewId();
    }
}
```

Voila. You have created your first Id! Want to create 100 Id's? Instead of:

`var id = UnifiedId.NewId();`

write:

`var id = generator.Take(100);`

This is because the `IdGenerator()` implements `IEnumerable` providing you with a never-ending stream of Id's (so you might want to be careful doing a `.Select(...)` or `Count()` on it!).

The above example creates a default `IdGenerator` with the GeneratorId (or: 'Worker Id') set to 0 and using a [`DefaultTimeSource`](IdGen/DefaultTimeSource.cs). If you're using multiple generators (across machines or in separate threads or...) you'll want to make sure each generator is assigned it's own unique Id. One way of doing this is by simply storing a value in your configuration file for example, another way may involve a service handing out GeneratorId's to machines/threads. IdGen **does not** provide a solution for this since each project or setup may have different requirements or infrastructure to provide these generator-id's.

The below sample is a bit more complicated; we set a custom epoch, define our own id-structure for generated Id's and then display some information about the setup:

```c#
using IdGen;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Let's say we take april 1st 2020 as our epoch
        var epoch = new DateTime(2020, 4, 1, 0, 0, 0, DateTimeKind.Utc);
            
        // Create an ID with 45 bits for timestamp, 2 for generator-id 
        // and 16 for sequence
        var structure = new IdStructure(45, 2, 16);
            
        // Prepare options
        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));
            
        // Create an IdGenerator with it's generator-id set to 0, our custom epoch 
        // and id-structure
        var generator = new IdGenerator(0, options);

        // Let's ask the id-structure how many generators we could instantiate 
        // in this setup (2 bits)
        Console.WriteLine("Max. generators       : {0}", structure.MaxGenerators);

        // Let's ask the id-structure how many sequential Id's we could generate 
        // in a single ms in this setup (16 bits)
        Console.WriteLine("Id's/ms per generator : {0}", structure.MaxSequenceIds);

        // Let's calculate the number of Id's we could generate, per ms, should we use
        // the maximum number of generators
        Console.WriteLine("Id's/ms total         : {0}", structure.MaxGenerators * structure.MaxSequenceIds);


        // Let's ask the id-structure configuration for how long we could generate Id's before
        // we experience a 'wraparound' of the timestamp
        Console.WriteLine("Wraparound interval   : {0}", structure.WraparoundInterval(generator.Options.TimeSource));

        // And finally: let's ask the id-structure when this wraparound will happen
        // (we'll have to tell it the generator's epoch)
        Console.WriteLine("Wraparound date       : {0}", structure.WraparoundDate(generator.Options.TimeSource.Epoch, generator.Options.TimeSource).ToString("O"));
    }
}
```

Output:
```
Max. generators       : 4
Id's/ms per generator : 65536
Id's/ms total         : 262144
Wraparound interval   : 407226.12:41:28.8320000 (about 1114 years)
Wraparound date       : 3135-03-14T12:41:28.8320000+00:00
```

IdGen also provides an `ITimeSouce` interface; this can be handy for [unittesting](IdGenTests/IdGenTests.cs) purposes or if you want to provide a time-source for the timestamp part of your Id's that is not based on the system time. For unittesting we use our own [`MockTimeSource`](IdGenTests/MockTimeSource.cs).

```xml
<configuration>
  <configSections>
    <section name="idGenSection" type="IdGen.Configuration.IdGeneratorsSection, IdGen.Configuration" />
  </configSections>

</configuration>
```

The attributes (`name`, `id`, `epoch`, `timestampBits`, `generatorIdBits` and `sequenceBits`) are required. The `tickDuration` is optional and defaults to the default tickduration from a `DefaultTimeSource`. The `sequenceOverflowStrategy` is optional too and defaults to `Throw`. Valid DateTime notations for the epoch are:

* `yyyy-MM-ddTHH:mm:ss`
* `yyyy-MM-dd HH:mm:ss`
* `yyyy-MM-dd`

You can get the IdGenerator from the config using the following code:

`var generator = AppConfigFactory.GetFromConfig("foo");`

## Upgrading from 2.x to 3.x

Upgrading from 2.x to 3.x should be pretty straightforward. The following things have changed:

* Most of the constructor overloads for the `IdGenerator` have been replaced with a single constructor which accepts `IdGeneratorOptions` that contains the `ITimeSource`, `IdStructure` and `SequenceOverflowStrategy`
* The `MaskConfig` class is now more appropriately named `IdStructure` since it describes the structure of the generated ID's.
* The `UseSpinWait` property has moved to the `IdGeneratorOptions` and is now an enum of type `SequenceOverflowStrategy` instead of a boolean value. Note that this property has also been renamed in the config file (from `useSpinWait` to `sequenceOverflowStrategy`) and is no longer a boolean but requires one of the values from `SequenceOverflowStrategy`.
* `ID` is now `Id` (only used as return value by the `FromId()` method)

The generated 2.x ID's are still compatible with 3.x ID's. This release is mostly better and more consistent naming of objects.

<hr>

© 2020 Sergiy Seletsky. All rights reserved.