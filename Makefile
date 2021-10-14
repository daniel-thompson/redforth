# SPDX-License-Identifier: LGPL-3.0-or-later

CC = gcc
CFLAGS = -Wall -Werror -g -Iwords

ifdef NO_OPT
CFLAGS += -O0
else
CFLAGS += -O2
endif

all : redforth

SRCS = libred.c main.c vm-gnuc.c
CROSS_OBJS = $(patsubst %.c,build-cross/%.o,$(SRCS))
HDRS = libred.h words/native-words.h words/stdc-words.h
CODEGEN_HDRS = words-core.h words-tools.h

crossforth : build-cross $(CROSS_OBJS)
	$(CC) $(CFLAGS) -o $@ $(CROSS_OBJS)

build-cross/%.o : %.c
	$(CC) $(CFLAGS) -c -o $@ $<

OBJS = $(patsubst %.c,build/%.o,$(SRCS))

redforth : build $(OBJS)
	$(CROSS_COMPILE)$(CC) $(CFLAGS) -o $@ $(OBJS)

build/%.o : %.c
	$(CROSS_COMPILE)$(CC) $(CFLAGS) -DHAVE_CODEGEN_WORDS -c -o $@ $<

$(CODEGEN_HDRS) : do_codegen
do_codegen : crossforth
	(cd words/; ../crossforth < ../codegen.fs)
.PHONY: do_codegen

build build-cross :
	mkdir $@

clean :
	$(RM) -r build-cross/ build/
	$(RM) redforth crossforth words-codegen.h debug.fs filetest.txt

eyeball : redforth
	cat eyeball.fs | ./redforth

cross-check : crossforth
	cat words/core-words.fs words/tools-words.fs eyeball.fs | ./crossforth | uniq -u > eyeball.log
	[ `cat eyeball.log | wc -l` -eq 2 ] || (cat eyeball.log; false)
	cat words/core-words.fs words/tools-words.fs selftest.fs | ./crossforth
	@$(RM) filetest.txt eyeball.log

check : redforth
	cat eyeball.fs | ./redforth | uniq -u > eyeball.log
	[ `cat eyeball.log | wc -l` -eq 1 ] || (cat eyeball.log; false)
	cat selftest.fs | ./redforth
	@$(RM) filetest.txt eyeball.log

debug : redforth
	cat core.fs tools.fs selftest.fs > debug.fs
	gdb redforth -ex "break rf_forth_exec" -ex "run < debug.fs"

$(CROSS_OBJS) $(OBJS) : Makefile $(HDRS)
build/vm-gnuc.o : $(CODEGEN_HDRS)
