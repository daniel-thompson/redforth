// SPDX-License-Identifier: LGPL-3.0-or-later

#ifndef RF_WORDS_BASIC_H_
#define RF_WORDS_BASIC_H_

#define JONES_VERSION 47

/*
 * BASIC STACK MANIPULATION  ------------------------------------------------
 */

QNATIVE(DROP) /* ( a -- ) drop top of stack */
#undef  LINK
#define LINK DROP
	(void) POP();
	NEXT();

QNATIVE(SWAP) /* ( a b -- b a ) swap top two elements on stack */
#undef  LINK
#define LINK SWAP
	tmp = dsp[0];
	dsp[0] = dsp[1];
	dsp[1] = tmp;
	NEXT();

QNATIVE(DUP) /* ( a -- a a ) duplicate top of stack */
#undef  LINK
#define LINK DUP
	PUSH(dsp[0]);
	NEXT();

QNATIVE(OVER) /* ( a b -- b a b ) duplicate second element of stack */
#undef  LINK
#define LINK OVER
	PUSH(dsp[1]);
	NEXT();

QNATIVE(ROT) /* ( a b c -- b c a ) "forwards" stack rotation (grab the third element of the stack) */
#undef  LINK
#define LINK ROT
	tmp = dsp[0];
	dsp[0] = dsp[2];
	dsp[2] = dsp[1];
	dsp[1] = tmp;
	NEXT();

NATIVE(NROT, "-ROT") /* ( a b c -- c a b ) "backwards" stack rotation (banish top-of-stack) */
#undef  LINK
#define LINK NROT
	tmp = dsp[0];
	dsp[0] = dsp[1];
	dsp[1] = dsp[2];
	dsp[2] = tmp;
	NEXT();

NATIVE(TWODROP, "2DROP") /* ( a b -- ) drop two elements of stack */
#undef  LINK
#define LINK TWODROP
	(void) POP();
	(void) POP();
	NEXT();

NATIVE(TWODUP, "2DUP") /* ( a b -- a b a b ) duplicate top two elements of stack */
#undef  LINK
#define LINK TWODUP
	PUSH(dsp[1]);
	PUSH(dsp[1]);
	NEXT();

NATIVE(TWOSWAP, "2SWAP") /* ( a b c d -- c d a b ) swap top two pairs of elements of stack */
#undef  LINK
#define LINK TWOSWAP
	tmp = dsp[0];
	dsp[0] = dsp[2];
	dsp[2] = tmp;

	tmp = dsp[1];
	dsp[1] = dsp[3];
	dsp[3] = tmp;

	NEXT();

NATIVE(QDUP, "?DUP") /* ( a -- 0 | a a ) duplicate top if stack if non-zero */
#undef  LINK
#define LINK QDUP
	if (dsp[0].u)
		PUSH(dsp[0]);
	NEXT();

NATIVE(ONEINCR, "1+") /* ( a -- a+1 ) increment top of stack */
#undef  LINK
#define LINK ONEINCR
	dsp[0].u += 1;
	NEXT();

NATIVE(ONEDECR, "1-") /* ( a -- a-1 ) decrement top of stack */
#undef  LINK
#define LINK ONEDECR
	dsp[0].u -= 1;
	NEXT();

NATIVE(CELLINCR, "CELL+") /* ( a -- a+<cellsz> add cell size to the top of stack) */
#undef  LINK
#define LINK CELLINCR
	dsp[0].u += sizeof(uintptr_t);
	NEXT();

NATIVE(CELLDECR, "CELL-") /* ( a -- a+<cellsz> substract cell size to the top of stack) */
#undef  LINK
#define LINK CELLDECR
	dsp[0].u -= sizeof(uintptr_t);
	NEXT();

NATIVE(ADD, "+") /* ( a b -- a+b )) */
	tmp = POP();
	dsp[0].u += tmp.u;
	NEXT();
#undef  LINK
#define LINK ADD
NATIVE(SUB, "-") /* ( a b -- b-a ) */
#undef  LINK
#define LINK SUB
	tmp = POP();
	dsp[0].u -= tmp.u;
	NEXT();

NATIVE(MUL, "*") /* ( a b -- a*b ) */
#undef  LINK
#define LINK MUL
	tmp = POP();
	dsp[0].n *= tmp.n;
	NEXT();

/*
 * Hopefully the compiler will recognise the combined divmod operation and
 * select an opcode accordingly.
 */
