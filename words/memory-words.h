// SPDX-License-Identifier: LGPL-3.0-or-later

#ifndef RF_MEMORY_WORDS_H_
#define RF_MEMORY_WORDS_H_

/*
 * Defines words found in the optional Memory-Allocation word set.
 *
 * It is implemented using the default stdlib.h malloc/free.
 */

QNATIVE(ALLOCATE)
#undef  LINK
#define LINK ALLOCATE
	dsp[0].p = malloc(dsp[0].u);
	PUSH((cell_t) { .n = dsp[0].p ? 0 : -ENOMEM });
	NEXT();

QNATIVE(FREE)
#undef  LINK
#define LINK FREE
	free(dsp[0].p);
	dsp[0].n = 0;
	NEXT();

QNATIVE(RESIZE)
#undef  LINK
#define LINK RESIZE
	tmp.p = realloc(dsp[1].p, dsp[0].u);
	if (tmp.p) {
		dsp[1] = tmp;
		dsp[0].n = 0;
	} else {
		dsp[0].n = -ENOMEM;
	}
	NEXT();

#endif /* RF_MEMORY_WORDS_H_ */
