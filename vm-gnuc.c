// SPDX-License-Identifier: LGPL-3.0-or-later

#include <ctype.h>
#include <inttypes.h>
#include <stdbool.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "libred.h"

#define PUSH(p) do { dsp[-1] = (p); dsp--; } while(0)
#define POP() (*dsp++)
#define PUSHRSP(p) do { rsp[-1] = (p); rsp--; } while(0)
#define POPRSP() (*rsp++)

/*
 * The PREVIOUS macro helps us statically configure the linked list within
 * the dictionary. It relies on a LINK macro to tell us the name of the
 * previous word.
 */
#define PREVIOUS ((struct header *)&GLUE(word_, LINK))

/*
 * The initial value for LINK ensures PREVIOUS will evaluate to the null pointer
 */
#define LINK NULL_LINK
#define word_NULL_LINK (*(char *) NULL)

/* Calculate the padded length of a word name (len must *include* '\0') */
#define RAW_NAMELEN(len) ((len)|(sizeof(void*)-1))

/* Calculate the padded length of a string literal */
#define NAMELEN(s) RAW_NAMELEN(sizeof(s))

/*
 * Generate an entry point and dictionary entry for a word implemented in C.
 *
 * Notice the trailing opening brace. This is a trick to ensure that we get
 * a compiler error is we forget to include the NEXT() macro at the end of
 * the word. The error message might not be easy to understand... but at least
 * we won't compile junk!
 */
#define NATIVE_FLAGS(label, wname, wflags)                                     \
label:                                                                         \
	(void) 0;                                                              \
	static const struct {                                                  \
		struct header hdr;                                             \
		char name[NAMELEN(wname)];                                     \
		uint8_t flags;                                                 \
		struct codefield cf;                                           \
	} word_##label = {                                                     \
	    {PREVIOUS},                                                        \
	    wname,                                                             \
	    wflags | ((NAMELEN(wname) + 1) / sizeof(void *)),                  \
	    {.codeword = &&label},                                             \
	};                                                                     \
	{                                                                      \
		/* code from the vm header file will appear here... */

#define RAW_NEXT()                                                             \
		do {                                                           \
			codeword = *ip++;                                      \
			goto **codeword;                                       \
		} while (0)

#define NEXT()                                                                 \
		RAW_NEXT();                                                    \
	}

/* Shorter versions of NATIVE_FLAGS() that can be used to reduce boilerplate */
#define NATIVE(label, wname) NATIVE_FLAGS(label, wname, 0)
#define QNATIVE(label) NATIVE_FLAGS(label, #label, 0)

/*
 * Generate a dictionary entry for executing a word written in Forth
 *
 * This uses the same mismatched brace trick to ensure we complete the
 * builtin with a closing word, typically EXIT.
 */
#define BUILTIN_FLAGS(label, wname, wflags)                                    \
	static const struct {                                                  \
		struct header hdr;                                             \
		char name[NAMELEN(wname)];                                     \
		uint8_t flags;                                                 \
		struct codefield cf;                                           \
		void *words[];                                                 \
	} word_##label = {                                                     \
	    {PREVIOUS},                                                        \
	    wname,                                                             \
	    wflags | ((NAMELEN(wname) + 1) / sizeof(void *)),                  \
	    {.codeword = &&DOCOL},                                             \
	    {

#define UNREACHABLE()                                                          \
	    }                                                                  \
	};

/* Other helpers to allow us to "compile" codewords for use by BUILTINs */
#define COMPILE(label) ((void **) &word_##label.cf.codeword),
#define COMPILE_LIT(lit) COMPILE(LIT) (void *) lit,
#define COMPILE_BRANCH(offset) COMPILE(BRANCH) (void *) (offset),
#define COMPILE_EXIT()                                                         \
	COMPILE(EXIT)                                                          \
	UNREACHABLE()

/* Shorter versions of BUILTIN_FLAGS() that can be used to reduce boilerplate */
#define BUILTIN(label, wname) BUILTIN_FLAGS(label, wname, 0)
#define QBUILTIN(label) BUILTIN_FLAGS(label, #label, 0)

/*
 * Generate words to load the address of variables
 */
#define VARIABLE(wname, vname)                                                 \
NATIVE(vname, wname)                                                           \
	PUSH((cell_t) { .p = &var_##vname });                                  \
	NEXT();

/*
 * Generate words to load the value of constants
 */
#define CONSTANT(wname, label, value)                                          \
NATIVE(label, wname)                                                           \
	PUSH((cell_t) { .u = (value) });                                       \
	NEXT();

/*
 * Entry point for the forth system.
 *
 * This simply declares the state variables and jumps to start (which is defined
 * at the very bottom of this file).
 *
 * In the middle is a bunch of macro soup that allows us to implement indirect
 * threading by taking the address of the labels (a GNU C extension).
 */
void rf_forth_exec(struct forth_task *ctx)
{
	/* Data and Return stacks */
	cell_t *dsp = (void *) ctx->dsp;
	cell_t *rsp = (void *) ctx->rsp;
	//void ****rsp = (void *) ctx->rsp;

	/* Named variables */
	uintptr_t var_STATE = 0;
	char *var_HERE = (char *) ctx->here;
	/* var_LATEST */
	cell_t *var_S0 = dsp;
	cell_t *var_R0 = (void *) rsp;
	uintptr_t var_BASE = 10;

	void ***ip = NULL;
	void **codeword;
	cell_t tmp;

	goto start;

DOCOL:
	PUSHRSP((cell_t) { .p = ip });
        ip = (void ***)(codeword + (sizeof(struct codefield) / sizeof(void *)));
        RAW_NEXT();

#include "words-basic.h"
#include "words-stdc.h"

start:
	if (!var_LATEST)
		var_LATEST = PREVIOUS;

	if (!ip) {
		/* Exit Forth if we run out of words */
		void **halt = (void **) (&word_PAUSE.cf.codeword);
		ip = &halt;

		/* Run BOOT */
		codeword = (void **) (&word_QUIT.cf.codeword);
		goto DOCOL;
	} else {
		RAW_NEXT();
	}
}