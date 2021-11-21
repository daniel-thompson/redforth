\ SPDX-License-Identifier: LGPL-3.0-or-later


: FILEID:CAT	( fileid -- )
	BEGIN
		HERE @ 1024 2 PICK READ-LINE
	0= WHILE
		DROP
		HERE @ SWAP TYPE
	REPEAT
	;

: APPEND-CHAR	( c-addr2 u2 c1 -- c-addr2 u2 )
	2 PICK !
	1+ SWAP 1+ SWAP
	;

: ENCODE-CHAR	( c-addr2 u2 c1 -- c-addr2 u2 )
	CASE
	[CHAR] \ OF
		[CHAR] \ APPEND-CHAR
		[CHAR] \ APPEND-CHAR
	ENDOF
	[CHAR] " OF
		[CHAR] \ APPEND-CHAR
		[CHAR] " APPEND-CHAR
	ENDOF
	'\n' OF
		[CHAR] \ APPEND-CHAR
		[CHAR] n APPEND-CHAR
	ENDOF
	
	( default case )
		APPEND-CHAR
		0		( dummy value to be dropped by ENDCASE )
	ENDCASE
	;

: ENCODE-STRING	( c-addr1 u1 c-addr2 -- u2 )
	SWAP		( c-addr1 c-addr2 u1 )
	0 SWAP		( c-addr1 c-addr2 0 u1 )
	0 DO		( c-addr1 c-addr2 u2 )
		2 PICK C@	( c-addr1 c-addr2 u2 c1 )
		ENCODE-CHAR	( c-addr1 c-addr2 u2 )
		ROT 1+ -ROT	
	LOOP
	-ROT 2DROP
	;

: ENCODE
	WORD

	( header )
	S\" S\" " TYPE
	2DUP TYPE
	S\" \" W/O CREATE-FILE DROP\n" TYPE


	( open file )
	R/O OPEN-FILE
	0<> IF
		." Bad filename" CR
		EXIT
	THEN

	( encode the file )
	BEGIN
		HERE @ 1024 2 PICK READ-LINE
				( ... u flag ior )
		0= -ROT		( ... success? u flag )
		DROP DUP 0>	( ... success? u not_eof? )
		ROT AND		( ... u good? )
	WHILE
		HERE @ SWAP
		2DUP +		( c-addr1 u1 c-addr2 )
		-ROT 2 PICK	( c-addr2 c-addr1 u1 c-addr2 )
		S\" S\\\" " TYPE
		ENCODE-STRING TYPE
		S\" \" 2 PICK WRITE-FILE DROP\n" TYPE
	REPEAT

	( footer )
	S\" CLOSE-FILE DROP\n" TYPE
	;
