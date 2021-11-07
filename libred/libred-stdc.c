// SPDX-License-Identifier: LGPL-3.0-or-later

#include "libred.h"

#include <ctype.h>
#include <inttypes.h>
#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

cell_t var_SOURCE_ID = (cell_t) { .p = NULL };
static FILE *rf_out = NULL;

char do_KEY(void)
{
	if (!var_SOURCE_ID.p)
		var_SOURCE_ID.p = stdin;

	int ch = fgetc(var_SOURCE_ID.p);
	if (ch == EOF) {
		if (var_SOURCE_ID.p == stdin) {
			exit(0);
		} else {
			var_SOURCE_ID.p = stdin;
			return do_KEY();
		}
	}

	return ch;
}

void do_EMIT(char ch)
{
	if (!rf_out)
		rf_out = stdout;
	fputc(ch, rf_out);
	fflush(rf_out);
}

void do_TYPE(const char *s, size_t len)
{
	if (!rf_out)
		rf_out = stdout;
	fwrite(s, 1, len, rf_out);
	fflush(rf_out);
}

void do_INCLUDE(char *fname)
{
	if (var_SOURCE_ID.p && var_SOURCE_ID.p != stdin)
		fclose(var_SOURCE_ID.p);

	var_SOURCE_ID.p = fopen(fname, "r");
	var_LINENO = 0;
}

void do_EXUDE(char *fname)
{
	if (rf_out && rf_out != stdout)
		fclose(rf_out);

	rf_out = fopen(fname, "w");
}
