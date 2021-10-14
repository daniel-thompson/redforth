\ SPDX-License-Identifier: LGPL-3.0-or-later

( Defines words found in the optional Programming-Tools word set.

  The following words from this word set are, for historical reasons,
  implemented in core.fs:

    BYE
    [DEFINED]
    DUMP
    .S
    FORGET
    SEE
    WORDS

  The following words from the standard are *not* currently implemented:

    AHEAD
    ASSEMBLER
    [ELSE]
    [IF]
    [THEN]
    [UNDEFINED]
    CODE
    CS-PICK
    CS-ROLL
    EDITOR
    NAME>COMPILE
    NAME>INTERPRET
    NAME>STRING
    NR>
    N>R
    SYNONYM
    ;CODE
    ?
)

( GET-CURRENT returns the compilation wordlist

  Strictly speaking GET-CURRENT is part of the optional Search-Order word set
  but currently we aren't really trying to implement that word set so we've
  dropped it here instead. We need it to implement WORDS and friends.
)
: GET-CURRENT ( -- wid )
	LATEST @
;


( [DEFINED] checks whether the string that follows is the name of a word

  TODO: Currently FIND differs from the ANS Forth so this implementation
        will not work on true ANS Forth systems.
)
: [DEFINED] IMMEDIATE	( -- flag )
	WORD FIND 0<>
;

( NAME>STRING converts the name token to a string.

  Name tokens are commonly used during TRAVERSE-WORDLIST and can be
  converted, using NAME>STRING, to the name of the word.
)
: NAME>STRING
	CELL+		( skip over the link pointer)
	DUP STRLEN      ( convert C-string to Forth string )
;

( TRAVERSE-WORDLIST runs an execution token for every word in the list.

  xt must make a specific modification to the top of the stack [ nt -- flag ].
  Additionally, because xt and wlist are already popped when xt is executed,
  xt is free to modify the stack as needed.
)
: TRAVERSE-WORDLIST ( xt wlist -- )
	2>R
	BEGIN
		2R@		( reload xt and link pointer )
		?DUP		( while not null )
	WHILE
		SWAP EXECUTE	( run xt )
		NOT IF
			2R> 2DROP
			EXIT
		THEN

		R> @ >R		( deref link pointer - go to previous word )
	REPEAT
	DROP
	2R> 2DROP
;

( (STRING>NAME) is the inner-loop for STRING>NAME )
: (STRING>NAME) ( c-addr u prev nt -- c-addr u prev TRUE | prev nt FALSE )
	DUP NAME>STRING
	5 PICK 5 PICK COMPARE
	0= IF		( got it )
		2SWAP 2DROP	( drop the search string )
		FALSE
	ELSE
		SWAP DROP	( update prev )
		TRUE
	THEN
;

: STRING>NAME ( c-addr u -- nt limit )
	HERE @ ' (STRING>NAME) GET-CURRENT TRAVERSE-WORDLIST
	( -- prev nt )
	DUP @		( follow link pointer )
	ROT MAX		( keep highest of prev and next )
;

( ITERATE-CODE steps through the data field of a DOCOL word.

  xt must make a specific modification to the top of the stack
  ( limit ip codeword -- limit ip ). xt may modify ip when decompiling
  words that require more than 1 cell.
)
: ITERATE-CODE ( xt nt limit -- )
	SWAP
	>DFA		( get the data address, ie. points after
	                  DOCOL | end-of-word start-of-data )

	BEGIN
		2DUP >
	WHILE
		DUP @		( limit ip codeword )
		3 PICK EXECUTE  ( limit ip )

		1 CELLS +
	REPEAT

	DROP 2DROP	( restore stack )
;

: STRCAT	( c-addr1 u1 c-addr2 u2 )
	3 PICK 3 PICK + 3 PICK 2SWAP
	0 DO
		DUP I + C@
		3 PICK I + C!
		SWAP 1+ SWAP
	LOOP
	DROP NIP NIP
;

: (?CFA)	( nt -- 0 FALSE | TRUE )
	>CFA OVER = IF
		0
		FALSE
	ELSE
		TRUE
	THEN

;

: ?CFA		( x -- x | 0 )
	( Due to the way (?CFA) indicates success/failure (via a conditional
	  0 on the stack) then we have to special case zero before we start
	)
	DUP 0= IF
		EXIT
	THEN
	' (?CFA) GET-CURRENT TRAVERSE-WORDLIST
	DUP 0= IF
		DROP
	ELSE
		DROP
		0
	THEN
;


