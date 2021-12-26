\ SPDX-License-Identifier: LGPL-3.0-or-later

( Non-standard words that provide shell-ish features.

  Rather obviously these are inspired by historically "incorrect" sources
  such as the C library and common Unix tools. However they are useful,
  especially when working on freestanding systems such as microcontrollers.
)

: MORE-LINES?	( u2 flag ior -- u2 good? )
	0= -ROT		( ... success? u flag )
	DROP DUP 0>	( ... success? u not_eof? )
	ROT AND		( ... u good? )
	;

: CAT-FILEID	( fileid -- fileid )
	BEGIN
		HERE @ 1024 2 PICK READ-LINE
	MORE-LINES? WHILE
		HERE @ SWAP TYPE
	REPEAT
	DROP
	;

: CAT-FILE	( addr c -- )
	R/O OPEN-FILE
	DUP IF
		." Error: " . CR
		EXIT
	THEN DROP
	CAT-FILEID
	CLOSE-FILE DROP
	;

: CAT		( -- )
	WORD CAT-FILE
	;

: STRCHR	( addr c ch -- addr c )
	DUP 0 DO
			( addr c ch )
		2 PICK I + C@	( addr c ch ch2 )
		OVER = IF
				( addr c ch )
			DROP
			SWAP I + SWAP I -
			UNLOOP DROP EXIT
		THEN
	LOOP DROP
	;
