\ SPDX-License-Identifier: LGPL-3.0-or-later

( Non-standardized words that can be useful for debugging )

(
	PRINTING THE DICTIONARY ----------------------------------------------------------------------

	ID. takes an address of a dictionary entry and prints the word's name.

	For example: LATEST @ ID. would print the name of the last word that was defined.
)
: ID.
	CELL+		( skip over the link pointer)
	DUP STRLEN      ( convert C-string to Forth string )
	TYPE
;

(
	'WORD word FIND ?HIDDEN' returns true if 'word' is flagged as hidden.

	'WORD word FIND ?IMMEDIATE' returns true if 'word' is flagged as immediate.
)
: ?HIDDEN
	>CFA 1-		( skip over the link pointer and name )
	C@		( get the flags/length byte )
	F_HIDDEN AND	( mask the F_HIDDEN flag and return it (as a truth value) )
;
: ?IMMEDIATE
	>CFA 1-		( skip over the link pointer and name )
	C@		( get the flags/length byte )
	F_IMMED AND	( mask the F_IMMED flag and return it (as a truth value) )
;

( Print a stack trace by walking up the return stack. )
: PRINT-STACK-TRACE
	RSP@				( start at caller of this function )
	BEGIN
		DUP R0 CELL- <		( RSP < R0 )
	WHILE
		DUP @			( get the return stack entry )
		CASE
		' EXCEPTION-MARKER CELL+ OF	( is it the exception stack frame? )
			." CATCH ( DSP="
			CELL+ DUP @ U.		( print saved stack pointer )
			." ) "
		ENDOF
						( default case )
			DUP
			DFA>			( look up the address to get the dictionary entry )
			?DUP IF			( and print it )
				2DUP			( dea addr dea )
				ID.			( print word from dictionary entry )
				[CHAR] + EMIT
				SWAP >DFA CELL+ - .	( print offset )
			THEN
		ENDCASE
		CELL+			( move up the stack )
	REPEAT
	DROP
	CR
;
