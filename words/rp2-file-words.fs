\ SPDX-License-Identifier: LGPL-3.0-or-later

: R/O [ LFS_O_RDONLY ] LITERAL ;
: W/O [ LFS_O_WRONLY LFS_O_CREAT OR ] LITERAL ;

: NEW-FILEID ( -- fileid )
        LFS_FILE_SIZE MALLOC
        ; 

: DELETE-FILEID ( fileid -- )
        FREE
        ;

: (OPEN-FILE)   ( addr u mode -- fileid ior)
	-ROT CSTRING
        NEW-FILEID
        DUP >R
        LFS FN-lfs_file_open CCALL4
        R> OVER IF
                FREE 0
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
        DUP
        LFS FN-lfs_file_close CCALL2
        SWAP OVER 0= IF
                FREE
        ELSE
                DROP
        THEN
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
