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

struct header *var_LATEST;
uintptr_t var_LINENO = 0;

struct codefield *to_CFA(struct header *l)
{
	char *name = (char *) (l + 1);
	size_t len = strlen(name) + 1;
	size_t offset = (len | (sizeof(void*)-1)) + 1;

	return (struct codefield *) (name + offset);
}

uint8_t *to_flags(struct header *l)
{
	uint8_t *cf = (uint8_t *) to_CFA(l);
	return cf - 1;
}

struct header *do_FIND(const char *s, size_t n)
{
	struct header *node = var_LATEST;

	while (node) {
		char *name = (char *) (node + 1);
		if (0 == strncmp(name, s, n) && name[n] == '\0') {
			uint8_t *flags = to_flags(node);
			if (!(*flags & F_HIDDEN))
				return node;
		}
		node = node->next;
	}

	return NULL;
}

char do_KEY(void)
{
	int ch = getchar();
	if (ch == EOF)
		exit(0);

	return ch;
}

void do_EMIT(char ch)
{
	putchar(ch);
	fflush(stdout);
}

uintptr_t do_NUMBER(char *cp, uintptr_t len, int base, char **endptr)
{
	uintptr_t n = 0;
	int i = 0;
	bool negative = false;

	for (i=0; i<len; i++) {

		if (i == 0 && (cp[0] == '+' || cp[0] == '-')) {
			negative = cp[0] == '-';
			continue;
		}

		uintptr_t digit;
		if (cp[i] >= '0' && cp[i] <= '9')
			digit = cp[i] - '0';
		else if (cp[i] >= 'A' && cp[i] <= 'Z')
			digit = 10 + (cp[i] - 'A');
		else if (cp[i] >= 'a' && cp[i] <= 'z')
			digit = 10 + (cp[i] - 'a');
		else
			digit = 37;

		if (digit > (base-1))
			break;

		n *= base;
		n += digit;
	}

	if (endptr)
		*endptr = cp + i;
	if (negative)
		return -n;
	return n;
}

char *do_WORD(void)
{
	static char buf[32];
	char *p = buf;
	int ch;

	do {
		ch = getchar();
		if (ch == '\n')
			var_LINENO++;
	} while (isspace(ch));

	if (ch == EOF)
		exit(0);

	/* Handle comments */
	if (ch == '\\') {
		while (ch != '\n' && ch != EOF)
			ch = getchar();
		var_LINENO++;
		return do_WORD();
	}

	/* Read the word */
	*p++ = ch;
	do {
		ch = getchar();
		*p++ = ch;
	} while (!isspace(ch) && ch != EOF);

	/* Terminate word */
	*--p = '\0';

	/* Did the word end with a newline? */
	if (ch == '\n')
		var_LINENO++;

	return buf;
}