NATIVE(DIVMOD, "/MOD") /* ( a b -- a%b a/b ) */
#undef  LINK
#define LINK DIVMOD
	cell_t b = dsp[0];
	cell_t a = dsp[1];
	dsp[1].n = a.n % b.n;
	dsp[0].n = a.n / b.n;
	NEXT();

/*
 * COMPARISON OPERATIONS  ---------------------------------------------------
 */

NATIVE(EQU, "=") /* = ( a b -- b==a ) top two words are equal? */
#undef  LINK
#define LINK EQU
	tmp = POP();
	dsp[0].n = dsp[0].n == tmp.n;
	NEXT();

NATIVE(NEQU, "<>") /* ( a b -- b!=a ) top two words are not equal? */
#undef  LINK
#define LINK NEQU
	tmp = POP();
	dsp[0].n = dsp[0].n != tmp.n;
	NEXT();

NATIVE(LT, "<") /* ( a b -- b<a) */
#undef  LINK
#define LINK LT
	tmp = POP();
	dsp[0].n = dsp[0].n < tmp.n;
	NEXT();

NATIVE(GT, ">") /* ( a b -- b>a) */
#undef  LINK
#define LINK GT
	tmp = POP();
	dsp[0].n = dsp[0].n > tmp.n;
	NEXT();

NATIVE(LTEQU, "<=") /* ( a b -- b<=a) */
	tmp = POP();
	dsp[0].n = dsp[0].n <= tmp.n;
	NEXT();
#undef  LINK
#define LINK LTEQU

NATIVE(GTEQU, ">=") /* ( a b -- b>=a) */
#undef  LINK
#define LINK GTEQU
	tmp = POP();
	dsp[0].n = dsp[0].n >= tmp.n;
	NEXT();

NATIVE(ZEQU, "0=") /* ( a -- a==0 ) */
#undef  LINK
#define LINK ZEQU
	dsp[0].n = dsp[0].n == 0;
	NEXT();

NATIVE(ZNEQU, "0<>") /* ( a -- a!=0 ) */
#undef  LINK
#define LINK ZNEQU
	dsp[0].n = dsp[0].n != 0;
	NEXT();

NATIVE(ZLT, "0<") /* ( a -- a<0 ) */
#undef  LINK
#define LINK ZLT
	dsp[0].n = dsp[0].n < 0;
	NEXT();

NATIVE(ZGT, "0>") /* ( a -- a>0 ) */
#undef  LINK
#define LINK ZGT
	dsp[0].n = dsp[0].n > 0;
	NEXT();

NATIVE(ZLE, "0<=") /* ( a -- a<=0 ) */
#undef  LINK
#define LINK ZLE
	dsp[0].n = dsp[0].n <= 0;
	NEXT();

NATIVE(ZGE, "0>=") /* ( a -- a>=0 ) */
#undef  LINK
#define LINK ZGE
	dsp[0].n = dsp[0].n >= 0;
	NEXT();

QNATIVE(AND) /* ( a b -- b&a ) */
#undef  LINK
#define LINK AND
	tmp = POP();
	dsp[0].u = dsp[0].u & tmp.u;
	NEXT();

QNATIVE(OR) /* ( a b -- b|a ) */
#undef  LINK
#define LINK OR
	tmp = POP();
	dsp[0].u = dsp[0].u | tmp.u;
	NEXT();

QNATIVE(XOR) /* ( a b -- b^a ) */
#undef  LINK
#define LINK XOR
	tmp = POP();
	dsp[0].u = dsp[0].u ^ tmp.u;
	NEXT();

QNATIVE(INVERT) /* ( a -- ~a ) */
#undef  LINK
#define LINK INVERT
	dsp[0].u = ~dsp[0].u;
	NEXT();

/*
 * RETURNING FROM FORTH WORDS  ----------------------------------------------
 */

QNATIVE(EXIT)
#undef  LINK
#define LINK EXIT
	ip = POPRSP().p;
	NEXT();

/* SECRET_EXIT acts identically to a normal EXIT. However it also acts
 * as a delimiter and tells SEE and >ROM that they can stop decompiling.
 *
 * This exit is "secret" because SEE will never decompile it (because
 * it is only ever added implicitly by the ; word).
 */
QNATIVE(SECRET_EXIT)
#undef  LINK
#define LINK SECRET_EXIT
	ip = POPRSP().p;
	NEXT();

