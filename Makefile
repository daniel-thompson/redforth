# SPDX-License-Identifier: LGPL-3.0-or-later

CC = gcc
CFLAGS = -Wall -Werror -g -Ilibred -Iwords

ifdef NO_OPT
CFLAGS += -O0
else
CFLAGS += -O2
endif

all : redforth

SRCS = libred.c main.c vm-gnuc.c
CROSS_OBJS = $(patsubst %.c,build-cross/%.o,$(SRCS))
HDRS = libred/libred.h words/native-words.h words/stdc-words.h
CODEGEN_FS = words/core-words.fs words/debug-words.fs words/tools-words.fs
CODEGEN_HDRS = $(patsubst %.fs,%.h,$(CODEGEN_FS))

build-cross/crossforth : build-cross $(CROSS_OBJS)
	$(CC) $(CFLAGS) -o $@ $(CROSS_OBJS)

build-cross/%.o : %.c
	$(CC) $(CFLAGS) -c -o $@ $<

OBJS = $(patsubst %.c,build/%.o,$(SRCS))

redforth : build $(OBJS)
	$(CROSS_COMPILE)$(CC) $(CFLAGS) $(CROSS_CFLAGS) -o $@ $(OBJS)

build/%.o : %.c
	$(CROSS_COMPILE)$(CC) $(CFLAGS) $(CROSS_CFLAGS) -DHAVE_CODEGEN_WORDS -c -o $@ $<

$(CODEGEN_HDRS) : do_codegen
do_codegen : build-cross/crossforth
	(cd words/; ../build-cross/crossforth < ../scripts/codegen.fs) > /dev/null
.PHONY: do_codegen

build build-cross :
	mkdir $@

clean :
	$(RM) -r build-cross/ build/
	$(RM) redforth $(CODEGEN_HDRS) debug.fs filetest.txt

eyeball : redforth
	cat eyeball.fs | ./redforth

cross-check : build-cross/crossforth
	cat $(CODEGEN_FS) scripts/eyeball.fs | ./build-cross/crossforth | uniq -u > eyeball.log
	[ `cat eyeball.log | wc -l` -eq 2 ] || (cat eyeball.log; false)
	cat $(CODEGEN_FS) scripts/selftest.fs | ./build-cross/crossforth
	@$(RM) filetest.txt eyeball.log

check : redforth
	cat scripts/eyeball.fs | ./redforth | uniq -u > eyeball.log
	[ `cat eyeball.log | wc -l` -eq 1 ] || (cat eyeball.log; false)
	cat scripts/selftest.fs | ./redforth
	@$(RM) filetest.txt eyeball.log

debug : redforth
	gdb redforth -ex "break rf_forth_exec" -ex "run < scripts/selftest.fs"

$(CROSS_OBJS) $(OBJS) : Makefile $(HDRS)
build/vm-gnuc.o : do_codegen $(CODEGEN_HDRS)

vpath %.c libred/ ports/unix/
