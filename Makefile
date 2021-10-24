# SPDX-License-Identifier: LGPL-3.0-or-later

#
# Wrap all supported cmake builds in a single Makefile. This allows us
# to test all builds in one-shot.
#

ifndef NO_ARMV7
CROSS_COMPILE ?= arm-linux-musleabihf-
PORTS += armv7
endif

ifndef NO_RPI_RP2
PORTS += rp2
endif

all : $(PORTS)

clean :
	$(RM) -r build/ $(patsubst %,build-%/,$(PORTS))

native :
	cmake -B build
	$(MAKE) -C build

armv7 : native
	cmake -B build-$@ \
		-DCMAKE_TOOLCHAIN_FILE=scripts/musl-cross.cmake \
		-DCROSS_COMPILE=$(CROSS_COMPILE) \
		-DREDFORTH_TOOLS_DIR=$(PWD)/build
	$(MAKE) -C build-$@/

rp2 : native pico-sdk
	cmake -B build-$@ \
		-DPICO_SDK_PATH=$(PWD)/pico-sdk \
		-DREDFORTH_TOOLS_DIR=$(PWD)/build
	$(MAKE) -C build-$@/

rp2-install : rp2
	cp build-rp2/redforth.uf2 /media/$(USER)/RPI-RP2

pico-sdk :
	git clone https://github.com/raspberrypi/pico-sdk
	(cd pico-sdk; git submodule update --init)

test : all
	$(MAKE) -C build/ test
	$(MAKE) -C build-armv7/ test