/*
 * LITERALS  ----------------------------------------------------------------
 */

QNATIVE(LIT)
#undef  LINK
#define LINK LIT
	PUSH((cell_t) { .p = *ip++ });
	NEXT();

/*
 * MEMORY  ------------------------------------------------------------------
 */

NATIVE(STORE, "!") /* ( x a-addr -- ) */
#undef  LINK
#define LINK STORE
	tmp = POP();
	*tmp.xp = POP();
	NEXT();

NATIVE(FETCH, "@") /* ( p1 -- x2 ) */
#undef  LINK
#define LINK FETCH
	tmp = POP();
	PUSH(*tmp.xp);
	NEXT();

NATIVE(ADDSTORE, "+!") /* ( u a-addr -- ) */
#undef  LINK
#define LINK ADDSTORE
	tmp = POP();
	tmp.xp->u += POP().u;
	NEXT();

NATIVE(SUBSTORE, "-!") /* ( u a-addr -- ) */
#undef  LINK
#define LINK SUBSTORE
	tmp = POP();
	tmp.xp->u -= POP().u;
	NEXT();

/*
 * ! and @ (STORE and FETCH) store 32-bit words.  It's also useful to be able
 * to read and write bytes so we also define standard words C@ and C!.
 *
 * Byte-oriented operations only work on architectures which permit them (i386
 * is one of those).
 */
NATIVE(CSTORE, "C!")
#undef  LINK
#define LINK CSTORE
	tmp = POP();
	*tmp.cp = POP().u;
	NEXT();

NATIVE(CFETCH, "C@")
#undef  LINK
#define LINK CFETCH
	tmp = POP();
	PUSH((cell_t) { .u = *tmp.cp });
	NEXT();

QNATIVE(CMOVE) /* ( src dst len -- ) */
#undef  LINK
#define LINK CMOVE
	size_t len = POP().u;
	void *dst = POP().p;
	void *src = POP().p;
	memmove(dst, src, len);
	NEXT();

/*
 * BUILT-IN VARIABLES  ------------------------------------------------------
 */

VARIABLE("STATE", STATE)
#undef  LINK
#define LINK STATE

VARIABLE("HERE", HERE)
#undef  LINK
#define LINK HERE

VARIABLE("LATEST", LATEST)
#undef  LINK
#define LINK LATEST

VARIABLE("S0", S0)
#undef  LINK
#define LINK S0

VARIABLE("BASE", BASE)
#undef  LINK
#define LINK BASE

/*
 * BUILT-IN CONSTANTS  ------------------------------------------------------
 */

CONSTANT("R0", R0, (uintptr_t) var_R0);
#undef  LINK
#define LINK R0

CONSTANT("VERSION", VERSION, JONES_VERSION)
#undef  LINK
#define LINK VERSION

CONSTANT("DOCOL", C_DOCOL, (uintptr_t) &&DOCOL);
#undef  LINK
#define LINK C_DOCOL

CONSTANT("F_IMMED", C_F_IMMED, F_IMMED)
#undef  LINK
#define LINK C_F_IMMED

CONSTANT("F_HIDDEN", C_F_HIDDEN, F_HIDDEN)
#undef  LINK
#define LINK C_F_HIDDEN

CONSTANT("F_LENMASK", C_F_LENMASK, F_LENMASK)
#undef  LINK
#define LINK C_F_LENMASK

NATIVE(BUILTIN_WORDS, "BUILTIN-WORDS")
#undef  LINK
#define LINK BUILTIN_WORDS
	PUSH((cell_t) { .p = const_BUILTIN_WORDS });
	NEXT();

/*
 * RETURN STACK  ------------------------------------------------------------
 */

NATIVE(TOR, ">R") /* ( a -- ) Move a from data to return stack */
#undef  LINK
#define LINK TOR
	PUSHRSP(POP());
	NEXT();

NATIVE(FROMR, "R>") /* ( -- a_) Move a from return to data stack */
#undef  LINK
#define LINK FROMR
	PUSH(POPRSP());
	NEXT();

NATIVE(RSPFETCH, "RSP@")
#undef  LINK
#define LINK RSPFETCH
	PUSH((cell_t) { .p = rsp });
	NEXT();

NATIVE(RSPSTORE, "RSP!")
#undef  LINK
#define LINK RSPSTORE
	rsp = POP().p;
	NEXT();

