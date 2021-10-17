\ SPDX-License-Identifier: LGPL-3.0-or-later

BUILTIN-WORDS
INCLUDE core-words.fs
LATEST @
INCLUDE debug-words.fs
LATEST @
INCLUDE tools-words.fs
LATEST @

( At this point the data stack has name tokens for all the new words
  stacked up in reverse order:

      builtin core debug tools

  We work back through this a pair at a time generating the code needed
  to have a complete run-from-ROM sytem.
)

EXUDE tools-words.h
." // Auto-generated from tools-words.fs" CR
." #ifndef RF_TOOLS_WORDS_H_" CR
." #define RF_TOOLS_WORDS_H_" CR
2DUP LIST>ROM DROP
." #endif /* RF_TOOLS_WORDS_H_*/" CR

EXUDE debug-words.h
." // Auto-generated from debug-words.fs" CR
." #ifndef RF_DEBUG_WORDS_H_" CR
." #define RF_DEBUG_WORDS_H_" CR
2DUP LIST>ROM DROP
." #endif /* RF_DEBUG_WORDS_H_*/" CR

EXUDE core-words.h
." // Auto-generated from core.fs" CR
." #ifndef RF_CORE_WORDS_H_" CR
." #define RF_CORE_WORDS_H_" CR
LIST>ROM
." #endif /* RF_CORE_WORDS_H_*/" CR
