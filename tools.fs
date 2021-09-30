\ SPDX-License-Identifier: LGPL-3.0-or-later

( Defines words found in the optional Programming-Tools word set.

  The following words from this word set are, for historical reasons,
  implemented in core.fs:

    BYE
    DUMP
    .S
    FORGET
    SEE
    WORDS

  The following words from the standard are *not* currently implemented:

    AHEAD
    ASSEMBLER
    [DEFINED]
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

( (WORDS) is the inner-loop for WORDS )
: (WORDS) ( nt -- flag )
	DUP ?HIDDEN NOT IF	( ignore hidden words )
		DUP NAME>STRING TYPE ( print the word )
		SPACE
	THEN
	DROP
	TRUE
;


( SEE decompiles a FORTH word.

  Using the name token and limit from STRING>NAME we can start decompiling
  the word starting from the data field area until we reach the limit.
)
: SEE	( -- )
	WORD STRING>NAME
			( nt limit === start-of-word end-of-word )
	SWAP		( end-of-word start-of-word )

	( begin the definition with : NAME [IMMEDIATE] )
	[CHAR] : EMIT SPACE DUP ID. SPACE
	DUP ?IMMEDIATE IF ." IMMEDIATE " THEN

	>DFA		( get the data address, ie. points after
	                  DOCOL | end-of-word start-of-data )

	( now we start decompiling until we hit the end of the word )
	BEGIN		( end start )
		2DUP >
	WHILE
		DUP @		( end start codeword )

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
\			1 CELLS +		( end start end start+4 )
\			<> IF			( end start | we're not at the end )
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

		1 CELLS +	( end start+4 )
	REPEAT

	[CHAR] ; EMIT CR

	2DROP		( restore stack )
;

( WORDS prints all the words defined in the dictionary.

  The list starts with the most recently defined word and will not include
  hidden words.
)
: WORDS ( -- )
	' (WORDS) GET-CURRENT TRAVERSE-WORDLIST CR
;
