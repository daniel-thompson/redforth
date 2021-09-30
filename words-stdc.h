// SPDX-License-Identifier: LGPL-3.0-or-later

#ifndef RF_WORDS_STDC_H_
#define RF_WORDS_STDC_H_

CONSTANT("FN-fopen", FN_fopen, (uintptr_t) &fopen)
#undef  LINK
#define LINK FN_fopen

CONSTANT("FN-fclose", FN_fclose, (uintptr_t) &fclose)
#undef  LINK
#define LINK FN_fclose

CONSTANT("FN-fread", FN_fread, (uintptr_t) &fread)
#undef  LINK
#define LINK FN_fread

CONSTANT("FN-fwrite", FN_fwrite, (uintptr_t) &fwrite)
#undef  LINK
#define LINK FN_fwrite

CONSTANT("FN-exit", FN_exit, (uintptr_t) &exit)
#undef  LINK
#define LINK FN_exit

CONSTANT("FN-malloc", FN_malloc, (uintptr_t) &malloc)
#undef  LINK
#define LINK FN_malloc

CONSTANT("FN-free", FN_free, (uintptr_t) &free)
#undef  LINK
#define LINK FN_free

#endif /* RF_WORDS_STDC_H_ */
