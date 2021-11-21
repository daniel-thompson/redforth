\ SPDX-License-Identifier: LGPL-3.0-or-later

: R/O O_RDONLY ;
: W/O O_WRONLY O_CREAT OR ;

: (OPEN-FILE)   ( addr u flags -- fileid ior)
	-ROT CSTRING
	511 -ROT		( a.k.a. 0777 )
        FN-open CCALL3 SX32
	DUP 0<
        ;

: CREATE-FILE   ( addr u flags -- fileid ior )
        O_TRUNC OR
        (OPEN-FILE)
        ;

: OPEN-FILE     ( addr u flags -- filed ior )
        O_APPEND OR
        (OPEN-FILE)
        ;

: CLOSE-FILE	( fileid -- ior )
	FN-close CCALL1 SX32
        ;

: READ-FILE     ( c-addr u1 fileid -- u2 ior )
        -ROT SWAP ROT
	FN-read CCALL3
        DUP 0< IF
		0 SWAP	( returned an error, zero bytes read )
        ELSE
		0	( success, but u2 could be zero if EOF )
        THEN
        ;

: WRITE-FILE	( c-addr u fileid - ior )
	-ROT SWAP ROT
	FN-write CCALL3
        DUP 0>= IF
                DROP 0
        THEN
        ;

: DELETE-FILE	( c-addr u -- ior )
	CSTRING
	FN-unlink CCALL1 SX32
	;

: INCLUDE	( -- )
	WORD R/O OPEN-FILE
	0= IF
		SOURCE-ID !
	THEN
	;

: READ-LINE-KEY	( fileid c-addr u1 u2 -- fileid c-addr u1 u2 ior)
	ROT		( fileid u1 u2 c-addr )
	DUP 1 5 PICK	( fileid u1 u2 c-addr c-addr 1 fileid )
	READ-FILE	( fileid u1 u2 c-addr 1 ior )
	DUP IF
		SWAP DROP	( fileid u1 u2 c-addr ior )
		>R -ROT R>	( fileid c-addr u1 u2 ior )
	ELSE
		DROP 0 = IF	( fileid u1 u2 c-addr )
			-ROT 0
			EXIT
		THEN
		1+ -ROT 1+	( fileid c-addr+1 u1 u2+1 )
		1		( fileid c-addr u1 u2 ior )
	THEN
	;

: READ-LINE-LF? ( c-addr u1 u2 -- c-addr u1 u2 bool )
	2 PICK 1- C@ 10 =
	;

: READ-LINE-EXIT ( fileid c-addr u1 u2 -- u2 TRUE 0 )
	-ROT 2DROP
	SWAP DROP
	TRUE 0
	;

: READ-LINE	( c-addr u1 fileid -- u2 flag ior )
	-ROT 0		( fileid c-addr u1 0 )
	BEGIN
		READ-LINE-KEY DUP 1 =
	WHILE		( fileid c-addr u1 u2 ior )
		DROP		( fileid c-addr u1 u2 )
		READ-LINE-LF?
		IF
			READ-LINE-EXIT
			EXIT
		THEN

		2DUP < IF
			READ-LINE-EXIT
			EXIT
		THEN
	REPEAT		( fileid c-addr u1 u2 ior )
	( READ-LINE-KEY TOS is 0 at EOF, 1 otherwise )
	DUP 0 > IF
		DROP
		0
	THEN
	>R
	-ROT 2DROP	( fileid u2 )
	SWAP DROP	( u2 )
	R>		( u2 ior )
	DUP 0= SWAP	( u2 flag ior )
	;

HIDE READ-LINE-KEY
HIDE READ-LINE-LF?
HIDE READ-LINE-EXIT
