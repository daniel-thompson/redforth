// SPDX-License-Identifier: LGPLv3-or-later

#include <ctype.h>
#include <inttypes.h>
#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "libred.h"

int main(int argc, const char *argv[])
{
	/*
	 * On Unix platforms there is no real need to minimise RAM usage
	 * since this will all be allocated-on-write anyway!
	 */
	uintptr_t stack[4096];
	uintptr_t rstack[4096];
	char here[512*1024];

	struct forth_task task = {
		.dsp = stack + 4096,
		.rsp = rstack + 4096,
		.here = (uintptr_t *) here,
	};

	if (argc > 1) {
		unsigned int len = 1;
		for (int i = 1; i < argc; i++)
			len += strlen(argv[i]) + 1;

		task.input = here + sizeof(here) - len;
		*task.input = '\0';
		for (int i = 1; i < argc; i++) {
			strcat(task.input, argv[i]);
			strcat(task.input, " ");
		}
	}

	rf_forth_exec(&task);
	printf(" unexpected VM exit\n");

	return 1;
}
