\ SPDX-License-Identifier: LGPL-3.0-or-later

( Standard FORTH provides some simple file access primitives which we model on
  top of Linux syscalls.

  The main complication is converting FORTH strings from c-addr u into a C
  string to hand to C functions.
)

: R/O Z" r" ;
: W/O Z" w" ;

: (OPEN-FILE)		( c-addr u mode -- fileid ior )
	-ROT CSTRING
	FN-fopen CCALL2
	DUP IF
		0
	ELSE
		-1
	THEN
	;

: CREATE-FILE		( c-addr u mode -- fileid ior )
	(OPEN-FILE)
	;

: OPEN-FILE		( c-addr u mode -- fileid ior )
	DUP C@
	[CHAR] w = IF
		DROP
		Z" a"
	THEN
	(OPEN-FILE)
	;

: CLOSE-FILE		( fileid -- ior )
	FN-fclose
	CCALL1
	0<>
;

: READ-FILE		( c-addr u1 fileid -- u2 ior )
	-ROT			( fileid c-addr u1 )
	SWAP			( fileid u1 c-addr )
	1 SWAP			( fileid u1 1 c-addr )
	FN-fread CCALL4		( u2 )
	DUP 0<			( u2 ior )
	;

: WRITE-FILE		( c-addr u fileid -- ior )
	-ROT			( fileid c-addr u1 )
	1 -ROT			( fileid 1 c-addr u1 )
	SWAP			( fileid 1 u1 c-addr )
	FN-fwrite CCALL4	( ior )
	1 <>			( ior )
	;
