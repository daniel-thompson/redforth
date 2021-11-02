\ SPDX-License-Identifier: LGPL-3.0-or-later

( Standard FORTH provides some simple file access primitives which we model on
  top of Linux syscalls.

  The main complication is converting FORTH strings from c-addr u into a C
  string to hand to C functions.
)

: MODE_R Z" r" ;
: MODE_W Z" w" ;
: MODE_A Z" a" ;

: OPEN-FILE ( addr u cstr -- file (if successful) | addr u cstr -- 0 (if there was an error) )
	-ROT CSTRING
	FN-fopen
	CCALL2
	;

: CLOSE-FILE	( stream -- FALSE (if successful) | stream -- TRUE (if there was an error) )
	FN-fclose
	CCALL1
	0<>
;

: READ-FILE	( addr u file - FALSE (if successful) | addr u file -- TRUE (if there was an errro) )
	-ROT
	1 -ROT
	SWAP
	FN-fread
	CCALL4
	0=
;

: WRITE-FILE	( addr u file - FALSE (if successful) | addr u file -- TRUE (if there was an errro) )
	-ROT
	1 -ROT
	SWAP
	FN-fwrite
	CCALL4
	0=
;
