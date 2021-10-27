\ SPDX-License-Identifier: MIT

VARIABLE ERROR_COUNT
0 ERROR_COUNT !

( ASSERT verifis that the provided condition, n, is TRUE )
: ASSERT ( n -- )
	0= IF
		1 ERROR_COUNT +!

		."  FAILED
"
		." <" DEPTH 1 CELLS / . ." >  " .S CR EMIT
		ABORT
	THEN
;

( SELFTEST compares two sequences on the stack and asserts that they are
  identical.
)
: SELFTEST ( N xN..x0 yN..y0 -- )
	DUP 0 DO
		DUP 1 + PICK	( yN N xN ... )
		ROT		( xN yN N )
		= ASSERT	( N xN+1 ... )
	LOOP
	0 DO
		DROP
	LOOP
;

( T{ starts an equivalence test by pushing control data to the return stack.

  T{ is the fist part of a T{ <SEQ1> -> <SEQ2> }T equivalence test. Such a
  test checks that the data stack effect of SEQ1 and SEQ2 are the same.

  Examples:
    T{ 1 2 + -> 3 }T
)
: T{
	." ."		( Record test suite progress )
	DEPTH		( Capture the current stack depth )
	R> SWAP >R >R	( Write is out to the return stack )
;

( -> is the centre part of an equivalence test.

  The code is the same as T{ but we do not share any code between the two
  because that would complicate the return stack management. When we add the
  control data we *must* preserve our own parent on the stack otherwise
  we will crash when we EXIT.
)
: ->
	DEPTH R> SWAP >R >R
;

( }T completes an equivalence test.

  Fetch the control data from the stack. Verify both sequences have the same
  stack depth, then launch a SELFTEST. All test data will be dropped from the
  stack.
)
: }T
	DEPTH
	R> R> R> ROT >R
	SWAP DUP ROT
	-
	-ROT -
	DUP ROT
	= ASSERT
	1 CELLS /
	DUP IF
		SELFTEST
	ELSE
		DROP
	THEN
;

( OK prints ok to the terminal to demonstrate test progress )
: OK ."  ok" CR ;


( Hello, good evening and welcome! )
CR CR


( --------------------------------------------------------------------------- )
." Test suite self tests "

( Check that a nop matches another nop... and that there is no stack leakage )
DEPTH CELL+
T{ -> }T
DEPTH = ASSERT

DEPTH CELL+
T{ 1002 1001 1000 -> 1002 1001 1000 }T
DEPTH = ASSERT

T{ DEPTH -> 0 }T

( Anti-tests:
  We can't run these yet but if we need to check that T{ }T correctly
  detect problems then we need to check these report errors. )
\ T{ 1 -> }T
\ T{ -> 1 }T
\ T{ 1 -> 0 }T
\ T{ 1 1 -> 1 0 }T
\ T{ 1 1 1 -> 1 0 1 }T

OK


( --------------------------------------------------------------------------- )
." Stack primitive tests "

T{ 1 2 3 DROP -> 1 2     }T
T{ 1 2 3 SWAP -> 1 3 2   }T
T{ 1 2 3 DUP  -> 1 2 3 3 }T
T{ 1 2 3 OVER -> 1 2 3 2 }T
T{ 1 2 3 ROT  -> 2 3 1   }T
T{ 1 2 3 -ROT -> 3 1 2   }T

T{ 1 2 3 4 2DROP -> 1 2     }T
T{ 1 2 3 4 2SWAP -> 3 4 1 2 }T

T{ 1 2 3 ?DUP -> 1 2 3 3 }T
T{ 1 2 0 ?DUP -> 1 2 0   }T

T{ 1 2 3 1+   -> 1 2 4   }T
T{ 1 2 3 1-   -> 1 2 2   }T

T{ 0 CELL+ CELL- -> 0 }T
\ CELL+ should increment by with 4 (32-bit cells) or 8 (64-bit cells)
T{ 0 CELL+ DUP 4 = SWAP 8 = OR -> TRUE }T

OK

( --------------------------------------------------------------------------- )
." Arithmetic tests "
T{ 1 2 + -> 3 }T
T{ 1 2 - -> -1 }T
T{ 9 9 * -> 81 }T
T{ 101 5 /MOD -> 1 20 }T
OK

( --------------------------------------------------------------------------- )
." Comparison tests "
\ These are copied from jonesforth/test_comparison.f

T{  1  0 < -> FALSE }T
T{  0  1 < -> TRUE  }T
T{  1 -1 < -> FALSE }T
T{ -1  1 < -> TRUE  }T
T{ -1  0 < -> TRUE  }T
T{  0 -1 < -> FALSE }T

T{  1  0 > -> TRUE  }T
T{  0  1 > -> FALSE }T
T{  1 -1 > -> TRUE  }T
T{ -1  1 > -> FALSE }T
T{ -1  0 > -> FALSE }T
T{  0 -1 > -> TRUE  }T

T{  1  1 <= -> TRUE  }T
T{  0  0 <= -> TRUE  }T
T{ -1 -1 <= -> TRUE  }T
T{  1  0 <= -> FALSE }T
T{  0  1 <= -> TRUE  }T
T{  1 -1 <= -> FALSE }T
T{ -1  1 <= -> TRUE  }T
T{ -1  0 <= -> TRUE  }T
T{  0 -1 <= -> FALSE }T

T{  1  1 >= -> TRUE  }T
T{  0  0 >= -> TRUE  }T
T{ -1 -1 >= -> TRUE  }T
T{  1  0 >= -> TRUE  }T
T{  0  1 >= -> FALSE }T
T{  1 -1 >= -> TRUE  }T
T{ -1  1 >= -> FALSE }T
T{ -1  0 >= -> FALSE }T
T{  0 -1 >= -> TRUE  }T

T{  1  1 = -> TRUE  }T
T{  1  0 = -> FALSE }T
T{  0  0 = -> TRUE  }T
T{  1 -1 = -> FALSE }T
T{ -1 -1 = -> TRUE  }T

T{  1  1 <> -> FALSE }T
T{  1  0 <> -> TRUE  }T
T{  0  0 <> -> FALSE }T
T{  1 -1 <> -> TRUE  }T
T{ -1 -1 <> -> FALSE }T

T{  1 0= -> FALSE }T
T{  0 0= -> TRUE  }T
T{ -1 0= -> FALSE }T

T{  1 0<> -> TRUE  }T
T{  0 0<> -> FALSE }T
T{ -1 0<> -> TRUE  }T

T{  1 0< -> FALSE }T
T{  0 0< -> FALSE }T
T{ -1 0< -> TRUE  }T

T{  1 0> -> TRUE  }T
T{  0 0> -> FALSE }T
T{ -1 0> -> FALSE }T

T{  1 0<= -> FALSE }T
T{  0 0<= -> TRUE  }T
T{ -1 0<= -> TRUE  }T

T{  1 0>= -> TRUE  }T
T{  0 0>= -> TRUE  }T
T{ -1 0>= -> FALSE }T

OK


( --------------------------------------------------------------------------- )
." Logic operations "
T{ 31 15 AND -> 15 }T
T{ 29 15 AND -> 13 }T
T{ 31 15 OR  -> 31 }T
T{ 29 15 OR  -> 31 }T
T{ 31 15 XOR -> 16 }T
T{ 29 15 XOR -> 18 }T
T{ -1 INVERT ->  0 }T
T{ -2 INVERT ->  1 }T
OK


( --------------------------------------------------------------------------- )
." Memory accessors "
T{ 17 DSP@                   @ -> 17 17    }T
T{ 20 21 DSP@ CELL+          @ -> 20 21 20 }T
T{ 17 DSP@ 21 SWAP           ! -> 21       }T
T{ 20 21 DSP@ CELL+ 17 SWAP  ! -> 17 21    }T
T{ 17 DSP@ 3 SWAP           +! -> 20       }T
T{ 17 DSP@ 3 SWAP           -! -> 14       }T

\ Only works on little-endian platforms
T{ 260 DSP@                 C@ -> 260 4    }T
T{ 260 DSP@ 1+              C@ -> 260 1    }T

\ Only works on little-endian platforms
T{ 260 DSP@ 8 SWAP          C! -> 264      }T
T{ 260 DSP@ 1+ 2 SWAP       C! -> 516      }T

T{ 6 9 42 DSP@ DUP CELL+ 1 CELLS CMOVE -> 6 42 42 }T
T{ 6 9 42 0 DSP@ DUP CELL+ SWAP 3 CELLS CMOVE -> 6 6 9 42 }T
OK


( --------------------------------------------------------------------------- )
." Built-in variables "
HERE @ \ Capture HERE before we define VARTEST:AT
: VARTEST:AT IMMEDIATE @ ;

T{ STATE @ -> 0 }T
T{ STATE ] VARTEST:AT [ -> 1 }T

T{ DUP HERE @ < -> TRUE }T

\ Check that the new LATEST is equal to the old HERE
T{ DUP LATEST @ = -> TRUE }T

\ This is a fairly weak check (it just checks that DSP points to a lower address than S0)
T{ S0 @ DSP@ > -> TRUE }T

T{ BASE @ -> 10 }T
T{ 20 16 BASE ! 20 A BASE ! -> 20 32 }T
T{ BASE @ -> 10 }T

DROP

OK


( --------------------------------------------------------------------------- )
." Constant loading "
T{ R0 RSP@ > -> TRUE }T
T{ VERSION -> 47 }T
\ DOCOL gives the same value as extracting the codeword from a builtin
T{ DOCOL -> S" QUIT" FIND >CFA @ }T
T{ F_IMMED 0<> -> TRUE }T
T{ F_IMMED DUP 1- AND -> 0 }T
T{ F_HIDDEN 0<> -> TRUE }T
T{ F_HIDDEN DUP 1- AND -> 0 }T
T{ F_LENMASK 0<> -> TRUE }T
OK


( --------------------------------------------------------------------------- )
." Return stack manipulation "
T{ 10 >R R> -> 10 }T
T{ 10 >R RSP@ @ R> -> 10 10 }T
T{ 10 >R 100 RSP@ ! R> -> 100 }T
T{ RSP@ DUP RSP! RSP@ = -> TRUE }T
\ Temporarily use HERE as the RSP so we can prove the store worked
T{ RSP@ HERE @ CELL+ RSP! 42 >R RSP! HERE @ @ -> 42 }T
T{ 2 >R 1 >R RDROP R> -> 2 }T
OK


( --------------------------------------------------------------------------- )
." Data stack load/store "
T{ 1000 1001 DSP@ @ -> 1000 1001 1001 }T
T{ 1000 1001 DSP@ CELL+ DSP! -> 1000 }T
OK


( --------------------------------------------------------------------------- )
." Input handling "
T{ KEY A -> 65 }T
T{ KEY A1000 -> 65 1000 }T
T{ WORD ABC SWAP DROP -> 3 }T
: WORDTEST WORD SWAP ROT + C@ ;
T{ 0 WORDTEST ABC -> 3 65 }T
T{ 2 WORDTEST ABC -> 3 67 }T
T{ S" 0" NUMBER -> 0 0 }T
T{ S" 10" NUMBER -> 10 0 }T
T{ S" -10" NUMBER -> -10 0 }T
T{ S" 20" 16 BASE ! NUMBER A BASE ! -> 32 0 }T
T{ S" 2A" 16 BASE ! NUMBER A BASE ! -> 42 0 }T
T{ S" 2147483647" NUMBER -> 2147483647 0 }T
T{ S" -2147483648" NUMBER -> -2147483648 0 }T
T{ S" 10A" NUMBER -> 10 1 }T
FORGET WORDTEST
OK

( --------------------------------------------------------------------------- )
." Dictionary lookups "
T{ S" +" FIND 0<> -> TRUE }T
T{ S" QUIT" FIND 0<> -> TRUE }T
T{ S" NONSENSE" FIND -> 0 }T
\ Compare codewords of primitive vs. builtin vs. normal
T{ S" +"    FIND >CFA @ DOCOL = -> FALSE }T
T{ S" QUIT" FIND >CFA @ DOCOL = -> TRUE }T
T{ S" T{"   FIND >CFA @ DOCOL = -> TRUE }T
T{ S" +" FIND >DFA -> S" +" FIND >CFA CELL+ }T

(
  There are no tests for the following words. All are necessary for the
  test suite to operate and have already been tested (albeit without
  explicit error reporting when it breaks):

  :
  CREATE
  ,
  ;
  IMMEDIATE
  '
  BRANCH
  0BRANCH
  LIT
  LITSTRING
  INTERPRET

  In addition we already tests LATEST, [ and ] in earlier variable
  tests.
)

: HIDEME FALSE ;
: HIDEME TRUE ;
S" HIDEME" FIND
T{ HIDEME -> TRUE }T
HIDE HIDEME
T{ HIDEME -> FALSE }T
HIDDEN
T{ HIDEME -> TRUE }T
FORGET HIDEME
FORGET HIDEME

OK


( --------------------------------------------------------------------------- )
." Odds and ends "
T{ CHAR A -> 65 }T
T{ CHAR 9 CHAR 0 - -> 9 0 - }T
T{ 9 9 S" *" FIND >CFA EXECUTE -> 81 }T
OK

." Decompiler tools "
T{ S" HIDE" FIND >CFA CFA> -> S" HIDE" FIND  }T
T{ S" HIDE" FIND >DFA DFA> -> S" HIDE" FIND  }T
OK


( --------------------------------------------------------------------------- )
." File read/write ..."
S" doesnotexist" MODE_R OPEN-FILE 0= ASSERT

VARIABLE OUTPUT_FILE
S" filetest.txt" MODE_W OPEN-FILE DUP OUTPUT_FILE ! 0<> ASSERT
S" CHUNK#1 " OUTPUT_FILE @ WRITE-FILE 0= ASSERT
S" CHUNK#2 " OUTPUT_FILE @ WRITE-FILE 0= ASSERT
S" CHUNK#3 " OUTPUT_FILE @ WRITE-FILE 0= ASSERT
OUTPUT_FILE @ CLOSE-FILE 0= ASSERT
FORGET OUTPUT_FILE

VARIABLE OUTPUT_FILE
S" filetest.txt" MODE_A OPEN-FILE DUP OUTPUT_FILE ! 0<> ASSERT
S" CHUNK#4 " OUTPUT_FILE @ WRITE-FILE 0= ASSERT
OUTPUT_FILE @ CLOSE-FILE 0= ASSERT
FORGET OUTPUT_FILE

VARIABLE INPUT_FILE
S" filetest.txt" MODE_R OPEN-FILE DUP INPUT_FILE ! 0<> ASSERT
HERE @ 8 INPUT_FILE @ READ-FILE 0= ASSERT
HERE @ 6 + C@ CHAR 1 = ASSERT
HERE @ 8 INPUT_FILE @ READ-FILE 0= ASSERT
HERE @ 6 + C@ CHAR 2 = ASSERT
HERE @ 8 INPUT_FILE @ READ-FILE 0= ASSERT
HERE @ 6 + C@ CHAR 3 = ASSERT
HERE @ 8 INPUT_FILE @ READ-FILE 0= ASSERT
HERE @ 6 + C@ CHAR 4 = ASSERT
INPUT_FILE @ CLOSE-FILE 0= ASSERT
FORGET INPUT_FILE

OK

." Memory allocation ..."
32 MALLOC DUP 0<> ASSERT
\ This is a crass "does it crash test"
DUP 31 + 31 SWAP C!
FREE
OK


( --------------------------------------------------------------------------- )
." Conditional compilation ... "

;SCAN will simply ignore every word entered; until we get a freestanding ;

:<> F1 100 ;
T{ F1 -> 100 }T
:<> F1 200 ;
T{ F1 -> 100 }T
FORGET F1
OK


( --------------------------------------------------------------------------- )
." Return stack double word "

T{ 1 >R 2 >R 2R> -> 1 2 }T
T{ 1 >R 2 >R 2R@ R> DROP R> DROP -> 1 2 }T
T{ 1 2 2>R R> R> -> 2 1 }T
OK

( --------------------------------------------------------------------------- )
." Other core words "

( ABS )
T{           1 ABS ->          1 }T
T{           0 ABS ->          0 }T
T{          -1 ABS ->          1 }T
T{  2147483647 ABS -> 2147483647 }T
T{ -2147483647 ABS -> 2147483647 }T

( ACCEPT )
: HELLO S" Hello" ;
T{ HERE @ 16 ACCEPT Hello
HERE @ SWAP HELLO COMPARE 0= -> TRUE }T
T{ HERE @ 4 ACCEPT Hello, world
HERE @ SWAP HELLO COMPARE 0= -> FALSE }T
T{ HERE @ 5 ACCEPT Hello, world
HERE @ SWAP HELLO COMPARE 0= -> TRUE }T
T{ HERE @ 6 ACCEPT Hello, world
HERE @ SWAP HELLO COMPARE 0= -> FALSE }T
FORGET HELLO

( [COMPILE] )
: IGNORE-TO-CR [COMPILE] \ ;
T{ 0 IGNORE-TO-CR 1+ 1 1 + 2 + 3
-> 0 }T
FORGET IGNORE-TO-CR

T{  1  2 MAX ->  2 }T
T{ -1  2 MAX ->  2 }T
T{  2  1 MAX ->  2 }T
T{  2 -1 MAX ->  2 }T
T{ -1 -2 MAX -> -1 }T

T{  1  2 MIN ->  1 }T
T{ -1  2 MIN -> -1 }T
T{  2  1 MIN ->  1 }T
T{  2 -1 MIN -> -1 }T
T{ -1 -2 MIN -> -2 }T

: "LEFT S" LEFT" ;
: "LONG S" LONG" ;
: "LONGING S" LONGING" ;
: "RIGHT S" RIGHT" ;
T{ "LEFT "RIGHT COMPARE -> -1 }T
T{ "LEFT "LEFT COMPARE -> 0 }T
T{ "LONG "LEFT COMPARE -> 1 }T
T{ "LONG "LONGING COMPARE -> -1 }T
T{ "LONGING "LONG COMPARE -> 1 }T
FORGET "LEFT

T{ 1 2 3 0 PICK 2 PICK 4 PICK -> 1 2 3 3 2 1 }T

OK


( --------------------------------------------------------------------------- )
." Programming tools tests "

T{ GET-CURRENT -> LATEST @ }T

0 [IF]
	0 ASSERT
[THEN]
1 [IF]
	1 ASSERT
[THEN]
0 [IF]
	0 ASSERT
[ELSE]
	1 ASSERT
[THEN]
1 [IF]
	1 ASSERT
[ELSE]
	0 ASSERT
[THEN]

: TOOLS1 [DEFINED] GET-CURRENT LITERAL ;
T{ [DEFINED] GET-CURRENT -> TRUE }T
T{ [DEFINED] TOOLS1 -> TRUE }T
T{ TOOLS1 -> TRUE }T
FORGET TOOLS1
T{ [DEFINED] TOOLS1 -> FALSE }T

: TOOLS2 ;
T{ GET-CURRENT NAME>STRING S" TOOLS2" COMPARE 0= -> TRUE }T
T{ GET-CURRENT @ NAME>STRING S" TOOLS2" COMPARE 0= -> FALSE }T
FORGET TOOLS2

( Two different ways to count the number of defined words... one of which uses
  TRAVERSE-WORDLIST, which is what we are testing
)
: TOOLS3 ( x nt -- x+1 TRUE ) DROP 1+ TRUE ;
: TOOLS4 ( -- x ) 0 ' TOOLS3 GET-CURRENT TRAVERSE-WORDLIST ;
: TOOLS5 ( -- x ) 0 LATEST @ BEGIN DUP 0<> WHILE SWAP 1+ SWAP @ REPEAT DROP ;
T{ TOOLS4 -> TOOLS5 }T
FORGET TOOLS3

( STRING>NAME is a vendor extension and has a rather odd interface that pushes
  nt and limit to the stack.
)
T{ S" +" STRING>NAME 0<> -> TRUE }T
T{ S" +" 2DUP STRING>NAME NAME>STRING COMPARE 0= -> TRUE }T
( does an unknown word yeild 0 )
T{ S" +++" STRING>NAME 0= -> TRUE }T

: TOOLS6 ( count ip codeword -- count ip ) DROP SWAP 1+ SWAP ;
: TOOLS7 ' TOOLS6 S" TOOLS6" STRING>NAME ITERATE-CODE ;
: TOOLS8 ' TOOLS6 S" SEE" STRING>NAME ITERATE-CODE ;
T{ 0 TOOLS7 -> 5 ( the four obvious words and EXIT ) }T
T{ 0 TOOLS8 0<> -> TRUE ( this is a does it crash test ;- ) }T
FORGET TOOLS6

: TOOLS9 S" Fabulous" ;
: TOOLS10 S" Absolutely " ;
: TOOLS11 S" Absolutely Fabulous" ;
T{ HERE @ 0 TOOLS9 STRCAT TOOLS9 COMPARE 0= -> TRUE }T
T{ HERE @ 0 TOOLS10 STRCAT TOOLS9 STRCAT TOOLS11 COMPARE 0= -> TRUE }T
FORGET TOOLS9

T{ 0 ?CFA -> FALSE }T
T{ HERE ?CFA -> FALSE }T
T{ GET-CURRENT >CFA ?CFA -> GET-CURRENT >CFA }T
( GET-CURRENT is implemented in Forth to the first value *after* its own CFA
  should point to a CFA )
T{ GET-CURRENT >CFA CELL+ @ ?CFA 0<> -> TRUE }T

( MANGLE )
: TOOLS12 S" TWODUP" S" 2DUP" ;
T{ TOOLS12 MANGLE COMPARE 0= -> TRUE }T
FORGET TOOLS12

OK


( --------------------------------------------------------------------------- )
( Final test... let's make sure execution stops after we say BYE )
." Checking for stack leaks ..."
DEPTH 0= ASSERT
OK

ERROR_COUNT @ 0= [IF]
	BYE
[ELSE]
	1 FN-exit CCALL1
[THEN]

0 ASSERT

\ These tests require eyeball/diff testing
\ WORDS
\ SEE TRUE
\ SEE WORDS
\ SEE HIDE
\ SEE >DFA
\ SEE +
\ SEE QUIT

\ HERE @ .
\ LATEST @ .
\ : DISPOSABLE ROT ROT ROT ;
\ WORDS
\ LATEST @ HERE !
\ LATEST @ @ LATEST !
\ HERE @ .
\ LATEST @ .
