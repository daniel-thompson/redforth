# SPDX-License-Identifier: LGPL-3.0-or-later

CC = gcc
CFLAGS = -Wall -Werror -g

ifdef NO_OPT
CFLAGS += -O0
else
CFLAGS += -O2
endif

all : redforth

SRCS = libred.c main.c vm-gnuc.c
CROSS_OBJS = $(patsubst %.c,build-cross/%.o,$(SRCS))
HDRS = libred.h words-basic.h words-stdc.h
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

$(CODEGEN_HDRS) : crossforth
	./crossforth < codegen.fs

build build-cross :
	mkdir $@

clean :
	$(RM) -r build-cross/ build/
	$(RM) redforth crossforth words-codegen.h debug.fs filetest.txt

eyeball : redforth
	cat core.fs tools.fs eyeball.fs | ./redforth

check : redforth
	cat core.fs tools.fs eyeball.fs | ./redforth | uniq -u > eyeball.log
	[ `cat eyeball.log | wc -l` -eq 2 ] || (cat eyeball.log; false)
	cat core.fs tools.fs selftest.fs | ./redforth
	@$(RM) filetest.txt eyeball.log

debug : redforth
	cat core.fs tools.fs selftest.fs > debug.fs
	gdb redforth -ex "break rf_forth_exec" -ex "run < debug.fs"

$(CROSS_OBJS) $(OBJS) : Makefile $(HDRS)
build/vm-gnuc.o : $(CODEGEN_HDRS)