QNATIVE(RDROP)
#undef  LINK
#define LINK RDROP
	(void) POPRSP();
	NEXT();

/*
 * PARAMETER (DATA) STACK  --------------------------------------------------
 */

NATIVE(DSPFETCH, "DSP@")
#undef  LINK
#define LINK DSPFETCH
	PUSH((cell_t) { .p = dsp });
	NEXT();

NATIVE(DSPSTORE, "DSP!")
#undef  LINK
#define LINK DSPSTORE
	tmp = POP();
	dsp = tmp.p;
	NEXT();

/*
 * INPUT AND OUTPUT  --------------------------------------------------------
 */

QNATIVE(KEY) /* ( -- ch ) read a character from stdin */
#undef  LINK
#define LINK KEY
	PUSH((cell_t) { .u = do_KEY() });
	NEXT();

QNATIVE(EMIT) /* ( ch -- ) send a character to stdout */
#undef  LINK
#define LINK EMIT
	do_EMIT(POP().u);
	NEXT();

QNATIVE(WORD) /* ( -- &str len ) read a word from stdin */
#undef  LINK
#define LINK WORD
	PUSH((cell_t) { .cp = do_WORD() });
	PUSH((cell_t) { .u = strlen(dsp[0].cp) });
	NEXT();

QNATIVE(NUMBER) /* ( len &str -- num pending ) */
#undef  LINK
#define LINK NUMBER
	uintptr_t len = POP().u;
	char *s = POP().cp;
	char *end;
	PUSH((cell_t) { .u = do_NUMBER(s, len, var_BASE, &end) });
	PUSH((cell_t) { .u = (s+len) - end });
	NEXT();

/*
 * DICTIONARY LOOK UPS  -----------------------------------------------------
 */

QNATIVE(FIND) /* ( &str len -- word ) */
#undef  LINK
#define LINK FIND
	uintptr_t len = POP().u;
	char *s = POP().cp;
	PUSH((cell_t) { .p = do_FIND(s, len) });
	NEXT();

NATIVE(TCFA, ">CFA") /* ( &link -- &codeword ) */
#undef  LINK
#define LINK TCFA
	struct header *link = POP().p;
	struct codefield *cf = to_CFA(link);
	codeword = &cf->codeword;
	PUSH((cell_t) { .p = codeword });
	NEXT();

BUILTIN(TDFA, ">DFA")
#undef  LINK
#define LINK TDFA
	COMPILE(TCFA)
	COMPILE(CELLINCR)
	COMPILE_EXIT()

/*
 * COMPILING  ---------------------------------------------------------------
 */

QNATIVE(CREATE) /* ( len @str -- ) Create a new dictionary entry */
#undef  LINK
#define LINK CREATE
	/*
	 * struct {
	 *     struct header hrt;
	 *     char name[RAW_NAMELEN(len+1)];
	 *     uint8_t flags;
	 *     struct codefield cf;
	 *
	 *     const void *words[]
	 * }
	 *
	 * CREATE only creates the dictionary entry up to the codeword
	 * (the last element of the codefield). Other words will append the
	 * codeword and the word array.
	 */
	uintptr_t len = POP().u;
	char *s = POP().cp;

	/* allocate the dictionary entry from HERE */
	struct header *link = (struct header *) var_HERE;
	char *name = (char *) (link + 1);
	uint8_t *flags = (uint8_t *) (name + RAW_NAMELEN(len+1));
	struct codefield *cf = (struct codefield *) (flags + 1);
	var_HERE = ((char *) (cf + 1)) - sizeof(cf->codeword);

	/* Add to the list */
	link->next = var_LATEST;
	var_LATEST = link;

	memcpy(name, s, len);
	name[len] = '\0';
	*flags = (RAW_NAMELEN(len+1) + 1) / sizeof(void *);

	/*
	 * This is not currently needed but codefield is intended to include
	 * (optional) profiling counters and it would be very easy to
	 * zero them.
	 */
	memset(cf, 0, sizeof(*cf));

	NEXT();

NATIVE(COMMA, ",")
#undef  LINK
#define LINK COMMA
	cell_t *mem = (cell_t *) var_HERE;
	*mem = POP();
	var_HERE = (char *) (mem + 1);
	NEXT();

NATIVE_FLAGS(LBRAC,"[", F_IMMED) /* ( -- ) Set STATE to 0 */
#undef  LINK
#define LINK LBRAC
	var_STATE = 0;
	NEXT();

