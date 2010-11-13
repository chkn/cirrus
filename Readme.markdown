Cirrus
======

Overview
--------

Threading sucks. It takes work and expertise to correctly utilize multiple threads for an actual performance gain. Cirrus is a multimedia-oriented framework that will leverage [hybrid threading](http://en.wikipedia.org/wiki/Thread_%28computer_science%29#N:M_.28Hybrid_threading.29) for both increased concurrency and programmer sanity.

Currently, I am releasing the concurrency core of Cirrus which implements [fibers](http://en.wikipedia.org/wiki/Fiber_%28computer_science%29) with [coroutines](http://en.wikipedia.org/wiki/Coroutine). It is easy to learn and can be consumed by any application targeting Mono/.NET v3.5 or later.


On the to do list:
------------------

1. Support for try...catch...finally blocks in async methods.
2. Make FutureCollection&lt;T&gt; explicitly implement IEnumerable&lt;T&gt; and support Linq (AsyncLinq anyone?)
3. Make the post-compiler fix up sequence points to ease debugging pains in instrumented assemblies.
4. Tests, tests, tests!
...


License and Contributions
-------------------------

MIT/X11 and welcome, respectively :)
