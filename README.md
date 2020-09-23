<img src="https://raw.githubusercontent.com/SergiySeletsky/Unified/master/docs/logo.png" width="128" height="128" />

# **Unified Id** - the identity of your data.

[![Pull Requests Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat)](https://github.com/SergiySeletsky/Unified/compare)
[![Build](https://github.com/SergiySeletsky/Unified/workflows/Build/badge.svg)](https://github.com/SergiySeletsky/Unified/actions?query=workflow:Build)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Unified&metric=alert_status)](https://sonarcloud.io/dashboard?id=Unified)
[![NuGet](https://img.shields.io/nuget/v/Unified)](https://www.nuget.org/packages/Unified)

## Why

What is Unified Id?
If GUID is too heavy for your application but you need a random global Id that can be used as a string or long basic type, you are in right place.

What are the main advantages?
| Feature | Unified | GUID |
| ------ | ------ | ------ |
| Size | 8 byte (13 as string) | 16 byte (36 as string)
| Partitioning | Build-in | No, external can be added
| Collisions | 0.00000001% in 10B IDs | 50% in 2.7e18 IDs
| Cast | implicit(string, ulong, long) | explicit(byte[], Parse/ToString)
| Generate | Build-in(byte[], string, GUID) | No, only random NewGuid
| Null/Empty-handling | Friendly as Empty | Exception

## Getting started

Install the [NuGet package](https://www.nuget.org/packages/Unified) and write the following code:

```c#
using Unified;

class Program
{
    static void Main()
    {
        var id = UnifiedId.NewId();
    }
}
```

You have created your first Unified Id!

Want to use it as a string? `string id = UnifiedId.NewId();` or long? `long id = UnifiedId.NewId();`

UnifiedId could be used as DDD ValueObject in your entities.

```c#
using Unified;

class User
{
    public UnifiedId UserId { set; get; }
}

class Program
{
    static void Main()
    {
        var user = new User
        {
            UserId = UnifiedId.NewId();
        };

        var settings = new JsonSerializerSettings // Could be added to global settings.
        { 
            Converters = new List<JsonConverter> 
            { 
                new UnifiedIdConverter() 
            }
        };
        var json = JsonConvert.SerializeObject(user, settings); // { "UserId": "AFHUTVDSGUGVQ" }
    }
}
```

## How it works

```c#
using Unified;

class Program
{
    static void Main()
    {
        var guid = Guid.NewGuid();
        var id = UnifiedId.FromGuid(guid);
        var fnv = id.ToUInt64();
        Console.WriteLine($"{guid} => {fnv} => {id}");
        // 8dd02ad1-62cc-4015-9502-49658ba240ae => 15834445116674749764 => DNFPVU1LD2DA4
    }
}
```

Unified Id generates 64bit FNV-1a Id's based on GUID and converts it to HEX32 to use as string.

![Algorithm](https://raw.githubusercontent.com/SergiySeletsky/Unified/master/docs/algorithm.png)

HEX32 is reversible, so you can convert it back from string to UInt64.
`var id = UnifiedId.Parse("DNFPVU1LD2DA4");`

Why FNV-1a 64bit? because it has the best space randomization in the case of GUID conversion, below is space representation.

![FNV-1a](https://raw.githubusercontent.com/SergiySeletsky/Unified/master/docs/fnv-1a-space.png)

Default method of generation is GUID based using method `var id = UnifiedId.NewId()`.

This value could be used as string converted in 32xHEX consisting of two parts.

[KEY][UNIFIED_ID] KEY - Partition/Shard Key and UNIFIED_ID as Row Unified Key together used as the global identity.

You can also generate this Id as a one-way hash using the following sources:

* `UnifiedId FromGuid(Guid id)`
* `UnifiedId FromBytes(byte[] bytes)`
* `UnifiedId FromString(string text)`
* `UnifiedId FromInt64(long number)`
* `UnifiedId FromUInt64(ulong number)`

Do you need sequential Id? `var id = new UnifiedId(DateTime.UtcNow.Ticks)`

Want to save partitioned data? It's easy...

```c#
using Unified;
using System;

class Program
{
    static void Main()
    {
        // Let's emulate the partitioned database.
        var db = new Dictionary<string, List<UnifiedId>>();

        // We will use 10M records, just to execute it fast.
        var all = 10000000; 
        for (var i = 0; i <= all; i++)
        {
            // Generate random Id.
            var id = UnifiedId.NewId();

            // Get it's partition key. Number of partitions could be customized, default 16K.
            var partition = id.PartitionKey();

            // Initialize partitions in your DB.
            if (!db.ContainsKey(partition))
            {
                db.Add(partition, new List<UnifiedId>());
            }

            // Add values to partitions.
            db[partition].Add(id);
        }
    }
}
```

Result:
```
DB Count             : 16384
Each item contain    : 610 elements +/-5%
```
We recommend using Unified Id for data sets size up to 10 billion Ids. More will increase the chance of collision.

<hr>

© 2020 Sergiy Seletsky. All rights reserved.