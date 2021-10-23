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
mkdir -p build
cd build
cmake ..
make
make test
~~~

Cross builds using musl-cross-make toolchains
---------------------------------------------

Try something like the following (adapted for your target):

~~~
mkdir -p build-armv7
cd build-armv7
cmake \
	-DCMAKE_TOOLCHAIN_FILE=../scripts/musl-cross.cmake \
	-DCROSS_COMPILE=arm-linux-musleabihf- \
	-DREDFORTH_TOOLS_DIR=../build \
	..
make -j `nproc`
~~~

Build for Raspberry Pi Pico
---------------------------

Try:

~~~
mkdir -p build-rp2
cd build-rp2
git clone https://github.com/raspberrypi/pico-sdk
(cd pico-sdk; git submodule update --init)
cmake -DPICO_SDK_PATH=pico-sdk -DREDFORTH_TOOLS_DIR=../build ..
make -j `nproc`
~~~
