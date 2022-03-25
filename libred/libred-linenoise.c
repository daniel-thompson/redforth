// SPDX-License-Identifier: LGPL-3.0-or-later

#include "libred.h"

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>

#include "linenoise.h"

static char buf[1025];
static const char *file_input;

cell_t var_SOURCE_ID = (cell_t) { .n = STDIN_FILENO };
static int fd_out = STDOUT_FILENO;

static char *line;
static const char *pending_input;
static const char newline[] = "\n";

static void complete(const char *buf, linenoiseCompletions *lc)
{
	int n = strlen(buf);

	for (struct header *node = var_LATEST; node; node = node->next) {
		char *name = (char *) (node + 1);
		if (0 == strncmp_toupper(name, buf, n))
			linenoiseAddCompletion(lc, name);
	}
}

void rf_forth_init_completions(void)
{
	linenoiseSetCompletionCallback(complete);
}

void do_QUEUE(const char *input)
{
	pending_input = input;
}

char do_KEY(void)
{
	char ch;

	if (file_input) {
		ch = *file_input++;
		if (*file_input == '\0')
			file_input = NULL;
		return ch;
	}
	
	if (!isatty(var_SOURCE_ID.n)) {
		ssize_t n = read(var_SOURCE_ID.n, buf, 1024);
		if (n == 0) { // EOF
			if (var_SOURCE_ID.n != STDIN_FILENO) {
				var_SOURCE_ID.n = STDIN_FILENO;
				return do_KEY();
			}

			exit(0);
		}

		buf[n] = '\0';
		file_input = buf;
		return do_KEY();
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
		line = linenoise("ok> ");
		if (!line)
			exit(0);
	} while (line[0] == '\0');

	linenoiseHistoryAdd(line);
	pending_input = line;
	return do_KEY();
}

void do_EMIT(char ch)
{
	write(fd_out, &ch, 1);
}

void do_TYPE(const char *s, size_t len)
{
	write(fd_out, s, len);
}

void do_INCLUDE(char *fname)
{
	if (var_SOURCE_ID.n > STDIN_FILENO)
		close(var_SOURCE_ID.n);

	var_SOURCE_ID.n = open(fname, O_RDONLY);
	var_LINENO = 0;
}

void do_EXUDE(char *fname)
{
	if (fd_out > STDOUT_FILENO)
		close(fd_out);

	fd_out = open(fname, O_WRONLY);
}
