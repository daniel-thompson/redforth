// SPDX-License-Identifier: LGPLv3-or-later

#include <ctype.h>
#include <inttypes.h>
#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

#include "libred.h"

uintptr_t sys_argc;
const char **sys_argv;

/*
 * On Unix platforms there is no real need to minimise RAM usage
 * since this will all be allocated-on-write anyway!
 */
static uintptr_t stack[4096];
static uintptr_t rstack[4096];
static char here[512*1024];

static void run_as_script(struct forth_task *task, const char *fname)
{
	task->input = here + sizeof(here) - (16 + strlen(fname));
	*task->input = '\0';

	/* strcat() is slower than explicit memcpy(), but less error prone! */
	strcat(task->input, "INCLUDE ");
	strcat(task->input, fname);
	strcat(task->input, " [DEFINED] MAIN [IF] MAIN [THEN] BYE ");
}

int main(int argc, const char *argv[])
{
	struct forth_task task = {
		.dsp = stack + 4096,
		.rsp = rstack + 4096,
		.here = (uintptr_t *) here,
	};

	/* Handle launching as a script (e.g. execute a filename) */
	if (argc > 0) {
		const char *fname = strrchr(argv[0], '/');
		if (fname)
			fname += 1;
		else
			fname = argv[0];

		if (0 != strcmp(fname, "redforth") &&
		    0 != strcmp(fname, "crossforth")) {
			run_as_script(&task, argv[0]);
			sys_argc = argc;
			sys_argv = argv;
		} else if (argc > 1 && 0 == access(argv[1], R_OK)) {
			run_as_script(&task, argv[1]);
			sys_argc = argc - 1;
			sys_argv = argv + 1;
		}
	}

	/* Handle Forth input on the command line */
	if (!task.input && argc > 1) {
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

	rf_forth_init_completions();
	rf_forth_exec(&task);
	printf(" unexpected VM exit\n");

	return 1;
}