NATIVE(RBRAC, "]") /* ( -- ) Set STATE to 1 */
#undef  LINK
#define LINK RBRAC
	var_STATE = 1;
	NEXT();

QNATIVE(HIDDEN)
#undef  LINK
#define LINK HIDDEN
	uint8_t *flags = to_flags(POP().p);
	*flags ^= F_HIDDEN;
	NEXT();

QBUILTIN(HIDE)
#undef  LINK
#define LINK HIDE
	COMPILE(WORD)
	COMPILE(FIND)
	COMPILE(HIDDEN)
	COMPILE_EXIT()

NATIVE_FLAGS(IMMEDIATE, "IMMEDIATE", F_IMMED)
#undef  LINK
#define LINK IMMEDIATE
	uint8_t *flags = to_flags(var_LATEST);
	*flags ^= F_IMMED;
	NEXT();

NATIVE(TICK, "'") /* ( -- codeword ) Skip the next codeword, instead push it to stack */
#undef  LINK
#define LINK TICK
	/*
	 * This sneaky approach only works in compiled code because it is relying
	 * on the compiler to convert a WORD to a codeword and place it in the
	 * instruction stream.
	 */
	PUSH((cell_t) { .p = (*ip++) });
	NEXT();

BUILTIN(COLON, ":")
#undef  LINK
#define LINK COLON
	COMPILE(WORD)		// Get the name of the new word
	COMPILE(CREATE)		// CREATE the dictionary entry / header
	COMPILE_LIT(&&DOCOL)	// Load DOCOL (the codeword)
	COMPILE(COMMA)		// Append the codeword
	COMPILE(LATEST)
	COMPILE(FETCH)
	COMPILE(HIDDEN)		// Make the new word hidden
	COMPILE(RBRAC)		// Go into compile mode
	COMPILE_EXIT()		// Return from the function

BUILTIN_FLAGS(SEMICOLON, ";", F_IMMED)
#undef  LINK
#define LINK SEMICOLON
	COMPILE_TICK(SECRET_EXIT)
	COMPILE(COMMA)		// Append EXIT (so the word will return)
	COMPILE(LATEST)
	COMPILE(FETCH)
	COMPILE(HIDDEN)		// Unhide the word
	COMPILE(LBRAC)		// Go back to IMMEDIATE mode
	COMPILE_EXIT()


/*
 * BRANCHING  ---------------------------------------------------------------
 */

QNATIVE(BRANCH)
#undef  LINK
#define LINK BRANCH
	ip = (void ***) ((char*) ip + (intptr_t) (*ip));
	NEXT();

NATIVE(ZBRANCH, "0BRANCH")
#undef  LINK
#define LINK ZBRANCH
	tmp = POP();
	if (tmp.u == 0)		// top of the stack is zero
		ip = (void ***) ((char*) ip + (intptr_t) (*ip));
	else
		ip++;		// otherwise we need to skip the offset
	NEXT();

/*
 * LITERAL STRINGS  ---------------------------------------------------------
 */

QNATIVE(LITSTRING)
#undef  LINK
#define LINK LITSTRING
	tmp.p  = *ip++;				// get the length of string
	PUSH((cell_t) { .p = ip });		// push start address
	PUSH(tmp);				// push length

	// skip past the string (rounding up to next cell boundary
	ip += (tmp.u + sizeof(*ip) - 1) / sizeof(*ip); // skip past the string
	NEXT();

QNATIVE(CLITSTRING) /* ( -- c-addr u ) Load a C string literal */
#undef	LINK
#define LINK CLITSTRING
	tmp.p  = *ip++;				// get the length of string
	PUSH((cell_t) { .p = *ip++ });		// push start address
	PUSH(tmp);				// push length

	/* Finally we must skip forward to the next instruction. When >ROM
	 * writes out string literals it pads the CLITSTRING instruction to
	 * ensure branches still point to the right place (and in a manner
	 * such that CLITSTRING can cope regardless of the cell size of
	 * the Forth running >ROM.
	 */
	while (NULL == *ip)			// now skip NULL instructions
		ip++;
	NEXT();

QNATIVE(TYPE)
#undef  LINK
#define LINK TYPE
	uintptr_t len = POP().u;
	do_TYPE(POP().p, len);
	NEXT();

/*
 * QUIT AND INTERPRET  ------------------------------------------------------
 */

