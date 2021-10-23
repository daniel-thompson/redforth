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

static FILE *rf_in = NULL;
static FILE *rf_out = NULL;

char do_KEY(void)
{
	if (!rf_in)
		rf_in = stdin;

	int ch = fgetc(rf_in);
#ifdef ENABLE_KEY_ECHO
	if (rf_in == stdin)
		putchar(ch);
#endif
	if (ch == EOF) {
		if (rf_in == stdin) {
			exit(0);
		} else {
			rf_in = stdin;
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

void do_TYPE(char *s, size_t len)
{
	if (!rf_out)
		rf_out = stdout;
	fwrite(s, 1, len, rf_out);
	fflush(rf_out);
}

void do_INCLUDE(char *fname)
{
	if (rf_in && rf_in != stdin)
		fclose(rf_in);

	rf_in = fopen(fname, "r");
	var_LINENO = 0;
}

void do_EXUDE(char *fname)
{
	if (rf_out && rf_out != stdout)
		fclose(rf_out);

	rf_out = fopen(fname, "w");
}
