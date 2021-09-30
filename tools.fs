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

( (WORDS) is the inner-loop for WORDS )
: (WORDS) ( nt -- flag )
	DUP ?HIDDEN NOT IF	( ignore hidden words )
		DUP ID.		( but if not hidden, print the word )
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
