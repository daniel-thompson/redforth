redforth - A simple, portable and  ROMable Forth implementation
===============================================================

redforth is currently incomplete and is missing several critical pieces
and is being aggressively refactored It is derived from
https://github.com/daniel-thompson/jonesforth-c so, if you are
interested in a simple, well documented Forth implemented in C then,
for now, you are better off looking at jonesforth-c!

Building
========

Native builds
-------------

Try:

~~~
mkdir -p build
cd build
cmake ..
make
make check
~~~

Cross builds using musl-cross-make toolchains
---------------------------------------------

Requires a native build because we need to "borrow" crossforth from the native
build. We need it to generate ROMable code.

Try something like the following (adapted for your target):

~~~
mkdir -p build-armv7
cd build-armv7
cmake  .. \
	-DCMAKE_TOOLCHAIN_FILE=../scripts/musl-cross.cmake \
	-DCROSS_COMPILE=arm-linux-musleabihf- \
	-DREDFORTH_TOOLS_DIR=../build
make
~~~