( MANGLE changes a Forth word name in to a C-compatible symbol name )
: MANGLE	( c-addr1 u1 -- c-addr2 u2 )
	2DUP S" '" COMPARE 0= IF 2DROP S" TICK" THEN
	2DUP S" 1+" COMPARE 0= IF 2DROP S" ONEINCR" THEN
	2DUP S" 1-" COMPARE 0= IF 2DROP S" ONEDECR" THEN
	2DUP S" CELL+" COMPARE 0= IF 2DROP S" CELLINCR" THEN
	2DUP S" CELL-" COMPARE 0= IF 2DROP S" CELLDECR" THEN
	2DUP S" >R" COMPARE 0= IF 2DROP S" TOR" THEN
	2DUP S" R>" COMPARE 0= IF 2DROP S" FROMR" THEN
	2DUP S" >CFA" COMPARE 0= IF 2DROP S" TCFA" THEN
	2DUP S" >DFA" COMPARE 0= IF 2DROP S" TDFA" THEN
	2DUP S" -" COMPARE 0= IF 2DROP S" SUB" THEN
	2DUP S" -ROT" COMPARE 0= IF 2DROP S" NROT" THEN
	2DUP S" <>" COMPARE 0= IF 2DROP S" NEQU" THEN
	2DUP S" 0<>" COMPARE 0= IF 2DROP S" ZNEQU" THEN
	2DUP S" DOCOL" COMPARE 0= IF 2DROP S" C_DOCOL" THEN
	2DUP S" F_LENMASK" COMPARE 0= IF 2DROP S" C_F_LENMASK" THEN
	2DUP S" F_HIDDEN" COMPARE 0= IF 2DROP S" C_F_HIDDEN" THEN
	2DUP S" F_IMMED" COMPARE 0= IF 2DROP S" C_F_IMMED" THEN

	HERE @ 64 + 0

	3 PICK C@
	CASE
	[CHAR] 0 OF S" Z" STRCAT ENDOF
	[CHAR] 1 OF S" ONE" STRCAT ENDOF
	[CHAR] 2 OF S" TWO" STRCAT ENDOF
	( default )
		DROP
		2SWAP 1+ SWAP 1- SWAP 2SWAP	( pre-undo next character )
		0
	ENDCASE
	2SWAP 1- SWAP 1+ SWAP 2SWAP	( next character )

	BEGIN
		2 PICK 0>
	WHILE
		3 PICK C@
		CASE
		[CHAR] ! OF S" STORE"     STRCAT ENDOF
		[CHAR] " OF S" DQ"        STRCAT ENDOF
		[CHAR] ' OF S" QT"        STRCAT ENDOF
		[CHAR] ( OF S" LPAREN"    STRCAT ENDOF
		[CHAR] ) OF S" RPAREN"    STRCAT ENDOF
		[CHAR] * OF S" MUL"       STRCAT ENDOF
		[CHAR] + OF S" ADD"       STRCAT ENDOF
		[CHAR] , OF S" COMMA"     STRCAT ENDOF
		[CHAR] - OF S" _"         STRCAT ENDOF
		[CHAR] . OF S" DOT"       STRCAT ENDOF
		[CHAR] / OF S" DIV"       STRCAT ENDOF
		92 OF S" BS"        STRCAT ENDOF
		( TODO: This is only to handle leading 2 characters... better done in a leading if condition )
		[CHAR] : OF S" COLON"     STRCAT ENDOF
		[CHAR] ; OF S" SEMICOLON" STRCAT ENDOF
		[CHAR] < OF S" LT"        STRCAT ENDOF
		[CHAR] = OF S" EQU"       STRCAT ENDOF
		[CHAR] > OF S" GT"        STRCAT ENDOF
		[CHAR] ? OF S" Q"         STRCAT ENDOF
		[CHAR] @ OF S" FETCH"     STRCAT ENDOF
		[CHAR] [ OF S" LBRAC"     STRCAT ENDOF
		[CHAR] ] OF S" RBRAC"     STRCAT ENDOF
		[CHAR] ~ OF S" TILDE"     STRCAT ENDOF
		( default )
			DUP 3 PICK 3 PICK + C!
			SWAP 1+ SWAP
		ENDCASE

		2SWAP 1- SWAP 1+ SWAP 2SWAP	( next character )
	REPEAT

	( ... c-addr1+u1 0 c-addr2 u2 )
	2SWAP 2DROP
;

