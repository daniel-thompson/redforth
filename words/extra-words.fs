\ SPDX-License-Identifier: LGPL-3.0-or-later

( Non-standard words that provide shell-ish features.

  Rather obviously these are inspired by historically "incorrect" sources
  such as the C library and common Unix tools. However they are useful,
  especially when working on freestanding systems such as microcontrollers.
)

\ VARIABLE ERROR_COUNT
\ 0 ERROR_COUNT !

( ASSERT verifies that the provided condition, n, is TRUE )
: ASSERT ( n -- )
	0= IF
		\ 1 ERROR_COUNT +!

		."  FAILED
"
		." <" DEPTH 1 CELLS / . ." >  " .S CR EMIT
		\ ABORT
		BYE
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

( --------------------------------------------------------------------------- )
." Test suite self tests "

( Check that a nop matches another nop... and that there is no stack leakage )
\ DEPTH CELL+
T{ -> }T
\ DEPTH = ASSERT

\ DEPTH CELL+
T{ 1002 1001 1000 -> 1002 1001 1000 }T
\ DEPTH = ASSERT

\ T{ DEPTH -> 0 }T
