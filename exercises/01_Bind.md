## Effects

Computation expressions express effects by means of normal F# syntax followed by a `!`, e.g. `let!`.
This is intended to highlight that some side effect is occurring in that binding, and that it isn't
pure.

Add a test to try to use the `let!`:

```
        test "TraceBuilder performs a side effect" {
            let expected = 1
            let actual = trace {
                let! result = expected + expected
                return result
            }
            Expect.equal actual (expected + expected) "Actual should match expected + expected"
        }
```

Build and run the program with `dotnet test`. Your program should fail to compile with the following error:

```
/Users/ryan/Code/computation-expressions-workshop/Start.fs(27,17): error FS0708: This control construct may only be used if the computation expression builder defines a 'Bind' method [/Users/ryan/Code/computation-expressions-workshop/computation-expressions-workshop.fsproj]
```

The compiler informs us that in order to use the `let!` keyword, we must implement the `Bind` method on our builder.

## Enabling Effects

1. Add a `member __.Bind(m, f) =` to your `TraceBuilder`.
2. In the body, add the following:

```
    printfn "%A %A" m f // <-- our side effect
    f m
```

The `Bind
What are `m` and `f`? Our side effect should print them out.

Running the program with `dotnet test` again, you should see that you have two passing tests.
You should also see something printed in the output that wasn't there before, e.g.

```
Test run for /Users/ryan/Code/computation-expressions-workshop/bin/Debug/netcoreapp2.1/computation-expressions-workshop.dll(.NETCoreApp,Version=v2.1)
Microsoft (R) Test Execution Command Line Tool Version 15.8.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
**<fun:actual@26>**

Total tests: 2. Passed: 2. Failed: 0. Skipped: 0.
Test Run Successful.
Test execution time: 1.4303 Seconds
```

The builder emitted a side effect by printing out the result.

## Better Effects
