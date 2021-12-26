// SPDX-License-Identifier: LGPL-3.0-or-later

#ifndef RF_UNIX_WORDS_H_
#define RF_UNIX_WORDS_H_

#define EXPORT_FUNCTION(fname) CONSTANT("FN-" #fname, FN_##fname, (uintptr_t) &(fname))

CONSTANT("O_RDONLY", C_O_RDONLY, O_RDONLY)
#undef  LINK
#define LINK C_O_RDONLY

CONSTANT("O_WRONLY", C_O_WRONLY, O_WRONLY)
#undef  LINK
#define LINK C_O_WRONLY

CONSTANT("O_CREAT", C_O_CREAT, O_CREAT)
#undef  LINK
#define LINK C_O_CREAT

CONSTANT("O_TRUNC", C_O_TRUNC, O_TRUNC)
#undef  LINK
#define LINK C_O_TRUNC

CONSTANT("O_APPEND", C_O_APPEND, O_APPEND)
#undef  LINK
#define LINK C_O_APPEND

EXPORT_FUNCTION(open)
#undef  LINK
#define LINK FN_open

EXPORT_FUNCTION(close)
#undef  LINK
#define LINK FN_close

EXPORT_FUNCTION(read)
#undef  LINK
#define LINK FN_read

EXPORT_FUNCTION(unlink)
#undef	LINK
#define	LINK FN_unlink

EXPORT_FUNCTION(write)
#undef  LINK
#define LINK FN_write

EXPORT_FUNCTION(exit)
#undef  LINK
#define LINK FN_exit

EXPORT_FUNCTION(opendir)
#undef  LINK
#define LINK FN_opendir

EXPORT_FUNCTION(closedir)
#undef  LINK
#define LINK FN_closedir

EXPORT_FUNCTION(readdir)
#undef  LINK
#define LINK FN_readdir

CONSTANT("dirent-d_name", DIRENT_D_NAME, offsetof(struct dirent, d_name))
#undef  LINK
#define LINK DIRENT_D_NAME

QNATIVE(ARG)	/* ( u -- addr count ) */
#undef  LINK
#define LINK ARG
	/* Behaves the same as the gforth word of the same name */
	uintptr_t u = POP().u;
	if (u >= sys_argc) {
		PUSH((cell_t) { .u = 0 });
		PUSH((cell_t) { .u = 0 });
	} else {
		PUSH((cell_t) { .p = (void *) sys_argv[u] });
		PUSH((cell_t) { .u = strlen(sys_argv[u]) });
	}
	NEXT();

CONSTANT("ARGC", ARGC, (uintptr_t) &sys_argc)
#undef  LINK
#define LINK ARGC

#endif /* RF_UNIX_WORDS_H_ */