( STRQUOTE changes a Forth word name in to a C-compatible string )
: STRQUOTE	( c-addr1 u1 -- c-addr2 u2 )
	HERE @ 64 + 0
	BEGIN
		2 PICK 0>
	WHILE
		3 PICK C@
		CASE
		0 OF
			2DUP +
			92 OVER C!
			[CHAR] 0 SWAP 1+ C!
			2 +
		ENDOF
		[CHAR] " OF
			2DUP +
			92 OVER C!
			[CHAR] " SWAP 1+ C!
			2 +
		ENDOF
		92 OF
			2DUP +
			92 OVER C!
			92 SWAP 1+ C!
			2 +
		ENDOF
		( default )
			DUP 3 PICK 3 PICK + C!
			SWAP 1+ SWAP
		ENDCASE

		2SWAP 1- SWAP 1+ SWAP 2SWAP	( next character )
	REPEAT

	( ... c-addr1+u1 0 c-addr2 u2 )
	2SWAP 2DROP
;

( (>ROM) is the inner loop for SEE and it's roll is to decompiles the
  codeword.

  In order to handle immediate values (LIT, LITSTRING and ') we may
  modify the value of ip in order to skip any embedded immediates.
)
: (>ROM)	( limit ip codeword -- limit ip )
	CASE
	0 OF			( C builtins may have 0 termination...
	                          ignore this!
				)
	ENDOF
	' LIT OF		( is it LIT ? )
		1 CELLS + DUP @		( get next word which is the
		                          integer constant )
		( Normally codeword literalss are compiled with a ' . In fact
		  both LIT and ' are implemented the same in the VM! So just in
		  case we'll do codeword detection on LIT as well!
		)
		DUP ?CFA IF
			." 	COMPILE_TICK(" CFA> NAME>STRING MANGLE TYPE ." )" CR	( and print it )
		ELSE DUP DOCOL = IF
				." 	COMPILE_LIT(&&DOCOL)" CR
				DROP
		ELSE
			." 	COMPILE_LIT(" ( 0 .R ) . ." )" CR	( and print it )
		THEN THEN
	ENDOF
	' LITSTRING OF		( is it LITSTRING ? )

		." 	COMPILE_LITSTRING(" [CHAR] " EMIT
		1 CELLS + DUP @		( get the length word )
		SWAP 1 CELLS + SWAP	( end start+4 length )
		2DUP STRQUOTE TYPE	( print the string )
		[CHAR] " EMIT ." )" CR	( finish the string with a
		                          final quote )
		+ ALIGNED		( end start+4+len, aligned )
		1 CELLS -		( because we're about to add 4 below )
	ENDOF
	' 0BRANCH OF		( is it 0BRANCH ? )
		." 	COMPILE_0BRANCH("
		1 CELLS + DUP @		( print the offset )
		1 CELLS /
		0 .R  ." )" CR
	ENDOF
	' BRANCH OF		( is it BRANCH ? )
		." 	COMPILE_BRANCH("
		1 CELLS + DUP @		( print the offset )
		1 CELLS /
		0 .R  ." )" CR
	ENDOF
	' ' OF			( is it ' (TICK) ? )
		1 CELLS + DUP @		( get the next codeword )
		." 	COMPILE_TICK(" CFA> NAME>STRING MANGLE TYPE ." )" CR	( and print it )
	ENDOF
	' EXIT OF		( is it EXIT? )
		( We expect the last word to be EXIT, and if it is then
		  we don't print it because EXIT is normally implied by
		  ;.  EXIT can also appear in the middle of words, and
		  then it needs to be printed.
		)

		2DUP			( end start end start )
\	TODO:
\         This code doesn't work for built-in words which have a terminating
\         NULL value on variable length arrays. The replacement code below
\         also hides exit when it is the last-but-one word (since there could
\         be nothing useful in the final slot).
\		1 CELLS +		( end start end start+4 )
\		<> IF			( end start | we're not at the end )
		2 CELLS +
		> IF
			." 	COMPILE(EXIT)" CR
		THEN
	ENDOF
				( default case: )
		DUP			( in the default case we always need to DUP before using )
		CFA> NAME>STRING MANGLE	( look up the codeword to get the dictionary name )
		." 	COMPILE(" TYPE ." )" CR
	ENDCASE
;



( >ROM converts a Forth word into something the C compiler can absorb!

  We cannot recover the full source because the control structures have been
  replaced with branches. However it is sufficient to allow us to reconstruct
  an equivalent.
)
: >ROM ( nt limit -- )

	( begin the definition with the BUILTIN_FLAGS(C_NAME, "WORDNAME", <flags>) macro )
	." BUILTIN_FLAGS("
	OVER NAME>STRING MANGLE TYPE
	." , " [CHAR] " EMIT OVER NAME>STRING STRQUOTE TYPE [CHAR] " EMIT
	." , 0"
	OVER ?IMMEDIATE IF ."  | F_IMMED" THEN
	OVER ?HIDDEN    IF ."  | F_HIDDEN" THEN
	." )" CR

	." #undef  LINK" CR
	." #define LINK " OVER NAME>STRING MANGLE TYPE CR

	' (>ROM) -ROT
	ITERATE-CODE

	( finalize and we are done )
	." 	COMPILE_EXIT()" CR CR
;

: (LATEST>ROM)	( nt -- flag )
	DUP
	BUILTIN-WORDS = IF
		DROP
		FALSE
	ELSE
		TRUE
	THEN
;

( LATEST>ROM walks the word list encoding anything that is not builtin )
: LATEST>ROM
	0
	' (LATEST>ROM) GET-CURRENT TRAVERSE-WORDLIST
	BEGIN
		DUP 0<>
	WHILE
		NAME>STRING STRING>NAME >ROM
	REPEAT
	DROP
;

( (SEE) is the inner loop for SEE and it's roll is to decompiles the
  codeword.

  In order to handle immediate values (LIT, LITSTRING and ') we may
  modify the value of ip in order to skip any embedded immediates.
)
: (SEE)		( limit ip codeword -- limit ip )
	CASE
	0 OF			( C builtins may have 0 termination...
	                          ignore this!
				)
	ENDOF
	' LIT OF		( is it LIT ? )
		1 CELLS + DUP @		( get next word which is the
		                          integer constant )
		.			( and print it )
	ENDOF
	' LITSTRING OF		( is it LITSTRING ? )
		[ CHAR S ] LITERAL EMIT '"' EMIT SPACE ( print S"<space> )
		1 CELLS + DUP @		( get the length word )
		SWAP 1 CELLS + SWAP		( end start+4 length )
		2DUP TYPE		( print the string )
		'"' EMIT SPACE		( finish the string with a
		                          final quote )
		+ ALIGNED		( end start+4+len, aligned )
		1 CELLS -		( because we're about to add 4 below )
	ENDOF
	' 0BRANCH OF		( is it 0BRANCH ? )
		." 0BRANCH ( "
		1 CELLS + DUP @		( print the offset )
		.
		." ) "
	ENDOF
	' BRANCH OF		( is it BRANCH ? )
		." BRANCH ( "
		1 CELLS + DUP @		( print the offset )
		.
		." ) "
	ENDOF
	' ' OF			( is it ' (TICK) ? )
		[ CHAR ' ] LITERAL EMIT SPACE
		1 CELLS + DUP @		( get the next codeword )
		CFA>			( and force it to be printed as a dictionary entry )
		ID. SPACE
	ENDOF
	' EXIT OF		( is it EXIT? )
		( We expect the last word to be EXIT, and if it is then
		  we don't print it because EXIT is normally implied by
		  ;.  EXIT can also appear in the middle of words, and
		  then it needs to be printed.
		)

		2DUP			( end start end start )
\	TODO:
\         This code doesn't work for built-in words which have a terminating
\         NULL value on variable length arrays. The replacement code below
\         also hides exit when it is the last-but-one word (since there could
\         be nothing useful in the final slot).
\		1 CELLS +		( end start end start+4 )
\		<> IF			( end start | we're not at the end )
		2 CELLS +
		> IF
			." EXIT "
		THEN
	ENDOF
				( default case: )
		DUP			( in the default case we always need to DUP before using )
		CFA>			( look up the codeword to get the dictionary entry )
		ID. SPACE		( and print it )
	ENDCASE
;

( SEE decompiles a FORTH word.

  Using the name token and limit from STRING>NAME we can start decompiling
  the word starting from the data field area until we reach the limit.
)
: SEE	( -- )
	' (SEE)
	WORD STRING>NAME
			( nt limit === start-of-word end-of-word )

	( begin the definition with : NAME [IMMEDIATE] )
	[CHAR] : EMIT SPACE
	OVER NAME>STRING TYPE SPACE
	OVER ?IMMEDIATE IF ." IMMEDIATE " THEN

	ITERATE-CODE

	( finalize and we are done )
	[CHAR] ; EMIT CR
;

( (WORDS) is the inner-loop for WORDS )
: (WORDS) ( nt -- flag )
	DUP ?HIDDEN NOT IF	( ignore hidden words )
		DUP NAME>STRING TYPE ( print the word )
		SPACE
	THEN
	DROP
	TRUE
;

( WORDS prints all the words defined in the dictionary.

  The list starts with the most recently defined word and will not include
  hidden words.
)
: WORDS ( -- )
	' (WORDS) GET-CURRENT TRAVERSE-WORDLIST CR
;
