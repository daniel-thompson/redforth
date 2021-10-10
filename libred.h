// SPDX-License-Identifier: LGPL-3.0-or-later

#ifndef RF_LIBRED_H_
#define RF_LIBRED_H_

#include <inttypes.h>
#include <stddef.h>


/*
 * FORTH CELL DEFINITION  ---------------------------------------------------
 */

typedef union cell {
	intptr_t n;	//!< Access the cell as a signed number
	uintptr_t u;	//!< Access the cell as an unsigned number

	void *p;	//!< Access as a generic pointer (for assignment of any pointer)
	char *cp;	//!< Access the cell as a string (or 8-bit ptr)
	union cell (*fnp)();	//!< Make function calls using the value in the cell
	union cell *xp;	//!< As a pointer to a cell. Use -> to dereference (e.g. tmp.xp->u )
} cell_t;

/*
 * FORTH DICTIONARY  --------------------------------------------------------
 */

/* Flag handling macros */
#define F_IMMED 0x80
#define F_HIDDEN 0x20
#define F_LENMASK 0x1f
#define F_LEN(f) ((f & F_LENMASK) * (sizeof(void*)))

struct header {
	struct header *next;
};

struct codefield {
	void *codeword;	//!< codeword is expected to be the first item in the codefield
};

struct codefield *to_CFA(struct header *l);
uint8_t *to_flags(struct header *l);
struct header *do_FIND(const char *s, size_t n);
char do_KEY(void);
void do_EMIT(char ch);
void do_TYPE(char *s, size_t len);
uintptr_t do_NUMBER(char *cp, uintptr_t len, int base, char **endptr);
char *do_WORD(void);
void do_INCLUDE(char *fname);
void do_EXUDE(char *fname);

/*
 * FORTH EXECUTION  ---------------------------------------------------------
 */

struct forth_task {
	uintptr_t *dsp;
	uintptr_t *rsp;
	uintptr_t *here;

	uintptr_t *ip;
};

extern struct header *var_LATEST;
extern uintptr_t var_LINENO;

void rf_forth_exec(struct forth_task *ctx);

/*
 * UTILITIES  ---------------------------------------------------------------
 */

#define containerof(ptr, type, member) \
	((type *) (((char *) ptr) - offsetof(type, member)))

/* Classic C pre-processor macros to expand and join tokens */
#define GLUE2(x, y) x ## y
#define GLUE(x, y) GLUE2(x, y)



#endif /* RF_LIBRED_H_ */
