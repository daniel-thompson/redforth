\ SPDX-License-Identifier: LGPL-3.0-or-later

: R/O [ LFS_O_RDONLY ] LITERAL ;
: W/O [ LFS_O_WRONLY LFS_O_CREAT OR ] LITERAL ;

: NEW-FILEID ( -- fileid )
	LFS_FILE_SIZE ALLOCATE
	0<> IF ABORT THEN
	;

: DELETE-FILEID ( fileid -- )
        FREE DROP
        ;

: (OPEN-FILE)   ( addr u mode -- fileid ior)
	-ROT CSTRING
        NEW-FILEID
        DUP >R
        LFS FN-lfs_file_open CCALL4
        R> OVER IF
                FREE	( this is an error so whatever FREE puts into
		          fileid is fine )
        THEN
        SWAP
        ;

: CREATE-FILE   ( addr u mode -- fileid ior )
        LFS_O_TRUNC OR
        (OPEN-FILE)
        ;

: OPEN-FILE     ( addr u mode -- filed ior )
        LFS_O_APPEND OR
        (OPEN-FILE)
        ;

: CLOSE-FILE	( fileid -- ior )
        DUP				( fileid fileid )
        LFS FN-lfs_file_close CCALL2	( fileid ior )
        SWAP OVER 0= IF			( ior fileid )
                FREE			( ior fior )
	THEN
	DROP				( ior )
        ;

: READ-FILE     ( c-addr u1 fileid -- u2 ior )
        -ROT SWAP ROT
        LFS FN-lfs_file_read CCALL4
        DUP 0< IF
                0 SWAP
        ELSE
                0
        THEN
        ;

: WRITE-FILE	( c-addr u fileid - ior )
	-ROT SWAP ROT
        LFS FN-lfs_file_write CCALL4
        DUP 0>= IF
                DROP 0
        THEN
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
