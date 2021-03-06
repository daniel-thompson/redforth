// SPDX-License-Identifier: LGPL-3.0-or-later

#include "libred.h"

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>

#include <pico/bootrom.h>

#include "linenoise.h"
#include "littlefs.h"

cell_t var_SOURCE_ID = (cell_t) { .p = NULL };

static char *line;
static const char *pending_input;
static const char newline[] = "\n";
static char pending_char = 0;


int linenoiseGetChar()
{
	if (pending_char) {
		char c = pending_char;
		pending_char = 0;
		return c;
	}

	return getchar();
}

void linenoiseUngetc(char c)
{
	pending_char = c;
}

void linenoiseWrite(const char *s, unsigned int len)
{
	(void) fwrite(s, len, 1, stdout);
	fflush(stdout);
}

void do_QUEUE(const char *input)
{
	pending_input = input;
}

char do_KEY(void)
{
	char ch;

	if (var_SOURCE_ID.p) {
		int res = lfs_file_read(&rp2_littlefs, var_SOURCE_ID.p, &ch, 1);
		if (res == 1)
			return ch;
		(void) lfs_file_close(&rp2_littlefs, var_SOURCE_ID.p);
		free(var_SOURCE_ID.p);
		var_SOURCE_ID.p = NULL;
	}

	if (pending_input) {
		ch = *pending_input++;
		if (*pending_input == '\0') {
			pending_input = NULL;
			if (line) {
				free(line);
				line = NULL;
				/* queue a newline... linenoise doesn't
				 * do that and we need the separator to
				 * complete the word
				 */
				pending_input = newline;
			}
		}
		return ch;
	}
	
	do {
		line = linenoise("");
		if (!line)
			exit(0);
	} while (line[0] == '\0');

	linenoiseHistoryAdd(line);
	pending_input = line;
	return do_KEY();
}

void do_EMIT(char ch)
{
	linenoiseWrite(&ch, 1);
}

void do_TYPE(const char *s, size_t len)
{
	linenoiseWrite(s, len);
}

void do_INCLUDE(char *fname)
{
	const char msg[] = "INCLUDE not supported\n";
	do_TYPE(msg, sizeof(msg)-1);
}

void do_EXUDE(char *fname)
{
	const char msg[] = "EXUDE not supported\n";
	do_TYPE(msg, sizeof(msg)-1);
}

void do_REBOOT()
{
	reset_usb_boot(0, 0);
}
