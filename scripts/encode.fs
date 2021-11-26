#!/usr/bin/env redforth
\ SPDX-License-Identifier: LGPL-3.0-or-later

(
  Encode an ASCII file as Forth source.

  This allows us to copy files to Forth target devices such as the
  Raspberry Pi Pico.

  It has two operating modes. Firstly, it can be used directly to encode
  files. For example:

      picocom --send-cmd scripts/encode.fs /dev/ttyACM0

  Secondly, we can encode into a temporary Forth file to generates the
  expected source. This is useful for testing the encoder and to handle
  more exotic cases. For example:

      redforth include scripts/encode.fs \
               encode myscript.fs bye \
               > myscript.xfr
      picocom --send-cmd "ascii-xfr -sd" /dev/ttyACM0

  From that point we can transfer the file to a Rasperry Pi Pico by
  issuing MOUNT to Forth and C-x C-s to picocom.
)

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

: ENCODE-FILE
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

: ENCODE
	WORD ENCODE-FILE
	;

: MAIN
	ARGC @ IF
		ARGC @ 1 DO
			I ARG ENCODE-FILE
		LOOP
	THEN
	;
