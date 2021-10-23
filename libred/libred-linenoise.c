// SPDX-License-Identifier: LGPL-3.0-or-later

#include "libred.h"

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>

#include "linenoise.h"

static char buf[1025];

static int fd_in = STDIN_FILENO;
static int fd_out = STDOUT_FILENO;

static char *line;
static const char *pending_input;
static const char newline[] = "\n";

char do_KEY(void)
{
	char ch;

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
	
	if (!isatty(fd_in)) {
		ssize_t n = read(fd_in, buf, 1024);
		if (n == 0) { // EOF
			if (fd_in != STDIN_FILENO) {
				fd_in = STDIN_FILENO;
				return do_KEY();
			}

			exit(0);
		}

		buf[n] = '\0';
		pending_input = buf;
		return do_KEY();
	}

	line = linenoise("");
	if (!line) {
		exit(0);
	}
	linenoiseHistoryAdd(line);
	pending_input = line;
	return do_KEY();
}

void do_EMIT(char ch)
{
	write(fd_out, &ch, 1);
}

void do_TYPE(char *s, size_t len)
{
	write(fd_out, s, len);
}

void do_INCLUDE(char *fname)
{
	if (fd_in > STDIN_FILENO)
		close(fd_in);

	fd_in = open(fname, O_RDONLY);
	var_LINENO = 0;
}

void do_EXUDE(char *fname)
{
	if (fd_out > STDOUT_FILENO)
		close(fd_out);

	fd_out = open(fname, O_WRONLY);
}
