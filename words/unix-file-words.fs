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
                0 SWAP
        ELSE
                0
        THEN
        ;

: WRITE-FILE	( c-addr u fileid - ior )
	-ROT SWAP ROT
	FN-write CCALL3
        DUP 0>= IF
                DROP 0
        THEN
        ;