QNATIVE(INTERPRET)
#undef  LINK
#define LINK INTERPRET
	char *word = do_WORD(), *end;
	uintptr_t literal = 0;
	bool is_literal;

	struct header *hdr = do_FIND(word, strlen(word));
	is_literal = !hdr;

	if (is_literal) {
		literal = strtoll(word, &end, var_BASE);
		if (word[end-word] == '\0')
			hdr = (struct header *) &word_LIT.hdr;
	}

	if (!hdr) {
		fprintf(stderr, "%"PRIdPTR": Bad word '%s'\n", var_LINENO, word);
		if (var_STATE) {
			// TODO: lexer driven error recovery? ABORT?
			exit(-1);
		}
		RAW_NEXT();
	}

	uint8_t *flags = to_flags(hdr);
	struct codefield *cf = (void *) (flags + 1);
	codeword = &cf->codeword;

	if (!var_STATE || (*flags & F_IMMED)) { // executing
		if (is_literal) {
			PUSH((cell_t) { .u = literal });
		} else {
			// this is effectively tail recursion from INTERPRET
			// into the new word
			goto **codeword;
		}
	} else { // compiling
		uintptr_t *mem = (uintptr_t *) var_HERE;
		*mem++ = (uintptr_t) codeword;
		if (is_literal)
			*mem++ = literal;
		var_HERE = (char *) mem;
	}

	NEXT();

// QUIT must not return (ie. must not call EXIT).
QBUILTIN(QUIT)
#undef  LINK
#define LINK QUIT
	COMPILE(R0)
	COMPILE(RSPSTORE)			// clear the return stack
	COMPILE(INTERPRET)			// interpret the next word...
	COMPILE_BRANCH(-2)			// ... forever
	COMPILE_EXIT();

/*
 * ODDS AND ENDS  -----------------------------------------------------------
 */

QNATIVE(CHAR)
#undef  LINK
#define LINK CHAR
	char *s = do_WORD();
	PUSH((cell_t) { .u = s[0] });
	NEXT();

QNATIVE(EXECUTE)
#undef  LINK
#define LINK EXECUTE
	codeword = POP().p;
	goto **codeword;

	// HACK: ensure we close the safety brace
	NEXT();

QNATIVE(RAWDOT) /* ( num -- ) */
#undef  LINK
#define LINK RAWDOT
	printf("%" PRIdPTR " ", POP().u);
	fflush(stdout);
	NEXT();

QNATIVE(PAUSE) /* ( ) exit the VM */
#undef  LINK
#define LINK PAUSE
	return;
	NEXT();

BUILTIN_FLAGS(COMMENT, "\\", F_IMMED)
#undef  LINK
#define LINK COMMENT
	COMPILE(KEY)
	COMPILE_LIT('\n')
	COMPILE(EQU)
	COMPILE_0BRANCH(-5)
	COMPILE_EXIT()

/*
 * C ABI GLUE  --------------------------------------------------------------
 */

QNATIVE(CCALL0) /* ( fn-addr -- retval ) */
#undef  LINK
#define LINK CCALL0
	dsp[0] = dsp[0].fnp();
	NEXT();

QNATIVE(CCALL1) /* ( arg1 fn-addr -- retval ) */
#undef  LINK
#define LINK CCALL1
	dsp[1] = dsp[0].fnp(dsp[1]);
	(void) POP();
	NEXT();

QNATIVE(CCALL2) /* ( arg2 arg1 fn-addr -- retval ) */
#undef  LINK
#define LINK CCALL2
	dsp[2] = dsp[0].fnp(dsp[1], dsp[2]);
	(void) POP();
	(void) POP();
	NEXT();

QNATIVE(CCALL3) /* ( arg3 arg2 arg1 addr -- retval ) */
#undef  LINK
#define LINK CCALL3
	dsp[3] = dsp[0].fnp(dsp[1], dsp[2], dsp[3]);
	(void) POP();
	(void) POP();
	(void) POP();
	NEXT();

QNATIVE(CCALL4) /* ( addr -- retval ) */
#undef  LINK
#define LINK CCALL4
	dsp[4] = dsp[0].fnp(dsp[1], dsp[2], dsp[3], dsp[4]);
	(void) POP();
	(void) POP();
	(void) POP();
	(void) POP();
	NEXT();

#endif /* RF_WORDS_BASIC_H_ */
