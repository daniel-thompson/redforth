// SPDX-License-Identifier: LGPLv3-or-later

#include <ctype.h>
#include <inttypes.h>
#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "pico/stdlib.h"

#include "libred.h"

int main(int argc, const char *argv[])
{
	stdio_init_all();

	uintptr_t stack[1024];
	uintptr_t rstack[1024];
	char here[64*1024];

	struct forth_task task = {
		.dsp = stack + 1024,
		.rsp = rstack + 1024,
		.here = (uintptr_t *) here,
	};

	rf_forth_exec(&task);
	printf(" unexpected VM exit\n");

	return 1;
}
