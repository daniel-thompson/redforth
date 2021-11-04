redforth - A simple, portable and  ROMable Forth implementation
===============================================================

redforth is currently incomplete and is missing several critical pieces
and is being aggressively refactored It is derived from
https://github.com/daniel-thompson/jonesforth-c so, if you are
interested in a simple, well documented Forth implemented in C then,
for now, you are better off looking at jonesforth-c!

Building
========

All cross builds require a native build first. We need to "borrow" crossforth
from the native build in order to generate ROMable code for the cross
builds.

Native builds
-------------

Try:

~~~
cmake -B build/
make -C build/ -j `nproc`
make test
~~~

Cross builds using musl-cross-make toolchains
---------------------------------------------

Try something like the following (adapted for your target):

~~~
cmake -B build-musl/ \
	-DCMAKE_TOOLCHAIN_FILE=scripts/musl-cross.cmake \
	-DCROSS_COMPILE=arm-linux-musleabihf- \
	-DREDFORTH_TOOLS_DIR=$PWD/build \
make -C build-musl/ -j `nproc`
~~~

Build for Raspberry Pi Pico
---------------------------

Try:

~~~
git submodule update --init
(cd ports/rp2/pico-sdk; git submodule update --init)
cmake -B build-rp2/ \
	-DPICO_SDK_PATH=$PWD/ports/rp2/pico-sdk \
	-DREDFORTH_TOOLS_DIR=$PWD/build
make -C build-rp2/ -j `nproc`
~~~
