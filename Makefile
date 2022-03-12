# SPDX-License-Identifier: LGPL-3.0-or-later

#
# Wrap all supported cmake builds in a single Makefile. This allows us
# to test all builds in one-shot.
#

PORTS = native

ifndef NO_ARMV7
CROSS_COMPILE ?= arm-linux-musleabihf-
PORTS += armv7
# Automatically run with qemu-arm if it is available
ifneq ($(shell command -v qemu-arm),)
QEMU_USER_ARMV7 = -DQEMU_USER=qemu-arm
endif
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
		-DREDFORTH_TOOLS_DIR=$(PWD)/build \
		$(QEMU_USER_ARMV7)
	$(MAKE) -C build-$@/

submodules :
	git submodule update --init
	(cd ports/rp2/pico-sdk; git submodule update --init)

rp2 : native submodules
	cmake -B build-$@ \
		-DPICO_SDK_PATH=$(PWD)/ports/rp2/pico-sdk \
		-DREDFORTH_TOOLS_DIR=$(PWD)/build
	$(MAKE) -C build-$@/

rp2-install : rp2
	if [ -d /var/run/media/$(USER)/RPI-RP2 ]; then \
	    cp build-rp2/redforth.uf2 /var/run/media/$(USER)/RPI-RP2; \
	else \
	    cp build-rp2/redforth.uf2 /media/$(USER)/RPI-RP2; \
	fi

rp2-interact :
	picocom --send-cmd ./scripts/encode.fs /dev/ttyACM0

test : all
	$(MAKE) -C build/ test
ifndef NO_ARMV7
	$(MAKE) -C build-armv7/ test
endif
