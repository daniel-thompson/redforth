\ SPDX-License-Identifier: LGPL-3.0-or-later

( "eyeball" tests

  Each test case consists of a Forth stanza that writes oneline to the terminal
  followed by a string containing the reference output.

  These are called the "eyeball" tests because, by showing the expected result
  on the next line, we could eyeball the differences to pass/fail the test.
  However despite the name we can actually generate a test verdict automatically
  by using a filter to remove duplicated output lines leaving only the output
  of failed tests.

  Before the test start we'll start off by emiting a newline because the splash
  message *doesn't* end with a newline.
)

( ASSERT verifis that the provided condition, n, is TRUE )
: ASSERT ( n -- )
	0= IF
		."  FAILED
"
		." <" DEPTH 1 CELLS / . ." >  " .S CR EMIT
		ABORT
	THEN
;

CR
DEPTH 0= ASSERT

( . )
10 . -10 . 0 . CR
." 10 -10 0 " CR

( U.

  Currently this test is disabled because U. doesn't work for -ve numbers.
)
\10 U. -1 U. 0 U. CR
\." 10 / 0 " CR

( .S )

1 2 3 .S DROP DROP DROP CR
." <3> 1 2 3 " CR


( Test SEE )

: TSEE1 10 40 * ;
: TSEE2 10 BEGIN 1- DUP <= 0 UNTIL DROP ;

SEE TSEE1
." : TSEE1 10 40 * ;" CR
SEE TSEE2
." : TSEE2 10 1- DUP <= 0 0BRANCH ( " -6 CELLS . ." ) DROP ;" CR

SEE HIDE
." : HIDE WORD FIND HIDDEN ;" CR

FORGET TSEE1


( Test PRINT-STACK-TRACE

  Providing the reference output is a little tricky because it will be
  different for different cell sizes but we can manage it!
)
: ALPHA 0 PRINT-STACK-TRACE DROP ;
: BRAVO 0 DROP ALPHA ;
: CHARLIE BRAVO 0 DROP ;
CHARLIE
FORGET ALPHA
." ALPHA+" 2 CELLS 0 .R ."  BRAVO+" 3 CELLS 0 .R ."  CHARLIE+0 " CR 

( Test WORDS

  Another tricky one... we basically have to cheat a little and print
  the words with and without TMPWORD and compare the output.
)
: TMPWORD ;
WORDS
FORGET TMPWORD
." TMPWORD " WORDS

( Test STRING>NAME )
S" WORDS" STRING>NAME DROP NAME>STRING TYPE CR
." WORDS" CR

( Make sure nothing we did in this test leaked anything on the data stack )
DEPTH 0= ASSERT
